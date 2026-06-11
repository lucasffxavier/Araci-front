using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class UpdateProjectSheetTypeTextPropertyCommand<T> : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly Guid _textoId;
        private readonly Action<ProjectSheetTemplateText, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateProjectSheetTypeTextPropertyCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            Guid textoId,
            Action<ProjectSheetTemplateText, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _textoId = textoId;
            _aplicar = aplicar ?? throw new ArgumentNullException(nameof(aplicar));
            _valorAnterior = valorAnterior;
            _valorNovo = valorNovo;
        }

        public void Execute()
        {
            Aplicar(_valorNovo);
        }

        public void Undo()
        {
            Aplicar(_valorAnterior);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(T valor)
        {
            ProjectSheetTemplateText? texto = _tipo.Textos.FirstOrDefault(t => t.Id == _textoId);

            if (texto == null)
                return;

            _aplicar(texto, valor);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }
    }

    public readonly struct ProjectSheetTemplateTextPositionState
    {
        public ProjectSheetTemplateTextPositionState(double x, double y)
            : this(x, y, 0.0, 0.0, 0.0, 0.0, false, false)
        {
        }

        public ProjectSheetTemplateTextPositionState(
            double x,
            double y,
            double leaderX,
            double leaderY,
            double leaderCotoveloX,
            double leaderCotoveloY,
            bool leaderCotoveloManual,
            bool aplicarLeader)
        {
            X = x;
            Y = y;
            LeaderX = leaderX;
            LeaderY = leaderY;
            LeaderCotoveloX = leaderCotoveloX;
            LeaderCotoveloY = leaderCotoveloY;
            LeaderCotoveloManual = leaderCotoveloManual;
            AplicarLeader = aplicarLeader;
        }

        public double X { get; }
        public double Y { get; }
        public double LeaderX { get; }
        public double LeaderY { get; }
        public double LeaderCotoveloX { get; }
        public double LeaderCotoveloY { get; }
        public bool LeaderCotoveloManual { get; }
        public bool AplicarLeader { get; }

        public void Aplicar(ProjectSheetTemplateText texto)
        {
            texto.X = X;
            texto.Y = Y;

            if (!AplicarLeader)
                return;

            texto.LeaderX = LeaderX;
            texto.LeaderY = LeaderY;
            texto.LeaderCotoveloX = LeaderCotoveloX;
            texto.LeaderCotoveloY = LeaderCotoveloY;
            texto.LeaderCotoveloManual = LeaderCotoveloManual;
        }

        public static ProjectSheetTemplateTextPositionState FromText(ProjectSheetTemplateText texto)
        {
            return new ProjectSheetTemplateTextPositionState(
                texto.X,
                texto.Y,
                texto.LeaderX,
                texto.LeaderY,
                texto.LeaderCotoveloX,
                texto.LeaderCotoveloY,
                texto.LeaderCotoveloManual,
                true);
        }
    }

    public readonly struct ProjectSheetTemplateTextContentState
    {
        public ProjectSheetTemplateTextContentState(string texto, double larguraCaixa)
        {
            Texto = texto ?? string.Empty;
            LarguraCaixa = larguraCaixa;
        }

        public string Texto { get; }
        public double LarguraCaixa { get; }

        public void Aplicar(ProjectSheetTemplateText templateText)
        {
            templateText.Texto = Texto;
            templateText.LarguraCaixa = LarguraCaixa;
        }

        public static ProjectSheetTemplateTextContentState FromText(ProjectSheetTemplateText texto)
        {
            return new ProjectSheetTemplateTextContentState(texto.Texto, texto.LarguraCaixa);
        }
    }

    public readonly struct ProjectSheetTemplateTextGraphicTypeState
    {
        public ProjectSheetTemplateTextGraphicTypeState(string nomeTipo, string familia, string categoria)
            : this(
                nomeTipo,
                familia,
                categoria,
                ProjectSheetTemplateText.DefaultTextColor,
                ProjectSheetTemplateText.DefaultFont,
                ProjectSheetTemplateText.DefaultTextHeight,
                ProjectSheetTemplateText.DefaultHorizontalAlignment)
        {
        }

        public ProjectSheetTemplateTextGraphicTypeState(
            string nomeTipo,
            string familia,
            string categoria,
            string corTexto,
            string fonte,
            double alturaTexto,
            string alinhamentoHorizontal)
        {
            NomeTipo = nomeTipo ?? string.Empty;
            Familia = familia ?? string.Empty;
            Categoria = categoria ?? string.Empty;
            CorTexto = corTexto ?? string.Empty;
            Fonte = fonte ?? string.Empty;
            AlturaTexto = alturaTexto;
            AlinhamentoHorizontal = alinhamentoHorizontal ?? string.Empty;
        }

        public string NomeTipo { get; }
        public string Familia { get; }
        public string Categoria { get; }
        public string CorTexto { get; }
        public string Fonte { get; }
        public double AlturaTexto { get; }
        public string AlinhamentoHorizontal { get; }

        public void Aplicar(ProjectSheetTemplateText texto)
        {
            texto.DefinirTipoTexto(NomeTipo, Familia, Categoria);
            texto.CorTexto = CorTexto;
            texto.Fonte = Fonte;
            texto.AlturaTexto = AlturaTexto;
            texto.AlinhamentoHorizontal = AlinhamentoHorizontal;
        }

        public static ProjectSheetTemplateTextGraphicTypeState FromText(ProjectSheetTemplateText texto)
        {
            return new ProjectSheetTemplateTextGraphicTypeState(
                texto.TipoTextoNome,
                texto.TipoTextoFamilia,
                texto.TipoTextoCategoria,
                texto.CorTexto,
                texto.Fonte,
                texto.AlturaTexto,
                texto.AlinhamentoHorizontal);
        }
    }

    public readonly struct ProjectSheetTemplateTextStyleState
    {
        public ProjectSheetTemplateTextStyleState(string corTexto, string fonte, double alturaTexto, string alinhamentoHorizontal)
        {
            CorTexto = corTexto ?? string.Empty;
            Fonte = fonte ?? string.Empty;
            AlturaTexto = alturaTexto;
            AlinhamentoHorizontal = alinhamentoHorizontal ?? string.Empty;
        }

        public string CorTexto { get; }
        public string Fonte { get; }
        public double AlturaTexto { get; }
        public string AlinhamentoHorizontal { get; }

        public void Aplicar(ProjectSheetTemplateText texto)
        {
            texto.CorTexto = CorTexto;
            texto.Fonte = Fonte;
            texto.AlturaTexto = AlturaTexto;
            texto.AlinhamentoHorizontal = AlinhamentoHorizontal;
        }

        public static ProjectSheetTemplateTextStyleState FromText(ProjectSheetTemplateText texto)
        {
            return new ProjectSheetTemplateTextStyleState(
                texto.CorTexto,
                texto.Fonte,
                texto.AlturaTexto,
                texto.AlinhamentoHorizontal);
        }
    }

    public readonly struct ProjectSheetTemplateTextLeaderState
    {
        public ProjectSheetTemplateTextLeaderState(
            bool leaderAtivo,
            double leaderX,
            double leaderY,
            bool leaderComCotovelo,
            double leaderCotoveloX,
            double leaderCotoveloY,
            bool leaderCotoveloManual)
        {
            LeaderAtivo = leaderAtivo;
            LeaderX = leaderX;
            LeaderY = leaderY;
            LeaderComCotovelo = leaderComCotovelo;
            LeaderCotoveloX = leaderCotoveloX;
            LeaderCotoveloY = leaderCotoveloY;
            LeaderCotoveloManual = leaderCotoveloManual;
        }

        public bool LeaderAtivo { get; }
        public double LeaderX { get; }
        public double LeaderY { get; }
        public bool LeaderComCotovelo { get; }
        public double LeaderCotoveloX { get; }
        public double LeaderCotoveloY { get; }
        public bool LeaderCotoveloManual { get; }

        public void Aplicar(ProjectSheetTemplateText texto)
        {
            texto.LeaderAtivo = LeaderAtivo;
            texto.LeaderX = LeaderX;
            texto.LeaderY = LeaderY;
            texto.LeaderComCotovelo = LeaderComCotovelo;
            texto.LeaderCotoveloX = LeaderCotoveloX;
            texto.LeaderCotoveloY = LeaderCotoveloY;
            texto.LeaderCotoveloManual = LeaderCotoveloManual;
        }

        public static ProjectSheetTemplateTextLeaderState FromText(ProjectSheetTemplateText texto)
        {
            return new ProjectSheetTemplateTextLeaderState(
                texto.LeaderAtivo,
                texto.LeaderX,
                texto.LeaderY,
                texto.LeaderComCotovelo,
                texto.LeaderCotoveloX,
                texto.LeaderCotoveloY,
                texto.LeaderCotoveloManual);
        }
    }
}