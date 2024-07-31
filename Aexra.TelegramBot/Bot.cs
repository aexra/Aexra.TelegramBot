using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Aexra.TelegramBot.Core.Helpers;
using Aexra.TelegramBot.Core.Attributes;
using Aexra.TelegramBot.Core.Extensions;
using Aexra.TelegramBot.Core.Types;

namespace Aexra.TelegramBot;

public class Bot
{
    private ITelegramBotClient _botClient;
    private ReceiverOptions _receiverOptions = new ReceiverOptions
    {
        AllowedUpdates = [UpdateType.Message],
        ThrowPendingUpdates = true,
    };

    private string _token;

    private ICollection<Type> Contexts;

    public Bot(string token)
    {
        _token = token;
    }

    public Bot Configure(Action<ReceiverOptions> receiverOptions)
    {
        return this;
    }

    public Bot AddMessageContexts()
    {
        Contexts = new List<Type>();

        var types = TypeHelper.GetTypesWith<ApiContextAttribute>(false);

        foreach (var contextType in types)
        {
            Contexts.Add(contextType);
        }

        Console.WriteLine($"Detected {types.Count()} context types: {string.Join(", ", types.Select(t => t.Name))}");

        ValidateRoutes();

        return this; 
    }

    private void ValidateRoutes()
    {
        // Проверяем ошибки пересекающихся рутов

        
    }

    public async Task Run()
    {
        _botClient = new TelegramBotClient(_token);

        using var cts = new CancellationTokenSource();

        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

        await SyncCommandsAsync();

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"{me.FirstName} запущен!");

        await Task.Delay(-1);
    }

    private IEnumerable<BotCommand> FindBotCommands()
    {
        var commands = new List<BotCommand>();

        foreach (var contextType in Contexts)
        {
            foreach (var method in contextType.GetMethodsWith<SlashCommandAttribute>())
            {
                var slashAttr = method.GetCustomAttributes(typeof(SlashCommandAttribute), false).First() as SlashCommandAttribute;
                commands.Add(new BotCommand() { Command = slashAttr.Name, Description = slashAttr.Description });
            }
        }

        return commands;
    }

    private async Task SyncCommandsAsync()
    {
        var commands = FindBotCommands();
        await _botClient.SetMyCommandsAsync(commands);

        Console.WriteLine($"Detected commands ({commands.Count()}):\n{string.Join("\n", commands.Select(c => "/" + c.Command))}");
    }

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await MessageReceived(botClient, update, cancellationToken);
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private async Task MessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine("Пришло сообщение!");
        Console.WriteLine(update.Message?.Text);

        // Находим контекст с нужным методом
        foreach (var contextType in Contexts)
        {
            var methods = contextType.GetMethodsWith<SlashCommandAttribute>();
            foreach (var method in methods) 
            {
                var slashAttr = method.GetCustomAttributes(typeof(SlashCommandAttribute), false).First() as SlashCommandAttribute;
                if (update.Message.Text.StartsWith("/" + slashAttr.Name))
                {
                    var obj = (MessageContextBase)Activator.CreateInstance(contextType);
                    obj.BotClient = botClient;
                    obj.CancellationToken = cancellationToken;
                    obj.Message = update.Message;

                    method.Invoke(obj, null);

                    return;
                }
            }
        }
    }

    private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
