using System.Security.Cryptography;

namespace Client.Interfaces
{
    interface IAsymmCrypt
    {
        byte[] Encrypt(byte[] DataToEncrypt, RSAParameters AsimmKeyInfo);

        byte[] Encrypt_Str(string Text, RSAParameters AsimmKeyInfo);

        byte[] Decrypt(byte[] DataToDecrypt, RSAParameters AsimmKeyInfo);

        string Decrypt_Str(byte[] DataToDecrypt, RSAParameters AsimmKeyInfo);

    }
}