using System.Linq;
using System.Windows;
using System.Windows.Input;

using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Deletar
{
    public class DeletarTool : ITool
    {
        private readonly EditorContext _context;

        public DeletarTool(EditorContext context)
        {
            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));
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
            var selecionados =
                _context.Selection
                    .Selecionados
                    .ToList();

            if (selecionados.Count == 0)
                return;

            using var tx =
                _context.BeginTransaction();

            foreach (var item in selecionados)
            {
                tx.Add(
                    new DeleteElementCommand(
                        item.Modelo,
                        _context));
            }

            tx.Commit();
            _context.Selection.Limpar();
        }

        public void OnMouseMove(Point position) { }

        public void OnMouseUp(Point position) { }

        public void OnKeyDown(Key key) { }
    }
}
