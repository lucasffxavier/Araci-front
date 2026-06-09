using System;
using System.IO;
using System.Linq;
using System.Text;
using Araci.Applications.Abstractions;
using Araci.Applications.Projects.Tables;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class ExportarTabelaUseCase
    {
        private readonly AraciDocument _document;
        private readonly IUserDialogService _dialogs;
        private readonly ProjectTableDataBuilder _builder;
        private readonly IProjectTableCsvExportService _csvExport;

        public ExportarTabelaUseCase(
            AraciDocument document,
            IUserDialogService dialogs,
            ProjectTableDataBuilder? builder = null,
            IProjectTableCsvExportService? csvExport = null)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _builder = builder ?? new ProjectTableDataBuilder();
            _csvExport = csvExport ?? new ProjectTableCsvExportService();
        }

        public bool Executar(ProjectTable? tabela)
        {
            if (tabela == null || !_document.Tabelas.Any(t => t.Id == tabela.Id))
            {
                _dialogs.ShowWarning("Exportar CSV", "Selecione uma tabela para exportar.");
                return false;
            }

            string? path = _dialogs.ShowSaveCsvDialog(CriarNomeArquivoSugerido(tabela.Nome));

            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                ProjectTableDataResult result = _builder.Build(_document, tabela);
                string csv = _csvExport.GenerateCsv(result);
                File.WriteAllText(path, csv, Encoding.UTF8);
                _dialogs.ShowInfo("Exportar CSV", "Tabela exportada com sucesso.");
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _dialogs.ShowError("Exportar CSV", $"Não foi possível salvar o arquivo CSV: {ex.Message}");
                return false;
            }
        }

        private static string CriarNomeArquivoSugerido(string nomeTabela)
        {
            string nome = string.IsNullOrWhiteSpace(nomeTabela) ? "Tabela" : nomeTabela.Trim();

            foreach (char invalid in Path.GetInvalidFileNameChars())
                nome = nome.Replace(invalid, '_');

            return $"{nome}.csv";
        }
    }
}
