using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelecionarTool : ITool
    {
        private ElementoViewModel?
            _selecionado;

        private Point _ultimoPonto;

        private bool _arrastando;

        public string Nome =>
            "Selecionar";

        public bool MantemBotaoAtivado =>
            true;

        public void Ativar()
        {
        }

        public void Desativar()
        {
            _arrastando = false;
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            if (vm == null)
            {
                SelectionService.Limpar();
                return;
            }

            SelectionService.Selecionar(vm);

            _selecionado = vm;

            _ultimoPonto = position;

            _arrastando = true;

            // =========================
            // BEGIN TRANSACTION
            // =========================

            AppServices
                .Commands
                .BeginTransaction();
        }

        public void OnMouseMove(
            Point position)
        {
            if (!_arrastando
                || _selecionado == null)
                return;

            Vector delta =
                position - _ultimoPonto;

            MoveService.Mover(
                _selecionado,
                delta);

            _ultimoPonto = position;
        }

        public void OnMouseUp(
            Point position)
        {
            if (_arrastando)
            {
                AppServices
                    .Commands
                    .CommitTransaction();
            }

            _arrastando = false;

            _selecionado = null;
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}