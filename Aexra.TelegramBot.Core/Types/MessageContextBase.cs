﻿using Telegram.Bot;
using Telegram.Bot.Types;

namespace Aexra.TelegramBot.Core.Types;

/// <summary>
/// Base for all ApiContexts
/// </summary>
public abstract class MessageContextBase
{
    public ITelegramBotClient BotClient { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public Message Message { get; set; }
}
