using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoCaboViewModel : TipoElementoViewModel
    {
        public TipoCaboViewModel(TipoCabo tipo)
            : base(tipo)
        {
        }

        protected TipoCabo TipoCabo => (TipoCabo)_tipo;

        public int Fases
        {
            get => TipoCabo.Fases;
            set
            {
                if (TipoCabo.Fases == value)
                    return;

                TipoCabo.Fases = value;
                OnPropertyChanged();
            }
        }

        public double R1
        {
            get => TipoCabo.R1;
            set
            {
                if (TipoCabo.R1 == value)
                    return;

                TipoCabo.R1 = value;
                OnPropertyChanged();
            }
        }

        public double X1
        {
            get => TipoCabo.X1;
            set
            {
                if (TipoCabo.X1 == value)
                    return;

                TipoCabo.X1 = value;
                OnPropertyChanged();
            }
        }

        public double R0
        {
            get => TipoCabo.R0;
            set
            {
                if (TipoCabo.R0 == value)
                    return;

                TipoCabo.R0 = value;
                OnPropertyChanged();
            }
        }

        public double X0
        {
            get => TipoCabo.X0;
            set
            {
                if (TipoCabo.X0 == value)
                    return;

                TipoCabo.X0 = value;
                OnPropertyChanged();
            }
        }

        public double C1
        {
            get => TipoCabo.C1;
            set
            {
                if (TipoCabo.C1 == value)
                    return;

                TipoCabo.C1 = value;
                OnPropertyChanged();
            }
        }

        public double C0
        {
            get => TipoCabo.C0;
            set
            {
                if (TipoCabo.C0 == value)
                    return;

                TipoCabo.C0 = value;
                OnPropertyChanged();
            }
        }

        public double Secao
        {
            get => TipoCabo.Secao;
            set
            {
                if (TipoCabo.Secao == value)
                    return;

                TipoCabo.Secao = value;
                OnPropertyChanged();
            }
        }
    }
}
