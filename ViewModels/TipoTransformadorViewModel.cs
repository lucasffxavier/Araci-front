using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoTransformadorViewModel : TipoElementoViewModel
    {
        public TipoTransformadorViewModel(TipoTransformador tipo)
            : base(tipo)
        {
        }

        private TipoTransformador TipoTransformador => (TipoTransformador)_tipo;

        public int Fases
        {
            get => TipoTransformador.Fases;
            set
            {
                if (TipoTransformador.Fases == value)
                    return;

                TipoTransformador.Fases = value;
                OnPropertyChanged();
            }
        }

        public int Enrolamentos
        {
            get => TipoTransformador.Enrolamentos;
            set
            {
                if (TipoTransformador.Enrolamentos == value)
                    return;

                TipoTransformador.Enrolamentos = value;
                OnPropertyChanged();
            }
        }

        public double TensaoPrimarioKV
        {
            get => TipoTransformador.TensaoPrimarioKV;
            set
            {
                if (TipoTransformador.TensaoPrimarioKV == value)
                    return;

                TipoTransformador.TensaoPrimarioKV = value;
                OnPropertyChanged();
            }
        }

        public double TensaoSecundarioKV
        {
            get => TipoTransformador.TensaoSecundarioKV;
            set
            {
                if (TipoTransformador.TensaoSecundarioKV == value)
                    return;

                TipoTransformador.TensaoSecundarioKV = value;
                OnPropertyChanged();
            }
        }

        public double PotenciaKVA
        {
            get => TipoTransformador.PotenciaKVA;
            set
            {
                if (TipoTransformador.PotenciaKVA == value)
                    return;

                TipoTransformador.PotenciaKVA = value;
                OnPropertyChanged();
            }
        }

        public double PotenciaMVA
        {
            get => TipoTransformador.PotenciaMVA;
            set
            {
                if (TipoTransformador.PotenciaMVA == value)
                    return;

                TipoTransformador.PotenciaMVA = value;
                OnPropertyChanged();
            }
        }
    }
}
