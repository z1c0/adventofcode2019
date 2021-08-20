using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 14 - START");
var sw = Stopwatch.StartNew();
Part1();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var input = ReadInput();
	var ore = Resolve("FUEL", 1, input, new());
	Console.WriteLine($"{ore} units of ORE are needed.");
}

static int Resolve(string unit, int amountNeeded, Dictionary<Quantity, List<Quantity>> input, Dictionary<string, int> oreBank)
{
	// consume unit
	// is unit banked?
	// yes: take from bank
	// no: produce subunit
	// goto1 with subunit


	var ore = 0;
	var from = input.Single(x => x.Key.Unit == unit);
	foreach (var f in from.Value)
	{
		if (f.Unit == "ORE")
		{
			oreBank.TryGetValue(unit, out var banked);
			if (banked >= amountNeeded)
			{
				oreBank[unit] = banked - amountNeeded;
			}
			else
			{
				var times = (int)Math.Ceiling((double)amountNeeded / from.Key.Amount);
				var oreProduced = times * f.Amount;
				var oreConsumed = times * f.Amount;
				oreBank[unit] = banked + (oreProduced - oreConsumed);
				ore += oreProduced;
			}
		}
		else
		{
			for (var i = 0; i < amountNeeded; i++)
			{
				ore += Resolve(f.Unit, f.Amount, input, oreBank);
			}
		}
	}
	return ore;
}

static Dictionary<Quantity, List<Quantity>> ReadInput()
{
	static Quantity ParseQuantity(string input)
	{
		var parts = input.Trim().Split(' ');
		return new Quantity
		{
			Amount = int.Parse(parts[0]),
			Unit = parts[1],
		};
	}
	var input = new Dictionary<Quantity, List<Quantity>>();
	foreach (var line in File.ReadAllLines("input.txt"))
	{
		var tokens = line.Split(" => ");
		var from = tokens[0].Split(',').Select(x => ParseQuantity(x)).ToList();
		var to = ParseQuantity(tokens[1]);
		input.Add(to, from);
	}
	return input;
}

record Quantity
{
	public int Amount { get; set; }
	public string Unit { get; set;}
}
