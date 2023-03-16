using System;
using System.Collections.Generic;
using System.Linq;
using Coflnet.Sky.Core;
using Newtonsoft.Json;

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
    public IEnumerable<SaveAuction> Parse(string json)
    {
        dynamic full = JsonConvert.DeserializeObject(json);
        foreach (var item in full.slots)
        {
            if (item == null)
                continue;

            var ExtraAttributes = item.nbt.value.ExtraAttributes.value;
            System.Console.WriteLine(ExtraAttributes.id.value);
            var attributesWithoutEnchantments = new Dictionary<string, object>();
            Denest(ExtraAttributes, attributesWithoutEnchantments);
            var enchantments = new Dictionary<string, int>();
            foreach (var enchantment in ExtraAttributes.enchantments.value)
            {
                Console.WriteLine(enchantment.Value.value.GetType());
                var val = new Newtonsoft.Json.Linq.JValue(2);

                enchantments.Add(enchantment.Name, (int)enchantment.Value.value);
            }
            ;
            var auction = new SaveAuction
            {
                Tag = ExtraAttributes.id.value,
                Enchantments = enchantments.Select(e => new Enchantment() { Type = Enum.Parse<Enchantment.EnchantmentType>(e.Key), Level = (byte)e.Value }).ToList(),
                Count = item.count,
                ItemName = item.displayName
            };
            if (attributesWithoutEnchantments.ContainsKey("modifier"))
            {
                auction.Reforge = Enum.Parse<ItemReferences.Reforge>(attributesWithoutEnchantments["modifier"].ToString(), true);
                attributesWithoutEnchantments.Remove("modifier");
            }
            auction.SetFlattenedNbt(NBT.FlattenNbtData(attributesWithoutEnchantments).GroupBy(e => e.Key).Select(e => e.First()).ToList());
            yield return auction;
        }
    }

    private static void Denest(dynamic ExtraAttributes, Dictionary<string, object> attributesWithoutEnchantments)
    {
        foreach (var attribute in ExtraAttributes)
        {
            if (attribute.Name != "enchantments")
            {
                var p = new Newtonsoft.Json.Linq.JProperty("a");
                // p.Name
                if (attribute.Value.type == "compound")
                    Denest(attribute.Value.value, attributesWithoutEnchantments);
                else
                    attributesWithoutEnchantments[attribute.Name] = attribute.Value.value;
            }
        }
    }
}
