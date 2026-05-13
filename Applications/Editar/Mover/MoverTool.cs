using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Mover
{
    public class MoverTool : ITool
    {
        private bool _movendo;

        private Point _ultimoPonto;

        // =========================
        // ESTADOS INICIAIS
        // =========================

        private readonly Dictionary<
            ElementoViewModel,
            ElementoEstado>
            _estadosIniciais
                = new();

        public string Nome =>
            "Mover";

        public bool MantemBotaoAtivado =>
            true;

        public void Ativar()
        {
        }

        public void Desativar()
        {
            _movendo = false;

            _estadosIniciais.Clear();
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            if (vm == null)
                return;

            if (!SelectionService
                .Selecionados
                .Contains(vm))
            {
                SelectionService
                    .Selecionar(vm);
            }

            _movendo = true;

            _ultimoPonto = position;

            // =========================
            // CAPTURA ESTADOS
            // =========================

            _estadosIniciais.Clear();

            foreach (var item in SelectionService
                .Selecionados)
            {
                _estadosIniciais[item] =
                    item.CapturarEstado();
            }
        }

        public void OnMouseMove(
            Point position)
        {
            if (!_movendo)
                return;

            Vector delta =
                position - _ultimoPonto;

            foreach (var item in SelectionService
                .Selecionados
                .ToList())
            {
                MoveService
                    .MoverVisual(
                        item,
                        delta);
            }

            _ultimoPonto = position;
        }

        public void OnMouseUp(
            Point position)
        {
            if (!_movendo)
                return;

            var composite =
                new CompositeCommand();

            foreach (var item in SelectionService
                .Selecionados)
            {
                if (!_estadosIniciais
                    .ContainsKey(item))
                {
                    continue;
                }

                var estadoInicial =
                    _estadosIniciais[item];

                var estadoFinal =
                    item.CapturarEstado();

                bool mudou =
                    estadoInicial.X != estadoFinal.X
                    ||
                    estadoInicial.Y != estadoFinal.Y;

                if (!mudou)
                    continue;

                composite.Add(
                    new MoveElementCommand(
                        item,
                        estadoInicial,
                        estadoFinal));
            }

            if (!composite.IsEmpty)
            {
                AppServices.Commands
                    .Execute(composite);
            }

            _movendo = false;

            _estadosIniciais.Clear();
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}