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
        public event Action<ITool>? FerramentaAlterada;

        private ITool _ferramentaAtual;

        public ToolService()
        {
            _ferramentaAtual = new SelecionarTool();
            _ferramentaAtual.Ativar();
        }

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

                FerramentaAlterada?.Invoke(_ferramentaAtual);
            }
        }

        public void AtivarFerramenta(ITool ferramenta)
        {
            FerramentaAtual = ferramenta;
        }

        public void VoltarParaSelecao()
        {
            if (_ferramentaAtual is SelecionarTool)
                return;

            FerramentaAtual = new SelecionarTool();
        }

        public bool FerramentaAtivaMantida()
        {
            return FerramentaAtual.MantemBotaoAtivado;
        }

        // 🔥 NOVO — DISPATCH

        public void HandleMouseDown(ElementoViewModel? vm, Point pos)
        {
            _ferramentaAtual.OnMouseDown(vm, pos);
        }

        public void HandleMouseMove(Point pos)
        {
            _ferramentaAtual.OnMouseMove(pos);
        }

        public void HandleMouseUp(Point pos)
        {
            _ferramentaAtual.OnMouseUp(pos);
        }

        public void HandleKeyDown(Key key)
        {
            // =========================
            // UNDO
            // =========================

            if (Keyboard.Modifiers == ModifierKeys.Control
                && key == Key.Z)
            {
                AppServices.Commands.Undo();
                return;
            }

            // =========================
            // REDO
            // =========================

            if (Keyboard.Modifiers == ModifierKeys.Control
                && key == Key.Y)
            {
                AppServices.Commands.Redo();
                return;
            }

            _ferramentaAtual.OnKeyDown(key);
        }
    }
}