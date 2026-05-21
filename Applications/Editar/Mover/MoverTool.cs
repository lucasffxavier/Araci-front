using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Mover
{
    public class MoverTool : ITool
    {
        private readonly SelecionarTool _selecionarTool;

        public MoverTool(EditorContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _selecionarTool = new SelecionarTool(
                context,
                modoSoMover: true,
                mostrarHud: true);
        }

        public string Nome => "Mover";
        public bool MantemBotaoAtivado => true;

        public void Ativar()
        {
            _selecionarTool.Ativar();
        }

        public void Desativar()
        {
            _selecionarTool.Desativar();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            _selecionarTool.OnMouseDown(vm, position, inputState);
        }

        public void OnMouseMove(Point position)
        {
            _selecionarTool.OnMouseMove(position);
        }

        public void OnMouseUp(Point position)
        {
            _selecionarTool.OnMouseUp(position);
        }

        public void OnKeyDown(Key key)
        {
            _selecionarTool.OnKeyDown(key);
        }
    }
}