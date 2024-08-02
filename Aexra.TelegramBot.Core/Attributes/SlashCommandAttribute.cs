namespace Aexra.TelegramBot.Core.Attributes;

/// <summary>
/// Attribute applicable to all SlashCommand in ApiContext
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SlashCommandAttribute : Attribute
{
    /// <summary>
    /// Command name
    /// </summary>
    public string Command { get; set; } = default!;

    /// <summary>
    /// Command description
    /// </summary>
    public string Description { get; set; } = default!;

    public SlashCommandAttribute(string name = "")
    {
        Command = name;
    }

    public SlashCommandAttribute(string name, string description)
    {
        Command = name;
        Description = description;
    }
}
