using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;
using Araci.Core.SceneQueries;
using Araci.Services;
using Araci.ViewModels;
using Araci.Services.Editing;

namespace Araci.Applications.Editar.Mover
{
    public class MoverTool : ITool
    {
        private readonly SelecionarTool _selecionarTool;

        public MoverTool(
            ISceneQueryService queries,
            SelectionService selection,
            SelectionBoxViewModel selectionBox,
            CableVertexEditService cableVertexEdit,
            BarraResizeService barraResize,
            MoveService move,
            MoveHudService moveHud,
            AlignmentGuideService alignmentGuides,
            MoveConstraintService moveConstraints,
            RotationService rotation)
        {
            _selecionarTool = new SelecionarTool(
                queries,
                selection,
                selectionBox,
                cableVertexEdit,
                barraResize,
                move,
                moveHud,
                alignmentGuides,
                moveConstraints,
                rotation,
                modoSoMover: true,
                mostrarHud: true);
        }

        public string Nome => "Mover";
        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _selecionarTool.IsBusy;

        public void Ativar()
        {
            _selecionarTool.Ativar();
        }

        public void Desativar()
        {
            _selecionarTool.Desativar();
        }

        public void Cancelar()
        {
            _selecionarTool.Cancelar();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            _selecionarTool.OnMouseDown(vm, position, inputState);
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            _selecionarTool.OnMouseMove(position, inputState);
        }

        public void OnMouseUp(Point position, ToolInputState inputState)
        {
            _selecionarTool.OnMouseUp(position, inputState);
        }

        public void OnKeyDown(Key key)
        {
            _selecionarTool.OnKeyDown(key);
        }
    }
}
