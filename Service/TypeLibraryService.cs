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
            InicializarBarras();
        }

        public ObservableCollection<TipoCabo> TiposCabos { get; } = new();

        public ObservableCollection<TipoCarga> TiposCargas { get; } = new();

        public ObservableCollection<TipoGerador> TiposGeradores { get; } = new();

        public ObservableCollection<TipoBarra> TiposBarras { get; } = new();

        public TipoCabo? TipoCaboPadrao => TiposCabos.FirstOrDefault();

        public TipoCarga? TipoCargaPadrao => TiposCargas.FirstOrDefault();

        public TipoGerador? TipoGeradorPadrao => TiposGeradores.FirstOrDefault();

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
                    R1 = 0.12,
                    X1 = 0.09,
                    R0 = 0.36,
                    X0 = 0.27,
                    C1 = 0.001,
                    C0 = 0.0007,
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
                    Tensao = "13.8",
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
                    ModeloFonte = 1,
                    FatorPotencia = 0.98
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
                    ClasseTensao = "13.8",
                    Fases = 3,
                    AlturaPadrao = 120,
                    NumeroConexoes = 6
                });
        }
    }
}
