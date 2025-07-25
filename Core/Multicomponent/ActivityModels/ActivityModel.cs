using Core.VariableTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Multicomponent.ActivityModels;

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
	public abstract double SpeciesActivityCoefficient(Chemical chemical, Temperature T);
}

