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
        {"type":315,"count":1,"metadata":0,"nbt":{"type":"compound","name":"","value":{"ench":{"type":"list","value":{"type":"end","value":[]}},"Unbreakable":{"type":"byte","value":1},"HideFlags":{"type":"int","value":254},
            "display":{"type":"compound","value":{"Lore":{"type":"list","value":{"type":"string","value":["§7Health: §a+200","§7Defense: §a+150","§7Mining Speed: §a+230 §9(+60) §d(+90)"]}},"Name":{"type":"string","value":"§f§f§dJaded Chestplate of Divan"}}},
            "ExtraAttributes":{"type":"compound","value":{"rarity_upgrades":{"type":"int","value":1},"gems":{"type":"compound","value":{"JADE_1":{"type":"string","value":"FINE"},"JADE_0":{"type":"string","value":"FINE"},
                "unlocked_slots":{"type":"list","value":{"type":"string","value":["JADE_0","JADE_1","TOPAZ_0","AMBER_0","AMBER_1"]}},"AMBER_0":{"type":"string","value":"FINE"},"AMBER_1":{"type":"string","value":"FINE"},"TOPAZ_0":{"type":"string","value":"FINE"}}},"modifier":{"type":"string","value":"jaded"},"id":{"type":"string","value":"DIVAN_CHESTPLATE"},
                "enchantments":{"type":"compound","value":{"ultimate_wisdom":{"type":"int","value":1},"growth":{"type":"int","value":5},"protection":{"type":"int","value":5}}},"uuid":{"type":"string","value":"ea533251-6328-4a3c-8477-649cfb93ff45"},"timestamp":{"type":"string","value":"7/24/23 2:42 AM"}}}}},
                "stackId":null,"name":"golden_chestplate","displayName":"Golden Chestplate","stackSize":1,"maxDurability":112,"slot":32},
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
                            "necromancer_souls": {
                                "type": "list",
                                "value": {
                                    "type": "compound",
                                    "value": [
                                        {
                                            "mob_id": {
                                                "type": "string",
                                                "value": "MASTER_CRYPT_TANK_ZOMBIE_70"
                                            }
                                        },
                                        {
                                            "mob_id": {
                                                "type": "string",
                                                "value": "MASTER_CRYPT_TANK_ZOMBIE_70"
                                            }
                                        },
                                        {
                                            "mob_id": {
                                                "type": "string",
                                                "value": "MASTER_CRYPT_TANK_ZOMBIE_70"
                                            }
                                        },
                                        {
                                            "mob_id": {
                                                "type": "string",
                                                "value": "MASTER_CRYPT_TANK_ZOMBIE_70"
                                            }
                                        },
                                        {
                                            "mob_id": {
                                                "type": "string",
                                                "value": "MASTER_CRYPT_TANK_ZOMBIE_70"
                                            }
                                        },
                                        {
                                            "mob_id": {
                                                "type": "string",
                                                "value": "MASTER_CRYPT_TANK_ZOMBIE_70"
                                            }
                                        }
                                    ]
                                }
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
        var deserialized = MessagePackSerializer.Deserialize<List<SaveAuction>>(serialized);
        var item = deserialized
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
        Assert.AreEqual("6", item.FlatenedNBT["MASTER_CRYPT_TANK_ZOMBIE_70"]);
        Assert.AreEqual(ItemReferences.Reforge.Heavy, item.Reforge);
        Assert.AreEqual(Tier.COMMON, item.Tier);

        var divan = deserialized.Where(i => i != null).Skip(1).First();
        Assert.AreEqual(new DateTime(2023, 7, 24), divan.ItemCreatedAt.Date);
        Assert.AreEqual("AMBER_0,AMBER_1,JADE_0,JADE_1,TOPAZ_0", divan.FlatenedNBT["unlocked_slots"]);
    }

    string petSample = """
        {
    "_events": {},
    "_eventsCount": 0,
    "id": 0,
    "type": "minecraft:inventory",
    "title": "Inventory",
    "slots": [
{
  "type": 397,
  "count": 1,
  "metadata": 3,
  "nbt": {
    "type": "compound",
    "name": "",
    "value": {
      "HideFlags": {
        "type": "int",
        "value": 254
      },
      "SkullOwner": {
        "type": "compound",
        "value": {
          "Id": {
            "type": "string",
            "value": "3a80504c-0993-3be5-92cd-2200f94a72b6"
          },
          "Properties": {
            "type": "compound",
            "value": {
              "textures": {
                "type": "list",
                "value": {
                  "type": "compound",
                  "value": [
                    {
                      "Value": {
                        "type": "string",
                        "value": "ewogICJ0aW1lc3RhbXAiIDogMTYwNjQyODA5NTg3NywKICAicHJvZmlsZUlkIiA6ICIzZmM3ZmRmOTM5NjM0YzQxOTExOTliYTNmN2NjM2ZlZCIsCiAgInByb2ZpbGVOYW1lIiA6ICJZZWxlaGEiLAogICJzaWduYXR1cmVSZXF1aXJlZCIgOiB0cnVlLAogICJ0ZXh0dXJlcyIgOiB7CiAgICAiU0tJTiIgOiB7CiAgICAgICJ1cmwiIDogImh0dHA6Ly90ZXh0dXJlcy5taW5lY3JhZnQubmV0L3RleHR1cmUvZjVmMjlhOTc1NTI5Mjc2ZDkxNmZjNjc5OTg4MzNjMTFlZTE3OGZmMjFlNTk0MWFmZGZiMGZhNzAxMGY4Mzc0ZSIKICAgIH0KICB9Cn0"
                      }
                    }
                  ]
                }
              }
            }
          }
        }
      },
      "display": {
        "type": "compound",
        "value": {
          "Lore": {
            "type": "list",
            "value": {
              "type": "string",
              "value": [
                "§8Fishing Pet, Grown-up Skin",
                "",
                "§b§lMAX LEVEL",
                "",
                "§7§eRight-click to add this pet to",
                "§eyour pet menu!",
                "",
                "§6§lLEGENDARY",
                "§8§m-----------------",
                "§7Seller: §b[MVP§e+§b] Parakkz",
                "§7Buy it now: §6190,000,000 coins",
                "",
                "§7Ends in: §e23h",
                "",
                "§eClick to inspect!"
              ]
            }
          },
          "Name": {
            "type": "string",
            "value": "§f§f§7[Lvl 100] §6Baby Yeti ✦"
          }
        }
      },
      "ExtraAttributes": {
        "type": "compound",
        "value": {
          "petInfo": {
            "type": "string",
            "value": "{\"type\":\"BABY_YETI\",\"active\":false,\"exp\":3.892246997045735E7,\"tier\":\"LEGENDARY\",\"hideInfo\":false,\"heldItem\":\"DWARF_TURTLE_SHELMET\",\"candyUsed\":10,\"skin\":\"YETI_GROWN_UP\",\"uuid\":\"28fdb3cb-1029-48cf-9591-3e7d8d67ba9a\",\"hideRightClick\":false}"
          },
          "id": {
            "type": "string",
            "value": "PET"
          },
          "uuid": {
            "type": "string",
            "value": "28fdb3cb-1029-48cf-9591-3e7d8d67ba9a"
          },
          "timestamp": {
            "type": "string",
            "value": "8/2/23 2:31 PM"
          }
        }
      }
    }
  },
  "stackId": null,
  "name": "skull",
  "displayName": "Head",
  "stackSize": 64,
  "slot": 11
}
]}
""";

    [Test]
    public void ParsePetRarity()
    {
        var parser = new InventoryParser();
        var serialized = MessagePackSerializer.Serialize(parser.Parse(petSample));
        var item = MessagePackSerializer.Deserialize<List<SaveAuction>>(serialized).First();
        Assert.AreEqual(Tier.LEGENDARY, item.Tier);
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

    [Test]
    public void Parse117Strings()
    {
        var parser = new InventoryParser();
        var data = parser.Parse("""
        {
        "_events": {},
        "_eventsCount": 0,
        "id": 0,
        "type": "minecraft:inventory",
        "title": "Inventory",
        "slots": [
            {
            "type": 746,
            "count": 1,
            "metadata": 0,
            "nbt": {
                "type": "compound",
                "name": "",
                "value": {
                    "Unbreakable": {
                        "type": "byte",
                        "value": 1
                    },
                    "HideFlags": {
                        "type": "int",
                        "value": 255
                    },
                    "display": {
                        "type": "compound",
                        "value": {
                            "Lore": {
                                "type": "list",
                                "value": {
                                    "type": "string",
                                    "value": [
                                        "{\"italic\":false,\"extra\":[{\"color\":\"gray\",\"text\":\"Defense: \"},{\"text\":\" \"},{\"color\":\"green\",\"text\":\"+10\"}],\"text\":\"\"}",
                                        "{\"italic\":false,\"text\":\"\"}",
                                        "{\"italic\":false,\"extra\":[{\"color\":\"gray\",\"text\":\"Growth I\"}],\"text\":\"\"}",
                                        "{\"italic\":false,\"extra\":[{\"color\":\"gray\",\"text\":\"Grants \"},{\"color\":\"green\",\"text\":\"+15 \"},{\"color\":\"red\",\"text\":\"❤ Health\"},{\"color\":\"gray\",\"text\":\".\"}],\"text\":\"\"}",
                                        "{\"italic\":false,\"text\":\"\"}",
                                        "{\"italic\":false,\"extra\":[{\"color\":\"gray\",\"text\":\"\"},{\"color\":\"red\",\"text\":\"You do not have a high enough\"}],\"text\":\"\"}",
                                        "{\"italic\":false,\"extra\":[{\"color\":\"red\",\"text\":\"Enchanting level to use some of\"}],\"text\":\"\"}",
                                        "{\"italic\":false,\"extra\":[{\"color\":\"red\",\"text\":\"the enchantments on this item!\"}],\"text\":\"\"}",
                                        "{\"italic\":false,\"text\":\"\"}",
                                        "{\"italic\":false,\"extra\":[{\"color\":\"gray\",\"text\":\"\"},{\"color\":\"dark_gray\",\"text\":\"This item can be reforged!\"}],\"text\":\"\"}",
                                        "{\"italic\":false,\"extra\":[{\"bold\":true,\"color\":\"white\",\"text\":\"COMMON HELMET\"}],\"text\":\"\"}"
                                    ]
                                }
                            },
                            "Name": {
                                "type": "string",
                                "value": "{\"italic\":false,\"extra\":[{\"color\":\"white\",\"text\":\"Iron Helmet\"}],\"text\":\"\"}"
                            }
                        }
                    },
                    "Enchantments": {
                        "type": "list",
                        "value": {
                            "type": "compound",
                            "value": [
                                {
                                    "lvl": {
                                        "type": "short",
                                        "value": 0
                                    },
                                    "id": {
                                        "type": "string",
                                        "value": "minecraft:protection"
                                    }
                                }
                            ]
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
            },
        ]}
        """);

        Assert.AreEqual("§fIron Helmet", data.First().ItemName);
        Assert.IsTrue(data.First().Context["lore"].StartsWith("§7Defense:  §a+10\n\n§7Growth I\n§7Grants §a+15 §c❤ Health"));
        Assert.AreEqual(new DateTime(2023, 2, 18), data.First().ItemCreatedAt.Date);
    }
}