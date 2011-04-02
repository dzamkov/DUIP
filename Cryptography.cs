using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace DUIP
{
    /// <summary>
    /// A method of encoding or decoding data using a stream. A cipher should reproduce data exactly.
    /// </summary>
    public abstract class Cipher
    {
        /// <summary>
        /// Encrypts the given data.
        /// </summary>
        public abstract void Encrypt(Key Key, ref byte[] Data);

        /// <summary>
        /// Decrypts the given data.
        /// </summary>
        public abstract void Decrypt(Key Key, ref byte[] Data);

        /// <summary>
        /// Creates a valid key pair for the cipher algorithim.
        /// </summary>
        public abstract void GenerateKeys(Random Random, out Key Encryption, out Key Decryption);

        /// <summary>
        /// Gets if this cipher has symmetric keys.
        /// </summary>
        public abstract bool Symmetric { get; }

        /// <summary>
        /// Gets the size of the encryption key.
        /// </summary>
        public abstract int EncryptKeySize { get; }

        /// <summary>
        /// Gets the size of the decryption key.
        /// </summary>
        public abstract int DecryptKeySize { get; }
    }

    /// <summary>
    /// A cipher that can work on a stream.
    /// </summary>
    public abstract class StreamCipher : Cipher
    {

    }

    /// <summary>
    /// An asymmetric cipher using the RSA algorithim.
    /// </summary>
    public class RSACipher : Cipher
    {
        public RSACipher(int KeySize)
        {
            this._KeySize = KeySize;
        }

        public override void Encrypt(Key Key, ref byte[] Data)
        {

        }

        public override void Decrypt(Key Key, ref byte[] Data)
        {

        }

        public override void GenerateKeys(Random Random, out Key Encryption, out Key Decryption)
        {
            
        }

        public override bool Symmetric
        {
            get
            {
                return false;
            }
        }

        public override int EncryptKeySize
        {
            get
            {
                return this._KeySize;
            }
        }

        public override int DecryptKeySize
        {
            get
            {
                return this._KeySize;
            }
        }

        private int _KeySize;
    }
}