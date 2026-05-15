using Araci.Models;
using System.Collections;
using System.Windows.Media;

namespace Araci.ViewModels
{
    public class CargaViewModel
        : ElementoViewModel
    {
        private readonly Carga _carga;

        protected override double LarguraBase => 70;

        protected override double AlturaBase => 70;

        public override IEnumerable TiposDisponiveis =>
            AppServices.Types.TiposCargas;

        public CargaViewModel(
            Carga carga)
            : base(carga)
        {
            _carga = carga;

            VisualState.DefinirVisualBase(
                Brushes.DimGray,
                2);

            AtualizarGeometria();
        }

        public string Nome
        {
            get => _carga.Nome;

            set
            {
                if (_carga.Nome != value)
                {
                    _carga.Nome = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Barra
        {
            get => _carga.Barra;

            set
            {
                if (_carga.Barra != value)
                {
                    _carga.Barra = value;
                    OnPropertyChanged();
                }
            }
        }

        public double PotenciaAtivaKW
        {
            get => _carga.PotenciaAtivaKW;

            set
            {
                if (_carga.PotenciaAtivaKW != value)
                {
                    _carga.PotenciaAtivaKW = value;
                    OnPropertyChanged();
                }
            }
        }

    }
}