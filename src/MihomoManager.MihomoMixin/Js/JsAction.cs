using Jint;
using SharpYaml;

namespace MihomoManager.MihomoMixin.Js;

public sealed class JsAction(string script) : IMihomoMixinAction
{
    public async ValueTask<string> MixinAsync(string current)
    {
        var target = YamlSerializer.Deserialize<object>(current);
        var scriptContent = await File.ReadAllTextAsync(script);

        using var engine = new Engine();
        _ = await engine.ExecuteAsync(scriptContent);

        var result = await engine.InvokeAsync("main", 
            target, new Action<string>(Console.Error.WriteLine));
        return YamlSerializer.Serialize(target);
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
