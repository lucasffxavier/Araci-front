using Araci.Services;

using System.Windows;
using System.Windows.Controls.Primitives;

namespace Araci
{
    public partial class MainWindow : Window
    {
        private readonly EditorContext _context;

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
        }

        public void ToggleArquivoMenu(
            UIElement referencia)
        {
            if (ArquivoPopup.IsOpen)
            {
                ArquivoPopup.IsOpen = false;
                return;
            }

            ArquivoPopup.PlacementTarget =
                referencia;

            ArquivoPopup.Placement =
                PlacementMode.Bottom;

            ArquivoPopup.IsOpen = true;
        }
    }
}