namespace dr.BarryBridge.Console;

public record ProgramArguments(DateTimeOffset Start, DateTimeOffset End, bool Csv)
{
    public static ProgramArguments FromArgs(string[] args)
    {
        bool csv = false;
        if (args.Length > 2)
        {
            csv = args[2].Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase);
        }
        
        DateTimeOffset start = DateTimeOffset.Now.Date.AddDays(-1);
        DateTimeOffset end = start.AddDays(1);
        if (args.Length >= 2)
        {
            DateTimeOffset.TryParse(args[0], out start);
            DateTimeOffset.TryParse(args[1], out end);
        }

        return new ProgramArguments(start, end, csv);
    }
}