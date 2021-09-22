using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Server.Crypt
{
    public class AESCrypt
    {
        public static string AESEncrypt(string message, byte[] key)
        {
            try
            {
                string enc_mess;
                byte[] enc_byte;

                using (MemoryStream memStream = new MemoryStream())
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.GenerateIV();

                        aes.Key = key;

                        memStream.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                        {
                            using (StreamWriter encryptWriter = new StreamWriter(cryptoStream))
                            {
                                encryptWriter.Write(message);
                            }
                        }

                        enc_byte = memStream.ToArray();
                        enc_mess = Encoding.Unicode.GetString(enc_byte);
                    }
                }

                return enc_mess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The encryption failed - {ex}");
                throw new Exception(ex.Message);
            }
        }

        public static string AESDecrypt(byte[] enc_mess, byte[] key)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] mess = new byte[enc_mess.Length - 16];

                for (int i = 0; i < 16; i++)
                    iv[i] = enc_mess[i];
                            
                for (int i = 16, j = 0; i < enc_mess.Length; i++, j++)
                    mess[j] = enc_mess[i];

                string decryptedMessage;

                using (MemoryStream memStream = new MemoryStream(mess))
                {
                    using (Aes aes = Aes.Create())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            using (StreamReader decryptReader = new StreamReader(cryptoStream))
                            {
                                decryptedMessage = decryptReader.ReadToEnd();
                            }
                        }
                    }
                }

                return decryptedMessage;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"The encryption failed - {ex}");
                throw new Exception(ex.Message);
            }

        }
    }
}