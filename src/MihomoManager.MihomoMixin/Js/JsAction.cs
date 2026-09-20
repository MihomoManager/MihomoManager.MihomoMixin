using System.Diagnostics;
using System.Reflection;
using Jint;

namespace MihomoManager.MihomoMixin.Js;

public sealed class JsAction(string script) : IMihomoMixinAction
{
    private static string? jsYamlCache;
    private static async ValueTask<string> LoadJsYamlAsync()
    {
        if (jsYamlCache is null)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MihomoManager.MihomoMixin.Js.js-yaml.umd.min.js");
            Debug.Assert(stream is not null);
            using var reader = new StreamReader(stream);
            jsYamlCache = await reader.ReadToEndAsync();
        }
        return jsYamlCache;
    }

    public async ValueTask<string> MixinAsync(string current)
    {
        var scriptContent = await File.ReadAllTextAsync(script);

        using var engine = new Engine();
        _ = await engine.ExecuteAsync(await LoadJsYamlAsync());
        _ = await engine.ExecuteAsync(scriptContent);

        var config = engine.GetValue("jsyaml").Get("load").Call(current);
        var result = await engine.InvokeAsync("main", config, new Action<string>(Console.Error.WriteLine));
        return engine.GetValue("jsyaml").Get("dump").Call(result).AsString();
    }

    public string ToStringForPrint()
    {
        return
            $"""
            js
              script: {script}
            """;
    }
}
