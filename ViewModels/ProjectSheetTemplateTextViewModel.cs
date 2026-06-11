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
        private bool _isSelected;
        private bool _isEditingInline;
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
            }
        }

        public double LarguraCaixa => _hasPreviewBoxWidth ? _previewBoxWidth : _texto.LarguraCaixa;
        public double ModelLarguraCaixa => _texto.LarguraCaixa;
        public double Rotacao => NormalizarRotacao(_texto.Rotacao);
        public TipoTextoAnotativo? TipoTexto => _tipoTexto;
        public string CorTexto => NormalizarCor(_texto.CorTexto);
        public string Fonte => NormalizarFonte(_texto.Fonte);
        public double AlturaTexto => NormalizarAltura(_texto.AlturaTexto);
        public string AlinhamentoHorizontal => NormalizarAlinhamento(_texto.AlinhamentoHorizontal);
        public bool Visible => _texto.Visible;
        public Brush ForegroundBrush => CriarBrush(CorTexto);
        public FontFamily FontFamily => new(string.IsNullOrWhiteSpace(Fonte) ? ProjectSheetTemplateText.DefaultFont : Fonte);
        public bool HasPreviewOffset => Math.Abs(_previewOffsetX) > 0.000001 || Math.Abs(_previewOffsetY) > 0.000001;
        public bool HasPreviewBoxWidth => _hasPreviewBoxWidth;
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
            double margem = Math.Max(0.0, tolerance);
            double largura = Math.Max(ProjectSheetTemplateText.MinBoxWidth, LarguraCaixa);
            double altura = Math.Max(AlturaTexto, AlturaVisual);
            var bounds = new Rect(X - margem, Y - margem, largura + margem * 2.0, altura + margem * 2.0);
            return bounds.Contains(position);
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
            OnPropertyChanged(nameof(Rotacao));
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
        }

        private void NotificarLarguraCaixa()
        {
            OnPropertyChanged(nameof(LarguraCaixa));
            OnPropertyChanged(nameof(AlturaEstimada));
            OnPropertyChanged(nameof(AlturaEdicao));
            OnPropertyChanged(nameof(AlturaVisual));
            OnPropertyChanged(nameof(RenderTransform));
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
        }

        private Transform CriarRenderTransform()
        {
            double rotacao = Rotacao;

            if (Math.Abs(rotacao) < 0.000001)
                return Transform.Identity;

            return new RotateTransform(rotacao);
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}