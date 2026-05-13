using Araci.Models;
using System.Windows.Media;

namespace Araci.ViewModels
{
    public class GeradorViewModel
        : ElementoViewModel
    {
        private readonly Gerador _gerador;

        protected override double LarguraBase => 80;

        protected override double AlturaBase => 80;

        public GeradorViewModel(
            Gerador gerador)
            : base(gerador)
        {
            _gerador = gerador;

            VisualState.DefinirVisualBase(
                Brushes.DimGray,
                2);

            AtualizarGeometria();
        }

        public string Nome
        {
            get => _gerador.Nome;

            set
            {
                if (_gerador.Nome != value)
                {
                    _gerador.Nome = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Barra
        {
            get => _gerador.Barra;

            set
            {
                if (_gerador.Barra != value)
                {
                    _gerador.Barra = value;
                    OnPropertyChanged();
                }
            }
        }

        public double PotenciaAtivaKW
        {
            get => _gerador.PotenciaAtivaKW;

            set
            {
                if (_gerador.PotenciaAtivaKW != value)
                {
                    _gerador.PotenciaAtivaKW = value;
                    OnPropertyChanged();
                }
            }
        }

        public double TensaoKV
        {
            get => _gerador.TensaoKV;

            set
            {
                if (_gerador.TensaoKV != value)
                {
                    _gerador.TensaoKV = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Fabricante
        {
            get => _gerador.Fabricante;

            set
            {
                if (_gerador.Fabricante != value)
                {
                    _gerador.Fabricante = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}