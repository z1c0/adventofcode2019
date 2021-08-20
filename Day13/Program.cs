using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

Console.WriteLine("Day 13 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var instructions = ReadInput();
	var arcade = new Arcade();
	arcade.Run(instructions);
	arcade.Draw();
	Console.WriteLine($"Number of block tiles: {arcade.Tiles.Count(t => t.Value == 2)}");
}

static void Part2()
{
	var instructions = ReadInput().ToList();
	instructions[0] = 2;
	var arcade = new Arcade();
	arcade.InputQueue.AddRange(File.ReadAllText("joystick.txt").Split(',').Select(a => long.Parse(a)));
	arcade.Run(instructions);
	arcade.Draw();
	//File.WriteAllText("joystick.txt", string.Join(',', arcade.InputHistory));
}

static IEnumerable<long> ReadInput()
{
	foreach (var line in File.ReadAllText("input.txt").Split(','))
	{
		yield return long.Parse(line);
	}
}

class Arcade
{
	private int _state = 0;
	private long _x = 0;
	private long _y = 0;

	public List<long> InputQueue { get; }= new();

	public List<long> InputHistory { get; } = new();
	internal Dictionary<(long, long), long> Tiles  { get; } = new();

	internal void Draw()
	{
		var maxX = Tiles.Keys.Max(k => k.Item1);
		var maxY = Tiles.Keys.Max(k => k.Item2);
		for (var y = 0; y <= maxY; y++)
		{
			for (var x = 0; x <= maxX; x++)
			{
				var p = Tiles[(x, y)] switch 
				{
					0 => ' ',
					1 => '+',
					2 => '*',
					3 => '-',
					4 => 'o',
					_ => throw new InvalidOperationException()
				};
				Console.Write(p);
			}
			Console.WriteLine();
		}
	}

	internal void Run(IEnumerable<long> instructions)
	{
		var cpu = new IntCodeCpu(instructions)
		{
			ReadInput = () => 
			{
				long command;
				if (InputQueue.Count > 0)
				{
					command = InputQueue[0];
					InputQueue.RemoveAt(0);
				}
				else
				{
					Draw();
					Console.Write("Joystick: ");
					command = Console.ReadKey().Key switch
					{
						ConsoleKey.LeftArrow => -1,
						ConsoleKey.RightArrow => 1,
						_ => 0
					};
				}
				InputHistory.Add(command);
				return command;
			},
			WriteOutput = (value) =>
			{
				switch (_state)
				{
					case 0:
						_x = value;
						break;
					case 1:
						_y = value;
						break;
					case 2:
						if (_x == -1 && _y == 0)
						{
							Console.WriteLine($"Score: {value}");
						}
						else
						{
							Tiles[(_x, _y)] = value;
						}
						break;
				}
				_state = (_state + 1) % 3;
			}
		};
		
		do
		{
			cpu.Run();
		}
		while (cpu.IsRunning);
	}
}

class IntCodeCpu
{
	private long _pc;
	private long _relativeBase;
	private readonly Dictionary<long, long> _memory;

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