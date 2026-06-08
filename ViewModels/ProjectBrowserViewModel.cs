using System.Collections.ObjectModel;

namespace Araci.ViewModels
{
    public sealed class ProjectBrowserViewModel
    {
        public ProjectBrowserViewModel()
        {
            Secoes = new ObservableCollection<ProjectBrowserSectionViewModel>
            {
                new("Vistas", "Vista principal"),
                new("Tabelas", "Nenhuma tabela"),
                new("Pranchas", "Nenhuma prancha")
            };
        }

        public ObservableCollection<ProjectBrowserSectionViewModel> Secoes { get; }
    }

    public sealed class ProjectBrowserSectionViewModel
    {
        public ProjectBrowserSectionViewModel(string titulo, string itemVisual)
        {
            Titulo = titulo;
            Itens = new ObservableCollection<string> { itemVisual };
        }

        public string Titulo { get; }
        public ObservableCollection<string> Itens { get; }
    }
}
