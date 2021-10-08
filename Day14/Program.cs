using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 14 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var instructions = ReadInput();
	var ore = Produce("FUEL", 1, instructions, new());
	Console.WriteLine($"{ore} units of ORE are needed.");
}

static void Part2()
{
	var instructions = ReadInput();
	const long oreBudget = 1000000000000L;
	var fuelUnits = 3209000;  // binary search ftw!
	var step = 1;
	while (true)
	{
		var ore = Produce("FUEL", fuelUnits, instructions, new());
		if (ore > oreBudget)
		{
			break;
		}
		if (fuelUnits % 1000 == 0) System.Console.WriteLine(fuelUnits);
		fuelUnits += step;
	}
	Console.WriteLine($"{fuelUnits - step} FUEL units can be produced.");
}

static long Produce(string what, long amountNeeded, Dictionary<Quantity, List<Quantity>> instructions, Dictionary<string, long> bank)
{
	var oreProduced = 0L;

	var instruction = instructions.Single(i => i.Key.Unit == what);
	// correct amountNeeded
	var amountProduced = instruction.Key.Amount; // amount produced in 1 reaction
	var numberOfReactions = (long)Math.Ceiling((double)amountNeeded / amountProduced); // reactions necessary
	foreach (var i in instruction.Value)
	{
		oreProduced += Consume(i.Unit, i.Amount * numberOfReactions, instructions, bank);
	}
	var amountReallyProduced = amountProduced * numberOfReactions;
	if (amountReallyProduced > amountNeeded)
	{
		bank.TryGetValue(what, out var n);
		bank[what] = n + (amountReallyProduced - amountNeeded);
	}

	return oreProduced;
}

static long Consume(string what, long amountNeeded, Dictionary<Quantity, List<Quantity>> instructions, Dictionary<string, long> bank)
{
	var oreProduced = 0L;
	if (bank.ContainsKey(what))
	{
		if (bank[what] >= amountNeeded)
		{
			bank[what] -= amountNeeded;
			amountNeeded = 0;
		}
		else
		{
			amountNeeded -= bank[what];
			bank[what] = 0;
		}
	}

	if (amountNeeded > 0)
	{
		if (what == "ORE")
		{
			oreProduced = amountNeeded;
		}
		else
		{
			oreProduced = Produce(what, amountNeeded, instructions, bank);
		}
	}
	return oreProduced;
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
