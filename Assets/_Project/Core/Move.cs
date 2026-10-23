using System;

namespace SlidingBlock.Core
{
    public readonly struct Move : IEquatable<Move>
    {
        public int BlockId { get; }
        public int Distance { get; }
        
        public Move(int blockId, int distance)
        {            
            if(distance == 0) throw new ArgumentOutOfRangeException(nameof(distance), distance, "Distance must not be zero");
            BlockId = blockId;
            Distance = distance;
        }

        public Move Inverse => new Move(BlockId, -Distance);
        
        public bool Equals(Move other)
        {
            return BlockId == other.BlockId && Distance == other.Distance;
        }
        public override bool Equals(object obj)
        {
            return obj is Move move && Equals(move);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BlockId, Distance);
        }

        public static bool operator ==(Move a, Move b) => a.Equals(b);
        public static bool operator !=(Move a, Move b) => !a.Equals(b);

        public override string ToString()
        {
            return $"Block {BlockId} {Distance:+0;-0}";
        }
    }
}
