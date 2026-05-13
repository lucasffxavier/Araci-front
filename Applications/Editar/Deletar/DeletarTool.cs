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
        public string Nome =>
            "Deletar";

        public bool MantemBotaoAtivado =>
            true;

        public void Ativar()
        {
        }

        public void Desativar()
        {
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            var selecionados =
                SelectionService
                    .Selecionados
                    .ToList();

            if (selecionados.Count == 0)
                return;

            var composite =
                new CompositeCommand();

            foreach (var item in selecionados)
            {
                composite.Add(
                    new DeleteElementCommand(
                        item));
            }

            AppServices.Commands
                .Execute(composite);
        }

        public void OnMouseMove(
            Point position)
        {
        }

        public void OnMouseUp(
            Point position)
        {
        }

        public void OnKeyDown(
            Key key)
        {
        }
    }
}