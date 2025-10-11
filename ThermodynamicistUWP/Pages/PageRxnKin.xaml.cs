using Core;
using Core.EquationsOfState;
using Core.Reactions;
using Core.Reactions.Kinetics;
using Core.VariableTypes;
using Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ThermodynamicistUWP.Dialogs;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace ThermodynamicistUWP
{
	public sealed partial class PageRxnKin : Page
	{
		public PageRxnViewModel ViewModel { get; } = new PageRxnViewModel();

		public ObservableCollection<RxnKinOutputItem> RxnKinOutputDataSource { get; set; }


		public PageRxnKin()
		{
			InitializeComponent();

			// Initializes rate law list in RateLaw dropdown.
			// Note the use of RateLawFactory instead of the RateLaw object directly.
			DropdownRateLaw.Items.Add(new ElementaryRateLawFactory());

			// Sets input values to defaults
			ViewModel.T = 273;
			ViewModel.P = 101325;
			ViewModel.FrequencyFactor = 1;
			ViewModel.ActivationEnergy = 0;

			// Set TreeOutputSelection options.
			//Microsoft.UI.Xaml.Controls.TreeViewNode node_thermo = new Microsoft.UI.Xaml.Controls.TreeViewNode
			//{
			//	Content = "Thermodynamic properties",
			//	IsExpanded = true
			//};
			//var node_rxnH = new Microsoft.UI.Xaml.Controls.TreeViewNode { Content = "Molar enthalpy of reaction [J/mol]" };
			//var node_rxnS = new Microsoft.UI.Xaml.Controls.TreeViewNode { Content = "Molar entropy of reaction [J/K/mol]" };
			//var node_rxnG = new Microsoft.UI.Xaml.Controls.TreeViewNode { Content = "Molar Gibbs energt of reaction [J/mol]" };
			//node_thermo.Children.Add(node_rxnH);
			//node_thermo.Children.Add(node_rxnS);
			//node_thermo.Children.Add(node_rxnG);
			//TreeOuputSelection.RootNodes.Add(node_thermo);
			//Microsoft.UI.Xaml.Controls.TreeViewNode node_Kinetics = new Microsoft.UI.Xaml.Controls.TreeViewNode
			//{
			//	Content = "Kinetics",
			//	IsExpanded = true
			//};
			//var node_PlotMolarityTransience = new Microsoft.UI.Xaml.Controls.TreeViewNode { Content = "Plot: molarity transience" };
			//node_Kinetics.Children.Add(node_PlotMolarityTransience);
			//TreeOuputSelection.RootNodes.Add(node_Kinetics);
			RxnKinOutputDataSource = GetData();
		}

		private void ButtonAddSpecies_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.AddItem( new ControlRxnSpeciesViewModel
			{
				Chemical = Chemical.Acetone,
				EoSFactory = new PengRobinsonEOSFactory(),
				Stoich = 1,
				Phase = "",
				IsReactant = true,
				DeleteCommand = ViewModel.DeleteCommand
			});
		}

		/// <summary>
		/// Checks inputs and packages into Core objects.
		/// </summary>
		private void RunCalc(object sender, RoutedEventArgs e)
		{
			// If any inputs are not set, do not attempt to run calculations!
			if (
				ViewModel.RateLawFactory == null ||
				double.IsNaN(ViewModel.T) || ViewModel.T == 0 ||
				double.IsNaN(ViewModel.P) || ViewModel.P == 0 ||
				double.IsNaN(ViewModel.FrequencyFactor) ||
				double.IsNaN(ViewModel.ActivationEnergy)
				)
			{
				ErrorDialog.ShowErrorDialog("Not all required inputs are set.");
				return;
			}

			// Validate RxnSpecies inputs.
			var chemicals = new List<Chemical>();
			foreach (var viewModel in ViewModel.Items)
			{
				if (!viewModel.CheckValidInput())
				{
					ErrorDialog.ShowErrorDialog("Reaction species contains invalid inputs.");
					return;
				}
				if (chemicals.Contains(viewModel.Chemical))
				{
					ErrorDialog.ShowErrorDialog($"Chemical '{Constants.ChemicalNames[viewModel.Chemical]}' has multiple entries." +
						$"\nBalance the reaction equation and try again.");
					return;
				} else
				{
					chemicals.Add(viewModel.Chemical);
				}
			}

			Temperature T = ViewModel.T;
			Pressure P = ViewModel.P;
			double frequencyFactor = ViewModel.FrequencyFactor;
			GibbsEnergy activationEnergy = ViewModel.ActivationEnergy;
			var rateLawFactory = ViewModel.RateLawFactory;

			// Get species list from ControlRxnSpecies
			var rxn = new Reaction(ViewModel.GetRxnSpeciesList(), rateLawFactory, frequencyFactor, activationEnergy);

			UpdateData(rxn, T, P);
			UpdatePlots(rxn, T, P);
		}

		/// <summary>
		/// Runs calculations, updates DataLabel with outputs.
		/// </summary>
		private void UpdateData(Reaction rxn, Temperature T, Pressure P)
		{
			try
			{
				var dHrxn = rxn.MolarEnthalpyOfReaction(T, P);
				DataLabel.Text = "Molar enthalpy of reaction: " + dHrxn.ToEngrNotation(5);
			} catch (Exception e)
			{
				ErrorDialog.ShowErrorDialog(e);
			}
		}

		/// <summary>
		/// Updates plots.
		/// </summary>
		private void UpdatePlots(Reaction rxn, Temperature T, Pressure P)
		{
			try
			{
				// Fills in the plot view with a view model using the new settings
				PlotViewKin.Model = new RxnKineticsPlotModel(rxn, T, P, 0.01, 10).Model;
				PlotViewKin.Background = new SolidColorBrush(Colors.White);
			}
			catch (Exception exception)
			{
				ErrorDialog.ShowErrorDialog(exception);
				return;
			}
		}

		/// <summary>
		/// Initialize the data for TreeRxnKinOutputSelection.
		/// </summary>
		/// <returns></returns>
		private ObservableCollection<RxnKinOutputItem> GetData()
		{
			return new ObservableCollection<RxnKinOutputItem>
			{
				new RxnKinOutputItem
				{
					Name = "Thermodynamic properties",
					Type = RxnKinOutputItem.ItemType.Folder,
					Children =
					{
						new RxnKinOutputItem
						{
							Name = "Molar enthalpy of reaction [J/mol]",
							Item = RxnKinOutputItem.ItemName.MolarEnthalpyOfReaction,
							Type = RxnKinOutputItem.ItemType.Number
						},
						new RxnKinOutputItem
						{
							Name = "Molar entropy of reaction [J/mol]",
							Item = RxnKinOutputItem.ItemName.MolarEntropyOfReaction,
							Type = RxnKinOutputItem.ItemType.Number
						},
						new RxnKinOutputItem
						{
							Name = "Molar Gibbs energy of reaction [J/mol]",
							Item = RxnKinOutputItem.ItemName.MolarGibbsEnergyOfReaction,
							Type = RxnKinOutputItem.ItemType.Number
						},
					},
				},
				new RxnKinOutputItem
				{
					Name = "Kinetics",
					Type = RxnKinOutputItem.ItemType.Folder,
					Children =
					{
						new RxnKinOutputItem
						{
							Name = "Molarity transience",
							Item = RxnKinOutputItem.ItemName.PlotMolarityTransience,
							Type = RxnKinOutputItem.ItemType.Plot
						},
					},
				},
			};
		}

	}


	public class RxnKinOutputItem
	{
		public enum ItemType
		{
			Folder,
			Number,
			Plot
		}

		public enum ItemName
		{
			MolarEnthalpyOfReaction,
			MolarEntropyOfReaction,
			MolarGibbsEnergyOfReaction,

			PlotMolarityTransience
		}

		public string Name { get; set; }

		public ItemName Item { get; set; }
		
		public ItemType Type { get; set; }
		public ObservableCollection<RxnKinOutputItem> Children { get; set; } = new ObservableCollection<RxnKinOutputItem>();

		private static readonly Dictionary<ItemName, string> ItemNameToString = new Dictionary<ItemName, string>()
		{
			[ItemName.MolarEnthalpyOfReaction] = "Molar enthalpy of reaction [J/mol]"
		};
	}

	public class RxnKinOutputItemTemplateSelector : DataTemplateSelector
	{
		/// <summary>
		/// Template to use for folder items in the TreeView.
		/// </summary>
		public DataTemplate FolderTemplate { get; set; }

		/// <summary>
		/// Template to use for number items in the TreeView.
		/// </summary>
		public DataTemplate NumberTemplate { get; set; }

		/// <summary>
		/// Template to use for plot items in the TreeView.
		/// </summary>
		public DataTemplate PlotTemplate { get; set; }

		/// <summary>
		/// Determines which template to use for each item in the TreeView based on its type.
		/// </summary>
		/// <param name="item">A RxnKinOutputItem.</param>
		protected override DataTemplate SelectTemplateCore(object item)
		{
			// Cast the item to the ExplorerItem type.
			var rxnKinOutputItem = (RxnKinOutputItem)item;

			// Return the appropriate template: FolderTemplate for folders, FileTemplate for files.
			switch (rxnKinOutputItem.Type)
			{
				case RxnKinOutputItem.ItemType.Folder:
					return FolderTemplate;
				case RxnKinOutputItem.ItemType.Number:
					return NumberTemplate;
				case RxnKinOutputItem.ItemType.Plot:
					return PlotTemplate;
				default:
					throw new ArgumentException("Could not determine RxnKinOutputItem type.");
			}
		}
	}

}
