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
		private readonly Func<ProjectSheetTypeViewModel?> _obterViewModelAtivo;
		private readonly InserirRetanguloNoTipoPranchaUseCase _inserirRetangulo;
		private readonly Action _voltarParaSelecao;

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

			double largura = Math.Min(ProjectSheetTemplateRectangle.DefaultWidth, Math.Max(ProjectSheetTemplateRectangle.MinDimension, viewModel.SheetWidth));
			double altura = Math.Min(ProjectSheetTemplateRectangle.DefaultHeight, Math.Max(ProjectSheetTemplateRectangle.MinDimension, viewModel.SheetHeight));
			Point centro = NormalizarPonto(position, viewModel);
			double x = centro.X - largura / 2.0;
			double y = centro.Y - altura / 2.0;

			ProjectSheetTemplateRectangle? retangulo = _inserirRetangulo.Inserir(viewModel.Id, x, y, largura, altura);

			if (retangulo == null)
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
			return new Point(
				Math.Max(0, Math.Min(viewModel.SheetWidth, ponto.X)),
				Math.Max(0, Math.Min(viewModel.SheetHeight, ponto.Y)));
		}
	}
}