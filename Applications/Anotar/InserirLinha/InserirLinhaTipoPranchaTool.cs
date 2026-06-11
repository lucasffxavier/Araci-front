using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.ViewModels;

namespace Araci.Applications.Anotar.InserirLinha
{
    public sealed class InserirLinhaTipoPranchaTool : ITool
    {
        private const double ToleranciaDistanciaQuadrada = 0.0001;
        private const double LineEndpointSnapTolerance = 8.0;

        private readonly Func<ProjectSheetTypeViewModel?> _obterViewModelAtivo;
        private readonly InserirLinhaNoTipoPranchaUseCase _inserirLinha;
        private readonly Action _voltarParaSelecao;
        private Point? _pontoInicial;

        public InserirLinhaTipoPranchaTool(
            Func<ProjectSheetTypeViewModel?> obterViewModelAtivo,
            InserirLinhaNoTipoPranchaUseCase inserirLinha,
            Action voltarParaSelecao)
        {
            _obterViewModelAtivo = obterViewModelAtivo ?? throw new ArgumentNullException(nameof(obterViewModelAtivo));
            _inserirLinha = inserirLinha ?? throw new ArgumentNullException(nameof(inserirLinha));
            _voltarParaSelecao = voltarParaSelecao ?? throw new ArgumentNullException(nameof(voltarParaSelecao));
        }

        public string Nome => "Linha";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _pontoInicial.HasValue;

        public void Ativar()
        {
        }

        public void Desativar()
        {
            Cancelar();
        }

        public void Cancelar()
        {
            _pontoInicial = null;

            ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();
            viewModel?.SetPreviewLine(null);
            viewModel?.ClearLineSnapPoints();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

            if (viewModel == null)
                return;

            Point ponto = ObterPontoDeCriacao(position, viewModel, aplicarSnap: !inputState.IsShiftPressed);

            if (!_pontoInicial.HasValue)
            {
                _pontoInicial = ponto;
                viewModel.ClearLineSnapPoints();
                AtualizarPreview(viewModel, ponto, false);
                return;
            }

            FinalizarLinha(viewModel, ponto, inputState.IsShiftPressed, inputState.IsControlPressed);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

            if (viewModel == null)
                return;

            if (!_pontoInicial.HasValue)
            {
                ObterPontoDeCriacao(position, viewModel, aplicarSnap: true);
                return;
            }

            Point ponto = ObterPontoDeCriacao(position, viewModel, aplicarSnap: !inputState.IsShiftPressed);
            AtualizarPreview(viewModel, ponto, inputState.IsShiftPressed);
        }

        public void OnMouseUp(Point position, ToolInputState inputState)
        {
        }

        public bool HandlesKey(Key key)
        {
            return key == Key.Escape;
        }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Escape)
                Cancelar();
        }

        private void FinalizarLinha(ProjectSheetTypeViewModel viewModel, Point pontoFinal, bool ortogonalizar, bool manterAtiva)
        {
            Point pontoInicial = _pontoInicial!.Value;
            Point pontoFinalAjustado = ortogonalizar
                ? AplicarOrtogonalizacao(pontoFinal, pontoInicial)
                : pontoFinal;

            if (DistanciaQuadrada(pontoInicial, pontoFinalAjustado) < ToleranciaDistanciaQuadrada)
            {
                AtualizarPreview(viewModel, pontoFinalAjustado, false);
                return;
            }

            _inserirLinha.Inserir(viewModel.Id, pontoInicial.X, pontoInicial.Y, pontoFinalAjustado.X, pontoFinalAjustado.Y);
            viewModel.Refresh();
            viewModel.ClearLineSnapPoints();

            if (manterAtiva)
            {
                _pontoInicial = pontoFinalAjustado;
                AtualizarPreview(viewModel, pontoFinalAjustado, false);
                return;
            }

            _pontoInicial = null;
            viewModel.SetPreviewLine(null);
            _voltarParaSelecao();
        }

        private void AtualizarPreview(ProjectSheetTypeViewModel viewModel, Point pontoFinal, bool ortogonalizar)
        {
            if (!_pontoInicial.HasValue)
                return;

            Point pontoInicial = _pontoInicial.Value;
            Point pontoFinalAjustado = ortogonalizar
                ? AplicarOrtogonalizacao(pontoFinal, pontoInicial)
                : pontoFinal;

            viewModel.SetPreviewLine(new ProjectSheetTemplateLine
            {
                X1 = pontoInicial.X,
                Y1 = pontoInicial.Y,
                X2 = pontoFinalAjustado.X,
                Y2 = pontoFinalAjustado.Y,
                Stroke = "#FF1E90FF"
            });
        }

        private static Point ObterPontoDeCriacao(Point ponto, ProjectSheetTypeViewModel viewModel, bool aplicarSnap)
        {
            Point normalizado = NormalizarPonto(ponto, viewModel);

            if (!aplicarSnap)
            {
                viewModel.ClearLineSnapPoints();
                return normalizado;
            }

            return viewModel.TrySnapLineEndpoint(normalizado, LineEndpointSnapTolerance, out Point snapPoint)
                ? NormalizarPonto(snapPoint, viewModel)
                : normalizado;
        }

        private static Point NormalizarPonto(Point ponto, ProjectSheetTypeViewModel viewModel)
        {
            return new Point(
                Math.Max(0, Math.Min(viewModel.SheetWidth, ponto.X)),
                Math.Max(0, Math.Min(viewModel.SheetHeight, ponto.Y)));
        }

        private static Point AplicarOrtogonalizacao(Point ponto, Point origem)
        {
            Vector delta = ponto - origem;

            if (Math.Abs(delta.X) < 0.0001 && Math.Abs(delta.Y) < 0.0001)
                return origem;

            return Math.Abs(delta.X) >= Math.Abs(delta.Y)
                ? new Point(ponto.X, origem.Y)
                : new Point(origem.X, ponto.Y);
        }

        private static double DistanciaQuadrada(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }
    }
}