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
        // =========================
        // MODO
        // =========================

        private readonly bool _modoSoMover;

        // =========================
        // ESTADO INTERNO
        // =========================

        private bool _arrastandoElementos;

        private bool _selecionandoJanela;

        private Point _inicioJanela;

        private Point _ultimoPontoMouse;

        private Point _pontoInicialArrasto;

        // =========================
        // INFO TOOL
        // =========================

        public string Nome =>
            "Selecionar";

        public bool MantemBotaoAtivado =>
            true;

        // =========================
        // CONSTRUTOR
        // =========================

        public SelecionarTool(
            bool modoSoMover = false)
        {
            _modoSoMover = modoSoMover;
        }

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
            _arrastandoElementos = false;

            _selecionandoJanela = false;

            AppServices.SelectionBox
                .Visivel = false;

            AppServices.MoveHud
                .Visivel = false;

            AppServices.MoveHud
                .Reset();
        }

        // =========================
        // MOUSE DOWN
        // =========================

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            bool ctrl =
                Keyboard.Modifiers
                    .HasFlag(ModifierKeys.Control);

            // =========================
            // CLIQUE EM ELEMENTO
            // =========================

            if (vm != null)
            {
                if (!_modoSoMover)
                {
                    // =========================
                    // SELEÇÃO COM CTRL
                    // =========================

                    if (ctrl)
                    {
                        SelectionService.Toggle(vm);
                    }
                    else if (!SelectionService
                                 .Selecionados
                                 .Contains(vm))
                    {
                        // =========================
                        // SELEÇÃO SIMPLES
                        // =========================

                        SelectionService.Selecionar(vm);
                    }
                }

                // =========================
                // INICIA ARRASTO
                // =========================

                _arrastandoElementos = true;

                _ultimoPontoMouse = position;

                _pontoInicialArrasto = position;

                MoveService.BeginMove(
                    SelectionService.Selecionados);

                // =========================
                // HUD
                // =========================

                var hud = AppServices.MoveHud;

                hud.Reset();

                var bounds =
                    CalcularBoundsSelecionados();

                hud.AtualizarPosicao(bounds);

                hud.Visivel = true;

                return;
            }

            // =========================
            // CLIQUE NO VAZIO
            // =========================

            if (_modoSoMover)
            {

                return;
            }

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

        // =========================
        // MOUSE MOVE
        // =========================

        public void OnMouseMove(
            Point position)
        {
            // =========================
            // ARRASTO DE ELEMENTOS
            // =========================

            if (_arrastandoElementos)
            {
                Vector delta =
                    position - _ultimoPontoMouse;

                if (delta.X != 0 || delta.Y != 0)
                {
                    foreach (var item in
                        SelectionService
                            .Selecionados
                            .ToList())
                    {
                        MoveService.MoverVisual(
                            item,
                            delta);
                    }
                }

                // =========================
                // HUD — DELTA ACUMULADO
                // =========================

                var deltaTotal =
                    position - _pontoInicialArrasto;

                var hud = AppServices.MoveHud;

                hud.DeltaX = deltaTotal.X;

                hud.DeltaY = deltaTotal.Y;

                var bounds =
                    CalcularBoundsSelecionados();

                hud.AtualizarPosicao(bounds);

                _ultimoPontoMouse = position;

                return;
            }

            // =========================
            // JANELA DE SELEÇÃO
            // =========================

            if (_selecionandoJanela)
            {
                AppServices.SelectionBox
                    .Atualizar(
                        _inicioJanela,
                        position);
            }
        }

        // =========================
        // MOUSE UP
        // =========================

        public void OnMouseUp(
            Point position)
        {
            // =========================
            // FINALIZA ARRASTO
            // =========================

            if (_arrastandoElementos)
            {
                MoveService.EndMove(
                    SelectionService
                        .Selecionados
                        .ToList());

                AppServices.MoveHud
                    .Visivel = false;

                AppServices.MoveHud
                    .Reset();
            }

            // =========================
            // FINALIZA JANELA DE SELEÇÃO
            // =========================

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

                AppServices.SelectionBox
                    .Visivel = false;
            }

            _arrastandoElementos = false;

            _selecionandoJanela = false;
        }

        // =========================
        // KEYBOARD
        // =========================

        public void OnKeyDown(
            Key key)
        {
        }

        // =========================
        // BOUNDS
        // =========================

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
    }
}