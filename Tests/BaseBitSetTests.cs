using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BitsetsNET.Tests
{
    /// <summary>
    /// Summary description for IBitSet
    /// </summary>
    public abstract class BaseBitSetTests
    {
        protected const int TEST_SET_LENGTH = 10;

        protected abstract IBitset CreateSetFromIndices(int[] indices, int length);

        [Fact]
        public void AndTest()
        {
            int[] first = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            int[] second = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            int[] result = first.Intersect(second).ToArray();
            IBitset expected = CreateSetFromIndices(result, TEST_SET_LENGTH);
            IBitset actual = CreateSetFromIndices(first, TEST_SET_LENGTH).And(CreateSetFromIndices(second, TEST_SET_LENGTH));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AndWithTest()
        {
            int[] first = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            int[] second = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            int[] result = first.Intersect(second).ToArray();
            IBitset testSet = CreateSetFromIndices(first, TEST_SET_LENGTH);
            testSet.AndWith(CreateSetFromIndices(second, TEST_SET_LENGTH));

            Assert.Equal(CreateSetFromIndices(result, TEST_SET_LENGTH), testSet);
        }

        [Fact]
        public void CloneTest()
        {
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            IBitset testSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            IBitset clone = testSet.Clone();
            Assert.Equal(clone, testSet);
        }

        [Fact]
        public void GetTest()
        {
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            IBitset testSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            bool expected = set.Contains(2);
            bool result = testSet.Get(2);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void OrTest()
        {
            int[] first = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            int[] second = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            int[] result = first.Union(second).ToArray();
            IBitset expected = CreateSetFromIndices(result, TEST_SET_LENGTH);
            IBitset actual = CreateSetFromIndices(first, TEST_SET_LENGTH).Or(CreateSetFromIndices(second, TEST_SET_LENGTH));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OrWithTest()
        {
            int[] first = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            int[] second = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            int[] result = first.Union(second).ToArray();

            IBitset testSet = CreateSetFromIndices(first, TEST_SET_LENGTH);
            testSet.OrWith(CreateSetFromIndices(second, TEST_SET_LENGTH));

            Assert.Equal(CreateSetFromIndices(result, TEST_SET_LENGTH), testSet);
        }

        [Fact]
        public void SetTrueTest()
        {
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            IBitset testSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            testSet.Set(8, true);
            bool expected = true;
            bool result = testSet.Get(8);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SetFalseTest()
        {
            int[] set = { 1, 2, 3 };
            IBitset testSet = CreateSetFromIndices(set, 4);
            testSet.Set(2, false);
            bool expected = false;
            bool result = testSet.Get(2);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SetRangeTrueTest()
        {
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            IBitset testSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            testSet.Set(7, 9, true);
            bool expected = true;
            bool result = testSet.Get(8);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SetRangeFalseTest()
        {
            int[] set = { 1, 2, 3 };
            IBitset testSet = CreateSetFromIndices(set, 4);
            testSet.Set(1, 3, false);
            bool expected = false;
            bool result = testSet.Get(2);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FlipTrueTest()
        {
            int[] set = { 1, 2, 3, 5 };
            IBitset testSet = CreateSetFromIndices(set, 6);
            testSet.Flip(4);
            bool expected = true;
            bool result = testSet.Get(4);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FlipFalseTest()
        {
            int[] set = { 1, 2, 3, 5 };
            IBitset testSet = CreateSetFromIndices(set, 6);
            testSet.Flip(2);
            bool expected = false;
            bool result = testSet.Get(2);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FlipRangeTrueTest()
        {
            int[] set = { 1, 2, 3, 7 };
            IBitset testSet = CreateSetFromIndices(set, 8);
            testSet.Flip(4, 6);
            bool expected = true;
            bool result = testSet.Get(5);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FlipRangeFalseTest()
        {
            int[] set = { 1, 2, 3, 7 };
            IBitset testSet = CreateSetFromIndices(set, 8);
            testSet.Flip(2, 4);
            bool expected = false;
            bool result = testSet.Get(3);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void DifferenceTest()
        {
            int[] set1 = { 1, 2, 3, 7 };
            IBitset testSet1 = CreateSetFromIndices(set1, 8);

            int[] set2 = { 1, 4, 7 };
            IBitset testSet2 = CreateSetFromIndices(set2, 8);

            // These sparse sets will all use array containers.
            IBitset arrayContainerDiffSet = testSet1.Difference(testSet2);

            Assert.False(arrayContainerDiffSet.Get(1));
            Assert.True(arrayContainerDiffSet.Get(3));

            // Test difference from large contiguous bitset to exercise bitsetcontainers.
            int[] set3 = SetGenerator.GetContiguousArray(0, 5000);
            IBitset testSet3 = CreateSetFromIndices(set3, 5000);

            int[] setExceptions = { 4 };
            int[] set4 = SetGenerator.GetContiguousArrayWithExceptions(0, 5000, setExceptions);
            IBitset testSet4 = CreateSetFromIndices(set4, 5000);

            // Both sets are using bitset containers
            IBitset bitsetContainerDiffSet = testSet3.Difference(testSet4);

            Assert.False(bitsetContainerDiffSet.Get(1));
            Assert.True(bitsetContainerDiffSet.Get(4));

            // Diff sets using bitset containers with array containers and vice versa
            IBitset mixedDiffSet1 = testSet4.Difference(testSet2);
            IBitset mixedDiffSet2 = testSet2.Difference(testSet4);

            Assert.False(mixedDiffSet1.Get(1));
            Assert.True(mixedDiffSet1.Get(3));

            Assert.False(mixedDiffSet2.Get(1));
            Assert.True(mixedDiffSet2.Get(4));
        }

        [Fact]
        public void CardinalityTest()
        {
            int[] set = SetGenerator.GetContiguousArray(1, 5000);
            IBitset testSet = CreateSetFromIndices(set, set.Max() + 1);

            int expected = set.Length;
            int actual = testSet.Cardinality();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EnumerationTest()
        {
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            IBitset testSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            List<int> enumeratedList = new List<int>();
            foreach (int i in testSet)
            {
                enumeratedList.Add(i);
            }

            Assert.Equal(enumeratedList.ToArray(), set);
        }

        [Fact]
        public void EqualsTest()
        {
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            IBitset testSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            IBitset otherSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            Assert.Equal(testSet, otherSet);
        }

        [Fact]
        public void GetHashCodeEqualityTest()
        {
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            IBitset testSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            IBitset otherTestSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            int hash = testSet.GetHashCode();
            int otherHash = otherTestSet.GetHashCode();
            Assert.Equal(hash, otherHash);
        }

        [Fact]
        public void GetHashCodeNotEqualTest()
        {
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            IBitset testSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            IBitset otherTestSet = CreateSetFromIndices(set, TEST_SET_LENGTH);
            otherTestSet.Flip(SetGenerator.GetRandomArray(1)[0]);
            int hash = testSet.GetHashCode();
            int otherHash = otherTestSet.GetHashCode();
            Assert.NotEqual(hash, otherHash);
        }

        [Fact]
        public void SetEdgeCaseTest()
        {
            IBitset testSet = CreateSetFromIndices(new int[] { }, TEST_SET_LENGTH);
            testSet.Set(0, 1, true);
            Assert.True(testSet.Get(0));
        }
    }
}