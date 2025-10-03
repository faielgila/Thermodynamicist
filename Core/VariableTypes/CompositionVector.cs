using System.Collections;

namespace Core.VariableTypes;

/// <summary>
/// Represents the species composition vector of a phase in the system.
/// CalculateRemainingFraction should be used to determine
/// the mole fraction of the system's species basis.
/// </summary>
public struct CompositionVector : IDictionary<Chemical, MoleFraction>
{
	public Dictionary<Chemical, MoleFraction> compositions;

	public CompositionVector(Dictionary<Chemical, MoleFraction> _compositions) => compositions = _compositions;

	public CompositionVector() => compositions = [];

	/// <summary>
	/// Creates an exact copy of the composition vector without reference to this original.
	/// </summary>
	public CompositionVector Copy()
	{
		Dictionary<Chemical, MoleFraction> dict = [];
		foreach (var item in compositions) dict.Add(item.Key, item.Value);
		return new CompositionVector(dict);
	}

	/// <summary>
	/// Calculates a final mole fraction based on a list of all other mole fractions.
	/// </summary>
	/// <param name="fractions">List of all other mole fractions. If there are N species in the system, List should be N-1 elements.</param>
	public static MoleFraction CalculateRemainingFraction(List<MoleFraction> fractions)
	{
		var sum = fractions.Aggregate((a, b) => a + b);
		if (sum > 1) throw new Exception("Mole fractions must sum to at most 1.");
		return 1 - sum;
	}

	/// <inheritdoc cref="CalculateRemainingFraction(List{MoleFraction})"/>
	public static MoleFraction CalculateRemainingFraction(Dictionary<Chemical, MoleFraction> fractions)
	{
		return CalculateRemainingFraction(fractions.Values.ToList());
	}


	#region IDictionary implementation

	public MoleFraction this[Chemical key] { get => ((IDictionary<Chemical, MoleFraction>)compositions)[key]; set => ((IDictionary<Chemical, MoleFraction>)compositions)[key] = value; }

	public ICollection<Chemical> Keys => ((IDictionary<Chemical, MoleFraction>)compositions).Keys;

	public ICollection<MoleFraction> Values => ((IDictionary<Chemical, MoleFraction>)compositions).Values;

	public int Count => ((ICollection<KeyValuePair<Chemical, MoleFraction>>)compositions).Count;

	public bool IsReadOnly => ((ICollection<KeyValuePair<Chemical, MoleFraction>>)compositions).IsReadOnly;

	public void Add(Chemical key, MoleFraction value)
	{
		((IDictionary<Chemical, MoleFraction>)compositions).Add(key, value);
	}

	public void Add(KeyValuePair<Chemical, MoleFraction> item)
	{
		((ICollection<KeyValuePair<Chemical, MoleFraction>>)compositions).Add(item);
	}

	public void Clear()
	{
		((ICollection<KeyValuePair<Chemical, MoleFraction>>)compositions).Clear();
	}

	public bool Contains(KeyValuePair<Chemical, MoleFraction> item)
	{
		return ((ICollection<KeyValuePair<Chemical, MoleFraction>>)compositions).Contains(item);
	}

	public bool ContainsKey(Chemical key)
	{
		return ((IDictionary<Chemical, MoleFraction>)compositions).ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<Chemical, MoleFraction>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<Chemical, MoleFraction>>)compositions).CopyTo(array, arrayIndex);
	}

	public IEnumerator<KeyValuePair<Chemical, MoleFraction>> GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<Chemical, MoleFraction>>)compositions).GetEnumerator();
	}

	public bool Remove(Chemical key)
	{
		return ((IDictionary<Chemical, MoleFraction>)compositions).Remove(key);
	}

	public bool Remove(KeyValuePair<Chemical, MoleFraction> item)
	{
		return ((ICollection<KeyValuePair<Chemical, MoleFraction>>)compositions).Remove(item);
	}

	public bool TryGetValue(Chemical key, out MoleFraction value)
	{
		return ((IDictionary<Chemical, MoleFraction>)compositions).TryGetValue(key, out value);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)compositions).GetEnumerator();
	}

	public static implicit operator Dictionary<Chemical, MoleFraction>(CompositionVector vec) => vec.compositions;

	#endregion
}


/// <summary>
/// Represents the phase fraction/composition vector of a system.
/// CalculateRemainingFraaction should be used to determine
/// the phase fraction of the system's phase basis.
/// </summary>
public struct PhaseFractionVector : IDictionary<string, MoleFraction>
{
	public Dictionary<string, MoleFraction> compositions;

	public PhaseFractionVector(Dictionary<string, MoleFraction> _compositions) => compositions = _compositions;

	public PhaseFractionVector() => compositions = [];

	/// <summary>
	/// Creates an exact copy of the fraction vector without reference to this original.
	/// </summary>
	public PhaseFractionVector Copy()
	{
		Dictionary<string, MoleFraction> dict = [];
		foreach (var item in compositions) dict.Add(item.Key, item.Value);
		return new PhaseFractionVector(dict);
	}

	/// <summary>
	/// Calculates a final phase fraction based on a list of all other phase fractions.
	/// </summary>
	/// <param name="fractions">List of all other phase fractions. If there are N phases in the system, List should be N-1 elements.</param>
	public static MoleFraction CalculateRemainingFraction(List<MoleFraction> fractions)
	{
		var sum = fractions.Aggregate((a, b) => a + b);
		if (sum > 1) throw new Exception("Phase mole fractions must sum to at most 1.");
		return 1 - sum;
	}

	/// <inheritdoc cref="CalculateRemainingFraction(List{MoleFraction})"/>
	public static MoleFraction CalculateRemainingFraction(Dictionary<string, MoleFraction> fractions)
	{
		return CalculateRemainingFraction(fractions.Values.ToList());
	}

	
	#region IDictionary implementation

	public MoleFraction this[string key] { get => ((IDictionary<string, MoleFraction>)compositions)[key]; set => ((IDictionary<string, MoleFraction>)compositions)[key] = value; }

	public ICollection<string> Keys => ((IDictionary<string, MoleFraction>)compositions).Keys;

	public ICollection<MoleFraction> Values => ((IDictionary<string, MoleFraction>)compositions).Values;

	public int Count => ((ICollection<KeyValuePair<string, MoleFraction>>)compositions).Count;

	public bool IsReadOnly => ((ICollection<KeyValuePair<string, MoleFraction>>)compositions).IsReadOnly;

	public void Add(string key, MoleFraction value)
	{
		((IDictionary<string, MoleFraction>)compositions).Add(key, value);
	}

	public void Add(KeyValuePair<string, MoleFraction> item)
	{
		((ICollection<KeyValuePair<string, MoleFraction>>)compositions).Add(item);
	}

	public void Clear()
	{
		((ICollection<KeyValuePair<string, MoleFraction>>)compositions).Clear();
	}

	public bool Contains(KeyValuePair<string, MoleFraction> item)
	{
		return ((ICollection<KeyValuePair<string, MoleFraction>>)compositions).Contains(item);
	}

	public bool ContainsKey(string key)
	{
		return ((IDictionary<string, MoleFraction>)compositions).ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<string, MoleFraction>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<string, MoleFraction>>)compositions).CopyTo(array, arrayIndex);
	}

	public IEnumerator<KeyValuePair<string, MoleFraction>> GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<string, MoleFraction>>)compositions).GetEnumerator();
	}

	public bool Remove(string key)
	{
		return ((IDictionary<string, MoleFraction>)compositions).Remove(key);
	}

	public bool Remove(KeyValuePair<string, MoleFraction> item)
	{
		return ((ICollection<KeyValuePair<string, MoleFraction>>)compositions).Remove(item);
	}

	public bool TryGetValue(string key, out MoleFraction value)
	{
		return ((IDictionary<string, MoleFraction>)compositions).TryGetValue(key, out value);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)compositions).GetEnumerator();
	}

	#endregion
}