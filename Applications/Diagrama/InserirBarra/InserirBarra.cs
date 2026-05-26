using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirBarra
{
    public class InserirBarraApplication
    {
        private readonly EditorContext _context;

        public InserirBarraApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirBarraTool(_context);
        }
    }

    public class InserirBarraTool : ITool
    {
        private readonly EditorContext _context;

        public InserirBarraTool(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Nome => "Inserir Barra";

        public bool MantemBotaoAtivado => true;

        public bool IsBusy => false;

        public void Ativar() { }

        public void Desativar() { }

        public void Cancelar() { }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            Point pontoSnap = _context.Snap.SnapFromElemento(vm, position);
            Barra barra = _context.ElementoFactory.CriarBarra();
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(barra, pontoSnap);
            barra.PosicaoX = posicao.X;
            barra.PosicaoY = posicao.Y;

            _context.Commands.Execute(new AddElementoCommand(barra, _context));
            _context.Tools.VoltarParaSelecao();
        }

        public void OnMouseMove(Point position, ToolInputState inputState) { }

        public void OnMouseUp(Point position, ToolInputState inputState) { }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Escape)
                _context.Tools.VoltarParaSelecao();
        }
    }
}
