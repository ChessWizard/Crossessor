using Crossessor.API.Features.Compares.CrossCompare;

namespace Crossessor.API.Application.Utilities.Helpers;

public static class EvaluationHelper
{
    public static double CalculateOverallScore(int accuracy, 
        int completeness,
        int clarity,
        int neutrality)
    {
        double total = accuracy + completeness + clarity + neutrality;
        double average = total / 4.0;
        return Math.Round(average, 2, MidpointRounding.AwayFromZero);
    }
}