using System.Collections.Generic;

namespace Araci.Applications.Projects.Tables
{
    public sealed class ProjectTableDataResult
    {
        public ProjectTableDataResult(
            IReadOnlyList<ProjectTableDataColumn> columns,
            IReadOnlyList<ProjectTableDataRow> rows)
        {
            Columns = columns;
            Rows = rows;
        }

        public IReadOnlyList<ProjectTableDataColumn> Columns { get; }
        public IReadOnlyList<ProjectTableDataRow> Rows { get; }
    }
}
