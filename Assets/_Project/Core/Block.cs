using System;

namespace SlidingBlock.Core
{
    public readonly struct Block
    {
        public int Id { get; }
        public GridPos Position { get; }
        public int Length { get; }
        public Axis Axis { get; }
        public bool IsTarget { get; }

        public Block(int id, GridPos position, int length, Axis axis, bool isTarget)
        {
            if(length < 1) throw new ArgumentOutOfRangeException(nameof(length), length, "Block length must be at least 1");
            Id = id;
            Position = position;
            Length = length;
            Axis = axis;
            IsTarget = isTarget;
        }
        public GridPos Direction => Axis switch
        {
            Axis.Horizontal => GridPos.Right,
            Axis.Vertical => GridPos.Up,
            _ => throw new ArgumentOutOfRangeException(nameof(Axis), Axis, "Invalid axis value")
        };
        public GridPos CellAt(int index)
        {
            if(index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range");
            return Position + Direction * index;
        }
        public Block WithPosition(GridPos newPosition)
        {
            return new Block(Id, newPosition, Length, Axis, IsTarget);
        }
        public override string ToString()
        {
            return $"Block(Id: {Id}, Position: {Position}, Length: {Length}, Axis: {Axis}, IsTarget: {IsTarget})";
        }
    }
}
