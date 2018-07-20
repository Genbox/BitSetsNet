using System;
using System.Collections.Generic;
using System.IO;

namespace BitsetsNET
{
    internal class RoaringArray
    {
        protected static short INITIAL_CAPACITY = 5;

        private ushort[] keys;
        private Container[] values;

        public RoaringArray() : this(INITIAL_CAPACITY) { }

        public RoaringArray(int capacity)
        {
            keys = new ushort[capacity];
            values = new Container[capacity];
        }

        public int Size { get; set; }

        public void Append(ushort key, Container value)
        {
            ExtendArray(1);
            keys[Size] = key;
            values[Size] = value;
            Size++;
        }

        /// <summary>Append copy of the one value from another array</summary>
        /// <param name="sa">Other array</param>
        /// <param name="index">Index in the other array</param>
        public void AppendCopy(RoaringArray sa, int index)
        {
            ExtendArray(1);
            keys[Size] = sa.keys[index];
            values[Size] = sa.values[index].Clone();
            Size++;
        }

        /// <summary>Append copies of the values from another array</summary>
        /// <param name="sa">other array</param>
        /// <param name="startingIndex">starting index in the other array</param>
        /// <param name="end">(exclusive) in the other array</param>
        public void AppendCopy(RoaringArray sa, int startingIndex, int end)
        {
            ExtendArray(end - startingIndex);
            for (int i = startingIndex; i < end; ++i)
            {
                keys[Size] = sa.keys[i];
                values[Size] = sa.values[i].Clone();
                Size++;
            }
        }

        internal int AdvanceUntil(ushort x, int pos)
        {
            int lower = pos + 1;

            // special handling for a possibly common sequential case
            if (lower >= Size || keys[lower] >= x)
            {
                return lower;
            }

            int spansize = 1; // could set larger

            // bootstrap an upper limit

            while (lower + spansize < Size && keys[lower + spansize] < x)
            {
                spansize *= 2; // hoping for compiler will reduce to shift
            }

            int upper = lower + spansize < Size ? lower + spansize : Size - 1;

            // maybe we are lucky (could be common case when the seek ahead
            // expected to be small and sequential will otherwise make us look bad)
            if (keys[upper] == x)
            {
                return upper;
            }

            if (keys[upper] < x)
            {
                // means array has no item key >= x
                return Size;
            }

            // we know that the next-smallest span was too small
            lower += spansize / 2;

            // else begin binary search
            // invariant: array[lower]<x && array[upper]>x
            while (lower + 1 != upper)
            {
                int mid = (lower + upper) / 2;
                if (keys[mid] == x)
                {
                    return mid;
                }

                if (keys[mid] < x)
                {
                    lower = mid;
                }
                else
                {
                    upper = mid;
                }
            }

            return upper;
        }

        /// <summary>
        /// Creates a deep copy of this roaring array
        /// </summary>
        /// <returns>A new roaring array</returns>
        public RoaringArray Clone()
        {
            RoaringArray sa = new RoaringArray();

            sa.keys = new ushort[keys.Length];
            keys.CopyTo(sa.keys, 0);

            sa.values = new Container[values.Length];
            values.CopyTo(sa.values, 0);

            for (int k = 0; k < Size; ++k)
            {
                sa.values[k] = sa.values[k].Clone();
            }

            sa.Size = Size;

            return sa;
        }

        /// <summary>
        /// Copies a range of keys and values from one location in 
        /// the roaring array to another.
        /// </summary>
        /// <param name="begin">Original starting index</param>
        /// <param name="end">Original ending index</param>
        /// <param name="newBegin">New starting index</param>
        internal void CopyRange(int begin, int end, int newBegin)
        {
            int range = end - begin;
            Array.Copy(keys, begin, keys, newBegin, range);
            Array.Copy(values, begin, values, newBegin, range);
        }

        public ushort GetKeyAtIndex(int i)
        {
            return keys[i];
        }

        public int GetIndex(ushort x)
        {
            //TODO: optimize this
            //before the binary search we optimize for frequent cases
            if (Size == 0 || keys[Size - 1] == x)
            {
                return Size - 1;
            }

            return BinarySearch(0, Size, x);
        }

        private int BinarySearch(int begin, int end, ushort key)
        {
            return Utility.UnsignedBinarySearch(keys, begin, end, key);
        }

        /// <summary>
        /// Logically resizes the Roaring Array after an in-place operation.
        /// Fills all keys and values after its new last index with zeros
        /// and null, respectively, and changes the size to the new size.
        /// </summary>
        /// <param name="newSize">the new size of the roaring array</param>
        public void Resize(int newSize)
        {
            Utility.Fill(keys, newSize, Size, (ushort)0);
            Utility.Fill(values, newSize, Size, null);
            Size = newSize;
        }

        public void SetContainerAtIndex(int i, Container c)
        {
            values[i] = c;
        }

        public Container GetContainerAtIndex(int i)
        {
            return values[i];
        }

        public void RemoveAtIndex(int i)
        {
            Array.Copy(keys, i + 1, keys, i, Size - i - 1);
            keys[Size - 1] = 0;
            Array.Copy(values, i + 1, values, i, Size - i - 1);
            values[Size - 1] = null;
            Size--;
        }

        public void RemoveIndexRange(int begin, int end)
        {
            if (end <= begin)
            {
                return;
            }

            int range = end - begin;

            Array.Copy(keys, end, keys, begin, Size - end);
            Array.Copy(values, end, values, begin, Size - end);

            for (int i = 1; i <= range; ++i)
            {
                keys[Size - i] = 0;
                values[Size - i] = null;
            }

            Size -= range;
        }

        /// <summary>
        /// insert a new key, it is assumed that it does not exist
        /// </summary>
        /// <param name="i"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void InsertNewKeyValueAt(int i, ushort key, Container value)
        {
            ExtendArray(1);
            Array.Copy(keys, i, keys, i + 1, Size - i);
            keys[i] = key;
            Array.Copy(values, i, values, i + 1, Size - i);
            values[i] = value;
            Size++;
        }

        /// <summary>
        /// Replaces the key and container value at a given index.
        /// </summary>
        /// <param name="i">the working index</param>
        /// <param name="key">key to set</param>
        /// <param name="c">container to set</param>
        public void ReplaceKeyAndContainerAtIndex(int i, ushort key, Container c)
        {
            keys[i] = key;
            values[i] = c;
        }

        // make sure there is capacity for at least k more elements
        public void ExtendArray(int k)
        {
            // size + 1 could overflow
            if (Size + k >= keys.Length)
            {
                int newCapacity;
                if (keys.Length < 1024)
                {
                    newCapacity = 2 * (Size + k);
                }
                else
                {
                    newCapacity = 5 * (Size + k) / 4;
                }

                //TODO: this may be jank
                Array.Resize(ref keys, newCapacity);
                Array.Resize(ref values, newCapacity);
            }
        }

        public override bool Equals(object o)
        {
            if (!(o is RoaringArray))
            {
                return false;
            }

            RoaringArray srb = (RoaringArray)o;
            if (srb.Size != Size)
            {
                return false;
            }

            for (int i = 0; i < srb.Size; ++i)
            {
                if (keys[i] != srb.keys[i] || !values[i].Equals(srb.values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Size;
                for (int i = 0; i < Size; ++i)
                {
                    hash = unchecked(17 * hash + keys[i] * 0xF0F0F0 + values[i].GetHashCode());
                }

                return hash;
            }
        }

        /// <summary>
        /// Serialize the roaring array into a binary format.
        /// </summary>
        /// <param name="writer">The writer to write the serialization to.</param>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Size);

            for (int i = 0; i < Size; i++)
            {
                writer.Write(keys[i]);
                values[i].Serialize(writer);
            }
        }

        /// <summary>
        /// Deserialize a roaring array from a binary format, as written by the Serialize method.
        /// </summary>
        /// <param name="reader">The reader from which to deserialize the roaring array.</param>
        /// <returns></returns>
        public static RoaringArray Deserialize(BinaryReader reader)
        {
            int size = reader.ReadInt32();
            RoaringArray array = new RoaringArray(size);
            array.Size = size;

            for (int i = 0; i < size; i++)
            {
                array.keys[i] = (ushort)reader.ReadInt16();
                array.values[i] = Container.Deserialize(reader);
            }

            return array;
        }

        /// <summary>
        /// Get an enumerator of the set indices of this bitset.
        /// </summary>
        /// <returns>A enumerator giving the set (i.e. for which the 
        /// bit is '1' or true) indices for this bitset.</returns>
        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < Size; i++)
            {
                int highbits = keys[i] << 16;
                foreach (ushort lowbits in values[i])
                {
                    yield return highbits + lowbits;
                }
            }
        }
    }
}