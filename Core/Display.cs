namespace Core;

public class Display
{
	public static string DoubleToEngrNotation(double number)
	{
		var exponent = Math.Floor(Math.Log10(number) / 3) * 3;
		var mantissa = number / Math.Pow(10,exponent);
		return mantissa + "×10^" + exponent;
	}
}