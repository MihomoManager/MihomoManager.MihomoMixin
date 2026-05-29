using MihomoManager.MihomoMixin.Edit;
using SharpYaml;
using SharpYaml.Model;
using System.Diagnostics;

namespace MihomoManager.MihomoMixin.Merge;

public sealed class MergeAction(string fileToMerge) : IMihomoMixinAction
{
    public async ValueTask<string> MixinAsync(string current)
    {
        var target = YamlSerializer.Deserialize<YamlElement>(current);
        var configurationFileContent = await File.ReadAllTextAsync(fileToMerge);
        var value = YamlSerializer.Deserialize<YamlElement>(configurationFileContent);
        return YamlSerializer.Serialize(this.Merge(target, value));
    }

    public string ToStringForPrint()
    {
        return
            $"""
            merge
              fileToMerge: {fileToMerge}
            """;
    }

    private YamlElement? Merge(YamlElement? target, YamlElement? value)
    {
        if (target is YamlMapping targetMapping)
        {
            if (value is YamlMapping valueMapping)
                return this.MergeMappingTargetInplace(targetMapping, valueMapping);
            else
                return value;
        }
        if (target is YamlSequence targetSequence)
        {
            if (value is YamlSequence valueSequence)
                return this.MergeSequenceValueInplace(targetSequence, valueSequence);
            else
                return value;
        }
        return value;
    }

    private YamlMapping MergeMappingTargetInplace(YamlMapping target, YamlMapping value)
    {
        foreach (var item in value)
        {
            if (item.Key is YamlValue keyYamlValue && target.ContainsKey(keyYamlValue.Value))
            {
                target[keyYamlValue.Value] = this.Merge(target[keyYamlValue.Value], item.Value);
                continue;
            }
            target.Add(item.Key, item.Value);
        }
        return target;
    }

    private YamlSequence MergeSequenceValueInplace(YamlSequence target, YamlSequence value)
    {
        var names = new Dictionary<string, int>();
        var strings = new HashSet<string>();

        foreach (var (index, item) in value.Index())
        {
            if (item is YamlMapping mapping)
            {
                _ = mapping.TryGetValue("name", out var name);
                if (name is YamlValue nameValue)
                {
                    names.Add(nameValue.Value, index);
                }
            }
            else if (item is YamlValue yamlValue)
            {
                _ = strings.Add(yamlValue.Value);
            }
        }

        foreach (var item in target)
        {
            if (item is YamlMapping mapping)
            {
                _ = mapping.TryGetValue("name", out var name);
                if (name is YamlValue nameValue)
                {
                    if (names.TryGetValue(nameValue.Value, out var index))
                    {
                        value[index] = this.MergeMappingTargetInplace(mapping, (YamlMapping)value[index]);
                        continue;
                    }
                }
            }
            else if (item is YamlValue yamlValue)
            {
                if (strings.Contains(yamlValue.Value))
                    continue;
            }

            value.Add(item);
        }
        return value;
    }
}
