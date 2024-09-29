using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LabsQueueBot
{
    public static class PasswordGenerator
    {
        public static string Password { get; private set; }
        public static void Generate(int length)
        {
            var symbols = "zxcvbnmlkjhgfdsaqwertyuiopZXCVBNMLKJHGFDSAQWERTYUIOP1234567890";
            var result = new StringBuilder();
            for (int i = 0; i < length; ++i)
                result.Append(symbols[RandomNumberGenerator.GetInt32(0, symbols.Length)]);
            Password = result.ToString();
        }
    }
}
