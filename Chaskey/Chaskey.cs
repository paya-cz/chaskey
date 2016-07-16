// ###############################################################
// # Downloaded from:         https://github.com/paya-cz/chaskey #
// # Author:                  Pavel Werl                         #
// # License:                 Public Domain                      #
// ###############################################################

// Chaskey paper:           https://eprint.iacr.org/2014/386.pdf
// Chaskey website:         http://mouha.be/chaskey/
// Implementation based on: http://mouha.be/wp-content/uploads/chaskey-speed.c

using System;

namespace Chaskey
{
    /// <summary>Implements Chaskey secure PRF.</summary>
    public sealed class Chaskey
    {
        #region Fields

        /// <summary>Raw key.</summary>
        private readonly uint[] key = new uint[4];

        /// <summary>Key derived from <see cref="key"/>, used when the message is properly aligned.</summary>
        private readonly uint[] keyAligned = new uint[4];

        /// <summary>Key derived from <see cref="key"/>, used when the message is not aligned.</summary>
        private readonly uint[] keyUnaligned = new uint[4];

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of Chaskey PRF using specified key and performs key scheduling.</summary>
        /// <param name="key"><para>Byte array holding the key.</para><para>Must be exactly 16 bytes long and must not be null.</para></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the key is not 128-bits long (16 bytes).</exception>
        public Chaskey(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length != 16)
                throw new ArgumentException("The key must be 128 bits long (16 bytes).", nameof(key));

            this.Initialize(key, 0, key.Length);
        }

        /// <summary>Initializes a new instance of Chaskey PRF using specified key and performs key scheduling.</summary>
        /// <param name="key"><para>Byte array holding the key.</para><para>Must not be null.</para></param>
        /// <param name="offset">Offset in <paramref name="key"/> where the actual key starts.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="offset"/> is negative.</exception>
        /// <exception cref="ArgumentException">Thrown when the key is not 128-bit.</exception>
        public Chaskey(byte[] key, int offset)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Array offset cannot be negative.");
            if (key.Length - offset < 16)
                throw new ArgumentException("The specified '" + nameof(offset) + "' parameter does not specify a valid 16-byte range in '" + nameof(key) + "'.");

            this.Initialize(key, offset, 16);
        }

        #endregion

        #region Key scheduling - (Initialize, TimesTwo)

        private void Initialize(byte[] key, int offset, int count)
        {
            // Copy key
            Buffer.BlockCopy(key, offset, this.key, 0, count);
            // Key schedule
            TimesTwo(this.keyAligned, this.key);
            TimesTwo(this.keyUnaligned, this.keyAligned);
        }

        private static void TimesTwo(uint[] @out, uint[] @in)
        {
            @out[0] = @in[0] << 1 ^ (0x87u & (uint)((int)@in[3] >> 31));
            @out[1] = @in[1] << 1 | @in[0] >> 31;
            @out[2] = @in[2] << 1 | @in[1] >> 31;
            @out[3] = @in[3] << 1 | @in[2] >> 31;
        }

        #endregion

        #region PRF computation (Compute)

        /// <summary>Computes Chaskey 128-bit tag for the specified message.</summary>
        /// <param name="data"><para>The byte array for which to compute Chaskey tag.</para><para>Must not be null.</para></param>
        /// <returns>Returns 128-bit (16 bytes) Chaskey tag.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        public byte[] Compute(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var tag = new byte[16];
            this.Compute(data, 0, data.Length, tag, 0);
            return tag;
        }
        
        /// <summary>Computes Chaskey 128-bit tag for the specified message.</summary>
        /// <param name="data">The byte array for which to compute Chaskey tag.</param>
        /// <returns>Returns 128-bit (16 bytes) Chaskey tag.</returns>
        /// <remarks>Does not throw exceptions.</remarks>
        public byte[] Compute(ArraySegment<byte> data)
        {
            var tag = new byte[16];
            this.Compute(data.Array, data.Offset, data.Count, tag, 0);
            return tag;
        }

        /// <summary>Computes Chaskey 128-bit tag for the specified message.</summary>
        /// <param name="data"><para>The byte array for which to compute Chaskey tag.</para><para>Must not be null.</para></param>
        /// <param name="offset">The zero-based index of the first element in the range.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>Returns 128-bit (16 bytes) Chaskey tag.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="offset"/> and <paramref name="count"/> do not specify a valid range in <paramref name="data"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        public byte[] Compute(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Array offset cannot be negative.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Number of array elements cannot be negative.");
            if (data.Length - offset < count)
                throw new ArgumentException("The specified '" + nameof(offset) + "' and '" + nameof(count) + "' parameters do not specify a valid range in '" + nameof(data) + "'.");

            var tag = new byte[16];
            this.Compute(data, offset, count, tag, 0);
            return tag;
        }

        /// <summary><para>Computes Chaskey 128-bit tag for the specified message.</para><para>Use this method for fastest allocation-free implementation.</para></summary>
        /// <param name="data"><para>The byte array for which to compute Chaskey tag.</para><para>Must not be null.</para></param>
        /// <param name="dataOffset">The zero-based index of the first element in the data range.</param>
        /// <param name="dataCount">The number of elements in the range.</param>
        /// <param name="tag"><para>The byte array that receives the computed Chaskey tag.</para><para>Must not be null.</para></param>
        /// <param name="tagOffset">The zero-based index of the range in <paramref name="tag"/> where to store the computed tag.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> or <paramref name="tag"/> is null.</exception>
        /// <exception cref="ArgumentException">
        ///     <para>Thrown when <paramref name="dataOffset"/> and <paramref name="dataCount"/> do not specify a valid range in <paramref name="data"/>.</para>
        ///     <para>Thrown when <paramref name="tagOffset"/> does not specify a valid 16-byte range in <paramref name="tag"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dataOffset"/>, <paramref name="dataCount"/> or <paramref name="tagOffset"/> is negative.</exception>
        public void Compute(byte[] data, int dataOffset, int dataCount, byte[] tag, int tagOffset)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (dataOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(dataOffset), "Array offset cannot be negative.");
            if (dataCount < 0)
                throw new ArgumentOutOfRangeException(nameof(dataCount), "Number of array elements cannot be negative.");
            if (data.Length - dataOffset < dataCount)
                throw new ArgumentException("The specified '" + nameof(dataOffset) + "' and '" + nameof(dataCount) + "' parameters do not specify a valid range in '" + nameof(data) + "'.");
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));
            if (tagOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(tagOffset), "Array offset cannot be negative.");
            if (tag.Length - tagOffset < 16)
                throw new ArgumentException("The specified '" + nameof(tagOffset) + "' parameter does not specify a valid 16-byte range in '" + nameof(tag) + "'.");

            // 128-bit internal state
            var v0 = this.key[0];
            var v1 = this.key[1];
            var v2 = this.key[2];
            var v3 = this.key[3];

            // Key used for last round
            uint[] finalizationKey;

            unsafe
            {
                fixed (byte* dataStart = data)
                {
                    // Initialize pointer to current data block to the start of the range
                    var dataPointer = (uint*)(dataStart + dataOffset);

                    // Process 128 bits of the message at a time
                    if (dataCount > 16)
                    {
                        // Pointer to the 2nd to last message block (aligned to 16 bytes)
                        var dataEndAligned = dataPointer + (dataCount - 1 >> 4 << 2);

                        for (; dataPointer != dataEndAligned; dataPointer += 4)
                        {
                            // Mix message bits into the state
                            v0 ^= dataPointer[0];
                            v1 ^= dataPointer[1];
                            v2 ^= dataPointer[2];
                            v3 ^= dataPointer[3];

                            // Mix the internal state (8 rounds)
                            for (int i = 0; i < 4; i++)
                            {
                                // Round 1
                                v0 += v1;
                                v2 += v3;
                                v1 = v1 << 5 | v1 >> 27;
                                v3 = v3 << 8 | v3 >> 24;
                                v1 ^= v0;
                                v3 ^= v2;
                                v0 = v0 << 16 | v0 >> 16;
                                v2 += v1;
                                v0 += v3;
                                v1 = v1 << 7 | v1 >> 25;
                                v3 = v3 << 13 | v3 >> 19;
                                v1 ^= v2;
                                v3 ^= v0;
                                v2 = v2 << 16 | v2 >> 16;

                                // Round 2
                                v0 += v1;
                                v2 += v3;
                                v1 = v1 << 5 | v1 >> 27;
                                v3 = v3 << 8 | v3 >> 24;
                                v1 ^= v0;
                                v3 ^= v2;
                                v0 = v0 << 16 | v0 >> 16;
                                v2 += v1;
                                v0 += v3;
                                v1 = v1 << 7 | v1 >> 25;
                                v3 = v3 << 13 | v3 >> 19;
                                v1 ^= v2;
                                v3 ^= v0;
                                v2 = v2 << 16 | v2 >> 16;
                            }
                        }
                    }

                    // Mix in the last block (0 to 16 bytes)
                    if (dataCount > 0 && (dataCount & 0xF) == 0)
                    {
                        finalizationKey = this.keyAligned;

                        // Mix the last 16 bytes into the state
                        v0 ^= dataPointer[0];
                        v1 ^= dataPointer[1];
                        v2 ^= dataPointer[2];
                        v3 ^= dataPointer[3];
                    }
                    else
                    {
                        finalizationKey = this.keyUnaligned;

                        // Mix remaining bytes (less than 16) into the state
                        switch (dataCount & 0xF)
                        {
                            case 15:
                                v0 ^= *dataPointer;
                                v1 ^= *(dataPointer + 1);
                                v2 ^= *(dataPointer + 2);
                                v3 ^= *(ushort*)(dataPointer + 3) | (uint)*((byte*)dataPointer + 14) << 16 | 1U << 24;
                                break;
                            case 14:
                                v0 ^= *dataPointer;
                                v1 ^= *(dataPointer + 1);
                                v2 ^= *(dataPointer + 2);
                                v3 ^= *(ushort*)(dataPointer + 3) | 1U << 16;
                                break;
                            case 13:
                                v0 ^= *dataPointer;
                                v1 ^= *(dataPointer + 1);
                                v2 ^= *(dataPointer + 2);
                                v3 ^= *(byte*)(dataPointer + 3) | 1U << 8;
                                break;
                            case 12:
                                v0 ^= *dataPointer;
                                v1 ^= *(dataPointer + 1);
                                v2 ^= *(dataPointer + 2);
                                v3 ^= 1U;
                                break;
                            case 11:
                                v0 ^= *dataPointer;
                                v1 ^= *(dataPointer + 1);
                                v2 ^= *(ushort*)(dataPointer + 2) | (uint)*((byte*)dataPointer + 10) << 16 | 1U << 24;
                                break;
                            case 10:
                                v0 ^= *dataPointer;
                                v1 ^= *(dataPointer + 1);
                                v2 ^= *(ushort*)(dataPointer + 2) | 1U << 16;
                                break;
                            case 9:
                                v0 ^= *dataPointer;
                                v1 ^= *(dataPointer + 1);
                                v2 ^= *(byte*)(dataPointer + 2) | 1U << 8;
                                break;
                            case 8:
                                v0 ^= *dataPointer;
                                v1 ^= *(dataPointer + 1);
                                v2 ^= 1U;
                                break;
                            case 7:
                                v0 ^= *dataPointer;
                                v1 ^= *(ushort*)(dataPointer + 1) | (uint)*((byte*)dataPointer + 6) << 16 | 1U << 24;
                                break;
                            case 6:
                                v0 ^= *dataPointer;
                                v1 ^= *(ushort*)(dataPointer + 1) | 1U << 16;
                                break;
                            case 5:
                                v0 ^= *dataPointer;
                                v1 ^= *(byte*)(dataPointer + 1) | 1U << 8;
                                break;
                            case 4:
                                v0 ^= *dataPointer;
                                v1 ^= 1U;
                                break;
                            case 3:
                                v0 ^= *(ushort*)dataPointer | (uint)*((byte*)dataPointer + 2) << 16 | 1U << 24;
                                break;
                            case 2:
                                v0 ^= *(ushort*)dataPointer | 1U << 16;
                                break;
                            case 1:
                                v0 ^= *(byte*)dataPointer | 1U << 8;
                                break;
                            case 0:
                                v0 ^= 1U;
                                break;
                        }
                    }
                }
            }

            // Finalization
            {
                v0 ^= finalizationKey[0];
                v1 ^= finalizationKey[1];
                v2 ^= finalizationKey[2];
                v3 ^= finalizationKey[3];

                // Mix the internal state (8 rounds)
                for (int i = 0; i < 4; i++)
                {
                    // Round 1
                    v0 += v1;
                    v2 += v3;
                    v1 = v1 << 5 | v1 >> 27;
                    v3 = v3 << 8 | v3 >> 24;
                    v1 ^= v0;
                    v3 ^= v2;
                    v0 = v0 << 16 | v0 >> 16;
                    v2 += v1;
                    v0 += v3;
                    v1 = v1 << 7 | v1 >> 25;
                    v3 = v3 << 13 | v3 >> 19;
                    v1 ^= v2;
                    v3 ^= v0;
                    v2 = v2 << 16 | v2 >> 16;

                    // Round 2
                    v0 += v1;
                    v2 += v3;
                    v1 = v1 << 5 | v1 >> 27;
                    v3 = v3 << 8 | v3 >> 24;
                    v1 ^= v0;
                    v3 ^= v2;
                    v0 = v0 << 16 | v0 >> 16;
                    v2 += v1;
                    v0 += v3;
                    v1 = v1 << 7 | v1 >> 25;
                    v3 = v3 << 13 | v3 >> 19;
                    v1 ^= v2;
                    v3 ^= v0;
                    v2 = v2 << 16 | v2 >> 16;
                }

                v0 ^= finalizationKey[0];
                v1 ^= finalizationKey[1];
                v2 ^= finalizationKey[2];
                v3 ^= finalizationKey[3];
            }

            // Return tag - the final internal state
            unsafe
            {
                fixed (byte* tagPointer = &tag[tagOffset])
                {
                    var tagPointerUInt = (uint*)tagPointer;
                    tagPointerUInt[0] = v0;
                    tagPointerUInt[1] = v1;
                    tagPointerUInt[2] = v2;
                    tagPointerUInt[3] = v3;
                }
            }
        }

        #endregion
    }
}
