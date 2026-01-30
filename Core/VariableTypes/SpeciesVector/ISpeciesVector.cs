using System.Collections;

namespace Core.VariableTypes;

/// <summary>
/// Represents a vector indexed by a chemical/species of any ThermoVariable.
/// </summary>
/// <typeparam name="ThermoType"></typeparam>
public abstract class ISpeciesVector<ThermoType> : IDictionary<Chemical, ThermoType> where ThermoType : ThermoVariable, new()
{
	public Dictionary<Chemical, ThermoType> dict;

	/// <summary>
	/// Creates an exact copy of the composition vector without reference to this original.
	/// </summary>
	//public abstract ISpeciesVector<ThermoType> DeepCopy();

	/// <summary>
	/// Multiplies the values in the vector by the given scalar.
	/// </summary>
	public void Scale(double scalar)
	{
		foreach (var key in dict.Keys)
		{
			dict[key] = new ThermoType()
			{
				Value = dict[key] * scalar
			};
		}
	}

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
public class GenericSpeciesVector<ThermoType> : ISpeciesVector<ThermoType> where ThermoType : ThermoVariable, new()
{
	/// <summary>
	/// Returns a new SpeciesVector which is scaled by the given double.
	/// Useful for converting vectors to a different ThermoType.
	/// </summary>
	/// <typeparam name="ReturnType">ThermoVariable type for the returned vector.</typeparam>
	public GenericSpeciesVector<ReturnType> ScalarMultiply<ReturnType>(double scalar) where ReturnType : ThermoVariable, new()
	{
		var outVec = new GenericSpeciesVector<ReturnType>();
		foreach (var item in this)
		{
			outVec.Add(item.Key, new ReturnType(){ Value = item.Value * scalar });
		}
		return outVec;
	}

	/// <summary>
	/// Adds a given species vector to this vector.
	/// If the given vector contains a species not present in the
	/// current vector, the new species will be included in this vector.
	/// </summary>
	public void CombineVectors(Dictionary<Chemical, ThermoType> second)
	{
		foreach (var kvp in second)
		{
			// If the species is already in this vector, add the value to the current one.
			if (dict.ContainsKey(kvp.Key))
			{
				dict[kvp.Key] = new ThermoType
				{
					Value = dict[kvp.Key] + kvp.Value
				};
			}
			// Otherwise, include the new species in the vector.
			else
			{
				dict.Add(kvp.Key, kvp.Value);
			}
		}
	}

	/// <inheritdoc cref="AddVector(Dictionary{Chemical, ThermoType})"/>
	public void CombineVectors(GenericSpeciesVector<ThermoType> second)
	{
		CombineVectors(second.dict);
	}

	/// <summary>
	/// Sums the values of the vectors across common species.
	/// </summary>
	public static GenericSpeciesVector<ThermoType> CombineVectors(GenericSpeciesVector<ThermoType> first, GenericSpeciesVector<ThermoType> second)
	{
		var keys = first.Keys.Union(second.Keys).ToList();
		var vec = new GenericSpeciesVector<ThermoType>();
		foreach (var species in keys)
		{
			if (first.ContainsKey(species)) vec.AddOrCombineValue(species, first[species]);
			if (second.ContainsKey(species)) vec.AddOrCombineValue(species, second[species]);
		}
		return vec;
	}

	/// <summary>
	/// If the key is not in the vector, a new key-value pair will be added.
	/// If the key is already in the vector, the value will be replaced with the
	/// sum of the existing and new value.
	/// </summary>
	public void AddOrCombineValue(Chemical species, ThermoType value)
	{
		// If the species is already in this vector, add the value to the current one.
		if (dict.ContainsKey(species))
		{
			dict[species] = new ThermoType
			{
				Value = dict[species] + value
			};
		}
		// Otherwise, include the new species in the vector.
		else
		{
			dict.Add(species, value);
		}
	}
}