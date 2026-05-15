using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCabo
{
    public class InserirCaboApplication
    {
        public void Executar()
        {
            CaboViewModel vm =
                ElementoFactory
                    .CriarCaboVM();

            AppServices.Commands.Execute(
                new AddElementoCommand(vm));
        }
    }
}
