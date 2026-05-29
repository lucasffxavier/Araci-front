using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Araci.Properties
{
    public partial class PropertiesHostView : UserControl
    {
        public static readonly RoutedEvent CloseRequestedEvent = EventManager.RegisterRoutedEvent(
            nameof(CloseRequested),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(PropertiesHostView));

        public event RoutedEventHandler CloseRequested
        {
            add => AddHandler(CloseRequestedEvent, value);
            remove => RemoveHandler(CloseRequestedEvent, value);
        }

        public PropertiesHostView()
        {
            InitializeComponent();
            AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (Keyboard.FocusedElement is not TextBox textBox)
                return;

            BindingExpression? binding = textBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null);
            Keyboard.ClearFocus();
            e.Handled = true;
        }
    }
}