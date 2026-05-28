using SharpYaml;
using SharpYaml.Model;

namespace MihomoManager.MihomoMixin;

public sealed class EditAction(string configurationFile) : IMihomoMixinAction
{
    public string Mixin(string current)
    {
        var target = YamlSerializer.Deserialize<YamlElement>(current);
        var configuration = YamlSerializer.Deserialize<EditActionConfiguration>(configurationFile);
        if (configuration is null)
            return current;

        if (target is not YamlMapping root)
            root = [];
        EditRules(root, configuration.Rules);
        EditProxies(root, configuration.Proxies);
        // EditProxyGroups(root, configuration.ProxyGroups, configuration.ProxyGroupsInject);

        return YamlSerializer.Serialize(root);
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

    // private static void EditProxyGroups(
    //     YamlMapping root, 
    //     EditActionConfiguration.Edit edit,
    //     IReadOnlyDictionary<string, IReadOnlyList<string>> inject)
    // {
    //     _ = root.TryGetValue("proxy-groups", out var rulesElement);
    //     if (rulesElement is not YamlSequence originalRules)
    //         originalRules = [];
    // 
    //     var rules = (YamlSequence)originalRules.DeepClone();
    //     rules.Clear();
    //     foreach (var item in edit.Prepend)
    //         rules.Add(item);
    // 
    //     var deleteSet = edit.Delete.ToHashSet();
    //     foreach (var item in originalRules)
    //     {
    //         if (item is YamlMapping itemYamlMapping)
    //         {
    //             _ = itemYamlMapping.TryGetValue("name", out var nameNode);
    //             if (nameNode is YamlValue name)
    //             {
    //                 var nameValue = name.Value;
    //                 if (deleteSet.Contains(nameValue))
    //                     continue;
    // 
    //                 if (inject.ContainsKey(nameValue))
    //                 {
    //                     _ = itemYamlMapping.TryGetValue("proxies", out var proxiesNode);
    //                     if (proxiesNode is not YamlSequence proxies)
    //                     {
    //                         proxies = [];
    //                         itemYamlMapping["proxies"] = proxies;
    //                     }
    //                     foreach (var i in inject[nameValue])
    //                     {
    //                         proxies.Add(new YamlValue(i));
    //                     }
    //                 }
    //             }
    //         }
    //         rules.Add(item);
    //     }
    // 
    //     foreach (var item in edit.Append)
    //         rules.Add(item);
    // 
    //     root["proxy-groups"] = rules;
    // }
}
