using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCabo
{
    public class InserirCaboApplication
    {
        private readonly ViewportService
            _viewportService;

        public InserirCaboApplication(
            ViewportService viewportService)
        {
            _viewportService =
                viewportService;
        }

        public void Executar()
        {
            CaboViewModel vm =
                ElementoFactory
                    .CriarCaboVM();

            _viewportService
                .AdicionarElemento(vm);
        }
    }
}