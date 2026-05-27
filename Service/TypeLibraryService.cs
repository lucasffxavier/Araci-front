using System.Collections.ObjectModel;
using System.Linq;
using Araci.Models.Tipos;

namespace Araci.Services
{
    public class TypeLibraryService
    {
        public TypeLibraryService()
        {
            InicializarCabos();
            InicializarCargas();
            InicializarGeradores();
            InicializarSin();
            InicializarBarras();
        }

        public ObservableCollection<TipoCabo> TiposCabos { get; } = new();

        public ObservableCollection<TipoCarga> TiposCargas { get; } = new();

        public ObservableCollection<TipoGerador> TiposGeradores { get; } = new();

        public ObservableCollection<TipoSin> TiposSin { get; } = new();

        public ObservableCollection<TipoBarra> TiposBarras { get; } = new();

        public TipoCabo? TipoCaboPadrao => TiposCabos.FirstOrDefault();

        public TipoCarga? TipoCargaPadrao => TiposCargas.FirstOrDefault();

        public TipoGerador? TipoGeradorPadrao => TiposGeradores.FirstOrDefault();

        public TipoSin? TipoSinPadrao => TiposSin.FirstOrDefault();

        public TipoBarra? TipoBarraPadrao => TiposBarras.FirstOrDefault();

        private void InicializarCabos()
        {
            TiposCabos.Add(
                new TipoCabo
                {
                    NomeTipo = "LC-500MCM",
                    Familia = "Cabos",
                    Categoria = "Cabos",
                    Fases = 3,
                    R1 = 0.1,
                    X1 = 0.2,
                    R0 = 0.3,
                    X0 = 0.6,
                    C1 = 3.4,
                    C0 = 1.6,
                    Secao = 253
                });
        }

        private void InicializarCargas()
        {
            TiposCargas.Add(
                new TipoCarga
                {
                    NomeTipo = "Carga MT",
                    Familia = "Cargas",
                    Categoria = "Cargas",
                    ModeloCarga = 1,
                    Conexao = "Wye",
                    Tensao = "12.47",
                    Fases = 3,
                    FatorPotencia = 0.96
                });
        }

        private void InicializarGeradores()
        {
            TiposGeradores.Add(
                new TipoGerador
                {
                    NomeTipo = "Gerador Eolico",
                    Familia = "Geradores",
                    Categoria = "Geradores",
                    Fases = 3,
                    TensaoKV = 12.47,
                    ModeloFonte = 1,
                    FatorPotencia = 0.98
                });
        }

        private void InicializarSin()
        {
            TiposSin.Add(
                new TipoSin
                {
                    NomeTipo = "Rede Externa",
                    Familia = "Fontes",
                    Categoria = "SIN",
                    Fases = 3,
                    TensaoKV = 12.47,
                    PotenciaCurtoMVA = 500,
                    RelacaoXR = 10
                });
        }

        private void InicializarBarras()
        {
            TiposBarras.Add(
                new TipoBarra
                {
                    NomeTipo = "Barra Vertical",
                    Familia = "Barras",
                    Categoria = "Barras",
                    ClasseTensao = "12.47",
                    Fases = 3,
                    AlturaPadrao = 120,
                    NumeroConexoes = 6
                });
        }
    }
}
