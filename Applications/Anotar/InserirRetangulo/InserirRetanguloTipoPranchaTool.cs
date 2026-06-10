using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.ViewModels;

namespace Araci.Applications.Anotar.InserirRetangulo
{
    public sealed class InserirRetanguloTipoPranchaTool : ITool
    {
        private const double ToleranciaArea = 0.0001;

        private readonly Func<ProjectSheetTypeViewModel?> _obterViewModelAtivo;
        private readonly InserirRetanguloNoTipoPranchaUseCase _inserirRetangulo;
        private readonly Action _voltarParaSelecao;
        private Point? _pontoInicial;

        public InserirRetanguloTipoPranchaTool(
            Func<ProjectSheetTypeViewModel?> obterViewModelAtivo,
            InserirRetanguloNoTipoPranchaUseCase inserirRetangulo,
            Action voltarParaSelecao)
        {
            _obterViewModelAtivo = obterViewModelAtivo ?? throw new ArgumentNullException(nameof(obterViewModelAtivo));
            _inserirRetangulo = inserirRetangulo ?? throw new ArgumentNullException(nameof(inserirRetangulo));
            _voltarParaSelecao = voltarParaSelecao ?? throw new ArgumentNullException(nameof(voltarParaSelecao));
        }

        public string Nome => "Retângulo";
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
            LimparPreview();
            _pontoInicial = null;
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            if (_pontoInicial.HasValue)
                return;

            ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

            if (viewModel == null)
                return;

            _pontoInicial = NormalizarPonto(position, viewModel);
            AtualizarPreview(_pontoInicial.Value, inputState.IsShiftPressed);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (!_pontoInicial.HasValue)
                return;

            AtualizarPreview(position, inputState.IsShiftPressed);
        }

        public void OnMouseUp(Point position, ToolInputState inputState)
        {
            if (!_pontoInicial.HasValue)
                return;

            Finalizar(position, inputState.IsShiftPressed, inputState.IsControlPressed);
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

        private void AtualizarPreview(Point pontoFinal, bool quadrado)
        {
            ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

            if (viewModel == null || !_pontoInicial.HasValue)
                return;

            Point finalNormalizado = NormalizarPonto(pontoFinal, viewModel);
            Rect rect = CriarRect(_pontoInicial.Value, finalNormalizado, quadrado);

            viewModel.SetPreviewRectangle(new ProjectSheetTemplateRectangle
            {
                X = rect.X,
                Y = rect.Y,
                Largura = Math.Max(ProjectSheetTemplateRectangle.MinDimension, rect.Width),
                Altura = Math.Max(ProjectSheetTemplateRectangle.MinDimension, rect.Height),
                Stroke = "#FF1E90FF",
                StrokeThickness = 1.0
            });
        }

        private void Finalizar(Point pontoFinal, bool quadrado, bool manterFerramentaAtiva)
        {
            ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

            if (viewModel == null || !_pontoInicial.HasValue)
            {
                Cancelar();
                return;
            }

            Point finalNormalizado = NormalizarPonto(pontoFinal, viewModel);
            Rect rect = CriarRect(_pontoInicial.Value, finalNormalizado, quadrado);

            LimparPreview();
            _pontoInicial = null;

            if (rect.Width * rect.Height < ToleranciaArea)
                return;

            ProjectSheetTemplateRectangle? retangulo = _inserirRetangulo.Inserir(
                viewModel.Id,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height);

            if (retangulo == null)
                return;

            viewModel.Refresh();
            viewModel.SelectRectangle(retangulo.Id);

            if (!manterFerramentaAtiva)
                _voltarParaSelecao();
        }

        private void LimparPreview()
        {
            ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();
            viewModel?.SetPreviewRectangle(null);
        }

        private static Rect CriarRect(Point inicio, Point fim, bool quadrado)
        {
            Vector delta = fim - inicio;

            if (quadrado)
            {
                double lado = Math.Min(Math.Abs(delta.X), Math.Abs(delta.Y));

                if (lado < ProjectSheetTemplateRectangle.MinDimension)
                    lado = Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y));

                double sinalX = Math.Sign(delta.X == 0 ? 1 : delta.X);
                double sinalY = Math.Sign(delta.Y == 0 ? 1 : delta.Y);
                fim = new Point(inicio.X + sinalX * lado, inicio.Y + sinalY * lado);
            }

            double x = Math.Min(inicio.X, fim.X);
            double y = Math.Min(inicio.Y, fim.Y);
            double largura = Math.Abs(fim.X - inicio.X);
            double altura = Math.Abs(fim.Y - inicio.Y);

            return new Rect(x, y, largura, altura);
        }

        private static Point NormalizarPonto(Point ponto, ProjectSheetTypeViewModel viewModel)
        {
            return new Point(
                Math.Max(0, Math.Min(viewModel.SheetWidth, ponto.X)),
                Math.Max(0, Math.Min(viewModel.SheetHeight, ponto.Y)));
        }
    }
}