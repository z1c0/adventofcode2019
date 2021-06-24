using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 5

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
Part1();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	new IntCodeCpu(ReadInput()).Run(1);
	Console.WriteLine("------------------------------------");
	new IntCodeCpu(ReadInput()).Run(5);
}

static IEnumerable<int> ReadInput()
{
	foreach (var line in File.ReadAllText("input.txt").Split(','))
	{
		yield return int.Parse(line);
	}
}

class IntCodeCpu
{
	private int _pc;
	private readonly List<int> _memory;

	internal IntCodeCpu(IEnumerable<int> input)
	{
		_pc = 0;
		_memory = input.ToList();
	}

	internal void Run(int input)
	{
		while (true)
		{
			var instruction = new Instruction(_memory, _pc);
			switch (instruction.OpCode)
			{
				case 1:
					// add
					WriteTo(instruction.Parameter3, ReadFrom(instruction.Parameter1) + ReadFrom(instruction.Parameter2));
					_pc += 4;
					break;
					
				case 2:
					// multiply
					WriteTo(instruction.Parameter3, ReadFrom(instruction.Parameter1) * ReadFrom(instruction.Parameter2));
					_pc += 4;
					break;

				case 3:
					// read input
					WriteTo(instruction.Parameter1, input);
					_pc += 2;
					break;

				case 4:
					// write output
					input = ReadFrom(instruction.Parameter1);
					Console.WriteLine(input);
					_pc += 2;
					break;

				case 5:
					// jump-if-true
					_pc = ReadFrom(instruction.Parameter1) != 0 ? ReadFrom(instruction.Parameter2) : _pc + 3;
					break;

				case 6:
					// jump-if-false
					_pc = ReadFrom(instruction.Parameter1) == 0 ? ReadFrom(instruction.Parameter2) : _pc + 3;
					break;

				case 7:
					// less than
					WriteTo(instruction.Parameter3, ReadFrom(instruction.Parameter1) < ReadFrom(instruction.Parameter2) ? 1 : 0);
					_pc += 4;
					break;

				case 8:
					// equals
					WriteTo(instruction.Parameter3, ReadFrom(instruction.Parameter1) == ReadFrom(instruction.Parameter2) ? 1 : 0);
					_pc += 4;
					break;

				case 99:
					// halt
					return;

				default:
					throw new InvalidProgramException($"Invalid OpCode {instruction.OpCode}");
			}	
		}
	}

	private void WriteTo(int address, int value)
	{
		_memory[address] = value;
	}

	private int ReadFrom(int address)
	{
		return _memory[address];
	}
}

internal enum ParameterMode
{
	Position,
	Immediate,
}

internal class Instruction
{
	private readonly List<int> _memory;
	private readonly int _pc;

	internal Instruction(List<int> memory, int pc)
	{
		_memory = memory;
		_pc = pc;
	}

	private static ParameterMode ParseParameterMode(int code) => code switch
	{
		0 => ParameterMode.Position,
		1 => ParameterMode.Immediate,
		_ => throw new InvalidProgramException()
	};

	public override string ToString()
	{
		return $"{OpCode}";
	}


	public int OpCode { get => _memory[_pc] % 100; }
	public int Parameter1 { get => GetParameter(1, 100); }
	public int Parameter2 { get => GetParameter(2, 1000); }
	public int Parameter3 { get => GetParameter(3, 10000); }

	private int GetParameter(int pos, int modePos)
	{	
		var fullCode = _memory[_pc];
		var addr = _pc + pos;
		var parameterMode = ParseParameterMode(fullCode / modePos % 10);
		return parameterMode == ParameterMode.Immediate ?
			addr :
			_memory[addr];
	}
}