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
        private const double LeaderArrowLength = 10.0;
        private const double LeaderArrowHalfWidth = 4.5;
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
        public double AlturaVisual => IsEditingInline ? AlturaEdicao : Altura;

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
                OnPropertyChanged(nameof(AlturaVisual));
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
                OnPropertyChanged(nameof(AlturaVisual));
                AtualizarNode();
                NotificarParametros();
            }
        }

        public new double Rotacao
        {
            get => NormalizarRotacao(Texto.Rotacao);
            set
            {
                double normalizada = NormalizarRotacao(value);

                if (Math.Abs(Texto.Rotacao - normalizada) < 0.000001)
                    return;

                Texto.Rotacao = normalizada;
                OnPropertyChanged();
                AtualizarNode();
                NotificarLeader();
                NotificarParametros();
            }
        }

        public bool LeaderAtivo
        {
            get => Texto.LeaderAtivo;
            set
            {
                if (Texto.LeaderAtivo == value)
                    return;

                bool criouPontoPadrao = value && !LeaderPossuiPontoValido();

                if (criouPontoPadrao)
                    CriarLeaderPadrao();

                Texto.LeaderAtivo = value;
                OnPropertyChanged();

                if (criouPontoPadrao)
                {
                    OnPropertyChanged(nameof(LeaderX));
                    OnPropertyChanged(nameof(LeaderY));
                    OnPropertyChanged(nameof(LeaderPoint));
                }

                NotificarLeader();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public double LeaderX
        {
            get => Texto.LeaderX;
            set
            {
                if (Math.Abs(Texto.LeaderX - value) < 0.000001)
                    return;

                Texto.LeaderX = value;
                OnPropertyChanged();
                NotificarLeader();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public double LeaderY
        {
            get => Texto.LeaderY;
            set
            {
                if (Math.Abs(Texto.LeaderY - value) < 0.000001)
                    return;

                Texto.LeaderY = value;
                OnPropertyChanged();
                NotificarLeader();
                AtualizarNode();
                NotificarParametros();
            }
        }

        public Point LeaderPoint
        {
            get => new(LeaderX, LeaderY);
            set
            {
                LeaderX = value.X;
                LeaderY = value.Y;
                OnPropertyChanged();
                NotificarLeader();
            }
        }

        public bool LeaderVisivel => LeaderAtivo && !IsEditingInline;
        public Point LeaderInicioLocal => CalcularLeaderInicioLocal();
        public Point LeaderFimLocal => WorldToLocal(LeaderPoint);
        public Point LeaderInicioWorld => LocalToWorld(LeaderInicioLocal);
        public PointCollection LeaderArrowPoints => CalcularLeaderArrowPoints();
        public PointCollection LeaderArrowWorldPoints => CalcularLeaderArrowWorldPoints();

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
                OnPropertyChanged(nameof(AlturaVisual));
                NotificarLeader();
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

            if (LeaderAtivo)
            {
                Texto.LeaderX += delta.X;
                Texto.LeaderY += delta.Y;
            }

            AtualizarNode();
            NotificarLeader();
        }

        public override ElementoEstado CapturarEstado()
        {
            return new ElementoEstado(Texto.PosicaoX, Texto.PosicaoY, Texto.LarguraCaixa, Texto.AlturaEstimada, Texto.Rotacao, null, Texto.LeaderAtivo, Texto.LeaderX, Texto.LeaderY);
        }

        public override void AplicarEstado(ElementoEstado estado)
        {
            Texto.PosicaoX = estado.X;
            Texto.PosicaoY = estado.Y;
            Texto.Rotacao = NormalizarRotacao(estado.Rotacao);

            if (estado.X2 > 0)
                Texto.LarguraCaixa = estado.X2;

            Texto.LeaderAtivo = estado.TextoLeaderAtivo;
            Texto.LeaderX = estado.TextoLeaderX;
            Texto.LeaderY = estado.TextoLeaderY;
            AtualizarNode();
            NotificarLeader();
        }

        protected override void NotificarGeometria()
        {
            base.NotificarGeometria();
            OnPropertyChanged(nameof(Conteudo));
            OnPropertyChanged(nameof(LarguraCaixa));
            OnPropertyChanged(nameof(Rotacao));
            OnPropertyChanged(nameof(LeaderAtivo));
            OnPropertyChanged(nameof(LeaderX));
            OnPropertyChanged(nameof(LeaderY));
            OnPropertyChanged(nameof(LeaderPoint));
            OnPropertyChanged(nameof(AlturaEdicao));
            OnPropertyChanged(nameof(AlturaVisual));
            NotificarLeader();
            NotificarParametrosDeTipo();
        }

        private Point CalcularLeaderInicioLocal()
        {
            double largura = Math.Max(1, Largura);
            double altura = Math.Max(1, AlturaVisual);
            Point centro = new(largura / 2, altura / 2);
            Point fim = LeaderFimLocal;
            Vector direcao = fim - centro;

            if (direcao.Length < 0.000001)
                return centro;

            double t = double.PositiveInfinity;

            if (direcao.X > 0.000001)
                t = Math.Min(t, (largura - centro.X) / direcao.X);
            else if (direcao.X < -0.000001)
                t = Math.Min(t, (0 - centro.X) / direcao.X);

            if (direcao.Y > 0.000001)
                t = Math.Min(t, (altura - centro.Y) / direcao.Y);
            else if (direcao.Y < -0.000001)
                t = Math.Min(t, (0 - centro.Y) / direcao.Y);

            if (double.IsNaN(t) || double.IsInfinity(t))
                t = 0;

            t = Math.Max(0, Math.Min(1, t));
            return centro + direcao * t;
        }

        private PointCollection CalcularLeaderArrowPoints()
        {
            Point fim = LeaderFimLocal;
            Point inicio = LeaderInicioLocal;
            Vector direcao = inicio - fim;

            if (direcao.Length < 0.000001)
                direcao = new Vector(0, -1);
            else
                direcao.Normalize();

            Vector normal = new(-direcao.Y, direcao.X);
            Point p1 = fim + direcao * LeaderArrowLength + normal * LeaderArrowHalfWidth;
            Point p2 = fim + direcao * LeaderArrowLength - normal * LeaderArrowHalfWidth;

            return new PointCollection { fim, p1, p2 };
        }

        private PointCollection CalcularLeaderArrowWorldPoints()
        {
            Point fim = LeaderPoint;
            Point inicio = LeaderInicioWorld;
            Vector direcao = inicio - fim;

            if (direcao.Length < 0.000001)
                direcao = new Vector(0, -1);
            else
                direcao.Normalize();

            Vector normal = new(-direcao.Y, direcao.X);
            Point p1 = fim + direcao * LeaderArrowLength + normal * LeaderArrowHalfWidth;
            Point p2 = fim + direcao * LeaderArrowLength - normal * LeaderArrowHalfWidth;

            return new PointCollection { fim, p1, p2 };
        }

        private Point WorldToLocal(Point world)
        {
            double largura = Math.Max(1, Largura);
            double altura = Math.Max(1, AlturaVisual);
            Point centroWorld = new(WorldX + largura / 2, WorldY + altura / 2);
            double radians = -Rotacao * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double dx = world.X - centroWorld.X;
            double dy = world.Y - centroWorld.Y;
            return new Point(largura / 2 + dx * cos - dy * sin, altura / 2 + dx * sin + dy * cos);
        }

        private Point LocalToWorld(Point local)
        {
            double largura = Math.Max(1, Largura);
            double altura = Math.Max(1, AlturaVisual);
            Point centroWorld = new(WorldX + largura / 2, WorldY + altura / 2);
            double radians = Rotacao * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double dx = local.X - largura / 2;
            double dy = local.Y - altura / 2;
            return new Point(centroWorld.X + dx * cos - dy * sin, centroWorld.Y + dx * sin + dy * cos);
        }

        private bool LeaderPossuiPontoValido()
        {
            return Math.Abs(Texto.LeaderX) > 0.000001 || Math.Abs(Texto.LeaderY) > 0.000001;
        }

        private void CriarLeaderPadrao()
        {
            Point ponto = LocalToWorld(new Point(Math.Max(1, Largura) + 70, Math.Max(1, AlturaVisual) + 50));
            Texto.LeaderX = ponto.X;
            Texto.LeaderY = ponto.Y;
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
            OnPropertyChanged(nameof(AlturaVisual));
            NotificarParametrosDeTipo();
            NotificarLeader();
            AtualizarNode();
        }

        private void NotificarLeader()
        {
            OnPropertyChanged(nameof(LeaderVisivel));
            OnPropertyChanged(nameof(LeaderPoint));
            OnPropertyChanged(nameof(LeaderInicioLocal));
            OnPropertyChanged(nameof(LeaderFimLocal));
            OnPropertyChanged(nameof(LeaderInicioWorld));
            OnPropertyChanged(nameof(LeaderArrowPoints));
            OnPropertyChanged(nameof(LeaderArrowWorldPoints));
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

        private static double NormalizarRotacao(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor))
                return 0;

            double normalizada = valor % 360;

            if (normalizada < 0)
                normalizada += 360;

            return normalizada >= 360 ? 0 : normalizada;
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