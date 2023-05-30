
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class FlipFilters
    {
        [DataMember(Name = "minProfit")]
        [SettingsDoc("Minimum profit of flips")]
        public long MinProfit;

        [DataMember(Name = "minProfitPercent")]
        [SettingsDoc("Minimum profit Percentage")]
        public int MinProfitPercent;

        [SettingsDoc("The minimum sales per 24 hours (has decimals)")]
        [DataMember(Name = "minVolume")]
        public double MinVolume;

        [SettingsDoc("Maximium cost of flips")]
        [DataMember(Name = "maxCost")]
        public long MaxCost;
    }


    [DataContract]
    public class FlipSettings : FlipFilters
    {
        [DataMember(Name = "filters")]
        public Dictionary<string, string> Filters;
        [DataMember(Name = "blacklist")]
        public List<ListEntry> BlackList;
        [DataMember(Name = "whitelist")]
        public List<ListEntry> WhiteList;

        [DataMember(Name = "lbin")]
        [SettingsDoc("Calculate profit based on lowest bin")]
        public bool BasedOnLBin;

        [DataMember(Name = "visibility")]
        public VisibilitySettings Visibility;

        [DataMember(Name = "mod")]
        public ModSettings ModSettings;

        [DataMember(Name = "finders")]
        [SettingsDoc("Which algorithms are selected for price estimation")]
        public LowPricedAuction.FinderType AllowedFinders;

        [DataMember(Name = "fastMode")]
        [SettingsDoc("Use the fast lane flips", true)]
        public bool FastMode;

        /// <summary>
        /// The initiating party that sent the change
        /// </summary>
        [DataMember(Name = "changer")]
        [SettingsDoc("The last changer of the settings", true)]
        public string Changer;
        [DataMember(Name = "onlyBin")]
        [SettingsDoc("Hide all auctions (not buy item now)")]
        public bool OnlyBin;
        [DataMember(Name = "whitelistAftermain")]
        [SettingsDoc("whitelisted items will only show if they also meet main filters (min profit etc)")]
        public bool WhitelistAfterMain;

        private List<FlipFilter> blackListFilters;
        private ListMatcher BlackListMatcher;
        private ListMatcher ForcedBlackListMatcher;
        private ListMatcher WhiteListMatcher;
        private ListMatcher AfterMainWhiteListMatcher;
        private Func<FlipInstance, bool> generalFilter;

        [SettingsDoc("Stop receiving any flips (just use other features) also stops the timer")]
        public bool DisableFlips;
        [SettingsDoc("Outputs more information to help with debugging issues")]
        public bool DebugMode;

        [DataMember(Name = "lastChange")]
        [SettingsDoc("", true)]
        public string LastChanged { get; set; }

        /// <summary>
        /// Determines if a flip matches a the <see cref="Filters"/>> of this instance
        /// </summary>
        /// <param name="flip"></param>
        /// <returns>true if it matches</returns>
        public (bool, string) MatchesSettings(FlipInstance flip)
        {
            if (IsFinderBlocked(flip.Finder))
                return (false, "finder " + flip.Finder.ToString());

            if (OnlyBin && !flip.Auction.Bin)
                return (false, "not bin");

            MakeSureMatchersAreInitialized();

            try
            {
                var forceBlacklistMatch = ForcedBlackListMatcher.IsMatch(flip);
                if (forceBlacklistMatch.Item1)
                    return (false, "forced blacklist " + forceBlacklistMatch.Item2);
            }
            catch (System.Exception e)
            {
                LogError(flip, "forced blacklist", ForcedBlackListMatcher, e);
            }
            try
            {
                var match = WhiteListMatcher.IsMatch(flip);
                if (match.Item1)
                    return (true, "whitelist " + match.Item2);
            }
            catch (System.Exception e)
            {
                LogError(flip, "whitelist ", WhiteListMatcher, e);
            }

            var main = MainSettingsMatch(flip);
            if (!main.Item1)
                return main;

            var awmatch = AfterMainWhiteListMatcher.IsMatch(flip);
            if (awmatch.Item1)
                return (true, "whitelist am " + awmatch.Item2);

            try
            {
                var match = BlackListMatcher.IsMatch(flip);
                if (match.Item1)
                    return (false, "blacklist " + match.Item2);
            }
            catch (System.Exception e)
            {
                LogError(flip, "blacklist ", BlackListMatcher, e);
            }

            return (true, "general filter");
        }

        private static void LogError(FlipInstance flip, string title, ListMatcher matcher, Exception e)
        {
            Activity.Current?.AddTag("flip", JSON.Stringify(flip));
            Activity.Current?.AddTag("filter", JSON.Stringify(matcher.FullList.Select(x => new { x.ItemTag, x.filter })));
            throw new Exception("Error while matching " + title + " " + e.Message, e);
        }

        private void MakeSureMatchersAreInitialized()
        {
            if (ForcedBlackListMatcher == null)
                ForcedBlackListMatcher = new ListMatcher(GetForceBlacklist());
            if (WhiteListMatcher == null)
                WhiteListMatcher = new ListMatcher(WhiteList?.Except(GetAfterMainWhitelist()).ToList());
            if (AfterMainWhiteListMatcher == null)
                AfterMainWhiteListMatcher = new ListMatcher(GetAfterMainWhitelist());
            if (BlackListMatcher == null)
                BlackListMatcher = new ListMatcher(BlackList?.Except(GetForceBlacklist()).ToList());
        }

        private (bool, string) MainSettingsMatch(FlipInstance flip)
        {
            if (flip.Volume < MinVolume)
                return (false, "minVolume");
            if (MaxCost != 0 && flip.LastKnownCost > MaxCost)
                return (false, "maxCost");
            GetPrice(flip, out long targetPrice, out long profit);
            if (profit < MinProfit)
                return (false, "minProfit");
            if (flip.LastKnownCost > 0 && (flip.ProfitPercentage < MinProfitPercent
                || BasedOnLBin && (profit * 100 / flip.LastKnownCost < MinProfitPercent)))
            {
                return (false, "profit Percentage");
            }
            if (flip.Auction == null)
                return (false, "auction not set");
            return (true, null);
        }

        public void CopyListMatchers(FlipSettings other)
        {
            if (other == null)
                return;
            if (other.ForcedBlackListMatcher != null && other.BlackList?.SequenceEqual(BlackList) == true)
                ForcedBlackListMatcher = other.ForcedBlackListMatcher;
            if (other.WhiteListMatcher != null && other.WhiteList?.SequenceEqual(WhiteList) == true)
                WhiteListMatcher = other.WhiteListMatcher;
            if (other.AfterMainWhiteListMatcher != null && other.WhiteList?.SequenceEqual(WhiteList) == true)
                AfterMainWhiteListMatcher = other.AfterMainWhiteListMatcher;
            if (other.BlackListMatcher != null && other.BlackList?.SequenceEqual(BlackList) == true)
                BlackListMatcher = other.BlackListMatcher;
        }

        public void RecompileMatchers()
        {
            ClearListMatchers();
            MakeSureMatchersAreInitialized();
        }

        public void ClearListMatchers()
        {
            ForcedBlackListMatcher = null;
            WhiteListMatcher = null;
            AfterMainWhiteListMatcher = null;
            BlackListMatcher = null;
        }

        public List<ListEntry> GetForceBlacklist()
        {
            return BlackList?.Where(b => b.filter?.Where(f => f.Key == "ForceBlacklist").Any() ?? false).ToList();
        }
        public List<ListEntry> GetAfterMainWhitelist()
        {
            return WhiteList?.Where(b => WhitelistAfterMain || (b.filter?.Where(f => f.Key == "AfterMainFilter").Any() ?? false)).ToList();
        }

        public bool IsFinderBlocked(LowPricedAuction.FinderType finder)
        {
            return finder == LowPricedAuction.FinderType.UNKOWN ||
                    AllowedFinders == LowPricedAuction.FinderType.UNKOWN && 
                        !(LowPricedAuction.FinderType.SNIPERS).HasFlag(finder)
                                                || AllowedFinders != LowPricedAuction.FinderType.UNKOWN && !AllowedFinders.HasFlag(finder)
                                                && (int)finder != 3;
        }

        /// <summary>
        /// Calculates the displayed price and profit
        /// </summary>
        /// <param name="flip"></param>
        /// <param name="targetPrice"></param>
        /// <param name="profit"></param>
        public void GetPrice(FlipInstance flip, out long targetPrice, out long profit)
        {
            targetPrice = (BasedOnLBin || flip.Finder == LowPricedAuction.FinderType.SNIPER ? (flip.LowestBin ?? 0) : flip.MedianPrice);
            if (targetPrice > 1_000_000)
                profit = targetPrice * 98 / 100 - flip.LastKnownCost;
            else
                profit = targetPrice * 99 / 100 - flip.LastKnownCost;
        }

        public override bool Equals(object obj)
        {
            return obj is FlipSettings settings &&
                   EqualityComparer<Dictionary<string, string>>.Default.Equals(Filters, settings.Filters) &&
                   EqualityComparer<List<ListEntry>>.Default.Equals(BlackList, settings.BlackList) &&
                   EqualityComparer<List<ListEntry>>.Default.Equals(WhiteList, settings.WhiteList) &&
                   BasedOnLBin == settings.BasedOnLBin &&
                   MinProfit == settings.MinProfit &&
                   MinProfitPercent == settings.MinProfitPercent &&
                   MinVolume == settings.MinVolume &&
                   MaxCost == settings.MaxCost &&
                   EqualityComparer<VisibilitySettings>.Default.Equals(Visibility, settings.Visibility) &&
                   EqualityComparer<ModSettings>.Default.Equals(ModSettings, settings.ModSettings) &&
                   AllowedFinders == settings.AllowedFinders &&
                   FastMode == settings.FastMode &&
                   Changer == settings.Changer &&
                   OnlyBin == settings.OnlyBin &&
                   EqualityComparer<List<FlipFilter>>.Default.Equals(blackListFilters, settings.blackListFilters) &&
                   EqualityComparer<ListMatcher>.Default.Equals(BlackListMatcher, settings.BlackListMatcher) &&
                   EqualityComparer<ListMatcher>.Default.Equals(WhiteListMatcher, settings.WhiteListMatcher);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Filters);
            hash.Add(BlackList);
            hash.Add(WhiteList);
            hash.Add(BasedOnLBin);
            hash.Add(MinProfit);
            hash.Add(MinProfitPercent);
            hash.Add(MinVolume);
            hash.Add(MaxCost);
            hash.Add(Visibility);
            hash.Add(ModSettings);
            hash.Add(AllowedFinders);
            hash.Add(FastMode);
            hash.Add(Changer);
            hash.Add(OnlyBin);
            hash.Add(blackListFilters);
            hash.Add(BlackListMatcher);
            hash.Add(WhiteListMatcher);
            return hash.ToHashCode();
        }

        public class ListMatcher
        {
            public List<ListEntry> FullList { get; }
            private HashSet<string> Ids = new HashSet<string>();
            private List<ListEntry> RemainingFilters = new List<ListEntry>();
            Dictionary<string, Func<FlipInstance, bool>> Matchers = new Dictionary<string, Func<FlipInstance, bool>>();


            public ListMatcher(List<ListEntry> BlackList)
            {
                if (BlackList == null || BlackList.Count == 0)
                    return;
                this.FullList = new(BlackList.Select(b => b.Clone()));
                ConcurrentDictionary<string, List<ListEntry>> forTags = ExtractFiltersForTags();
                foreach (var item in FullList)
                {
                    AddFiltersBasedOnTags(forTags, item);
                    AddElement(item);
                    if (item.ItemTag != null)
                        AddElement(new ListEntry()
                        {
                            filter = item.filter,
                            ItemTag = "STARRED_" + item.ItemTag
                        });
                }
                ConcurrentDictionary<string, Expression<Func<FlipInstance, bool>>> isMatch = new();
                foreach (var item in RemainingFilters)
                {
                    string key = KeyFromTag(item.ItemTag);
                    isMatch.AddOrUpdate(key, item.GetExpression(), (k, old) => old.Or(item.GetExpression()));
                }
                AssignMatchers(isMatch);
            }

            private void AssignMatchers(ConcurrentDictionary<string, Expression<Func<FlipInstance, bool>>> isMatch)
            {
                foreach (var item in isMatch)
                {
                    try
                    {
                        Matchers.Add(item.Key, item.Value.Compile());

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Could not compile matcher for {item.Key}  {item.Value}");
                        throw e;
                    }
                }
            }

            private ConcurrentDictionary<string, List<ListEntry>> ExtractFiltersForTags()
            {
                var forTags = new ConcurrentDictionary<string, List<ListEntry>>();
                foreach (var item in FullList.Where(b => b.filter != null && b.filter.Any(f => f.Key == FlipFilter.FilterForName)).ToList())
                {
                    var tags = item.filter.Where(f => f.Key == FlipFilter.FilterForName).Select(f => f.Value).First().Split(',');
                    foreach (var tag in tags)
                    {
                        var element = forTags.GetOrAdd(tag, new List<ListEntry>());
                        element.Add(item);
                    }
                    FullList.Remove(item);
                }

                return forTags;
            }

            private static void AddFiltersBasedOnTags(ConcurrentDictionary<string, List<ListEntry>> forTags, ListEntry item)
            {
                if (item.Tags != null && item.Tags.Any())
                {
                    foreach (var tag in item.Tags)
                    {
                        if (!forTags.TryGetValue(tag, out var list))
                            continue;
                        if (item.filter == null)
                            item.filter = new();
                        foreach (var element in list)
                        {
                            foreach (var filter in element.filter)
                            {
                                if (item.filter.Any(f => f.Key == filter.Key) || filter.Key == FlipFilter.FilterForName)
                                    continue;
                                item.filter.Add(filter.Key, filter.Value);
                            }
                        }
                    }
                }
            }

            private static string KeyFromTag(string tag)
            {
                var key = "";
                if (tag != null)
                    key = tag;
                return key;
            }

            private void AddElement(ListEntry item)
            {
                if (item.filter == null || item.filter.Count == 0 || (item.filter.Count == 1 && item.filter.First().Key == "ForceBlacklist"))
                    Ids.Add(item.ItemTag);
                else
                    RemainingFilters.Add(item);
            }

            public (bool, string) IsMatch(FlipInstance flip)
            {
                if (Ids.Contains(flip.Auction.Tag))
                    return (true, "for " + flip.Auction.Tag);

                if (flip.Auction.Tag != null && Matchers.TryGetValue(flip.Auction.Tag, out Func<FlipInstance, bool> matcher) && matcher(flip))
                    return (true, "matched filter for item");
                // general filters without a tag
                if (Matchers.TryGetValue("", out matcher) && matcher(flip))
                    return (true, "matched general filter");
                /*foreach (var item in RemainingFilters)
                {
                    if (item.MatchesSettings(flip))
                        return (true, $"filter for {item.filter.Keys.First()}: {item.filter.Values.First()}");
                }*/
                return (false, "no match");
            }
        }

    }

}
