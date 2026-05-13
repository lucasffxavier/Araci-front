using Araci.Models;
using System.Windows.Media;

namespace Araci.ViewModels
{
    public class CargaViewModel : ElementoViewModel
    {
        // =========================
        // MODELO
        // =========================

        private readonly Carga _carga;
        public override double Largura => 70;
        public override double Altura => 70;
        // =========================
        // CONSTRUTOR
        // =========================

        public CargaViewModel(Carga carga)
            : base(carga)
        {
            _carga = carga;

            VisualState.DefinirVisualBase(
                Brushes.DimGray,
                2);
        }

        // =========================
        // IDENTIFICAÇÃO
        // =========================

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

        // =========================
        // PARÂMETROS
        // =========================

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

        public double TensaoKV
        {
            get => _carga.TensaoKV;
            set
            {
                if (_carga.TensaoKV != value)
                {
                    _carga.TensaoKV = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Conexao
        {
            get => _carga.Conexao;
            set
            {
                if (_carga.Conexao != value)
                {
                    _carga.Conexao = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}