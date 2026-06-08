using System;
using System.Collections.ObjectModel;

namespace Araci.ViewModels
{
    public sealed class ProjectBrowserViewModel
    {
        public ProjectBrowserViewModel()
        {
            Secoes = new ObservableCollection<ProjectBrowserSectionViewModel>
            {
                new("Vistas", new Uri("pack://application:,,,/Resources/Icons/vistas.svg"), "Vista principal"),
                new("Tabelas", new Uri("pack://application:,,,/Resources/Icons/tabela.svg"), "Nenhuma tabela"),
                new("Pranchas", new Uri("pack://application:,,,/Resources/Icons/prancha.svg"), "Nenhuma prancha")
            };
        }

        public ObservableCollection<ProjectBrowserSectionViewModel> Secoes { get; }
    }

    public sealed class ProjectBrowserSectionViewModel
    {
        public ProjectBrowserSectionViewModel(string titulo, Uri icone, string itemVisual)
        {
            Titulo = titulo;
            Icone = icone;
            Itens = new ObservableCollection<string> { itemVisual };
        }

        public string Titulo { get; }
        public Uri Icone { get; }
        public ObservableCollection<string> Itens { get; }
    }
}
