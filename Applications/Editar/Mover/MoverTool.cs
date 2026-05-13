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
        private Point _pontoInicial;

        private readonly Dictionary<
            ElementoViewModel,
            ElementoEstado>
            _estadosIniciais
                = new();

        public string Nome => "Mover";

        public bool MantemBotaoAtivado => true;

        public void Ativar() { }

        public void Desativar()
        {
            _movendo = false;
            _estadosIniciais.Clear();

            var hud = AppServices.MoveHud;
            hud.Visivel = false;
            hud.Reset();
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            if (vm == null)
                return;

            if (!SelectionService.Selecionados.Contains(vm))
                SelectionService.Selecionar(vm);

            _movendo = true;

            _ultimoPonto = position;
            _pontoInicial = position;

            var hud = AppServices.MoveHud;
            hud.Visivel = true;
            hud.Reset();

            _estadosIniciais.Clear();

            foreach (var item in SelectionService.Selecionados)
            {
                _estadosIniciais[item] =
                    item.CapturarEstado();
            }
        }

        public void OnMouseMove(Point position)
        {
            if (!_movendo)
                return;

            Vector delta =
                position - _ultimoPonto;

            foreach (var item in SelectionService.Selecionados.ToList())
            {
                MoveService.MoverVisual(item, delta);
            }

            var deltaTotal =
                position - _pontoInicial;

            var hud = AppServices.MoveHud;
            hud.DeltaX = deltaTotal.X;
            hud.DeltaY = deltaTotal.Y;

            // 🔥 CENTRO DO GRUPO
            var bounds =
                CalcularBoundsSelecionados();

            hud.AtualizarPosicao(bounds);

            _ultimoPonto = position;
        }

        public void OnMouseUp(Point position)
        {
            if (!_movendo)
                return;

            using var tx =
                AppServices.BeginTransaction();

            foreach (var item in SelectionService.Selecionados)
            {
                if (!_estadosIniciais.ContainsKey(item))
                    continue;

                var inicial = _estadosIniciais[item];
                var final = item.CapturarEstado();

                if (inicial.X == final.X &&
                    inicial.Y == final.Y)
                    continue;

                tx.Add(new MoveElementCommand(
                    item,
                    inicial,
                    final));
            }

            tx.Commit();

            var hud = AppServices.MoveHud;
            hud.Visivel = false;
            hud.Reset();

            _movendo = false;
            _estadosIniciais.Clear();
        }

        private Rect CalcularBoundsSelecionados()
        {
            var items = SelectionService.Selecionados;

            if (items.Count == 0)
                return new Rect();

            double minX = items.Min(i => i.Bounds.X);
            double minY = items.Min(i => i.Bounds.Y);
            double maxX = items.Max(i => i.Bounds.Right);
            double maxY = items.Max(i => i.Bounds.Bottom);

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        public void OnKeyDown(Key key) { }
    }
}