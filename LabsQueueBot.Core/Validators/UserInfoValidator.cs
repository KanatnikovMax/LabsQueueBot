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
            if (name.Split(' ').Length != 2)
            {
                builder.AppendLine("Фамилия и имя должны быть двумя отдельными словами");
            }
            else
            {
                if (name.Split(' ')[0].Trim().Length < 2)
                {
                    builder.AppendLine("Фамилия должна содержать как минимум две буквы");
                }

                if (name.Split(' ')[1].Trim().Length < 2)
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
}