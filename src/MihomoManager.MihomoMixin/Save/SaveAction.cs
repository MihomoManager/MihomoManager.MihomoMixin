using MihomoManager.MihomoMixin.Edit;
using SharpYaml;
using SharpYaml.Model;
using System.Diagnostics;

namespace MihomoManager.MihomoMixin.Output;

public sealed class SaveAction(string destination) : IMihomoMixinAction
{
    public async ValueTask<string> MixinAsync(string current)
    {
        await File.WriteAllTextAsync(destination, current);
        return current;
    }

    public string ToStringForPrint()
    {
        return
            $"""
            save
              destination: {destination}
            """;
    }
}
