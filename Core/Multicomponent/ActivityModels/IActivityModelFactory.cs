namespace Core.Multicomponent.ActivityModels;

/// <summary>
/// Provides an interface for defining ActivityModelFactories, which removes the need to create
/// a fully-specified ActivityModel instance. To be used whenever the type of an ActivityModel
/// needs to be passed around, but does not need to be instatiated yet.
/// When the activity model is finally needed, use <see cref="Create"/> to generate an
/// <see cref="ActivityModel"/> using the implemented ActivityModelFactory.
/// </summary>
/// <example>
/// This factory is used in the UI and is bound to the DropdownActivityModel control.
/// Using a factory instead of the model directly means that the UI doesn't need to instantiate
/// an activity model, so that the EoS can be properly instantiated later when all the input
/// controls are set to the desired value.
/// </example>
public interface IActivityModelFactory
{
	/// <summary>
	/// Stores a human-readable name for the activity model.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Generate an instance of the activity model.
	/// </summary>
	/// <returns>An <see cref="ActivityModel"/> instance.</returns>
	ActivityModel Create(List<MixtureSpecies> speciesList);
}

/// <inheritdoc cref="IActivityModelFactory"/>
public class UNIFACActivityModelFactory : IActivityModelFactory
{
	public string Name => "UNIFAC";

	public ActivityModel Create(List<MixtureSpecies> speciesList) => new UNIFACActivityModel(speciesList);
}

/// <inheritdoc cref="IActivityModelFactory"/>
public class IdealMixtureModelFactory : IActivityModelFactory
{
	public string Name => "Ideal Mixture/Solution";

	public ActivityModel Create(List<MixtureSpecies> speciesList) => new IdealMixture("vapor", speciesList);
}