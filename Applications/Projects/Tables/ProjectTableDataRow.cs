using System;
using System.Collections.Generic;
using Araci.Core.Documents;

namespace Araci.Applications.Projects.Tables
{
    public sealed class ProjectTableDataRow
    {
        public ProjectTableDataRow(
            Guid elementoId,
            string elementoNome,
            ProjectTableElementCategory categoria,
            IReadOnlyList<ProjectTableDataCell> cells)
        {
            ElementoId = elementoId;
            ElementoNome = elementoNome;
            Categoria = categoria;
            Cells = cells;
        }

        public Guid ElementoId { get; }
        public string ElementoNome { get; }
        public ProjectTableElementCategory Categoria { get; }
        public IReadOnlyList<ProjectTableDataCell> Cells { get; }
    }
}
