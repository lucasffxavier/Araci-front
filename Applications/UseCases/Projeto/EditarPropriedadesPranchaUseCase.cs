using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class EditarPropriedadesPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public EditarPropriedadesPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool AlterarNumero(Guid id, string numero)
        {
            return Alterar(id, p => p.Numero, (p, valor) => p.Numero = NormalizarTexto(valor), NormalizarTexto(numero));
        }

        public bool AlterarTipoPrancha(Guid id, Guid tipoPranchaId)
        {
            ProjectSheet? prancha = ObterPrancha(id);

            if (prancha == null)
                return false;

            if (!_document.TiposPrancha.Any(t => t.Id == tipoPranchaId))
                return false;

            return Alterar(
                id,
                p => p.SheetTypeId,
                (p, valor) => p.SheetTypeId = valor,
                (Guid?)tipoPranchaId);
        }

        public bool AlterarFormato(Guid id, ProjectSheetFormat formato)
        {
            ProjectSheet? prancha = ObterPrancha(id);

            if (prancha == null)
                return false;

            return AlterarEstadoFolha(
                prancha,
                new SheetPageState(prancha.FormatoFolha, prancha.OrientacaoFolha, prancha.LarguraFolha, prancha.AlturaFolha),
                CriarEstadoFormato(formato, prancha.OrientacaoFolha, prancha.LarguraFolha, prancha.AlturaFolha));
        }

        public bool AlterarOrientacao(Guid id, ProjectSheetOrientation orientacao)
        {
            ProjectSheet? prancha = ObterPrancha(id);

            if (prancha == null)
                return false;

            return AlterarEstadoFolha(
                prancha,
                new SheetPageState(prancha.FormatoFolha, prancha.OrientacaoFolha, prancha.LarguraFolha, prancha.AlturaFolha),
                CriarEstadoFormato(prancha.FormatoFolha, orientacao, prancha.LarguraFolha, prancha.AlturaFolha));
        }

        public bool AlterarLargura(Guid id, double largura)
        {
            double normalizada = NormalizarDimensao(largura, ProjectSheet.DefaultWidth);
            ProjectSheet? prancha = ObterPrancha(id);

            if (prancha == null)
                return false;

            return AlterarEstadoFolha(
                prancha,
                new SheetPageState(prancha.FormatoFolha, prancha.OrientacaoFolha, prancha.LarguraFolha, prancha.AlturaFolha),
                new SheetPageState(ProjectSheetFormat.Personalizado, prancha.OrientacaoFolha, normalizada, prancha.AlturaFolha));
        }

        public bool AlterarAltura(Guid id, double altura)
        {
            double normalizada = NormalizarDimensao(altura, ProjectSheet.DefaultHeight);
            ProjectSheet? prancha = ObterPrancha(id);

            if (prancha == null)
                return false;

            return AlterarEstadoFolha(
                prancha,
                new SheetPageState(prancha.FormatoFolha, prancha.OrientacaoFolha, prancha.LarguraFolha, prancha.AlturaFolha),
                new SheetPageState(ProjectSheetFormat.Personalizado, prancha.OrientacaoFolha, prancha.LarguraFolha, normalizada));
        }

        private bool Alterar<T>(Guid id, Func<ProjectSheet, T> obter, Action<ProjectSheet, T> aplicar, T valorNovo)
        {
            ProjectSheet? prancha = ObterPrancha(id);

            if (prancha == null)
                return false;

            T valorAnterior = obter(prancha);

            if (Equals(valorAnterior, valorNovo))
                return true;

            _commands.Execute(new UpdateProjectSheetPropertyCommand<T>(
                _document,
                prancha,
                aplicar,
                valorAnterior,
                valorNovo));

            return true;
        }

        private bool AlterarEstadoFolha(ProjectSheet prancha, SheetPageState anterior, SheetPageState novo)
        {
            if (anterior.Equals(novo))
                return true;

            _commands.Execute(new UpdateProjectSheetPropertyCommand<SheetPageState>(
                _document,
                prancha,
                AplicarEstadoFolha,
                anterior,
                novo));

            return true;
        }

        private ProjectSheet? ObterPrancha(Guid id)
        {
            return _document.Pranchas.FirstOrDefault(p => p.Id == id);
        }

        private static SheetPageState CriarEstadoFormato(ProjectSheetFormat formato, ProjectSheetOrientation orientacao, double larguraAtual, double alturaAtual)
        {
            if (formato == ProjectSheetFormat.Personalizado)
                return new SheetPageState(formato, orientacao, larguraAtual, alturaAtual);

            (double largura, double altura) = ProjectSheet.ObterDimensoesFormato(formato, orientacao);
            return new SheetPageState(formato, orientacao, largura, altura);
        }

        private static void AplicarEstadoFolha(ProjectSheet prancha, SheetPageState estado)
        {
            prancha.FormatoFolha = estado.Formato;
            prancha.OrientacaoFolha = estado.Orientacao;
            prancha.LarguraFolha = estado.Largura;
            prancha.AlturaFolha = estado.Altura;
        }

        private static string NormalizarTexto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
        }

        private static double NormalizarDimensao(double valor, double fallback)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheet.MinDimension
                ? fallback
                : valor;
        }

        private readonly record struct SheetPageState(
            ProjectSheetFormat Formato,
            ProjectSheetOrientation Orientacao,
            double Largura,
            double Altura);
    }
}