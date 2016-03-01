﻿#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: StockSharp.Hydra.Panes.HydraPublic
File: PaneWindow.xaml.cs
Created: 2015, 11, 11, 2:32 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License

using Ecng.Serialization;

namespace StockSharp.Hydra.Panes
{
	public partial class PaneWindow
	{
        public PaneWindow()
		{
			InitializeComponent();
		}

        private IPane _dataContext;
        public IPane Pane
		{
			get { return (IPane)_dataContext; }
			set { _dataContext = value; }
		}
    }
}