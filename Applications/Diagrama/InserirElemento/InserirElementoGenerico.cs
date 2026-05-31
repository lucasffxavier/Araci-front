using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirElemento
{
    public class InserirElementoGenericoTool : ITool
    {
        private readonly EditorContext _context;
        private readonly ElementDefinition _definition;
        private readonly InsertPreviewController<ElementoViewModel, Elemento> _preview;

        public InserirElementoGenericoTool(EditorContext context, ElementDefinition definition)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _preview = new InsertPreviewController<ElementoViewModel, Elemento>(_context, CriarPreview, vm => vm.Modelo);
        }

        public string Nome => $"Inserir {_definition.NomeAmigavel}";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _preview.HasPreview;

        public void Ativar() { }

        public void Desativar()
        {
            _preview.Clear();
        }

        public void Cancelar()
        {
            _preview.Clear();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            if (_preview.IsPreview(vm))
                vm = null;

            _preview.Update(position, vm);
            Elemento modeloPreview = _preview.ObterModeloPreview();
            double x = modeloPreview.PosicaoX;
            double y = modeloPreview.PosicaoY;
            double rotacao = _preview.CurrentRotation;
            _preview.Clear();
            _context.InserirElemento.Executar(_definition.Kind, x, y, rotacao);
            _context.Tools.VoltarParaSelecao();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            _preview.Update(position);
        }

        public void OnMouseUp(Point position, ToolInputState inputState) { }

        public bool HandlesKey(Key key)
        {
            return key is Key.Escape or Key.Space;
        }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Escape)
            {
                _preview.Clear();
                _context.Tools.VoltarParaSelecao();
                return;
            }

            if (key == Key.Space)
                _preview.RotateClockwise();
        }

        private ElementoViewModel CriarPreview()
        {
            return _context.ElementoFactory.CriarViewModel(_definition.Kind);
        }
    }
}
