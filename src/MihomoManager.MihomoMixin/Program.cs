using MihomoManager.MihomoMixin;
using System.Text;


var knownActions = new Dictionary<string, Func<string, IMihomoMixinAction>>()
{
    ["merge"] = (s) => new MergeAction(s),
    ["edit"] = (s) => new EditAction(s)
};
var actions = new List<(int index, string action, string script)>();
async Task PrintActionsToErrorAsync()
{
    int actionWidth = knownActions.Keys.Max(x => x.Length);

    foreach (var action in actions)
    {
        await Console.Error.WriteLineAsync($"  {action.index}. {action.action.PadRight(actionWidth)}  {action.script}");
    }
}

foreach (var (index, arg) in args.Chunk(2).Index())
{
    if (arg.Length == 1)
    {
        await Console.Error.WriteLineAsync($"No script provided for action {arg[0]}. Resolved Actions:");
        await PrintActionsToErrorAsync();
        return 1;
    }
    if (!knownActions.ContainsKey(arg[0]))
    {
        await Console.Error.WriteLineAsync($"Unknown action {arg[0]}. Resolved Actions:");
        await PrintActionsToErrorAsync();
        return 1;
    }
    actions.Add((index + 1, arg[0], arg[1]));
}


var current = "{}";
foreach (var action in actions)
{
    string script;
    try
    {
        script = await File.ReadAllTextAsync(action.script);
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Failed to load script for action {action.index} ({action.action}).");
        await Console.Error.WriteLineAsync($"Script path:");
        await Console.Error.WriteLineAsync($"{action.script}");
        await Console.Error.WriteLineAsync();
        await Console.Error.WriteLineAsync($"All Actions:");
        await PrintActionsToErrorAsync();
        await Console.Error.WriteLineAsync();
        await Console.Error.WriteLineAsync($"Exception:");
        await Console.Error.WriteLineAsync($"{ex}");
        return 1;
    }

    try
    {
        current = knownActions[action.action](script).Mixin(current);
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Failed to execute action {action.index} ({action.action}).");
        await Console.Error.WriteLineAsync($"Script path:");
        await Console.Error.WriteLineAsync($"{action.script}");
        await Console.Error.WriteLineAsync();
        await Console.Error.WriteLineAsync($"Script:");
        await Console.Error.WriteLineAsync($"{script}");
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

await Console.Out.WriteLineAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(current)));
return 0;
