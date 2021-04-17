using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 1

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	long sum = 0;
	foreach (var i in ReadInput())
	{
		sum += GetFuelSimple(i);
	}
	Console.WriteLine($"Total fuel required: {sum}");
}

static void Part2()
{
	long sum = 0;
	foreach (var i in ReadInput())
	{
		sum += GetFuelComplex(i);
	}
	Console.WriteLine($"Total fuel required: {sum}");
}

static int GetFuelSimple(int i)
{
	return i / 3 - 2;
}

static int GetFuelComplex(int i)
{
	var fuel = 0;
	while (true)
	{
		i = GetFuelSimple(i);
		if (i <= 0)
		{
			break;
		}
		fuel += i;
	}
	return fuel;
}

static IEnumerable<int> ReadInput()
{
	foreach (var line in File.ReadAllLines("input.txt"))
	{
		yield return int.Parse(line);
	}
}