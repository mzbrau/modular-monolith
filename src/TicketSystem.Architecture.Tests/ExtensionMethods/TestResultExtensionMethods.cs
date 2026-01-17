using System.Text;

namespace TicketSystem.Architecture.Tests.ExtensionMethods;

public static class TestResultExtensionMethods
{
    public static string GetDetails(this NetArchTest.Rules.TestResult? result, string message)
    {
        var builder = new StringBuilder();
        builder.AppendLine(message);
        if (result is { IsSuccessful: false, FailingTypes: not null })
        {
            builder.AppendLine("Failing types:");
            foreach (var name in result.FailingTypeNames)
            {
                builder.AppendLine($"- {name}");
            }
        }

        return builder.ToString();
    }
}