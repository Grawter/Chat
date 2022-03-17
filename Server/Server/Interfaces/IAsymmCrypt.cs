using System.Security.Cryptography;

namespace Server.Interfaces
{
    interface IAsymmCrypt
    {
        byte[] Encrypt(byte[] DataToEncrypt, RSAParameters AsimmKeyInfo);
        byte[] Decrypt(byte[] DataToDecrypt, RSAParameters AsimmKeyInfo);

    }
}