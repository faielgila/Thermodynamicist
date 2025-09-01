using Core.VariableTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core;

public class MetadataCalculationCache : CalculationCache
{
	/// <summary>
	/// Gets the cached value given the set of input variables provided.
	/// </summary>
	/// <param name="methodName">Name of the method/function used to generate the value.</param>
	/// <param name="inputVars">Array of input variables.</param>
	/// <returns>Value cached from the provided inputVars, or null if the value has not been cached yet.</returns>
	public double? GetCached(string methodName, object metadata, ThermoVariable[] inputVars)
	{
		bool IsCached = hashCache.TryGetValue(GenerateHashcode(methodName, metadata, inputVars), out double outputVar);
		if (IsCached) return outputVar;
		else return null;
	}

	private int GenerateHashcode(string methodName, object metadata, ThermoVariable[] inputVars)
	{
		// Leftmost part of the hashcode is from the method name.
		int hashcode = methodName.GetHashCode();

		// Middle part of the hashcode is from the supplied metadata.
		hashcode = (hashcode << 32) + metadata.GetHashCode();

		// Concatenate each integer (representing a hashcode) into one big hashcode
		// TODO : Develop an algorithm for which the order in which input inputVars
		//        are stored in the array does not affect the final hashcode
		foreach (ThermoVariable var in inputVars)
		{
			hashcode = (hashcode << 32) + var.Value.GetHashCode();
		}

		return hashcode;
	}
}
