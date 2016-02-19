﻿#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: StockSharp.Terminal.TerminalPublic
File: App.xaml.cs
Created: 2015, 11, 11, 3:22 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License

using System.Windows;
using System.Windows.Threading;

namespace StockSharp.Terminal
{
	public partial class App
	{
		private void ApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show(MainWindow, e.Exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}

		private void App_OnStartup(object sender, StartupEventArgs e)
		{
            DevExpress.Xpf.Docking.DockLayoutManagerParameters.DockingItemIntervalHorz = 1;
            DevExpress.Xpf.Docking.DockLayoutManagerParameters.DockingItemIntervalVert = 1;
	        DevExpress.Xpf.Docking.DockLayoutManagerParameters.DockingRootMargin = new Thickness(0);

			//DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = DevExpress.Xpf.Core.Theme.DXStyleName;
			//DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = DevExpress.Xpf.Core.Theme.Office2010BlackName;
			//DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = DevExpress.Xpf.Core.Theme.MetropolisDarkName;
		}
	}
}