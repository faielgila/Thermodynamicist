using System.Collections;

namespace Core.VariableTypes;

/// <summary>
/// Represents the phase fraction/composition vector of a system.
/// CalculateRemainingFraaction should be used to determine
/// the phase fraction of the system's phase basis.
/// </summary>
public class PhaseFractionVector : IDictionary<string, MoleFraction>
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