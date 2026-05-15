using System;
using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ToolService
    {
        // =========================
        // EVENTOS
        // =========================

        public event Action<ITool>?
            FerramentaAlterada;

        // =========================
        // ESTADO
        // =========================

        private ITool _ferramentaAtual;

        private readonly EditorContext _context;

        // =========================
        // CONSTRUTOR
        // =========================

        public ToolService()
            : this(AppServices.Current)
        {
        }

        public ToolService(EditorContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));

            _ferramentaAtual =
                new SelecionarTool(_context);

            _ferramentaAtual.Ativar();
        }

        // =========================
        // TOOL ATUAL
        // =========================

        public ITool FerramentaAtual
        {
            get => _ferramentaAtual;

            set
            {
                if (_ferramentaAtual == value)
                    return;

                _ferramentaAtual.Desativar();

                _ferramentaAtual = value;

                _ferramentaAtual.Ativar();

                FerramentaAlterada?.Invoke(
                    _ferramentaAtual);
            }
        }

        // =========================
        // ATIVAR
        // =========================

        public void AtivarFerramenta(ITool ferramenta)
        {
            FerramentaAtual = ferramenta;
        }

        // =========================
        // VOLTAR SELEÇÃO
        // =========================

        public void VoltarParaSelecao()
        {
            if (_ferramentaAtual is SelecionarTool)
                return;

            FerramentaAtual =
                new SelecionarTool(_context);
        }

        // =========================
        // TOOLBAR
        // =========================

        public bool FerramentaAtivaMantida()
        {
            return FerramentaAtual
                .MantemBotaoAtivado;
        }

        // =========================
        // MOUSE DOWN
        // =========================

        public void HandleMouseDown(ElementoViewModel? vm, Point pos)
        {
            _ferramentaAtual.OnMouseDown(vm, pos);
        }

        // =========================
        // MOUSE MOVE
        // =========================

        public void HandleMouseMove(Point pos)
        {
            _ferramentaAtual.OnMouseMove(pos);
        }

        // =========================
        // MOUSE UP
        // =========================

        public void HandleMouseUp(Point pos)
        {
            _ferramentaAtual.OnMouseUp(pos);
        }

        // =========================
        // KEYBOARD
        // =========================

        public void HandleKeyDown(Key key)
        {
            // =========================
            // UNDO
            // =========================

            if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.Z)
            {
                _context.Commands.Undo();

                return;
            }

            // =========================
            // REDO
            // =========================

            if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.Y)
            {
                _context.Commands.Redo();

                return;
            }

            // =========================
            // COPY
            // =========================

            if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.C)
            {
                ClipboardService.CopiarSelecionados(_context);

                return;
            }

            // =========================
            // PASTE
            // =========================

            if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.V)
            {
                ClipboardService.Colar(_context);

                return;
            }

            // =========================
            // DISPATCH TOOL
            // =========================

            _ferramentaAtual.OnKeyDown(key);
        }
    }
}
