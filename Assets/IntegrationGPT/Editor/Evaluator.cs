using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Evaluation Manager
internal class Evaluator
{
    private static Evaluator instance = new Evaluator();
    public static Evaluator Instance { get { return instance; } }

    public event Action<object> OnEvaluationSuccess = delegate { };
    public event Action<string, CompilationErrorException> OnEvaluationError = delegate { };
    public event Action<string> OnBeforeEvaluation = delegate { };

    private AdhocWorkspace Workspace { get; set; }
    private SourceText Text { get; set; }
    private string Code { get; set; }

    private ScriptState ScriptState { get; set; }

    public static void Init(ref Evaluator value)
    {
        if (value == null)  // UI window opened
        {
            value = instance;
        }
        else // Domain reload
        {
            instance = value;
        }
    }

    public Evaluator()
    {
        Init();
    }

    public async void Init()
    {
        List<MetadataReference> references = new List<MetadataReference>();
        foreach (var assembly in Inspector.GetReferencableAssemblies())
            references.Add(MetadataReference.CreateFromFile(assembly.Location));

        var options = ScriptOptions.Default.WithReferences(references);

        ScriptState = await CSharpScript.RunAsync("", options);
    }

    public async Task Evaluate(string code)
    {
        OnBeforeEvaluation(code);
        var error = await EvaluateSilently(code);

        // Don't call delegate method in try/catch block to avoid silencing throws
        if (error == null)
            OnEvaluationSuccess(ScriptState.ReturnValue);
        else
        {
            var message = string.Join(Environment.NewLine, error.Diagnostics);
            OnEvaluationError(message, error);
        }
    }

    // Evaluate without telling the delegates
    public async Task<CompilationErrorException> EvaluateSilently(string code)
    {
        CompilationErrorException error = null;
        try
        {
            ScriptState = await ScriptState.ContinueWithAsync(code);
        }
        catch (CompilationErrorException e)
        {
            error = e;
        }

        return error;
    }

    // More then a little possibly not the best way to do this. There has to be a more
    // solid way to add a namespace without simply re-evaluating.
    public async Task<CompilationErrorException> AddNamespace(string ns)
    {
        return await AddNamespace(new List<string> { ns });
    }

    public async Task<CompilationErrorException> AddNamespace(IEnumerable<string> namespaces)
    {
        var code = "";
        foreach (var ns in namespaces)
            code += $"using {ns};\n";

        var error = await EvaluateSilently(code);
        if (error != null)
            Debug.Log("Error adding namespace: " + error.Message + "\n\n" + code);

        return error;
    }
}
