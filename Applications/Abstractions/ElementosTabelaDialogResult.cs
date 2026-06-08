using System.Collections.Generic;
using Araci.Core.Documents;

namespace Araci.Applications.Abstractions
{
    public sealed class ElementosTabelaDialogResult
    {
        public ElementosTabelaDialogResult(
            IReadOnlyList<ProjectTableElementCategory> categorias,
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados)
        {
            Categorias = categorias;
            CamposSelecionados = camposSelecionados;
        }

        public IReadOnlyList<ProjectTableElementCategory> Categorias { get; }
        public IReadOnlyList<ProjectTableFieldSelection> CamposSelecionados { get; }
    }
}
