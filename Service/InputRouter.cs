using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Mover;
using Araci.Applications.Editar.Selecionar;
using Araci.ViewModels;

namespace Araci.Services
{
    public class InputRouter
    {
        private readonly EditorContext _context;
        private string _shortcutBuffer = string.Empty;
        private DateTime _lastShortcutKeyTime = DateTime.MinValue;
        private static readonly TimeSpan ShortcutTimeout = TimeSpan.FromSeconds(1);

        public InputRouter(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Point UltimaPosicaoMouseMundo { get; private set; }
        public bool PossuiUltimaPosicaoMouseMundo { get; private set; }

        public ITool ToolAtual
        {
            get => _context.Tools.FerramentaAtual;
            set => _context.Tools.AtivarFerramenta(value);
        }

        public void MouseDown(ElementoViewModel? vm, Point position)
        {
            AtualizarUltimaPosicao(position);
            ToolAtual.OnMouseDown(vm, position, CapturarEstadoAtual(position));
        }

        public void MouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            AtualizarUltimaPosicao(position);
            ToolAtual.OnMouseDown(vm, position, inputState);
        }

        public void MouseMove(Point position)
        {
            MouseMove(position, CapturarEstadoAtual(position));
        }

        public void MouseMove(Point position, ToolInputState inputState)
        {
            AtualizarUltimaPosicao(position);

            if (ToolAtual.IsBusy)
                _context.Hover.Clear();
            else
                _context.Hover.Update(position);

            ToolAtual.OnMouseMove(position, inputState);
        }

        public void MouseUp(Point position)
        {
            MouseUp(position, CapturarEstadoAtual(position));
        }

        public void MouseUp(Point position, ToolInputState inputState)
        {
            AtualizarUltimaPosicao(position);
            ToolAtual.OnMouseUp(position, inputState);
        }

        public bool KeyDown(Key key)
        {
            bool control = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (control && key == Key.Z)
            {
                LimparAtalho();
                _context.Commands.Undo();
                return true;
            }

            if (control && key == Key.Y)
            {
                LimparAtalho();
                _context.Commands.Redo();
                return true;
            }

            if (control && key == Key.C)
            {
                LimparAtalho();
                ClipboardService.CopiarSelecionados(_context);
                return true;
            }

            if (control && key == Key.V)
            {
                LimparAtalho();
                ClipboardService.Colar(_context);
                return true;
            }

            if (key == Key.Delete)
            {
                LimparAtalho();
                _context.SafeDelete.DeleteActiveHandleOrSelection();
                return true;
            }

            if (key == Key.Escape)
            {
                LimparAtalho();
                return HandleEscape();
            }

            if (key == Key.Space)
            {
                LimparAtalho();

                if (ToolAtual.IsBusy || ToolAtual.HandlesKey(key) || _context.Selection.Selecionados.Any(RotationService.PodeRotacionar))
                {
                    ToolAtual.OnKeyDown(key);
                    return true;
                }

                return false;
            }

            if (TryHandleTwoKeyShortcut(key))
                return true;

            ToolAtual.OnKeyDown(key);
            return false;
        }

        private bool TryHandleTwoKeyShortcut(Key key)
        {
            if (Keyboard.Modifiers != ModifierKeys.None)
            {
                LimparAtalho();
                return false;
            }

            if (Keyboard.FocusedElement is TextBox)
            {
                LimparAtalho();
                return false;
            }

            char? c = KeyToChar(key);

            if (c == null)
                return false;

            DateTime now = DateTime.Now;

            if (now - _lastShortcutKeyTime > ShortcutTimeout)
                _shortcutBuffer = string.Empty;

            _lastShortcutKeyTime = now;
            _shortcutBuffer = (_shortcutBuffer + c.Value).ToUpperInvariant();

            if (_shortcutBuffer.Length > 2)
                _shortcutBuffer = _shortcutBuffer[^2..];

            if (_shortcutBuffer.Length < 2)
                return true;

            bool handled = ExecutarAtalho(_shortcutBuffer);
            LimparAtalho();
            return handled;
        }

        private bool ExecutarAtalho(string shortcut)
        {
            ElementDefinition? definition = _context.Elements.FindByShortcut(shortcut);

            if (definition != null)
                return _context.Tools.AtivarInsercaoElemento(definition.Kind);

            switch (shortcut)
            {
                case "SE":
                    _context.Tools.VoltarParaSelecao();
                    return true;
                case "MV":
                    _context.Tools.AtivarFerramenta(new MoverTool(_context));
                    return true;
                default:
                    return false;
            }
        }

        private static char? KeyToChar(Key key)
        {
            if (key >= Key.A && key <= Key.Z)
                return (char)('A' + (key - Key.A));

            return null;
        }

        private void LimparAtalho()
        {
            _shortcutBuffer = string.Empty;
            _lastShortcutKeyTime = DateTime.MinValue;
        }

        private bool HandleEscape()
        {
            if (ToolAtual is SelecionarTool)
            {
                if (ToolAtual.IsBusy)
                    ToolAtual.Cancelar();
                else
                    _context.Selection.Limpar();

                return true;
            }

            _context.Tools.VoltarParaSelecao();
            return true;
        }

        private void AtualizarUltimaPosicao(Point position)
        {
            UltimaPosicaoMouseMundo = position;
            PossuiUltimaPosicaoMouseMundo = true;
        }

        private static ToolInputState CapturarEstadoAtual(Point worldPosition)
        {
            return new ToolInputState(Keyboard.Modifiers, null, 0, worldPosition, default);
        }
    }
}