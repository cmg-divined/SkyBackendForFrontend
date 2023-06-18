namespace Coflnet.Sky.Commands.Shared;

public class LastMayorDetailedFlipFilter : CurrentMayorDetailedFlipFilter
{
    protected override string TargetMayor()
    {
        return DiHandler.GetService<Sky.Mayor.Client.Api.IMayorApi>().MayorLastGet();
    }
}
