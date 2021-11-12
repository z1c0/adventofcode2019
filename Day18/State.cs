using System;
using System.Collections.Generic;
using System.Linq;
using aoc;

internal interface IState : IComparable
{
	IEnumerable<IState> GetAdjacent();
	int KeyCount { get; }
	int Distance { get; }

	bool HasKeyForDoor(char door);
	void AddKey(char key);
}

internal class State : IState
{
	internal State(Grid map, int x, int y, HashSet<string> cache)
	{
		_map = map;
		X = x;
		Y = y;
		_cache = cache;
	}

	private readonly Grid _map;

	internal int X { get; set; }
	internal int Y { get; set; }

	private readonly HashSet<string> _cache;

	internal HashSet<char> Keys { get; init; } = new ();

	public int Distance { get; set; }

	public override string ToString()
	{
		return $"{X}/{Y}";
	}

	private string FingerPrint()
	{
		var k = string.Join('|', Keys.OrderBy(k => k));
		return string.Format($"{X}/{Y}-{k}");
	}

	public int CompareTo(object obj)
	{
		return ((State)obj).Distance.CompareTo(Distance);
	}

	public IEnumerable<IState> GetAdjacent()
	{
		return GetAdjacent(this);
	}

	internal IEnumerable<State> GetAdjacent(IState keyBroker)
	{
		var n1 = Clone();
		n1.X--;
		n1.Distance++;
		var n2 = Clone();
		n2.X++;
		n2.Distance++;
		var n3 = Clone();
		n3.Y--;
		n3.Distance++;
		var n4 = Clone();
		n4.Y++;
		n4.Distance++;
		if (n1.Check(_map, keyBroker))
		{
			yield return n1;
		}
		if (n2.Check(_map, keyBroker))
		{
			yield return n2;
		}
		if (n3.Check(_map, keyBroker))
		{
			yield return n3;
		}
		if (n4.Check(_map, keyBroker))
		{
			yield return n4;
		}
	}

	internal State Clone()
	{
		return new State(_map, X, Y, _cache)
		{
			Keys = Keys.ToHashSet(),
			Distance = Distance,
		};
	}

	private bool Check(Grid map, IState keyBroker)
	{
		var fingerPrint = FingerPrint();
		if (_cache.Contains(fingerPrint))
		{
			return false;
		}
		_cache.Add(fingerPrint);

		if (!map.IsInBounds(X, Y))
		{
			return false;
		}
		var c = map[X, Y];
		if (c == '#')
		{
			return false;
		}
		if (c >= 'A' && c <= 'Z')
		{		
			// Do we have a key for this door?
			return keyBroker.HasKeyForDoor(c);
		}
		if (c >= 'a' && c <= 'z')
		{
			keyBroker.AddKey(c);
		}
		return true;
	}

	public bool HasKeyForDoor(char door)
	{
		return Keys.Contains(char.ToLower(door));
	}

	public void AddKey(char key)
	{
		Keys.Add(key);
	}

	public int KeyCount { get => Keys.Count; }
}
