using Core.Multicomponent;
using Core.Multicomponent.ActivityModels;
using Core.VariableTypes;

namespace Core.Reactions.Kinetics;

/// <summary>
/// Provides an interface for defining RateLawFactories, which removes the need to create
/// a fully-specified RateLaw instance. To be used whenever the type of a RateLaw
/// needs to be passed around, but does not need to be instatiated yet.
/// When the rate law is finally needed, use <see cref="Create"/> to generate a
/// <see cref="RateLaw"/> using the implemented RateLawFactory.
/// </summary>
/// <example>
/// This factory is used in the UI and is bound to the DropdownRateLaw control.
/// Using a factory instead of the model directly means that the UI doesn't need to instantiate
/// a rate law, so that the rate law can be properly instantiated later when all the input
/// controls are set to the desired value.
/// </example>
public interface IRateLawFactory
{
	/// <summary>
	/// Stores a human-readable name for the rate law.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Stores the frequency factor to pass into the RateLaw.
	/// </summary>
	double frequencyFactor { get; }

	/// <summary>
	/// Stores the frequency factor to pass into the RateLaw.
	/// </summary>
	GibbsEnergy activationEnergy { get; }

	/// <summary>
	/// Generate an instance of the rate law.
	/// </summary>
	/// <returns>A <see cref="RateLaw"/> instance.</returns>
	RateLaw Create(List<RxnSpecies> speciesList);
}


/// <inheritdoc cref="IRateLawFactory"/>
public class ElementaryRateLawFactory(double _frequencyFactor, GibbsEnergy _activationEnergy) : IRateLawFactory
{
	public string Name => "Elementary";

	public double frequencyFactor => _frequencyFactor;

	public GibbsEnergy activationEnergy => _activationEnergy;

	public RateLaw Create(List<RxnSpecies> speciesList)
		=> new ElementaryRateLaw(speciesList, frequencyFactor, activationEnergy);
}