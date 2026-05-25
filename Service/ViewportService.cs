using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Araci.Core.Viewport;
using Araci.Models;
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

        public Point CentroTela => new(Largura / 2, Altura / 2);

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

        public void Pan(Vector delta)
        {
            Camera.Pan(delta);
        }

        public void ZoomInAt(Point screenPoint)
        {
            Camera.ZoomInAt(screenPoint);
        }

        public void ZoomOutAt(Point screenPoint)
        {
            Camera.ZoomOutAt(screenPoint);
        }

        public void ZoomInAtCenter()
        {
            Camera.ZoomInAt(CentroTela);
        }

        public void ZoomOutAtCenter()
        {
            Camera.ZoomOutAt(CentroTela);
        }

        public void ResetCamera()
        {
            Camera.Reset();
        }

        public void Zoom100AtCenter()
        {
            Camera.Zoom100At(CentroTela);
        }

        public void ZoomExtents(double margem = 40)
        {
            var boundsValidos = Elementos
                .Select(e => e.Bounds)
                .Where(b => !b.IsEmpty)
                .ToList();

            if (boundsValidos.Count == 0)
            {
                Camera.Reset();
                return;
            }

            Rect total = boundsValidos[0];

            for (int i = 1; i < boundsValidos.Count; i++)
                total.Union(boundsValidos[i]);

            Camera.Fit(total, new Size(Largura, Altura), margem);
        }

        public void RegistrarViewModel(ElementoViewModel vm)
        {
            if (vm == null)
                return;

            _vm.RegistrarViewModel(vm);
        }

        public ElementoViewModel? ObterViewModel(Elemento modelo)
        {
            return _vm.ObterViewModel(modelo);
        }

        public void AtualizarViewModel(Elemento modelo)
        {
            _vm.AtualizarViewModel(modelo);
        }
    }
}
