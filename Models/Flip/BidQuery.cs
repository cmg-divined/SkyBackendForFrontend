
using System;
using System.Collections.Generic;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    internal class BidQuery
    {
        public string Key { get; }
        public long Amount { get; }
        public long HighestOwnBid { get; }
        public DateTime End { get; }
        public string Tag { get; }
        public Tier Tier { get; }
        public List<NBTLookup> Nbt { get; }
        public List<Enchantment> Enchants { get; }

        public BidQuery(string key, long amount, long highestOwnBid, DateTime end, string tag, Tier tier, List<NBTLookup> nbt, List<Enchantment> enchants)
        {
            Key = key;
            Amount = amount;
            HighestOwnBid = highestOwnBid;
            End = end;
            Tag = tag;
            Tier = tier;
            Nbt = nbt;
            Enchants = enchants;
        }

        public override bool Equals(object obj)
        {
            return obj is BidQuery other &&
                   Key == other.Key &&
                   Amount == other.Amount &&
                   HighestOwnBid == other.HighestOwnBid &&
                   End == other.End &&
                   Tag == other.Tag &&
                   Tier == other.Tier &&
                   EqualityComparer<List<NBTLookup>>.Default.Equals(Nbt, other.Nbt) &&
                   EqualityComparer<List<Enchantment>>.Default.Equals(Enchants, other.Enchants);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Amount, HighestOwnBid, End, Tag, Tier, Nbt, Enchants);
        }
    }
}
