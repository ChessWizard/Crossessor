using Crossessor.API.Domain.Enums;

namespace Crossessor.API.Application.Utilities.Helpers;

public static class ModelHelper
{
    public static string GetModelByType(this ModelType modelType)
    {
        Dictionary<ModelType, string> modelTypeMapping = new()
        {
            { ModelType.GPT_4o, "gpt-4o" },
            { ModelType.Gemini_1_5_Flash, "gemini-1.5-flash" }
        };
        
        return modelTypeMapping.TryGetValue(modelType, out string model) ? model : string.Empty;
    }
}