using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client.Crypt
{
    public class AESCrypt
    {
        public static async Task<byte[]> AESEncrypt(string message, byte[] key)
        {
            try
            {
                byte[] enc_byte;

                using (Aes aes = Aes.Create())
                {
                    aes.GenerateIV();
                    aes.Key = key;

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        memStream.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                        {
                            using (StreamWriter encryptWriter = new StreamWriter(cryptoStream))
                            {
                                await encryptWriter.WriteAsync(message);
                            }
                        }

                        enc_byte = memStream.ToArray();
                    }
                }

                return enc_byte;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The encryption string AES failed - {ex}");
                throw new Exception(ex.Message);
            }
        }

        public static byte[] AESEncrypt(byte[] message, byte[] key)
        {
            try
            {
                byte[] enc_byte;

                using (Aes aes = Aes.Create())
                {
                    aes.GenerateIV();
                    aes.Key = key;

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        memStream.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                        {
                            using (StreamWriter encryptWriter = new StreamWriter(cryptoStream))
                            {
                                cryptoStream.Write(message, 0, message.Length);
                                cryptoStream.FlushFinalBlock();
                            }
                        }

                        enc_byte = memStream.ToArray();
                    }
                }

                return enc_byte;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The encryption byte AES failed - {ex}");
                throw new Exception(ex.Message);
            }
        }

        public static async Task<string> AESDecrypt_String(byte[] enc_mess, byte[] key)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    int BlockSize = aes.BlockSize / 8;

                    byte[] iv = new byte[BlockSize];
                    byte[] mess = new byte[enc_mess.Length - BlockSize];

                    for (int i = 0; i < BlockSize; i++)
                        iv[i] = enc_mess[i];

                    for (int i = BlockSize, j = 0; i < enc_mess.Length; i++, j++)
                        mess[j] = enc_mess[i];

                    string decryptedMessage;

                    using (MemoryStream memStream = new MemoryStream(mess))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            using (StreamReader decryptReader = new StreamReader(cryptoStream))
                            {
                                decryptedMessage = await decryptReader.ReadToEndAsync();
                            }
                        }
                    }

                    return decryptedMessage;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"The decryption string AES failed - {ex}");
                throw new Exception(ex.Message);
            }

        }

        public static byte[] AESDecrypt_Byte(byte[] enc_mess, byte[] key)
        {
            try
            {
                byte[] iv = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    iv[i] = enc_mess[i];
                }

                byte[] mess = new byte[enc_mess.Length - 16];
                for (int i = 16, j = 0; i < enc_mess.Length; i++, j++)
                    mess[j] = enc_mess[i];

                byte[] decryptedMessage;

                using (var aes = Aes.Create())
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(mess, 0, mess.Length);
                            cryptoStream.FlushFinalBlock();
                        }

                        decryptedMessage = ms.ToArray();
                    }

                }

                return decryptedMessage;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"The decryption byte AES failed - {ex}");
                throw new Exception(ex.Message);
            }

        }
    }
}