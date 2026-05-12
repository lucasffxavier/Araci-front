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
        public string Nome => "Deletar";

        public bool MantemBotaoAtivado => true;

        public void Ativar() { }

        public void Desativar() { }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            var selecionados =
                AppServices.Editor
                    .ElementosSelecionados
                    .ToList();

            if (selecionados.Count == 0)
                return;

            AppServices.Commands
                .BeginTransaction();

            foreach (var item in selecionados)
            {
                AppServices.Commands.Execute(
                    new DeleteElementCommand(item));
            }

            AppServices.Commands
                .CommitTransaction();
        }

        public void OnMouseMove(Point position)
        {
        }

        public void OnMouseUp(Point position)
        {
        }

        public void OnKeyDown(Key key)
        {
        }
    }
}