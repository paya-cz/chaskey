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
        private uint[] key = new uint[4];

        /// <summary>Key derived from <see cref="key"/>, used when the message is properly aligned.</summary>
        private uint[] keyAligned = new uint[4];

        /// <summary>Key derived from <see cref="key"/>, used when the message is not aligned.</summary>
        private uint[] keyUnaligned = new uint[4];

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of Chaskey PRF using specified key and performs key scheduling.</summary>
        /// <param name="key"><para>Byte array holding the key.</para><para>Must not be null.</para></param>
        /// <param name="offset">Offset in <paramref name="key"/> where the actual key starts.</param>
        /// <param name="length">Length of the key. Must be 128-bits (16 bytes).</param>
        /// <exception cref="System.ArgumentNulLException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="offset"/> or <paramref name="length"/> are out of range.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the key is not 128-bit.</exception>
        public Chaskey(byte[] key, int offset, int length)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (length != 128 / 8)
                throw new ArgumentException("The specified key is not of valid size for this algorithm.");
            if (length > key.Length - offset)
                throw new ArgumentOutOfRangeException("length", "Attempt to initialize beyond end of buffer.");

            // Copy key
            Buffer.BlockCopy(key, offset, this.key, 0, length);
            // Key schedule
            Subkeys(this.key, this.keyAligned, this.keyUnaligned);
        }

        #endregion

        #region Key scheduling - (TimesTwo, Subkeys)

        private static readonly uint[] C = new[] { 0x00u, 0x87u };

        private static void Subkeys(uint[] key, uint[] keyAligned, uint[] keyUnaligned)
        {
            TimesTwo(keyAligned, key);
            TimesTwo(keyUnaligned, keyAligned);
        }

        private static void TimesTwo(uint[] @out, uint[] @in)
        {
            @out[0] = (@in[0] << 1) ^ C[@in[3] >> 31];
            @out[1] = (@in[1] << 1) | (@in[0] >> 31);
            @out[2] = (@in[2] << 1) | (@in[1] >> 31);
            @out[3] = (@in[3] << 1) | (@in[2] >> 31);
        }

        #endregion

        #region PRF computation (Compute)

        /// <summary>Computes Chaskey tag for the specified message.</summary>
        /// <param name="message">Message which is used to compute the Chaskey tag.</param>
        /// <returns>Returns 128-bit (16 bytes) tag.</returns>
        /// <remarks>Does not throw exceptions.</remarks>
        public unsafe byte[] Compute(ArraySegment<byte> message)
        {
            fixed (byte* pMessageStart = message.Array)
            {
                var pMessage = (uint*)pMessageStart;
                // Pointer to the 2nd to last message block (aligned to 16 bytes)
                var pMessageEndAligned = pMessage + (((message.Count - 1) >> 4) << 2);

                // 128-bit internal state
                var state0 = this.key[0];
                var state1 = this.key[1];
                var state2 = this.key[2];
                var state3 = this.key[3];

                // Process 128 bits of the message at a time
                if (message.Count > 0)
                {
                    for (; pMessage != pMessageEndAligned; pMessage += 4)
                    {
                        // Mix message bits into the state
                        state0 ^= pMessage[0];
                        state1 ^= pMessage[1];
                        state2 ^= pMessage[2];
                        state3 ^= pMessage[3];

                        // Mix the internal state
                        Permute(ref state0, ref state1, ref state2, ref state3);
                    }
                }

                // Process last block (0 to 16 bytes)
                {
                    uint[] lastBlockKey;

                    if (message.Count > 0 && (message.Count & 0xF) == 0)
                    {
                        lastBlockKey = this.keyAligned;

                        // Mix last 16 bytes into the state
                        state0 ^= pMessage[0];
                        state1 ^= pMessage[1];
                        state2 ^= pMessage[2];
                        state3 ^= pMessage[3];
                    }
                    else
                    {
                        lastBlockKey = this.keyUnaligned;

                        // Mix last few bytes (less than 16) into the state
                        var lastBlock = new byte[16];
                        var i = 0;
                        for (var p = (byte*)pMessage; p != pMessageStart + message.Count; p++, i++)
                            lastBlock[i] = *p;
                        lastBlock[i++] = 0x01; // padding bit

                        fixed (byte* pLastBlock = lastBlock)
                        {
                            var pLastBlock32 = (uint*)pLastBlock;
                            state0 ^= pLastBlock32[0];
                            state1 ^= pLastBlock32[1];
                            state2 ^= pLastBlock32[2];
                            state3 ^= pLastBlock32[3];
                        }
                    }

                    state0 ^= lastBlockKey[0];
                    state1 ^= lastBlockKey[1];
                    state2 ^= lastBlockKey[2];
                    state3 ^= lastBlockKey[3];

                    Permute(ref state0, ref state1, ref state2, ref state3);

                    state0 ^= lastBlockKey[0];
                    state1 ^= lastBlockKey[1];
                    state2 ^= lastBlockKey[2];
                    state3 ^= lastBlockKey[3];
                }

                // Return tag - the final internal state
                var tag = new byte[sizeof(uint) * 4];
                fixed (byte* pTag = tag)
                {
                    var pTag32 = (uint*)pTag;
                    pTag32[0] = state0;
                    pTag32[1] = state1;
                    pTag32[2] = state2;
                    pTag32[3] = state3;
                }
                return tag;
            }
        }

        #endregion

        #region Chaskey round function

        /// <summary>Left bitwise rotation.</summary>
        /// <param name="x">Variable to rotate.</param>
        /// <param name="b">Number of bits to rotate.</param>
        /// <returns>Rotated variable.</returns>
        /// <remarks>Does not throw exceptions.</remarks>
        private static uint ROTL32(uint x, int b)
        {
            return (x << b) | (x >> (32 - b));
        }

        /// <summary>Performs single round of Chaskey on a 128-bit internal state.</summary>
        /// <param name="v0">32 bits of internal state.</param>
        /// <param name="v1">32 bits of internal state.</param>
        /// <param name="v2">32 bits of internal state.</param>
        /// <param name="v3">32 bits of internal state.</param>
        /// <remarks>Does not throw exceptions.</remarks>
        private static void ChaskeyRound(ref uint v0, ref uint v1, ref uint v2, ref uint v3)
        {
            v0 += v1; v1 = ROTL32(v1, 5); v1 ^= v0; v0 = ROTL32(v0, 16);
            v2 += v3; v3 = ROTL32(v3, 8); v3 ^= v2;
            v0 += v3; v3 = ROTL32(v3, 13); v3 ^= v0;
            v2 += v1; v1 = ROTL32(v1, 7); v1 ^= v2; v2 = ROTL32(v2, 16);
        }

        /// <summary>Performs 8 rounds of Chaskey on a 128-bit internal state.</summary>
        /// <param name="v0">32 bits of internal state.</param>
        /// <param name="v1">32 bits of internal state.</param>
        /// <param name="v2">32 bits of internal state.</param>
        /// <param name="v3">32 bits of internal state.</param>
        /// <remarks>Does not throw exceptions.</remarks>
        private static void Permute(ref uint v0, ref uint v1, ref uint v2, ref uint v3)
        {
            for (int i = 0; i < 8; i++)
            {
#if CHASKEY_INLINE
                v0 += v1;
                v1 = (v1 << 5) | (v1 >> (32 - 5));
                v1 ^= v0;
                v0 = (v0 << 16) | (v0 >> (32 - 16));

                v2 += v3;
                v3 = (v3 << 8) | (v3 >> (32 - 8));
                v3 ^= v2;

                v0 += v3;
                v3 = (v3 << 13) | (v3 >> (32 - 13));
                v3 ^= v0;

                v2 += v1;
                v1 = (v1 << 7) | (v1 >> (32 - 7));
                v1 ^= v2;
                v2 = (v2 << 16) | (v2 >> (32 - 16));
#else
                ChaskeyRound(ref v0, ref v1, ref v2, ref v3);
#endif
            }
        }

        #endregion
    }
}
