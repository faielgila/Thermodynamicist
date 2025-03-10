namespace Core.EquationsOfState
{
    public interface IEquationOfStateFactory
    {
        string Name { get; }

        EquationOfState Create(Chemical chemical);
    }

    public class PengRobinsonEOSFactory : IEquationOfStateFactory
    {
        public string Name => "Peng-Robinson";

        public EquationOfState Create(Chemical species) => new PengRobinsonEOS(species);
    }

    public class VanDerWaalsEOSFactory : IEquationOfStateFactory
    {
        public string Name => "van der Waals";

        public EquationOfState Create(Chemical species) => new VanDerWaalsEOS(species);
    }

    public class ModSolidLiquidVaporEOSFactory : IEquationOfStateFactory
    {
        public string Name => "modified Solid-Liquid-Vapor";

        public EquationOfState Create(Chemical species) => new ModSolidLiquidVaporEOS(species);
    }
}
