using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.ViewModels;

namespace Araci.Applications.Anotar.InserirTexto
{
    public sealed class InserirTextoTipoPranchaTool : ITool
    {
        private readonly Func<ProjectSheetTypeViewModel?> _obterViewModelAtivo;
        private readonly InserirTextoNoTipoPranchaUseCase _inserirTexto;
        private readonly Action _voltarParaSelecao;

        public InserirTextoTipoPranchaTool(
            Func<ProjectSheetTypeViewModel?> obterViewModelAtivo,
            InserirTextoNoTipoPranchaUseCase inserirTexto,
            Action voltarParaSelecao)
        {
            _obterViewModelAtivo = obterViewModelAtivo ?? throw new ArgumentNullException(nameof(obterViewModelAtivo));
            _inserirTexto = inserirTexto ?? throw new ArgumentNullException(nameof(inserirTexto));
            _voltarParaSelecao = voltarParaSelecao ?? throw new ArgumentNullException(nameof(voltarParaSelecao));
        }

        public string Nome => "Texto";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => false;

        public void Ativar()
        {
        }

        public void Desativar()
        {
        }

        public void Cancelar()
        {
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

            if (viewModel == null)
                return;

            Point ponto = NormalizarPonto(position, viewModel);
            ProjectSheetTemplateText? texto = _inserirTexto.Inserir(viewModel.Id, ponto.X, ponto.Y);

            if (texto == null)
                return;

            viewModel.Refresh();

            if (!inputState.IsControlPressed)
                _voltarParaSelecao();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
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
                _voltarParaSelecao();
        }

        private static Point NormalizarPonto(Point ponto, ProjectSheetTypeViewModel viewModel)
        {
            double larguraCaixa = CalcularLarguraInicialTexto(viewModel);
            double xMaximo = Math.Max(0.0, viewModel.SheetWidth - larguraCaixa);

            return new Point(
                Math.Max(0.0, Math.Min(xMaximo, ponto.X)),
                Math.Max(0.0, Math.Min(viewModel.SheetHeight, ponto.Y)));
        }

        private static double CalcularLarguraInicialTexto(ProjectSheetTypeViewModel viewModel)
        {
            double larguraNatural = ProjectSheetTemplateText.CalcularLarguraNatural(
                ProjectSheetTemplateText.DefaultText,
                ProjectSheetTemplateText.DefaultTextHeight);

            double larguraFolha = double.IsNaN(viewModel.SheetWidth) || double.IsInfinity(viewModel.SheetWidth)
                ? ProjectSheetTemplateText.MinBoxWidth
                : Math.Max(ProjectSheetTemplateText.MinBoxWidth, viewModel.SheetWidth);

            return Math.Min(larguraNatural, larguraFolha);
        }
    }
}