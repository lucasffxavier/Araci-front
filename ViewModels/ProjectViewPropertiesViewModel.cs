using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectViewPropertiesViewModel : INotifyPropertyChanged
    {
        private readonly AraciDocument _document;
        private readonly ProjectView _vista;
        private readonly RenomearItemProjetoUseCase _renomearItemProjeto;
        private readonly EditarPropriedadesVistaUseCase _editarPropriedadesVista;

        public ProjectViewPropertiesViewModel(
            AraciDocument document,
            ProjectView vista,
            RenomearItemProjetoUseCase renomearItemProjeto,
            EditarPropriedadesVistaUseCase editarPropriedadesVista,
            bool exibirParametrosRecorte = false)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _vista = vista ?? throw new ArgumentNullException(nameof(vista));
            _renomearItemProjeto = renomearItemProjeto ?? throw new ArgumentNullException(nameof(renomearItemProjeto));
            _editarPropriedadesVista = editarPropriedadesVista ?? throw new ArgumentNullException(nameof(editarPropriedadesVista));
            _ = exibirParametrosRecorte;
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            _document.PropriedadesVistaAlteradas += OnPropriedadesVistaAlteradas;
        }

        public string Titulo => "Vista";

        public IReadOnlyList<ProjectViewDiscipline> Disciplinas { get; } =
            new[]
            {
                ProjectViewDiscipline.Coordenacao,
                ProjectViewDiscipline.Eletrica,
                ProjectViewDiscipline.Solar,
                ProjectViewDiscipline.Eolica,
                ProjectViewDiscipline.Distribuicao,
                ProjectViewDiscipline.Subestacao
            };

        public string Nome
        {
            get => _vista.Nome;
            set
            {
                string anterior = _vista.Nome;
                bool renomeado = _renomearItemProjeto.RenomearVista(_vista.Id, value);

                if (!renomeado || anterior != _vista.Nome)
                    OnPropertyChanged();
            }
        }

        public string Escala
        {
            get => _vista.Escala;
            set
            {
                if (_editarPropriedadesVista.AlterarEscala(_vista.Id, value))
                    OnPropertyChanged();
            }
        }

        public ProjectViewDiscipline Disciplina
        {
            get => _vista.Disciplina;
            set
            {
                if (_editarPropriedadesVista.AlterarDisciplina(_vista.Id, value))
                    OnPropertyChanged();
            }
        }

        public bool RecortarVista
        {
            get => _vista.RecortarVista;
            set
            {
                if (_editarPropriedadesVista.AlterarRecortarVista(_vista.Id, value))
                    OnPropertyChanged();
            }
        }

        public bool RegiaoRecorteVisivel
        {
            get => _vista.RegiaoRecorteVisivel;
            set
            {
                if (_editarPropriedadesVista.AlterarRegiaoRecorteVisivel(_vista.Id, value))
                    OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnItemProjetoRenomeado()
        {
            OnPropertyChanged(nameof(Nome));
        }

        private void OnPropriedadesVistaAlteradas(ProjectView vista)
        {
            if (vista.Id != _vista.Id)
                return;

            OnPropertyChanged(nameof(Escala));
            OnPropertyChanged(nameof(Disciplina));
            OnPropertyChanged(nameof(RecortarVista));
            OnPropertyChanged(nameof(RegiaoRecorteVisivel));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class ProjectViewCropRegionPropertiesViewModel : INotifyPropertyChanged
    {
        private readonly AraciDocument _document;
        private readonly ProjectView _vista;
        private readonly EditarPropriedadesVistaUseCase _editarPropriedadesVista;

        public ProjectViewCropRegionPropertiesViewModel(
            AraciDocument document,
            ProjectView vista,
            EditarPropriedadesVistaUseCase editarPropriedadesVista)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _vista = vista ?? throw new ArgumentNullException(nameof(vista));
            _editarPropriedadesVista = editarPropriedadesVista ?? throw new ArgumentNullException(nameof(editarPropriedadesVista));
            _document.ItemProjetoRenomeado += OnItemProjetoRenomeado;
            _document.PropriedadesVistaAlteradas += OnPropriedadesVistaAlteradas;
        }

        public string Titulo => "Região de recorte";
        public string VistaNome => _vista.Nome;

        public string LarguraTexto
        {
            get => FormatarNumero(_vista.RecorteLargura);
            set
            {
                if (!TentarConverterNumero(value, out double valor))
                {
                    OnPropertyChanged();
                    return;
                }

                if (_editarPropriedadesVista.AlterarRecorteLargura(_vista.Id, valor))
                    OnPropertyChanged();
            }
        }

        public string AlturaTexto
        {
            get => FormatarNumero(_vista.RecorteAltura);
            set
            {
                if (!TentarConverterNumero(value, out double valor))
                {
                    OnPropertyChanged();
                    return;
                }

                if (_editarPropriedadesVista.AlterarRecorteAltura(_vista.Id, valor))
                    OnPropertyChanged();
            }
        }

        public bool RecortarVista
        {
            get => _vista.RecortarVista;
            set
            {
                if (_editarPropriedadesVista.AlterarRecortarVista(_vista.Id, value))
                    OnPropertyChanged();
            }
        }

        public bool RegiaoRecorteVisivel
        {
            get => _vista.RegiaoRecorteVisivel;
            set
            {
                if (_editarPropriedadesVista.AlterarRegiaoRecorteVisivel(_vista.Id, value))
                    OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnItemProjetoRenomeado()
        {
            OnPropertyChanged(nameof(VistaNome));
        }

        private void OnPropriedadesVistaAlteradas(ProjectView vista)
        {
            if (vista.Id != _vista.Id)
                return;

            OnPropertyChanged(nameof(VistaNome));
            OnPropertyChanged(nameof(LarguraTexto));
            OnPropertyChanged(nameof(AlturaTexto));
            OnPropertyChanged(nameof(RecortarVista));
            OnPropertyChanged(nameof(RegiaoRecorteVisivel));
        }

        private static string FormatarNumero(double valor)
        {
            return valor.ToString("0.###", CultureInfo.CurrentCulture);
        }

        private static bool TentarConverterNumero(string? texto, out double valor)
        {
            if (double.TryParse(texto, NumberStyles.Float, CultureInfo.CurrentCulture, out valor))
                return true;

            return double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out valor);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
