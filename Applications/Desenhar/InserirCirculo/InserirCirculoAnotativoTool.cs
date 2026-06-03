using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Abstractions;
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

namespace Araci.Applications.Desenhar.InserirCirculo
{
    public class InserirCirculoAnotativoTool : ITool
    {
        private const double RaioMinimo = 1.0;
        private const double SnapTolerance = 10.0;

        private readonly CommandHistoryManager _commands;
        private readonly AraciDocument _document;
        private readonly NameService _names;
        private readonly ElementoFactory _factory;
        private readonly CoreScene _scene;
        private readonly ISceneQueryService _sceneQueries;
        private readonly Action _voltarParaSelecao;
        private Point? _centro;
        private CirculoAnotativoViewModel? _preview;

        public InserirCirculoAnotativoTool(CommandHistoryManager commands, AraciDocument document, NameService names, ElementoFactory factory, CoreScene scene, ISceneQueryService sceneQueries, Action voltarParaSelecao)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _voltarParaSelecao = voltarParaSelecao ?? throw new ArgumentNullException(nameof(voltarParaSelecao));
        }

        public string Nome => "Círculo";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _centro.HasValue || _preview != null;

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
            Point ponto = AplicarSnapQuadrante(position);

            if (!_centro.HasValue)
            {
                Iniciar(ponto);
                return;
            }

            Finalizar(ponto);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (!_centro.HasValue)
                return;

            AtualizarPreview(AplicarSnapQuadrante(position));
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

        private void Iniciar(Point centro)
        {
            _centro = centro;
            AtualizarPreview(centro);
        }

        private void Finalizar(Point pontoRaio)
        {
            Point centro = _centro!.Value;
            double raio = CalcularRaio(centro, pontoRaio);

            if (raio < RaioMinimo)
            {
                AtualizarPreview(pontoRaio);
                return;
            }

            CirculoAnotativo circulo = _factory.CriarModelo<CirculoAnotativo>(ElementKinds.CirculoAnotativo);
            circulo.PosicaoX = centro.X;
            circulo.PosicaoY = centro.Y;
            circulo.Raio = raio;
            _commands.Execute(new AddElementoCommand(circulo, _document, _names));
            _sceneQueries.Invalidate();
            LimparPreview();
            _centro = null;
            _voltarParaSelecao();
        }

        private void AtualizarPreview(Point pontoRaio)
        {
            if (!_centro.HasValue)
                return;

            CirculoAnotativoViewModel preview = ObterPreview();
            Point centro = _centro.Value;
            preview.Circulo.PosicaoX = centro.X;
            preview.Circulo.PosicaoY = centro.Y;
            preview.Circulo.Raio = Math.Max(RaioMinimo, CalcularRaio(centro, pontoRaio));
            preview.AtualizarAposModeloAlterado();
            _sceneQueries.Invalidate();
        }

        private CirculoAnotativoViewModel ObterPreview()
        {
            if (_preview != null)
                return _preview;

            CirculoAnotativo circulo = _factory.CriarModelo<CirculoAnotativo>(ElementKinds.CirculoAnotativo);

            if (_factory.CriarViewModel(circulo) is not CirculoAnotativoViewModel preview)
                throw new InvalidOperationException("Nao foi possivel criar preview de CirculoAnotativo.");

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

        private Point AplicarSnapQuadrante(Point ponto)
        {
            return TrySnapQuadrante(ponto, out Point snap) ? snap : ponto;
        }

        private bool TrySnapQuadrante(Point ponto, out Point snap)
        {
            double melhor = SnapTolerance * SnapTolerance;
            snap = ponto;
            bool encontrou = false;

            foreach (CirculoAnotativoViewModel circulo in _scene.Elementos.OfType<CirculoAnotativoViewModel>())
            {
                if (circulo.IsPreview)
                    continue;

                Point centro = new(circulo.Circulo.PosicaoX, circulo.Circulo.PosicaoY);
                double raio = circulo.Circulo.Raio;

                if (TestarQuadrante(ponto, new Point(centro.X, centro.Y - raio), ref snap, ref melhor))
                    encontrou = true;

                if (TestarQuadrante(ponto, new Point(centro.X + raio, centro.Y), ref snap, ref melhor))
                    encontrou = true;

                if (TestarQuadrante(ponto, new Point(centro.X, centro.Y + raio), ref snap, ref melhor))
                    encontrou = true;

                if (TestarQuadrante(ponto, new Point(centro.X - raio, centro.Y), ref snap, ref melhor))
                    encontrou = true;
            }

            return encontrou;
        }

        private static bool TestarQuadrante(Point ponto, Point candidato, ref Point snap, ref double melhor)
        {
            double dx = ponto.X - candidato.X;
            double dy = ponto.Y - candidato.Y;
            double distancia = dx * dx + dy * dy;

            if (distancia > melhor)
                return false;

            melhor = distancia;
            snap = candidato;
            return true;
        }

        private static double CalcularRaio(Point centro, Point ponto)
        {
            double dx = ponto.X - centro.X;
            double dy = ponto.Y - centro.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}