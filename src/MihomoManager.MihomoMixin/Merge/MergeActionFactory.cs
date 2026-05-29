namespace MihomoManager.MihomoMixin.Merge;

public sealed class MergeActionFactory : IMihomoMixinActionFactory
{
    public string Name => "merge";

    public int ParameterCount => 1;

    public IMihomoMixinAction Create(Span<string> arguments)
    {
        return new MergeAction(arguments[0]);
    }
}
