using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor.Scripting.Python;
using UnityEngine;

namespace UnityEditor.GPT
{
    public class GPTWindow : EditorWindow
    {
        [MenuItem("AI/GPTWindow")]
        public static void Open()
        {
            var win = GetWindow<GPTWindow>();
            win.titleContent = new GUIContent(nameof(GPTWindow));
            win.minSize = new Vector2(400, 300);
            win.Show();
        }

        public static async void recv(int typeID, string question, string msg)
        {
            var type = (AIType)typeID;
            if (s_Instance != null)
                await s_Instance.Recv(type, question, msg);
        }

        static GPTWindow s_Instance;

        Evaluator m_Evaluator;
        StringBuilder m_Builder;
        Dictionary<AIType, string> m_ApiUrlKeys;

        GUIStyle m_OutputStyle;
        Vector2 m_OutputPos;
        string m_OutputText;
        Vector2 m_InputPos;
        string m_InputText;

        AIType m_CurrentType;
        Config m_CurrentData;
        int[] m_AITypeIDs;
        string[] m_AITypeNames;
        Dictionary<AIType, Config> m_Prompts;

        void OnEnable()
        {
            s_Instance = this;

            Evaluator.Init(ref m_Evaluator);
            Evaluator.Instance.OnEvaluationSuccess -= OnEvaluationSuccess;
            Evaluator.Instance.OnEvaluationSuccess += OnEvaluationSuccess;
            Evaluator.Instance.OnEvaluationError -= OnEvaluationError;
            Evaluator.Instance.OnEvaluationError += OnEvaluationError;

            FilePostprocessor.TextFileChanged -= TextFileChanged;
            FilePostprocessor.TextFileChanged += TextFileChanged;

            m_Builder = new StringBuilder();
            var aiTypes = Enum.GetValues(typeof(AIType));
            var len = aiTypes.Length;
            m_AITypeNames = new string[len];
            m_AITypeIDs = new int[len];

            for (int i = 0; i < len; i++)
            {
                var item = (AIType)aiTypes.GetValue(i);
                m_AITypeIDs[i] = (int)item;
                m_AITypeNames[i] = item.ToString();
            }

            InitPrompts();

            m_ApiUrlKeys = new Dictionary<AIType, string>()
            {
                { AIType.OpenAI, "API_URL" },
                { AIType.Bing, "BING_PROXY_URL" },
            };
        }

        void OnDisable()
        {
            if (Evaluator.Instance != null)
            {
                Evaluator.Instance.OnEvaluationSuccess -= OnEvaluationSuccess;
                Evaluator.Instance.OnEvaluationError -= OnEvaluationError;
            }

            FilePostprocessor.TextFileChanged -= TextFileChanged;
        }

        void OnGUI()
        {
            InitStyles();

            var windowSize = position.size;
            var execHeight = 50;
            var labelHeight = 16;
            windowSize.y -= execHeight;
            windowSize.y -= labelHeight * 2;
            var outputHeight = windowSize.y * 0.5F;
            var inputHeight = windowSize.y * 0.5F;

            EditorGUILayout.BeginHorizontal(GUILayout.Height(labelHeight));
            {
                EditorGUILayout.LabelField(" Output", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Clear Output"))
                    m_OutputText = string.Empty;
            }
            EditorGUILayout.EndHorizontal();

            m_OutputPos = EditorGUILayout.BeginScrollView(m_OutputPos, GUILayout.Height(outputHeight));
            {
                GUILayout.TextField(m_OutputText, m_OutputStyle, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(" Input Command", EditorStyles.boldLabel, GUILayout.Height(labelHeight));
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField("AI:", EditorStyles.boldLabel, GUILayout.Width(20));
                GUI.changed = false;
                m_CurrentType = (AIType)EditorGUILayout.IntPopup((int)m_CurrentType, m_AITypeNames, m_AITypeIDs, GUILayout.Width(80));
                if (GUI.changed)
                    UpdateCurrentPromptData();

                GUILayout.Space(10);

                EditorGUILayout.LabelField("Prompt:", EditorStyles.boldLabel, GUILayout.Width(50));
                if (m_CurrentData != null)
                    m_CurrentData.Index = EditorGUILayout.Popup(m_CurrentData.Index, m_CurrentData.Names);
                else
                    EditorGUILayout.LabelField(string.Empty, EditorStyles.popup);
            }
            EditorGUILayout.EndHorizontal();
            m_InputPos = EditorGUILayout.BeginScrollView(m_InputPos, GUILayout.Height(inputHeight));
            {
                m_InputText = GUILayout.TextArea(m_InputText, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginVertical(GUILayout.Height(execHeight));
            {
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_InputText));
                if (GUILayout.Button("Execute", GUILayout.Height(32)))
                {
                    m_Builder.Clear();

                    if (m_CurrentData != null && m_ApiUrlKeys.ContainsKey(m_CurrentType))
                    {
                        m_Builder.AppendLine($"import os");
                        m_Builder.AppendLine($"os.environ['{m_ApiUrlKeys[m_CurrentType]}']='{m_CurrentData.api_url ?? string.Empty}'");
                    }

                    m_Builder.AppendLine($"import gpt");
                    m_Builder.AppendLine($"gpt.set_prompt('''{m_CurrentData?.GetValue() ?? string.Empty}''')");
                    m_Builder.AppendLine($"gpt.ask_{m_CurrentType.ToString().ToLower()}('''{m_InputText.Trim()}''')");
                    PythonRunner.RunString(m_Builder.ToString());
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();
        }

        void InitStyles()
        {
            if (m_OutputStyle == null)
            {
                m_OutputStyle = new GUIStyle(EditorStyles.textField);
                m_OutputStyle.richText = true;
            }
        }

        void InitPrompts()
        {
            if (m_Prompts == null)
                m_Prompts = new Dictionary<AIType, Config>();

            var folderPath = Path.Combine(Application.dataPath, "IntegrationGPT");
            var aiTypes = Enum.GetValues(typeof(AIType));
            for (int i = 0; i < aiTypes.Length; i++)
            {
                var type = (AIType)aiTypes.GetValue(i);
                var fileName = $"{type.ToString().ToLower()}_config.json";
                var filePath = Path.Combine(folderPath, fileName);

                if (!File.Exists(filePath))
                {
                    m_Prompts.Remove(type);
                    continue;
                }

                var fileContent = File.ReadAllText(filePath);
                try
                {
                    if (!m_Prompts.TryGetValue(type, out var promptData))
                    {
                        promptData = new Config();
                        m_Prompts.Add(type, promptData);
                    }

                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(fileContent);
                    promptData.SetConfig(config);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            UpdateCurrentPromptData();
        }

        void UpdateCurrentPromptData()
        {
            m_Prompts?.TryGetValue(m_CurrentType, out m_CurrentData);
        }

        async Task Recv(AIType type, string question, string msg)
        {
            m_OutputText += $"<color=#FFCC00>You</color>: {question}\n<color=#00CCFF>{type}</color>:{msg}\n\n";

            var match = Regex.Match(msg, @"```(?:csharp)?\s*(.*?)\s*```", RegexOptions.Singleline);
            if (match.Success)
            {
                var code = match.Groups[1].Value;
                await Evaluator.Instance.Evaluate(code);
            }
        }

        void OnEvaluationError(string output, Microsoft.CodeAnalysis.Scripting.CompilationErrorException error)
        {
            Debug.LogError(output);
            Debug.LogException(error);
        }

        async void OnEvaluationSuccess(object output)
        {
            var error = await Evaluator.Instance.EvaluateSilently("var method = typeof(TemplateClass).GetMethod(\"Test\", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static); method?.Invoke(null, null);");
            if (error != null)
                Debug.LogException(error);
        }

        void TextFileChanged()
        {
            InitPrompts();
        }
    }

    public enum AIType
    {
        OpenAI = 0,
        Bing,
        Bard,
    }

    [Serializable]
    class Config
    {
        // json key
        public string api_url;
        public Dictionary<string, string> prompts;

        // internal field
        public int Index;
        public string[] Names;

        public void SetConfig(Config config)
        {
            // copy
            api_url = config.api_url;
            prompts = config.prompts;

            // init
            Names = prompts.Keys.ToArray();
            if (Index < 0 || Index >= Names.Length)
                Index = 0;
        }

        public string GetValue()
        {
            var index = Index;
            if (index >= 0 && index < Names.Length)
            {
                var name = Names[index];
                prompts.TryGetValue(name, out var value);
                return value;
            }

            return string.Empty;
        }
    }

    class FilePostprocessor : AssetPostprocessor
    {
        public static event Action TextFileChanged;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var item in importedAssets)
            {
                var ext = Path.GetExtension(item);
                if (ext == ".json" || ext == ".txt")
                {
                    TextFileChanged?.Invoke();
                    break;
                }
            }
        }
    }
}