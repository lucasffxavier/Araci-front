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
    public class CirculoAnotativoViewModel : ElementoViewModel
    {
        public CirculoAnotativoViewModel(
            CirculoAnotativo modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(modelo, new CirculoAnotativoNode(modelo), types, names, typePropertiesDialogs)
        {
            SelecionarPrimeiroTipoDisponivel();
        }

        public CirculoAnotativo Circulo => (CirculoAnotativo)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposLinhasAnotativas;

        public override TipoElementoViewModel? TipoViewModel => TipoElementoViewModelFactory.Criar(Tipo);

        public TipoLinhaAnotativa? TipoLinha => Circulo.TipoLinha;

        public override TipoElemento Tipo
        {
            get => base.Tipo;
            set
            {
                if (ReferenceEquals(Circulo.Tipo, value))
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
            get => Circulo.PosicaoX;
            set
            {
                if (Math.Abs(Circulo.PosicaoX - value) < 0.0001)
                    return;

                Circulo.PosicaoX = value;
                AtualizarNode();
            }
        }

        public override double Y
        {
            get => Circulo.PosicaoY;
            set
            {
                if (Math.Abs(Circulo.PosicaoY - value) < 0.0001)
                    return;

                Circulo.PosicaoY = value;
                AtualizarNode();
            }
        }

        public override double Largura => Bounds.Width;

        public override double Altura => Bounds.Height;

        public double Raio
        {
            get => Circulo.Raio;
            set
            {
                double novoValor = Math.Max(1, value);

                if (Math.Abs(Circulo.Raio - novoValor) < 0.0001)
                    return;

                Circulo.Raio = novoValor;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Diametro));
                AtualizarNode();
                NotificarParametros();
            }
        }

        public double Diametro => Circulo.Diametro;

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
            get => Circulo.Nome;
            set
            {
                if (Circulo.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string CorLinha
        {
            get => Circulo.CorLinha;
            set
            {
                if (Circulo.CorLinha == value)
                    return;

                Circulo.CorLinha = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StrokeBrush));
                OnPropertyChanged(nameof(RenderData));
                NotificarParametros();
            }
        }

        public double EspessuraLinha
        {
            get => Circulo.EspessuraLinha;
            set
            {
                if (Math.Abs(Circulo.EspessuraLinha - value) < 0.0001)
                    return;

                Circulo.EspessuraLinha = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RenderData));
                NotificarParametros();
            }
        }

        public bool Visivel
        {
            get => Circulo.Visivel;
            set
            {
                if (Circulo.Visivel == value)
                    return;

                Circulo.Visivel = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string EstiloLinha => TipoLinha?.EstiloLinha ?? "Contínuo";

        public Brush StrokeBrush => CriarBrush(CorLinha);

        public DoubleCollection? StrokeDashArray => CriarStrokeDashArray(EstiloLinha);

        public override void Mover(Vector delta)
        {
            Circulo.PosicaoX += delta.X;
            Circulo.PosicaoY += delta.Y;
            AtualizarNode();
        }

        public override ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(
                Circulo.PosicaoX,
                Circulo.PosicaoY,
                Circulo.Raio,
                0,
                Circulo.Rotacao);
        }

        public override void AplicarEstado(ElementoEstado estado)
        {
            Circulo.PosicaoX = estado.X;
            Circulo.PosicaoY = estado.Y;
            Circulo.Raio = estado.X2;
            Circulo.Rotacao = estado.Rotacao;
            AtualizarNode();
        }

        protected override void NotificarGeometria()
        {
            base.NotificarGeometria();
            OnPropertyChanged(nameof(Raio));
            OnPropertyChanged(nameof(Diametro));
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