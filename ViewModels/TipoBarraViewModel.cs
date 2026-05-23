using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoBarraViewModel : TipoElementoViewModel
    {
        public TipoBarraViewModel(TipoBarra tipo)
            : base(tipo)
        {
        }

        protected TipoBarra TipoBarra => (TipoBarra)_tipo;

        public string ClasseTensao
        {
            get => TipoBarra.ClasseTensao;
            set
            {
                if (TipoBarra.ClasseTensao == value)
                    return;

                TipoBarra.ClasseTensao = value;
                OnPropertyChanged();
            }
        }

        public int Fases
        {
            get => TipoBarra.Fases;
            set
            {
                if (TipoBarra.Fases == value)
                    return;

                TipoBarra.Fases = value;
                OnPropertyChanged();
            }
        }

        public double AlturaPadrao
        {
            get => TipoBarra.AlturaPadrao;
            set
            {
                if (TipoBarra.AlturaPadrao == value)
                    return;

                TipoBarra.AlturaPadrao = value;
                OnPropertyChanged();
            }
        }

        public int NumeroConexoes
        {
            get => TipoBarra.NumeroConexoes;
            set
            {
                if (TipoBarra.NumeroConexoes == value)
                    return;

                TipoBarra.NumeroConexoes = value;
                OnPropertyChanged();
            }
        }
    }
}
