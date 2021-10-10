using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 18 - START");
var sw = Stopwatch.StartNew();
Part1();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{	
	var map = ReadInput();
	var (pos, numberOfKeys) = AnalyzeMap(map);
	Console.WriteLine($"Start position: {pos}");
	Console.WriteLine($"Number of keys: {numberOfKeys}");
	//Print(map);
	var minSteps = int.MaxValue;
	Move(pos.x, pos.y, map, numberOfKeys, new(), new(), new(), -1, ref minSteps, new());
}

static void Move(int x, int y, char[,] map, int numberOfKeys, List<char> doors, List<char> keys, HashSet<string> history, int steps, ref int minSteps, HashSet<string> cache)
{
	steps++;
	if (steps > minSteps)
	{
		return;
	}
	var w = map.GetLength(1);
	var h = map.GetLength(0);
	if (x < 0 || x >= w || y < 0 || y >= h)
	{
		return;
	}
	var c = map[y, x];
	if (c == '#')
	{
		return;
	}
	if (c >= 'A' && c <= 'Z')
	{
		if (!keys.Contains(char.ToLower(c)))
		{
			return;
		}
		if (!doors.Contains(c))
		{
			doors.Add(c);
		}
	}

	var fingerPrintCache = CreateCacheFingerprint(x, y, doors, keys);
	if (cache.Contains(fingerPrintCache))
	{
		return;
	}
	cache.Add(fingerPrintCache);


	var fingerPrint = CreateFingerprint(x, y, doors, keys);
	if (!history.Contains(fingerPrint))
	{
		history.Add(fingerPrint);

		if (c >= 'a' && c <= 'z')
		{
			if (!keys.Contains(c))
			{
				keys.Add(c);
				if (keys.Count == numberOfKeys && steps <= minSteps)
				{
					minSteps = steps;
					Console.WriteLine($"All keys found after {minSteps} steps: {string.Join(", ", keys)} doors: {string.Join(", ", doors)}");
					return;
				}
			}
		}
		
		Move(x - 1, y, map, numberOfKeys, doors.ToList(), keys.ToList(), history.ToHashSet(), steps, ref minSteps, cache);
		Move(x + 1, y, map, numberOfKeys, doors.ToList(), keys.ToList(), history.ToHashSet(), steps, ref minSteps, cache);
		Move(x, y - 1, map, numberOfKeys, doors.ToList(), keys.ToList(), history.ToHashSet(), steps, ref minSteps, cache);
		Move(x, y + 1, map, numberOfKeys, doors.ToList(), keys.ToList(), history.ToHashSet(), steps, ref minSteps, cache);
	}
}

static string CreateFingerprint(int x, int y, List<char> doors, List<char> keys)
{
	return $"{x}/{y}|{string.Join(',', doors)}|{string.Join(',', keys)}";
}

static string CreateCacheFingerprint(int x, int y, List<char> doors, List<char> keys)
{
	//return $"{x}/{y}|{string.Join(',', doors.OrderBy(d => d))}|{string.Join(',', keys.OrderBy(k => k))}";
	return $"{x}/{y}|{string.Join(',', keys)}";
	//return $"{string.Join(',', doors)}|{string.Join(',', keys)}";
}

static ((int x, int y) pos, int numberOfKeys) AnalyzeMap(char[,] map)
{
	var pos = (-1, -1);
	var numberOfKeys = 0;
	for (var y = 0; y < map.GetLength(0); y++)
	{
		for (var x = 0; x < map.GetLength(1); x++)
		{
			var c = map[y, x];
			if (c == '@')
			{
				pos.Item1 = x;
				pos.Item2 = y;
			}
			if (c >= 'a' && c <= 'z')
			{
				numberOfKeys++;
			}
		}		
	}
	return (pos, numberOfKeys);
}

static void Print(char[,] map)
{
	for (var y = 0; y < map.GetLength(0); y++)
	{
		for (var x = 0; x < map.GetLength(1); x++)
		{
			Console.Write(map[y, x]);
		}
		Console.WriteLine();
	}
}

static char[,] ReadInput()
{
	var lines = File.ReadAllLines("input.txt");
	var h = lines.Length;
	var w = lines.First().Length;
	var map = new char[h, w];
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			map[y, x] = lines[y][x];
		}
	}
	return map;
}
