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

        public void Ativar()
        {
        }

        public void Desativar()
        {
        }

        // =========================
        // MOUSE DOWN
        // =========================

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position)
        {
            // =========================
            // CLICK DIRETO NO ELEMENTO
            // =========================

            if (vm != null)
            {
                ExecutarDelete(
                    new[] { vm });

                return;
            }

            // =========================
            // DELETE DA SELEÇÃO
            // =========================

            var selecionados =
                SelectionService
                    .Selecionados
                    .ToList();

            if (selecionados.Count == 0)
                return;

            ExecutarDelete(
                selecionados);
        }

        // =========================
        // EXECUTAR DELETE
        // =========================

        private void ExecutarDelete(
            System.Collections.Generic.IEnumerable<ElementoViewModel> elementos)
        {
            var lista =
                elementos
                    .Distinct()
                    .ToList();

            if (lista.Count == 0)
                return;

            AppServices.Commands
                .BeginTransaction();

            foreach (var item in lista)
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