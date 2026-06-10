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
            TiposPrancha = new ObservableCollection<ProjectSheetType>();

            GarantirTipoPranchaPadrao();
            CriarVistaPadrao();
        }

        public ObservableCollection<Elemento> Elementos { get; }
        public ObservableCollection<ProjectView> Vistas { get; }
        public ObservableCollection<ProjectTable> Tabelas { get; }
        public ObservableCollection<ProjectSheet> Pranchas { get; }
        public ObservableCollection<ProjectSheetType> TiposPrancha { get; }
        public Guid? VistaAtivaId { get; private set; }
        public ProjectView? VistaAtiva => Vistas.FirstOrDefault(v => v.Id == VistaAtivaId);
        public ProjectSheetType TipoPranchaPadrao => GarantirTipoPranchaPadrao();
        public event System.Action? VistaAtivaAlterada;
        public event System.Action? ItemProjetoRenomeado;
        public event System.Action<ProjectView>? PropriedadesVistaAlteradas;
        public event System.Action<ProjectTable>? PropriedadesTabelaAlteradas;
        public event System.Action<ProjectSheet>? PropriedadesPranchaAlteradas;
        public event System.Action<ProjectSheetType>? PropriedadesTipoPranchaAlteradas;

        public IEnumerable<Elemento> ObterElementosDaVistaAtiva()
        {
            return VistaAtivaId.HasValue && Vistas.Any(v => v.Id == VistaAtivaId.Value)
                ? ObterElementosDaVista(VistaAtivaId.Value)
                : Enumerable.Empty<Elemento>();
        }

        public IEnumerable<Elemento> ObterElementosDaVista(Guid vistaId)
        {
            return Elementos.Where(e => e.ViewId == vistaId);
        }

        public IEnumerable<Elemento> ObterElementosDaVistaDoElementoOuAtiva(Elemento? elemento)
        {
            return elemento?.ViewId.HasValue == true && Vistas.Any(v => v.Id == elemento.ViewId.Value)
                ? ObterElementosDaVista(elemento.ViewId.Value)
                : ObterElementosDaVistaAtiva();
        }

        public bool PertenceAVistaAtiva(Elemento? elemento)
        {
            return elemento != null &&
                VistaAtivaId.HasValue &&
                Vistas.Any(v => v.Id == VistaAtivaId.Value) &&
                elemento.ViewId == VistaAtivaId.Value;
        }

        public ProjectView CriarNovaVista()
        {
            ProjectView vista = CriarModeloNovaVista();
            AdicionarVista(vista);
            return vista;
        }

        public ProjectTable CriarNovaTabela()
        {
            ProjectTable tabela = CriarModeloNovaTabela();
            AdicionarTabela(tabela);
            return tabela;
        }

        public ProjectSheet CriarNovaPrancha()
        {
            ProjectSheet prancha = CriarModeloNovaPrancha();
            AdicionarPrancha(prancha);
            return prancha;
        }

        public ProjectSheetType CriarNovoTipoPrancha()
        {
            ProjectSheetType tipo = CriarModeloNovoTipoPrancha();
            AdicionarTipoPrancha(tipo);
            return tipo;
        }

        public ProjectView CriarModeloNovaVista()
        {
            return new ProjectView { Nome = CriarNomeUnico("Vista", Vistas.Select(v => v.Nome)) };
        }

        public ProjectTable CriarModeloNovaTabela()
        {
            return new ProjectTable { Nome = CriarNomeUnico("Tabela", Tabelas.Select(t => t.Nome)) };
        }

        public ProjectSheet CriarModeloNovaPrancha()
        {
            int indice = CriarIndiceUnico("Prancha", Pranchas.Select(p => p.Nome));
            ProjectSheetType tipoPadrao = GarantirTipoPranchaPadrao();

            return new ProjectSheet
            {
                Nome = $"Prancha {indice}",
                Numero = $"A{indice:000}",
                SheetTypeId = tipoPadrao.Id
            };
        }

        public ProjectSheetType CriarModeloNovoTipoPrancha()
        {
            return new ProjectSheetType
            {
                Nome = CriarNomeUnicoIgnoreCase("Tipo de prancha", TiposPrancha.Select(t => t.Nome))
            };
        }

        public ProjectView CriarDuplicataVista(ProjectView origem)
        {
            return new ProjectView
            {
                Nome = CriarNomeUnico(origem?.Nome ?? "Vista", Vistas.Select(v => v.Nome)),
                Escala = origem?.Escala ?? "1:100",
                Disciplina = origem?.Disciplina ?? ProjectViewDiscipline.Eletrica,
                RecortarVista = origem?.RecortarVista ?? false,
                RegiaoRecorteVisivel = origem?.RegiaoRecorteVisivel ?? true,
                CameraX = origem?.CameraX ?? 0,
                CameraY = origem?.CameraY ?? 0,
                Zoom = origem?.Zoom ?? 1.0
            };
        }

        public ProjectTable CriarDuplicataTabela(ProjectTable origem)
        {
            return new ProjectTable
            {
                Nome = CriarNomeUnico(origem?.Nome ?? "Tabela", Tabelas.Select(t => t.Nome)),
                Disciplina = origem?.Disciplina ?? ProjectViewDiscipline.Eletrica,
                CategoriasElementos = origem?.CategoriasElementos.ToList() ?? new List<ProjectTableElementCategory>(),
                CamposSelecionados = origem?.CamposSelecionados
                    .Select(c => new ProjectTableFieldSelection
                    {
                        Categoria = c.Categoria,
                        CampoId = c.CampoId,
                        NomeExibicao = c.NomeExibicao,
                        Ordem = c.Ordem
                    })
                    .ToList() ?? new List<ProjectTableFieldSelection>(),
                FiltroVistaId = origem?.FiltroVistaId,
                ModoFiltro = origem?.ModoFiltro ?? ProjectTableFilterLogicalMode.Todas,
                Filtros = origem?.Filtros
                    .Select(f => new ProjectTableFilterRule
                    {
                        Ordem = f.Ordem,
                        Categoria = f.Categoria,
                        CampoId = f.CampoId,
                        NomeExibicao = f.NomeExibicao,
                        Operador = f.Operador,
                        Valor = f.Valor
                    })
                    .ToList() ?? new List<ProjectTableFilterRule>(),
                Ordenacoes = origem?.Ordenacoes
                    .Select(o => new ProjectTableSorting
                    {
                        Ordem = o.Ordem,
                        Categoria = o.Categoria,
                        CampoId = o.CampoId,
                        NomeExibicao = o.NomeExibicao,
                        Direcao = o.Direcao
                    })
                    .ToList() ?? new List<ProjectTableSorting>()
            };
        }

        public ProjectSheet CriarDuplicataPrancha(ProjectSheet origem)
        {
            return new ProjectSheet
            {
                Nome = CriarNomeUnico(origem?.Nome ?? "Prancha", Pranchas.Select(p => p.Nome)),
                Numero = CriarNumeroPranchaUnico(),
                SheetTypeId = ObterTipoPranchaIdValidoOuPadrao(origem?.SheetTypeId),
                FormatoFolha = origem?.FormatoFolha ?? ProjectSheetFormat.A1,
                OrientacaoFolha = origem?.OrientacaoFolha ?? ProjectSheetOrientation.Paisagem,
                LarguraFolha = origem?.LarguraFolha ?? ProjectSheet.DefaultWidth,
                AlturaFolha = origem?.AlturaFolha ?? ProjectSheet.DefaultHeight,
                Tabelas = origem?.Tabelas
                    .Where(i => i != null)
                    .Select(i => i.CriarCopia(gerarNovoId: true))
                    .ToList() ?? new List<ProjectSheetTableInstance>()
            };
        }

        public ProjectSheetType CriarDuplicataTipoPrancha(ProjectSheetType origem)
        {
            return new ProjectSheetType
            {
                Nome = CriarNomeUnicoIgnoreCase(origem?.Nome ?? "Tipo de prancha", TiposPrancha.Select(t => t.Nome)),
                FormatoFolha = origem?.FormatoFolha ?? ProjectSheetFormat.A1,
                OrientacaoFolha = origem?.OrientacaoFolha ?? ProjectSheetOrientation.Paisagem,
                LarguraFolha = origem?.LarguraFolha ?? ProjectSheet.DefaultWidth,
                AlturaFolha = origem?.AlturaFolha ?? ProjectSheet.DefaultHeight,
                Linhas = origem?.Linhas
                    .Where(l => l != null)
                    .Select(l => l.CriarCopia(gerarNovoId: true))
                    .ToList() ?? new List<ProjectSheetTemplateLine>()
            };
        }

        public void AdicionarVista(ProjectView vista)
        {
            if (vista == null || Vistas.Any(v => v.Id == vista.Id))
                return;

            Vistas.Add(vista);
            GarantirVistaAtivaValida();
        }

        public void RemoverVista(ProjectView vista)
        {
            if (vista == null || !Vistas.Contains(vista))
                return;

            if (Vistas.Count <= 1)
                return;

            Vistas.Remove(vista);
            GarantirVistaAtivaValida();
        }

        public void RestaurarVista(ProjectView vista, int indice)
        {
            if (vista == null || Vistas.Any(v => v.Id == vista.Id))
                return;

            int indiceSeguro = indice < 0 || indice > Vistas.Count ? Vistas.Count : indice;
            Vistas.Insert(indiceSeguro, vista);
            GarantirVistaAtivaValida();
        }

        public void AdicionarTabela(ProjectTable tabela)
        {
            if (tabela == null || Tabelas.Any(t => t.Id == tabela.Id))
                return;

            Tabelas.Add(tabela);
        }

        public void RemoverTabela(ProjectTable tabela)
        {
            if (tabela == null || !Tabelas.Contains(tabela))
                return;

            Tabelas.Remove(tabela);
        }

        public void RestaurarTabela(ProjectTable tabela, int indice)
        {
            if (tabela == null || Tabelas.Any(t => t.Id == tabela.Id))
                return;

            int indiceSeguro = indice < 0 || indice > Tabelas.Count ? Tabelas.Count : indice;
            Tabelas.Insert(indiceSeguro, tabela);
        }

        public void AdicionarPrancha(ProjectSheet prancha)
        {
            if (prancha == null || Pranchas.Any(p => p.Id == prancha.Id))
                return;

            GarantirAssociacaoTipoPrancha(prancha);
            Pranchas.Add(prancha);
        }

        public void RemoverPrancha(ProjectSheet prancha)
        {
            if (prancha == null || !Pranchas.Contains(prancha))
                return;

            Pranchas.Remove(prancha);
        }

        public void RestaurarPrancha(ProjectSheet prancha, int indice)
        {
            if (prancha == null || Pranchas.Any(p => p.Id == prancha.Id))
                return;

            int indiceSeguro = indice < 0 || indice > Pranchas.Count ? Pranchas.Count : indice;
            GarantirAssociacaoTipoPrancha(prancha);
            Pranchas.Insert(indiceSeguro, prancha);
        }

        public void AdicionarTipoPrancha(ProjectSheetType tipo)
        {
            if (tipo == null || TiposPrancha.Any(t => t.Id == tipo.Id))
                return;

            if (tipo.Id == Guid.Empty)
                tipo.Id = Guid.NewGuid();

            TiposPrancha.Add(tipo);
        }

        public void RemoverTipoPrancha(ProjectSheetType tipo)
        {
            if (tipo == null || !TiposPrancha.Contains(tipo))
                return;

            if (TiposPrancha.Count <= 1 || TipoPranchaEstaEmUso(tipo.Id))
                return;

            TiposPrancha.Remove(tipo);
        }

        public void RestaurarTipoPrancha(ProjectSheetType tipo, int indice)
        {
            if (tipo == null || TiposPrancha.Any(t => t.Id == tipo.Id))
                return;

            if (tipo.Id == Guid.Empty)
                tipo.Id = Guid.NewGuid();

            int indiceSeguro = indice < 0 || indice > TiposPrancha.Count ? TiposPrancha.Count : indice;
            TiposPrancha.Insert(indiceSeguro, tipo);
        }

        public void RenomearVista(ProjectView vista, string nome)
        {
            if (vista == null || !Vistas.Contains(vista))
                return;

            vista.Nome = nome;
            ItemProjetoRenomeado?.Invoke();
        }

        public void AtualizarPropriedadesVista(ProjectView vista)
        {
            if (vista == null || !Vistas.Contains(vista))
                return;

            PropriedadesVistaAlteradas?.Invoke(vista);
        }

        public void RenomearTabela(ProjectTable tabela, string nome)
        {
            if (tabela == null || !Tabelas.Contains(tabela))
                return;

            tabela.Nome = nome;
            ItemProjetoRenomeado?.Invoke();
        }

        public void AtualizarPropriedadesTabela(ProjectTable tabela)
        {
            if (tabela == null || !Tabelas.Contains(tabela))
                return;

            PropriedadesTabelaAlteradas?.Invoke(tabela);
        }

        public void RenomearPrancha(ProjectSheet prancha, string nome)
        {
            if (prancha == null || !Pranchas.Contains(prancha))
                return;

            prancha.Nome = nome;
            ItemProjetoRenomeado?.Invoke();
        }

        public void AtualizarPropriedadesPrancha(ProjectSheet prancha)
        {
            if (prancha == null || !Pranchas.Contains(prancha))
                return;

            PropriedadesPranchaAlteradas?.Invoke(prancha);
            ItemProjetoRenomeado?.Invoke();
        }

        public void RenomearTipoPrancha(ProjectSheetType tipo, string nome)
        {
            if (tipo == null || !TiposPrancha.Contains(tipo))
                return;

            tipo.Nome = nome;
            ItemProjetoRenomeado?.Invoke();
            PropriedadesTipoPranchaAlteradas?.Invoke(tipo);
        }

        public void AtualizarPropriedadesTipoPrancha(ProjectSheetType tipo)
        {
            if (tipo == null || !TiposPrancha.Contains(tipo))
                return;

            PropriedadesTipoPranchaAlteradas?.Invoke(tipo);
            ItemProjetoRenomeado?.Invoke();
        }

        public bool TipoPranchaEstaEmUso(Guid tipoId)
        {
            return tipoId != Guid.Empty && Pranchas.Any(p => p.SheetTypeId == tipoId);
        }

        public void SubstituirVistas(IEnumerable<ProjectView> vistas)
        {
            Vistas.Clear();

            foreach (ProjectView vista in vistas.Where(v => v != null && !string.IsNullOrWhiteSpace(v.Nome)))
                Vistas.Add(vista);

            if (Vistas.Count == 0)
                CriarVistaPadrao();

            GarantirVistaAtivaValida();
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
            GarantirTipoPranchaPadrao();

            foreach (ProjectSheet prancha in pranchas.Where(p => p != null && !string.IsNullOrWhiteSpace(p.Nome)))
            {
                prancha.Tabelas ??= new List<ProjectSheetTableInstance>();
                GarantirAssociacaoTipoPrancha(prancha);
                Pranchas.Add(prancha);
            }
        }

        public void SubstituirTiposPrancha(IEnumerable<ProjectSheetType> tipos)
        {
            TiposPrancha.Clear();

            foreach (ProjectSheetType tipo in (tipos ?? Enumerable.Empty<ProjectSheetType>()).Where(t => t != null && !string.IsNullOrWhiteSpace(t.Nome)))
            {
                if (tipo.Id == Guid.Empty)
                    tipo.Id = Guid.NewGuid();

                if (!TiposPrancha.Any(t => t.Id == tipo.Id))
                    TiposPrancha.Add(tipo);
            }

            GarantirTipoPranchaPadrao();

            foreach (ProjectSheet prancha in Pranchas)
                GarantirAssociacaoTipoPrancha(prancha);
        }

        public void AdicionarElemento(Elemento elemento)
        {
            AdicionarElemento(elemento, preservarVistaExistente: false);
        }

        public void AdicionarElementoPreservandoVista(Elemento elemento)
        {
            AdicionarElemento(elemento, preservarVistaExistente: true);
        }

        private void AdicionarElemento(Elemento elemento, bool preservarVistaExistente)
        {
            if (Elementos.Contains(elemento))
                return;

            PrepararVistaElemento(elemento, preservarVistaExistente);
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
            TiposPrancha.Clear();
            GarantirTipoPranchaPadrao();
            CriarVistaPadrao();
        }

        private ProjectSheetType GarantirTipoPranchaPadrao()
        {
            ProjectSheetType? existente = TiposPrancha.FirstOrDefault();

            if (existente != null)
                return existente;

            ProjectSheetType tipo = ProjectSheetType.CriarPadrao();
            TiposPrancha.Add(tipo);
            return tipo;
        }

        private void GarantirAssociacaoTipoPrancha(ProjectSheet prancha)
        {
            prancha.SheetTypeId = ObterTipoPranchaIdValidoOuPadrao(prancha.SheetTypeId);
        }

        private Guid ObterTipoPranchaIdValidoOuPadrao(Guid? tipoId)
        {
            if (tipoId.HasValue && tipoId.Value != Guid.Empty && TiposPrancha.Any(t => t.Id == tipoId.Value))
                return tipoId.Value;

            return GarantirTipoPranchaPadrao().Id;
        }

        private void CriarVistaPadrao()
        {
            var vista = new ProjectView { Nome = "Vista principal" };
            Vistas.Add(vista);
            AtualizarVistaAtiva(vista.Id);
        }

        public void DefinirVistaAtiva(Guid vistaId)
        {
            if (Vistas.Any(v => v.Id == vistaId))
                AtualizarVistaAtiva(vistaId);

            GarantirVistaAtivaValida();
        }

        public void DefinirVistaAtiva(Guid? vistaId)
        {
            if (vistaId.HasValue)
                DefinirVistaAtiva(vistaId.Value);
            else
                GarantirVistaAtivaValida();
        }

        private void GarantirVistaAtivaValida()
        {
            if (VistaAtivaId.HasValue && Vistas.Any(v => v.Id == VistaAtivaId.Value))
                return;

            if (Vistas.Count == 0)
                CriarVistaPadrao();
            else
                AtualizarVistaAtiva(Vistas[0].Id);
        }

        private void AtualizarVistaAtiva(Guid? vistaId)
        {
            if (VistaAtivaId == vistaId)
                return;

            VistaAtivaId = vistaId;
            VistaAtivaAlterada?.Invoke();
        }

        private void PrepararVistaElemento(Elemento elemento, bool preservarVistaExistente)
        {
            GarantirVistaAtivaValida();

            if (preservarVistaExistente && elemento.ViewId.HasValue && Vistas.Any(v => v.Id == elemento.ViewId.Value))
                return;

            elemento.ViewId = VistaAtivaId;
        }

        private static string CriarNomeUnico(string prefixo, IEnumerable<string> nomesExistentes)
        {
            return $"{prefixo} {CriarIndiceUnico(prefixo, nomesExistentes)}";
        }

        private static string CriarNomeUnicoIgnoreCase(string prefixo, IEnumerable<string> nomesExistentes)
        {
            var existentes = nomesExistentes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!existentes.Contains(prefixo))
                return prefixo;

            int indice = 2;

            while (existentes.Contains($"{prefixo} {indice}"))
                indice++;

            return $"{prefixo} {indice}";
        }

        private static int CriarIndiceUnico(string prefixo, IEnumerable<string> nomesExistentes)
        {
            int indice = 1;

            while (nomesExistentes.Contains($"{prefixo} {indice}"))
                indice++;

            return indice;
        }

        private string CriarNumeroPranchaUnico()
        {
            int indice = 1;

            while (Pranchas.Any(p => p.Numero == $"A{indice:000}"))
                indice++;

            return $"A{indice:000}";
        }
    }
}
