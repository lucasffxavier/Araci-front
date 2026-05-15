using System.Collections.ObjectModel;
using System.Linq;

using Araci.Models.Tipos;

namespace Araci.Services
{
    public class TypeLibraryService
    {
        // ====================================================
        // CABOS
        // ====================================================

        public ObservableCollection<TipoCabo>
            TiposCabos
        { get; }
            = new();

        // ====================================================
        // CARGAS
        // ====================================================

        public ObservableCollection<TipoCarga>
            TiposCargas
        { get; }
            = new();

        // ====================================================
        // GERADORES
        // ====================================================

        public ObservableCollection<TipoGerador>
            TiposGeradores
        { get; }
            = new();

        // ====================================================
        // CONSTRUTOR
        // ====================================================

        public TypeLibraryService()
        {
            InicializarCabos();
            InicializarCargas();
            InicializarGeradores();
        }

        // ====================================================
        // PADRÕES
        // ====================================================

        public TipoCabo? TipoCaboPadrao =>
            TiposCabos.FirstOrDefault();

        public TipoCarga? TipoCargaPadrao =>
            TiposCargas.FirstOrDefault();

        public TipoGerador? TipoGeradorPadrao =>
            TiposGeradores.FirstOrDefault();

        // ====================================================
        // CABOS
        // ====================================================

        private void InicializarCabos()
        {
            TiposCabos.Add(
                new TipoCabo
                {
                    NomeTipo = "LC-500MCM",
                    Ampacidade = 520,
                    Resistencia = 0.12,
                    Reatancia = 0.09,
                    Capacitancia = 0.001,
                    Fases = 3,
                    Neutro = true
                });

            TiposCabos.Add(
                new TipoCabo
                {
                    NomeTipo = "LC-750MCM",
                    Ampacidade = 680,
                    Resistencia = 0.08,
                    Reatancia = 0.07,
                    Capacitancia = 0.002,
                    Fases = 3,
                    Neutro = true
                });
        }

        // ====================================================
        // CARGAS
        // ====================================================

        private void InicializarCargas()
        {
            TiposCargas.Add(
                new TipoCarga
                {
                    NomeTipo = "Carga MT",
                    TensaoKV = 34.5,
                    Conexao = "Wye",
                    ModeloCarga = "Potência Constante",
                    Fases = 3,
                    FatorPotencia = 0.96
                });
        }

        // ====================================================
        // GERADORES
        // ====================================================

        private void InicializarGeradores()
        {
            TiposGeradores.Add(
                new TipoGerador
                {
                    NomeTipo = "Gerador Eólico",
                    Fabricante = "WEG",
                    Modelo = "WTG-5MW",
                    PotenciaNominalKW = 5000,
                    TensaoKV = 34.5,
                    Fases = 3
                });
        }
    }
}
