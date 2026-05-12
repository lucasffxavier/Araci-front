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

            _ultimoPonto = position;

            _arrastando = true;

            AppServices
                .Commands
                .BeginTransaction();
        }

        public void OnMouseMove(
            Point position)
        {
            if (!_arrastando)
                return;

            Vector delta =
                position - _ultimoPonto;

            var selecionados =
                AppServices
                    .Editor
                    .ElementosSelecionados
                    .ToList();

            foreach (var vm in selecionados)
            {
                MoveService.Mover(vm, delta);
            }

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
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}