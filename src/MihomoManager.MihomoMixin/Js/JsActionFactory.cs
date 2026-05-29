namespace MihomoManager.MihomoMixin.Js;

public sealed class JsActionFactory : IMihomoMixinActionFactory
{
    public string Name => "js";

    public int ParameterCount => 1;

    public IMihomoMixinAction Create(Span<string> arguments)
    {
        return new JsAction(arguments[0]);
    }
}
