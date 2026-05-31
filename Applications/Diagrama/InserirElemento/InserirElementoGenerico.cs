using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Applications.UseCases.Diagrama;
using Araci.Core.SceneQueries;
using Araci.Core.Scenes;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirElemento
{
    public class InserirElementoGenericoTool : ITool
    {
        private readonly ElementDefinition _definition;
        private readonly ElementoFactory _factory;
        private readonly InserirElementoUseCase _inserirElemento;
        private readonly Action _voltarParaSelecao;
        private readonly InsertPreviewController<ElementoViewModel, Elemento> _preview;

        public InserirElementoGenericoTool(
            ElementDefinition definition,
            ElementoFactory factory,
            InserirElementoUseCase inserirElemento,
            SnapService snap,
            ElementGeometryService geometry,
            TerminalLayoutService terminalLayout,
            AlignmentGuideService alignmentGuides,
            Scene scene,
            ISceneQueryService sceneQueries,
            Action voltarParaSelecao)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _inserirElemento = inserirElemento ?? throw new ArgumentNullException(nameof(inserirElemento));
            _voltarParaSelecao = voltarParaSelecao ?? throw new ArgumentNullException(nameof(voltarParaSelecao));
            _preview = new InsertPreviewController<ElementoViewModel, Elemento>(
                CriarPreview,
                vm => vm.Modelo,
                snap,
                geometry,
                terminalLayout,
                alignmentGuides,
                scene,
                sceneQueries);
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
            _inserirElemento.Executar(_definition.Kind, x, y, rotacao);
            _voltarParaSelecao();
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
                _voltarParaSelecao();
                return;
            }

            if (key == Key.Space)
                _preview.RotateClockwise();
        }

        private ElementoViewModel CriarPreview()
        {
            return _factory.CriarViewModel(_definition.Kind);
        }
    }
}
