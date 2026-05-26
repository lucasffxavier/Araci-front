using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirGerador
{
    public class InserirGeradorApplication
    {
        private readonly EditorContext _context;

        public InserirGeradorApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirGeradorTool(_context);
        }
    }

    public class InserirGeradorTool : ITool
    {
        private readonly EditorContext _context;

        public InserirGeradorTool(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Nome => "Inserir Gerador";

        public bool MantemBotaoAtivado => true;

        public bool IsBusy => false;

        public void Ativar() { }

        public void Desativar() { }

        public void Cancelar() { }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            Point pontoSnap = _context.Snap.SnapFromElemento(vm, position);
            Gerador gerador = _context.ElementoFactory.CriarGerador();
            Point posicao = CalcularPosicaoPorCentro(pontoSnap, 70, 70);
            gerador.PosicaoX = posicao.X;
            gerador.PosicaoY = posicao.Y;

            _context.Commands.Execute(new AddElementoCommand(gerador, _context));
            _context.Tools.VoltarParaSelecao();
        }

        public void OnMouseMove(Point position, ToolInputState inputState) { }

        public void OnMouseUp(Point position, ToolInputState inputState) { }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Escape)
                _context.Tools.VoltarParaSelecao();
        }

        private static Point CalcularPosicaoPorCentro(Point centro, double largura, double altura)
        {
            return new Point(
                centro.X - largura / 2,
                centro.Y - altura / 2);
        }
    }
}
