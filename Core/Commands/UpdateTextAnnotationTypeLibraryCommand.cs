using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Araci.Models.Tipos;

namespace Araci.Core.Commands
{
    public sealed class UpdateTextAnnotationTypeLibraryCommand : IUndoableCommand
    {
        private readonly ObservableCollection<TipoTextoAnotativo> _tipos;
        private readonly List<TextAnnotationTypeBeforeState> _antes;
        private readonly List<TextAnnotationTypeAfterState> _depois;
        private readonly TipoTextoAnotativo? _tipoSelecionadoAntes;
        private readonly int _indiceSelecionadoDepois;
        private readonly Action<TipoTextoAnotativo>? _selecionarTipo;
        private readonly Action? _tiposAlterados;

        public UpdateTextAnnotationTypeLibraryCommand(
            ObservableCollection<TipoTextoAnotativo> tipos,
            IEnumerable<TipoTextoAnotativo> tiposAntes,
            IEnumerable<UpdateTextAnnotationTypeChange> alteracoesDepois,
            TipoTextoAnotativo? tipoSelecionadoAntes,
            int indiceSelecionadoDepois,
            Action<TipoTextoAnotativo>? selecionarTipo,
            Action? tiposAlterados)
        {
            _tipos = tipos ?? throw new ArgumentNullException(nameof(tipos));
            _antes = (tiposAntes ?? throw new ArgumentNullException(nameof(tiposAntes)))
                .Where(t => t != null)
                .Select(t => new TextAnnotationTypeBeforeState(t, ClonarTipo(t)))
                .ToList();
            _depois = (alteracoesDepois ?? throw new ArgumentNullException(nameof(alteracoesDepois)))
                .Where(t => t != null)
                .Select(t => new TextAnnotationTypeAfterState(t.TipoReal, ClonarTipo(t.EstadoDepois)))
                .ToList();
            _tipoSelecionadoAntes = tipoSelecionadoAntes;
            _indiceSelecionadoDepois = indiceSelecionadoDepois;
            _selecionarTipo = selecionarTipo;
            _tiposAlterados = tiposAlterados;
        }

        public void Execute()
        {
            AplicarDepois();
        }

        public void Undo()
        {
            AplicarAntes();
        }

        public void Redo()
        {
            AplicarDepois();
        }

        private void AplicarDepois()
        {
            _tipos.Clear();
            TipoTextoAnotativo? selecionado = null;

            for (int i = 0; i < _depois.Count; i++)
            {
                TextAnnotationTypeAfterState estado = _depois[i];
                TipoTextoAnotativo tipo = estado.ObterOuCriarTipoReal();

                CopiarValores(estado.EstadoDepois, tipo);
                _tipos.Add(tipo);

                if (i == _indiceSelecionadoDepois)
                    selecionado = tipo;
            }

            AtualizarColecao();

            if (selecionado != null)
                _selecionarTipo?.Invoke(selecionado);

            _tiposAlterados?.Invoke();
        }

        private void AplicarAntes()
        {
            _tipos.Clear();

            foreach (TextAnnotationTypeBeforeState estado in _antes)
            {
                CopiarValores(estado.EstadoAntes, estado.TipoReal);
                _tipos.Add(estado.TipoReal);
            }

            AtualizarColecao();

            if (_tipoSelecionadoAntes != null)
                _selecionarTipo?.Invoke(_tipoSelecionadoAntes);

            _tiposAlterados?.Invoke();
        }

        private void AtualizarColecao()
        {
            CollectionViewSource.GetDefaultView(_tipos)?.Refresh();
        }

        private static TipoTextoAnotativo ClonarTipo(TipoTextoAnotativo origem)
        {
            return new TipoTextoAnotativo
            {
                NomeTipo = origem.NomeTipo,
                Familia = origem.Familia,
                Categoria = origem.Categoria,
                CorTexto = origem.CorTexto,
                Fonte = origem.Fonte,
                AlturaTexto = origem.AlturaTexto,
                AlinhamentoHorizontal = origem.AlinhamentoHorizontal
            };
        }

        private static void CopiarValores(TipoTextoAnotativo origem, TipoTextoAnotativo destino)
        {
            destino.NomeTipo = origem.NomeTipo;
            destino.Familia = origem.Familia;
            destino.Categoria = origem.Categoria;
            destino.CorTexto = origem.CorTexto;
            destino.Fonte = origem.Fonte;
            destino.AlturaTexto = origem.AlturaTexto;
            destino.AlinhamentoHorizontal = origem.AlinhamentoHorizontal;
        }

        private sealed class TextAnnotationTypeBeforeState
        {
            public TextAnnotationTypeBeforeState(TipoTextoAnotativo tipoReal, TipoTextoAnotativo estadoAntes)
            {
                TipoReal = tipoReal;
                EstadoAntes = estadoAntes;
            }

            public TipoTextoAnotativo TipoReal { get; }
            public TipoTextoAnotativo EstadoAntes { get; }
        }

        private sealed class TextAnnotationTypeAfterState
        {
            private TipoTextoAnotativo? _tipoCriado;

            public TextAnnotationTypeAfterState(TipoTextoAnotativo? tipoReal, TipoTextoAnotativo estadoDepois)
            {
                TipoReal = tipoReal;
                EstadoDepois = estadoDepois;
            }

            public TipoTextoAnotativo? TipoReal { get; }
            public TipoTextoAnotativo EstadoDepois { get; }

            public TipoTextoAnotativo ObterOuCriarTipoReal()
            {
                if (TipoReal != null)
                    return TipoReal;

                _tipoCriado ??= ClonarTipo(EstadoDepois);
                return _tipoCriado;
            }
        }
    }

    public sealed class UpdateTextAnnotationTypeChange
    {
        public UpdateTextAnnotationTypeChange(TipoTextoAnotativo? tipoReal, TipoTextoAnotativo estadoDepois)
        {
            TipoReal = tipoReal;
            EstadoDepois = estadoDepois ?? throw new ArgumentNullException(nameof(estadoDepois));
        }

        public TipoTextoAnotativo? TipoReal { get; }
        public TipoTextoAnotativo EstadoDepois { get; }
    }
}
