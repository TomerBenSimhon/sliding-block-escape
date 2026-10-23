using NUnit.Framework;

namespace SlidingBlock.Core.Tests
{
    public class GridPosTests
    {
        // Equality comes first: Assert.AreEqual in every other test relies on Equals being correct.

        [Test]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            var a = new GridPos(1, 2);
            var b = new GridPos(1, 2);

            bool result = a.Equals(b);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentCoordinates_ReturnsFalse()
        {
            // X == Y on purpose: a buggy Equals that compares X to its own Y would wrongly return true here.
            var a = new GridPos(3, 3);
            var b = new GridPos(0, 7);

            bool result = a.Equals(b);

            Assert.IsFalse(result);
        }

        [Test]
        public void EqualityOperator_SameCoordinates_ReturnsTrue()
        {
            var a = new GridPos(1, 2);
            var b = new GridPos(1, 2);

            bool result = a == b;

            Assert.IsTrue(result);
        }

        [Test]
        public void InequalityOperator_DifferentCoordinates_ReturnsTrue()
        {
            var a = new GridPos(3, 3);
            var b = new GridPos(0, 7);

            bool result = a != b;

            Assert.IsTrue(result);
        }

        [Test]
        public void GetHashCode_EqualPositions_ReturnSameHash()
        {
            var a = new GridPos(3, 4);
            var b = new GridPos(3, 4);

            int hashA = a.GetHashCode();
            int hashB = b.GetHashCode();

            Assert.AreEqual(hashA, hashB);
        }

        [Test]
        public void Add_TwoPositions_AddsComponents()
        {
            var a = new GridPos(1, 2);
            var b = new GridPos(3, 4);

            var result = a + b;

            Assert.AreEqual(new GridPos(4, 6), result);
        }

        [Test]
        public void Subtract_TwoPositions_SubtractsComponents()
        {
            var a = new GridPos(5, 7);
            var b = new GridPos(2, 3);

            var result = a - b;

            Assert.AreEqual(new GridPos(3, 4), result);
        }

        [Test]
        public void Multiply_ByScalar_ScalesComponentsInBothOrders()
        {
            var pos = new GridPos(2, 3);

            var scalarOnRight = pos * 3;
            var scalarOnLeft = 3 * pos;

            Assert.AreEqual(new GridPos(6, 9), scalarOnRight);
            Assert.AreEqual(new GridPos(6, 9), scalarOnLeft);
        }

        [Test]
        public void ToString_ReturnsCoordinatesInParentheses()
        {
            var pos = new GridPos(2, 3);

            string result = pos.ToString();

            Assert.AreEqual("(2, 3)", result);
        }
    }
}
