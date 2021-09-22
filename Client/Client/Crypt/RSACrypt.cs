using System.Security.Cryptography;

namespace Client.Crypt
{
    public class RSACrypt
    {
        static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo)
        {
            RSA RSA = RSA.Create();

            RSA.ImportParameters(RSAKeyInfo);

            return RSA.Encrypt(DataToEncrypt, RSAEncryptionPadding.Pkcs1);
        }

        static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo)
        {
            RSA RSA = RSA.Create();

            RSA.ImportParameters(RSAKeyInfo);

            return RSA.Decrypt(DataToDecrypt, RSAEncryptionPadding.Pkcs1);
        }
    }
}