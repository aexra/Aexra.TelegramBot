using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Aexra.TelegramBot.Core.Helpers;
using Aexra.TelegramBot.Core.Attributes;
using Aexra.TelegramBot.Core.Extensions;
using Aexra.TelegramBot.Core.Types;
using System.Text.RegularExpressions;

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

    private ICollection<Type> _contexts;
    private IEnumerable<SlashCommand> _commands;

    /// <summary>
    /// Framework uses token received from BotFather
    /// </summary>
    /// <param name="token"></param>
    public Bot(string token)
    {
        _token = token;
    }

    /// <summary>
    /// Configures bot options (only receiverOptions for now)
    /// </summary>
    /// <param name="receiverOptions"></param>
    /// <returns></returns>
    public Bot Configure(Action<ReceiverOptions> receiverOptions)
    {
        receiverOptions.Invoke(_receiverOptions);
        return this;
    }

    /// <summary>
    /// Scans application assembly to find all classes with ApiContext attribute and its Commands
    /// </summary>
    /// <returns></returns>
    public Bot AddMessageContexts()
    {
        FindApiContexts();
        FindApiCommands();

        ValidateCommands();

        return this; 
    }

    /// <summary>
    /// Runs bot
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Find all ApiContexts and save them to list
    /// </summary>
    private void FindApiContexts()
    {
        _contexts = new List<Type>();

        var types = TypeHelper.GetTypesWith<ApiContextAttribute>(false);

        foreach (var contextType in types)
        {
            _contexts.Add(contextType);
        }

        Console.WriteLine($"Detected {types.Count()} context types: {string.Join(", ", types.Select(t => t.Name))}");
    }

    /// <summary>
    /// Find all Commands and save them to list
    /// </summary>
    private void FindApiCommands()
    {
        _commands = FindSlashCommands();
    }

    /// <summary>
    /// Validates all commands name. Throws Exception if command name is not match [a-z0-9]* or found more than once
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void ValidateCommands()
    {
        var regex = new Regex("[a-z0-9]*");
        foreach (var command in _commands)
        {
            if (!regex.IsMatch(command.Command))
            {
                throw new Exception($"Invalid command name \"{command.Command}\"");
            }
        }

        if (_commands.Select(c => c.Command).ToHashSet().Count != _commands.Count())
        {
            throw new Exception($"Found two or more commands with the same name");
        }

        Console.WriteLine($"Detected commands ({_commands.Count()}):\n{string.Join("\n", _commands.Select(c => "/" + c.Command))}");
    }

    /// <summary>
    /// Scans ApiContexts for SlashCommands and returns them
    /// </summary>
    /// <returns></returns>
    private IEnumerable<SlashCommand> FindSlashCommands()
    {
        var commands = new List<SlashCommand>();

        foreach (var contextType in _contexts)
        {
            foreach (var method in contextType.GetMethodsWith<SlashCommandAttribute>())
            {
                var slashAttr = method.GetCustomAttributes(typeof(SlashCommandAttribute), false).First() as SlashCommandAttribute;
                commands.Add(new SlashCommand() { Command = slashAttr.Command, Description = slashAttr.Description, Method = method });
            }
        }

        return commands;
    }

    /// <summary>
    /// Sends info about bot commands to Telegram to help autofill them in chats
    /// </summary>
    /// <returns></returns>
    private async Task SyncCommandsAsync()
    {
        await _botClient.SetMyCommandsAsync(_commands.Select(c => new BotCommand() { Command = c.Command, Description = c.Description }));
    }

    /// <summary>
    /// Telegram.Bot update handler 
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// MessageReceived event handler that makes all the context instantiation and command call work
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task MessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Find context with the right method
        foreach (var contextType in _contexts)
        {
            var methods = contextType.GetMethodsWith<SlashCommandAttribute>();
            foreach (var method in methods) 
            {
                var slashAttr = method.GetCustomAttributes(typeof(SlashCommandAttribute), false).First() as SlashCommandAttribute;
                if (update.Message.Text.StartsWith("/" + slashAttr.Command))
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

    /// <summary>
    /// Telegram.Bot error handler 
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
