using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Araci.Core.Documents;
using Araci.Models.Tipos;
using Araci.Services.Catalog;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateTextViewModel : INotifyPropertyChanged
    {
        private const double TextHorizontalMargin = 8.0;
        private const double DefaultSelectionThickness = 1.5;

        private readonly ProjectSheetTemplateText _texto;
        private readonly TypeLibraryService _types;
        private TipoTextoAnotativo? _tipoTexto;
        private double _previewOffsetX;
        private double _previewOffsetY;
        private bool _hasPreviewBoxWidth;
        private double _previewBoxWidth;
        private bool _hasPreviewRotation;
        private double _previewRotation;
        private bool _isSelected;
        private bool _isEditingInline;
        private bool _hasPreviewLeaderPoint;
        private Point _previewLeaderPoint;
        private bool _hasPreviewLeaderCotoveloPoint;
        private Point _previewLeaderCotoveloPoint;
        private string _conteudoEdicao = string.Empty;

        public ProjectSheetTemplateTextViewModel(ProjectSheetTemplateText texto)
            : this(texto, new TypeLibraryService())
        {
        }

        public ProjectSheetTemplateTextViewModel(ProjectSheetTemplateText texto, TypeLibraryService types)
        {
            _texto = texto ?? throw new ArgumentNullException(nameof(texto));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _conteudoEdicao = _texto.Texto;
            AtualizarTipoTexto();
        }

        public Guid Id => _texto.Id;
        public double X => _texto.X + _previewOffsetX;
        public double Y => _texto.Y + _previewOffsetY;
        public double ModelX => _texto.X;
        public double ModelY => _texto.Y;
        public string Nome => _texto.Nome;
        public string Conteudo => _texto.Texto;

        public string ConteudoEdicao
        {
            get => _conteudoEdicao;
            set
            {
                string normalizado = value ?? string.Empty;

                if (string.Equals(_conteudoEdicao, normalizado, StringComparison.Ordinal))
                    return;

                _conteudoEdicao = normalizado;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AlturaEdicao));
                OnPropertyChanged(nameof(AlturaVisual));
                NotificarLeader();
            }
        }

        public double LarguraCaixa => _hasPreviewBoxWidth ? _previewBoxWidth : _texto.LarguraCaixa;
        public double ModelLarguraCaixa => _texto.LarguraCaixa;
        public double Rotacao => _hasPreviewRotation ? _previewRotation : NormalizarRotacao(_texto.Rotacao);
        public double ModelRotacao => NormalizarRotacao(_texto.Rotacao);
        public TipoTextoAnotativo? TipoTexto => _tipoTexto;
        public string CorTexto => NormalizarCor(_texto.CorTexto);
        public string Fonte => NormalizarFonte(_texto.Fonte);
        public double AlturaTexto => NormalizarAltura(_texto.AlturaTexto);
        public string AlinhamentoHorizontal => NormalizarAlinhamento(_texto.AlinhamentoHorizontal);
        public bool Visible => _texto.Visible;
        public Brush ForegroundBrush => CriarBrush(CorTexto);
        public FontFamily FontFamily => new(string.IsNullOrWhiteSpace(Fonte) ? ProjectSheetTemplateText.DefaultFont : Fonte);
        public bool LeaderAtivo => _texto.LeaderAtivo;
        public bool LeaderComCotovelo => _texto.LeaderComCotovelo;
        public bool LeaderVisivel => LeaderAtivo && !IsEditingInline;
        public bool LeaderRetoVisivel => LeaderVisivel && !LeaderComCotovelo;
        public bool LeaderCotoveloVisivel => LeaderVisivel && LeaderComCotovelo;
        public string LeaderEstiloSeta => TipoTexto?.LeaderEstiloSeta ?? "Seta preenchida";
        public string LeaderCor => TipoTexto?.LeaderCor ?? CorTexto;
        public double LeaderEspessura => Math.Max(0.1, TipoTexto?.LeaderEspessura ?? 1.2);
        public double LeaderTamanhoSeta => Math.Max(1.0, TipoTexto?.LeaderTamanhoSeta ?? 10.0);
        public double LeaderArrowLength => Math.Max(1.0, LeaderTamanhoSeta);
        public double LeaderArrowHalfWidth => Math.Max(2.0, LeaderTamanhoSeta * 0.45);
        public bool LeaderArrowVisivel => LeaderVisivel && LeaderEstiloSeta != "Sem seta";
        public bool LeaderArrowFillVisivel => LeaderArrowVisivel && LeaderEstiloSeta == "Seta preenchida";
        public bool LeaderArrowOpenVisivel => LeaderArrowVisivel && LeaderEstiloSeta == "Seta aberta";
        public Brush LeaderBrush => CriarBrush(LeaderCor);
        public Point LeaderPoint => CalcularLeaderPointWorld();
        public Point LeaderCotoveloPoint => CalcularLeaderCotoveloWorld();
        public Point LeaderInicioLocal => CalcularLeaderInicioLocal();
        public Point LeaderFimLocal => WorldToLocal(LeaderPoint);
        public Point LeaderInicioWorld => LocalToWorld(LeaderInicioLocal);
        public PointCollection LeaderPolylineWorldPoints => CalcularLeaderPolylineWorldPoints();
        public PointCollection LeaderArrowWorldPoints => CalcularLeaderArrowWorldPoints();
        public PointCollection LeaderOpenArrowWorldPoints => CalcularLeaderOpenArrowWorldPoints();
        public bool HasPreviewOffset => Math.Abs(_previewOffsetX) > 0.000001 || Math.Abs(_previewOffsetY) > 0.000001;
        public bool HasPreviewBoxWidth => _hasPreviewBoxWidth;
        public bool HasPreviewRotation => _hasPreviewRotation;
        public bool HasPreviewLeaderPoint => _hasPreviewLeaderPoint;
        public bool HasPreviewLeaderCotoveloPoint => _hasPreviewLeaderCotoveloPoint;
        public bool IsNotEditingInline => !IsEditingInline;
        public Brush SelectionBorderBrush => IsSelected ? Brushes.DodgerBlue : Brushes.Transparent;
        public Thickness SelectionBorderThickness => IsSelected ? new Thickness(DefaultSelectionThickness) : new Thickness(0.0);

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectionBorderBrush));
                OnPropertyChanged(nameof(SelectionBorderThickness));
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
                OnPropertyChanged(nameof(AlturaVisual));
                OnPropertyChanged(nameof(SelectionBorderBrush));
                OnPropertyChanged(nameof(SelectionBorderThickness));
                NotificarLeader();
            }
        }

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

        public double LineHeight => Math.Max(AlturaTexto * 1.25, AlturaTexto + 1.0);
        public double AlturaEstimada => CalcularAlturaEstimada(Conteudo, LarguraCaixa, AlturaTexto, LineHeight);
        public double AlturaEdicao => CalcularAlturaEstimada(ConteudoEdicao, LarguraCaixa, AlturaTexto, LineHeight);
        public double AlturaVisual => IsEditingInline ? AlturaEdicao : AlturaEstimada;
        public Transform RenderTransform => CriarRenderTransform();

        public bool Contains(Point position, double tolerance)
        {
            if (!PontoValido(position))
                return false;

            double margem = Math.Max(0.0, tolerance);
            double largura = Math.Max(ProjectSheetTemplateText.MinBoxWidth, LarguraCaixa);
            double altura = Math.Max(AlturaTexto, AlturaVisual);
            Point local = WorldToLocal(position);
            var bounds = new Rect(-margem, -margem, largura + margem * 2.0, altura + margem * 2.0);
            return bounds.Contains(local);
        }

        public void SetPreviewOffset(double deltaX, double deltaY)
        {
            _previewOffsetX = ValorFinito(deltaX) ? deltaX : 0.0;
            _previewOffsetY = ValorFinito(deltaY) ? deltaY : 0.0;
            NotificarPosicao();
        }

        public void ClearPreviewOffset()
        {
            if (!HasPreviewOffset)
                return;

            _previewOffsetX = 0.0;
            _previewOffsetY = 0.0;
            NotificarPosicao();
        }

        public void SetPreviewBoxWidth(double larguraCaixa)
        {
            double larguraNormalizada = NormalizarLargura(larguraCaixa);

            if (_hasPreviewBoxWidth && Math.Abs(_previewBoxWidth - larguraNormalizada) < 0.0001)
                return;

            _hasPreviewBoxWidth = true;
            _previewBoxWidth = larguraNormalizada;
            NotificarLarguraCaixa();
            OnPropertyChanged(nameof(HasPreviewBoxWidth));
        }

        public void ClearPreviewBoxWidth()
        {
            if (!_hasPreviewBoxWidth)
                return;

            _hasPreviewBoxWidth = false;
            _previewBoxWidth = 0.0;
            NotificarLarguraCaixa();
            OnPropertyChanged(nameof(HasPreviewBoxWidth));
        }

        public bool SetPreviewRotation(double rotacao)
        {
            double rotacaoNormalizada = NormalizarRotacao(rotacao);

            if (_hasPreviewRotation && Math.Abs(_previewRotation - rotacaoNormalizada) < 0.0001)
                return true;

            _hasPreviewRotation = true;
            _previewRotation = rotacaoNormalizada;
            OnPropertyChanged(nameof(HasPreviewRotation));
            NotificarRotacao();
            return true;
        }

        public void ClearPreviewRotation()
        {
            if (!_hasPreviewRotation)
                return;

            _hasPreviewRotation = false;
            _previewRotation = 0.0;
            OnPropertyChanged(nameof(HasPreviewRotation));
            NotificarRotacao();
        }

        public bool SetPreviewLeaderPoint(Point point)
        {
            if (!PontoValido(point))
                return false;

            if (_hasPreviewLeaderPoint && DistanciaQuadrada(_previewLeaderPoint, point) < 0.000001)
                return true;

            _hasPreviewLeaderPoint = true;
            _previewLeaderPoint = point;
            OnPropertyChanged(nameof(HasPreviewLeaderPoint));
            NotificarLeader();
            return true;
        }

        public void ClearPreviewLeaderPoint()
        {
            if (!_hasPreviewLeaderPoint)
                return;

            _hasPreviewLeaderPoint = false;
            _previewLeaderPoint = default;
            OnPropertyChanged(nameof(HasPreviewLeaderPoint));
            NotificarLeader();
        }

        public bool SetPreviewLeaderCotoveloPoint(Point point)
        {
            if (!PontoValido(point))
                return false;

            if (_hasPreviewLeaderCotoveloPoint && DistanciaQuadrada(_previewLeaderCotoveloPoint, point) < 0.000001)
                return true;

            _hasPreviewLeaderCotoveloPoint = true;
            _previewLeaderCotoveloPoint = point;
            OnPropertyChanged(nameof(HasPreviewLeaderCotoveloPoint));
            NotificarLeader();
            return true;
        }

        public void ClearPreviewLeaderCotoveloPoint()
        {
            if (!_hasPreviewLeaderCotoveloPoint)
                return;

            _hasPreviewLeaderCotoveloPoint = false;
            _previewLeaderCotoveloPoint = default;
            OnPropertyChanged(nameof(HasPreviewLeaderCotoveloPoint));
            NotificarLeader();
        }

        public void IniciarEdicaoInline()
        {
            ConteudoEdicao = Conteudo;
            IsEditingInline = true;
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

        public void Refresh()
        {
            AtualizarTipoTexto();
            OnPropertyChanged(nameof(Id));
            NotificarPosicao();
            OnPropertyChanged(nameof(ModelX));
            OnPropertyChanged(nameof(ModelY));
            OnPropertyChanged(nameof(Nome));
            OnPropertyChanged(nameof(Conteudo));

            if (!IsEditingInline)
                _conteudoEdicao = Conteudo;

            OnPropertyChanged(nameof(ConteudoEdicao));
            OnPropertyChanged(nameof(LarguraCaixa));
            OnPropertyChanged(nameof(ModelLarguraCaixa));
            OnPropertyChanged(nameof(HasPreviewBoxWidth));
            OnPropertyChanged(nameof(HasPreviewRotation));
            OnPropertyChanged(nameof(HasPreviewLeaderPoint));
            OnPropertyChanged(nameof(HasPreviewLeaderCotoveloPoint));
            OnPropertyChanged(nameof(Rotacao));
            OnPropertyChanged(nameof(ModelRotacao));
            OnPropertyChanged(nameof(Visible));
            OnPropertyChanged(nameof(AlturaEstimada));
            OnPropertyChanged(nameof(AlturaEdicao));
            OnPropertyChanged(nameof(AlturaVisual));
            OnPropertyChanged(nameof(RenderTransform));
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(IsEditingInline));
            OnPropertyChanged(nameof(IsNotEditingInline));
            OnPropertyChanged(nameof(SelectionBorderBrush));
            OnPropertyChanged(nameof(SelectionBorderThickness));
            NotificarVisual();
            NotificarLeader();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void AtualizarTipoTexto()
        {
            _tipoTexto = ResolverTipoTexto();
        }

        private TipoTextoAnotativo? ResolverTipoTexto()
        {
            if (!_texto.PossuiTipoTexto)
                return null;

            return _types.TiposTextosAnotativos.FirstOrDefault(t =>
                string.Equals(t.NomeTipo, _texto.TipoTextoNome, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Familia, _texto.TipoTextoFamilia, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Categoria, _texto.TipoTextoCategoria, StringComparison.OrdinalIgnoreCase)) ?? _types.TipoTextoAnotativoPadrao;
        }

        private void NotificarPosicao()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(HasPreviewOffset));
            NotificarLeader();
        }

        private void NotificarLarguraCaixa()
        {
            OnPropertyChanged(nameof(LarguraCaixa));
            OnPropertyChanged(nameof(AlturaEstimada));
            OnPropertyChanged(nameof(AlturaEdicao));
            OnPropertyChanged(nameof(AlturaVisual));
            OnPropertyChanged(nameof(RenderTransform));
            NotificarLeader();
        }

        private void NotificarRotacao()
        {
            OnPropertyChanged(nameof(Rotacao));
            OnPropertyChanged(nameof(ModelRotacao));
            OnPropertyChanged(nameof(RenderTransform));
            NotificarLeader();
        }

        private void NotificarVisual()
        {
            OnPropertyChanged(nameof(TipoTexto));
            OnPropertyChanged(nameof(CorTexto));
            OnPropertyChanged(nameof(Fonte));
            OnPropertyChanged(nameof(AlturaTexto));
            OnPropertyChanged(nameof(AlinhamentoHorizontal));
            OnPropertyChanged(nameof(ForegroundBrush));
            OnPropertyChanged(nameof(FontFamily));
            OnPropertyChanged(nameof(TextAlignment));
            OnPropertyChanged(nameof(TextBoxHorizontalContentAlignment));
            OnPropertyChanged(nameof(LineHeight));
            OnPropertyChanged(nameof(AlturaEstimada));
            OnPropertyChanged(nameof(AlturaEdicao));
            OnPropertyChanged(nameof(AlturaVisual));
            OnPropertyChanged(nameof(RenderTransform));
            NotificarLeader();
        }

        private void NotificarLeader()
        {
            OnPropertyChanged(nameof(LeaderAtivo));
            OnPropertyChanged(nameof(LeaderComCotovelo));
            OnPropertyChanged(nameof(LeaderVisivel));
            OnPropertyChanged(nameof(LeaderRetoVisivel));
            OnPropertyChanged(nameof(LeaderCotoveloVisivel));
            OnPropertyChanged(nameof(LeaderEstiloSeta));
            OnPropertyChanged(nameof(LeaderCor));
            OnPropertyChanged(nameof(LeaderEspessura));
            OnPropertyChanged(nameof(LeaderTamanhoSeta));
            OnPropertyChanged(nameof(LeaderArrowLength));
            OnPropertyChanged(nameof(LeaderArrowHalfWidth));
            OnPropertyChanged(nameof(LeaderArrowVisivel));
            OnPropertyChanged(nameof(LeaderArrowFillVisivel));
            OnPropertyChanged(nameof(LeaderArrowOpenVisivel));
            OnPropertyChanged(nameof(LeaderBrush));
            OnPropertyChanged(nameof(LeaderPoint));
            OnPropertyChanged(nameof(LeaderCotoveloPoint));
            OnPropertyChanged(nameof(LeaderInicioLocal));
            OnPropertyChanged(nameof(LeaderFimLocal));
            OnPropertyChanged(nameof(LeaderInicioWorld));
            OnPropertyChanged(nameof(LeaderPolylineWorldPoints));
            OnPropertyChanged(nameof(LeaderArrowWorldPoints));
            OnPropertyChanged(nameof(LeaderOpenArrowWorldPoints));
        }

        private Transform CriarRenderTransform()
        {
            double rotacao = Rotacao;

            if (Math.Abs(rotacao) < 0.000001)
                return Transform.Identity;

            return new RotateTransform(rotacao);
        }

        private Point CalcularLeaderInicioLocal()
        {
            double largura = Math.Max(1.0, LarguraCaixa);
            double altura = Math.Max(1.0, AlturaVisual);
            Point centro = new(largura / 2.0, altura / 2.0);
            Point fim = LeaderFimLocal;
            Vector direcao = fim - centro;

            if (direcao.Length < 0.000001)
                return centro;

            double t = double.PositiveInfinity;

            if (direcao.X > 0.000001)
                t = Math.Min(t, (largura - centro.X) / direcao.X);
            else if (direcao.X < -0.000001)
                t = Math.Min(t, (0.0 - centro.X) / direcao.X);

            if (direcao.Y > 0.000001)
                t = Math.Min(t, (altura - centro.Y) / direcao.Y);
            else if (direcao.Y < -0.000001)
                t = Math.Min(t, (0.0 - centro.Y) / direcao.Y);

            if (double.IsNaN(t) || double.IsInfinity(t))
                t = 0.0;

            t = Math.Max(0.0, Math.Min(1.0, t));
            return centro + direcao * t;
        }

        private Point CalcularLeaderPointWorld()
        {
            if (_hasPreviewLeaderPoint)
                return _previewLeaderPoint;

            if (_texto.PossuiLeaderPointValido)
                return new Point(_texto.LeaderX + _previewOffsetX, _texto.LeaderY + _previewOffsetY);

            return LocalToWorld(new Point(Math.Max(1.0, LarguraCaixa) + 70.0, Math.Max(1.0, AlturaVisual) + 50.0));
        }

        private Point CalcularLeaderCotoveloWorld()
        {
            if (_hasPreviewLeaderCotoveloPoint)
                return _previewLeaderCotoveloPoint;

            if (_texto.LeaderCotoveloManual && _texto.PossuiLeaderCotoveloPointValido)
                return new Point(_texto.LeaderCotoveloX + _previewOffsetX, _texto.LeaderCotoveloY + _previewOffsetY);

            return CalcularLeaderCotoveloMedioWorld();
        }

        private Point CalcularLeaderCotoveloMedioWorld()
        {
            Point inicio = LeaderInicioWorld;
            Point fim = LeaderPoint;
            return new Point((inicio.X + fim.X) / 2.0, (inicio.Y + fim.Y) / 2.0);
        }

        private PointCollection CalcularLeaderPolylineWorldPoints()
        {
            return new PointCollection
            {
                LeaderInicioWorld,
                LeaderCotoveloPoint,
                LeaderPoint
            };
        }

        private PointCollection CalcularLeaderArrowWorldPoints()
        {
            Point fim = LeaderPoint;
            Point inicio = LeaderComCotovelo ? LeaderCotoveloPoint : LeaderInicioWorld;
            Vector direcao = inicio - fim;

            if (direcao.Length < 0.000001)
                direcao = new Vector(0.0, -1.0);
            else
                direcao.Normalize();

            Vector normal = new(-direcao.Y, direcao.X);
            Point p1 = fim + direcao * LeaderArrowLength + normal * LeaderArrowHalfWidth;
            Point p2 = fim + direcao * LeaderArrowLength - normal * LeaderArrowHalfWidth;

            return new PointCollection { fim, p1, p2 };
        }

        private PointCollection CalcularLeaderOpenArrowWorldPoints()
        {
            PointCollection pontos = CalcularLeaderArrowWorldPoints();

            if (pontos.Count < 3)
                return pontos;

            return new PointCollection { pontos[1], pontos[0], pontos[2] };
        }

        private Point WorldToLocal(Point world)
        {
            double largura = Math.Max(1.0, LarguraCaixa);
            double altura = Math.Max(1.0, AlturaVisual);
            Point centroWorld = new(X + largura / 2.0, Y + altura / 2.0);
            double radians = -Rotacao * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double dx = world.X - centroWorld.X;
            double dy = world.Y - centroWorld.Y;
            return new Point(largura / 2.0 + dx * cos - dy * sin, altura / 2.0 + dx * sin + dy * cos);
        }

        private Point LocalToWorld(Point local)
        {
            double largura = Math.Max(1.0, LarguraCaixa);
            double altura = Math.Max(1.0, AlturaVisual);
            Point centroWorld = new(X + largura / 2.0, Y + altura / 2.0);
            double radians = Rotacao * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double dx = local.X - largura / 2.0;
            double dy = local.Y - altura / 2.0;
            return new Point(centroWorld.X + dx * cos - dy * sin, centroWorld.Y + dx * sin + dy * cos);
        }

        private static double CalcularAlturaEstimada(string? texto, double larguraCaixa, double alturaTexto, double lineHeight)
        {
            int linhas = ContarLinhasRenderizadas(texto, larguraCaixa, alturaTexto);
            double alturaLinha = Math.Max(lineHeight, alturaTexto);
            return Math.Max(alturaTexto, linhas * alturaLinha + 4.0);
        }

        private static int ContarLinhasRenderizadas(string? texto, double larguraCaixa, double alturaTexto)
        {
            string[] linhasManuais = ObterLinhasManuais(texto);
            int caracteresPorLinha = CalcularCaracteresPorLinha(larguraCaixa, alturaTexto);
            int total = 0;

            foreach (string linha in linhasManuais)
                total += Math.Max(1, ContarQuebrasLinha(linha, caracteresPorLinha));

            return Math.Max(1, total);
        }

        private static int ContarQuebrasLinha(string linha, int caracteresPorLinha)
        {
            string texto = linha ?? string.Empty;

            if (texto.Length == 0)
                return 1;

            if (texto.Length <= caracteresPorLinha)
                return 1;

            int total = 0;
            int inicio = 0;

            while (inicio < texto.Length)
            {
                int restante = texto.Length - inicio;

                if (restante <= caracteresPorLinha)
                {
                    total++;
                    break;
                }

                int limite = inicio + caracteresPorLinha;
                int quebra = texto.LastIndexOf(' ', limite, caracteresPorLinha);

                if (quebra <= inicio)
                    quebra = limite;

                total++;
                inicio = quebra;

                while (inicio < texto.Length && texto[inicio] == ' ')
                    inicio++;
            }

            return Math.Max(1, total);
        }

        private static int CalcularCaracteresPorLinha(double larguraCaixa, double alturaTexto)
        {
            double larguraUtil = Math.Max(1.0, NormalizarLargura(larguraCaixa) - TextHorizontalMargin);
            double altura = NormalizarAltura(alturaTexto);
            double larguraMedia = Math.Max(1.0, altura * 0.58);
            return Math.Max(1, (int)Math.Floor(larguraUtil / larguraMedia));
        }

        private static string[] ObterLinhasManuais(string? texto)
        {
            return (texto ?? string.Empty)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n')
                .Split('\n');
        }

        private static string NormalizarCor(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? ProjectSheetTemplateText.DefaultTextColor : valor.Trim();
        }

        private static string NormalizarFonte(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? ProjectSheetTemplateText.DefaultFont : valor.Trim();
        }

        private static string NormalizarAlinhamento(string? valor)
        {
            return valor switch
            {
                "Centro" => "Centro",
                "Direita" => "Direita",
                _ => "Esquerda"
            };
        }

        private static double NormalizarLargura(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheetTemplateText.MinBoxWidth
                ? ProjectSheetTemplateText.DefaultBoxWidth
                : valor;
        }

        private static double NormalizarAltura(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheetTemplateText.MinTextHeight
                ? ProjectSheetTemplateText.DefaultTextHeight
                : valor;
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
                object? valor = ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(cor) ? ProjectSheetTemplateText.DefaultTextColor : cor);

                if (valor is Color color)
                    return new SolidColorBrush(color);
            }
            catch (FormatException)
            {
            }

            return Brushes.Black;
        }

        private static bool ValorFinito(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool PontoValido(Point point)
        {
            return ValorFinito(point.X) && ValorFinito(point.Y);
        }

        private static double DistanciaQuadrada(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}