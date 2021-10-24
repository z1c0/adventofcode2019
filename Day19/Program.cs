using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 19 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();

Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var grid = BuildGrid(0, 0, 50, 50);
	Print(grid);
}

static void Part2()
{
	// I simply binary-searched my way into the solution.
	var x = 974;
	var y = 1242;
	var grid = BuildGrid(x, y, 100, 100);
	Print(grid);
	Console.WriteLine($"{x} * 10000 + {y} = {x * 10000 + y }");
}

static long[,] BuildGrid(int startX, int startY, int w, int h)
{
	var instructions = ReadInput().ToList();
	var grid = new long[w, h];
	var wantX = false;
	var x = 0;
	var y = 0;
	var cpu = new IntCodeCpu()
	{
		ReadInput = () => { wantX = ! wantX; return wantX ? x : y; },
		WriteOutput = o => grid[x - startX, y - startY] = o,
	};

	for (y = startY; y < startY + h; y++)
	{
		for (x = startX; x < startX + w; x++)
		{
			cpu.Reset(instructions);
			cpu.Run();
		}
	}
	return grid;
}

static void Print(long[,] grid)
{
	var affectedPoints = 0;
	var w = grid.GetLength(0);
	var h = grid.GetLength(1);
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			var c = '.';
			if (grid[x, y] == 1)
			{
				c = '#';
				affectedPoints++;
			}
			Console.Write(c);
		}
		Console.WriteLine();
	}
	Console.WriteLine($"{affectedPoints} points are affected by the tractor beam.");
	System.Console.WriteLine();
}

static IEnumerable<long> ReadInput()
{
	foreach (var line in File.ReadAllText("input.txt").Split(','))
	{
		yield return long.Parse(line);
	}
}

class IntCodeCpu
{
	private long _pc;
	private long _relativeBase;
	private Dictionary<long, long> _memory;

	public void Reset(IEnumerable<long> instructions)
	{
		_pc = 0;
		_relativeBase = 0;
		long i = 0;
		_memory = instructions.ToDictionary(a => i++);
	}

	public bool IsRunning { get; private set; }

	internal Func<long> ReadInput { get; set; }
	internal Action<long> WriteOutput { get; set; }

	internal void Run()
	{
		IsRunning = true;
		while (true)
		{
			var opCode = GetCurrentOpCode();
			switch (opCode)
			{
				case 1:
					// add
					WriteTo(GetParameter(3), ReadFrom(GetParameter(1)) + ReadFrom(GetParameter(2)));
					_pc += 4;
					break;

				case 2:
					// multiply
					WriteTo(GetParameter(3), ReadFrom(GetParameter(1)) * ReadFrom(GetParameter(2)));
					_pc += 4;
					break;

				case 3:
					// read input
					WriteTo(GetParameter(1), ReadInput());
					_pc += 2;
					break;

				case 4:
					// write output
					WriteOutput(ReadFrom(GetParameter(1)));
					_pc += 2;
					return;

				case 5:
					// jump-if-true
					_pc = ReadFrom(GetParameter(1)) != 0 ? ReadFrom(GetParameter(2)) : _pc + 3;
					break;

				case 6:
					// jump-if-false
					_pc = ReadFrom(GetParameter(1)) == 0 ? ReadFrom(GetParameter(2)) : _pc + 3;
					break;

				case 7:
					// less than
					WriteTo(GetParameter(3), ReadFrom(GetParameter(1)) < ReadFrom(GetParameter(2)) ? 1 : 0);
					_pc += 4;
					break;

				case 8:
					// equals
					WriteTo(GetParameter(3), ReadFrom(GetParameter(1)) == ReadFrom(GetParameter(2)) ? 1 : 0);
					_pc += 4;
					break;

				case 9:
					// adjust relative base
					_relativeBase += ReadFrom(GetParameter(1));
					_pc += 2;
					break;

				case 99:
					// halt
					IsRunning = false;
					return;

				default:
					throw new InvalidProgramException($"Invalid OpCode {opCode}");
			}
		}
	}

	private long GetCurrentOpCode() => _memory[_pc] % 100;

	private long GetParameter(int pos)
	{
		var modePos = (long)Math.Pow(10, pos + 1);
		var fullCode = _memory[_pc];
		var v = _pc + pos;
		var parameterMode = ParseParameterMode(fullCode / modePos % 10);
		return parameterMode switch
		{
			ParameterMode.Immediate => v,
			ParameterMode.Position => ReadFrom(v),
			ParameterMode.Relative => ReadFrom(v) + _relativeBase,
			_ => throw new InvalidOperationException(),
		};
	}

	private void WriteTo(long address, long value)
	{
		_memory[address] = value;
	}

	private long ReadFrom(long address)
	{
		if (_memory.TryGetValue(address, out var value))
		{
			return value;
		}
		return 0;
	}

	private static ParameterMode ParseParameterMode(long code) => code switch
	{
		0 => ParameterMode.Position,
		1 => ParameterMode.Immediate,
		2 => ParameterMode.Relative,
		_ => throw new InvalidProgramException()
	};
}

internal enum ParameterMode
{
	Position,
	Immediate,
	Relative,
}