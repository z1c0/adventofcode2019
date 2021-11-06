using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace aoc
{
	public class Grid
	{
		public Grid(int width, int height)
		{
			Width = width;
			Height = height;
			_data = new char[Height, Width];
		}

		public void Print()
		{
			var sb = new StringBuilder();
			PrintTo(sb);
			Console.Write(sb);
		}

		internal (int X, int Y) Find(char c)
		{
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					if (_data[y, x] == c)
					{
						return (x, y);
					}
				}
			}
			return (-1, -1);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			PrintTo(sb);
			return sb.ToString();
		}

		public void PrintTo(StringBuilder sb)
		{
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					sb.Append(_data[y, x]);
				}
				sb.AppendLine();
			}
		}

		internal Grid Clone()
		{
			var clone = new Grid(Width, Height);
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					clone._data[y, x] = _data[y, x];
				}
			}
			return clone;
		}

		public Dictionary<char, int> CountDistinct()
		{
			var counts = new Dictionary<char, int>();
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					var c = _data[y, x];
					if (!counts.TryAdd(c, 1))
					{
						counts[c]++;
					}
				}
			}
			return counts;
		}

		public void Fill(char c, Func<int, int, bool> predicate = null)
		{
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					if (predicate == null || predicate(x, y))
					{
						_data[y, x] = c;
					}
				}
			}
		}

		public Dictionary<char, int> CountAdjacent4Distinct(int x, int y)
		{
			return GetAdjacentDistinct(x, y, false);
		}
		
		public Dictionary<char, int> CountAdjacent8Distinct(int x, int y)
		{
			return GetAdjacentDistinct(x, y, true);
		}

		private Dictionary<char, int> GetAdjacentDistinct(int x, int y, bool all)
		{
			var adjacents = new Dictionary<char, int>();
			void CheckThenAdd(int x, int y)
			{
				if (IsInBounds(x, y))
				{
					var c = _data[y, x];
					if (!adjacents.TryAdd(c, 1))
					{
						adjacents[c]++;
					}
					
				}
			}
			CheckThenAdd(x - 1, y);
			CheckThenAdd(x + 1, y);
			CheckThenAdd(x, y - 1);
			CheckThenAdd(x, y + 1);
			if (all)
			{
				CheckThenAdd(x - 1, y - 1);
				CheckThenAdd(x + 1, y - 1);
				CheckThenAdd(x - 1, y + 1);
				CheckThenAdd(x + 1, y + 1);
			}
			return adjacents;
		}

		public int CountAdjacent8(int x, int y, char c)
		{
			var counts = CountAdjacent8Distinct(x, y);
			counts.TryGetValue(c, out var count);
			return count;
		}

		public int CountAdjacent4(int x, int y, char c)
		{
			var counts = CountAdjacent4Distinct(x, y);
			counts.TryGetValue(c, out var count);
			return count;
		}

		public bool IsInBounds(int x, int y)
		{
			return x >= 0 && y >= 0 && x < Width && y < Height;
		}

		public int Count(char c)
		{
			var counts = CountDistinct();
			counts.TryGetValue(c, out var count);
			return count;
		}

		public int Count(Func<char, bool> func)
		{
			var count = 0;
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					if (func(_data[y, x]))
					{
						count++;
					}
				}
			}
			return count;
		}

		public char this[(int X, int Y) p]
		{
			get => this[p.X, p.Y];
			set => this[p.X, p.Y] = value;
		}

		public char this[int x, int y]
		{
			get => _data[y, x];
			set => _data[y, x] = value;
		}
		
		public int Width { get; }
		public int Height { get; }
		private readonly char[,] _data;

		internal static Grid FromFile(string fileName)
		{
			var lines = File.ReadAllLines(fileName);
			var h = lines.Length;
			var w = lines.First().Length;
			var grid = new Grid(w, h);
			for (var y = 0; y < h; y++)
			{
				for (var x = 0; x < w; x++)
				{
					grid._data[y, x] = lines[y][x];
				}
			}
			return grid;
		}
	}
}