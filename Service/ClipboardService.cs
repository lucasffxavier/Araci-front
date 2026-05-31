using System;
using System.Collections.Generic;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public static class ClipboardService
    {
        public static void CopiarSelecionados(EditorContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            context.CopiarElementos.Executar(context.Selection.Selecionados);
        }

        public static void Colar(EditorContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            IReadOnlyList<Elemento> colados = context.ColarElementos.Executar();

            if (colados.Count == 0)
                return;

            context.Selection.Limpar();

            foreach (Elemento elemento in colados)
            {
                ElementoViewModel? vm = context.Viewport?.ObterViewModel(elemento);
                if (vm != null)
                    context.Selection.Selecionar(vm, true);
            }

            context.SceneQueries.Invalidate();
            context.CableVertexEdit.Refresh();
        }
    }
}
