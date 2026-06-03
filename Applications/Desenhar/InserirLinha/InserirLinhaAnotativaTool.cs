using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;
using Araci.Applications.Factories;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.Services.Naming;
using Araci.ViewModels;
using CoreScene = Araci.Core.Scenes.Scene;
using CommandHistoryManager = Araci.Core.Commands.CommandManager;

namespace Araci.Applications.Desenhar.InserirLinha
{
    public class InserirLinhaAnotativaTool : ITool
    {
        private const double ToleranciaDistanciaQuadrada = 0.0001;

        private readonly CommandHistoryManager _commands;
        private readonly AraciDocument _document;
        private readonly NameService _names;
        private readonly ElementoFactory _factory;
        private readonly CoreScene _scene;
        private readonly ISceneQueryService _sceneQueries;
        private readonly LinhaEndpointEditService _linhaEndpointEdit;
        private readonly Action _voltarParaSelecao;
        private Point? _pontoInicial;
        private LinhaAnotativaViewModel? _preview;

        public InserirLinhaAnotativaTool(
            CommandHistoryManager commands,
            AraciDocument document,
            NameService names,
            ElementoFactory factory,
            CoreScene scene,
            ISceneQueryService sceneQueries,
            LinhaEndpointEditService linhaEndpointEdit,
            Action voltarParaSelecao)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _linhaEndpointEdit = linhaEndpointEdit ?? throw new ArgumentNullException(nameof(linhaEndpointEdit));
            _voltarParaSelecao = voltarParaSelecao ?? throw new ArgumentNullException(nameof(voltarParaSelecao));
        }

        public string Nome => "Linha";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _pontoInicial.HasValue || _preview != null;

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
            _linhaEndpointEdit.LimparSnapInsercao();
            _pontoInicial = null;
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            Point? snap = _linhaEndpointEdit.AtualizarSnapInsercao(_scene.Elementos, position);
            Point ponto = snap ?? position;

            if (!_pontoInicial.HasValue)
            {
                Iniciar(ponto);
                return;
            }

            FinalizarSegmento(
                ponto,
                inputState.IsShiftPressed && !snap.HasValue,
                inputState.IsControlPressed);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            Point? snap = _linhaEndpointEdit.AtualizarSnapInsercao(_scene.Elementos, position);
            Point ponto = snap ?? position;

            if (!_pontoInicial.HasValue)
                return;

            AtualizarPreview(ponto, inputState.IsShiftPressed && !snap.HasValue);
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

        private void Iniciar(Point ponto)
        {
            _pontoInicial = ponto;
            AtualizarPreview(ponto);
        }

        private void FinalizarSegmento(Point pontoFinal, bool ortogonalizar, bool manterAtiva)
        {
            Point pontoInicial = _pontoInicial!.Value;
            Point pontoFinalAjustado = ortogonalizar
                ? AplicarOrtogonalizacao(pontoFinal, pontoInicial)
                : pontoFinal;

            if (DistanciaQuadrada(pontoInicial, pontoFinalAjustado) < ToleranciaDistanciaQuadrada)
            {
                AtualizarPreview(pontoFinalAjustado);
                return;
            }

            LinhaAnotativa linha = _factory.CriarModelo<LinhaAnotativa>(ElementKinds.LinhaAnotativa);

            linha.PosicaoX = pontoInicial.X;
            linha.PosicaoY = pontoInicial.Y;
            linha.X2 = pontoFinalAjustado.X - pontoInicial.X;
            linha.Y2 = pontoFinalAjustado.Y - pontoInicial.Y;

            _commands.Execute(new AddElementoCommand(linha, _document, _names));
            _sceneQueries.Invalidate();

            if (manterAtiva)
            {
                _pontoInicial = pontoFinalAjustado;
                AtualizarPreview(pontoFinalAjustado);
                return;
            }

            LimparPreview();
            _linhaEndpointEdit.LimparSnapInsercao();
            _pontoInicial = null;
            _voltarParaSelecao();
        }

        private void AtualizarPreview(Point pontoFinal)
        {
            AtualizarPreview(pontoFinal, false);
        }

        private void AtualizarPreview(Point pontoFinal, bool ortogonalizar)
        {
            if (!_pontoInicial.HasValue)
                return;

            LinhaAnotativaViewModel preview = ObterPreview();
            Point pontoInicial = _pontoInicial.Value;
            Point pontoFinalAjustado = ortogonalizar
                ? AplicarOrtogonalizacao(pontoFinal, pontoInicial)
                : pontoFinal;

            preview.Linha.PosicaoX = pontoInicial.X;
            preview.Linha.PosicaoY = pontoInicial.Y;
            preview.Linha.X2 = pontoFinalAjustado.X - pontoInicial.X;
            preview.Linha.Y2 = pontoFinalAjustado.Y - pontoInicial.Y;
            preview.AtualizarAposModeloAlterado();
            _sceneQueries.Invalidate();
        }

        private LinhaAnotativaViewModel ObterPreview()
        {
            if (_preview != null)
                return _preview;

            LinhaAnotativa linha = _factory.CriarModelo<LinhaAnotativa>(ElementKinds.LinhaAnotativa);

            if (_factory.CriarViewModel(linha) is not LinhaAnotativaViewModel preview)
                throw new InvalidOperationException("Nao foi possivel criar preview de LinhaAnotativa.");

            preview.IsPreview = true;
            _preview = preview;
            _scene.Elementos.Add(preview);
            _sceneQueries.Invalidate();
            return preview;
        }

        private void LimparPreview()
        {
            if (_preview == null)
                return;

            _preview.IsPreview = false;
            _scene.Elementos.Remove(_preview);
            _preview = null;
            _sceneQueries.Invalidate();
        }

        private static double DistanciaQuadrada(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return dx * dx + dy * dy;
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
    }
}