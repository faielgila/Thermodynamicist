using Core.VariableTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Data;

/// <summary>
/// Represents a sorted data table of input type Tx and ourput type Ty.
/// Example: A table of chemical potentials at various mixture compositions. This
/// class would be used to find a composition given a composition without requiring
/// an expensive search algorithm.
/// </summary>
public class InterpolableTable<Tx, Ty> where Tx : ThermoVariable, new() where Ty : ThermoVariable, new()
{
	public SortedDictionary<Tx, Ty> Table { get; private set; }

	public (string x, string y) headers = ("col1", "col2");

	public InterpolableTable(Dictionary<Tx, Ty> data)
	{
		Table = new SortedDictionary<Tx, Ty>(data) ?? throw new ArgumentNullException(nameof(data));
		Resort();
	}

	public InterpolableTable()
	{
		Table = [];
	}

	/// <summary>
	/// Appends the row to the bottom of the table.
	/// Does not reorder or invert the table.
	/// Will overwrite the existing value of 'y' if 'x' already has an entry in the table.
	/// </summary>
	public void Add(Tx x, Ty y)
	{
		if (Table.ContainsKey(x)) Table[x] = y;
		else {  Table.Add(x, y); }
	}

	/// <summary>
	/// Appends the given table to the bottom of this table.
	/// </summary>
	public void Append(InterpolableTable<Tx, Ty> table)
	{
		foreach (var row in table.Table)
		{
			Add(row.Key, row.Value);
		}
	}

	/// <summary>
	/// Orders the rows in the table by input column.
	/// </summary>
	public void Resort()
	{
		Table.OrderBy(x => x.Key);
	}

	/// <summary>
	/// Swaps the columns of the table and returns a new <see cref="InterpolableTable{Ty, Tx}"/>
	/// with the same order as the original table.
	/// </summary>
	public InterpolableTable<Ty, Tx> Invert()
	{
		var TableInv = new InterpolableTable<Ty, Tx>();
		foreach (var entry in Table)
		{
			TableInv.Add(entry.Value, entry.Key);
		}
		return TableInv;
	}

	/// <summary>
	/// Returns the value corresponding to the given input.
	/// Should the input x not be present in the table, the
	/// value will be inferred using linear interpolation.
	/// Table need not be ordered before using this method,
	/// as it will reorder the table if a calculation is needed.
	/// </summary>
	/// <returns>
	/// Value in the table if key already exists;
	/// interpolated value if key is within bounds;
	/// null if key is outside bounds.
	/// </returns>
	public Ty? GetValue(Tx? x)
	{
		if (x is null) return null;

		// Should x already have an entry in the table, no need for interpolation.
		if (Table.TryGetValue(x, out var y)) return y;

		var max = Table.Keys.Max();
		var min = Table.Keys.Min();
		if (x > max || x < min)
			return null;

		Resort();
		// Find the largest key in the table which is smaller than x.
		var x0 = Table.Last(row => row.Key < x).Key;
		var y0 = Table[x0];
		// Find the smallest key in the table which is larger than x.
		var x1 = Table.First(row => row.Key > x).Key;
		var y1 = Table[x1];

		// Interpolate values.
		y = new Ty
		{
			Value = y0 + (x - x0) * (y1 - y0) / (x1 - x0)
		};
		Table.Add(x, y);
		Resort();
		return y;
	}

	/// <summary>
	/// Converts the table to a string of comma-separated values.
	/// </summary>
	public string ToCSVString()
	{
		var sb = new StringBuilder("");
		sb.Append($"{headers.x},{headers.y}");

		foreach (var entry in Table)
		{
			sb.Append($"\n{entry.Key},{entry.Value}");
		}
		return sb.ToString();
	}
}
