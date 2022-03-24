using System;
using System.Text;
using Server.Crypt;

namespace Server.Helpers
{
    public class HashManager
    {
        private static string salt2 = "NsoYaS206NmJHfFkUYLkQsfJHAfA8216ixv";

        public static string GenerateHash(string password, byte[] salt1)
        {
            byte[] round1 = SHA256Hash.GenerateSaltedHash(Encoding.Unicode.GetBytes(password), salt1);
            byte[] round2 = SHA256Hash.GenerateSaltedHash(round1, Encoding.Unicode.GetBytes(salt2));
            string result = Convert.ToBase64String(round2);

            return result;
        }

        public static bool Access(string password, byte[] salt1, string hashpassword)
        {
            byte[] round1 = SHA256Hash.GenerateSaltedHash(Encoding.Unicode.GetBytes(password), salt1);
            byte[] round2 = SHA256Hash.GenerateSaltedHash(round1, Encoding.Unicode.GetBytes(salt2));
            byte[] hashbyte = Convert.FromBase64String(hashpassword);

            return SHA256Hash.CompareByteArrays(hashbyte, round2);
        }
    }
}