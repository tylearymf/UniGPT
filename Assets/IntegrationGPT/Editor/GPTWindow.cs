using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor.Scripting.Python;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

        GUIStyle m_OutputStyle;
        Vector2 m_OutputPos;
        string m_OutputText;
        Vector2 m_InputPos;
        string m_InputText;

        AIType m_AIType;
        int[] m_AITypeIDs;
        string[] m_AITypeNames;

        void OnEnable()
        {
            s_Instance = this;

            Evaluator.Init(ref m_Evaluator);
            Evaluator.Instance.OnEvaluationSuccess -= OnEvaluationSuccess;
            Evaluator.Instance.OnEvaluationSuccess += OnEvaluationSuccess;
            Evaluator.Instance.OnEvaluationError -= OnEvaluationError;
            Evaluator.Instance.OnEvaluationError += OnEvaluationError;

            System.Environment.SetEnvironmentVariable("all_proxy", "socks5://127.0.0.1:30801/");

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
        }

        void OnDisable()
        {
            if (Evaluator.Instance != null)
            {
                Evaluator.Instance.OnEvaluationSuccess -= OnEvaluationSuccess;
                Evaluator.Instance.OnEvaluationError -= OnEvaluationError;
            }
        }

        void OnGUI()
        {
            if (m_OutputStyle == null)
            {
                m_OutputStyle = new GUIStyle(EditorStyles.textField);
                m_OutputStyle.richText = true;
            }

            var windowSize = position.size;
            var extraHeight = 65;
            var labelHeight = 16;
            windowSize.y -= extraHeight;
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

            EditorGUILayout.LabelField(" Input Command", EditorStyles.boldLabel, GUILayout.Height(labelHeight));
            m_InputPos = EditorGUILayout.BeginScrollView(m_InputPos, GUILayout.Height(inputHeight));
            {
                m_InputText = GUILayout.TextArea(m_InputText, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginVertical(GUILayout.Height(extraHeight));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    m_AIType = (AIType)EditorGUILayout.IntPopup((int)m_AIType, m_AITypeNames, m_AITypeIDs);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Execute", GUILayout.Height(30)))
                {
                    var text = m_InputText.Trim();
                    m_Builder.Clear();
                    m_Builder.AppendLine("import gpt");
                    m_Builder.AppendLine($"gpt.ask_{m_AIType.ToString().ToLower()}('{text}')");
                    PythonRunner.RunString(m_Builder.ToString(), "__main__");
                }
            }
            EditorGUILayout.EndVertical();
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
    }

    public enum AIType
    {
        OpenAI = 0,
        Bing,
        Bard,
    }
}