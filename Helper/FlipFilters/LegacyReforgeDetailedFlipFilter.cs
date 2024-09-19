
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Matched unobtainable reforges, hurtful, demoinc, forceful strong, on swords also rich and bows also odd")]
public class LegacyReforgeDetailedFlipFilter : BoolDetailedFlipFilter
{
    public override Expression<Func<FlipInstance, bool>> GetStateExpression(bool target)
    {
        var generalReforges = new HashSet<ItemReferences.Reforge> {
            ItemReferences.Reforge.Hurtful, ItemReferences.Reforge.Demonic, ItemReferences.Reforge.Forceful, ItemReferences.Reforge.Strong,
            ItemReferences.Reforge.rich_sword, ItemReferences.Reforge.odd_bow};
        return a => generalReforges.Contains(a.Auction.Reforge);
    }
}