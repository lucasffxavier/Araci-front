using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoGeradorViewModel
        : TipoElementoViewModel
    {
        // =========================
        // TIPO FORTE
        // =========================

        protected TipoGerador TipoGerador =>
            (TipoGerador)_tipo;

        // =========================
        // CONSTRUTOR
        // =========================

        public TipoGeradorViewModel(TipoGerador tipo)
            : base(tipo)
        {
        }

        // =========================
        // PROPRIEDADES
        // =========================

        public string CategoriaGerador =>
            TipoGerador.CategoriaGerador;

        public string Fabricante =>
            TipoGerador.Fabricante;

        public string Modelo =>
            TipoGerador.Modelo;

        public double PotenciaNominalKW =>
            TipoGerador.PotenciaNominalKW;

        public double TensaoKV =>
            TipoGerador.TensaoKV;

        public int Fases =>
            TipoGerador.Fases;
    }
}