using System;

namespace SlidingBlock.Core
{
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public int X { get; }
        public int Y { get; }
        public static readonly GridPos Right = new GridPos(1, 0);
        public static readonly GridPos Up    = new GridPos(0, 1);

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridPos other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPos pos && Equals(pos);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !(a == b);
        public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.X + b.X, a.Y + b.Y);
        public static GridPos operator -(GridPos a, GridPos b) => new GridPos(a.X - b.X, a.Y - b.Y);
        public static GridPos operator *(GridPos a, int scalar) => new GridPos(a.X * scalar, a.Y * scalar);
        public static GridPos operator *(int scalar, GridPos a) => a * scalar;
        public override string ToString() => $"({X}, {Y})";
    }
}
