using System.Text;

namespace LabsQueueBot.Core.Validators;

public static class UserInfoValidator
{
    public static string? ValidateName(string? name)
    {
        var builder = new StringBuilder();

        if (name == null)
        {
            builder.AppendLine("Фамилия и имя не могут быть null");
        }
        else
        {
            var splitName = name.Split(' ');
            if (splitName.Length != 2)
            {
                builder.AppendLine("Фамилия и имя должны быть двумя отдельными словами");
            }
            else
            {
                if (splitName[0].Trim().Length < 2)
                {
                    builder.AppendLine("Фамилия должна содержать как минимум две буквы");
                }

                if (splitName[1].Trim().Length < 2)
                {
                    builder.AppendLine("Имя должно содержать как минимум две буквы");
                }
                
                if (name.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
                {
                    builder.AppendLine("Имя и фамилия не должны содержать цифр и специальных символов");
                }
            }
        }

        return builder.Length > 0
            ? builder.ToString()
            : null;
    }

    public static string? ValidateUsername(string? username)
    {
        var builder = new StringBuilder();

        if (username == null)
        {
            builder.AppendLine("Имя пользователя не может быть null");
        }
        else
        {
            if (!username.StartsWith('@'))
            {
                builder.AppendLine("Имя пользователя должно начинаться с символа @");
            }

            if (username.Length < 6)
            {
                builder.AppendLine("Минимальная длина имени пользователя - 6 символов с учетом @");
            }

            if (username.Skip(1).Any(c => !char.IsBetween(c, 'A', 'z') && !char.IsDigit(c) && c != '_'))
            {
                builder.AppendLine("Имя пользователя может содержать только символы A-z, 0-9 и _");
            }
        }

        return builder.Length > 0
            ? builder.ToString()
            : null;
    }
}