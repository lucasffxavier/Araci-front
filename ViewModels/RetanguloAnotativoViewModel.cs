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
    public class RetanguloAnotativoViewModel : ElementoViewModel
    {
        public RetanguloAnotativoViewModel(
            RetanguloAnotativo modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(modelo, new RetanguloAnotativoNode(modelo), types, names, typePropertiesDialogs)
        {
            SelecionarPrimeiroTipoDisponivel();
        }

        public RetanguloAnotativo Retangulo => (RetanguloAnotativo)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposLinhasAnotativas;

        public override TipoElementoViewModel? TipoViewModel => TipoElementoViewModelFactory.Criar(Tipo);

        public TipoLinhaAnotativa? TipoLinha => Retangulo.TipoLinha;

        public override TipoElemento Tipo
        {
            get => base.Tipo;
            set
            {
                if (ReferenceEquals(Retangulo.Tipo, value))
                    return;

                base.Tipo = value;
                OnPropertyChanged(nameof(TipoLinha));
                OnPropertyChanged(nameof(EstiloLinha));
                OnPropertyChanged(nameof(StrokeDashArray));
                OnPropertyChanged(nameof(RenderData));
            }
        }

        public override double WorldX => Bounds.X;

        public override double WorldY => Bounds.Y;

        public override double X
        {
            get => Retangulo.PosicaoX;
            set
            {
                if (Math.Abs(Retangulo.PosicaoX - value) < 0.0001)
                    return;

                Retangulo.PosicaoX = value;
                AtualizarNode();
            }
        }

        public override double Y
        {
            get => Retangulo.PosicaoY;
            set
            {
                if (Math.Abs(Retangulo.PosicaoY - value) < 0.0001)
                    return;

                Retangulo.PosicaoY = value;
                AtualizarNode();
            }
        }

        public override double Largura => Bounds.Width;

        public override double Altura => Bounds.Height;

        public override ElementoRenderData RenderData => new(
            Largura,
            Altura,
            new Point(0, 0),
            new Point(Largura, Altura),
            CriarBrush(CorLinha),
            EspessuraLinha,
            StrokeDashArray);

        public string Nome
        {
            get => Retangulo.Nome;
            set
            {
                if (Retangulo.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public double LarguraRetangulo
        {
            get => Retangulo.Largura;
            set
            {
                if (Math.Abs(Retangulo.Largura - value) < 0.0001)
                    return;

                Retangulo.Largura = value;
                OnPropertyChanged();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public double AlturaRetangulo
        {
            get => Retangulo.Altura;
            set
            {
                if (Math.Abs(Retangulo.Altura - value) < 0.0001)
                    return;

                Retangulo.Altura = value;
                OnPropertyChanged();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public string CorLinha
        {
            get => Retangulo.CorLinha;
            set
            {
                if (Retangulo.CorLinha == value)
                    return;

                Retangulo.CorLinha = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StrokeBrush));
                OnPropertyChanged(nameof(RenderData));
                NotificarParametros();
            }
        }

        public double EspessuraLinha
        {
            get => Retangulo.EspessuraLinha;
            set
            {
                if (Math.Abs(Retangulo.EspessuraLinha - value) < 0.0001)
                    return;

                Retangulo.EspessuraLinha = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RenderData));
                NotificarParametros();
            }
        }

        public bool Visivel
        {
            get => Retangulo.Visivel;
            set
            {
                if (Retangulo.Visivel == value)
                    return;

                Retangulo.Visivel = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string EstiloLinha => TipoLinha?.EstiloLinha ?? "Contínuo";

        public Brush StrokeBrush => CriarBrush(CorLinha);

        public DoubleCollection? StrokeDashArray => CriarStrokeDashArray(EstiloLinha);

        public override void Mover(Vector delta)
        {
            Retangulo.PosicaoX += delta.X;
            Retangulo.PosicaoY += delta.Y;
            AtualizarNode();
        }

        public override ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(
                Retangulo.PosicaoX,
                Retangulo.PosicaoY,
                Retangulo.Largura,
                Retangulo.Altura,
                Retangulo.Rotacao);
        }

        public override void AplicarEstado(ElementoEstado estado)
        {
            Retangulo.PosicaoX = estado.X;
            Retangulo.PosicaoY = estado.Y;
            Retangulo.Largura = estado.X2;
            Retangulo.Altura = estado.Y2;
            Retangulo.Rotacao = estado.Rotacao;
            AtualizarNode();
        }

        protected override void NotificarGeometria()
        {
            base.NotificarGeometria();
            OnPropertyChanged(nameof(LarguraRetangulo));
            OnPropertyChanged(nameof(AlturaRetangulo));
            OnPropertyChanged(nameof(RenderData));
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

        private static DoubleCollection? CriarStrokeDashArray(string estilo)
        {
            string normalizado = NormalizarEstilo(estilo);

            return normalizado switch
            {
                "tracejado" => new DoubleCollection { 6, 4 },
                "tracoponto" => new DoubleCollection { 8, 3, 2, 3 },
                "tracodoispontos" => new DoubleCollection { 8, 3, 2, 3, 2, 3 },
                _ => null
            };
        }

        private static string NormalizarEstilo(string valor)
        {
            return valor
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("ç", "c")
                .Replace("Ç", "c")
                .Replace("ã", "a")
                .Replace("Ã", "a")
                .Replace("í", "i")
                .Replace("Í", "i")
                .ToLowerInvariant()
                .Trim();
        }
    }
}