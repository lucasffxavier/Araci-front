using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCarga
{
    public class InserirCargaApplication
    {
        // =========================
        // SERVIÇO
        // =========================

        private readonly ViewportService
            _viewportService;

        // =========================
        // CONSTRUTOR
        // =========================

        public InserirCargaApplication(
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
            CargaViewModel vm =
                ElementoFactory
                .CriarCargaVM();

            _viewportService
                .AdicionarElemento(vm);
        }
    }
}