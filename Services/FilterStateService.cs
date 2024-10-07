using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coflnet.Sky.Core;
using Coflnet.Sky.Items.Client.Model;
using Coflnet.Sky.Mayor.Client.Model;
using dev;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Coflnet.Sky.Commands.Shared
{
    public class FilterStateService
    {
        public class FilterState
        {
            public ConcurrentDictionary<ItemCategory, HashSet<string>> itemCategories { get; set; } = new();
            public string CurrentMayor { get; set; }
            public string NextMayor { get; set; }
            public string PreviousMayor { get; set; }
            public DateTime LastUpdate { get; set; }
            public Dictionary<int, HashSet<string>> IntroductionAge { get; set; } = new();
            public HashSet<string> ExistingTags { get; set; } = new();
            public List<string> CurrentPerks { get; set; } = new();
        }

        private SemaphoreSlim updateLock = new SemaphoreSlim(1, 1);

        public FilterState State { get; set; } = new FilterState();

        private Coflnet.Sky.Mayor.Client.Api.IMayorApi mayorApi;
        private Coflnet.Sky.Items.Client.Api.IItemsApi itemsApi;
        private ILogger<FilterStateService> logger;

        public FilterStateService(ILogger<FilterStateService> logger, Coflnet.Sky.Mayor.Client.Api.IMayorApi mayorApi, Coflnet.Sky.Items.Client.Api.IItemsApi itemsApi)
        {
            this.logger = logger;
            this.mayorApi = mayorApi;
            this.itemsApi = itemsApi;
        }

        public async Task UpdateState()
        {
            if (DateTime.Now - State.LastUpdate > TimeSpan.FromHours(1))
            {
                State.LastUpdate = DateTime.Now;
            }
            else
                return;

            try
            {
                // Fetch previous mayor
                State.PreviousMayor = (await mayorApi.MayorLastGetAsync()).ToLower();

                // Fetch current mayor
                var response = await mayorApi.MayorCurrentGetWithHttpInfoAsync();
                try
                {
                    var mayorResponse = JsonConvert.DeserializeObject<ModelCandidate>(response.Data.ToString());
                    State.CurrentMayor = mayorResponse?.Name?.ToLower();
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"Could not load current mayor: {response.Data}");
                    throw new Exception("Error deserializing mayor data", ex);
                }

                // Fetch next mayor
                State.NextMayor = (await mayorApi.MayorNextGetAsync())?.Name?.ToLower();

                logger.LogInformation("Current mayor is {current}", State.CurrentMayor);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not load mayor information");
            }

            // Load item categories
            foreach (var item in State.itemCategories.Keys)
            {
                try
                {
                    GetItemCategory(item);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Could not load item category {0}", item);
                }
            }

            // Load existing item tags
            var items = await itemsApi.ItemNamesGetAsync();
            foreach (var item in items.Select(i => i.Tag))
            {
                State.ExistingTags.Add(item);
            }
            logger.LogInformation("Loaded {0} item tags", State.ExistingTags.Count);
        }

        public void GetItemCategory(ItemCategory category)
        {
            var items = itemsApi.ItemsCategoryCategoryItemsGet(category);
            if (!State.itemCategories.ContainsKey(category))
            {
                State.itemCategories[category] = new HashSet<string>();
            }

            foreach (var item in items)
            {
                State.itemCategories[category].Add(item);
            }
        }

        public HashSet<string> GetIntroductionAge(int days)
        {
            if (!State.IntroductionAge.ContainsKey(days))
            {
                var items = itemsApi.ItemsRecentGet(days);
                if (items == null && days == 1)
                    return new HashSet<string>(); // handled via known tags

                if (items == null)
                {
                    Activity.Current?.AddTag("error", "could_not_load");
                    throw new CoflnetException("could_not_load", $"Could not load new items from {days} days");
                }
                State.IntroductionAge[days] = new HashSet<string>(items);
            }
            return State.IntroductionAge[days];
        }

        public async Task UpdateState(FilterState newState)
        {
            if (updateLock.CurrentCount == 0)
            {
                return;
            }

            try
            {
                await updateLock.WaitAsync();
                UpdateState(newState, State);
            }
            finally
            {
                updateLock.Release();
            }
        }

        private static void UpdateState(FilterState newState, FilterState local)
        {
            local.CurrentMayor = newState.CurrentMayor;
            local.NextMayor = newState.NextMayor;
            local.PreviousMayor = newState.PreviousMayor;

            foreach (var item in newState.ExistingTags)
            {
                local.ExistingTags.Add(item);
            }

            foreach (var day in newState.IntroductionAge)
            {
                local.IntroductionAge.TryAdd(day.Key, new());
                foreach (var item in day.Value)
                {
                    local.IntroductionAge[day.Key].Add(item);
                }
            }

            foreach (var item in newState.itemCategories)
            {
                local.itemCategories.AddOrUpdate(item.Key, item.Value, (k, v) => v.Union(item.Value).ToHashSet());
            }

            local.LastUpdate = DateTime.UtcNow;
        }
    }
}
