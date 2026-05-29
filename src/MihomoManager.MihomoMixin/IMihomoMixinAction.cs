using System;
using System.Collections.Generic;
using System.Text;

namespace MihomoManager.MihomoMixin;

public interface IMihomoMixinAction
{
    ValueTask<string> MixinAsync(string current);
    string ToStringForPrint();
}
