using Araci.Core.Commands;
using Araci.Services;

namespace Araci.Applications.Diagrama.InserirCarga
{
    public class InserirCargaApplication
    {
        private readonly EditorContext _context;

        public InserirCargaApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            var carga = _context.ElementoFactory.CriarCarga();
            carga.PosicaoX = 100;
            carga.PosicaoY = 100;
            _context.Commands.Execute(new AddElementoCommand(carga, _context));
        }
    }
}
