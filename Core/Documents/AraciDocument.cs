using System.Collections.ObjectModel;
using Araci.Models;

namespace Araci.Core.Documents
{
    public class AraciDocument
    {
        public AraciDocument()
        {
            Elementos = new ObservableCollection<Elemento>();
        }

        public ObservableCollection<Elemento> Elementos { get; }

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
        }
    }
}
