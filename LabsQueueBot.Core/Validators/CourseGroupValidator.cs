using System.Text;

namespace LabsQueueBot.Core.Validators;

public static class CourseGroupValidator
{
    public static string? ValidateFormatted(string? rawCourseGroup)
        => InternalValidate(rawCourseGroup, " ", 4, 2);
    
    public static string? Validate(string? rawCourseGroup)
        => InternalValidate(rawCourseGroup, ":", 2, 1);

    public static string? Validate(byte course, byte group)
    {
        var builder = new StringBuilder();
        
        if (course is < 1 or > 6)
        {
            builder.AppendLine("Некорректный номер курса");
        }

        if (group is < 1 or > 99)
        {
            builder.AppendLine("Некорректный номер группы");
        }
        
        return builder.Length > 0
            ? builder.ToString()
            : null;
    }
    
    private static string? InternalValidate(string? rawCourseGroup, string parser, int partsCount, int step)
    {
        if (rawCourseGroup is null)
        {
            return "Номер курса и группы не может быть null";
        }
        
        var builder = new StringBuilder();

        var parsed = rawCourseGroup.Split(parser);
        if (parsed.Length == partsCount)
        {
            if (!byte.TryParse(parsed[0], out var course) || course is < 1 or > 6)
            {
                builder.AppendLine("Некорректный номер курса");
            }
            if (!byte.TryParse(parsed[step], out var group) || group is < 1 or > 99)
            {
                builder.AppendLine("Некорректный номер группы");
            }
        }
        else
        {
            builder.AppendLine("Некорректный формат");
        }
        
        return builder.Length > 0
            ? builder.ToString()
            : null;
    }
}