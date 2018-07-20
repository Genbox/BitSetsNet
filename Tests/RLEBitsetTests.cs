using System.Collections;
using System.IO;
using Xunit;

namespace BitsetsNET.Tests
{
    public class RLEBitsetTests : BaseBitSetTests
    {
        protected override IBitset CreateSetFromIndices(int[] indices, int length)
        {
            return RLEBitset.CreateFrom(indices, length);
        }

        [Fact]
        public virtual void SerializationTest()
        {
            int[] indicies = SetGenerator.GetRandomArray(TEST_SET_LENGTH);

            RLEBitset actual = (RLEBitset)CreateSetFromIndices(indicies, TEST_SET_LENGTH);
            RLEBitset expected;

            using (MemoryStream ms = new MemoryStream())
            {
                actual.Serialize(ms);
                ms.Position = 0;
                expected = RLEBitset.Deserialize(ms);
            }

            Assert.Equal(actual, expected);
        }

        [Fact]
        public virtual void SetMethodDoesntMutateLength()
        {
            RLEBitset testSet = (RLEBitset)RLEBitset.CreateFrom(new int[] { }, TEST_SET_LENGTH);
            testSet.Set(0, true);
            testSet.Set(1, false);
            Assert.Equal(TEST_SET_LENGTH, testSet.Length());
        }

        [Fact]
        public virtual void ToBitArrayTest()
        {
            int TEST_SET_LENGTH = 10;
            int[] set = SetGenerator.GetRandomArray(TEST_SET_LENGTH);
            BitArray setArray = new BitArray(TEST_SET_LENGTH);

            foreach (int index in set)
            {
                setArray[index] = true;
            }

            RLEBitset testSet = (RLEBitset)CreateSetFromIndices(set, TEST_SET_LENGTH);
            BitArray testArray = testSet.ToBitArray();

            bool expected = true;
            bool actual = true;

            for (int i = 0; i < setArray.Length; i++)
            {
                if (setArray[i])
                {
                    actual &= setArray[i] == testArray[i];
                }
            }

            Assert.Equal(expected, actual);
        }
    }
}