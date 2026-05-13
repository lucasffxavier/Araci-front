using System;
using System.Linq;
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
        // DRAG GLOBAL
        // =========================

        private bool
            _movendoElementos;

        private Point
            _ultimaPosicaoMouse;

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
            _ultimaPosicaoMouse = pos;

            // =========================
            // DRAG GLOBAL APENAS
            // PARA FERRAMENTA SELECIONAR
            // =========================

            if (_ferramentaAtual is SelecionarTool)
            {
                bool clicouElementoSelecionado =
                    vm != null &&
                    SelectionService
                        .Selecionados
                        .Contains(vm);

                if (clicouElementoSelecionado)
                {
                    _movendoElementos = true;
                }
            }

            // =========================
            // DISPATCH TOOL
            // =========================

            _ferramentaAtual
                .OnMouseDown(vm, pos);
        }

        // =========================
        // MOUSE MOVE
        // =========================

        public void HandleMouseMove(
            Point pos)
        {
            // =========================
            // MOVIMENTO GLOBAL
            // SOMENTE SELEÇÃO
            // =========================

            if (_movendoElementos &&
                _ferramentaAtual is SelecionarTool)
            {
                Vector delta =
                    pos - _ultimaPosicaoMouse;

                if (delta.X != 0 ||
                    delta.Y != 0)
                {
                    foreach (var item in
                        SelectionService
                            .Selecionados
                            .ToList())
                    {
                        MoveService.MoverVisual(
                            item,
                            delta);
                    }
                }

                _ultimaPosicaoMouse = pos;
            }

            // =========================
            // DISPATCH TOOL
            // =========================

            _ferramentaAtual
                .OnMouseMove(pos);
        }

        // =========================
        // MOUSE UP
        // =========================

        public void HandleMouseUp(
            Point pos)
        {
            // =========================
            // FINALIZA DRAG GLOBAL
            // =========================

            if (_movendoElementos &&
                _ferramentaAtual is SelecionarTool)
            {
                MoveService.EndMove(
                    SelectionService
                        .Selecionados
                        .ToList());
            }

            _movendoElementos = false;

            // =========================
            // DISPATCH TOOL
            // =========================

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