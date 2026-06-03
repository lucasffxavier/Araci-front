using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using Araci.Core.Rendering;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services;
using Araci.Services.Catalog;
using Araci.Services.Naming;
using Araci.Services.UI;

namespace Araci.ViewModels
{
    public class LinhaAnotativaViewModel : ElementoViewModel
    {
        public LinhaAnotativaViewModel(
            LinhaAnotativa modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(modelo, new LinhaAnotativaNode(modelo), types, names, typePropertiesDialogs)
        {
        }

        public LinhaAnotativa Linha => (LinhaAnotativa)Modelo;

        public override IEnumerable TiposDisponiveis => Array.Empty<TipoElemento>();

        public override TipoElementoViewModel? TipoViewModel => null;

        public override double WorldX => Bounds.X;

        public override double WorldY => Bounds.Y;

        public override ElementoRenderData RenderData => new(
            Largura,
            Altura,
            PontoLocalInicial,
            PontoLocalFinal,
            CriarBrush(CorLinha),
            EspessuraLinha);

        public string Nome
        {
            get => Linha.Nome;
            set
            {
                if (Linha.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double X2
        {
            get => Linha.X2;
            set
            {
                if (Math.Abs(Linha.X2 - value) < 0.0001)
                    return;

                Linha.X2 = value;
                OnPropertyChanged();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public double Y2
        {
            get => Linha.Y2;
            set
            {
                if (Math.Abs(Linha.Y2 - value) < 0.0001)
                    return;

                Linha.Y2 = value;
                OnPropertyChanged();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public string CorLinha
        {
            get => Linha.CorLinha;
            set
            {
                if (Linha.CorLinha == value)
                    return;

                Linha.CorLinha = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RenderData));
                NotificarParametros();
            }
        }

        public double EspessuraLinha
        {
            get => Linha.EspessuraLinha;
            set
            {
                if (Math.Abs(Linha.EspessuraLinha - value) < 0.0001)
                    return;

                Linha.EspessuraLinha = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RenderData));
                NotificarParametros();
            }
        }

        public bool Visivel
        {
            get => Linha.Visivel;
            set
            {
                if (Linha.Visivel == value)
                    return;

                Linha.Visivel = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        protected override Point PontoLocalInicial
        {
            get
            {
                var node = (LinhaAnotativaNode)Node;
                return new Point(
                    node.PontoInicial.X - Bounds.X,
                    node.PontoInicial.Y - Bounds.Y);
            }
        }

        protected override Point PontoLocalFinal
        {
            get
            {
                var node = (LinhaAnotativaNode)Node;
                return new Point(
                    node.PontoFinal.X - Bounds.X,
                    node.PontoFinal.Y - Bounds.Y);
            }
        }

        private static Brush CriarBrush(string cor)
        {
            try
            {
                object? valor = ColorConverter.ConvertFromString(cor);

                if (valor is Color color)
                    return new SolidColorBrush(color);
            }
            catch (FormatException)
            {
            }

            return Brushes.Black;
        }
    }
}
