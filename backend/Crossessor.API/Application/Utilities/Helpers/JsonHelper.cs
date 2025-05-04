using System.Text.RegularExpressions;

namespace Crossessor.API.Application.Utilities.Helpers;

public static class JsonHelper
{
    public static string ExtractPromptResponseJson(this string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";

        var match = Regex.Match(raw, @"\{[\s\S]*?\}");
        return match.Success ? match.Value : "{}";
    }
}