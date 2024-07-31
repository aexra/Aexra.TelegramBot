namespace Aexra.TelegramBot.Core.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class SlashCommandAttribute : Attribute
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

    public SlashCommandAttribute(string name = "")
    {
        Name = name;
    }

    public SlashCommandAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
