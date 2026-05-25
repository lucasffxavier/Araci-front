using System;
using System.Collections.ObjectModel;
using System.Linq;
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

            string nomeAnterior = elemento.Nome;

            if (string.IsNullOrWhiteSpace(elemento.Nome) || NomeEmUso(elemento.Nome, elemento))
                elemento.Nome = GerarNomeUnico(elemento);

            SincronizarBarraDoNovoElemento(elemento, nomeAnterior);

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

        public string GerarNomeUnico(Elemento elemento)
        {
            string prefixo = ObterPrefixo(elemento);

            for (int i = 1; i < int.MaxValue; i++)
            {
                string nome = $"{prefixo}-{i.ToString("D3")}";

                if (!NomeEmUso(nome, elemento))
                    return nome;
            }

            return $"{prefixo}-{Guid.NewGuid():N}";
        }

        private bool NomeEmUso(string nome, Elemento elementoAtual)
        {
            return Elementos.Any(e =>
                !ReferenceEquals(e, elementoAtual) &&
                string.Equals(e.Nome, nome, StringComparison.OrdinalIgnoreCase));
        }

        private static void SincronizarBarraDoNovoElemento(Elemento elemento, string nomeAnterior)
        {
            if (elemento is not ElementoEquipamento equipamento)
                return;

            if (string.IsNullOrWhiteSpace(equipamento.Barra) ||
                string.Equals(equipamento.Barra, nomeAnterior, StringComparison.OrdinalIgnoreCase))
            {
                equipamento.Barra = elemento.Nome;
            }
        }

        private static string ObterPrefixo(Elemento elemento)
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
