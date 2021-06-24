using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 8

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
const int width = 25;
const int height = 6;
Part1(width, height);
Part2(width, height);
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1(int width, int height)
{
	var layers = ReadInput(width, height);
	var min0Digits = layers.Min(l => CountDigits(l, 0));
	var l = layers.Single(l => CountDigits(l, 0) == min0Digits);
	var digits1 = CountDigits(l, 1);
	var digits2 = CountDigits(l, 2);
	Console.WriteLine($"Number of 1 digits * number of 2 digits: {digits1 * digits2}");
}

static void Part2(int width, int height)
{
	var layers = ReadInput(width, height);
	var merged = new int[width, height];
	for (var y = 0; y < height; y++)
	{
		for (var x = 0; x < width; x++)
		{
			foreach (var l in layers)
			{
				var pixel = l[x, y];
				if (pixel != 2)
				{
					merged[x, y] = pixel;
					break;
				}
			}
		}
	}
	Print(merged);
}

static void Print(int[,] layer)
{
	var width = layer.GetLength(0);
	var height = layer.GetLength(1);
	for (var y = 0; y < height; y++)
	{
		for (var x = 0; x < width; x++)
		{
			Console.Write(layer[x, y] == 0 ? ' ' : '*');
		}
		Console.WriteLine();
	}
}

static int CountDigits(int[,] layer, int digit)
{
	var count = 0;
	var width = layer.GetLength(0);
	var height = layer.GetLength(1);
	for (var y = 0; y < height; y++)
	{
		for (var x = 0; x < width; x++)
		{
			if (layer[x, y] == digit)
			{
				count++;
			}
		}
	}
	return count;
}

static IEnumerable<int[,]> ReadInput(int width, int height)
{
	var input = File.ReadAllText("input.txt");
	var pos = 0;
	while (pos < input.Length)
	{
		var layer = new int[width, height];
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				layer[x, y] = input[pos++] - '0';
			}
		}
		yield return layer;
	}
}

