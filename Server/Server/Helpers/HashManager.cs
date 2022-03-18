using System;
using System.Text;
using Server.Crypt;

namespace Server.Helpers
{
    public class HashManager
    {
        private static string sec_word = "NsoYaS206NmJHfFkUYLkQsfJHAfA8216ixv";

        public static string GenerateHash(string password, byte[] salt)
        {
            byte[] round1 = SHA256Hash.GenerateSaltedHash(Encoding.Unicode.GetBytes(sec_word), salt);
            byte[] round2 = SHA256Hash.GenerateSaltedHash(Encoding.Unicode.GetBytes(password), round1);
            string result = Convert.ToBase64String(round2);

            return result;
        }

        public static bool Access(string password, byte[] salt, string hashpassword)
        {
            byte[] round1 = SHA256Hash.GenerateSaltedHash(Encoding.Unicode.GetBytes(sec_word), salt);
            byte[] round2 = SHA256Hash.GenerateSaltedHash(Encoding.Unicode.GetBytes(password), round1);
            byte[] hashbyte = Convert.FromBase64String(hashpassword);

            return SHA256Hash.CompareByteArrays(hashbyte, round2);
        }
    }
}