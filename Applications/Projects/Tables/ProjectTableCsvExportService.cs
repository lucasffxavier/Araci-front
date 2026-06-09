using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Araci.Applications.Projects.Tables
{
    public sealed class ProjectTableCsvExportService : IProjectTableCsvExportService
    {
        private const char Delimiter = ';';

        public string GenerateCsv(ProjectTableDataResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            var builder = new StringBuilder();
            AppendLine(builder, result.Columns.Select(c => c.NomeExibicao));

            foreach (ProjectTableDataRow row in result.Rows)
                AppendLine(builder, row.Cells.Select(c => c.DisplayValue));

            return builder.ToString();
        }

        private static void AppendLine(StringBuilder builder, IEnumerable<string> values)
        {
            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append(string.Join(Delimiter, values.Select(Escape)));
        }

        private static string Escape(string? value)
        {
            string text = value ?? string.Empty;
            bool quote = text.Contains(Delimiter) ||
                text.Contains('"') ||
                text.Contains('\n') ||
                text.Contains('\r');

            if (!quote)
                return text;

            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
    }
}
