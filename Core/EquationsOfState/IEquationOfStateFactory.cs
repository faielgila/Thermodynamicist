namespace Core.EquationsOfState
{
	/// <summary>
	/// Provides an interface for defining EoSFactories, which removes the need to create
	/// a fully-specified EoS instance. To be used whenever the type of an EoS needs to be
	/// passed around, but does not need to be instatiated yet.
	/// When the EoS is finally needed, use <see cref="Create(Chemical)"/> to generate an
	/// <see cref="EquationOfState"/> using the implemented EoSFactory.
	/// </summary>
	/// <example>
	/// This factory is used in the UI and is bound to the DropdownEOS control.
	/// Using a factory instead of the EoS directly means that the UI doesn't need to instantiate
	/// an EoS, so that the EoS can be properly instantiated later when all the input controls are
	/// set to the desired value.
	/// </example>
	public interface IEquationOfStateFactory
	{
		/// <summary>
		/// Stores a human-readable name for the EoS.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Generate an instance of the EoS with the specified <see cref="Chemical"/>.
		/// </summary>
		/// <param name="chemical"></param>
		/// <returns>An <see cref="EquationOfState"/> instance.</returns>
		EquationOfState Create(Chemical chemical);
	}

	/// <inheritdoc cref="IEquationOfStateFactory"/>
	public class PengRobinsonEOSFactory : IEquationOfStateFactory
	{
		public string Name => "Peng-Robinson";

		public EquationOfState Create(Chemical species) => new PengRobinsonEOS(species);
	}

	/// <inheritdoc cref="IEquationOfStateFactory"/>
	public class VanDerWaalsEOSFactory : IEquationOfStateFactory
	{
		public string Name => "van der Waals";

		public EquationOfState Create(Chemical species) => new VanDerWaalsEOS(species);
	}

	/// <inheritdoc cref="IEquationOfStateFactory"/>
	public class ModSolidLiquidVaporEOSFactory : IEquationOfStateFactory
	{
		public string Name => "modified Solid-Liquid-Vapor";

		public EquationOfState Create(Chemical species) => new ModSolidLiquidVaporEOS(species);
	}
}
