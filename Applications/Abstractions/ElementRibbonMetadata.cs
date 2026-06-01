using System;

namespace Araci.Applications.Abstractions
{
    public sealed class ElementRibbonMetadata
    {
        public ElementRibbonMetadata(
            string nome,
            string? categoria,
            string? icone,
            int ordem,
            bool exibir,
            string? atalho)
        {
            Nome = string.IsNullOrWhiteSpace(nome) ? throw new ArgumentException("Nome do Ribbon deve ser informado.", nameof(nome)) : nome;
            Categoria = string.IsNullOrWhiteSpace(categoria) ? "Inserir" : categoria;
            Icone = NormalizarIcone(icone);
            Ordem = ordem;
            Exibir = exibir;
            Atalho = string.IsNullOrWhiteSpace(atalho) ? string.Empty : atalho.Trim().ToUpperInvariant();
        }

        public string Nome { get; }
        public string Categoria { get; }
        public string Icone { get; }
        public int Ordem { get; }
        public bool Exibir { get; }
        public string Atalho { get; }

        private static string NormalizarIcone(string? icone)
        {
            if (string.IsNullOrWhiteSpace(icone))
                return string.Empty;

            string valor = icone.Trim();

            if (valor.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                return valor;

            return $"pack://application:,,,/Resources/Icons/{valor}";
        }
    }
}
