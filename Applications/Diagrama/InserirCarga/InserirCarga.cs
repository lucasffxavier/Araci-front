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
        private CargaViewModel? _preview;

        public InserirCargaTool(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Nome => "Inserir Carga";

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
            Carga carga = _context.ElementoFactory.CriarCarga();
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(carga, pontoSnap);
            carga.PosicaoX = posicao.X;
            carga.PosicaoY = posicao.Y;

            LimparPreview();
            _context.Commands.Execute(new AddElementoCommand(carga, _context));
            _context.Tools.VoltarParaSelecao();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            Point pontoSnap = _context.Snap.Snap(position);
            CargaViewModel preview = ObterPreview();
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(preview.Carga, pontoSnap);
            preview.Carga.PosicaoX = posicao.X;
            preview.Carga.PosicaoY = posicao.Y;
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

        private CargaViewModel ObterPreview()
        {
            if (_preview != null)
                return _preview;

            _preview = _context.ElementoFactory.CriarCargaVM();
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
