using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateLinePropertiesViewModel : INotifyPropertyChanged
    {
        private readonly ProjectSheetType _tipo;
        private readonly ProjectSheetTemplateLine _linha;
        private readonly MoverLinhaDoTipoPranchaUseCase _editarLinha;

        public ProjectSheetTemplateLinePropertiesViewModel(
            ProjectSheetType tipo,
            ProjectSheetTemplateLine linha,
            MoverLinhaDoTipoPranchaUseCase editarLinha)
        {
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
            _editarLinha = editarLinha ?? throw new ArgumentNullException(nameof(editarLinha));
        }

        public Guid Id => _linha.Id;
        public string Titulo => "Linha do Tipo de Prancha";

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

        public string Stroke
        {
            get => _linha.Stroke;
            set
            {
                if (_editarLinha.AlterarStroke(_tipo.Id, _linha.Id, value))
                    OnPropertyChanged();
            }
        }

        public double StrokeThickness
        {
            get => _linha.StrokeThickness;
            set
            {
                if (_editarLinha.AlterarEspessura(_tipo.Id, _linha.Id, value))
                    OnPropertyChanged();
            }
        }

        public bool AindaExiste => _tipo.Linhas.Any(l => l.Id == _linha.Id);

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Refresh()
        {
            OnCoordinatePropertiesChanged();
            OnPropertyChanged(nameof(Stroke));
            OnPropertyChanged(nameof(StrokeThickness));
            OnPropertyChanged(nameof(AindaExiste));
        }

        private void OnCoordinatePropertiesChanged()
        {
            OnPropertyChanged(nameof(X1));
            OnPropertyChanged(nameof(Y1));
            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}