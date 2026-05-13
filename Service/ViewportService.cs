using System.Windows;

using Araci.ViewModels;

namespace Araci.Services
{
    public class ViewportService
    {
        private readonly ViewportViewModel _vm;

        public ViewportService(
            ViewportViewModel vm)
        {
            _vm = vm;
        }

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

        public void AtualizarTamanho(
            Size size)
        {
            Largura =
                Math.Max(0, size.Width);

            Altura =
                Math.Max(0, size.Height);
        }

        public void AdicionarElemento(
            ElementoViewModel vm)
        {
            if (vm == null)
                return;

            if (!_vm.Document.Elementos.Contains(vm))
            {
                _vm.Document.Elementos.Add(vm);
            }
        }

        public void AdicionarCabo(
            CaboViewModel vm)
        {
            AdicionarElemento(vm);
        }

        public void RemoverElemento(
            ElementoViewModel vm)
        {
            if (vm == null)
                return;

            if (_vm.Document.Elementos.Contains(vm))
            {
                _vm.Document.Elementos.Remove(vm);
            }
        }
    }
}