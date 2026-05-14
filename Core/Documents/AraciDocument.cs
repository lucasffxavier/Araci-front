using System.Collections.ObjectModel;

using Araci.ViewModels;

namespace Araci.Core.Documents
{

    public class AraciDocument
    {
        // =========================
        // ELEMENTOS DO DOCUMENTO
        // =========================

        public ObservableCollection<ElementoViewModel>
            Elementos
        { get; }

        // =========================
        // CONSTRUTOR
        // =========================

        public AraciDocument()
        {
            Elementos =
                new ObservableCollection<
                    ElementoViewModel>();
        }

        // =========================
        // ELEMENTOS
        // =========================

        public void AdicionarElemento(
            ElementoViewModel elemento)
        {
            if (!Elementos.Contains(elemento))
            {
                Elementos.Add(elemento);
            }
        }

        public void RemoverElemento(
            ElementoViewModel elemento)
        {
            Elementos.Remove(elemento);
        }

        public void Limpar()
        {
            Elementos.Clear();
        }
    }
}