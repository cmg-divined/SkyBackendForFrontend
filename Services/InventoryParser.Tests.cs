using System.Collections.Generic;
using System.Linq;
using Coflnet.Sky.Core;
using MessagePack;
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
        var item = MessagePackSerializer.Deserialize<List<SaveAuction>>(MessagePackSerializer.Serialize(parser.Parse(jsonSample)));
        Assert.AreEqual("IRON_HELMET", item.First().Tag);
        Assert.AreEqual("Iron Helmet", item.First().ItemName);
        Assert.AreEqual(1, item.First().Enchantments.Count);
        Assert.AreEqual(1, item.First().Enchantments.Where(e => e.Type == Core.Enchantment.EnchantmentType.growth).First().Level);
        Assert.AreEqual("0cf52647-c130-43ec-9c46-e2dc162d4894", item.First().FlatenedNBT["uuid"]);
        Assert.AreEqual("PET_ITEM_FARMING_SKILL_BOOST_EPIC", item.First().FlatenedNBT["heldItem"]);
        Assert.AreEqual("FINE", item.First().FlatenedNBT["JADE_0"]);
        Assert.AreEqual("PERFECT", item.First().FlatenedNBT["COMBAT_0"]);
        Assert.AreEqual("4303281387", item.First().FlatenedNBT["mined_crops"]);
        Assert.AreEqual("SHADOW_WARP_SCROLL WITHER_SHIELD_SCROLL", item.First().FlatenedNBT["ability_scroll"]);
        Assert.AreEqual(ItemReferences.Reforge.Heavy, item.First().Reforge);

    }
}