using System.Collections.Generic;
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

        private readonly Dictionary<
            ElementoViewModel,
            ElementoEstado>
            _estadosIniciais
                = new();

        public string Nome => "Selecionar";

        public bool MantemBotaoAtivado => true;

        public void Ativar() { }

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
                Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (vm != null)
            {
                if (ctrl)
                    SelectionService.Toggle(vm);
                else if (!SelectionService.Selecionados.Contains(vm))
                    SelectionService.Selecionar(vm);

                _ultimoPonto = position;
                _arrastandoElementos = true;

                _estadosIniciais.Clear();

                foreach (var item in SelectionService.Selecionados)
                {
                    _estadosIniciais[item] =
                        item.CapturarEstado();
                }

                return;
            }

            if (!ctrl)
                SelectionService.Limpar();

            _inicioJanela = position;
            _selecionandoJanela = true;

            AppServices.SelectionBox.Visivel = true;
            AppServices.SelectionBox.Atualizar(position, position);
        }

        public void OnMouseMove(Point position)
        {
            if (_arrastandoElementos)
            {
                Vector delta =
                    position - _ultimoPonto;

                foreach (var item in SelectionService.Selecionados.ToList())
                {
                    MoveService.MoverVisual(item, delta);
                }

                _ultimoPonto = position;
                return;
            }

            if (_selecionandoJanela)
            {
                AppServices.SelectionBox
                    .Atualizar(_inicioJanela, position);
            }
        }

        public void OnMouseUp(Point position)
        {
            if (_arrastandoElementos)
            {
                // 🔥 Sem HUD e sem transaction (movimento leve)
            }

            if (_selecionandoJanela)
            {
                var rect =
                    AppServices.SelectionBox.Bounds;

                foreach (var item in AppServices.Document.Elementos)
                {
                    if (rect.IntersectsWith(item.Bounds))
                    {
                        SelectionService.Selecionar(item, true);
                    }
                }
            }

            _arrastandoElementos = false;
            _selecionandoJanela = false;

            _estadosIniciais.Clear();

            AppServices.SelectionBox.Visivel = false;
        }

        public void OnKeyDown(Key key) { }
    }
}