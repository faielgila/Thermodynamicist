using System.Collections;

namespace Core.VariableTypes;

/// <summary>
/// Represents a vector indexed by a chemical/species of any ThermoVariable.
/// </summary>
/// <typeparam name="ThermoType"></typeparam>
public abstract class ISpeciesVector<ThermoType> : IDictionary<Chemical, ThermoType> where ThermoType : ThermoVariable
{
	public Dictionary<Chemical, ThermoType> dict;

	/// <summary>
	/// Creates an exact copy of the composition vector without reference to this original.
	/// </summary>
	//public abstract ISpeciesVector<ThermoType> DeepCopy();

	protected ISpeciesVector() => dict = [];

	protected ISpeciesVector(Dictionary<Chemical, ThermoType> _keyValuePairs) => dict = _keyValuePairs;

	public static implicit operator Dictionary<Chemical, ThermoType>(ISpeciesVector<ThermoType> vec) => vec.dict;


	public ICollection<Chemical> Keys => ((IDictionary<Chemical, ThermoType>)dict).Keys;

	public ICollection<ThermoType> Values => ((IDictionary<Chemical, ThermoType>)dict).Values;

	public int Count => ((ICollection<KeyValuePair<Chemical, ThermoType>>)dict).Count;

	public bool IsReadOnly => ((ICollection<KeyValuePair<Chemical, ThermoType>>)dict).IsReadOnly;

	public ThermoType this[Chemical key] { get => ((IDictionary<Chemical, ThermoType>)dict)[key]; set => ((IDictionary<Chemical, ThermoType>)dict)[key] = value; }

	public void Add(Chemical key, ThermoType value)
	{
		((IDictionary<Chemical, ThermoType>)dict).Add(key, value);
	}

	public bool ContainsKey(Chemical key)
	{
		return ((IDictionary<Chemical, ThermoType>)dict).ContainsKey(key);
	}

	public bool Remove(Chemical key)
	{
		return ((IDictionary<Chemical, ThermoType>)dict).Remove(key);
	}

	public bool TryGetValue(Chemical key, out ThermoType value)
	{
		return ((IDictionary<Chemical, ThermoType>)dict).TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<Chemical, ThermoType> item)
	{
		((ICollection<KeyValuePair<Chemical, ThermoType>>)dict).Add(item);
	}

	public void Clear()
	{
		((ICollection<KeyValuePair<Chemical, ThermoType>>)dict).Clear();
	}

	public bool Contains(KeyValuePair<Chemical, ThermoType> item)
	{
		return ((ICollection<KeyValuePair<Chemical, ThermoType>>)dict).Contains(item);
	}

	public void CopyTo(KeyValuePair<Chemical, ThermoType>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<Chemical, ThermoType>>)dict).CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<Chemical, ThermoType> item)
	{
		return ((ICollection<KeyValuePair<Chemical, ThermoType>>)dict).Remove(item);
	}

	public IEnumerator<KeyValuePair<Chemical, ThermoType>> GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<Chemical, ThermoType>>)dict).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)dict).GetEnumerator();
	}
}


/// <summary>
/// Represents a generic ISpeciesVector for one-off use cases where
/// the <see cref="ISpeciesVector{ThermoType}"/> interface is more
/// desireable than simply using Dictionary<Chemical, ThermoType>.
/// </summary>
/// <typeparam name="ThermoType"></typeparam>
public class GenericSpeciesVector<ThermoType> : ISpeciesVector<ThermoType> where ThermoType : ThermoVariable;