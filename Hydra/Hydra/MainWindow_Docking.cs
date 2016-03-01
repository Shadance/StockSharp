using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Ecng.ComponentModel;
using Ecng.Serialization;
using StockSharp.Hydra.Core;
using StockSharp.Hydra.Panes;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace StockSharp.Hydra
{
    public partial class MainWindow
    {
        public ObservableCollection<PaneWindow> MyPanes { get; } = new ObservableCollection<PaneWindow>();

        public void ShowPane(IPane pane)
        {
            if (pane == null)
                throw new ArgumentNullException(nameof(pane));

            var wnd = new PaneWindow { Pane = pane };
            MyPanes.Add(wnd);
            wnd.IsActive = true;
        }

        //
        // Сводка:
        //     Возвращает первый удовлетворяющий условию элемент последовательности или значение
        //     по умолчанию, если ни одного такого элемента не найдено.
        //
        // Параметры:
        //   predicate:
        //     Функция для проверки каждого элемента на соответствие условию.
        //
        // Параметры типа:
        //   TSource:
        //     Тип TPane в последовательности PaneWindow.
        //
        // Возврат:
        //     default(TSource), если последовательность source пуста или ни один ее элемент
        //     не прошел проверку, определенную предикатом predicate; в противном случае — первый
        //     элемент последовательности source, прошедший проверку, определенную предикатом
        //     predicate.
        //
        // Исключения:
        //   T:System.ArgumentNullException:
        //     Значение параметра source или predicate — null.
        private PaneWindow _FindPaneWindow<TSource>(Func<TSource, bool> predicate)
        {
            return MyPanes.FirstOrDefault(pw => (pw?.Pane is TSource) && predicate((TSource)pw.Pane));
        }

        private TaskPane EnsureTaskPane(IHydraTask task)
        {
            var wnd = _FindPaneWindow<TaskPane>(pw => pw?.Task == task);
            if (wnd != null)
            {
                wnd.IsActive = true;
                return null;
            }
            else
                return new TaskPane { Task = task };
        }

        private void _FocusTaskPane(IHydraTask task)
        {
            var wnd = _FindPaneWindow<TaskPane>(pw => pw?.Task == task);
            if (wnd != null)
                wnd.IsActive = true;
        }

        private SettingsStorage _SavePanes()
        {
            var ss = new SettingsStorage();

            var xmlLayoutSerializer = new XmlLayoutSerializer(Docking);
            var stream = new StringWriter();
            xmlLayoutSerializer.Serialize(stream);

            ss.Add("DockingLayout", stream.ToString());
            
/*            foreach (var paneWnd in MyPanes)
            {
                var pane = paneWnd.Pane;
                if (!pane.IsValid) continue;

                var settings = pane.SaveEntire(false);
                settings.SetValue("isActive", paneWnd.IsActive);
                settings.SetValue("isVisible", paneWnd.IsVisible);
                ss.Add(paneWnd.GetDisplayName(), settings);
            }
*/
            return ss;
        }

        private void _LoadPanes(SettingsStorage panesSettings)
        {
            var settings = panesSettings.GetValue<String>("DockingLayout");
            if (settings == null) return;
            //var serializer = new XmlLayoutSerializer(Docking);
            //serializer.Deserialize(new StringReader(settings));
        }
    }
}