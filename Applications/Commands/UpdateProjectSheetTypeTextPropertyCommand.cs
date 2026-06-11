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
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public void Aplicar(ProjectSheetTemplateText texto)
        {
            texto.X = X;
            texto.Y = Y;
        }

        public static ProjectSheetTemplateTextPositionState FromText(ProjectSheetTemplateText texto)
        {
            return new ProjectSheetTemplateTextPositionState(texto.X, texto.Y);
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
}