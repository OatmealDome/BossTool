using System;

namespace BossTool
{
    public static class ByteUtil
    {
        public static byte[] ByteArrayFromString(string s)
        {
            if (s.Length % 2 != 0)
            {
                throw new Exception("Invalid length");
            }

            byte[] array = new byte[s.Length / 2];

            for (int i = 0; i < s.Length / 2; i++)
            {
                array[i] = Convert.ToByte(s.Substring(2 * i, 2), 16);
            }

            return array;
        }
    }
}
