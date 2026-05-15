using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoCargaViewModel
        : TipoElementoViewModel
    {
        // =========================
        // TIPO FORTE
        // =========================

        protected TipoCarga TipoCarga =>
            (TipoCarga)_tipo;

        // =========================
        // CONSTRUTOR
        // =========================

        public TipoCargaViewModel(TipoCarga tipo)
            : base(tipo)
        {
        }

        // =========================
        // PROPRIEDADES
        // =========================

        public string ModeloCarga =>
            TipoCarga.ModeloCarga;

        public string Conexao =>
            TipoCarga.Conexao;

        public double TensaoKV =>
            TipoCarga.TensaoKV;

        public int Fases =>
            TipoCarga.Fases;

        public double FatorPotencia =>
            TipoCarga.FatorPotencia;
    }
}