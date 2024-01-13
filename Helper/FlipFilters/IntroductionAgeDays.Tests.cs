using System.Collections.Generic;
using Coflnet.Sky.Core;
using Coflnet.Sky.Items.Client.Api;
using Moq;
using NUnit.Framework;

namespace Coflnet.Sky.Commands.Shared;

public class IntroductionAgeDaysTests
{
    [Test]
    public void ShouldNotShowNewItem()
    {
        var mock = new Mock<IItemsApi>();
        DiHandler.OverrideService<IItemsApi, IItemsApi>(mock.Object);
        DiHandler.OverrideService<Mayor.Client.Api.IMayorApi, Mayor.Client.Api.IMayorApi>(new Mock<Mayor.Client.Api.IMayorApi>().Object);
        DiHandler.OverrideService<FilterStateService, FilterStateService>(new FilterStateService());
        mock.Setup(x => x.ItemsRecentGet(1, 0)).Returns(new List<string>() { "different" });
        ItemDetails.Instance.TagLookup = new System.Collections.Concurrent.ConcurrentDictionary<string, int>(
            new Dictionary<string, int>() {
                { "different", 1 },
                { "diff2", 1 },
                { "diff3", 3},
                { "diff4", 4},
                { "diff5", 5},
                { "diff6", 6},
                { "diff7", 7},
                { "diff8", 8},
                { "diff9", 9},
                { "diff10", 10},
                { "diff11", 11}
            }
        );
        var filter = new IntroductionAgeDaysDetailedFlipFilter();
        var comparer = filter.GetExpression(new Dictionary<string, string>(), "1").Compile();
        Assert.IsNotNull(comparer);
        var flipSample = new FlipInstance() { Auction = new Core.SaveAuction() { Tag = "test" } };
        // adding new item now does not change
        ItemDetails.Instance.TagLookup.TryAdd("test", 1);
        Assert.IsTrue(comparer(flipSample));
    }
}