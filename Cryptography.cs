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
        public RSACipher()
        {

        }

        public override void Encrypt(Key Key, ref byte[] Data)
        {

        }

        public override void Decrypt(Key Key, ref byte[] Data)
        {
            throw new NotImplementedException();
        }

        public override void GenerateKeys(Random Random, out Key Encryption, out Key Decryption)
        {
            RSACryptoServiceProvider p = new RSACryptoServiceProvider();
            RSAParameters r = p.ExportParameters(true);
            

            throw new NotImplementedException();
        }

        public override bool Symmetric
        {
            get
            {
                return false;
            }
        }
    }
}