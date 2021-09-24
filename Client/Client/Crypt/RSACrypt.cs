using System;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Client.Crypt
{
    public class RSACrypt
    {
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
                MessageBox.Show($"The encryption RSA failed - {ex}");
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
                MessageBox.Show($"The decryption RSA failed - {ex}");
                throw new Exception(ex.Message);
            }
        }
    }
}