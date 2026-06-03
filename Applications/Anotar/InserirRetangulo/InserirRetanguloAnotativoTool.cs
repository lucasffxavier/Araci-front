using System;
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

namespace Araci.Applications.Anotar.InserirRetangulo
{
    public class InserirRetanguloAnotativoTool : ITool
    {
        private const double ToleranciaArea = 0.0001;

        private readonly CommandHistoryManager _commands;
        private readonly AraciDocument _document;
        private readonly NameService _names;
        private readonly ElementoFactory _factory;
        private readonly CoreScene _scene;
        private readonly ISceneQueryService _sceneQueries;
        private readonly Action _voltarParaSelecao;
        private Point? _pontoInicial;
        private RetanguloAnotativoViewModel? _preview;

        public InserirRetanguloAnotativoTool(
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

        public string Nome => "Retângulo";
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

            Finalizar(position, inputState.IsShiftPressed);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (!_pontoInicial.HasValue)
                return;

            AtualizarPreview(position, inputState.IsShiftPressed);
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
            AtualizarPreview(ponto, false);
        }

        private void Finalizar(Point pontoFinal, bool quadrado)
        {
            Point inicio = _pontoInicial!.Value;
            Rect rect = CriarRect(inicio, pontoFinal, quadrado);

            if (rect.Width * rect.Height < ToleranciaArea)
            {
                AtualizarPreview(pontoFinal, quadrado);
                return;
            }

            RetanguloAnotativo retangulo = _factory.CriarModelo<RetanguloAnotativo>(ElementKinds.RetanguloAnotativo);
            AplicarRect(retangulo, rect);
            _commands.Execute(new AddElementoCommand(retangulo, _document, _names));
            _sceneQueries.Invalidate();
            LimparPreview();
            _pontoInicial = null;
            _voltarParaSelecao();
        }

        private void AtualizarPreview(Point pontoFinal, bool quadrado)
        {
            if (!_pontoInicial.HasValue)
                return;

            RetanguloAnotativoViewModel preview = ObterPreview();
            Rect rect = CriarRect(_pontoInicial.Value, pontoFinal, quadrado);
            AplicarRect(preview.Retangulo, rect);
            preview.AtualizarAposModeloAlterado();
            _sceneQueries.Invalidate();
        }

        private RetanguloAnotativoViewModel ObterPreview()
        {
            if (_preview != null)
                return _preview;

            RetanguloAnotativo retangulo = _factory.CriarModelo<RetanguloAnotativo>(ElementKinds.RetanguloAnotativo);

            if (_factory.CriarViewModel(retangulo) is not RetanguloAnotativoViewModel preview)
                throw new InvalidOperationException("Nao foi possivel criar preview de RetanguloAnotativo.");

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

        private static Rect CriarRect(Point inicio, Point fim, bool quadrado)
        {
            Vector delta = fim - inicio;

            if (quadrado)
            {
                double lado = Math.Min(Math.Abs(delta.X), Math.Abs(delta.Y));

                if (lado < 0.0001)
                    lado = Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y));

                fim = new Point(
                    inicio.X + Math.Sign(delta.X == 0 ? 1 : delta.X) * lado,
                    inicio.Y + Math.Sign(delta.Y == 0 ? 1 : delta.Y) * lado);
            }

            double x = Math.Min(inicio.X, fim.X);
            double y = Math.Min(inicio.Y, fim.Y);
            double largura = Math.Abs(fim.X - inicio.X);
            double altura = Math.Abs(fim.Y - inicio.Y);

            return new Rect(x, y, largura, altura);
        }

        private static void AplicarRect(RetanguloAnotativo retangulo, Rect rect)
        {
            retangulo.PosicaoX = rect.X;
            retangulo.PosicaoY = rect.Y;
            retangulo.Largura = rect.Width;
            retangulo.Altura = rect.Height;
        }
    }
}