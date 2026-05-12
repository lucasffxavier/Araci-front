using Araci.ViewModels;
using System.Windows;

namespace Araci.Applications.Commands
{
    public interface ICommandHandler
    {
        void Execute(ElementoViewModel vm, Vector? delta = null);
    }
}