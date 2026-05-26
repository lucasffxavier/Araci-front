using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCarga
{
    public class InserirCargaApplication
    {
        private readonly EditorContext _context;

        public InserirCargaApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirCargaTool(_context);
        }
    }

    public class InserirCargaTool : ITool
    {
        private readonly EditorContext _context;

        public InserirCargaTool(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Nome => "Inserir Carga";

        public bool MantemBotaoAtivado => true;

        public bool IsBusy => false;

        public void Ativar() { }

        public void Desativar() { }

        public void Cancelar() { }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            Point pontoSnap = _context.Snap.SnapFromElemento(vm, position);
            Carga carga = _context.ElementoFactory.CriarCarga();
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(carga, pontoSnap);
            carga.PosicaoX = posicao.X;
            carga.PosicaoY = posicao.Y;

            _context.Commands.Execute(new AddElementoCommand(carga, _context));
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
