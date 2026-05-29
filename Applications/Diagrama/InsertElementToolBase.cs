using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama
{
    public abstract class InsertElementToolBase<TViewModel, TModel> : ITool
        where TViewModel : ElementoViewModel
        where TModel : Elemento
    {
        private readonly InsertPreviewController<TViewModel, TModel> _preview;

        protected InsertElementToolBase(EditorContext context, Func<TViewModel> criarPreview, Func<TViewModel, TModel> obterModeloPreview)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            _preview = new InsertPreviewController<TViewModel, TModel>(Context, criarPreview, obterModeloPreview);
        }

        protected EditorContext Context { get; }
        protected abstract string ToolName { get; }
        public string Nome => ToolName;
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
            TModel modeloPreview = _preview.ObterModeloPreview();
            TModel modelo = CriarModeloReal();
            modelo.PosicaoX = modeloPreview.PosicaoX;
            modelo.PosicaoY = modeloPreview.PosicaoY;
            modelo.Rotacao = _preview.CurrentRotation;
            Context.TerminalLayout.AtualizarTerminais(modelo);
            _preview.Clear();
            Context.Commands.Execute(new AddElementoCommand(modelo, Context));
            Context.Tools.VoltarParaSelecao();
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
                Context.Tools.VoltarParaSelecao();
                return;
            }

            if (key == Key.Space)
                _preview.RotateClockwise();
        }

        protected abstract TModel CriarModeloReal();
    }
}