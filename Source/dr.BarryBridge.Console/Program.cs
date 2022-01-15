using System.Globalization;
using dr.BarryBridge.Common;
using dr.BarryBridge.Console;
using static System.Console;
using static System.FormattableString;

var arguments = ProgramArguments.FromArgs(args);

string? token = Environment.GetEnvironmentVariable("BarryToken");
if (String.IsNullOrEmpty(token))
{
    throw new ApplicationException("Token not found in environment variable 'BarryToken'.");
}

CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var api = new BarryApi(new HttpClient(), token);
var meteringPoints = await api.GetMeteringPoints(cts.Token);

foreach (var p in meteringPoints)
{
    if (arguments.Csv)
    {
        var headers = new[] { "Mpid", "Start", "End", "Kwh", "CostPerKwh", "CostForHour" };
        WriteLine(string.Join(",", headers));
    }
    else
    {
        WriteLine($"MPID: {p.Mpid}\t{p.Country} {p.PriceCode} from {arguments.Start} to {arguments.End}:\n");
    }

    var period = new TimePeriod(arguments.Start, arguments.End);

    var consumption = await api.GetConsumption(p, period, cts.Token);
    var hourCost = await api.GetHourlyCost(p, period, cts.Token);
    var consumedCost = consumption.Join(hourCost, c => c.Start, c => c.Start,
        (cons, cost) => new
        {
            cons.Mpid, cons.Start, cons.End, cons.Quantity, CostPerKwh = cost.Value,
            CostForHour = cost.Value * cons.Quantity, cost.Currency
        }).ToArray();
    foreach (var c in consumedCost)
    {
        if (arguments.Csv)
        {
            var values = new[]
            {
                c.Mpid,
                c.Start.ToLocalTime().ToString(),
                c.End.ToLocalTime().ToString(),
                c.Quantity.ToString("0.00", CultureInfo.InvariantCulture),
                c.CostPerKwh.ToString("0.00", CultureInfo.InvariantCulture),
                c.CostForHour.ToString("0.00", CultureInfo.InvariantCulture)
            };
            WriteLine(String.Join(",", values));
        }
        else
        {
            WriteLine(Invariant($"{c.Quantity,5:0.00} kWh @ {c.CostPerKwh,5:0.00} / hour totalling {c.CostForHour,5:0.00} {c.Currency} from {c.Start.LocalDateTime:HH:mm} to {c.End.LocalDateTime:HH:mm}"));
        }
    }
    
}
