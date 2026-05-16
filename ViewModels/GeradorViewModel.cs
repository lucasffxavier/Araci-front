using System.Collections;

using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;

namespace Araci.ViewModels
{
    public class GeradorViewModel
        : ElementoViewModel
    {
        public GeradorViewModel(
            Gerador modelo,
            TypeLibraryService types)
            : base(
                modelo,
                new EquipamentoNode(modelo),
                types)
        {
            SelecionarPrimeiroTipoDisponivel();
        }

        public Gerador Gerador =>
            (Gerador)Modelo;

        public override IEnumerable TiposDisponiveis =>
            Types.TiposGeradores;

        public double PotenciaAtivaKW
        {
            get => Gerador.PotenciaAtivaKW;

            set
            {
                if (Gerador.PotenciaAtivaKW
                    == value)
                {
                    return;
                }

                Gerador.PotenciaAtivaKW =
                    value;

                OnPropertyChanged();
            }
        }

        public double FatorPotencia
        {
            get => Gerador.FatorPotencia;

            set
            {
                if (Gerador.FatorPotencia
                    == value)
                {
                    return;
                }

                Gerador.FatorPotencia =
                    value;

                OnPropertyChanged();
            }
        }

        public string Alimentador
        {
            get => Gerador.Alimentador;

            set
            {
                if (Gerador.Alimentador
                    == value)
                {
                    return;
                }

                Gerador.Alimentador =
                    value;

                OnPropertyChanged();
            }
        }


        public string Nome
        {
            get => Gerador.Nome;

            set
            {
                if (Gerador.Nome
                    == value)
                {
                    return;
                }

                Gerador.Nome =
                    value;

                OnPropertyChanged();
            }
        }


        public string Barra
        {
            get => Gerador.Barra;

            set
            {
                if (Gerador.Barra
                    == value)
                {
                    return;
                }

                Gerador.Barra =
                    value;

                OnPropertyChanged();
            }
        }
    }
}