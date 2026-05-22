using System.Collections.ObjectModel;
using System.Linq;
using Araci.Models.Tipos;

namespace Araci.Services
{
    public class TypeLibraryService
    {
        public ObservableCollection<TipoCabo> TiposCabos { get; } = new();
        public ObservableCollection<TipoCarga> TiposCargas { get; } = new();
        public ObservableCollection<TipoGerador> TiposGeradores { get; } = new();

        // =========================
        // BARRAS
        // =========================

        public ObservableCollection<TipoBarra> TiposBarras { get; } = new();

        public TipoBarra? TipoBarraPadrao =>
            TiposBarras.FirstOrDefault();

        public TypeLibraryService()
        {
            InicializarCabos();
            InicializarCargas();
            InicializarGeradores();
            InicializarBarras();
        }

        private void InicializarBarras()
        {
            TiposBarras.Add(
                new TipoBarra
                {
                    NomeTipo = "Barra Vertical",
                    AlturaPadrao = 120,
                    NumeroConexoes = 6
                });
        }

        public TipoCabo? TipoCaboPadrao => TiposCabos.FirstOrDefault();
        public TipoCarga? TipoCargaPadrao => TiposCargas.FirstOrDefault();
        public TipoGerador? TipoGeradorPadrao => TiposGeradores.FirstOrDefault();

        private void InicializarCabos()
        {
            TiposCabos.Add(new TipoCabo { NomeTipo = "LC-500MCM" });
        }

        private void InicializarCargas()
        {
            TiposCargas.Add(new TipoCarga { NomeTipo = "Carga MT" });
        }

        private void InicializarGeradores()
        {
            TiposGeradores.Add(new TipoGerador { NomeTipo = "Gerador Eólico" });
        }
    }
}