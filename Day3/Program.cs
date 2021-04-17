using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 3

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
Part1();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var grid = new Dictionary<(int, int), int>();
	var (wire1, wire2) = ReadInput();
	var results = new List<(int, int)>();
	FollowWire(grid, wire1, null);
	FollowWire(grid, wire2, results);
	Console.WriteLine($"Closest Manhattan intersection distance {results.Min(t => t.Item1)}");
	Console.WriteLine($"Closest combined intersection distance {results.Min(t => t.Item2)}");
}

static void FollowWire(Dictionary<(int, int), int> grid, IEnumerable<(char dir, int length)> wire, List<(int, int)> results)
{
	var x = 0;
	var y = 0;
	var distance = 0;
	foreach (var (dir, length) in wire)
	{
		var offsX = 0;
		var offsY = 0;
		switch (dir)
		{
			case 'U':
				offsY = -1;
				break;
			case 'R':
				offsX = 1;
				break;
			case 'D':
				offsY = 1;
				break;
			case 'L':
				offsX = -1;
				break;
			default:
				throw new InvalidOperationException();
		}
		for (var i = 0; i < length; i++)
		{
			x += offsX;
			y += offsY;
			var current = distance + (i + 1);
			if (results == null)
			{
				grid[(x, y)] = current;
			}
			else
			{
				if (grid.ContainsKey((x, y)))
				{
					var manhattan = Math.Abs(x) + Math.Abs(y);
					var combined = grid[(x, y)] + current;
					results.Add((manhattan, combined));
				}
			}
		}
		distance += length;
	}
}

static (IEnumerable<(char dir, int length)> wire1, IEnumerable<(char dir, int length)> wire2) ReadInput()
{
	static IEnumerable<(char, int)> ParseLine(string line)
	{
			foreach (var token in line.Split(','))
			{
				yield return (token[0], int.Parse(token[1..]));
			}
	}
	var lines = File.ReadAllLines("input.txt");
	return (ParseLine(lines[0]), ParseLine(lines[1]));
}
