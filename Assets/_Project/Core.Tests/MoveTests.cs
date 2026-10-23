using System;
using NUnit.Framework;

namespace SlidingBlock.Core.Tests
{
    public class MoveTests
    {
        [Test]
        public void Constructor_ValidArguments_StoresAllProperties()
        {
            var move = new Move(5, 3);

            Assert.AreEqual(5, move.BlockId);
            Assert.AreEqual(3, move.Distance);
        }

        [Test]
        public void Constructor_ZeroDistance_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Move(2, 0));
        }

        [Test]
        public void Constructor_NegativeDistance_IsAllowed()
        {
            // Negative = left/down. Guards against tightening the check to distance <= 0.
            Assert.DoesNotThrow(() => new Move(5, -2));
        }

        [Test]
        public void Inverse_NegatesDistanceAndKeepsBlockId()
        {
            var move = new Move(5, 3);

            var inverse = move.Inverse;

            Assert.AreEqual(new Move(5, -3), inverse);
        }

        [Test]
        public void Inverse_OfInverse_ReturnsOriginalMove()
        {
            var move = new Move(5, 3);

            var roundTrip = move.Inverse.Inverse;

            Assert.AreEqual(move, roundTrip);
        }

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new Move(5, 3);
            var b = new Move(5, 3);

            bool result = a.Equals(b);

            Assert.IsTrue(result);
        }

        [Test]
        public void EqualityOperator_SameValues_ReturnsTrue()
        {
            var a = new Move(5, 3);
            var b = new Move(5, 3);

            bool result = a == b;

            Assert.IsTrue(result);
        }

        // The two "different" tests change one field each, so a bug that compares
        // only BlockId or only Distance fails one of them.

        [Test]
        public void Equals_SameBlockDifferentDistance_ReturnsFalse()
        {
            var a = new Move(5, 3);
            var b = new Move(5, -3);

            bool result = a.Equals(b);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_DifferentBlockSameDistance_ReturnsFalse()
        {
            var a = new Move(5, 3);
            var b = new Move(2, 3);

            bool result = a.Equals(b);

            Assert.IsFalse(result);
        }

        [Test]
        public void InequalityOperator_DifferentValues_ReturnsTrue()
        {
            var a = new Move(5, 3);
            var b = new Move(5, -3);

            bool result = a != b;

            Assert.IsTrue(result);
        }

        [Test]
        public void GetHashCode_EqualMoves_ReturnSameHash()
        {
            var a = new Move(5, 3);
            var b = new Move(5, 3);

            int hashA = a.GetHashCode();
            int hashB = b.GetHashCode();

            Assert.AreEqual(hashA, hashB);
        }

        [Test]
        public void ToString_PositiveDistance_IncludesPlusSign()
        {
            var move = new Move(5, 3);

            string result = move.ToString();

            Assert.AreEqual("Block 5 +3", result);
        }

        [Test]
        public void ToString_NegativeDistance_IncludesMinusSign()
        {
            var move = new Move(5, -3);

            string result = move.ToString();

            Assert.AreEqual("Block 5 -3", result);
        }
    }
}
