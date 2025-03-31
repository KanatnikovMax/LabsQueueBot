using LabsQueueBot.Bot;
using LabsQueueBot.Db.Entities;
using LabsQueueBot.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using User = LabsQueueBot.Db.Entities.User;

namespace LabsQueueBot.Commands.Implements;

public class ShowCommand(
    ILogger logger,
    IRepository<User> usersRepository,
    IRepository<Subject> subjectsRepository,
    IRepository<SerialNumber> serialNumberRepository) : ICommand
{
    public string Name => "/show";

    public User.UserState State => User.UserState.ShowQueue;
    public UserRule.Rule AcceptUserUserRule => UserRule.Rule.Use;

    public string Definition => "";

    public async Task Execute(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var id = update.Message.Chat.Id;
        var user = await usersRepository.GetByIdAsync(id, cancellationToken);

        switch (user.State)
        {
            case User.UserState.None:
            {
                await SendSubjectsKeyboard(user, botClient, cancellationToken);
                break;
            }
            case User.UserState.ShowQueue:
            {
                break;
            }
            default:
                throw new Exception(); //TODO добавить разные ексепшены
        }
    }

    public bool Contains(string command)
    {
        return Name.Equals(command);
    }

    private async Task SendSubjectsKeyboard(User user, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        var subjects = (await subjectsRepository.GetAllAsync(s =>
                    s.CourseNumber == user.CourseNumber && s.GroupNumber == user.GroupNumber,
                cancellationToken))
            .Select(s => s.SubjectName).ToList();
        var keyboard = InlineKeyboardCreator.ListToKeyboard(subjects, false, true, 1);

        user.State = User.UserState.ShowQueue;
        await usersRepository.SaveAsync(user, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: user.Id,
            text: "Выберите предмет:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task SendQueueList(User user, ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var id = update.CallbackQuery.Message.Chat.Id;

        await botClient.DeleteMessageAsync(
            chatId: id,
            messageId: update.Message.MessageId,
            cancellationToken: cancellationToken);
        
        if (update.CallbackQuery.Message.Text != "Выберите предмет:")
        {
            await botClient.SendTextMessageAsync(
                chatId: id,
                text: "",
                cancellationToken: cancellationToken);
            return;
        }

        var subjectName = update.CallbackQuery.Data;
        
        switch (subjectName)
        {
            case "Назад":
            {
                user.State = User.UserState.None;
                await usersRepository.SaveAsync(user, cancellationToken);
                break;
            }
            case "Добавить":
            {
                user.State = User.UserState.AddSubject; 
                
                await usersRepository.SaveAsync(user, cancellationToken);;
                await botClient.SendTextMessageAsync(
                    chatId: id,
                    text: "Введите название дисциплины, которую хотите добавить",
                    cancellationToken: cancellationToken);
                break; 
            }
            default:
            {
                var subject = (await subjectsRepository.GetAllAsync(s =>
                        s.CourseNumber == user.CourseNumber
                        && s.GroupNumber == user.GroupNumber
                        && s.SubjectName == subjectName,
                    cancellationToken
                )).FirstOrDefault();

                if (subject is null)
                {
                    user.State = User.UserState.None;
                    await usersRepository.SaveAsync(user, cancellationToken);
                    await botClient.SendTextMessageAsync(
                        chatId: id,
                        text: "Такой дисциплины не существует.",
                        cancellationToken: cancellationToken);
                    break;
                }
                
                var queue = (await serialNumberRepository.GetAllAsync(sn =>
                    sn.SubjectId == subject.Id)).Where(); 
                
                break;
            }
        }

        
        
        
        


        //добавление пользователем новой дисциплины
        

        return new SendMessageRequest(id, Groups.ShowQueue(id, subjectName));
    }
    
    private async Task 
}