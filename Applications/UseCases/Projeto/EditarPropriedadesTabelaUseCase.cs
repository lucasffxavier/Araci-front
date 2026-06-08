using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public class EditarPropriedadesTabelaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public EditarPropriedadesTabelaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool AlterarDisciplina(Guid id, ProjectViewDiscipline disciplina)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            if (tabela.Disciplina == disciplina)
                return true;

            _commands.Execute(new UpdateProjectTablePropertyCommand<ProjectViewDiscipline>(
                _document,
                tabela,
                (t, valor) => t.Disciplina = valor,
                tabela.Disciplina,
                disciplina));

            return true;
        }

        public bool AlterarCategoriasElementos(Guid id, IReadOnlyList<ProjectTableElementCategory> categorias)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            List<ProjectTableElementCategory> valorNovo = NormalizarCategorias(categorias);
            List<ProjectTableElementCategory> valorAnterior = NormalizarCategorias(tabela.CategoriasElementos);

            if (valorAnterior.SequenceEqual(valorNovo))
                return true;

            _commands.Execute(new UpdateProjectTablePropertyCommand<IReadOnlyList<ProjectTableElementCategory>>(
                _document,
                tabela,
                (t, valor) => t.CategoriasElementos = valor.ToList(),
                valorAnterior,
                valorNovo));

            return true;
        }

        public bool AlterarElementosTabela(
            Guid id,
            IReadOnlyList<ProjectTableElementCategory> categorias,
            IReadOnlyList<ProjectTableFieldSelection> campos)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            List<ProjectTableElementCategory> categoriasNovas = NormalizarCategorias(categorias);
            List<ProjectTableElementCategory> categoriasAnteriores = NormalizarCategorias(tabela.CategoriasElementos);
            List<ProjectTableFieldSelection> camposNovos = NormalizarCampos(campos, categoriasNovas);
            List<ProjectTableFieldSelection> camposAnteriores = NormalizarCampos(tabela.CamposSelecionados, categoriasAnteriores);

            if (categoriasAnteriores.SequenceEqual(categoriasNovas) && CamposIguais(camposAnteriores, camposNovos))
                return true;

            _commands.Execute(new UpdateProjectTableElementsCommand(
                _document,
                tabela,
                categoriasAnteriores,
                categoriasNovas,
                camposAnteriores,
                camposNovos));

            return true;
        }

        private static List<ProjectTableElementCategory> NormalizarCategorias(IEnumerable<ProjectTableElementCategory>? categorias)
        {
            return (categorias ?? Enumerable.Empty<ProjectTableElementCategory>())
                .Distinct()
                .OrderBy(categoria => categoria)
                .ToList();
        }

        private static List<ProjectTableFieldSelection> NormalizarCampos(
            IEnumerable<ProjectTableFieldSelection>? campos,
            IReadOnlyList<ProjectTableElementCategory> categorias)
        {
            HashSet<ProjectTableElementCategory> categoriasPermitidas = categorias.ToHashSet();

            return (campos ?? Enumerable.Empty<ProjectTableFieldSelection>())
                .Where(c => categoriasPermitidas.Contains(c.Categoria))
                .Where(c => !string.IsNullOrWhiteSpace(c.CampoId))
                .OrderBy(c => c.Ordem)
                .GroupBy(c => new { c.Categoria, CampoId = c.CampoId.Trim() })
                .Select((g, index) =>
                {
                    ProjectTableFieldSelection campo = g.First();
                    return new ProjectTableFieldSelection
                    {
                        Categoria = campo.Categoria,
                        CampoId = campo.CampoId.Trim(),
                        NomeExibicao = string.IsNullOrWhiteSpace(campo.NomeExibicao) ? campo.CampoId.Trim() : campo.NomeExibicao.Trim(),
                        Ordem = index
                    };
                })
                .ToList();
        }

        private static bool CamposIguais(IReadOnlyList<ProjectTableFieldSelection> a, IReadOnlyList<ProjectTableFieldSelection> b)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Categoria != b[i].Categoria ||
                    !string.Equals(a[i].CampoId, b[i].CampoId, StringComparison.Ordinal) ||
                    !string.Equals(a[i].NomeExibicao, b[i].NomeExibicao, StringComparison.Ordinal) ||
                    a[i].Ordem != b[i].Ordem)
                    return false;
            }

            return true;
        }
    }
}
