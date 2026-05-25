using Araci.Core.Commands;
using Araci.Services;

namespace Araci.Applications.Diagrama.InserirGerador
{
    public class InserirGeradorApplication
    {
        private readonly EditorContext _context;

        public InserirGeradorApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            var gerador = _context.ElementoFactory.CriarGerador();
            gerador.PosicaoX = 150;
            gerador.PosicaoY = 150;
            _context.Commands.Execute(new AddElementoCommand(gerador, _context));
        }
    }
}
