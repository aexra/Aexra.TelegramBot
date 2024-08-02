namespace Aexra.TelegramBot.Core.Types;

/// <summary>
/// SlashCommand class for framework usage
/// </summary>
public class SlashCommand
{
    public string Command { get; set; }
    public string Description { get; set; }
    public System.Reflection.MethodInfo Method { get; set; }
}
