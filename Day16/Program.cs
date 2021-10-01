using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 16 - START");
var sw = Stopwatch.StartNew();
Part1();
//Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var input = ReadInput().ToList();
	var output = input.ToList();
	const int phases = 100;
	for (var phase = 1; phase <= phases; phase++)
	{
		CalculatePhase(input, output);
		input = output;
	}
	Print(input, 0);
}

static void Part2()
{
	var tmp = ReadInput().ToArray();
	var input = new List<int>();
	for (var i = 0; i < 10000; i++)
	{
		input.AddRange(tmp);
	}
	var output = input.ToList();
	const int phases = 100;
	for (var phase = 1; phase <= phases; phase++)
	{
		CalculatePhase(input, output);
		input = output;
		System.Console.WriteLine($"{phase}");
	}

	var offset = 0;
	foreach (var d in tmp.Take(7))
	{
		offset *= 10;
		offset += d;
	}
	Print(input, offset);
}

static void CalculatePhase(List<int> input, List<int> output)
{
	var pattern = new int[] { 0, 1, 0, -1 };
	var count = input.Count;
	var offs = 1;
	for (var i = 0; i < count; i++)
	{
		long l = 0;
		var j = offs;
		while (j <= count)
		{
			var pos = j / (i + 1) % 4;
			l += input[j - 1] * pattern[pos];
			j++;
		}
		output[i] = (int)Math.Abs(l % 10);
		offs++;
	}
}

static void Print(IEnumerable<int> signal, int skip)
{
	Console.WriteLine($"Signal : {string.Join(string.Empty, signal.Skip(skip).Take(8))}");
}

static IEnumerable<int> ReadInput()
{
	return File.ReadAllText("input.txt").Select(c => int.Parse(c.ToString()));
}

