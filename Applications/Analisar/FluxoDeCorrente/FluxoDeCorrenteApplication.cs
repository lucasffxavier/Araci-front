using System.Collections.Generic;
using System.Linq;
using Araci.API;
using Araci.Maestro;
using Araci.Models;
using Araci.Services;

namespace Araci.Applications.Analisar.FluxoDeCorrente
{
    public class FluxoDeCorrenteApplication
    {
        private readonly EditorContext _context;

        public FluxoDeCorrenteApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            CoreApi api = new(_context);
            CoreMaestro maestro = new(api);

            IList<Elemento> elementos = maestro.ObterElementos()
                .Where(elemento => elemento.PossuiParametro(Elemento.PARAM_NOME))
                .ToList();

            IList<string> nomes = maestro.ObterValoresParametroTexto(
                elementos,
                Elemento.PARAM_NOME);

            string mensagem = nomes.Count == 0
                ? "Nenhum elemento encontrado no projeto."
                : string.Join(System.Environment.NewLine, nomes);

            maestro.JanelaMensagem("Fluxo de corrente", mensagem);
        }
    }
}
