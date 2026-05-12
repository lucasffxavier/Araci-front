using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
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

        public void OnMouseDown(ElementoViewModel? vm, Point position)
        {
            if (vm == null)
                return;

            AppServices.Viewport?.RemoverElemento(vm);
            SelectionService.Limpar();
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