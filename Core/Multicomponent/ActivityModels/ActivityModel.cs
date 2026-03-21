using ThermodynamicistCore.VariableTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThermodynamicistCore.Multicomponent.ActivityModels;

public abstract class ActivityModel
{
	public List<MixtureSpecies> speciesList;

	public ActivityModel(List<MixtureSpecies> _speciesList)
	{
		speciesList = _speciesList;
	}

	/// <summary>
	/// Estimates the activity coefficient for the given species in the mixture.
	/// </summary>
	/// <returns>activity coefficient, dimensionless</returns>
	public abstract double SpeciesActivityCoefficient(Chemical species, Temperature T, Pressure P);

	/// <summary>
	/// Returns an exact copy of the activity model.
	/// </summary>
	public abstract ActivityModel Copy();

	/// <summary>
	/// Returns the factory which can be used to spin up new instances of the activity model.
	/// </summary>
	public abstract IActivityModelFactory Factory();
}

