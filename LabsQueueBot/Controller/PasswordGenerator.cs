using System.Security.Cryptography;
using System.Text;

namespace LabsQueueBot
{
    /// <summary>
    /// Генератор случайной последовательности символов; <br/>
    /// хранит в себе сгенерированную последовательность
    /// </summary>
    public static class PasswordGenerator
    {
        public static string Password { get; private set; }
        /// <summary>
        /// Генерирует случайную последовательности символов
        /// </summary>
        /// <param name="length"> необходимая длина последовательности </param>
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
