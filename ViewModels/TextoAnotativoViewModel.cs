using System;
using System.Collections;
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
        private bool _isEditingInline;
        private string _conteudoEdicao = string.Empty;

        public TextoAnotativoViewModel(
            TextoAnotativo modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs)
            : base(modelo, new TextoAnotativoNode(modelo), types, names, typePropertiesDialogs)
        {
            SelecionarPrimeiroTipoDisponivel();

            if (Texto.LarguraCaixa <= TextoAnotativo.LarguraCaixaMinima + 0.000001)
                Texto.AjustarLarguraAoConteudo();

            AtualizarAposModeloAlterado();
        }

        public TextoAnotativo Texto => (TextoAnotativo)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposTextosAnotativos;

        public override TipoElementoViewModel? TipoViewModel
        {
            get
            {
                if (Texto.TipoTexto != null)
                    return new TipoTextoAnotativoViewModel(Texto.TipoTexto, Types.TiposTextosAnotativos, SelecionarTipoTexto, NotificarAlteracaoVisualPorTipo);

                return null;
            }
        }

        public TipoTextoAnotativo? TipoTexto => Texto.TipoTexto;

        public override TipoElemento Tipo
        {
            get => base.Tipo;
            set
            {
                if (ReferenceEquals(Texto.Tipo, value))
                    return;

                base.Tipo = value;
                NotificarAlteracaoVisualPorTipo();
                NotificarParametros();
            }
        }

        public override double WorldX => Bounds.X;
        public override double WorldY => Bounds.Y;
        public override double Largura => Bounds.Width;
        public override double Altura => Bounds.Height;

        public double AlturaEdicao => Math.Max(Texto.AlturaEstimada, TextoAnotativo.CalcularAlturaEstimada(ConteudoEdicao, LarguraCaixa, AlturaTexto));

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

                Texto.Texto = value ?? string.Empty;
                OnPropertyChanged();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public string ConteudoEdicao
        {
            get => _conteudoEdicao;
            set
            {
                if (_conteudoEdicao == value)
                    return;

                _conteudoEdicao = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AlturaEdicao));
            }
        }

        public double LarguraCaixa
        {
            get => Texto.LarguraCaixa;
            set
            {
                if (Math.Abs(Texto.LarguraCaixa - value) < 0.000001)
                    return;

                Texto.LarguraCaixa = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AlturaEdicao));
                AtualizarNode();
                NotificarParametros();
            }
        }

        public bool IsEditingInline
        {
            get => _isEditingInline;
            private set
            {
                if (_isEditingInline == value)
                    return;

                _isEditingInline = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditingInline));
                OnPropertyChanged(nameof(IsInteractionLocked));
                OnPropertyChanged(nameof(AlturaEdicao));
                AtualizarNode();
            }
        }

        public bool IsNotEditingInline => !IsEditingInline;
        public bool IsInteractionLocked => IsEditingInline;

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

        public HorizontalAlignment TextBoxHorizontalContentAlignment => AlinhamentoHorizontal switch
        {
            "Centro" => HorizontalAlignment.Center,
            "Direita" => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Left
        };

        public FontFamily FontFamily => new(string.IsNullOrWhiteSpace(Fonte) ? "Arial" : Fonte);

        public void IniciarEdicaoInline()
        {
            ConteudoEdicao = Conteudo;
            IsEditingInline = true;
            IsHover = false;
        }

        public void CancelarEdicaoInline()
        {
            ConteudoEdicao = Conteudo;
            IsEditingInline = false;
        }

        public void EncerrarEdicaoInline()
        {
            IsEditingInline = false;
        }

        public void AtualizarAposTipoAlterado()
        {
            NotificarAlteracaoVisualPorTipo();
        }

        public override void Mover(Vector delta)
        {
            if (IsEditingInline)
                return;

            Texto.PosicaoX += delta.X;
            Texto.PosicaoY += delta.Y;
            AtualizarNode();
        }

        public override ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(Texto.PosicaoX, Texto.PosicaoY, Texto.LarguraCaixa, Texto.AlturaEstimada, Texto.Rotacao);
        }

        public override void AplicarEstado(ElementoEstado estado)
        {
            Texto.PosicaoX = estado.X;
            Texto.PosicaoY = estado.Y;
            Texto.Rotacao = estado.Rotacao;

            if (estado.X2 > 0)
                Texto.LarguraCaixa = estado.X2;

            AtualizarNode();
        }

        protected override void NotificarGeometria()
        {
            base.NotificarGeometria();
            OnPropertyChanged(nameof(Conteudo));
            OnPropertyChanged(nameof(LarguraCaixa));
            OnPropertyChanged(nameof(AlturaEdicao));
            NotificarParametrosDeTipo();
        }

        private void SelecionarTipoTexto(TipoTextoAnotativo tipo)
        {
            Tipo = tipo;
        }

        private void NotificarAlteracaoVisualPorTipo()
        {
            OnPropertyChanged(nameof(TipoTexto));
            OnPropertyChanged(nameof(TipoViewModel));
            OnPropertyChanged(nameof(AlturaEdicao));
            NotificarParametrosDeTipo();
            AtualizarNode();
        }

        private void NotificarParametrosDeTipo()
        {
            OnPropertyChanged(nameof(CorTexto));
            OnPropertyChanged(nameof(Fonte));
            OnPropertyChanged(nameof(AlturaTexto));
            OnPropertyChanged(nameof(AlinhamentoHorizontal));
            OnPropertyChanged(nameof(ForegroundBrush));
            OnPropertyChanged(nameof(FontFamily));
            OnPropertyChanged(nameof(TextAlignment));
            OnPropertyChanged(nameof(TextBoxHorizontalContentAlignment));
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
    }
}
