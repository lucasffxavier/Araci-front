using System;
using System.Collections.ObjectModel;
using System.Windows;

using Araci.Core.Viewport;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ViewportService
    {
        // =========================
        // VIEWMODEL
        // =========================

        private readonly ViewportViewModel _vm;

        // =========================
        // CONSTRUTOR
        // =========================

        public ViewportService(
            ViewportViewModel vm)
        {
            _vm = vm
                ?? throw new ArgumentNullException(nameof(vm));
        }

        // =========================
        // ELEMENTOS VISUAIS
        // =========================

        public ObservableCollection<ElementoViewModel>
            Elementos =>
                _vm.Elementos;

        // =========================
        // CAMERA
        // =========================

        public Camera Camera
        { get; }
            = new Camera();

        // =========================
        // TAMANHO
        // =========================

        public double Largura
        { get; private set; }
            = 1000;

        public double Altura
        { get; private set; }
            = 800;

        public Rect Bounds =>
            new Rect(
                0,
                0,
                Largura,
                Altura);

        public Point WorldToScreen(
            Point point)
        {
            return Camera.WorldToScreen(point);
        }

        public Point ScreenToWorld(
            Point point)
        {
            return Camera.ScreenToWorld(point);
        }

        // =========================
        // VIEWPORT
        // =========================

        public void AtualizarTamanho(
            Size size)
        {
            Largura =
                Math.Max(
                    0,
                    size.Width);

            Altura =
                Math.Max(
                    0,
                    size.Height);
        }

        // =========================
        // ELEMENTOS
        // =========================

        public void RegistrarViewModel(
            ElementoViewModel vm)
        {
            if (vm == null)
                return;

            _vm.RegistrarViewModel(vm);
        }
    }
}
