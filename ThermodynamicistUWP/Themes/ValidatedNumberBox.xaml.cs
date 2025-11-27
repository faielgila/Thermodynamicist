using CommunityToolkit.WinUI;
using Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ThermodynamicistUWP
{
	public sealed partial class ValidatedNumberBox : UserControl
	{
		//public ValidatedNumberBoxViewModel ViewModel { get; } = new ValidatedNumberBoxViewModel();

		#region DependencyProperties

		public ValidatedNumberBoxViewModel ViewModel
		{
			get { return (ValidatedNumberBoxViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}
		public static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.Register(
				nameof(ViewModel), typeof(ValidatedNumberBoxViewModel),
				typeof(ValidatedNumberBox),
				new PropertyMetadata(null));

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
				nameof(Value), typeof(double),
				typeof(ValidatedNumberBox),
				new PropertyMetadata(double.NaN));

		public event TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs> ValueChanged
		{
			add { NumBox.ValueChanged += value; }
			remove { NumBox.ValueChanged -= value; }
		}

		public bool IsValidValue
		{
			get { return (bool)GetValue(IsValidValueProperty); }
			set { SetValue(IsValidValueProperty, value); }
		}
		public static readonly DependencyProperty IsValidValueProperty =
			DependencyProperty.Register(
				nameof(IsValidValue), typeof(bool),
				typeof(ValidatedNumberBox),
				new PropertyMetadata(true));

		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}
		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register(nameof(Header), typeof(string), typeof(ValidatedNumberBox), new PropertyMetadata(null));

		public string PlaceholderText
		{
			get { return (string)GetValue(PlaceholderTextProperty); }
			set { SetValue(PlaceholderTextProperty, value); }
		}
		public static readonly DependencyProperty PlaceholderTextProperty =
			DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(ValidatedNumberBox), new PropertyMetadata(null));

		public bool AcceptsExpression
		{
			get { return (bool)GetValue(AcceptsExpressionProperty); }
			set { SetValue(AcceptsExpressionProperty, value); }
		}
		public static readonly DependencyProperty AcceptsExpressionProperty =
			DependencyProperty.Register(nameof(AcceptsExpression), typeof(bool), typeof(ValidatedNumberBox), new PropertyMetadata(false));

		public Func<double, List<object>, bool> ErrorValidationFunction
		{
			get { return GetValue(ErrorValidationFunctionProperty) as Func<double, List<object>, bool>; }
			set { SetValue(ErrorValidationFunctionProperty, value); }
		}
		public static readonly DependencyProperty ErrorValidationFunctionProperty =
			DependencyProperty.Register(
				nameof(ErrorValidationFunction), typeof(Func<double, List<object>, bool>),
				typeof(ValidatedNumberBox),
				new PropertyMetadata(null));

		public Func<double, List<object>, bool> WarningValidationFunction
		{
			get { return GetValue(WarningValidationFunctionProperty) as Func<double, List<object>, bool>; }
			set { SetValue(WarningValidationFunctionProperty, value); }
		}
		public static readonly DependencyProperty WarningValidationFunctionProperty =
			DependencyProperty.Register(
				nameof(WarningValidationFunction), typeof(Func<double, List<object>, bool>),
				typeof(ValidatedNumberBox),
				new PropertyMetadata(null));

		public List<object> ErrorValidationParameters
		{
			get { return GetValue(ErrorValidationParametersProperty) as List<object>; }
			set { SetValue(ErrorValidationParametersProperty, value); }
		}
		public static readonly DependencyProperty ErrorValidationParametersProperty =
			DependencyProperty.Register(
				nameof(ErrorValidationParameters), typeof(List<object>),
				typeof(ValidatedNumberBox),
				new PropertyMetadata(null));

		public List<object> WarningValidationParameters
		{
			get { return GetValue(WarningValidationParametersProperty) as List<object>; }
			set { SetValue(WarningValidationParametersProperty, value); }
		}
		public static readonly DependencyProperty WarningValidationParametersProperty =
			DependencyProperty.Register(
				nameof(WarningValidationParameters), typeof(List<object>),
				typeof(ValidatedNumberBox),
				new PropertyMetadata(null));

		public bool CanBeNaN
		{
			get { return (bool)GetValue(CanBeNaNProperty); }
			set { SetValue(CanBeNaNProperty, value); }
		}
		public static readonly DependencyProperty CanBeNaNProperty =
			DependencyProperty.Register(nameof(CanBeNaN), typeof(bool), typeof(ValidatedNumberBox), new PropertyMetadata(false));

		public bool CanBeZero
		{
			get { return (bool)GetValue(CanBeZeroProperty); }
			set { SetValue(CanBeZeroProperty, value); }
		}
		public static readonly DependencyProperty CanBeZeroProperty =
			DependencyProperty.Register(nameof(CanBeZero), typeof(bool), typeof(ValidatedNumberBox), new PropertyMetadata(true));

		public bool CanBeNegative
		{
			get { return (bool)GetValue(CanBeNegativeProperty); }
			set { SetValue(CanBeNegativeProperty, value); }
		}
		public static readonly DependencyProperty CanBeNegativeProperty =
			DependencyProperty.Register(nameof(CanBeNegative), typeof(bool), typeof(ValidatedNumberBox), new PropertyMetadata(true));

		public bool MustBeInteger
		{
			get { return (bool)GetValue(MustBeIntegerProperty); }
			set { SetValue(MustBeIntegerProperty, value); }
		}
		public static readonly DependencyProperty MustBeIntegerProperty =
			DependencyProperty.Register(nameof(MustBeInteger), typeof(bool), typeof(ValidatedNumberBox), new PropertyMetadata(false));

		#endregion

		public ValidatedNumberBox()
		{
			this.InitializeComponent();
			ViewModel = new ValidatedNumberBoxViewModel();
		}

		public double GetValue()
		{
			return NumBox.Value;
		}

		public void MarkWithError()
		{
			IsValidValue = false;
			NumBox.Style = this.FindResource("NumberBoxErrorStyle") as Style;
			InfoBadgeNumBox.Style = this.FindResource("ControlErrorInfoBadgeStyle") as Style;
			InfoBadgeNumBox.Visibility = Visibility.Visible;
		}

		public void MarkWithWarning()
		{
			NumBox.Style = this.FindResource("NumberBoxWarningStyle") as Style;
			InfoBadgeNumBox.Style = this.FindResource("ControlWarningInfoBadgeStyle") as Style;
			InfoBadgeNumBox.Visibility= Visibility.Visible;
		}

		public void ClearMarks()
		{
			IsValidValue = true;
			NumBox.Style = this.FindResource("NumberBoxDefaultStyle") as Style;
			InfoBadgeNumBox.Visibility = Visibility.Collapsed;
		}

		public void UpdateMarks()
		{
			if (!CanBeNaN && double.IsNaN(Value))
			{
				MarkWithError(); return;
			}

			if (!CanBeZero && Value == 0)
			{
				MarkWithError(); return;
			}

			if (!CanBeNegative && Value < 0)
			{
				MarkWithError(); return;
			}

			if (MustBeInteger && (int)Value != Value)
			{
				MarkWithError(); return;
			}

			ClearMarks();
		}

		private void NumBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
		{
			if (MustBeInteger && (int)ViewModel.Value != ViewModel.Value)
			{
				ViewModel.Value = (int)ViewModel.Value;
			}
			//Value = ViewModel.Value;
			UpdateMarks();
		}
	}
}
