using Araci.Services;
using Araci.Properties;
using Araci.ViewModels;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Araci
{
    public partial class MainWindow : Window
    {
        private readonly EditorContext _context;
        private readonly GridLength _propertiesColumnWidth = new(320);

        public MainWindow()
        {
            InitializeComponent();
            _context = new EditorContext();
            UnitValueConverter.CurrentUnits = _context.Settings.Units;
            Viewport.Inicializar(_context);
            InicializarRibbon();
        }

        private void InicializarRibbon()
        {
            DataContext = _context;
            ArquivoMenu.DataContext = _context;
        }

        public void ToggleArquivoMenu(UIElement referencia)
        {
            if (ArquivoPopup.IsOpen)
            {
                ArquivoPopup.IsOpen = false;
                return;
            }

            ArquivoPopup.PlacementTarget = referencia;
            ArquivoPopup.Placement = PlacementMode.Bottom;
            ArquivoPopup.IsOpen = true;
        }

        public void FocarViewport()
        {
            Viewport.Focus();
            System.Windows.Input.Keyboard.Focus(Viewport);
        }

        public void MostrarPropriedades()
        {
            PropertiesColumn.Width = _propertiesColumnWidth;
            PropertiesHost.Visibility = Visibility.Visible;
        }

        public void MostrarConfiguracaoUnidades()
        {
            var viewModel = new UnitsSettingsViewModel(_context.Settings.Units);
            var window = new UnitsWindow(viewModel)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                viewModel.ApplyTo(_context.Settings.Units);
                _context.RefreshProperties();
            }

            FocarViewport();
        }

        private void OnPropertiesCloseRequested(object sender, RoutedEventArgs e)
        {
            PropertiesColumn.Width = new GridLength(0);
            PropertiesHost.Visibility = Visibility.Collapsed;
            FocarViewport();
        }
    }
}
