using System;

namespace Coflnet.Sky.Commands.Shared;

public class LastMayorDetailedFlipFilter : CurrentMayorDetailedFlipFilter
{
    protected override Func<string> TargetMayor(FilterStateService service)
    {
        return ()=> service.State.PreviousMayor;
    }
}
