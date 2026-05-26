using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.ViewModels;

namespace Araci.Services
{
    public class InputRouter
    {
        private readonly EditorContext _context;

        public InputRouter(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ITool ToolAtual
        {
            get => _context.Tools.FerramentaAtual;
            set => _context.Tools.AtivarFerramenta(value);
        }

        public void MouseDown(ElementoViewModel? vm, Point position)
        {
            ToolAtual.OnMouseDown(vm, position, CapturarEstadoAtual());
        }

        public void MouseMove(Point position)
        {
            if (ToolAtual.IsBusy)
                _context.Hover.Clear();
            else
                _context.Hover.Update(position);

            ToolAtual.OnMouseMove(position);
        }

        public void MouseUp(Point position)
        {
            ToolAtual.OnMouseUp(position);
        }

        public bool KeyDown(Key key)
        {
            bool control = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (control && key == Key.Z)
            {
                _context.Commands.Undo();
                return true;
            }

            if (control && key == Key.Y)
            {
                _context.Commands.Redo();
                return true;
            }

            if (control && key == Key.C)
            {
                ClipboardService.CopiarSelecionados(_context);
                return true;
            }

            if (control && key == Key.V)
            {
                ClipboardService.Colar(_context);
                return true;
            }

            if (key == Key.Escape)
            {
                if (ToolAtual.IsBusy)
                {
                    ToolAtual.Cancelar();
                    return true;
                }

                if (ToolAtual is not Araci.Applications.Editar.Selecionar.SelecionarTool)
                {
                    _context.Tools.VoltarParaSelecao();
                    return true;
                }

                _context.Selection.Limpar();
                return true;
            }

            ToolAtual.OnKeyDown(key);
            return false;
        }

        private static ToolInputState CapturarEstadoAtual()
        {
            return new ToolInputState(
                Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
        }
    }
}
