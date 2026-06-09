using Araci.Core.Documents;

namespace Araci.Applications.Projects.Tables
{
    public sealed class ProjectTableDataColumn
    {
        public ProjectTableDataColumn(
            ProjectTableElementCategory categoria,
            string campoId,
            string nomeExibicao,
            int ordem)
        {
            Categoria = categoria;
            CampoId = campoId;
            NomeExibicao = nomeExibicao;
            Ordem = ordem;
        }

        public ProjectTableElementCategory Categoria { get; }
        public string CampoId { get; }
        public string NomeExibicao { get; }
        public int Ordem { get; }
    }
}
