using SharpYaml.Model;
using static MihomoManager.MihomoMixin.Edit.EditActionConfiguration;

namespace MihomoManager.MihomoMixin.Edit;

internal sealed record EditActionConfiguration(
    Modification Rules, Modification Proxies, Modification ProxyGroups
)
{
    public sealed record Modification(
        IReadOnlyList<string> Delete,
        YamlSequence Prepend,
        YamlSequence Append
    );
}