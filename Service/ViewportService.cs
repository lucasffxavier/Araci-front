using System.Collections.ObjectModel;
using System.Windows;
using Araci.Core.Viewport;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ViewportService
    {
        private readonly ViewportViewModel _vm;

        public ViewportService(ViewportViewModel vm)
        {
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        }

        public ObservableCollection<ElementoViewModel> Elementos => _vm.Elementos;

        public Camera Camera { get; } = new();

        public double Largura { get; private set; } = 1000;
        public double Altura { get; private set; } = 800;

        public Rect Bounds => new(0, 0, Largura, Altura);

        public Point WorldToScreen(Point point)
        {
            return Camera.WorldToScreen(point);
        }

        public Point ScreenToWorld(Point point)
        {
            return Camera.ScreenToWorld(point);
        }

        public void AtualizarTamanho(Size size)
        {
            Largura = Math.Max(0, size.Width);
            Altura = Math.Max(0, size.Height);
        }

        public void RegistrarViewModel(ElementoViewModel vm)
        {
            if (vm == null)
                return;

            _vm.RegistrarViewModel(vm);
        }
    }
}