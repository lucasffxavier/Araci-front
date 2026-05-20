using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

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
            CargaViewModel vm = _context.ElementoFactory.CriarCargaVM();
            vm.X = 100;
            vm.Y = 100;
            _context.Commands.Execute(new AddElementoCommand(vm, _context));
        }
    }
}