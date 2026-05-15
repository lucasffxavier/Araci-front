using Araci.Models;
using System.Collections;
using System.Windows.Media;

namespace Araci.ViewModels
{
    public class CargaViewModel
        : ElementoViewModel
    {
        private readonly Carga
            _carga;

        protected override double
            LarguraBase =>
                70;

        protected override double
            AlturaBase =>
                70;

        public override IEnumerable
            TiposDisponiveis =>
                AppServices.Types.TiposCargas;

        public CargaViewModel(
            Carga carga)
            : base(carga)
        {
            _carga = carga;

            SelecionarPrimeiroTipoDisponivel();

            VisualState.DefinirVisualBase(
                Brushes.DimGray,
                2);

            AtualizarGeometria();
        }

        // =========================
        // DADOS
        // =========================

        public string Nome
        {
            get => _carga.Nome;

            set
            {
                if (_carga.Nome == value)
                    return;

                _carga.Nome = value;

                OnPropertyChanged();
            }
        }

        public string Barra
        {
            get => _carga.Barra;

            set
            {
                if (_carga.Barra == value)
                    return;

                _carga.Barra = value;

                OnPropertyChanged();
            }
        }

        public string Alimentador
        {
            get => _carga.Alimentador;

            set
            {
                if (_carga.Alimentador == value)
                    return;

                _carga.Alimentador = value;

                OnPropertyChanged();
            }
        }

        public double PotenciaAtivaKW
        {
            get => _carga.PotenciaAtivaKW;

            set
            {
                if (_carga.PotenciaAtivaKW == value)
                    return;

                _carga.PotenciaAtivaKW = value;

                OnPropertyChanged();
            }
        }

        public double PotenciaReativaKvar
        {
            get => _carga.PotenciaReativaKvar;

            set
            {
                if (_carga.PotenciaReativaKvar == value)
                    return;

                _carga.PotenciaReativaKvar = value;

                OnPropertyChanged();
            }
        }

        protected override string ValidateProperty(
            string propertyName)
        {
            return propertyName switch
            {
                nameof(PotenciaAtivaKW) when PotenciaAtivaKW < 0 =>
                    "Potencia ativa deve ser maior ou igual a zero.",

                nameof(PotenciaReativaKvar) when PotenciaReativaKvar < 0 =>
                    "Potencia reativa deve ser maior ou igual a zero.",

                _ => string.Empty
            };
        }
    }
}
