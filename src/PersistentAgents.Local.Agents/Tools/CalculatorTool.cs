using System.ComponentModel;

namespace PersistentAgents.Local.Agents.Tools;

public static class CalculatorTool
{
    [Description("Multiplies two numbers and returns the result.")]
    public static string Multiply(
        [Description("The first number to multiply.")]
        double left,

        [Description("The second number to multiply.")]
        double right)
    {
        var result = left * right;

        if (!double.IsFinite(result))
        {
            return "Error: The calculation produced an invalid result.";
        }

        return $"The result of {left} multiplied by {right} is {result}.";
    }

    [Description("Divides one number by another and returns the result.")]
    public static string Divide(
        [Description("The dividend (number to be divided).")]
        double dividend,

        [Description("The divisor (number to divide by). Must not be zero.")]
        double divisor)
    {
        Console.WriteLine("[Tool Executed] Calculator.Divide entered.");

        Console.WriteLine($"[Tool Arguments] Dividend: {dividend}");
        Console.WriteLine($"[Tool Arguments] Divisor: {divisor}");

        // Validate divisor
        if (divisor == 0)
        {
            Console.WriteLine("[Tool Validation] Division by zero rejected.");

            return "Error: Division by zero is not allowed.";
        }

        // Perform calculation
        var result = dividend / divisor;

        // Validate result
        if (!double.IsFinite(result))
        {
            Console.WriteLine("[Tool Validation] Invalid calculation result.");

            return "Error: The calculation produced an invalid result.";
        }

        Console.WriteLine($"[Tool Result] {result}");

        return $"The result of {dividend} divided by {divisor} is {result}.";
    }
}
