// =========================
// ARQUIVO: Applications/Editar/Mover/MoverTool.cs
// =========================

using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Mover
{
    public class MoverTool : ITool
    {
        // =========================
        // TOOL INTERNA
        // =========================

        private readonly SelecionarTool _selecionarTool;

        // =========================
        // INFO TOOL
        // =========================

        public string Nome => "Mover";

        public bool MantemBotaoAtivado => true;

        // =========================
        // CONSTRUTOR
        // =========================

        public MoverTool()
        {
            _selecionarTool = new SelecionarTool(
                modoSoMover: true,
                mostrarHud: true);
        }

        // =========================
        // ATIVAR
        // =========================

        public void Ativar()
        {
            _selecionarTool.Ativar();
        }

        // =========================
        // DESATIVAR
        // =========================

        public void Desativar()
        {
            _selecionarTool.Desativar();
        }

        // =========================
        // MOUSE DOWN
        // =========================

        public void OnMouseDown(ElementoViewModel? vm, Point position)
        {
            _selecionarTool.OnMouseDown(vm, position);
        }

        // =========================
        // MOUSE MOVE
        // =========================

        public void OnMouseMove(Point position)
        {
            _selecionarTool.OnMouseMove(position);
        }

        // =========================
        // MOUSE UP
        // =========================

        public void OnMouseUp(Point position)
        {
            _selecionarTool.OnMouseUp(position);
        }

        // =========================
        // KEYBOARD
        // =========================

        public void OnKeyDown(Key key)
        {
            _selecionarTool.OnKeyDown(key);
        }
    }
}