using MihomoManager.MihomoMixin;
using MihomoManager.MihomoMixin.Js;
using MihomoManager.MihomoMixin.Merge;
using MihomoManager.MihomoMixin.Output;

if (args.Length == 0)
{
    await Console.Error.WriteLineAsync("For help, please check https://github.com/MihomoManager/MihomoManager.MihomoMixin");
    return 1;
}

var knownActions = new IMihomoMixinActionFactory[]
{
    new MergeActionFactory(),
    new EditActionFactory(),
    new SaveActionFactory(),
    new JsActionFactory()
};
var knownActionsDictionary = new Dictionary<string, IMihomoMixinActionFactory>();
foreach (var action in knownActions)
{
    knownActionsDictionary.Add(action.Name, action);
}

var actions = new List<IMihomoMixinAction>();
async Task PrintActionsToErrorAsync()
{
    foreach (var (index, action) in actions.Index())
    {
        await Console.Error.WriteLineAsync($"{index + 1}. {action.ToStringForPrint()}");
    }
}

for (int i = 0; i < args.Length; )
{
    var actionName = args[i];

    if (!knownActionsDictionary.TryGetValue(actionName, out var action))
    {
        await Console.Error.WriteLineAsync($"Unknown action {actionName}. Resolved Actions:");
        await PrintActionsToErrorAsync();
        await Console.Error.WriteLineAsync($"Resolving. {actionName}    <-- Unknown action");
        return 1;
    }
    var argumentBegin = i + 1;
    i = argumentBegin + action.ParameterCount;

    if (i > args.Length)
    {
        var foundArguments = args[argumentBegin..];

        await Console.Error.WriteLineAsync($"No sufficient arguments provided for action {actionName}. Resolved Actions:");
        await PrintActionsToErrorAsync();
        await Console.Error.WriteLineAsync($"Resolving. {actionName}    <-- Only {foundArguments.Length} arguments provided but requires {action.ParameterCount}");
        await Console.Error.WriteLineAsync($"  Arguments found:");
        foreach (var (index, argument) in foundArguments.Index())
        {
            await Console.Error.WriteLineAsync($"    {index + 1}. {argument}");
        }
        return 1;
    }

    actions.Add(action.Create(args.AsSpan(argumentBegin, action.ParameterCount)));
}


var current = "{}";
foreach (var (index, action) in actions.Index())
{
    try
    {
        current = await action.MixinAsync(current);
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Failed to execute action {index + 1}.");
        await Console.Error.WriteLineAsync($"Action:");
        await Console.Error.WriteLineAsync($"{action.ToStringForPrint()}");
        await Console.Error.WriteLineAsync();
        await Console.Error.WriteLineAsync($"Input:");
        await Console.Error.WriteLineAsync($"{current}");
        await Console.Error.WriteLineAsync();
        await Console.Error.WriteLineAsync($"All Actions:");
        await PrintActionsToErrorAsync();
        await Console.Error.WriteLineAsync();
        await Console.Error.WriteLineAsync($"Exception:");
        await Console.Error.WriteLineAsync($"{ex}");
        return 1;
    }
}

return 0;
