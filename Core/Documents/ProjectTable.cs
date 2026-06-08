using System;
using System.Collections.Generic;

namespace Araci.Core.Documents
{
    public enum ProjectTableElementCategory
    {
        Barras,
        Cabos,
        Cargas,
        Geradores,
        Transformadores,
        Sin
    }

    public class ProjectTableFieldSelection
    {
        public ProjectTableElementCategory Categoria { get; set; }
        public string CampoId { get; set; } = string.Empty;
        public string NomeExibicao { get; set; } = string.Empty;
        public int Ordem { get; set; }
    }

    public class ProjectTable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public ProjectViewDiscipline Disciplina { get; set; } = ProjectViewDiscipline.Eletrica;
        public List<ProjectTableElementCategory> CategoriasElementos { get; set; } = new();
        public List<ProjectTableFieldSelection> CamposSelecionados { get; set; } = new();
    }
}
