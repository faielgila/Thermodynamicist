using System.Collections;

namespace Core.VariableTypes;

/// <summary>
/// Represents the species composition vector of a phase in the system.
/// CalculateRemainingFraction should be used to determine
/// the mole fraction of the system's species basis.
/// </summary>
public class CompositionVector : ISpeciesVector<MoleFraction>
{

	public CompositionVector(Dictionary<Chemical, MoleFraction> _compositions) => dict = _compositions;

	public CompositionVector() => dict = [];

	/// <summary>
	/// Creates an exact copy of the composition vector without reference to this original.
	/// </summary>
	public CompositionVector DeepCopy()
	{
		CompositionVector dictCopy = [];
		foreach (var item in dict) dictCopy.Add(item.Key, item.Value);
		return dictCopy;
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


	public static implicit operator Dictionary<Chemical, MoleFraction>(CompositionVector vec) => vec.dict;
}