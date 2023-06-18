namespace Coflnet.Sky.Commands.Shared;

public class NextMayorDetailedFlipFilter : CurrentMayorDetailedFlipFilter
{
    protected override string TargetMayor()
    {
        return DiHandler.GetService<Sky.Mayor.Client.Api.IMayorApi>().MayorNextGet().Name;
    }
}