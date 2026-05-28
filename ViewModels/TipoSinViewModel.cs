using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoSinViewModel : TipoElementoViewModel
    {
        public TipoSinViewModel(TipoSin tipo)
            : base(tipo)
        {
        }

        private TipoSin TipoSin => (TipoSin)_tipo;

        public int Fases
        {
            get => TipoSin.Fases;
            set
            {
                if (TipoSin.Fases == value)
                    return;
                TipoSin.Fases = value;
                OnPropertyChanged();
            }
        }

        public double PotenciaCurtoMVA
        {
            get => TipoSin.PotenciaCurtoMVA;
            set
            {
                if (TipoSin.PotenciaCurtoMVA == value)
                    return;
                TipoSin.PotenciaCurtoMVA = value;
                OnPropertyChanged();
            }
        }

        public double RelacaoXR
        {
            get => TipoSin.RelacaoXR;
            set
            {
                if (TipoSin.RelacaoXR == value)
                    return;
                TipoSin.RelacaoXR = value;
                OnPropertyChanged();
            }
        }
    }
}