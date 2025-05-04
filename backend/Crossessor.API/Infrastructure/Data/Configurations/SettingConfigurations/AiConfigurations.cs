namespace Crossessor.API.Infrastructure.Data.Configurations.SettingConfigurations;

public class AiConfigurations
{
    public OpenAi OpenAi { get; set; } = default!;
    public Gemini Gemini { get; set; } = default!;
}

public class OpenAi
{
    public string Key { get; set; } = default!;
}

public class Gemini
{
    public string Key { get; set; } = default!;
}
