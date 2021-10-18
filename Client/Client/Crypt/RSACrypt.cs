using System;
using System.Text;
using System.Security.Cryptography;
using Client.Interfaces;
using Client.Helpers;

namespace Client.Crypt
{
    public class RSACrypt
    {
        static IShowInfo showInfo = new ShowInfo();

        static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo)
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

        static public byte[] RSAEncrypt_Str(string Text, RSAParameters RSAKeyInfo)
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
                throw new Exception("Было принято сообщение, но его удалось не расшифровать");
            }
            //catch (Exception ex)
            //{
            //    showInfo.ShowMessage($"The decryption string AES failed - {ex}", 3);
            //    throw new Exception(ex.Message);
            //}
        }

        static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo)
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

        static public string RSADecrypt_Str(byte[] DataToDecrypt, RSAParameters RSAKeyInfo)
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
                throw new Exception("Было принято сообщение, но его удалось не расшифровать");
            }
            //catch (Exception ex)
            //{
            //    showInfo.ShowMessage($"The decryption string AES failed - {ex}", 3);
            //    throw new Exception(ex.Message);
            //}
        }

    }
}