using System;
using System.Collections.Generic;
using MessagePack;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared;

#nullable enable
[MessagePackObject]
public class UpdateMessage
{
    [Key(0)]
    public UpdateKind Kind;

    [Key(1)]
    public DateTime ReceivedAt;
    [Key(2)]
    public ChestView? Chest;
    [Key(3)]
    public List<string>? ChatBatch;
    [Key(4)]
    public string? PlayerId;
    [Key(5)]
    public string? SessionId { get; set; }

    public enum UpdateKind 
    {
        UNKOWN,
        CHAT,
        INVENTORY,
        API = 4,

    }
}

[MessagePackObject]
public class ChestView
{
    /// <summary>
    /// All items in the ui view
    /// </summary>
    [Key(0)]
    public List<Item> Items = new ();
    [Key(1)]
    public string Name;
}


#nullable restore