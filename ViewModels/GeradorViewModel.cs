    using Araci.Models;

    namespace Araci.ViewModels
    {
        public class GeradorViewModel : ElementoViewModel
        {
            // =========================
            // MODELO
            // =========================

            private readonly Gerador _gerador;

            // =========================
            // CONSTRUTOR
            // =========================

            public GeradorViewModel(Gerador gerador)
                : base(gerador)
            {
                _gerador = gerador;
            }

            // =========================
            // IDENTIFICAÇÃO
            // =========================

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

            // =========================
            // ELÉTRICO
            // =========================

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
        }
    }