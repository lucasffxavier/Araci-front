using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

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
            BarraViewModel vm = _context.ElementoFactory.CriarBarraVM();
            vm.X = 200;
            vm.Y = 100;
            _context.Commands.Execute(new AddElementoCommand(vm, _context));
        }
    }
}