using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coflnet.Sky.Items.Client.Model;
using Coflnet.Sky.Mayor.Client.Model;
using Newtonsoft.Json;

namespace Coflnet.Sky.Commands.Shared;
public class FilterStateService
{
    public class FilterState
    {

        public ConcurrentDictionary<ItemCategory, HashSet<string>> itemCategories { get; set; } = new();
        public string CurrentMayor { get; set; }
        public string NextMayor { get; set; }
        public string PreviousMayor { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public FilterState State { get; set; } = new FilterState();

    private Mayor.Client.Api.IMayorApi mayorApi;
    private Items.Client.Api.IItemsApi itemsApi;

    public FilterStateService()
    {
        mayorApi = DiHandler.GetService<Mayor.Client.Api.IMayorApi>();
        itemsApi = DiHandler.GetService<Items.Client.Api.IItemsApi>();
    }

    public async Task UpdateState()
    {
        if (DateTime.Now - State.LastUpdate > TimeSpan.FromHours(1))
        {
            State.LastUpdate = DateTime.Now;
        }
        else
            return;
        var response = await mayorApi.MayorCurrentGetWithHttpInfoAsync();
        State.CurrentMayor = JsonConvert.DeserializeObject<ModelCandidate>(response.Data.ToString()).Name;
        State.NextMayor = mayorApi.MayorNextGet().Name;
        State.PreviousMayor = mayorApi.MayorLastGet();
        foreach (var item in State.itemCategories.Keys)
        {
            GetItemCategory(item);
        }
    }

    public void GetItemCategory(ItemCategory category)
    {
        var items = itemsApi.ItemsCategoryCategoryItemsGet(category);
        if (!State.itemCategories.ContainsKey(category))
            State.itemCategories[category] = new HashSet<string>();

        foreach (var item in items)
        {
            State.itemCategories[category].Add(item);
        }
    }
}
