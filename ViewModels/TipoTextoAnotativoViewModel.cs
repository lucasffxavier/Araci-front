using System.Collections.Generic;
using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoTextoAnotativoViewModel : TipoElementoViewModel
    {
        public TipoTextoAnotativoViewModel(TipoTextoAnotativo tipo)
            : base(tipo)
        {
        }

        protected TipoTextoAnotativo TipoTexto => (TipoTextoAnotativo)_tipo;

        public IReadOnlyList<string> FontesDisponiveis { get; } = new[]
        {
            "Arial",
            "Arial Narrow",
            "Calibri",
            "Segoe UI",
            "Courier New",
            "Times New Roman",
            "ISOCP",
            "ISOCPEUR",
            "Romans",
            "Simplex"
        };

        public IReadOnlyList<string> AlinhamentosDisponiveis { get; } = new[]
        {
            "Esquerda",
            "Centro",
            "Direita"
        };

        public string CorTexto
        {
            get => TipoTexto.CorTexto;
            set
            {
                if (TipoTexto.CorTexto == value)
                    return;

                TipoTexto.CorTexto = value;
                OnPropertyChanged();
            }
        }

        public string Fonte
        {
            get => TipoTexto.Fonte;
            set
            {
                if (TipoTexto.Fonte == value)
                    return;

                TipoTexto.Fonte = value;
                OnPropertyChanged();
            }
        }

        public double AlturaTexto
        {
            get => TipoTexto.AlturaTexto;
            set
            {
                if (System.Math.Abs(TipoTexto.AlturaTexto - value) < 0.0001)
                    return;

                TipoTexto.AlturaTexto = value;
                OnPropertyChanged();
            }
        }

        public string AlinhamentoHorizontal
        {
            get => TipoTexto.AlinhamentoHorizontal;
            set
            {
                if (TipoTexto.AlinhamentoHorizontal == value)
                    return;

                TipoTexto.AlinhamentoHorizontal = value;
                OnPropertyChanged();
            }
        }
    }
}