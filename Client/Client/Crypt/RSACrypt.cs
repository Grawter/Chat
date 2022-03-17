using System;
using System.Text;
using System.Security.Cryptography;
using Client.Helpers;
using Client.Interfaces;

namespace Client.Crypt
{
    public class RSACrypt : IAsymmCrypt
    {
        IShowInfo showInfo = new ShowInfo();

        public byte[] Encrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo)
        {
            try
            {
                RSA RSA = RSA.Create();
                RSA.ImportParameters(RSAKeyInfo);
                return RSA.Encrypt(DataToEncrypt, RSAEncryptionPadding.Pkcs1);
            }
            catch (Exception ex)
            {
                showInfo.ShowMessage($"The encryption RSA failed - {ex}", 3);
                throw new Exception(ex.Message);
            }
        }

        public byte[] Encrypt_Str(string Text, RSAParameters RSAKeyInfo)
        {
            try
            {
                byte[] DataToEncrypt = Encoding.Unicode.GetBytes(Text);

                RSA RSA = RSA.Create();
                RSA.ImportParameters(RSAKeyInfo);
                return RSA.Encrypt(DataToEncrypt, RSAEncryptionPadding.Pkcs1);
            }
            catch (Exception)
            {
                throw new Exception("Не удалось зашифровать сообщение");
            }
            //catch (Exception ex)
            //{
            //    showInfo.ShowMessage($"The decryption string AES failed - {ex}", 3);
            //    throw new Exception(ex.Message);
            //}
        }

        public byte[] Decrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo)
        {
            try
            {
                RSA RSA = RSA.Create();
                RSA.ImportParameters(RSAKeyInfo);
                return RSA.Decrypt(DataToDecrypt, RSAEncryptionPadding.Pkcs1);
            }
            catch (Exception ex)
            {
                showInfo.ShowMessage($"The decryption RSA failed - {ex}", 3);
                throw new Exception(ex.Message);
            }
        }

        public string Decrypt_Str(byte[] DataToDecrypt, RSAParameters RSAKeyInfo)
        {
            try
            {
                RSA RSA = RSA.Create();
                RSA.ImportParameters(RSAKeyInfo);
                byte[] message = RSA.Decrypt(DataToDecrypt, RSAEncryptionPadding.Pkcs1);
                return Encoding.Unicode.GetString(message);
            }
            catch (Exception)
            {
                throw new Exception("Было принято сообщение, но его не удалось расшифровать");
            }
            //catch (Exception ex)
            //{
            //    showInfo.ShowMessage($"The decryption string AES failed - {ex}", 3);
            //    throw new Exception(ex.Message);
            //}
        }

    }
}