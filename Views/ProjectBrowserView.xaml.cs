using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Araci.ViewModels;

namespace Araci.Views
{
    public partial class ProjectBrowserView : UserControl
    {
        public static readonly RoutedEvent CloseRequestedEvent = EventManager.RegisterRoutedEvent(
            nameof(CloseRequested),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ProjectBrowserView));

        public event RoutedEventHandler CloseRequested
        {
            add => AddHandler(CloseRequestedEvent, value);
            remove => RemoveHandler(CloseRequestedEvent, value);
        }

        public ProjectBrowserView()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
        }

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button { DataContext: ProjectBrowserItemViewModel item } &&
                item.IniciarEdicaoCommand.CanExecute(null))
            {
                item.IniciarEdicaoCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void OnRenameTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            FocarTextBoxEdicao(sender as TextBox);
        }

        private void OnRenameTextBoxIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
                FocarTextBoxEdicao(sender as TextBox);
        }

        private void OnRenameTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox { DataContext: ProjectBrowserItemViewModel item } &&
                item.IsEditing &&
                item.ConfirmarEdicaoCommand.CanExecute(null))
            {
                item.ConfirmarEdicaoCommand.Execute(null);
            }
        }

        private void OnRenameTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox { DataContext: ProjectBrowserItemViewModel item })
                return;

            if (e.Key == Key.Enter && item.ConfirmarEdicaoCommand.CanExecute(null))
            {
                item.ConfirmarEdicaoCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && item.CancelarEdicaoCommand.CanExecute(null))
            {
                item.CancelarEdicaoCommand.Execute(null);
                e.Handled = true;
            }
        }

        private static void FocarTextBoxEdicao(TextBox? textBox)
        {
            if (textBox?.DataContext is not ProjectBrowserItemViewModel { IsEditing: true })
                return;

            textBox.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                textBox.Focus();
                Keyboard.Focus(textBox);
                textBox.SelectAll();
            }));
        }
    }
}
