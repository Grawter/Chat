

namespace Client.Helpers.KeySet
{
    public class ByteConvStr
    {
        static public string ByteToStr(byte[] key)
        {
            string key_str = "";

            foreach (var item in key)
            {
                if (item < 10)
                    key_str += "00" + item;
                else if (item >= 10 && item < 100)
                    key_str += "0" + item;
                else
                    key_str += item;
            }

            return key_str;
        }

        static public byte[] StrToByte(string str)
        {
            if (str.Contains("."))
            {
                string[] str_mass = str.Split(".");

                byte[] key_mass = new byte[str_mass.Length];

                for (int i = 0; i < str_mass.Length; i++)
                {
                    key_mass[i] = byte.Parse(str_mass[i]);
                }

                return key_mass;
            }
            else
            {
                byte[] key_mass = new byte[str.Length / 3];

                for (int i = 0, j = 0; i < str.Length / 3; i++, j += 3)
                {
                    key_mass[i] = byte.Parse(str[j].ToString() + str[j + 1] + str[j + 2]);
                }

                return key_mass;
            }
            
        }
    }
}