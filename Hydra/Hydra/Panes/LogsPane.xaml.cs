using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ecng.Common;
using Ecng.Serialization;
using NPOI.SS.UserModel.Charts;
using StockSharp.Algo.Storages;
using StockSharp.Hydra.Controls;
using StockSharp.Hydra.Core;
using StockSharp.Localization;
using StockSharp.Xaml;

namespace StockSharp.Hydra.Panes
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class LogsPane : IPane
    {
        string IPane.Title => LocalizedStrings.Str3237;
        Uri IPane.Icon => null;
        bool IPane.IsValid => true;
        public Monitor monitorControl => MonitorControl;

        public LogsPane()
        {
            InitializeComponent();
        }

        void IPersistable.Load(SettingsStorage storage)
        {

            if (storage.ContainsKey("LogMonitor"))
                MonitorControl.Load(storage.GetValue<SettingsStorage>("LogMonitor"));
        }

        void IPersistable.Save(SettingsStorage storage)
        {
            storage.SetValue("LogMonitor", MonitorControl.Save());
        }

        void IDisposable.Dispose()
        {
            MonitorControl.DoDispose();
        }

    }
}
