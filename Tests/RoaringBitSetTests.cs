using System.IO;
using Xunit;

namespace BitsetsNET.Tests
{
    public class RoaringBitSetTests : BaseBitSetTests
    {
        protected override IBitset CreateSetFromIndices(int[] indices, int length)
        {
            return RoaringBitset.Create(indices);
        }

        [Fact]
        public virtual void SerializationTest()
        {
            int TEST_SET_LENGTH = 10;
            int[] indicies = SetGenerator.GetRandomArray(TEST_SET_LENGTH);

            RoaringBitset actual = (RoaringBitset)CreateSetFromIndices(indicies, TEST_SET_LENGTH);
            RoaringBitset expected;

            using (MemoryStream ms = new MemoryStream())
            {
                actual.Serialize(ms);
                ms.Position = 0;
                expected = RoaringBitset.Deserialize(ms);
            }

            Assert.Equal(actual, expected);
        }

        [Fact]
        public virtual void SetTrueLarge()
        {
            int[] set = SetGenerator.GetContiguousArray(9, 5009);
            IBitset testSet = CreateSetFromIndices(set, 5000);
            testSet.Set(8, true);
            bool expected = true;
            bool result = testSet.Get(8);
            Assert.Equal(expected, result);
        }

        [Fact]
        public virtual void SetFalseLarge()
        {
            int[] set = SetGenerator.GetContiguousArray(0, 5000);
            IBitset testSet = CreateSetFromIndices(set, 5000);
            testSet.Set(2, false);
            bool expected = false;
            bool result = testSet.Get(2);
            Assert.Equal(expected, result);
        }

        [Fact]
        public virtual void SetRangeTrueLargeTest()
        {
            int[] set = SetGenerator.GetContiguousArray(0, 5000);
            IBitset testSet = CreateSetFromIndices(set, 5000);
            testSet.Set(5007, 5009, true);
            bool expected = true;
            bool result = testSet.Get(8);
            Assert.Equal(expected, result);
        }

        [Fact]
        public virtual void SetRangeFalseLargeTest()
        {
            int[] set = SetGenerator.GetContiguousArray(0, 5000);
            IBitset testSet = CreateSetFromIndices(set, 5000);
            testSet.Set(1, 3, false);
            bool expected = false;
            bool result = testSet.Get(2);
            Assert.Equal(expected, result);
        }

        [Fact]
        public virtual void DifferenceWithTest()
        {
            // Test arrayContainer-based sets
            int[] set1 = { 1, 2, 3, 7 };
            RoaringBitset testSet1 = RoaringBitset.Create(set1);

            int[] set2 = { 1, 4, 7 };
            RoaringBitset testSet2 = RoaringBitset.Create(set2);

            testSet1.DifferenceWith(testSet2);

            Assert.False(testSet1.Get(1));
            Assert.True(testSet1.Get(3));

            // Test bitsetContainer-based sets
            int[] set3 = SetGenerator.GetContiguousArray(0, 5000);
            RoaringBitset testSet3 = RoaringBitset.Create(set3);

            int[] setExceptions = { 4 };
            int[] set4 = SetGenerator.GetContiguousArrayWithExceptions(0, 5000, setExceptions);
            RoaringBitset testSet4 = RoaringBitset.Create(set4);

            // Reduce contiguous array to single value (4) via DifferenceWith
            testSet3.DifferenceWith(testSet4);

            Assert.False(testSet3.Get(2));
            Assert.True(testSet3.Get(4));

            // Reduce testSet2 to 4 as well
            testSet2.DifferenceWith(testSet4);
            Assert.False(testSet2.Get(1));
            Assert.True(testSet2.Get(4));

            // Remove contents of set1 from set4
            testSet4.DifferenceWith(testSet1);
            Assert.False(testSet4.Get(2));
            Assert.True(testSet4.Get(6));
        }
    }
}