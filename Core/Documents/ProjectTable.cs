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

    public enum ProjectTableFilterLogicalMode
    {
        Todas,
        Qualquer
    }

    public enum ProjectTableFilterOperator
    {
        Contem,
        NaoContem,
        ComecaCom,
        TerminaCom,
        IgualA,
        DiferenteDe
    }

    public class ProjectTableFilterRule
    {
        public int Ordem { get; set; }
        public ProjectTableElementCategory Categoria { get; set; }
        public string CampoId { get; set; } = string.Empty;
        public string NomeExibicao { get; set; } = string.Empty;
        public ProjectTableFilterOperator Operador { get; set; } = ProjectTableFilterOperator.Contem;
        public string Valor { get; set; } = string.Empty;
    }

    public class ProjectTable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public ProjectViewDiscipline Disciplina { get; set; } = ProjectViewDiscipline.Eletrica;
        public List<ProjectTableElementCategory> CategoriasElementos { get; set; } = new();
        public List<ProjectTableFieldSelection> CamposSelecionados { get; set; } = new();
        public Guid? FiltroVistaId { get; set; }
        public ProjectTableFilterLogicalMode ModoFiltro { get; set; } = ProjectTableFilterLogicalMode.Todas;
        public List<ProjectTableFilterRule> Filtros { get; set; } = new();
    }
}
