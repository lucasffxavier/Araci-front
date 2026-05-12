using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Mover
{
    public class MoverTool : ITool
    {
        private ElementoViewModel? _selecionado;
        private Point _ultimoPonto;
        private bool _arrastando;

        public string Nome => "Mover";

        public bool MantemBotaoAtivado => true;

        public void Ativar() { }

        public void Desativar()
        {
            _arrastando = false;
            AppServices.MoveHud.Visivel = false;
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position)
        {
            if (vm == null)
                return;

            SelectionService.Selecionar(vm);

            _selecionado = vm;
            _ultimoPonto = position;
            _arrastando = true;

            var hud = AppServices.MoveHud;
            hud.Reset();
            hud.Visivel = true;
        }

        public void OnMouseMove(Point position)
        {
            if (!_arrastando || _selecionado == null)
                return;

            Vector delta = position - _ultimoPonto;

            MoveService.Mover(_selecionado, delta);

            _ultimoPonto = position;

            var hud = AppServices.MoveHud;

            hud.X = position.X + 20;
            hud.Y = position.Y - 10;

            hud.DeltaX += delta.X;
            hud.DeltaY += delta.Y;
        }

        public void OnMouseUp(Point position)
        {
            _arrastando = false;
            _selecionado = null;

            AppServices.MoveHud.Visivel = false;
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}