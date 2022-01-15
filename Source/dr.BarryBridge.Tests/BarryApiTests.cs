using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using dr.BarryBridge.Common;
using FluentAssertions;
using NUnit.Framework;

namespace dr.BarryBridge.Tests;

[Explicit("Calls real BarryApi - Requires a BarryToken to run")]
[Category("Integration")]
public class BarryApiTests
{
    public TimePeriod LastWeek { get; } = new(DateTimeOffset.Now.AddDays(-7).Date, DateTimeOffset.Now.Date);
    public BarryApi Sut { get; private set; }
    
    [SetUp]
    public void BeforeEach()
    {
        var token = Environment.GetEnvironmentVariable("BarryToken");
        if (token == null)
        {
            throw new ApplicationException(
                "Please set the environment variable 'BarryToken' to a valid Barry API token to run these tests.");
        }

        Sut = new BarryApi(new HttpClient(), token);
    }

    [Test]
    public async Task GetMeteringPoints_GetsAtLeastOne()
    {
        var meteringPoints = await Sut.GetMeteringPoints(default);

        meteringPoints.Count().Should().BeGreaterThan(0);
    }
    
    [Test]
    public async Task GetConsumption_ReturnsSomeConsumption()
    {
        var meteringPoints = await Sut.GetMeteringPoints(default);
        var mp = meteringPoints.First();

        var consumption = await Sut.GetConsumption(mp, LastWeek, default);

        consumption.Should().NotBeNullOrEmpty();
        consumption.Count.Should().BeGreaterThan(100);
        consumption.Should().OnlyContain(c => c.Quantity > 0 && c.Start >= LastWeek.Start);
    }

    [Test]
    public async Task GetHourlyCost_ReturnsSomeCost()
    {
        var meteringPoints = await Sut.GetMeteringPoints(default);
        var mp = meteringPoints.First();

        var consumption = await Sut.GetHourlyCost(mp, LastWeek, default);

        consumption.Should().NotBeNullOrEmpty();
        consumption.Count.Should().BeGreaterThan(100);
        consumption.Should().OnlyContain(c => c.Value > 0 && c.Start >= LastWeek.Start);   
    }
}