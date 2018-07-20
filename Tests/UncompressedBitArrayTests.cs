namespace BitsetsNET.Tests
{
    public class UncompressedBitArrayTests : BaseBitSetTests
    {
        protected override IBitset CreateSetFromIndices(int[] indices, int length)
        {
            return new UncompressedBitArray(indices, length);
        }
    }
}