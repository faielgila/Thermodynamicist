using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace Core.ViewModels
{
	public class OutputItem
	{
		public enum ItemType
		{
			Folder,
			Number,
			Plot
		}

		public string OutputName { get; set; }

		public string DisplayName { get; set; }

		public Func<string, string> DisplayFormat { get; set; }

		public ItemType Type { get; set; }

		public string Glyph { get; private set; }

		public static readonly Dictionary<ItemType, string> ItemTypeToGlyphString = new()
		{
			[ItemType.Folder] = "\uED41",
			[ItemType.Number] = "\uE8EF",
			[ItemType.Plot] = "\uE9D2"
		};

		public ObservableCollection<OutputItem> Children { get; set; } = [];

		public OutputItem(string _outputName, string _displayName, Func<string, string> _displayFormat, ItemType _type)
		{
			OutputName = _outputName;
			DisplayName = _displayName;
			DisplayFormat = _displayFormat;
			Type = _type;
			Glyph = ItemTypeToGlyphString[_type];
		}

		public OutputItem(ItemType _type)
		{
			Type = _type;
			Glyph = ItemTypeToGlyphString[_type];
		}
	}


	public partial class OutputSelectionPopupViewModel : ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<OutputItem> _allOutputOptions = [];

		[ObservableProperty]
		private ObservableCollection<OutputItem> _availableOutputOptions = [];

		[ObservableProperty]
		private ObservableCollection<OutputItem> _selectedOutputOptions = [];

		public OutputSelectionPopupViewModel() { }

		public OutputSelectionPopupViewModel(ObservableCollection<OutputItem> _options)
		{
			_allOutputOptions = _options;
		}
	}
}
