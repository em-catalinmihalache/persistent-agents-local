using System.ComponentModel;

namespace PersistentAgents.Local.Agents.Tools;

public static class CurrentDateTimeTool
{
    [Description("Returns the current local date and time.")]
    public static string GetCurrentDateTime()
    {
        Console.WriteLine("[Tool Executed] GetCurrentDateTime");

        return DateTimeOffset.Now.ToString("O");
    }

    [Description("Returns the current local date.")]
    public static string GetCurrentDate()
    {
        Console.WriteLine("[Tool Executed] GetCurrentDate");

        return DateTimeOffset.Now.ToString("yyyy-MM-dd");
    }

    [Description("Returns the current local time.")]
    public static string GetCurrentTime()
    {
        Console.WriteLine("[Tool Executed] GetCurrentTime");

        return DateTimeOffset.Now.ToString("HH:mm:ss zzz");
    }
}