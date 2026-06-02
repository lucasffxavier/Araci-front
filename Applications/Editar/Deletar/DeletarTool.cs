using System.Linq;
using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Services;
using Araci.ViewModels;
using Araci.Services.Editing;

namespace Araci.Applications.Editar.Deletar
{
    public class DeletarTool : ITool
    {
        private readonly SafeDeleteService _safeDelete;

        public DeletarTool(SafeDeleteService safeDelete)
        {
            _safeDelete = safeDelete
                ?? throw new System.ArgumentNullException(nameof(safeDelete));
        }

        public string Nome => "Deletar";

        public bool MantemBotaoAtivado => true;

        public bool IsBusy => false;

        public void Ativar() { }

        public void Desativar() { }

        public void Cancelar() { }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position,
            ToolInputState inputState)
        {
            _safeDelete.DeleteSelection();
        }

        public void OnMouseMove(Point position, ToolInputState inputState) { }

        public void OnMouseUp(Point position, ToolInputState inputState) { }

        public void OnKeyDown(Key key) { }
    }
}
