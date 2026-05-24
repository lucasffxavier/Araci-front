using System;
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

        private void AtualizarNomes(Type tipo)
        {
            var lista = Elementos
                .Where(e => e.GetType() == tipo)
                .ToList();

            for (int i = 0; i < lista.Count; i++)
            {
                Elemento elemento = lista[i];
                string nomeAntigo = elemento.Nome;
                string prefixo = ObterPrefixo(elemento);
                string nomeNovo = $"{prefixo}-{(i + 1).ToString("D3")}";

                elemento.Nome = nomeNovo;
                SincronizarBarraAutomatica(elemento, nomeAntigo, nomeNovo);
            }
        }

        private static void SincronizarBarraAutomatica(Elemento elemento, string nomeAntigo, string nomeNovo)
        {
            if (elemento is not ElementoEquipamento equipamento)
                return;

            if (string.IsNullOrWhiteSpace(equipamento.Barra) ||
                string.Equals(equipamento.Barra, nomeAntigo, StringComparison.OrdinalIgnoreCase))
            {
                equipamento.Barra = nomeNovo;
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
