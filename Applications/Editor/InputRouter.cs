using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editor
{
    public class InputRouter
    {
        private readonly ToolService _tools;
        private readonly ICommandHistory _commands;
        private readonly ISafeDeleteService _safeDelete;
        private readonly ISelectionService _selection;
        private readonly IElementCatalog _elements;
        private readonly IHoverService _hover;
        private readonly Action _copiarSelecionados;
        private readonly Action _colar;
        private string _shortcutBuffer = string.Empty;
        private DateTime _lastShortcutKeyTime = DateTime.MinValue;
        private static readonly TimeSpan ShortcutTimeout = TimeSpan.FromSeconds(1);

        public InputRouter(
            ToolService tools,
            ICommandHistory commands,
            ISafeDeleteService safeDelete,
            ISelectionService selection,
            IElementCatalog elements,
            IHoverService hover,
            Action copiarSelecionados,
            Action colar)
        {
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _safeDelete = safeDelete ?? throw new ArgumentNullException(nameof(safeDelete));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));
            _hover = hover ?? throw new ArgumentNullException(nameof(hover));
            _copiarSelecionados = copiarSelecionados ?? throw new ArgumentNullException(nameof(copiarSelecionados));
            _colar = colar ?? throw new ArgumentNullException(nameof(colar));
        }

        public Point UltimaPosicaoMouseMundo { get; private set; }
        public bool PossuiUltimaPosicaoMouseMundo { get; private set; }

        public ITool ToolAtual
        {
            get => _tools.FerramentaAtual;
            set => _tools.AtivarFerramenta(value);
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
                _hover.Clear();
            else
                _hover.Update(position);

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
                _commands.Undo();
                return true;
            }

            if (control && key == Key.Y)
            {
                LimparAtalho();
                _commands.Redo();
                return true;
            }

            if (control && key == Key.C)
            {
                LimparAtalho();
                _copiarSelecionados();
                return true;
            }

            if (control && key == Key.V)
            {
                LimparAtalho();
                _colar();
                return true;
            }

            if (key == Key.Delete)
            {
                LimparAtalho();
                _safeDelete.DeleteActiveHandleOrSelection();
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

                if (ToolAtual.IsBusy || ToolAtual.HandlesKey(key) || _selection.Selecionados.Any(RotationService.PodeRotacionar))
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
            ElementDefinition? definition = _elements.FindByShortcut(shortcut);

            if (definition != null)
                return _tools.AtivarInsercaoElemento(definition.Kind);

            switch (shortcut)
            {
                case "SE":
                    _tools.VoltarParaSelecao();
                    return true;
                case "MV":
                    _tools.AtivarMover();
                    return true;
                case "AL":
                    _tools.AtivarAlinhar();
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
                    _selection.Limpar();

                return true;
            }

            _tools.VoltarParaSelecao();
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
