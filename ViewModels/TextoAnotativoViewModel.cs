using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Araci.Core.Rendering;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services.Catalog;
using Araci.Services.Naming;
using Araci.Services.UI;

namespace Araci.ViewModels
{
    public class TextoAnotativoViewModel : ElementoViewModel
    {
        private TipoTextoAnotativoViewModel? _tipoTextoViewModelAssinado;

        public TextoAnotativoViewModel(
            TextoAnotativo modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(modelo, new TextoAnotativoNode(modelo), types, names, typePropertiesDialogs)
        {
            SelecionarPrimeiroTipoDisponivel();
            AssinarTipoTextoAtual();
            AtualizarAposModeloAlterado();
        }

        public TextoAnotativo Texto => (TextoAnotativo)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposTextosAnotativos;

        public override TipoElementoViewModel? TipoViewModel => TipoElementoViewModelFactory.Criar(Tipo);

        public TipoTextoAnotativo? TipoTexto => Texto.TipoTexto;

        public override TipoElemento Tipo
        {
            get => base.Tipo;
            set
            {
                if (ReferenceEquals(Texto.Tipo, value))
                    return;

                DesassinarTipoTextoAtual();
                base.Tipo = value;
                AssinarTipoTextoAtual();
                NotificarAlteracaoVisualPorTipo();
                NotificarParametros();
            }
        }

        public override double WorldX => Bounds.X;
        public override double WorldY => Bounds.Y;
        public override double Largura => Bounds.Width;
        public override double Altura => Bounds.Height;

        public override ElementoRenderData RenderData => new(Largura, Altura, new Point(0, 0), new Point(Largura, Altura), ForegroundBrush, 1);

        public string Nome
        {
            get => Texto.Nome;
            set
            {
                if (Texto.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Conteudo
        {
            get => Texto.Texto;
            set
            {
                if (Texto.Texto == value)
                    return;

                Texto.Texto = value;
                OnPropertyChanged();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public string CorTexto => Texto.CorTexto;
        public double AlturaTexto => Texto.AlturaTexto;
        public string Fonte => Texto.Fonte;
        public string AlinhamentoHorizontal => Texto.AlinhamentoHorizontal;

        public bool Visivel
        {
            get => Texto.Visivel;
            set
            {
                if (Texto.Visivel == value)
                    return;

                Texto.Visivel = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public Brush ForegroundBrush => CriarBrush(CorTexto);

        public TextAlignment TextAlignment => AlinhamentoHorizontal switch
        {
            "Centro" => TextAlignment.Center,
            "Direita" => TextAlignment.Right,
            _ => TextAlignment.Left
        };

        public FontFamily FontFamily => new(string.IsNullOrWhiteSpace(Fonte) ? "Arial" : Fonte);

        public override void Mover(Vector delta)
        {
            Texto.PosicaoX += delta.X;
            Texto.PosicaoY += delta.Y;
            AtualizarNode();
        }

        public override ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(Texto.PosicaoX, Texto.PosicaoY, Texto.LarguraEstimada, Texto.AlturaEstimada, Texto.Rotacao);
        }

        public override void AplicarEstado(ElementoEstado estado)
        {
            Texto.PosicaoX = estado.X;
            Texto.PosicaoY = estado.Y;
            Texto.Rotacao = estado.Rotacao;
            AtualizarNode();
        }

        protected override void NotificarGeometria()
        {
            base.NotificarGeometria();
            OnPropertyChanged(nameof(Conteudo));
            OnPropertyChanged(nameof(CorTexto));
            OnPropertyChanged(nameof(AlturaTexto));
            OnPropertyChanged(nameof(Fonte));
            OnPropertyChanged(nameof(AlinhamentoHorizontal));
            OnPropertyChanged(nameof(FontFamily));
            OnPropertyChanged(nameof(TextAlignment));
            OnPropertyChanged(nameof(ForegroundBrush));
            OnPropertyChanged(nameof(RenderData));
        }

        private void AssinarTipoTextoAtual()
        {
            if (TipoViewModel is not TipoTextoAnotativoViewModel tipoTextoViewModel)
                return;

            _tipoTextoViewModelAssinado = tipoTextoViewModel;
            _tipoTextoViewModelAssinado.PropertyChanged += OnTipoTextoPropertyChanged;
        }

        private void DesassinarTipoTextoAtual()
        {
            if (_tipoTextoViewModelAssinado == null)
                return;

            _tipoTextoViewModelAssinado.PropertyChanged -= OnTipoTextoPropertyChanged;
            _tipoTextoViewModelAssinado = null;
        }

        private void OnTipoTextoPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName is nameof(TipoTextoAnotativoViewModel.CorTexto) or
                    nameof(TipoTextoAnotativoViewModel.Fonte) or
                    nameof(TipoTextoAnotativoViewModel.AlturaTexto) or
                    nameof(TipoTextoAnotativoViewModel.AlinhamentoHorizontal))
            {
                NotificarAlteracaoVisualPorTipo();
            }
        }

        private void NotificarAlteracaoVisualPorTipo()
        {
            OnPropertyChanged(nameof(TipoTexto));
            OnPropertyChanged(nameof(CorTexto));
            OnPropertyChanged(nameof(Fonte));
            OnPropertyChanged(nameof(AlturaTexto));
            OnPropertyChanged(nameof(AlinhamentoHorizontal));
            OnPropertyChanged(nameof(ForegroundBrush));
            OnPropertyChanged(nameof(FontFamily));
            OnPropertyChanged(nameof(TextAlignment));
            OnPropertyChanged(nameof(RenderData));
            AtualizarNode();
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