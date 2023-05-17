using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Coflnet.Sky.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coflnet.Sky.Commands.Shared;

public class InventoryParser
{
    /* json sample
    {
    "_events": {},
    "_eventsCount": 0,
    "id": 0,
    "type": "minecraft:inventory",
    "title": "Inventory",
    "slots": [
        null,
        null,
        null,
        null,
        null,
        {
            "type": 306,
            "count": 1,
            "metadata": 0,
            "nbt": {
                "type": "compound",
                "name": "",
                "value": {
                    "ench": {
                        "type": "list",
                        "value": {
                            "type": "end",
                            "value": []
                        }
                    },
                    "Unbreakable": {
                        "type": "byte",
                        "value": 1
                    },
                    "HideFlags": {
                        "type": "int",
                        "value": 254
                    },
                    "display": {
                        "type": "compound",
                        "value": {
                            "Lore": {
                                "type": "list",
                                "value": {
                                    "type": "string",
                                    "value": [
                                        "┬º7Defense: ┬ºa+10",
                                        "",
                                        "┬º7Growth I",
                                        "┬º7Grants ┬ºa+15 ┬ºcÔØñ Health┬º7.",
                                        "",
                                        "┬º7┬ºcYou do not have a high enough",
                                        "┬ºcEnchanting level to use some of",
                                        "┬ºcthe enchantments on this item!",
                                        "",
                                        "┬º7┬º8This item can be reforged!",
                                        "┬ºf┬ºlCOMMON HELMET"
                                    ]
                                }
                            },
                            "Name": {
                                "type": "string",
                                "value": "┬ºfIron Helmet"
                            }
                        }
                    },
                    "ExtraAttributes": {
                        "type": "compound",
                        "value": {
                            "id": {
                                "type": "string",
                                "value": "IRON_HELMET"
                            },
                            "enchantments": {
                                "type": "compound",
                                "value": {
                                    "growth": {
                                        "type": "int",
                                        "value": 1
                                    }
                                }
                            },
                            "mined_crops": {
                                "type": "long",
                                "value": [
                                    1,
                                    8314091
                                ]
                            },
                            "uuid": {
                                "type": "string",
                                "value": "0cf52647-c130-43ec-9c46-e2dc162d4894"
                            },
                            "timestamp": {
                                "type": "string",
                                "value": "2/18/23 4:27 AM"
                            }
                        }
                    }
                }
            },
            "name": "iron_helmet",
            "displayName": "Iron Helmet",
            "stackSize": 1,
            "slot": 5
        }
	  ]
}*/
    public IEnumerable<SaveAuction> Parse(dynamic data)
    {
        dynamic full = null;
        if (data is string json)
            full = JsonConvert.DeserializeObject(json);
        else
            full = data;
        if (full is JArray array)
        {
            foreach (var item in ParseChatTriggers(array))
                yield return item;
            yield break;
        }
        if (full.slots == null)
        {
            Activity.Current?.AddEvent(new ActivityEvent("Log", default, new(new Dictionary<string, object>() {
                { "message", "The inventory json at key `jsonNbt` is missing the slots property. Make sure you serialized bot.inventory" },
                { "json", JsonConvert.SerializeObject(data) } })));
            throw new CoflnetException("missing_slots", "The inventory json at key `jsonNbt` is missing the slots property. Make sure you serialized bot.inventory");
        }
        foreach (var item in full.slots)
        {
            if (item == null)
            {
                yield return null;
                continue;
            }

            var ExtraAttributes = item.nbt.value?.ExtraAttributes?.value ?? item.ExtraAttributes;
            if (ExtraAttributes == null)
            {
                yield return new SaveAuction()
                {
                    Tag = "UNKOWN",
                    Enchantments = new(),
                    Count = 1,
                    ItemName = item.displayName,
                    Uuid = ExtraAttributes?.uuid?.value ?? Random.Shared.Next().ToString(),
                };
                continue;
            }
            Dictionary<string, object> attributesWithoutEnchantments = null;
            SaveAuction auction = null;
            try
            {
                NewMethod(item, ExtraAttributes, out attributesWithoutEnchantments, out auction);
                auction?.SetFlattenedNbt(NBT.FlattenNbtData(attributesWithoutEnchantments).GroupBy(e => e.Key).Select(e => e.First()).ToList());
                if (auction.Tag == "PET")
                {
                    auction.Tag += "_" + auction.FlatenedNBT.FirstOrDefault(e => e.Key == "type").Value.ToString();
                }
            }
            catch (System.Exception e)
            {
                Activity.Current?.AddEvent(new ActivityEvent("Log", default, new(new Dictionary<string, object>() { {
                    "message", "Error while parsing inventory" }, { "error", e }, {"item", JsonConvert.SerializeObject(item)} })));
                dev.Logger.Instance.Error(e, "Error while parsing inventory");
            }
            yield return auction;
        }
    }

    private void NewMethod(dynamic item, dynamic ExtraAttributes, out Dictionary<string, object> attributesWithoutEnchantments, out SaveAuction auction)
    {
        attributesWithoutEnchantments = new Dictionary<string, object>();
        Denest(ExtraAttributes, attributesWithoutEnchantments);
        var enchantments = new Dictionary<string, int>();
        if (ExtraAttributes.enchantments?.value != null)
            foreach (var enchantment in ExtraAttributes.enchantments.value)
            {
                enchantments.Add(enchantment.Name, (int)enchantment.Value.value);
            }

        auction = new SaveAuction
        {
            Tag = ExtraAttributes.id.value,
            Enchantments = enchantments.Select(e => new Enchantment() { Type = Enum.Parse<Enchantment.EnchantmentType>(e.Key), Level = (byte)e.Value }).ToList(),
            Count = item.count,
            ItemName = item.nbt.value?.display?.value?.Name?.value ?? item.displayName,
            Uuid = ExtraAttributes?.uuid?.value ?? Random.Shared.Next().ToString(),
        };
        var description = item.nbt.value?.display?.value?.Lore?.value?.value?.ToObject<string[]>() as string[];
        NBT.GetAndAssignTier(auction, description.LastOrDefault()?.ToString());
        if (attributesWithoutEnchantments.ContainsKey("modifier"))
        {
            auction.Reforge = Enum.Parse<ItemReferences.Reforge>(attributesWithoutEnchantments["modifier"].ToString(), true);
            attributesWithoutEnchantments.Remove("modifier");
        }
    }


    private IEnumerable<SaveAuction> ParseChatTriggers(JArray full)
    {
        foreach (var item in full)
        {
            var extraAttributes = item["tag"]["ExtraAttributes"];
            if (extraAttributes == null)
            {
                yield return new SaveAuction()
                {
                    Count = (int)item["Count"],
                    ItemName = item["display"]["Name"].ToString(),
                    Enchantments = new(),
                };
                continue;
            }
            var enchants = extraAttributes["enchantments"]?.ToObject<Dictionary<string, int>>()?
                    .Select(e => new Enchantment() { Type = Enum.Parse<Enchantment.EnchantmentType>(e.Key), Level = (byte)e.Value })
                    .ToList();

            var flatNbt = NBT.FlattenNbtData(extraAttributes.ToObject<Dictionary<string, object>>()
                        .Where(e => e.Key != "enchantments").ToDictionary(e => e.Key, e => e.Value));
            var auction = new SaveAuction()
            {
                Count = (int)item["Count"],
                ItemName = item["tag"]["display"]["Name"].ToString(),
                Enchantments = enchants,
                Tag = extraAttributes["id"].ToString(),
                Uuid = extraAttributes["uuid"]?.ToString() ?? Random.Shared.Next().ToString(),
            };
            NBT.GetAndAssignTier(auction, item["tag"]["display"]["Lore"]?.LastOrDefault()?.ToString());
            auction.SetFlattenedNbt(flatNbt);

            yield return auction;
        }
    }


    private static void Denest(dynamic ExtraAttributes, Dictionary<string, object> attributesWithoutEnchantments)
    {
        foreach (JProperty attribute in ExtraAttributes)
        {
            if (attribute.Name == "enchantments")
                continue;

            // p.Name
            var type = attribute.Value["type"].ToString();
            if (type == "list")
            {
                var list = new List<object>();
                foreach (var item in attribute.Value["value"]["value"])
                {
                    list.Add(item.ToString());
                }
                attributesWithoutEnchantments[attribute.Name] = list;
            }
            else if ((attribute.Name.EndsWith("_0") || attribute.Name.EndsWith("_1") || attribute.Name.EndsWith("_2") || attribute.Name.EndsWith("_3") || attribute.Name.EndsWith("_4"))
                        && type == "compound")
            {
                // has uuid
                var values = attribute.Value["value"];
                attributesWithoutEnchantments[attribute.Name] = values["quality"]["value"].ToString();
                attributesWithoutEnchantments[attribute.Name + ".uuid"] = values["uuid"].ToString();
            }
            else if (type == "compound")
                Denest(attribute.Value["value"], attributesWithoutEnchantments);
            else if (type == "long")
                attributesWithoutEnchantments[attribute.Name] = ((long)attribute.Value["value"][0] << 32) + (int)attribute.Value["value"][1];
            else
                attributesWithoutEnchantments[attribute.Name] = attribute.Value["value"];
        }
    }
}
