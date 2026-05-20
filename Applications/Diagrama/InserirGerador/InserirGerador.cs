using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

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
            GeradorViewModel vm = _context.ElementoFactory.CriarGeradorVM();
            vm.X = 150;
            vm.Y = 150;
            _context.Commands.Execute(new AddElementoCommand(vm, _context));
        }
    }
}