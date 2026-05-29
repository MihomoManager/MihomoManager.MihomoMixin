using MihomoManager.MihomoMixin.Edit;
using MihomoManager.MihomoMixin.Merge;
using SharpYaml;
using SharpYaml.Model;

namespace MihomoManager.MihomoMixin;

public sealed class EditActionFactory : IMihomoMixinActionFactory
{
    public string Name => "edit";

    public int ParameterCount => 1;

    public IMihomoMixinAction Create(Span<string> arguments)
    {
        return new EditAction(arguments[0]);
    }
}
