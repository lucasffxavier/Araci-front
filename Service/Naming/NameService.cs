using System;
using System.Linq;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services.Catalog;

namespace Araci.Services.Naming
{
    public class NameService
    {
        private readonly AraciDocument _document;
        private readonly ElementRegistryService? _registry;

        public NameService(AraciDocument document)
            : this(document, null)
        {
        }

        public NameService(
            AraciDocument document,
            ElementRegistryService? registry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _registry = registry;
        }

        public string ObterPrefixo(Elemento elemento)
        {
            return _registry?.GetNamePrefix(elemento) ?? "ELM";
        }

        public string GerarNomeUnico(Elemento elemento)
        {
            return GerarNomeUnico(ObterPrefixo(elemento), elemento);
        }

        public string GerarNomeUnico(string prefixo, Elemento? elementoAtual = null)
        {
            string basePrefixo = NormalizarPrefixo(prefixo);

            for (int i = 1; i < int.MaxValue; i++)
            {
                string nome = $"{basePrefixo}-{i:D3}";

                if (!NomeEmUso(nome, elementoAtual))
                    return nome;
            }

            return $"{basePrefixo}-{Guid.NewGuid():N}";
        }

        public bool NomeEmUso(string nome, Elemento? elementoAtual = null)
        {
            string normalizado = NormalizarNomeVisual(nome);

            if (string.IsNullOrWhiteSpace(normalizado))
                return false;

            return _document.Elementos.Any(e =>
                !ReferenceEquals(e, elementoAtual) &&
                string.Equals(e.Nome, normalizado, StringComparison.OrdinalIgnoreCase));
        }

        public string NormalizarNomeVisual(string nome)
        {
            return string.IsNullOrWhiteSpace(nome)
                ? string.Empty
                : nome.Trim();
        }

        public void GarantirNomeUnico(Elemento elemento)
        {
            ArgumentNullException.ThrowIfNull(elemento);

            string nomeAnterior = elemento.Nome;
            string nome = NormalizarNomeVisual(elemento.Nome);

            if (string.IsNullOrWhiteSpace(nome))
                nome = GerarNomeUnico(elemento);
            else if (NomeEmUso(nome, elemento))
                nome = GerarNomeUnico(ExtrairPrefixo(nome), elemento);

            elemento.Nome = nome;
            SincronizarBarraDoEquipamento(elemento, nomeAnterior);
        }

        public void Renomear(Elemento elemento, string novoNome)
        {
            ArgumentNullException.ThrowIfNull(elemento);

            string nomeAnterior = elemento.Nome;
            string nome = NormalizarNomeVisual(novoNome);

            if (string.IsNullOrWhiteSpace(nome))
                nome = GerarNomeUnico(elemento);
            else if (NomeEmUso(nome, elemento))
                nome = GerarNomeUnico(ExtrairPrefixo(nome), elemento);

            elemento.Nome = nome;
            SincronizarBarraDoEquipamento(elemento, nomeAnterior);
        }

        private static string ExtrairPrefixo(string nome)
        {
            string normalizado = string.IsNullOrWhiteSpace(nome)
                ? "ELM"
                : nome.Trim();

            int separador = normalizado.LastIndexOf('-');

            if (separador > 0 &&
                separador < normalizado.Length - 1 &&
                normalizado[(separador + 1)..].All(char.IsDigit))
            {
                return normalizado[..separador];
            }

            return normalizado;
        }

        private static string NormalizarPrefixo(string prefixo)
        {
            string normalizado = string.IsNullOrWhiteSpace(prefixo)
                ? "ELM"
                : prefixo.Trim();

            return string.IsNullOrWhiteSpace(normalizado) ? "ELM" : normalizado;
        }

        private static void SincronizarBarraDoEquipamento(Elemento elemento, string nomeAnterior)
        {
            if (elemento is not ElementoEquipamento equipamento)
                return;

            if (string.IsNullOrWhiteSpace(equipamento.Barra) ||
                string.Equals(equipamento.Barra, nomeAnterior, StringComparison.OrdinalIgnoreCase))
            {
                equipamento.Barra = elemento.Nome;
            }
        }
    }
}
