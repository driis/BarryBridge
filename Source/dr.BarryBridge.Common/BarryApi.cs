using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace dr.BarryBridge.Common;

public class BarryApi
{
    private readonly HttpClient _client;
    private int _requestId = Random.Shared.Next();
    
    public BarryApi(HttpClient client, string token)
    {
        _client = client;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _client.BaseAddress = new Uri("https://jsonrpc.barry.energy");
    }

    public async Task<IReadOnlyCollection<MeteringPoint>> GetMeteringPoints(CancellationToken ct)
    {
        var rpc = await Invoke<RpcResult<MeteringPoint>>("co.getbarry.api.v1.OpenApiController.getMeteringPoints", ct);
        return rpc.Result;
    }

    public async Task<IReadOnlyCollection<Consumption>> GetConsumption(MeteringPoint mp, TimePeriod period, CancellationToken ct)
    {
        var rpc = await Invoke<RpcResult<Consumption>>("co.getbarry.api.v1.OpenApiController.getAggregatedConsumption", ct, 
            new [] {mp.Mpid}, period.Start.ToUniversalTime(), period.End.ToUniversalTime());
        return rpc.Result;
    }
    
    public async Task<IReadOnlyCollection<Cost>> GetHourlyCost(MeteringPoint mp, TimePeriod period, CancellationToken ct)
    {
        var rpc = await Invoke<RpcResult<Cost>>("co.getbarry.api.v1.OpenApiController.getTotalKwHourlyPrice", ct, 
            mp.Mpid, period.Start.ToUniversalTime(), period.End.ToUniversalTime());
        return rpc.Result;
    }


    private async Task<T> Invoke<T>(string method, CancellationToken ct, params object[] parameters)
    {
        int id = Interlocked.Increment(ref _requestId);
        var body = new
        {
            method,
            id,
            jsonrpc = "2.0",
            @params = parameters
        };

        var response = await _client.PostAsJsonAsync("json-rpc", body, cancellationToken: ct);
        response.EnsureSuccessStatusCode();
        var deserialized = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        if (deserialized == null)
        {
            throw new ApplicationException(
                $"Could not deserialize body as {typeof(T)}, even though a success status code {response.StatusCode} was returned");
        }

        return deserialized;
    }
}


public record RpcResult<T>(int Id, string Jsonrpc, T[] Result);
public record MeteringPoint(string Mpid, string Country, string PriceCode);
public record Consumption(string Mpid, decimal Quantity, DateTimeOffset Start, DateTimeOffset End);
public record Cost(string Mpid, decimal Value, string Currency, DateTimeOffset Start, DateTimeOffset End);

public record TimePeriod(DateTimeOffset Start, DateTimeOffset End);