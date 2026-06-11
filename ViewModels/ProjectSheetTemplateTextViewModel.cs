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
        private readonly ProjectSheetTemplateText _texto;
        private readonly TypeLibraryService _types;
        private TipoTextoAnotativo? _tipoTexto;

        public ProjectSheetTemplateTextViewModel(ProjectSheetTemplateText texto)
            : this(texto, new TypeLibraryService())
        {
        }

        public ProjectSheetTemplateTextViewModel(ProjectSheetTemplateText texto, TypeLibraryService types)
        {
            _texto = texto ?? throw new ArgumentNullException(nameof(texto));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            AtualizarTipoTexto();
        }

        public Guid Id => _texto.Id;
        public double X => _texto.X;
        public double Y => _texto.Y;
        public double ModelX => _texto.X;
        public double ModelY => _texto.Y;
        public string Nome => _texto.Nome;
        public string Conteudo => _texto.Texto;
        public double LarguraCaixa => _texto.LarguraCaixa;
        public double Rotacao => NormalizarRotacao(_texto.Rotacao);
        public TipoTextoAnotativo? TipoTexto => _tipoTexto;
        public string CorTexto => TipoTexto?.CorTexto ?? NormalizarCor(_texto.CorTexto);
        public string Fonte => TipoTexto?.Fonte ?? NormalizarFonte(_texto.Fonte);
        public double AlturaTexto => NormalizarAltura(TipoTexto?.AlturaTexto ?? _texto.AlturaTexto);
        public string AlinhamentoHorizontal => TipoTexto?.AlinhamentoHorizontal ?? NormalizarAlinhamento(_texto.AlinhamentoHorizontal);
        public bool Visible => _texto.Visible;
        public Brush ForegroundBrush => CriarBrush(CorTexto);
        public FontFamily FontFamily => new(string.IsNullOrWhiteSpace(Fonte) ? ProjectSheetTemplateText.DefaultFont : Fonte);

        public TextAlignment TextAlignment => AlinhamentoHorizontal switch
        {
            "Centro" => TextAlignment.Center,
            "Direita" => TextAlignment.Right,
            _ => TextAlignment.Left
        };

        public double LineHeight => Math.Max(AlturaTexto * 1.25, AlturaTexto + 1.0);

        public void Refresh()
        {
            AtualizarTipoTexto();
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(ModelX));
            OnPropertyChanged(nameof(ModelY));
            OnPropertyChanged(nameof(Nome));
            OnPropertyChanged(nameof(Conteudo));
            OnPropertyChanged(nameof(LarguraCaixa));
            OnPropertyChanged(nameof(Rotacao));
            OnPropertyChanged(nameof(Visible));
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
            OnPropertyChanged(nameof(LineHeight));
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}