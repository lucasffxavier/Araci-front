using System.Collections.ObjectModel;
using System.Linq;
using Araci.Models;

namespace Araci.Core.Documents
{
    public class AraciDocument
    {
        public ObservableCollection<Elemento> Elementos { get; }

        public AraciDocument()
        {
            Elementos = new ObservableCollection<Elemento>();
        }

        public void AdicionarElemento(Elemento elemento)
        {
            if (Elementos.Contains(elemento))
                return;

            Elementos.Add(elemento);
            AtualizarNomes(elemento.GetType());
        }

        public void RemoverElemento(Elemento elemento)
        {
            if (!Elementos.Contains(elemento))
                return;

            Elementos.Remove(elemento);
            AtualizarNomes(elemento.GetType());
        }

        public void Limpar()
        {
            Elementos.Clear();
        }

        private void AtualizarNomes(System.Type tipo)
        {
            var lista = Elementos
                .Where(e => e.GetType() == tipo)
                .ToList(); // mantém ordem de inserção

            for (int i = 0; i < lista.Count; i++)
            {
                string prefixo = ObterPrefixo(lista[i]);
                lista[i].Nome = $"{prefixo}-{(i + 1).ToString("D3")}";
            }
        }

        private string ObterPrefixo(Elemento elemento)
        {
            return elemento switch
            {
                Cabo => "CABO",
                Carga => "CARGA",
                Gerador => "GERADOR",
                Barra => "BARRA",
                _ => "ELM"
            };
        }
    }
}