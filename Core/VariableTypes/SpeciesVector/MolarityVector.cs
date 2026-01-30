namespace Core.VariableTypes;

/// <summary>
/// Represents the species molarity vector of a system.
/// </summary>
public class MolarityVector : ISpeciesVector<Molarity>
{
	public MolarityVector(Dictionary<Chemical, Molarity> _concentrations) => dict = _concentrations;

	public MolarityVector() => dict = [];

	/// <summary>
	/// Calculates the total molar concentration of the mixture.
	/// </summary>
	/// <returns>molar concentration, in [mol/L]</returns>
	public Molarity Total()
	{
		Molarity val = 0;
		foreach (var item in dict)
		{
			val += item.Value;
		}
		return val;
	}

	/// <summary>
	/// Creates an exact copy of the vector without reference to this original.
	/// </summary>
	public MolarityVector DeepCopy()
	{
		MolarityVector dictCopy = [];
		foreach (var item in dict) dictCopy.Add(item.Key, item.Value);
		return dictCopy;
	}

	/// <summary>
	/// Adds a given species vector to this vector.
	/// If the given vector contains a species not present in the
	/// current vector, the new species will be included in this vector.
	/// </summary>
	public void CombineVectors(Dictionary<Chemical, Molarity> second)
	{
		foreach (var kvp in second)
		{
			// If the species is already in this vector, add the value to the current one.
			if (dict.ContainsKey(kvp.Key))
			{
				dict[kvp.Key] += kvp.Value;
			}
			// Otherwise, include the new species in the vector.
			else
			{
				dict.Add(kvp.Key, kvp.Value);
			}
		}
	}

	/// <inheritdoc cref="AddVector(Dictionary{Chemical, MolarityVector})"/>
	public void CombineVectors(MolarityVector second)
	{
		CombineVectors(second.dict);
	}

	/// <summary>
	/// Sums the values of the vectors across common species.
	/// </summary>
	public static MolarityVector CombineVectors(MolarityVector first, MolarityVector second)
	{
		var keys = first.Keys.Union(second.Keys).ToList();
		var vec = new MolarityVector();
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
	public void AddOrCombineValue(Chemical species, Molarity value)
	{
		if (dict.ContainsKey(species))
		{
			dict[species] += value;
		}
		else
		{
			dict.Add(species, value);
		}
	}


	public static implicit operator Dictionary<Chemical, Molarity>(MolarityVector vec) => vec.dict;
}