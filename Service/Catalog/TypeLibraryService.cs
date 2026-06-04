using System.Collections.ObjectModel;
using System.Linq;
using Araci.Models.Tipos;

namespace Araci.Services.Catalog
{
    public class TypeLibraryService
    {
        public TypeLibraryService()
        {
            InicializarCabos();
            InicializarCargas();
            InicializarGeradores();
            InicializarSin();
            InicializarTransformadores();
            InicializarBarras();
            InicializarLinhasAnotativas();
            InicializarTextosAnotativos();
        }

        public ObservableCollection<TipoCabo> TiposCabos { get; } = new();
        public ObservableCollection<TipoCarga> TiposCargas { get; } = new();
        public ObservableCollection<TipoGerador> TiposGeradores { get; } = new();
        public ObservableCollection<TipoSin> TiposSin { get; } = new();
        public ObservableCollection<TipoTransformador> TiposTransformadores { get; } = new();
        public ObservableCollection<TipoBarra> TiposBarras { get; } = new();
        public ObservableCollection<TipoLinhaAnotativa> TiposLinhasAnotativas { get; } = new();
        public ObservableCollection<TipoTextoAnotativo> TiposTextosAnotativos { get; } = new();

        public TipoCabo? TipoCaboPadrao => TiposCabos.FirstOrDefault();
        public TipoCarga? TipoCargaPadrao => TiposCargas.FirstOrDefault();
        public TipoGerador? TipoGeradorPadrao => TiposGeradores.FirstOrDefault();
        public TipoSin? TipoSinPadrao => TiposSin.FirstOrDefault();
        public TipoTransformador? TipoTransformadorPadrao => TiposTransformadores.FirstOrDefault();
        public TipoBarra? TipoBarraPadrao => TiposBarras.FirstOrDefault();
        public TipoLinhaAnotativa? TipoLinhaAnotativaPadrao => TiposLinhasAnotativas.FirstOrDefault();
        public TipoTextoAnotativo? TipoTextoAnotativoPadrao => TiposTextosAnotativos.FirstOrDefault();

        private void InicializarCabos()
        {
            TiposCabos.Add(new TipoCabo { NomeTipo = "LC-500MCM", Familia = "Cabos", Categoria = "Cabos", Fases = 3, R1 = 0.1, X1 = 0.2, R0 = 0.3, X0 = 0.6, C1 = 3.4, C0 = 1.6, Secao = 253 });
        }

        private void InicializarCargas()
        {
            TiposCargas.Add(new TipoCarga { NomeTipo = "Carga MT", Familia = "Cargas", Categoria = "Cargas", ModeloCarga = 1, Conexao = "Wye", Tensao = "12.47", Fases = 3, FatorPotencia = 0.96 });
        }

        private void InicializarGeradores()
        {
            TiposGeradores.Add(new TipoGerador { NomeTipo = "Gerador Eolico", Familia = "Geradores", Categoria = "Geradores", Fases = 3, TensaoKV = 12.47, ModeloFonte = 1, FatorPotencia = 0.98 });
        }

        private void InicializarSin()
        {
            TiposSin.Add(new TipoSin { NomeTipo = "Rede Externa", Familia = "Fontes", Categoria = "SIN", Fases = 3, PotenciaCurtoMVA = 500, RelacaoXR = 10 });
        }

        private void InicializarTransformadores()
        {
            TiposTransformadores.Add(new TipoTransformador { NomeTipo = "Transformador 2 Enrolamentos", Familia = "Transformadores", Categoria = "Transformadores", Fases = 3, Enrolamentos = 2, RPercentual = 1, XPercentual = 5, LigacaoPrimario = "Wye", LigacaoSecundario = "Wye" });
        }

        private void InicializarBarras()
        {
            TiposBarras.Add(new TipoBarra { NomeTipo = "Barra Vertical", Familia = "Barras", Categoria = "Barras", ClasseTensao = "12.47", Fases = 3, AlturaPadrao = 120, NumeroConexoes = 6 });
        }

        private void InicializarLinhasAnotativas()
        {
            TiposLinhasAnotativas.Add(CriarTipoLinha("Linha contínua", "Contínuo"));
            TiposLinhasAnotativas.Add(CriarTipoLinha("Linha tracejada", "Tracejado"));
            TiposLinhasAnotativas.Add(CriarTipoLinha("Linha traço ponto", "Traço ponto"));
            TiposLinhasAnotativas.Add(CriarTipoLinha("Linha traço dois pontos", "Traço dois pontos"));
        }

        private void InicializarTextosAnotativos()
        {
            TiposTextosAnotativos.Add(CriarTipoTexto("Texto padrão", "#FF000000", "Arial", 14, "Esquerda"));
            TiposTextosAnotativos.Add(CriarTipoTexto("Texto pequeno", "#FF000000", "Arial", 10, "Esquerda"));
            TiposTextosAnotativos.Add(CriarTipoTexto("Texto título", "#FF000000", "Arial", 20, "Centro"));
        }

        private static TipoLinhaAnotativa CriarTipoLinha(string nome, string estilo)
        {
            return new TipoLinhaAnotativa { NomeTipo = nome, Familia = "Anotações", Categoria = "Linhas", EstiloLinha = estilo };
        }

        private static TipoTextoAnotativo CriarTipoTexto(string nome, string cor, string fonte, double altura, string alinhamento)
        {
            return new TipoTextoAnotativo { NomeTipo = nome, Familia = "Anotações", Categoria = "Textos", CorTexto = cor, Fonte = fonte, AlturaTexto = altura, AlinhamentoHorizontal = alinhamento };
        }
    }
}