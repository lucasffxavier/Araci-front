using Araci.Models;

namespace Araci.ViewModels
{
    public class CaboViewModel : ElementoViewModel
    {
        // =========================
        // MODELO
        // =========================

        private readonly Cabo _cabo;

        // =========================
        // CONSTRUTOR
        // =========================

        public CaboViewModel(Cabo cabo)
            : base(cabo)
        {
            _cabo = cabo;
        }

        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string Nome
        {
            get => _cabo.Nome;
            set
            {
                if (_cabo.Nome != value)
                {
                    _cabo.Nome = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // BARRAS
        // =========================

        public string Barra1
        {
            get => _cabo.Barra1;
            set
            {
                if (_cabo.Barra1 != value)
                {
                    _cabo.Barra1 = value;

                    OnPropertyChanged();
                }
            }
        }

        public string Barra2
        {
            get => _cabo.Barra2;
            set
            {
                if (_cabo.Barra2 != value)
                {
                    _cabo.Barra2 = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // COMPRIMENTO
        // =========================

        public double Comprimento
        {
            get => _cabo.Comprimento;
            set
            {
                if (_cabo.Comprimento != value)
                {
                    _cabo.Comprimento = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // LINECODE
        // =========================

        public string LineCode
        {
            get => _cabo.LineCode;
            set
            {
                if (_cabo.LineCode != value)
                {
                    _cabo.LineCode = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // IMPEDÂNCIA
        // =========================

        public double Resistencia
        {
            get => _cabo.Resistencia;
            set
            {
                if (_cabo.Resistencia != value)
                {
                    _cabo.Resistencia = value;

                    OnPropertyChanged();
                }
            }
        }

        public double Reatancia
        {
            get => _cabo.Reatancia;
            set
            {
                if (_cabo.Reatancia != value)
                {
                    _cabo.Reatancia = value;

                    OnPropertyChanged();
                }
            }
        }

        public double Capacitancia
        {
            get => _cabo.Capacitancia;
            set
            {
                if (_cabo.Capacitancia != value)
                {
                    _cabo.Capacitancia = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // AMPACIDADE
        // =========================

        public double Ampacidade
        {
            get => _cabo.Ampacidade;
            set
            {
                if (_cabo.Ampacidade != value)
                {
                    _cabo.Ampacidade = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // FASES
        // =========================

        public int Fases
        {
            get => _cabo.Fases;
            set
            {
                if (_cabo.Fases != value)
                {
                    _cabo.Fases = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // NEUTRO
        // =========================

        public bool Neutro
        {
            get => _cabo.Neutro;
            set
            {
                if (_cabo.Neutro != value)
                {
                    _cabo.Neutro = value;

                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // GEOMETRIA
        // =========================

        public double X2
        {
            get => _cabo.PosicaoX2;
            set
            {
                if (_cabo.PosicaoX2 != value)
                {
                    _cabo.PosicaoX2 = value;

                    OnPropertyChanged();
                }
            }
        }

        public double Y2
        {
            get => _cabo.PosicaoY2;
            set
            {
                if (_cabo.PosicaoY2 != value)
                {
                    _cabo.PosicaoY2 = value;

                    OnPropertyChanged();
                }
            }
        }
    }
}