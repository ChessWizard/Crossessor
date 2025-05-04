using Crossessor.API.Application.Enums;

namespace Crossessor.API.Application.Utilities.Helpers;

// TODO: İleride proje büyüdükçe PromptBuilder yapısına çekilmeli
public static class PromptHelper
{
    public static string GetSystemPromptByBehaviour(this SystemBehaviourType systemBehaviourType)
    {
        Dictionary<SystemBehaviourType, string> systemBehaviourTypeMapping = new()
        {
            { SystemBehaviourType.MathProfessor, "You are an expert math professor." },
            { SystemBehaviourType.SeniorSoftwareDeveloper, "You are an senior software developer." },
            { SystemBehaviourType.Historian, "You are an experienced historian." },
        };
        
        return systemBehaviourTypeMapping.TryGetValue(systemBehaviourType, out string systemBehaviour) ? systemBehaviour : string.Empty;
    }
    
    public static string GetUserPromptByBehaviour(this UserBehaviourType userBehaviourType, string userPrompt)
    {
        var userBehaviourTypeMapping = new Dictionary<UserBehaviourType, string>
        {
            {
                UserBehaviourType.Evaluation,
                $$"""
                 
                 Model response to be evaluated: {{userPrompt}}

                 Please evaluate the following response. 
                 Evaluation criteria: Accuracy, Completeness, Clarity, Neutrality. 
                 Assign a score between 0 and 100 for each criterion and provide your reasoning.
                 Respond in the following JSON format:
                 {
                     "accuracy": number,
                     "completeness": number,
                     "clarity": number,
                     "neutrality": number,
                     "comment": string
                 }

                 Note: Your answer must be in the same language as the response being evaluated. Produce a valid and complete JSON output. The response must not be incomplete or truncated. Do not include any explanation or formatting.

                """
            },
            {
                UserBehaviourType.Default,
                $"Question: {userPrompt} Note: Your answer must be in the same language as the response being evaluated. The response must not be incomplete or truncated. Do not include any explanation or formatting."
            }
        };

        return userBehaviourTypeMapping.TryGetValue(userBehaviourType, out var userBehaviour)
            ? userBehaviour
            : string.Empty;
    }
}