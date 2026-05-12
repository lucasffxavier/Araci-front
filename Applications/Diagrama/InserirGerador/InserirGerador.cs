using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirGerador
{
    public class InserirGeradorApplication
    {
        // =========================
        // SERVIÇO
        // =========================

        private readonly ViewportService
            _viewportService;

        // =========================
        // CONSTRUTOR
        // =========================

        public InserirGeradorApplication(
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
            GeradorViewModel vm =
                ElementoFactory
                .CriarGeradorVM();

            _viewportService
                .AdicionarElemento(vm);
        }
    }
}