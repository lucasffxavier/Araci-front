using System.Collections.ObjectModel;
using Araci.Models;

namespace Araci.Core.Documents
{
    public class AraciDocument
    {
        public AraciDocument()
        {
            Elementos = new ObservableCollection<Elemento>();
            Vistas = new ObservableCollection<ProjectView>();
            Tabelas = new ObservableCollection<ProjectTable>();
            Pranchas = new ObservableCollection<ProjectSheet>();

            CriarVistaPadrao();
        }

        public ObservableCollection<Elemento> Elementos { get; }
        public ObservableCollection<ProjectView> Vistas { get; }
        public ObservableCollection<ProjectTable> Tabelas { get; }
        public ObservableCollection<ProjectSheet> Pranchas { get; }

        public void AdicionarElemento(Elemento elemento)
        {
            if (Elementos.Contains(elemento))
                return;

            Elementos.Add(elemento);
        }

        public void RemoverElemento(Elemento elemento)
        {
            if (!Elementos.Contains(elemento))
                return;

            Elementos.Remove(elemento);
        }

        public void Limpar()
        {
            Elementos.Clear();
            Vistas.Clear();
            Tabelas.Clear();
            Pranchas.Clear();
            CriarVistaPadrao();
        }

        private void CriarVistaPadrao()
        {
            Vistas.Add(new ProjectView { Nome = "Vista principal" });
        }
    }
}
