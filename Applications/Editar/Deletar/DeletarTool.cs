using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Deletar
{
    public class DeletarTool : ITool
    {
        public string Nome => "Deletar";

        public bool MantemBotaoAtivado => true;

        public void Ativar() { }

        public void Desativar() { }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            if (vm == null)
                return;

            var command =
                new DeleteElementCommand(vm);

            AppServices
                .Commands
                .Execute(command);
        }

        public void OnMouseMove(Point position)
        {
        }

        public void OnMouseUp(Point position)
        {
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}