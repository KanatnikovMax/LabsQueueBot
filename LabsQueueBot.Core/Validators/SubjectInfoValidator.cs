namespace LabsQueueBot.Core.Validators;

public static class SubjectInfoValidator
{
    public static string? ValidateSubjectName(string? subjectName)
    {
        if (string.IsNullOrWhiteSpace(subjectName))
        {
            return "Название предмета не может быть пустым";
        }

        if (subjectName.Trim().Length > 100)
        {
            return "Название предмета не может содержать больше 100 символов";
        }

        return null;
    }
}