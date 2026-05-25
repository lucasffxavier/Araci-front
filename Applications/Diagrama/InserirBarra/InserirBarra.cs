using Araci.Core.Commands;
using Araci.Services;

namespace Araci.Applications.Diagrama.InserirBarra
{
    public class InserirBarraApplication
    {
        private readonly EditorContext _context;

        public InserirBarraApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            var barra = _context.ElementoFactory.CriarBarra();
            barra.PosicaoX = 200;
            barra.PosicaoY = 100;
            _context.Commands.Execute(new AddElementoCommand(barra, _context));
        }
    }
}
