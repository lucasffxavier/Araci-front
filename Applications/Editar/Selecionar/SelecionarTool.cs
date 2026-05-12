using System.Linq;
using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelecionarTool : ITool
    {
        private Point _ultimoPonto;

        private bool _arrastandoElementos;

        private bool _selecionandoJanela;

        private Point _inicioJanela;

        public string Nome =>
            "Selecionar";

        public bool MantemBotaoAtivado =>
            true;

        public void Ativar()
        {
        }

        public void Desativar()
        {
            _arrastandoElementos = false;
            _selecionandoJanela = false;

            AppServices.SelectionBox
                .Visivel = false;
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            bool ctrl =
                Keyboard.Modifiers
                    .HasFlag(ModifierKeys.Control);

            // =========================
            // CLICK EM ELEMENTO
            // =========================

            if (vm != null)
            {
                if (ctrl)
                {
                    SelectionService.Toggle(vm);
                }
                else
                {
                    if (!SelectionService
                        .Selecionados
                        .Contains(vm))
                    {
                        SelectionService
                            .Selecionar(vm);
                    }
                }

                _ultimoPonto = position;

                _arrastandoElementos = true;

                AppServices.Commands
                    .BeginTransaction();

                return;
            }

            // =========================
            // CLICK VAZIO
            // =========================

            if (!ctrl)
            {
                SelectionService.Limpar();
            }

            _inicioJanela = position;

            _selecionandoJanela = true;

            AppServices.SelectionBox
                .Visivel = true;

            AppServices.SelectionBox
                .Atualizar(position, position);
        }

        public void OnMouseMove(
            Point position)
        {
            // =========================
            // MOVE ELEMENTOS
            // =========================

            if (_arrastandoElementos)
            {
                Vector delta =
                    position - _ultimoPonto;

                foreach (var item in SelectionService
                    .Selecionados
                    .ToList())
                {
                    MoveService.Mover(item, delta);
                }

                _ultimoPonto = position;

                return;
            }

            // =========================
            // SELECTION BOX
            // =========================

            if (_selecionandoJanela)
            {
                AppServices.SelectionBox
                    .Atualizar(
                        _inicioJanela,
                        position);
            }
        }

        public void OnMouseUp(
            Point position)
        {
            // =========================
            // FINALIZAR MOVE
            // =========================

            if (_arrastandoElementos)
            {
                AppServices.Commands
                    .CommitTransaction();
            }

            // =========================
            // FINALIZAR JANELA
            // =========================

            if (_selecionandoJanela)
            {
                var rect =
                    AppServices.SelectionBox
                        .Bounds;

                foreach (var item in AppServices
                    .Document
                    .Elementos)
                {
                    if (rect.IntersectsWith(
                        item.Bounds))
                    {
                        SelectionService
                            .Selecionar(item, true);
                    }
                }
            }

            _arrastandoElementos = false;
            _selecionandoJanela = false;

            AppServices.SelectionBox
                .Visivel = false;
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}