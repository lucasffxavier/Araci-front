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

        public TipoGeradorViewModel(
            TipoGerador tipo)
            : base(tipo)
        {
        }

        // =========================
        // PROPRIEDADES
        // =========================

        public string CategoriaGerador
        {
            get => TipoGerador.CategoriaGerador;

            set
            {
                if (TipoGerador.CategoriaGerador == value)
                    return;

                TipoGerador.CategoriaGerador = value;

                OnPropertyChanged();
            }
        }

        public string Fabricante
        {
            get => TipoGerador.Fabricante;

            set
            {
                if (TipoGerador.Fabricante == value)
                    return;

                TipoGerador.Fabricante = value;

                OnPropertyChanged();
            }
        }

        public string Modelo
        {
            get => TipoGerador.Modelo;

            set
            {
                if (TipoGerador.Modelo == value)
                    return;

                TipoGerador.Modelo = value;

                OnPropertyChanged();
            }
        }

        public double PotenciaNominalKW
        {
            get => TipoGerador.PotenciaNominalKW;

            set
            {
                if (TipoGerador.PotenciaNominalKW == value)
                    return;

                TipoGerador.PotenciaNominalKW = value;

                OnPropertyChanged();
            }
        }

        public double TensaoKV
        {
            get => TipoGerador.TensaoKV;

            set
            {
                if (TipoGerador.TensaoKV == value)
                    return;

                TipoGerador.TensaoKV = value;

                OnPropertyChanged();
            }
        }

        public int Fases
        {
            get => TipoGerador.Fases;

            set
            {
                if (TipoGerador.Fases == value)
                    return;

                TipoGerador.Fases = value;

                OnPropertyChanged();
            }
        }
    }
}
