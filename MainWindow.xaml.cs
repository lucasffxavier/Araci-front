using Araci.Services;
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

        private void OnPropertiesCloseRequested(object sender, RoutedEventArgs e)
        {
            PropertiesColumn.Width = new GridLength(0);
            PropertiesHost.Visibility = Visibility.Collapsed;
            FocarViewport();
        }
    }
}