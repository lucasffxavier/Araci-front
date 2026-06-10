using System;
using System.Collections.Generic;
using Araci.Core.Commands;
using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoLinhaAnotativaViewModel : TipoElementoViewModel
    {
        private readonly Action? _tipoAlterado;
        private readonly string _nomeOriginal;
        private readonly string _familiaOriginal;
        private readonly string _categoriaOriginal;
        private readonly string _estiloLinhaOriginal;

        public TipoLinhaAnotativaViewModel(TipoLinhaAnotativa tipo)
            : this(tipo, null)
        {
        }

        public TipoLinhaAnotativaViewModel(TipoLinhaAnotativa tipo, Action? tipoAlterado)
            : base(tipo)
        {
            _tipoAlterado = tipoAlterado;
            _nomeOriginal = tipo.NomeTipo;
            _familiaOriginal = tipo.Familia;
            _categoriaOriginal = tipo.Categoria;
            _estiloLinhaOriginal = tipo.EstiloLinha;
        }

        protected TipoLinhaAnotativa TipoLinha => (TipoLinhaAnotativa)_tipo;

        public IReadOnlyList<string> EstilosLinhaDisponiveis { get; } = new[] { "Contínuo", "Tracejado", "Traço ponto", "Traço dois pontos" };

        public string EstiloLinha
        {
            get => TipoLinha.EstiloLinha;
            set
            {
                if (TipoLinha.EstiloLinha == value)
                    return;

                TipoLinha.EstiloLinha = value;
                OnPropertyChanged();
            }
        }

        public IUndoableCommand? CreateCommitCommand(Action? tipoAlterado)
        {
            var composite = new CompositeCommand();

            if (!string.Equals(_nomeOriginal, TipoLinha.NomeTipo, StringComparison.Ordinal))
            {
                composite.Add(new UpdateLineAnnotationTypePropertyCommand<string>(
                    TipoLinha,
                    (t, value) => t.NomeTipo = value,
                    _nomeOriginal,
                    TipoLinha.NomeTipo));
            }

            if (!string.Equals(_familiaOriginal, TipoLinha.Familia, StringComparison.Ordinal))
            {
                composite.Add(new UpdateLineAnnotationTypePropertyCommand<string>(
                    TipoLinha,
                    (t, value) => t.Familia = value,
                    _familiaOriginal,
                    TipoLinha.Familia));
            }

            if (!string.Equals(_categoriaOriginal, TipoLinha.Categoria, StringComparison.Ordinal))
            {
                composite.Add(new UpdateLineAnnotationTypePropertyCommand<string>(
                    TipoLinha,
                    (t, value) => t.Categoria = value,
                    _categoriaOriginal,
                    TipoLinha.Categoria));
            }

            if (!string.Equals(_estiloLinhaOriginal, TipoLinha.EstiloLinha, StringComparison.Ordinal))
            {
                composite.Add(new UpdateLineAnnotationTypePropertyCommand<string>(
                    TipoLinha,
                    (t, value) => t.EstiloLinha = value,
                    _estiloLinhaOriginal,
                    TipoLinha.EstiloLinha));
            }

            if (composite.IsEmpty)
                return null;

            return new CommitTipoLinhaAnotativaCommand(composite, tipoAlterado);
        }

        public override void CommitChanges()
        {
            _tipoAlterado?.Invoke();
        }

        public override void CancelChanges()
        {
            TipoLinha.NomeTipo = _nomeOriginal;
            TipoLinha.Familia = _familiaOriginal;
            TipoLinha.Categoria = _categoriaOriginal;
            TipoLinha.EstiloLinha = _estiloLinhaOriginal;
            NotificarTudo();
        }

        private void NotificarTudo()
        {
            OnPropertyChanged(nameof(NomeTipo));
            OnPropertyChanged(nameof(Familia));
            OnPropertyChanged(nameof(Categoria));
            OnPropertyChanged(nameof(EstiloLinha));
        }

        private sealed class CommitTipoLinhaAnotativaCommand : IUndoableCommand
        {
            private readonly IUndoableCommand _inner;
            private readonly Action? _afterChanged;

            public CommitTipoLinhaAnotativaCommand(IUndoableCommand inner, Action? afterChanged)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _afterChanged = afterChanged;
            }

            public void Execute()
            {
                _inner.Execute();
                _afterChanged?.Invoke();
            }

            public void Undo()
            {
                _inner.Undo();
                _afterChanged?.Invoke();
            }

            public void Redo()
            {
                _inner.Redo();
                _afterChanged?.Invoke();
            }
        }
    }
}