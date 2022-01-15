# BarryBridge
A (very basic) integration of [Barry's](https://barry.energy/) API in C# / .NET6

Barry is a Danish electricity provider. I made this to be able to extract data from their API and use it for analysis of my own energy consumption. It is also an example of how one could integrate with their API in .NET. Their [API documentation can be found here](https://developer.barry.energy/).

Currently the main functionality is a console app that can output data in .csv format. I use that to import into Google Sheets. I have an ambition to create a website with some visualization of the data, at some point; but this is currently not implemented. 

## How to run 
First, you will need a Barry API token. Get it from their app (consult their documentation); and put in an environment variable named `BarryToken`.

Then you will run the program in a terminal with the following 3 parameters:
* Start date for time period to extract data
* End date for time period to extract data
* Csv flag - true to output csv data, false outputs a more human-readable output.

As an example, in the `Source/dr.BarryBridge.Console/` directory, you could run:
```
dotnet run 2022-01-01 2022-02-01 true
```

## CSV Output
The CSV contains one row per hour in the time period you defined, provided Barry has data for that time period. In the CSV you will see the following columns.
* Mpid: Metering Point ID, identifies your electricity meter. For most people you will have one meter and one Mpid, but there could be more for some advanced users.
* Start: Date and time
* End: Date and time
* Kwh: Number of kilowatt-hours the meter registered 
* CostPerKwh: Cost per kWh in this hour
* CostForHour: Cost for the hour (Kwh * CostPerKwh)
