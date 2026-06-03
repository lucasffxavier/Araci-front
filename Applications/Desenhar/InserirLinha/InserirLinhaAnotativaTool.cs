using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
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
            Action voltarParaSelecao)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
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
            _pontoInicial = null;
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            if (!_pontoInicial.HasValue)
            {
                Iniciar(position);
                return;
            }

            FinalizarSegmento(position, inputState.IsShiftPressed);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (!_pontoInicial.HasValue)
                return;

            AtualizarPreview(position);
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

        private void FinalizarSegmento(Point pontoFinal, bool manterAtiva)
        {
            Point pontoInicial = _pontoInicial!.Value;

            if (DistanciaQuadrada(pontoInicial, pontoFinal) < ToleranciaDistanciaQuadrada)
            {
                AtualizarPreview(pontoFinal);
                return;
            }

            var linha = new LinhaAnotativa
            {
                PosicaoX = pontoInicial.X,
                PosicaoY = pontoInicial.Y,
                X2 = pontoFinal.X - pontoInicial.X,
                Y2 = pontoFinal.Y - pontoInicial.Y
            };

            _commands.Execute(new AddElementoCommand(linha, _document, _names));
            _sceneQueries.Invalidate();

            if (manterAtiva)
            {
                _pontoInicial = pontoFinal;
                AtualizarPreview(pontoFinal);
                return;
            }

            LimparPreview();
            _pontoInicial = null;
            _voltarParaSelecao();
        }

        private void AtualizarPreview(Point pontoFinal)
        {
            if (!_pontoInicial.HasValue)
                return;

            LinhaAnotativaViewModel preview = ObterPreview();
            Point pontoInicial = _pontoInicial.Value;

            preview.Linha.PosicaoX = pontoInicial.X;
            preview.Linha.PosicaoY = pontoInicial.Y;
            preview.Linha.X2 = pontoFinal.X - pontoInicial.X;
            preview.Linha.Y2 = pontoFinal.Y - pontoInicial.Y;
            preview.AtualizarAposModeloAlterado();
            _sceneQueries.Invalidate();
        }

        private LinhaAnotativaViewModel ObterPreview()
        {
            if (_preview != null)
                return _preview;

            var linha = new LinhaAnotativa();

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
    }
}
