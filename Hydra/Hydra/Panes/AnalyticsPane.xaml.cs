﻿#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: StockSharp.Hydra.Panes.HydraPublic
File: AnalyticsPane.xaml.cs
Created: 2015, 11, 11, 2:32 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License
namespace StockSharp.Hydra.Panes
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Windows.Controls;
	using System.Windows.Input;

	using Ecng.Xaml.Charting;
	using Ecng.Xaml.Charting.ChartModifiers;
	using Ecng.Xaml.Charting.Visuals;
	using Ecng.Collections;
	using Ecng.Common;
	using Ecng.ComponentModel;
	using Ecng.Configuration;
	using Ecng.Serialization;
	using Ecng.Xaml;
	using Ecng.Xaml.Grids;

	using Ookii.Dialogs.Wpf;

	using StockSharp.Algo;
	using StockSharp.Algo.Storages;
	using StockSharp.Algo.Strategies.Analytics;
	using StockSharp.BusinessEntities;
	using StockSharp.Hydra.Core;
	using StockSharp.Logging;
	using StockSharp.Algo.Strategies;
	using StockSharp.Localization;
	using StockSharp.Xaml.Code;

	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	public partial class AnalyticsPane : IPane
	{
		private class ScriptTemplate
		{
			public string Title { get; set; }

			public string Body { get; set; }
		}

		[DisplayNameLoc(LocalizedStrings.Str225Key)]
		[DescriptionLoc(LocalizedStrings.Str2836Key)]
		private class AnalyticsScriptParameters : NotifiableObject, IPersistable
		{
			private Security _security;
			private DateTime _from;
			private DateTime _to;
			private IMarketDataDrive _drive;
			private StorageFormats _storageFormat;

			public AnalyticsScriptParameters()
			{
				To = DateTime.MaxValue;
			}

			[DisplayNameLoc(LocalizedStrings.SecurityKey)]
			[DescriptionLoc(LocalizedStrings.SecurityKey, true)]
			[CategoryLoc(LocalizedStrings.GeneralKey)]
			[PropertyOrder(0)]
			public Security Security
			{
				get { return _security; }
				set
				{
					_security = value;
					NotifyChanged(nameof(Security));
				}
			}

			[DisplayNameLoc(LocalizedStrings.Str343Key)]
			[DescriptionLoc(LocalizedStrings.Str1222Key)]
			[CategoryLoc(LocalizedStrings.GeneralKey)]
			[PropertyOrder(1)]
			public DateTime From
			{
				get { return _from; }
				set
				{
					_from = value;
					NotifyChanged(nameof(From));
				}
			}

			[DisplayNameLoc(LocalizedStrings.Str345Key)]
			[DescriptionLoc(LocalizedStrings.Str418Key, true)]
			[CategoryLoc(LocalizedStrings.GeneralKey)]
			[PropertyOrder(2)]
			public DateTime To
			{
				get { return _to; }
				set
				{
					_to = value;
					NotifyChanged(nameof(To));
				}
			}

			[DisplayNameLoc(LocalizedStrings.Str2804Key)]
			[DescriptionLoc(LocalizedStrings.Str2838Key)]
			[CategoryLoc(LocalizedStrings.GeneralKey)]
			[Editor(typeof(DriveComboBoxEditor), typeof(DriveComboBoxEditor))]
			[PropertyOrder(3)]
			public IMarketDataDrive Drive
			{
				get { return _drive; }
				set
				{
					_drive = value;
					NotifyChanged(nameof(Drive));
				}
			}

			[DisplayNameLoc(LocalizedStrings.Str2239Key)]
			[DescriptionLoc(LocalizedStrings.Str2240Key)]
			[CategoryLoc(LocalizedStrings.GeneralKey)]
			[PropertyOrder(4)]
			public StorageFormats StorageFormat
			{
				get { return _storageFormat; }
				set
				{
					_storageFormat = value;
					NotifyChanged(nameof(StorageFormat));
				}
			}

			public void Load(SettingsStorage storage)
			{
				if (storage.ContainsKey(nameof(Security)))
					Security = ConfigManager.GetService<IEntityRegistry>().Securities.ReadById(storage.GetValue<string>("Security"));

				From = storage.GetValue<DateTime>(nameof(From));
				To = storage.GetValue<DateTime>(nameof(To));

				if (storage.ContainsKey(nameof(Drive)))
					Drive = DriveCache.Instance.GetDrive(storage.GetValue<string>(nameof(Drive)));

				StorageFormat = storage.GetValue<StorageFormats>(nameof(StorageFormat));
			}

			public void Save(SettingsStorage storage)
			{
				if (Security != null)
					storage.SetValue(nameof(Security), Security.Id);

				storage.SetValue(nameof(From), From);
				storage.SetValue(nameof(To), To);

				if (Drive != null)
					storage.SetValue(nameof(Drive), Drive.Path);

				storage.SetValue(nameof(StorageFormat), StorageFormat.To<string>());
			}
		}

		public static RoutedCommand SaveCommand = new RoutedCommand();

		public static RoutedCommand LoadCommand = new RoutedCommand();

		public static RoutedCommand StartCommand = new RoutedCommand();

		public static RoutedCommand StopCommand = new RoutedCommand();

		private readonly AnalyticsScriptParameters _parameters = new AnalyticsScriptParameters();
		private Type _scriptType;
		private BaseAnalyticsStrategy _analyticStrategy;
		private static IEnumerable<CodeReference> _refs;

		public AnalyticsPane()
		{
			InitializeComponent();
		
			PropertyGrid.SelectedObject = _parameters;

			if (_refs == null)
			{
				_refs = CodeExtensions
					.DefaultReferences
					.Where(s => !s.CompareIgnoreCase("StockSharp.Xaml.Diagram"))
					.Concat(new[] { typeof(UltrachartSurface).Assembly.GetName().Name, "Hydra" })
					.ToReferences();	
			}

			CodePanel.References?.AddRange(_refs);

			Templates.ItemsSource = new[]
			{
				new ScriptTemplate { Title = LocalizedStrings.Str2839, Body = Properties.Resources.DailyHighestVolumeStrategy },
				new ScriptTemplate { Title = LocalizedStrings.Str2840, Body = Properties.Resources.PriceVolumeDistributionStrategy }
			};

			Templates.SelectedIndex = 0;
		}

		private string Code
		{
			get { return CodePanel.Code; }
			set { CodePanel.Code = value; }
		}

		void IPersistable.Load(SettingsStorage storage)
		{
			Code = storage.GetValue<string>("Code");
			CodePanel.References.Clear();
			CodePanel.References.AddRange(storage.GetValue<CodeReference[]>("References"));
			CodePanel.Load(storage.GetValue<SettingsStorage>("CodePanel"));
			//ResultChart.Load(storage.GetValue<SettingsStorage>("ResultChart"));
			//ResultGrid.Load(storage.GetValue<SettingsStorage>("ResultGrid"));
			_parameters.Load(storage.GetValue<SettingsStorage>("Parameters"));

			CodePanel_OnCompilingCode();
		}

		void IPersistable.Save(SettingsStorage storage)
		{
			storage.SetValue("Code", Code);
			storage.SetValue("References", CodePanel.References.ToArray());
			storage.SetValue("CodePanel", CodePanel.Save());
			//storage.SetValue("ResultChart", ResultChart.Save());
			//storage.SetValue("ResultGrid", ResultGrid.Save());
			storage.SetValue("Parameters", _parameters.Save());
		}

		string IPane.Title => LocalizedStrings.Str1221;

		Uri IPane.Icon => null;

		bool IPane.IsValid => true;

		private void CodePanel_OnCompilingCode()
		{
			var result = CompilationLanguages.CSharp.CompileCode(Code, Guid.NewGuid().ToString(), CodePanel.References,
				UserConfig.Instance.AnalyticsDir, UserConfig.Instance.AnalyticsDir);

			CodePanel.ShowCompilationResult(result, _analyticStrategy != null);

			if (result.HasErrors())
				return;

			_scriptType = result.Assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && t.IsSubclassOf(typeof(Strategy)));
		}

		private void ExecutedSaveCommand(object sender, ExecutedRoutedEventArgs e)
		{
			var dlg = new VistaSaveFileDialog
			{
				RestoreDirectory = true,
				Filter = @"csharp files (*.cs)|*.cs|All files (*.*)|*.*",
				DefaultExt = "cs"
			};

			if (dlg.ShowDialog(this.GetWindow()) == true)
			{
				File.WriteAllText(dlg.FileName, CodePanel.Code);
			}
		}

		private void CanExecuteSaveCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CodePanel != null && !CodePanel.Code.IsEmpty();
		}

		private void ExecutedLoadCommand(object sender, ExecutedRoutedEventArgs e)
		{
			var dlg = new VistaOpenFileDialog
			{
				RestoreDirectory = true,
				Filter = @"csharp files (*.cs)|*.cs|All files (*.*)|*.*"
			};

			if (dlg.ShowDialog(this.GetWindow()) == true)
			{
				CodePanel.Code = File.ReadAllText(dlg.FileName);
			}
		}

		private void ExecutedStartCommand(object sender, ExecutedRoutedEventArgs e)
		{
			var chart = new UltrachartSurface
			{
				ChartModifier = new ModifierGroup(
					new RubberBandXyZoomModifier { ExecuteOn = ExecuteOn.MouseLeftButton },
					new ZoomExtentsModifier { ExecuteOn = ExecuteOn.MouseDoubleClick }
				)
			};
			ThemeManager.SetTheme(chart, "Chrome");
			ChartPanel.Content = chart;

			var grid = new UniversalGrid();
			GridPanel.Content = grid;

			try
			{
				_analyticStrategy = _scriptType.CreateInstance<BaseAnalyticsStrategy>();
				_analyticStrategy.ProcessStateChanged += s =>
				{
					if (_analyticStrategy != null && _analyticStrategy.ProcessState == ProcessStates.Stopped)
					{
						//_isProgress = false;
						_analyticStrategy = null;
					}
				};

				//_isProgress = true;
				_analyticStrategy.Security = _parameters.Security;
				_analyticStrategy.From = _parameters.From;
				_analyticStrategy.To = _parameters.To;
				_analyticStrategy.Environment.SetValue("Drive", _parameters.Drive);
				_analyticStrategy.Environment.SetValue("StorageFormat", _parameters.StorageFormat);
				_analyticStrategy.Environment.SetValue("Chart", chart);
				_analyticStrategy.Environment.SetValue("Grid", grid);
				_analyticStrategy.Start();
			}
			catch (Exception ex)
			{
				ex.LogError();
				//_isProgress = false;
				_analyticStrategy = null;
			}
		}

		private void CanExecuteStartCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = _scriptType != null;
		}

		private void ExecutedStopCommand(object sender, ExecutedRoutedEventArgs e)
		{
			var s = _analyticStrategy;

			if (s != null)
				s.Stop();
		}

		private void CanExecuteStopCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = _analyticStrategy != null;
		}

		private void Templates_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var template = (ScriptTemplate)Templates.SelectedItem;

			if (template == null)
				return;

			Code = template.Body;
			CodePanel_OnCompilingCode();
		}

		void IDisposable.Dispose()
		{
			ExecutedStopCommand(null, null);
		}
	}
}