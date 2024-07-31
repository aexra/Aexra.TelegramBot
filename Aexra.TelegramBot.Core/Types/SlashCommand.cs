namespace Aexra.TelegramBot.Core.Types;
internal class SlashCommand
{
    public string Name { get; set; }
    public string Description { get; set; }
    public System.Reflection.MethodInfo Command { get; set; }
}
