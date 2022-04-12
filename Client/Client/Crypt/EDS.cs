using System;
using System.Security.Cryptography;
using System.Text;

namespace Client.Crypt
{
    class EDS
    {
        static RSAParameters pubkey = new RSAParameters
        {
            Exponent = new byte[] { 1, 0, 1 },

            Modulus = Convert.FromBase64String("sLGkM/VuzOG6blcKFn+bM/gzW7zgtKoHknh676I11xcJQK7d3xITkfcoNYyLqmjpIvizb6/sf/tV9BYaFAa64FDlzD40d2jj5XvViHi8Bleqh9enSDIwU/qRpvpx5/DaDJFun" +
                    "DoAbfYt3Xp4Smd8WFGFw4z9KZ/5q1uO96PkWy8NiiyZQcUqCvyqMuiPxJi2NilflIgWvk9DbHqfZBeSKgJk33d80tmHU+asqkbEh/VM55ZJPCpRHBesdPrt2Da6BM8R//y+qhbxXnP7c39A3piBvjuIagWseansIEJxng3eO" +
                    "u8XqSHAzGEA0TfVW35Pl5k5oYF0DeXjJBC/xiyRvQ==")
        };

        static public bool Check_byte(byte[] data, byte[] signedHash)
        {
            try
            {
                

                RSA rsa = RSA.Create(pubkey);

                byte[] hash;
                using (SHA256 sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(data);
                }

                RSAPKCS1SignatureDeformatter RSADeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                RSADeformatter.SetHashAlgorithm("SHA256");

                if (RSADeformatter.VerifySignature(hash, signedHash))
                    return true; // Подпись верифицирована
                else
                    return false; // Подпись не верифицирована

            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Данные не были проверены!");
                throw new CryptographicException(ex.Message);
            }
        }

        static public bool Check_str(string data, string EDS)
        {
            try
            {
                byte[] signedHash = Convert.FromBase64String(EDS);

                RSA rsa = RSA.Create(pubkey);

                byte[] hash;
                using (SHA256 sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(Encoding.Default.GetBytes(data));
                }

                RSAPKCS1SignatureDeformatter RSADeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                RSADeformatter.SetHashAlgorithm("SHA256");

                if (RSADeformatter.VerifySignature(hash, signedHash))
                    return true; // Подпись верифицирована
                else
                    return false; // Подпись не верифицирована

            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Данные не были проверены!");
                throw new CryptographicException(ex.Message);
            }
        }
    }
}
