using System;
using System.Collections.Generic;
using System.Linq;
using Coflnet.Sky.Core;
using MessagePack;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Coflnet.Sky.Commands.Shared;

public class InventoryParserTests
{
    string jsonSample = """
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
        {
            "type": 160,
            "count": 1,
            "metadata": 7,
            "nbt": {
                "type": "compound",
                "name": "",
                "value": {
                    "display": {
                        "type": "compound",
                        "value": {
                            "Lore": {
                                "type": "list",
                                "value": {
                                    "type": "string",
                                    "value": [
                                        "§7This slot may be changed to a",
                                        "§7shortcut for your favorite game",
                                        "§7or mode!",
                                        "",
                                        "§7You may change this slot at any",
                                        "§7time using Right Click."
                                    ]
                                }
                            },
                            "Name": {
                                "type": "string",
                                "value": "§eCustom Slot"
                            }
                        }
                    }
                }
            },
            "name": "stained_glass_pane",
            "displayName": "Stained Glass Pane",
            "stackSize": 64,
            "slot": 16
        },
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
                                "value": "PET"
                            },
                            "gems": {
                                "type": "compound",
                                "value": {
                                    "JADE_0": {
                                        "type": "string",
                                        "value": "FINE"
                                    },
                                    "COMBAT_0": {
                                        "type": "compound",
                                        "value": {
                                            "uuid": {
                                                "type": "string",
                                                "value": "a5c233ba-9554-4c80-a697-1a78c66c045d"
                                            },
                                            "quality": {
                                                "type": "string",
                                                "value": "PERFECT"
                                            }
                                        }
                                    }
                                }
                            },
                            "ability_scroll": {
                                "type": "list",
                                "value": {
                                    "type": "string",
                                    "value": [
                                        "WITHER_SHIELD_SCROLL",
                                        "SHADOW_WARP_SCROLL"
                                    ]
                                }
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
                            "modifier": {
                                "type": "string",
                                "value": "heavy"
                            },
                            "mined_crops": {
                                "type": "long",
                                "value": [
                                    1,
                                    8314091
                                ]
                            },
                            "petInfo": {
                                "type": "string",
                                "value": "{\"type\":\"ELEPHANT\",\"active\":false,\"exp\":3.397827122665796E7,\"tier\":\"LEGENDARY\",\"hideInfo\":false,\"heldItem\":\"PET_ITEM_FARMING_SKILL_BOOST_EPIC\",\"candyUsed\":10,\"uuid\":\"8760755f-f72b-4624-8cf2-c51b21e35acc\",\"hideRightClick\":false}"
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
}
""";
    [Test]
    public void Parse()
    {
        var parser = new InventoryParser();
        var serialized = MessagePackSerializer.Serialize(parser.Parse(jsonSample));
        var item = MessagePackSerializer.Deserialize<List<SaveAuction>>(serialized)
                        .Where(i => i != null).Last();
        Console.WriteLine(JsonConvert.SerializeObject(item, Formatting.Indented));
        Assert.AreEqual("PET_ELEPHANT", item.Tag);
        Assert.AreEqual("┬ºfIron Helmet", item.ItemName);
        Assert.AreEqual(1, item.Enchantments.Count);
        Assert.AreEqual(1, item.Enchantments.Where(e => e.Type == Core.Enchantment.EnchantmentType.growth).First().Level);
        Assert.AreEqual("0cf52647-c130-43ec-9c46-e2dc162d4894", item.FlatenedNBT["uuid"]);
        Assert.AreEqual("PET_ITEM_FARMING_SKILL_BOOST_EPIC", item.FlatenedNBT["heldItem"]);
        Assert.AreEqual("33978271,22665796", item.FlatenedNBT["exp"].Replace('.', ','));
        Assert.AreEqual("FINE", item.FlatenedNBT["JADE_0"]);
        Assert.AreEqual("PERFECT", item.FlatenedNBT["COMBAT_0"]);
        Assert.AreEqual("4303281387", item.FlatenedNBT["mined_crops"]);
        Assert.AreEqual("SHADOW_WARP_SCROLL WITHER_SHIELD_SCROLL", item.FlatenedNBT["ability_scroll"]);
        Assert.AreEqual(ItemReferences.Reforge.Heavy, item.Reforge);
        Assert.AreEqual(Tier.COMMON, item.Tier);
    }
private string jsonSampleCT = """
[{"id":"minecraft:tnt","Count":2,"tag":{"ench":[],"HideFlags":254,"display":{"Lore":["§7Breaks weak walls. Can be used","§7to blow up Crypts in §cThe","§cCatacombs §7and §3Crystal","§3Hollows§7.","","§9§lRARE"],
"Name":"§9Superboom TNT"},"ExtraAttributes":{"id":"SUPERBOOM_TNT"}},"Damage":0},
{"id":"minecraft:stained_glass","Count":1,"tag":{"HideFlags":254,"display":{"Lore":["§7§oA rare space helmet forged","§7§ofrom shards of moon glass.","","§7§8This item can be reforged!","§c§lSPECIAL HELMET"],
"Name":"§cSpace Helmet"},"ExtraAttributes":{"id":"DCTR_SPACE_HELM","uuid":"b14aefbd-cbf8-4ca1-aa2e-5c0422807c60","timestamp":"4/8/23 10:01 AM", enchantments:{impaling:3,chance:4,piercing:1,infinite_quiver:10,ultimate_soul_eater:5,snipe:3,telekinesis:1,power:7}}},"Damage":14}]
""";
    [Test]
    public void ParseCT()
    {
        var parser = new InventoryParser();
        var serialized = MessagePackSerializer.Serialize(parser.Parse(jsonSampleCT));
        var item = MessagePackSerializer.Deserialize<List<SaveAuction>>(serialized)
                        .Where(i => i != null).Last();
        Assert.AreEqual("DCTR_SPACE_HELM", item.Tag);
        Assert.AreEqual("§cSpace Helmet", item.ItemName);
        Assert.AreEqual(ItemReferences.Reforge.None, item.Reforge);
        Assert.AreEqual(8, item.Enchantments.Count);
        Assert.AreEqual(3, item.Enchantments.Where(e => e.Type == Core.Enchantment.EnchantmentType.impaling).First().Level);
        Assert.AreEqual(4, item.Enchantments.Where(e => e.Type == Core.Enchantment.EnchantmentType.chance).First().Level);
        Assert.AreEqual(1, item.Count);
        Assert.AreEqual("b14aefbd-cbf8-4ca1-aa2e-5c0422807c60", item.FlatenedNBT["uuid"]);
        Assert.AreEqual("4/8/23 10:01 AM", item.FlatenedNBT["timestamp"]);
        Assert.AreEqual(Tier.SPECIAL, item.Tier);
    }

}