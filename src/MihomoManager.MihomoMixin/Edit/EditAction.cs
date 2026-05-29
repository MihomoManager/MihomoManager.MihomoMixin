using SharpYaml;
using SharpYaml.Model;

namespace MihomoManager.MihomoMixin.Edit;

public sealed class EditAction(string configurationFile) : IMihomoMixinAction
{
    public async ValueTask<string> MixinAsync(string current)
    {
        var target = YamlSerializer.Deserialize<YamlElement>(current);
        var configurationFileContent = await File.ReadAllTextAsync(configurationFile);
        var configuration = YamlSerializer.Deserialize<EditActionConfiguration>(configurationFileContent);
        if (configuration is null)
            return current;

        if (target is not YamlMapping root)
            root = [];
        EditRules(root, configuration.Rules);
        EditProxies(root, configuration.Proxies);
        EditProxyGroups(root, configuration.ProxyGroups);

        return YamlSerializer.Serialize(root);
    }

    public string ToStringForPrint()
    {
        return
            $"""
            edit
              configurationFile: {configurationFile}
            """;
    }
    private static void EditRules(YamlMapping root, EditActionConfiguration.Modification edit)
    {
        _ = root.TryGetValue("rules", out var rulesElement);
        if (rulesElement is not YamlSequence originalRules)
            originalRules = [];

        var rules = (YamlSequence)originalRules.DeepClone();
        rules.Clear();
        foreach (var item in edit.Prepend)
            rules.Add(item);

        var deleteSet = edit.Delete.ToHashSet();
        foreach (var item in originalRules)
        {
            if (item is YamlValue itemYamlValue && deleteSet.Contains(itemYamlValue.Value))
                continue;
            rules.Add(item);
        }

        foreach (var item in edit.Append)
            rules.Add(item);

        root["rules"] = rules;
    }

    private static void EditProxies(YamlMapping root, EditActionConfiguration.Modification edit)
    {
        _ = root.TryGetValue("proxies", out var rulesElement);
        if (rulesElement is not YamlSequence originalRules)
            originalRules = [];

        var rules = (YamlSequence)originalRules.DeepClone();
        rules.Clear();
        foreach (var item in edit.Prepend)
            rules.Add(item);

        var deleteSet = edit.Delete.ToHashSet();
        foreach (var item in originalRules)
        {
            if (item is YamlMapping itemYamlMapping)
            {
                _ = itemYamlMapping.TryGetValue("name", out var nameNode);
                if (nameNode is YamlValue name && deleteSet.Contains(name.Value))
                    continue;
            }
            rules.Add(item);
        }

        foreach (var item in edit.Append)
            rules.Add(item);

        root["proxies"] = rules;
    }

    private static void EditProxyGroups(
        YamlMapping root, 
        EditActionConfiguration.Modification edit)
    {
        _ = root.TryGetValue("proxy-groups", out var rulesElement);
        if (rulesElement is not YamlSequence originalRules)
            originalRules = [];

        var rules = (YamlSequence)originalRules.DeepClone();
        rules.Clear();
        foreach (var item in edit.Prepend)
            rules.Add(item);

        var deleteSet = edit.Delete.ToHashSet();
        foreach (var item in originalRules)
        {
            if (item is YamlMapping itemYamlMapping)
            {
                _ = itemYamlMapping.TryGetValue("name", out var nameNode);
                if (nameNode is YamlValue name && deleteSet.Contains(name.Value))
                    continue;
            }
            rules.Add(item);
        }

        foreach (var item in edit.Append)
            rules.Add(item);

        root["proxy-groups"] = rules;
    }
}
