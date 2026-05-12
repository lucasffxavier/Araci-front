using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCabo
{
    public class InserirCaboApplication
    {
        // =========================
        // SERVIÇO
        // =========================

        private readonly ViewportService
            _viewportService;

        // =========================
        // CONSTRUTOR
        // =========================

        public InserirCaboApplication(
            ViewportService viewportService)
        {
            _viewportService =
                viewportService;
        }

        // =========================
        // EXECUTAR
        // =========================

        public void Executar()
        {
            CaboViewModel vm =
                ElementoFactory
                .CriarCaboVM();

            _viewportService
                .AdicionarCabo(vm);
        }
    }
}