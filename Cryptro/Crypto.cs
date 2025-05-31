using System;
using System.Security.Cryptography;
using System.Text;



namespace Cryptro
{
   static class Crypto
    {    
        #region Methods

        /// <summary>
        /// Encrypt the plain text using the secret provided key. 
        /// </summary>
        /// <param name="PlainText">Plain text in readable form.</param>
        /// <param name="SecretKey">Secret key, keep it secret, keep it safe.</param>
        /// <returns></returns>
        public static string Encrypt(string PlainText, string SecretKey)
        {
            try
            {
                byte[] salt = GenerateSalt();
                (byte[] key, byte[] iv) = DeriveKeyAndIV(SecretKey, salt);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(salt, 0, salt.Length); // prepend salt

                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(PlainText);
                        }

                        return Convert.ToBase64String(ms.ToArray());
                    }
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Decrypt the cipher text using the secret provided key. 
        /// </summary>
        /// <param name="CipherText">Cipher text in non-readable form.</param>
        /// <param name="SecretKey">Secret key, keep it secret, keep it safe.</param>
        /// <returns></returns>
        public static string Decrypt(string CipherText, string SecretKey)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(CipherText);
                byte[] salt = new byte[16];
                Array.Copy(cipherBytes, 0, salt, 0, salt.Length);

                (byte[] key, byte[] iv) = DeriveKeyAndIV(SecretKey, salt);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(cipherBytes, salt.Length, cipherBytes.Length - salt.Length))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            //RandomNumberGenerator.Fill(salt);
            //return salt;

            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);            
            return salt;
        }

        private static (byte[] Key, byte[] IV) DeriveKeyAndIV(string SecretKey, byte[] salt)
        {
            const int iterations = 100_000; 
            using (var pbkdf2 = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                byte[] fullKey = PBKDF2(pbkdf2, salt, iterations, 48); // 32 bytes key + 16 bytes IV
                byte[] key = new byte[32];
                byte[] iv = new byte[16];
                Array.Copy(fullKey, 0, key, 0, 32);
                Array.Copy(fullKey, 32, iv, 0, 16);
                return (key, iv);
            }
        }

        private static byte[] PBKDF2(HMAC hmac, byte[] salt, int iterations, int outputBytes)
        {
            int hashLength = hmac.HashSize / 8;
            int blockCount = (int)Math.Ceiling((double)outputBytes / hashLength);
            byte[] output = new byte[outputBytes];
            byte[] buffer = new byte[hashLength];

            for (int i = 1; i <= blockCount; i++)
            {
                byte[] intBlock = BitConverter.GetBytes(i);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(intBlock);

                hmac.Initialize();
                hmac.TransformBlock(salt, 0, salt.Length, null, 0);
                hmac.TransformFinalBlock(intBlock, 0, intBlock.Length);
                Array.Copy(hmac.Hash, 0, buffer, 0, hashLength);

                byte[] temp = (byte[])buffer.Clone();

                for (int j = 1; j < iterations; j++)
                {
                    temp = hmac.ComputeHash(temp);
                    for (int k = 0; k < buffer.Length; k++)
                    {
                        buffer[k] ^= temp[k];
                    }
                }

                Array.Copy(buffer, 0, output, (i - 1) * hashLength, Math.Min(hashLength, outputBytes - (i - 1) * hashLength));
            }

            return output;
        }

        #endregion

    }
}
