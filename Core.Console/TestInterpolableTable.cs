using Spectre.Console;
using ThermodynamicistCore;
using ThermodynamicistCore.Data;
using ThermodynamicistCore.VariableTypes;

static class TestInterpolableTable
{
	public static void Test()
	{
		var data = new Dictionary<Temperature, Pressure>()
		{
			[100] = 100e3,
			[400] = 400e3,
			[300] = 200e3,
			[200] = 300e3
		};
		var table = new InterpolableTable<Temperature, Pressure>(data);
		Console.WriteLine(table.GetValue(150).ToString());
	}
}
