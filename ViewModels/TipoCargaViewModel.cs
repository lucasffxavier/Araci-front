using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoCargaViewModel : TipoElementoViewModel
    {
        public TipoCargaViewModel(TipoCarga tipo)
            : base(tipo)
        {
        }

        protected TipoCarga TipoCarga => (TipoCarga)_tipo;

        public int ModeloCarga
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

        public string Tensao
        {
            get => TipoCarga.Tensao;
            set
            {
                if (TipoCarga.Tensao == value)
                    return;

                TipoCarga.Tensao = value;
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
