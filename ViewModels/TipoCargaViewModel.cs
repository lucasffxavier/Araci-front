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

        public TipoCargaViewModel(
            TipoCarga tipo)
            : base(tipo)
        {
        }

        // =========================
        // PROPRIEDADES
        // =========================

        public string ModeloCarga
        {
            get => TipoCarga.ModeloCarga;

            set
            {
                if (TipoCarga.ModeloCarga == value)
                    return;

                TipoCarga.ModeloCarga = value;

                OnPropertyChanged();
            }
        }

        public string Conexao
        {
            get => TipoCarga.Conexao;

            set
            {
                if (TipoCarga.Conexao == value)
                    return;

                TipoCarga.Conexao = value;

                OnPropertyChanged();
            }
        }

        public double TensaoKV
        {
            get => TipoCarga.TensaoKV;

            set
            {
                if (TipoCarga.TensaoKV == value)
                    return;

                TipoCarga.TensaoKV = value;

                OnPropertyChanged();
            }
        }

        public int Fases
        {
            get => TipoCarga.Fases;

            set
            {
                if (TipoCarga.Fases == value)
                    return;

                TipoCarga.Fases = value;

                OnPropertyChanged();
            }
        }

        public double FatorPotencia
        {
            get => TipoCarga.FatorPotencia;

            set
            {
                if (TipoCarga.FatorPotencia == value)
                    return;

                TipoCarga.FatorPotencia = value;

                OnPropertyChanged();
            }
        }
    }
}
