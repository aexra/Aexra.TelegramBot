# [Aexra.TelegramBot](https://www.nuget.org/packages/Aexra.TelegramBot/)

This is a simple framework written around [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot).

## 🔨 Getting started

All you need here is an empty .NET Console Project.

### Configuration

In ```Program.cs``` copy the following script

```cs
using Aexra.TelegramBot;

var bot = new Bot("YOUR_BOT_TOKEN");

await bot.Run();
```

The bot will run, but will not do anything.

Firstly, you can Configure your bot with ReceiverOptions like so, for example:

```cs
var bot = new Bot("YOUR_BOT_TOKEN")
  .Configure(options =>
  {
      options.ThrowPendingUpdates = false;
  });
```

### Context style

This framework uses Context style for implementing commands.

Every class that inherit ```MessageContextBase``` and has attribute ```ApiContext``` is considered a command context.

```cs
[ApiContext]
internal class EchoMessageContext : MessageContextBase
{
    
}
```

> [!IMPORTANT]
> ```MessageContextBase``` has following fields:
> ```cs
> public ITelegramBotClient BotClient { get; }
> public CancellationToken CancellationToken { get; }
> public Message Message { get; }
> ```

Here you can add Slash Commands like so:

```cs
[ApiContext]
internal class EchoMessageContext : MessageContextBase
{
    [SlashCommand("echo", "Returns user input")]
    public async Task Echo()
    {
        await BotClient.SendTextMessageAsync(Message.Chat.Id, Message.Text);
    }
}
```

> [!NOTE]
> Every command must have a unique name and fit regex ```[a-z0-9]*```

You must call AddMessageContexts method on bot to use them:

```
var bot = new Bot("YOUR_BOT_TOKEN")
    .AddMessageContexts();
```

### Sample

Sample bot configure looks like this:

```cs
var bot = new Bot("YOUR_BOT_TOKEN")
    .Configure(options =>
    {
        options.ThrowPendingUpdates = false;
    })
    .AddMessageContexts();
```
