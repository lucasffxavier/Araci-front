using System.Windows;
using System.Windows.Input;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Base
{
    public interface ITool
    {
        string Nome { get; }
        bool MantemBotaoAtivado { get; }
        bool IsBusy { get; }

        void Ativar();
        void Desativar();
        void Cancelar();

        void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState);
        void OnMouseMove(Point position);
        void OnMouseUp(Point position);
        void OnKeyDown(Key key);
    }
}
