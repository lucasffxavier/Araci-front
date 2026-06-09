using Araci.Core.Documents;

namespace Araci.Applications.Projects.Tables
{
    public sealed class ProjectTableDataCell
    {
        public ProjectTableDataCell(
            ProjectTableElementCategory categoria,
            string campoId,
            string nomeExibicao,
            object? rawValue,
            string displayValue)
        {
            Categoria = categoria;
            CampoId = campoId;
            NomeExibicao = nomeExibicao;
            RawValue = rawValue;
            DisplayValue = displayValue;
        }

        public ProjectTableElementCategory Categoria { get; }
        public string CampoId { get; }
        public string NomeExibicao { get; }
        public object? RawValue { get; }
        public string DisplayValue { get; }
    }
}
