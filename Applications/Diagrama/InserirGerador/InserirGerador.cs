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
        private GeradorViewModel? _preview;

        public InserirGeradorTool(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Nome => "Inserir Gerador";

        public bool MantemBotaoAtivado => true;

        public bool IsBusy => _preview != null;

        public void Ativar() { }

        public void Desativar()
        {
            LimparPreview();
        }

        public void Cancelar()
        {
            LimparPreview();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            if (ReferenceEquals(vm, _preview))
                vm = null;

            Point pontoSnap = _context.Snap.SnapFromElemento(vm, position);
            Gerador gerador = _context.ElementoFactory.CriarGerador();
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(gerador, pontoSnap);
            gerador.PosicaoX = posicao.X;
            gerador.PosicaoY = posicao.Y;

            LimparPreview();
            _context.Commands.Execute(new AddElementoCommand(gerador, _context));
            _context.Tools.VoltarParaSelecao();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            Point pontoSnap = _context.Snap.Snap(position);
            GeradorViewModel preview = ObterPreview();
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(preview.Gerador, pontoSnap);
            preview.Gerador.PosicaoX = posicao.X;
            preview.Gerador.PosicaoY = posicao.Y;
            preview.AtualizarAposModeloAlterado();
            _context.SceneQueries.Invalidate();
        }

        public void OnMouseUp(Point position, ToolInputState inputState) { }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Escape)
            {
                LimparPreview();
                _context.Tools.VoltarParaSelecao();
            }
        }

        private GeradorViewModel ObterPreview()
        {
            if (_preview != null)
                return _preview;

            _preview = _context.ElementoFactory.CriarGeradorVM();
            _context.Scene.Elementos.Add(_preview);
            _context.SceneQueries.Invalidate();

            return _preview;
        }

        private void LimparPreview()
        {
            if (_preview == null)
                return;

            _context.Scene.Elementos.Remove(_preview);
            _preview = null;
            _context.SceneQueries.Invalidate();
        }
    }
}
