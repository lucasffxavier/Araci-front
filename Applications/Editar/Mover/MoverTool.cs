using System.Linq;
using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Mover
{
    public class MoverTool : ITool
    {
        private bool _movendo;

        private Point _ultimoPonto;
        private Point _pontoInicial;

        public string Nome => "Mover";

        public bool MantemBotaoAtivado => true;

        public void Ativar()
        {
        }

        public void Desativar()
        {
            _movendo = false;

            AppServices.MoveHud.Visivel = false;
            AppServices.MoveHud.Reset();
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
                SelectionService.Selecionar(vm);
            }

            _movendo = true;

            _ultimoPonto = position;
            _pontoInicial = position;

            MoveService.BeginMove(
                SelectionService
                    .Selecionados
                    .ToList());

            var hud = AppServices.MoveHud;

            hud.Visivel = true;
            hud.Reset();
        }

        public void OnMouseMove(Point position)
        {
            if (!_movendo)
                return;

            Vector delta =
                position - _ultimoPonto;

            foreach (var item in
                SelectionService.Selecionados)
            {
                MoveService.MoverVisual(
                    item,
                    delta);
            }

            var deltaTotal =
                position - _pontoInicial;

            var hud = AppServices.MoveHud;

            hud.DeltaX = deltaTotal.X;
            hud.DeltaY = deltaTotal.Y;

            var bounds =
                CalcularBoundsSelecionados();

            hud.AtualizarPosicao(bounds);

            _ultimoPonto = position;
        }

        public void OnMouseUp(Point position)
        {
            if (!_movendo)
                return;

            MoveService.EndMove(
                SelectionService
                    .Selecionados
                    .ToList());

            AppServices.MoveHud.Visivel = false;
            AppServices.MoveHud.Reset();

            _movendo = false;
        }

        private Rect CalcularBoundsSelecionados()
        {
            var items =
                SelectionService.Selecionados;

            if (items.Count == 0)
                return Rect.Empty;

            double minX =
                items.Min(i => i.Bounds.Left);

            double minY =
                items.Min(i => i.Bounds.Top);

            double maxX =
                items.Max(i => i.Bounds.Right);

            double maxY =
                items.Max(i => i.Bounds.Bottom);

            return new Rect(
                minX,
                minY,
                maxX - minX,
                maxY - minY);
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}