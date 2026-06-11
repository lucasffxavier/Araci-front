using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.ViewModels;

namespace Araci.Applications.Anotar.InserirCirculo
{
	public sealed class InserirCirculoTipoPranchaTool : ITool
	{
		private const double RaioMinimo = 1.0;

		private readonly Func<ProjectSheetTypeViewModel?> _obterViewModelAtivo;
		private readonly InserirCirculoNoTipoPranchaUseCase _inserirCirculo;
		private readonly Action _voltarParaSelecao;
		private Point? _centro;

		public InserirCirculoTipoPranchaTool(
			Func<ProjectSheetTypeViewModel?> obterViewModelAtivo,
			InserirCirculoNoTipoPranchaUseCase inserirCirculo,
			Action voltarParaSelecao)
		{
			_obterViewModelAtivo = obterViewModelAtivo ?? throw new ArgumentNullException(nameof(obterViewModelAtivo));
			_inserirCirculo = inserirCirculo ?? throw new ArgumentNullException(nameof(inserirCirculo));
			_voltarParaSelecao = voltarParaSelecao ?? throw new ArgumentNullException(nameof(voltarParaSelecao));
		}

		public string Nome => "Círculo";
		public bool MantemBotaoAtivado => true;
		public bool IsBusy => _centro.HasValue;

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
			_centro = null;
		}

		public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
		{
			if (_centro.HasValue)
				return;

			ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

			if (viewModel == null)
				return;

			_centro = NormalizarPonto(position, viewModel);
			AtualizarPreview(_centro.Value);
		}

		public void OnMouseMove(Point position, ToolInputState inputState)
		{
			if (!_centro.HasValue)
				return;

			AtualizarPreview(position);
		}

		public void OnMouseUp(Point position, ToolInputState inputState)
		{
			if (!_centro.HasValue)
				return;

			Finalizar(position, inputState.IsControlPressed);
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

		private void AtualizarPreview(Point pontoRaio)
		{
			ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

			if (viewModel == null || !_centro.HasValue)
				return;

			Point centroNormalizado = NormalizarPonto(_centro.Value, viewModel);
			Point pontoFinalNormalizado = NormalizarPonto(pontoRaio, viewModel);
			double raio = Math.Max(ProjectSheetTemplateCircle.MinRadius, CalcularRaio(centroNormalizado, pontoFinalNormalizado));
			raio = LimitarRaioAoTipoPrancha(centroNormalizado, raio, viewModel);

			viewModel.SetPreviewCircle(new ProjectSheetTemplateCircle
			{
				X = centroNormalizado.X,
				Y = centroNormalizado.Y,
				Raio = raio,
				Stroke = "#FF1E90FF",
				StrokeThickness = 1.0
			});
		}

		private void Finalizar(Point pontoRaio, bool manterFerramentaAtiva)
		{
			ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();

			if (viewModel == null || !_centro.HasValue)
			{
				Cancelar();
				return;
			}

			Point centroNormalizado = NormalizarPonto(_centro.Value, viewModel);
			Point pontoFinalNormalizado = NormalizarPonto(pontoRaio, viewModel);
			double raio = LimitarRaioAoTipoPrancha(
				centroNormalizado,
				CalcularRaio(centroNormalizado, pontoFinalNormalizado),
				viewModel);

			LimparPreview();
			_centro = null;

			if (raio < RaioMinimo)
				return;

			ProjectSheetTemplateCircle? circulo = _inserirCirculo.Inserir(
				viewModel.Id,
				centroNormalizado.X,
				centroNormalizado.Y,
				raio);

			if (circulo == null)
				return;

			viewModel.Refresh();

			if (!manterFerramentaAtiva)
				_voltarParaSelecao();
		}

		private void LimparPreview()
		{
			ProjectSheetTypeViewModel? viewModel = _obterViewModelAtivo();
			viewModel?.SetPreviewCircle(null);
		}

		private static Point NormalizarPonto(Point ponto, ProjectSheetTypeViewModel viewModel)
		{
			return new Point(
				Math.Max(0, Math.Min(viewModel.SheetWidth, ponto.X)),
				Math.Max(0, Math.Min(viewModel.SheetHeight, ponto.Y)));
		}

		private static double LimitarRaioAoTipoPrancha(Point centro, double raio, ProjectSheetTypeViewModel viewModel)
		{
			double raioMaximo = Math.Min(
				Math.Min(centro.X, centro.Y),
				Math.Min(Math.Max(0.0, viewModel.SheetWidth - centro.X), Math.Max(0.0, viewModel.SheetHeight - centro.Y)));

			if (raioMaximo < ProjectSheetTemplateCircle.MinRadius)
				return ProjectSheetTemplateCircle.MinRadius;

			return Math.Max(ProjectSheetTemplateCircle.MinRadius, Math.Min(raio, raioMaximo));
		}

		private static double CalcularRaio(Point centro, Point ponto)
		{
			double dx = ponto.X - centro.X;
			double dy = ponto.Y - centro.Y;
			return Math.Sqrt(dx * dx + dy * dy);
		}
	}
}
