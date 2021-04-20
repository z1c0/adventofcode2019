using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 4

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
Part1();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

void Part1()
{
	var (from, to) = ReadInput();
	var count = 0;
	var count2 = 0;
	System.Console.WriteLine(ValidateStrict(112233));
	System.Console.WriteLine(ValidateStrict(123444));
	System.Console.WriteLine(ValidateStrict(111122));
	for (var password = from; password <= to; password++)
	{
		if (Validate(password))
		{
			count++;
		}
		if (ValidateStrict(password))
		{
			count2++;
		}
	}
	Console.WriteLine($"{count} passwords are valid");
	Console.WriteLine($"{count2} passwords are strictly valid");
}

bool ValidateStrict(int password)
{
	var digits = password.ToString();
	var tuple = false;
	for (var i = 0; i < digits.Length - 1; i++)
	{
		var d1 = digits[i];
		var d2 = digits[i + 1];
		if (d1 > d2)
		{
			return false;
		}
		if (d1 == d2)
		{
			var tupleCandidate = true;
			var j = i + 2;
			while (j < digits.Length && digits[j] == d1)
			{
				tupleCandidate = false;
				j++;
				i = j - 2;
			}
			if (tupleCandidate)
			{
				tuple = true;
			}
		}
	}
	return tuple;
}

bool Validate(int password)
{
	var digits = password.ToString();
	var tuple = false;
	for (var i = 0; i < digits.Length - 1; i++)
	{
		var d1 = digits[i];
		var d2 = digits[i + 1];
		if (d1 > d2)
		{
			return false;
		}
		if (d1 == d2)
		{
			tuple = true;
		}
	}
	return tuple;
}

(int from, int to) ReadInput()
{
	var tokens = File.ReadAllText("input.txt").Split('-');
	return (int.Parse(tokens[0]), int.Parse(tokens[1]));
}
