using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectBrowserViewModel
    {
        private readonly AraciDocument _document;

        public ProjectBrowserViewModel(AraciDocument document)
        {
            _document = document;
            Secoes = new ObservableCollection<ProjectBrowserSectionViewModel>
            {
                new("Vistas"),
                new("Tabelas"),
                new("Pranchas")
            };

            AtualizarSecoes();

            _document.Vistas.CollectionChanged += OnDocumentCollectionChanged;
            _document.Tabelas.CollectionChanged += OnDocumentCollectionChanged;
            _document.Pranchas.CollectionChanged += OnDocumentCollectionChanged;
        }

        public ObservableCollection<ProjectBrowserSectionViewModel> Secoes { get; }

        private void OnDocumentCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            AtualizarSecoes();
        }

        private void AtualizarSecoes()
        {
            AtualizarItens(Secoes[0], _document.Vistas.Select(v => v.Nome));
            AtualizarItens(Secoes[1], _document.Tabelas.Select(t => t.Nome).DefaultIfEmpty("Nenhuma tabela"));
            AtualizarItens(Secoes[2], _document.Pranchas.Select(p => FormatarPrancha(p)).DefaultIfEmpty("Nenhuma prancha"));
        }

        private static void AtualizarItens(ProjectBrowserSectionViewModel secao, IEnumerable<string> itens)
        {
            secao.Itens.Clear();

            foreach (string item in itens)
                secao.Itens.Add(item);
        }

        private static string FormatarPrancha(ProjectSheet prancha)
        {
            return string.IsNullOrWhiteSpace(prancha.Numero)
                ? prancha.Nome
                : $"{prancha.Numero} - {prancha.Nome}";
        }
    }

    public sealed class ProjectBrowserSectionViewModel
    {
        public ProjectBrowserSectionViewModel(string titulo)
        {
            Titulo = titulo;
            Itens = new ObservableCollection<string>();
        }

        public string Titulo { get; }
        public ObservableCollection<string> Itens { get; }
    }
}
