using System.Collections.Generic;
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
            Vistas = new ObservableCollection<ProjectView>();
            Tabelas = new ObservableCollection<ProjectTable>();
            Pranchas = new ObservableCollection<ProjectSheet>();

            CriarVistaPadrao();
        }

        public ObservableCollection<Elemento> Elementos { get; }
        public ObservableCollection<ProjectView> Vistas { get; }
        public ObservableCollection<ProjectTable> Tabelas { get; }
        public ObservableCollection<ProjectSheet> Pranchas { get; }

        public ProjectView CriarNovaVista()
        {
            var vista = new ProjectView { Nome = CriarNomeUnico("Vista", Vistas.Select(v => v.Nome)) };
            Vistas.Add(vista);
            return vista;
        }

        public ProjectTable CriarNovaTabela()
        {
            var tabela = new ProjectTable { Nome = CriarNomeUnico("Tabela", Tabelas.Select(t => t.Nome)) };
            Tabelas.Add(tabela);
            return tabela;
        }

        public ProjectSheet CriarNovaPrancha()
        {
            int indice = CriarIndiceUnico("Prancha", Pranchas.Select(p => p.Nome));
            var prancha = new ProjectSheet
            {
                Nome = $"Prancha {indice}",
                Numero = $"A{indice:000}"
            };

            Pranchas.Add(prancha);
            return prancha;
        }

        public void SubstituirVistas(IEnumerable<ProjectView> vistas)
        {
            Vistas.Clear();

            foreach (ProjectView vista in vistas.Where(v => v != null && !string.IsNullOrWhiteSpace(v.Nome)))
                Vistas.Add(vista);

            if (Vistas.Count == 0)
                CriarVistaPadrao();
        }

        public void SubstituirTabelas(IEnumerable<ProjectTable> tabelas)
        {
            Tabelas.Clear();

            foreach (ProjectTable tabela in tabelas.Where(t => t != null && !string.IsNullOrWhiteSpace(t.Nome)))
                Tabelas.Add(tabela);
        }

        public void SubstituirPranchas(IEnumerable<ProjectSheet> pranchas)
        {
            Pranchas.Clear();

            foreach (ProjectSheet prancha in pranchas.Where(p => p != null && !string.IsNullOrWhiteSpace(p.Nome)))
                Pranchas.Add(prancha);
        }

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

        private static string CriarNomeUnico(string prefixo, IEnumerable<string> nomesExistentes)
        {
            return $"{prefixo} {CriarIndiceUnico(prefixo, nomesExistentes)}";
        }

        private static int CriarIndiceUnico(string prefixo, IEnumerable<string> nomesExistentes)
        {
            int indice = 1;

            while (nomesExistentes.Contains($"{prefixo} {indice}"))
                indice++;

            return indice;
        }
    }
}
