
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("How many ah spaces are left, is -1 if not supported and 0 if not known")]
public class ListingSlotsLeft : NumberDetailedFlipFilter
{
    public override object[] Options => new object[] { 1, 20 };
    protected override Expression<Func<FlipInstance, double>> GetSelector(FilterContext filters)
    {
        var slots = filters.playerInfo?.AhSlotsOpen ?? -1;
        return f => slots;
    }
}