using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 15 - START");
var sw = Stopwatch.StartNew();
var history = new Dictionary<(int, int), long>
{
	{ (0, 0), 1 }
};
Part1(history);
Part2(history);
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1(Dictionary<(int, int), long> history)
{
	var instructions = ReadInput().ToList();
	var queue = new Queue<Droid>();
	EnqueueClones(queue, new Droid(instructions), history);
	while (queue.Any())
	{
		var d = queue.Dequeue();
		var code = d.Run();
		history.TryAdd((d.X, d.Y), code);
		switch (code)
		{
			case 0:
			  // wall
				break;
			case 1:
				// step
				EnqueueClones(queue, d, history);
				break;
			case 2:
				// oxygen system
				Console.WriteLine(d);
				EnqueueClones(queue, d, history);
				break;
			default:
				throw new InvalidOperationException();
		}
	}
}

static void Part2(Dictionary<(int X, int Y), long> history)
{
	Print(history);
	var minutes = 0;
	while (history.Values.Any(v => v == 1))
	{
		var oxygenCells = history.Where(e => e.Value == 2).ToList();
		foreach (var e in oxygenCells)
		{
			TryOxygenFill((e.Key.X, e.Key.Y - 1), history);
			TryOxygenFill((e.Key.X, e.Key.Y + 1), history);
			TryOxygenFill((e.Key.X - 1, e.Key.Y), history);
			TryOxygenFill((e.Key.X + 1, e.Key.Y), history);
		}
		minutes++;
	}
	Console.WriteLine($"After {minutes} minutes the whole area is filled with oxygen.");
}

static void TryOxygenFill((int x, int y) pos, Dictionary<(int X, int Y), long> history)
{
	if (history[pos] == 1)
	{
		history[pos] = 2;
	}
}

static void Print(Dictionary<(int X, int Y), long> history)
{
	var minX = history.Keys.Min(k => k.X);
	var minY = history.Keys.Min(k => k.Y);
	var maxX = history.Keys.Max(k => k.X);
	var maxY = history.Keys.Max(k => k.Y);
	for (var y = minY; y <= maxY; y++)
	{
		for (var x = minX; x <= maxX; x++)
		{
			var c = ' ';
			if (history.TryGetValue((x, y), out long v))
			{
				switch (v)
				{
					case 0:
						c = '#';
						break;
					case 1:
						c = '.';
						break;
					case 2:
						c = 'O';
						break;
				}
			}
			Console.Write(c);
		}
		Console.WriteLine();
	}	
	Console.WriteLine();
}


static void EnqueueClones(Queue<Droid> queue, Droid droid, Dictionary<(int, int), long> history)
{
	foreach (var clone in droid.CreateClones().ToArray())
	{
		if (!history.ContainsKey((clone.X, clone.Y)))
		{
			queue.Enqueue(clone);
		}
	}
}

static IEnumerable<long> ReadInput()
{
	foreach (var line in File.ReadAllText("input.txt").Split(','))
	{
		yield return long.Parse(line);
	}
}

class Droid
{
	private readonly IntCodeCpu _cpu;

	public Droid(IEnumerable<long> instructions)
	{
		_cpu = new IntCodeCpu(instructions.ToList());
	}
	private Droid(IntCodeCpu cpu, long input, int x, int y, int generation)
	{
		_cpu = cpu;
		_cpu.ReadInput = () => input;
		X = x;
		Y = y;
		Generation = generation;
	}

	internal int X { get; }
	internal int Y { get;}
	public int Generation { get; }

	internal IEnumerable<Droid> CreateClones()
	{
		yield return new Droid(_cpu.Clone(), 1, X, Y - 1, Generation + 1);
		yield return new Droid(_cpu.Clone(), 2, X, Y + 1, Generation + 1);
		yield return new Droid(_cpu.Clone(), 3, X - 1, Y, Generation + 1);
		yield return new Droid(_cpu.Clone(), 4, X + 1, Y, Generation + 1);
	}

	public override string ToString()
	{
		return $"Droid: Generation {Generation} @ ({X}/{Y})";
	}

	internal long Run()
	{
		long returnCode = -1;
		_cpu.WriteOutput = (o) => returnCode = o;
		_cpu.Run();
		return returnCode;
	}
}

class IntCodeCpu
{
	private long _pc;
	private long _relativeBase;
	private readonly Dictionary<long, long> _memory;

	private IntCodeCpu(long pc, long relativeBase, Dictionary<long, long> memory)
	{
			_pc = pc;
			_relativeBase = relativeBase;
			_memory = memory;
	}

	public IntCodeCpu(IEnumerable<long> instructions)
	{
		_pc = 0;
		_relativeBase = 0;
		long i = 0;
		_memory = instructions.ToDictionary(a => i++);
	}

	public bool IsRunning { get; private set; }

	internal Func<long> ReadInput { get; set; }
	internal Action<long> WriteOutput { get; set; }

	internal IntCodeCpu Clone()
	{
		return new IntCodeCpu(_pc, _relativeBase, new Dictionary<long, long>(_memory));
	}

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