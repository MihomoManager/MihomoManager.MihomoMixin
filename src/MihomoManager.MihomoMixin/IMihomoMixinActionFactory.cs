using System;
using System.Collections.Generic;
using System.Text;

namespace MihomoManager.MihomoMixin;

public interface IMihomoMixinActionFactory
{
    string Name { get; }
    int ParameterCount { get; }
    IMihomoMixinAction Create(Span<string> arguments);
}
