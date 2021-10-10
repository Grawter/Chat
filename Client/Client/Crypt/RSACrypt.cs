using Client.Helpers;
using Client.Interfaces;
using System;
using System.Security.Cryptography;

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
    }
}