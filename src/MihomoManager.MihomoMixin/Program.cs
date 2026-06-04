using Jint;
using MihomoManager.MihomoMixin;
using MihomoManager.MihomoMixin.Edit;
using MihomoManager.MihomoMixin.Js;
using MihomoManager.MihomoMixin.Merge;
using MihomoManager.MihomoMixin.Save;
using System.Collections.Frozen;

internal class Program
{
    private static readonly FrozenDictionary<string, IMihomoMixinActionFactory> knownActions = new IMihomoMixinActionFactory[]
    {
        new MergeActionFactory(),
        new EditActionFactory(),
        new SaveActionFactory(),
        new JsActionFactory()
    }.ToFrozenDictionary(x => x.Name);

    private static async Task PrintActionsToErrorAsync(IEnumerable<IMihomoMixinAction> actions)
    {
        foreach (var (index, action) in actions.Index())
        {
            await Console.Error.WriteLineAsync($"{index + 1}. {action.ToStringForPrint()}");
        }
    }

    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            await Console.Error.WriteLineAsync("For help, please check https://github.com/MihomoManager/MihomoManager.MihomoMixin");
            return 1;
        }

        var actions = new List<IMihomoMixinAction>();

        for (int i = 0; i < args.Length;)
        {
            var actionName = args[i];

            if (!knownActions.TryGetValue(actionName, out var action))
            {
                await Console.Error.WriteLineAsync($"Unknown action {actionName}. Resolved Actions:");
                await PrintActionsToErrorAsync(actions);
                await Console.Error.WriteLineAsync($"Resolving. {actionName}    <-- Unknown action");
                return 1;
            }
            var argumentBegin = i + 1;
            i = argumentBegin + action.ParameterCount;

            if (i > args.Length)
            {
                var foundArguments = args[argumentBegin..];

                await Console.Error.WriteLineAsync($"No sufficient arguments provided for action {actionName}. Resolved Actions:");
                await PrintActionsToErrorAsync(actions);
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
                await PrintActionsToErrorAsync(actions);
                await Console.Error.WriteLineAsync();
                await Console.Error.WriteLineAsync($"Exception:");
                await Console.Error.WriteLineAsync($"{ex}");
                return 1;
            }
        }

        return 0;
    }
}