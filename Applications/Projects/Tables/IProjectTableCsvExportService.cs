namespace Araci.Applications.Projects.Tables
{
    public interface IProjectTableCsvExportService
    {
        string GenerateCsv(ProjectTableDataResult result);
    }
}
