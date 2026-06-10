using System;
using System.Collections;
using System.ComponentModel;
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
        private TipoLinhaAnotativa? _tipoLinhaAssinado;

        public LinhaAnotativaViewModel(
            LinhaAnotativa modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(modelo, new LinhaAnotativaNode(modelo), types, names, typePropertiesDialogs)
        {
            SelecionarPrimeiroTipoDisponivel();
            AtualizarAssinaturaTipoLinha();
        }

        public LinhaAnotativa Linha => (LinhaAnotativa)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposLinhasAnotativas;

        public override TipoElementoViewModel? TipoViewModel => TipoElementoViewModelFactory.Criar(Tipo);

        public TipoLinhaAnotativa? TipoLinha => Linha.TipoLinha;

        public override TipoElemento Tipo
        {
            get => base.Tipo;
            set
            {
                if (ReferenceEquals(Linha.Tipo, value))
                    return;

                DesassinarTipoLinha();
                base.Tipo = value;
                AtualizarAssinaturaTipoLinha();
                OnPropertyChanged(nameof(TipoLinha));
                NotificarEstiloTipoLinha();
            }
        }

        public override double WorldX => Bounds.X;

        public override double WorldY => Bounds.Y;

        public override double X
        {
            get => Linha.PosicaoX;
            set
            {
                if (Math.Abs(Linha.PosicaoX - value) < 0.0001)
                    return;

                Linha.PosicaoX = value;
                AtualizarNode();
            }
        }

        public override double Y
        {
            get => Linha.PosicaoY;
            set
            {
                if (Math.Abs(Linha.PosicaoY - value) < 0.0001)
                    return;

                Linha.PosicaoY = value;
                AtualizarNode();
            }
        }

        public override ElementoRenderData RenderData => new(
            Largura,
            Altura,
            PontoLocalInicial,
            PontoLocalFinal,
            CriarBrush(CorLinha),
            EspessuraLinha,
            StrokeDashArray);

        public double Comprimento => Math.Sqrt(X2 * X2 + Y2 * Y2);

        public Point PontoInicial => new(Linha.PosicaoX, Linha.PosicaoY);

        public Point PontoFinal => new(Linha.PosicaoX + X2, Linha.PosicaoY + Y2);

        public DoubleCollection? StrokeDashArray => CriarStrokeDashArray(EstiloLinha);

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
                string normalizada = TipoLinhaAnotativa.NormalizarCor(value);

                if (Linha.CorLinha == normalizada)
                    return;

                Linha.CorLinha = normalizada;
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
                double normalizada = TipoLinhaAnotativa.NormalizarEspessura(value);

                if (Math.Abs(Linha.EspessuraLinha - normalizada) < 0.0001)
                    return;

                Linha.EspessuraLinha = normalizada;
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

        public string EstiloLinha => TipoLinha?.EstiloLinha ?? "Contínuo";

        public override void Mover(Vector delta)
        {
            Linha.PosicaoX += delta.X;
            Linha.PosicaoY += delta.Y;
            AtualizarNode();
        }

        public override ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(
                Linha.PosicaoX,
                Linha.PosicaoY,
                Linha.X2,
                Linha.Y2,
                Linha.Rotacao);
        }

        public override void AplicarEstado(ElementoEstado estado)
        {
            Linha.PosicaoX = estado.X;
            Linha.PosicaoY = estado.Y;
            Linha.X2 = estado.X2;
            Linha.Y2 = estado.Y2;
            Linha.Rotacao = estado.Rotacao;
            AtualizarNode();
        }

        protected override void NotificarGeometria()
        {
            base.NotificarGeometria();
            OnPropertyChanged(nameof(PontoInicial));
            OnPropertyChanged(nameof(PontoFinal));
            OnPropertyChanged(nameof(Comprimento));
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

        private void AtualizarAssinaturaTipoLinha()
        {
            _tipoLinhaAssinado = TipoLinha;

            if (_tipoLinhaAssinado != null)
                _tipoLinhaAssinado.PropertyChanged += OnTipoLinhaPropertyChanged;
        }

        private void DesassinarTipoLinha()
        {
            if (_tipoLinhaAssinado != null)
                _tipoLinhaAssinado.PropertyChanged -= OnTipoLinhaPropertyChanged;

            _tipoLinhaAssinado = null;
        }

        private void OnTipoLinhaPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.EstiloLinha))
            {
                NotificarEstiloTipoLinha();
            }
        }

        private void NotificarEstiloTipoLinha()
        {
            OnPropertyChanged(nameof(EstiloLinha));
            OnPropertyChanged(nameof(StrokeDashArray));
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