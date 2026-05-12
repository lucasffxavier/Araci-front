using System.Windows;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ViewportService
    {
        private readonly ViewportViewModel _vm;

        public ViewportService(ViewportViewModel vm)
        {
            _vm = vm;
        }

        public double Largura { get; set; } = 1000;
        public double Altura { get; set; } = 800;

        public void AtualizarTamanho(Size size)
        {
            // 🔥 remove borda visual (1px * 2)
            Largura = size.Width;
            Altura = size.Height;
        }

        // 🔥 RESTAURADO
        public void AdicionarElemento(ElementoViewModel vm)
        {
            _vm.Document.Elementos.Add(vm);
        }

        // 🔥 RESTAURADO
        public void AdicionarCabo(CaboViewModel vm)
        {
            _vm.Document.Elementos.Add(vm);
        }

        public void RemoverElemento(ElementoViewModel vm)
        {
            _vm.Document.Elementos.Remove(vm);
        }
    }
}