using System.Collections.ObjectModel;

using Araci.Models;

namespace Araci.Core.Documents
{

    public class AraciDocument
    {
        // =========================
        // ELEMENTOS DO DOCUMENTO
        // =========================

        public ObservableCollection<Elemento>
            Elementos
        { get; }

        // =========================
        // CONSTRUTOR
        // =========================

        public AraciDocument()
        {
            Elementos =
                new ObservableCollection<Elemento>();
        }

        // =========================
        // ELEMENTOS
        // =========================

        public void AdicionarElemento(
            Elemento elemento)
        {
            if (!Elementos.Contains(elemento))
            {
                Elementos.Add(elemento);
            }
        }

        public void RemoverElemento(
            Elemento elemento)
        {
            Elementos.Remove(elemento);
        }

        public void Limpar()
        {
            Elementos.Clear();
        }
    }
}
