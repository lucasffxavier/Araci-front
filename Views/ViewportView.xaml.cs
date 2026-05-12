using Araci.Applications.Editar.Selecionar;
using Araci.Services;
using Araci.ViewModels;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Araci.Controls;

namespace Araci.Views
{
    public partial class ViewportView : UserControl
    {
        private readonly ViewportViewModel _viewportViewModel;

        public ViewportView()
        {
            InitializeComponent();

            _viewportViewModel = new ViewportViewModel();

            DataContext = _viewportViewModel;

            AppServices.Viewport =
                new ViewportService(_viewportViewModel);

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            DependencyObject? origem = e.OriginalSource as DependencyObject;

            while (origem != null)
            {
                if (origem is CaboControl
                    || origem is CargaControl
                    || origem is GeradorControl)
                {
                    return;
                }

                origem = VisualTreeHelper.GetParent(origem);
            }

            if (AppServices.Tools.FerramentaAtual is SelecionarTool)
            {
                SelectionService.Limpar();
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                AppServices.Tools.VoltarParaSelecao();
                SelectionService.Limpar();
                e.Handled = true;
            }
        }
    }
}