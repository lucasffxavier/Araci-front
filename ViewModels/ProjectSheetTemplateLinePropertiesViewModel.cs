using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;
using Araci.Models.Tipos;
using Araci.Services.Catalog;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateLinePropertiesViewModel : INotifyPropertyChanged
    {
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateLine _linha;
        private readonly MoverLinhaDoTipoPranchaUseCase _editarLinha;
        private readonly TypeLibraryService _types;

        public ProjectSheetTemplateLinePropertiesViewModel(
            ProjectSheetType tipo,
            ProjectSheetTemplateLine linha,
            MoverLinhaDoTipoPranchaUseCase editarLinha,
            TypeLibraryService types)
        {
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
            _editarLinha = editarLinha ?? throw new ArgumentNullException(nameof(editarLinha));
            _types = types ?? throw new ArgumentNullException(nameof(types));

            foreach (TipoLinhaAnotativa tipoLinha in _types.TiposLinhasAnotativas)
                tipoLinha.PropertyChanged += OnTipoLinhaPropertyChanged;
        }

        public Guid Id => _linha.Id;
        public string Titulo => "Linha do Tipo de Prancha";
        public IReadOnlyList<TipoLinhaAnotativa> TiposDisponiveis => _types.TiposLinhasAnotativas;
        public IReadOnlyList<string> EstilosLinhaDisponiveis { get; } = new[] { "Contínuo", "Tracejado", "Traço ponto", "Traço dois pontos" };

        public TipoLinhaAnotativa? TipoLinha
        {
            get => ResolverTipoLinha();
            set
            {
                if (value == null)
                    return;

                if (_editarLinha.AlterarTipoGrafico(_tipo.Id, _linha.Id, value))
                    Refresh();
            }
        }

        public double X1
        {
            get => _linha.X1;
            set
            {
                if (_editarLinha.AlterarCoordenadas(_tipo.Id, _linha.Id, value, _linha.Y1, _linha.X2, _linha.Y2))
                    OnCoordinatePropertiesChanged();
            }
        }

        public double Y1
        {
            get => _linha.Y1;
            set
            {
                if (_editarLinha.AlterarCoordenadas(_tipo.Id, _linha.Id, _linha.X1, value, _linha.X2, _linha.Y2))
                    OnCoordinatePropertiesChanged();
            }
        }

        public double X2
        {
            get => _linha.X2;
            set
            {
                if (_editarLinha.AlterarCoordenadas(_tipo.Id, _linha.Id, _linha.X1, _linha.Y1, value, _linha.Y2))
                    OnCoordinatePropertiesChanged();
            }
        }

        public double Y2
        {
            get => _linha.Y2;
            set
            {
                if (_editarLinha.AlterarCoordenadas(_tipo.Id, _linha.Id, _linha.X1, _linha.Y1, _linha.X2, value))
                    OnCoordinatePropertiesChanged();
            }
        }

        public string CorLinha
        {
            get => TipoLinha?.CorLinha ?? _linha.Stroke;
            set
            {
                TipoLinhaAnotativa? tipoLinha = TipoLinha;

                bool alterou = tipoLinha != null
                    ? _editarLinha.AlterarCorTipo(tipoLinha, value)
                    : _editarLinha.AlterarStroke(_tipo.Id, _linha.Id, value);

                if (alterou)
                    OnStylePropertiesChanged();
            }
        }

        public double EspessuraLinha
        {
            get => TipoLinha?.EspessuraLinha ?? _linha.StrokeThickness;
            set
            {
                TipoLinhaAnotativa? tipoLinha = TipoLinha;

                bool alterou = tipoLinha != null
                    ? _editarLinha.AlterarEspessuraTipo(tipoLinha, value)
                    : _editarLinha.AlterarEspessura(_tipo.Id, _linha.Id, value);

                if (alterou)
                    OnStylePropertiesChanged();
            }
        }

        public string EstiloLinha
        {
            get => TipoLinha?.EstiloLinha ?? "Contínuo";
            set
            {
                TipoLinhaAnotativa? tipoLinha = TipoLinha;

                if (tipoLinha == null)
                    return;

                if (_editarLinha.AlterarEstiloTipo(tipoLinha, value))
                    OnStylePropertiesChanged();
            }
        }

        public bool AindaExiste => _tipo.Linhas.Any(l => l.Id == _linha.Id);

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Refresh()
        {
            OnCoordinatePropertiesChanged();
            OnPropertyChanged(nameof(TipoLinha));
            OnStylePropertiesChanged();
            OnPropertyChanged(nameof(AindaExiste));
        }

        private TipoLinhaAnotativa? ResolverTipoLinha()
        {
            if (_linha.PossuiTipoLinha)
            {
                TipoLinhaAnotativa? tipo = _types.TiposLinhasAnotativas.FirstOrDefault(t =>
                    string.Equals(t.NomeTipo, _linha.TipoLinhaNome, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Familia, _linha.TipoLinhaFamilia, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Categoria, _linha.TipoLinhaCategoria, StringComparison.OrdinalIgnoreCase));

                if (tipo != null)
                    return tipo;
            }

            return _types.TipoLinhaAnotativaPadrao;
        }

        private void OnTipoLinhaPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            TipoLinhaAnotativa? tipoLinha = TipoLinha;

            if (tipoLinha == null || !ReferenceEquals(sender, tipoLinha))
                return;

            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.CorLinha) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.EspessuraLinha) ||
                e.PropertyName == nameof(TipoLinhaAnotativa.EstiloLinha))
            {
                OnStylePropertiesChanged();
            }
        }

        private void OnCoordinatePropertiesChanged()
        {
            OnPropertyChanged(nameof(X1));
            OnPropertyChanged(nameof(Y1));
            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }

        private void OnStylePropertiesChanged()
        {
            OnPropertyChanged(nameof(CorLinha));
            OnPropertyChanged(nameof(EspessuraLinha));
            OnPropertyChanged(nameof(EstiloLinha));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}