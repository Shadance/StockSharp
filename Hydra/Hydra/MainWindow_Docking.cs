using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ecng.Common;
using Ecng.ComponentModel;
using Ecng.Serialization;
using StockSharp.Hydra.Core;
using StockSharp.Hydra.Panes;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace StockSharp.Hydra
{
    public partial class MainWindow
    {
        /// <summary>
        /// Содержит все модальные окна типа PaneWindow (LayoutDocument)
        /// </summary>
//        public static ObservableCollection<LayoutDocument> MyPanes { get; } = new ObservableCollection<LayoutDocument>();

        public LayoutDocument ShowPane(IPane pane)
        {
            if (pane == null)
                throw new ArgumentNullException(nameof(pane));

            var wnd = new LayoutDocument
            {
                Title = pane.Title,
                ToolTip = pane.Title,
                Description = nameof(pane) + " " + pane.Title,
                CanClose = true,
                CanFloat = true
            };

            if (!pane.Icon.IsNull())
            {
                // Create the source
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = pane.Icon;
                img.EndInit();
                wnd.IconSource = img;
            }

            wnd.Content = pane;

            DocumentPane.Children.Add(wnd);
            wnd.IsActive = true;
            return wnd;
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
        private LayoutContent _FindPane<TSource>(Func<TSource, bool> predicate)
        {
            return DocumentPane.Children.FirstOrDefault(pw => (pw?.Content is TSource) && predicate( (TSource)pw.Content ));
        }

        private TaskPane EnsureTaskPane(IHydraTask task)
        {
            var wnd = _FindPane<TaskPane>(pw => pw?.Task == task);
            if (wnd != null)
            {
                wnd.IsActive = true;
                return null;
            }

            return new TaskPane { Task = task };
        }

        private void _FocusTaskPane(IHydraTask task)
        {
            var wnd = _FindPane<TaskPane>(pw => pw?.Task == task);
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
            //var settings = panesSettings.GetValue<String>("DockingLayout");
            //if (settings == null) return;
            //var serializer = new XmlLayoutSerializer(Docking);
            //serializer.Deserialize(new StringReader(settings));
        }
    }
}