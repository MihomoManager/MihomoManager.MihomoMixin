namespace MihomoManager.MihomoMixin.Save;

public sealed class SaveActionFactory : IMihomoMixinActionFactory
{
    public string Name => "save";

    public int ParameterCount => 1;

    public IMihomoMixinAction Create(Span<string> arguments)
    {
        return new SaveAction(arguments[0]);
    }
}
