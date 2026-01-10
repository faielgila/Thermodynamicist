namespace Core.VariableTypes;

/// <summary>
/// Represents the species molarity vector of a system.
/// </summary>
public class MolarityVector : ISpeciesVector<Molarity>
{
	public MolarityVector(Dictionary<Chemical, Molarity> _concentrations) => dict = _concentrations;

	public MolarityVector() => dict = [];

	/// <summary>
	/// Creates an exact copy of the vector without reference to this original.
	/// </summary>
	public MolarityVector DeepCopy()
	{
		MolarityVector dictCopy = [];
		foreach (var item in dict) dictCopy.Add(item.Key, item.Value);
		return dictCopy;
	}


	public static implicit operator Dictionary<Chemical, Molarity>(MolarityVector vec) => vec.dict;
}