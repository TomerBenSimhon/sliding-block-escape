using System;
using NUnit.Framework;

namespace SlidingBlock.Core.Tests
{
    public class BlockTests
    {
        [Test]
        public void Constructor_ValidArguments_StoresAllProperties()
        {
            // Non-default values on purpose (Vertical, true): a constructor that forgot to assign
            // Axis or IsTarget would leave them at Horizontal/false and still pass with defaults.
            var position = new GridPos(1, 2);

            var block = new Block(7, position, 3, Axis.Vertical, true);

            Assert.AreEqual(7, block.Id);
            Assert.AreEqual(position, block.Position);
            Assert.AreEqual(3, block.Length);
            Assert.AreEqual(Axis.Vertical, block.Axis);
            Assert.IsTrue(block.IsTarget);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_LengthLessThanOne_Throws(int length)
        {
            var position = new GridPos(1, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => new Block(1, position, length, Axis.Horizontal, false));
        }

        [Test]
        public void Direction_Horizontal_IsRight()
        {
            var block = new Block(0, new GridPos(0, 0), 2, Axis.Horizontal, false);

            Assert.AreEqual(GridPos.Right, block.Direction);
        }

        [Test]
        public void Direction_Vertical_IsUp()
        {
            var block = new Block(0, new GridPos(0, 0), 2, Axis.Vertical, false);

            Assert.AreEqual(GridPos.Up, block.Direction);
        }

        [Test]
        public void CellAt_Horizontal_ReturnsCellsRightOfAnchor()
        {
            var block = new Block(1, new GridPos(1, 2), 3, Axis.Horizontal, false);

            var first = block.CellAt(0);
            var middle = block.CellAt(1);
            var last = block.CellAt(2);

            Assert.AreEqual(new GridPos(1, 2), first);
            Assert.AreEqual(new GridPos(2, 2), middle);
            Assert.AreEqual(new GridPos(3, 2), last);
        }

        [Test]
        public void CellAt_Vertical_ReturnsCellsAboveAnchor()
        {
            var block = new Block(1, new GridPos(4, 0), 2, Axis.Vertical, false);

            var first = block.CellAt(0);
            var last = block.CellAt(1);

            Assert.AreEqual(new GridPos(4, 0), first);
            Assert.AreEqual(new GridPos(4, 1), last);
        }

        [TestCase(-1)]
        [TestCase(3)] // == Length: the classic off-by-one
        public void CellAt_IndexOutOfRange_Throws(int index)
        {
            var block = new Block(1, new GridPos(1, 2), 3, Axis.Horizontal, false);

            Assert.Throws<ArgumentOutOfRangeException>(() => block.CellAt(index));
        }

        [Test]
        public void WithPosition_ReturnsBlockWithNewPositionAndSameProperties()
        {
            var block = new Block(7, new GridPos(1, 2), 3, Axis.Vertical, true);
            var newPosition = new GridPos(5, 4);

            var moved = block.WithPosition(newPosition);

            Assert.AreEqual(newPosition, moved.Position);
            Assert.AreEqual(7, moved.Id);
            Assert.AreEqual(3, moved.Length);
            Assert.AreEqual(Axis.Vertical, moved.Axis);
            Assert.IsTrue(moved.IsTarget);
        }

        [Test]
        public void WithPosition_DoesNotChangeOriginal()
        {
            var originalPosition = new GridPos(1, 2);
            var block = new Block(7, originalPosition, 3, Axis.Vertical, true);

            block.WithPosition(new GridPos(5, 4));

            Assert.AreEqual(originalPosition, block.Position);
        }
    }
}
