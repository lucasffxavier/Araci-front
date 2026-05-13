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
        // =========================
        // ESTADO
        // =========================

        private bool _movendo;

        private Point _ultimoPonto;

        private Point _pontoInicial;

        // =========================
        // INFO TOOL
        // =========================

        public string Nome => "Mover";

        public bool MantemBotaoAtivado => true;

        // =========================
        // ATIVAR
        // =========================

        public void Ativar()
        {
        }

        // =========================
        // DESATIVAR
        // =========================

        public void Desativar()
        {
            CancelarMovimento();
        }

        // =========================
        // MOUSE DOWN
        // =========================

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            // =========================
            // CLIQUE NO VAZIO
            // =========================

            if (vm == null)
            {
                CancelarMovimento();

                SelectionService.Limpar();

                return;
            }

            // =========================
            // GARANTE SELEÇÃO
            // =========================

            if (!SelectionService
                    .Selecionados
                    .Contains(vm))
            {
                SelectionService.Selecionar(vm);
            }

            // =========================
            // INICIA MOVIMENTO
            // =========================

            _movendo = true;

            _ultimoPonto = position;

            _pontoInicial = position;

            MoveService.BeginMove(
                SelectionService
                    .Selecionados
                    .ToList());

            // =========================
            // HUD
            // =========================

            var hud =
                AppServices.MoveHud;

            hud.Reset();

            var bounds =
                CalcularBoundsSelecionados();

            hud.AtualizarPosicao(bounds);

            hud.Visivel = true;
        }

        // =========================
        // MOUSE MOVE
        // =========================

        public void OnMouseMove(
            Point position)
        {
            if (!_movendo)
                return;

            Vector delta =
                position - _ultimoPonto;

            // =========================
            // MOVE ELEMENTOS
            // =========================

            foreach (var item in
                SelectionService.Selecionados)
            {
                MoveService.MoverVisual(
                    item,
                    delta);
            }

            // =========================
            // DELTA TOTAL
            // =========================

            var deltaTotal =
                position - _pontoInicial;

            // =========================
            // HUD
            // =========================

            var hud =
                AppServices.MoveHud;

            hud.DeltaX =
                deltaTotal.X;

            hud.DeltaY =
                deltaTotal.Y;

            var bounds =
                CalcularBoundsSelecionados();

            hud.AtualizarPosicao(bounds);

            _ultimoPonto = position;
        }

        // =========================
        // MOUSE UP
        // =========================

        public void OnMouseUp(
            Point position)
        {
            if (!_movendo)
                return;

            MoveService.EndMove(
                SelectionService
                    .Selecionados
                    .ToList());

            AppServices.MoveHud
                .Visivel = false;

            AppServices.MoveHud
                .Reset();

            _movendo = false;
        }

        // =========================
        // KEYBOARD
        // =========================

        public void OnKeyDown(
            Key key)
        {
        }

        // =========================
        // CANCELAR
        // =========================

        private void CancelarMovimento()
        {
            _movendo = false;

            AppServices.MoveHud
                .Visivel = false;

            AppServices.MoveHud
                .Reset();
        }

        // =========================
        // BOUNDS
        // =========================

        private Rect CalcularBoundsSelecionados()
        {
            var items =
                SelectionService
                    .Selecionados;

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
    }
}