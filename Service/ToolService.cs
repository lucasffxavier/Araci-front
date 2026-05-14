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

        private ITool
            _ferramentaAtual;

        // =========================
        // CONSTRUTOR
        // =========================

        public ToolService()
        {
            _ferramentaAtual =
                new SelecionarTool();

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

        public void AtivarFerramenta(
            ITool ferramenta)
        {
            FerramentaAtual =
                ferramenta;
        }

        // =========================
        // VOLTAR SELEÇÃO
        // =========================

        public void VoltarParaSelecao()
        {
            if (_ferramentaAtual
                is SelecionarTool)
            {
                return;
            }

            FerramentaAtual =
                new SelecionarTool();
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

        public void HandleMouseDown(
            ElementoViewModel? vm,
            Point pos)
        {
            _ferramentaAtual
                .OnMouseDown(vm, pos);
        }

        // =========================
        // MOUSE MOVE
        // =========================

        public void HandleMouseMove(
            Point pos)
        {
            _ferramentaAtual
                .OnMouseMove(pos);
        }

        // =========================
        // MOUSE UP
        // =========================

        public void HandleMouseUp(
            Point pos)
        {
            _ferramentaAtual
                .OnMouseUp(pos);
        }

        // =========================
        // KEYBOARD
        // =========================

        public void HandleKeyDown(
            Key key)
        {
            // =========================
            // UNDO
            // =========================

            if (Keyboard.Modifiers ==
                    ModifierKeys.Control &&
                key == Key.Z)
            {
                AppServices.Commands
                    .Undo();

                return;
            }

            // =========================
            // REDO
            // =========================

            if (Keyboard.Modifiers ==
                    ModifierKeys.Control &&
                key == Key.Y)
            {
                AppServices.Commands
                    .Redo();

                return;
            }

            // =========================
            // DISPATCH TOOL
            // =========================

            _ferramentaAtual
                .OnKeyDown(key);
        }
    }
}