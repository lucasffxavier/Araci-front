using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCabo
{
    public class InserirCaboApplication
    {
        private readonly EditorContext _context;

        public InserirCaboApplication(EditorContext context)
        {
            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            CaboViewModel vm =
                _context.ElementoFactory
                    .CriarCaboVM();

            _context.Commands.Execute(
                new AddElementoCommand(
                    vm,
                    _context));
        }
    }
}
