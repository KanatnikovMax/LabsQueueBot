using System.Text;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LabsQueueBot;

public class Help : Command
{
    public override string Definition => "/help - Список команд";

    public override InlineKeyboardMarkup? GetKeyboard(Update update)
    {
        return null;
    }

    public override SendMessageRequest Run(Update update)
    {
        long id;
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            id = update.Message.Chat.Id;
        else
            id = update.CallbackQuery.Message.Chat.Id;

        var builder = new StringBuilder();

        builder.AppendLine("При добавлении в очередь пользователь записывается в список ожидающих. "
                           + "Каждый день в 19:00 список ожидающих случайным образом перемешивается и добавляется в конец"
                           + " соответствующей очереди, тем, кто подписан на рассылку, приходит уведомление с его местами в очередях, "
                           + "в которые он записан. Пользователи с админскими правами соответствующей командой "
                           + "могут вызвать генерацию очередей для своей группы в любой момент времени. "
                           + "Для получения админских прав староста группы (или другое ответственное лицо) должен написать админу "
                           + "бота в лс (ссылка на профиль админа в описании)\n");

        // описание команд
        builder.AppendLine($"\n{new Help().Definition}");

        builder.AppendLine($"\n{new SwitchNotification().Definition}");

        builder.AppendLine("\nДействия с очередями");
        builder.AppendLine(new Subjects().Definition);
        builder.AppendLine(new ShowQueue().Definition);
        builder.AppendLine(new Join().Definition);
        builder.AppendLine(new Quit().Definition);
        builder.AppendLine(new Skip().Definition);

        builder.AppendLine("\nИзменить информацию о себе");
        builder.AppendLine(new Rename().Definition);
        builder.AppendLine(new SetGroup().Definition);

        builder.AppendLine($"\n{new Stop().Definition}");

        return new SendMessageRequest(id, builder.ToString());
    }
}