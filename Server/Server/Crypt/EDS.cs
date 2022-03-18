using System;
using System.Security.Cryptography;
using System.Text;

namespace Server.Crypt
{
    class EDS
    {
        static public byte[] Sign_byte(byte[] data)
        {
            try
            {
                RSAParameters privkey = new RSAParameters
                {
                    Exponent = new byte[] { 1, 0, 1 },

                    Modulus = Convert.FromBase64String("sLGkM/VuzOG6blcKFn+bM/gzW7zgtKoHknh676I11xcJQK7d3xITkfcoNYyLqmjpIvizb6/sf/tV9BYaFAa64FDlzD40d2jj5XvViHi8Bleqh9enSDIwU/qRpvpx5/DaDJFun" +
                    "DoAbfYt3Xp4Smd8WFGFw4z9KZ/5q1uO96PkWy8NiiyZQcUqCvyqMuiPxJi2NilflIgWvk9DbHqfZBeSKgJk33d80tmHU+asqkbEh/VM55ZJPCpRHBesdPrt2Da6BM8R//y+qhbxXnP7c39A3piBvjuIagWseansIEJxng3eO" +
                    "u8XqSHAzGEA0TfVW35Pl5k5oYF0DeXjJBC/xiyRvQ=="),

                    D = Convert.FromBase64String("mIPtYXATgMPDAWUd7wecdwJhimkBaNBvEs3uB5RzdsKKmQuXtenLfK3jRj2XfLNrsWUMtXYb5OiUa8j8H5cjjWYmxOJYtU2TkU0PpunYGn1nVDQgT6AKuSJKwN/inDr4a+rImA80ubX" +
                    "KJyeEV2z2FuHfXUkAaQKOJow0XqtX/B+1AtIeVbfAUxaw5ecnaOHf206mewGhpQrk9+Nhj+yD7iXUAE3g/nVFVsCMtECKOq/twAv9c8oK5CATxfpYAkGgq9eKlTIXHmhIZ/tjF0kdLWTUeB8XJqUf39dXea80U13oyu4thdg" +
                    "z0oy+Ip6AxbNswyrmtAR5jGxRDQJhKBzZHQ=="),

                    P = Convert.FromBase64String("z6918TIa4cq7979tHuAOwIUA23k71642wMjd+DAr2tu6EtQVNbWE/Q+J+NkXV6QNdRKPWtJax+Xzu6362Y6sICvsn44wGI3AMU5JeTj2XY9LjHk5IbhCQ2nsCp9ODVwEPVl0HhWSfE1muLsY+7SPhCV7SJMIEmt/0Sw/Q2oF9iM="),

                    Q = Convert.FromBase64String("2cyB7gelS4TitUgGKsnzW8tZaPEiPPpl98I3R/V5SUe2cACYhLj0YmOBukDSiKqajQ7sG1IBLVN4TxQPIFzrx95uihUGUwMfCveW20uirSWctxXraSAxKMAiND4K+BMW7ZHZyj6hvxiT7AO56IkM5VjEVXaG5XYKd+3toiCKpp8="),

                    DP = Convert.FromBase64String("BstWsG1TM8/OFmzMxRXPZbz0KjntG0E3yVbI7DWnOaG2lKcbioZFs4GuodV4TgWv6wSQbAg1sBR1xM4MRtHED8lgFFgyepsojnRhMvYKGUV200Gj/NZqVIpCjpZnl0hLWzZx9gn+oNJnaeO1DYb6qvgO8HDe8rtRkyY/a5UsXME="),

                    DQ = Convert.FromBase64String("uH4nFdX+LCGN1VgEeVyGvBxeSanxgcvuclhfcaI7slp/7RJkIBF4WpvF+VsNvF0f/BtG38GwY4/QWtQ4GAb+Z1lBM9CpgcokEqs2a97/F0LEJMb5mhx95/IRR0XDDu9EvcuNvGcfpnuQWh2x7ouhk+mEroxvqpI6PesbH/oG2Ss="),

                    InverseQ = Convert.FromBase64String("AhUKfR8rPRdQkrRKf9gbOs7cUzBpJ6LVzDgTOtA/5KPyHoFhJqsMBWXKejnc4kzo/ej5z9DOoKFH9MKvAnNxZkt3MA0SEXvKP9UFl2Vrvjhgh4ZhpEg49wiIlmOZrV8x5Ie7OxkpJO8ZJzprkivqJA7Wbjxt5Huaac5Z2OvJ2Hs="),
                };

                RSA rsa = RSA.Create(privkey);

                byte[] hash;
                using (HashAlgorithm sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(data);
                }

                RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(rsa);

                RSAFormatter.SetHashAlgorithm("SHA256");

                byte[] signedHash = RSAFormatter.CreateSignature(hash);

                return signedHash;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Данные не были подписаны!");
                throw new CryptographicException(ex.Message);
            }
        }

        static public string Sign_str(string data)
        {
            try
            {
                RSAParameters privkey = new RSAParameters { 
                    Exponent = new byte[] { 1, 0, 1 },
                    
                    Modulus = Convert.FromBase64String("sLGkM/VuzOG6blcKFn+bM/gzW7zgtKoHknh676I11xcJQK7d3xITkfcoNYyLqmjpIvizb6/sf/tV9BYaFAa64FDlzD40d2jj5XvViHi8Bleqh9enSDIwU/qRpvpx5/DaDJFun" +
                    "DoAbfYt3Xp4Smd8WFGFw4z9KZ/5q1uO96PkWy8NiiyZQcUqCvyqMuiPxJi2NilflIgWvk9DbHqfZBeSKgJk33d80tmHU+asqkbEh/VM55ZJPCpRHBesdPrt2Da6BM8R//y+qhbxXnP7c39A3piBvjuIagWseansIEJxng3eO" +
                    "u8XqSHAzGEA0TfVW35Pl5k5oYF0DeXjJBC/xiyRvQ=="),
                    
                    D = Convert.FromBase64String("mIPtYXATgMPDAWUd7wecdwJhimkBaNBvEs3uB5RzdsKKmQuXtenLfK3jRj2XfLNrsWUMtXYb5OiUa8j8H5cjjWYmxOJYtU2TkU0PpunYGn1nVDQgT6AKuSJKwN/inDr4a+rImA80ubX" +
                    "KJyeEV2z2FuHfXUkAaQKOJow0XqtX/B+1AtIeVbfAUxaw5ecnaOHf206mewGhpQrk9+Nhj+yD7iXUAE3g/nVFVsCMtECKOq/twAv9c8oK5CATxfpYAkGgq9eKlTIXHmhIZ/tjF0kdLWTUeB8XJqUf39dXea80U13oyu4thdg" +
                    "z0oy+Ip6AxbNswyrmtAR5jGxRDQJhKBzZHQ=="),
                    
                    P = Convert.FromBase64String("z6918TIa4cq7979tHuAOwIUA23k71642wMjd+DAr2tu6EtQVNbWE/Q+J+NkXV6QNdRKPWtJax+Xzu6362Y6sICvsn44wGI3AMU5JeTj2XY9LjHk5IbhCQ2nsCp9ODVwEPVl0HhWSfE1muLsY+7SPhCV7SJMIEmt/0Sw/Q2oF9iM="),
                    
                    Q = Convert.FromBase64String("2cyB7gelS4TitUgGKsnzW8tZaPEiPPpl98I3R/V5SUe2cACYhLj0YmOBukDSiKqajQ7sG1IBLVN4TxQPIFzrx95uihUGUwMfCveW20uirSWctxXraSAxKMAiND4K+BMW7ZHZyj6hvxiT7AO56IkM5VjEVXaG5XYKd+3toiCKpp8="),
                    
                    DP = Convert.FromBase64String("BstWsG1TM8/OFmzMxRXPZbz0KjntG0E3yVbI7DWnOaG2lKcbioZFs4GuodV4TgWv6wSQbAg1sBR1xM4MRtHED8lgFFgyepsojnRhMvYKGUV200Gj/NZqVIpCjpZnl0hLWzZx9gn+oNJnaeO1DYb6qvgO8HDe8rtRkyY/a5UsXME="),
                    
                    DQ = Convert.FromBase64String("uH4nFdX+LCGN1VgEeVyGvBxeSanxgcvuclhfcaI7slp/7RJkIBF4WpvF+VsNvF0f/BtG38GwY4/QWtQ4GAb+Z1lBM9CpgcokEqs2a97/F0LEJMb5mhx95/IRR0XDDu9EvcuNvGcfpnuQWh2x7ouhk+mEroxvqpI6PesbH/oG2Ss="),
                    
                    InverseQ = Convert.FromBase64String("AhUKfR8rPRdQkrRKf9gbOs7cUzBpJ6LVzDgTOtA/5KPyHoFhJqsMBWXKejnc4kzo/ej5z9DOoKFH9MKvAnNxZkt3MA0SEXvKP9UFl2Vrvjhgh4ZhpEg49wiIlmOZrV8x5Ie7OxkpJO8ZJzprkivqJA7Wbjxt5Huaac5Z2OvJ2Hs="),
                };

                RSA rsa = RSA.Create(privkey);
                
                byte[] hash;
                using (HashAlgorithm sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(Encoding.Default.GetBytes(data));
                }

                RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(rsa);

                RSAFormatter.SetHashAlgorithm("SHA256");

                byte[] signedHash = RSAFormatter.CreateSignature(hash);

                return Convert.ToBase64String(signedHash);
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Данные не были подписаны!");
                throw new CryptographicException(ex.Message);          
            }
        }        
    }
}
