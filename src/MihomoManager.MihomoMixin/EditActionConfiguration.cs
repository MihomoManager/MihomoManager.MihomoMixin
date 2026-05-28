using SharpYaml.Model;
using static MihomoManager.MihomoMixin.EditActionConfiguration;

namespace MihomoManager.MihomoMixin;

internal sealed record EditActionConfiguration(
    Modification Rules, Modification Proxies, Modification ProxyGroups 
    // IReadOnlyDictionary<string, IReadOnlyList<string>> ProxyGroupsInject
)
{
    public sealed record Modification(
        IReadOnlyList<string> Delete,
        YamlSequence Prepend,
        YamlSequence Append
    );
}