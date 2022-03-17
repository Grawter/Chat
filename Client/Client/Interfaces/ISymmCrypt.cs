using System.Threading.Tasks;

namespace Client.Interfaces
{
    interface ISymmCrypt
    {
        Task<byte[]> Encrypt(string message, byte[] key);

        Task<string> Decrypt(byte[] enc_mess, byte[] key);
    }
}