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
        private BarraViewModel? _preview;

        public InserirBarraTool(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Nome => "Inserir Barra";

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
            Barra barra = _context.ElementoFactory.CriarBarra();
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(barra, pontoSnap);
            barra.PosicaoX = posicao.X;
            barra.PosicaoY = posicao.Y;

            LimparPreview();
            _context.Commands.Execute(new AddElementoCommand(barra, _context));
            _context.Tools.VoltarParaSelecao();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            Point pontoSnap = _context.Snap.Snap(position);
            BarraViewModel preview = ObterPreview();
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(preview.Barra, pontoSnap);
            preview.Barra.PosicaoX = posicao.X;
            preview.Barra.PosicaoY = posicao.Y;
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

        private BarraViewModel ObterPreview()
        {
            if (_preview != null)
                return _preview;

            _preview = _context.ElementoFactory.CriarBarraVM();
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
