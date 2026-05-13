using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelecionarTool : ITool
    {
        private bool _arrastandoElementos;

        private bool _selecionandoJanela;

        private Point _inicioJanela;

        public string Nome => "Selecionar";

        public bool MantemBotaoAtivado => true;

        public void Ativar()
        {
        }

        public void Desativar()
        {
            _arrastandoElementos = false;
            _selecionandoJanela = false;

            AppServices.SelectionBox.Visivel = false;
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            bool ctrl =
                Keyboard.Modifiers
                    .HasFlag(ModifierKeys.Control);

            if (vm != null)
            {
                if (ctrl)
                {
                    SelectionService.Toggle(vm);
                }
                else if (!SelectionService
                             .Selecionados
                             .Contains(vm))
                {
                    SelectionService.Selecionar(vm);
                }

                _arrastandoElementos = true;

                MoveService.BeginMove(
                    SelectionService.Selecionados);

                return;
            }

            if (!ctrl)
            {
                SelectionService.Limpar();
            }

            _inicioJanela = position;

            _selecionandoJanela = true;

            AppServices.SelectionBox.Visivel = true;

            AppServices.SelectionBox.Atualizar(
                position,
                position);
        }

        public void OnMouseMove(Point position)
        {
            if (_selecionandoJanela)
            {
                AppServices.SelectionBox
                    .Atualizar(
                        _inicioJanela,
                        position);
            }
        }

        public void OnMouseUp(Point position)
        {
            if (_arrastandoElementos)
            {
                MoveService.EndMove(
                    SelectionService.Selecionados);
            }

            if (_selecionandoJanela)
            {
                var rect =
                    AppServices
                        .SelectionBox
                        .Bounds;

                foreach (var item in
                    AppServices.Document.Elementos)
                {
                    if (rect.IntersectsWith(
                            item.Bounds))
                    {
                        SelectionService
                            .Selecionar(
                                item,
                                true);
                    }
                }
            }

            _arrastandoElementos = false;
            _selecionandoJanela = false;

            AppServices.SelectionBox.Visivel = false;
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}