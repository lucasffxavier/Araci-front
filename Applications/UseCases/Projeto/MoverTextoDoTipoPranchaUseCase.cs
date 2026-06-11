using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Models.Tipos;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverTextoDoTipoPranchaUseCase
    {
        private const double MinDeltaSquared = 0.0001;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public MoverTextoDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Mover(Guid tipoId, Guid textoId, double deltaX, double deltaY)
        {
            if (!TemDeltaValido(deltaX, deltaY))
                return false;

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            return AlterarPosicao(tipo, texto, texto.X + deltaX, texto.Y + deltaY);
        }

        public bool AlterarPosicao(Guid tipoId, Guid textoId, double x, double y)
        {
            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            return AlterarPosicao(tipo, texto, x, y);
        }

        public bool AlterarNome(Guid tipoId, Guid textoId, string nome)
        {
            string nomeNormalizado = NormalizarNome(nome);

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (string.Equals(texto.Nome, nomeNormalizado, StringComparison.Ordinal))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<string>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.Nome = value,
                texto.Nome,
                nomeNormalizado));

            return true;
        }

        public bool AlterarConteudo(Guid tipoId, Guid textoId, string conteudo)
        {
            string conteudoNormalizado = conteudo ?? string.Empty;

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (string.Equals(texto.Texto, conteudoNormalizado, StringComparison.Ordinal))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<string>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.Texto = value,
                texto.Texto,
                conteudoNormalizado));

            return true;
        }

        public bool AlterarLarguraCaixa(Guid tipoId, Guid textoId, double larguraCaixa)
        {
            if (!LarguraValida(larguraCaixa))
                return false;

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (Math.Abs(texto.LarguraCaixa - larguraCaixa) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<double>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.LarguraCaixa = value,
                texto.LarguraCaixa,
                larguraCaixa));

            return true;
        }

        public bool AlterarRotacao(Guid tipoId, Guid textoId, double rotacao)
        {
            double rotacaoNormalizada = NormalizarRotacao(rotacao);

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            double rotacaoAtual = NormalizarRotacao(texto.Rotacao);

            if (Math.Abs(rotacaoAtual - rotacaoNormalizada) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<double>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.Rotacao = value,
                texto.Rotacao,
                rotacaoNormalizada));

            return true;
        }

        public bool AlterarLeaderAtivo(Guid tipoId, Guid textoId, bool leaderAtivo)
        {
            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (texto.LeaderAtivo == leaderAtivo)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<bool>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.LeaderAtivo = value,
                texto.LeaderAtivo,
                leaderAtivo));

            return true;
        }

        public bool AlterarLeaderComCotovelo(Guid tipoId, Guid textoId, bool leaderComCotovelo)
        {
            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (texto.LeaderComCotovelo == leaderComCotovelo)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<bool>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.LeaderComCotovelo = value,
                texto.LeaderComCotovelo,
                leaderComCotovelo));

            return true;
        }

        public bool AlterarTipoTexto(Guid tipoId, Guid textoId, TipoTextoAnotativo tipoTexto)
        {
            if (tipoTexto == null)
                return false;

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            var estadoNovo = CriarEstadoTipo(tipoTexto);

            if (TipoEEstiloIgual(texto, estadoNovo))
                return false;

            var estadoAnterior = ProjectSheetTemplateTextGraphicTypeState.FromText(texto);

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<ProjectSheetTemplateTextGraphicTypeState>(
                _document,
                tipo,
                texto.Id,
                (t, value) => value.Aplicar(t),
                estadoAnterior,
                estadoNovo));

            return true;
        }

        public bool AlterarCorTexto(Guid tipoId, Guid textoId, string corTexto)
        {
            string corNormalizada = NormalizarCor(corTexto);

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (string.Equals(texto.CorTexto, corNormalizada, StringComparison.OrdinalIgnoreCase))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<string>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.CorTexto = value,
                texto.CorTexto,
                corNormalizada));

            return true;
        }

        public bool AlterarFonte(Guid tipoId, Guid textoId, string fonte)
        {
            string fonteNormalizada = NormalizarFonte(fonte);

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (string.Equals(texto.Fonte, fonteNormalizada, StringComparison.Ordinal))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<string>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.Fonte = value,
                texto.Fonte,
                fonteNormalizada));

            return true;
        }

        public bool AlterarAlturaTexto(Guid tipoId, Guid textoId, double alturaTexto)
        {
            if (!AlturaValida(alturaTexto))
                return false;

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (Math.Abs(texto.AlturaTexto - alturaTexto) < 0.000001)
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<double>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.AlturaTexto = value,
                texto.AlturaTexto,
                alturaTexto));

            return true;
        }

        public bool AlterarAlinhamentoHorizontal(Guid tipoId, Guid textoId, string alinhamentoHorizontal)
        {
            string alinhamentoNormalizado = NormalizarAlinhamento(alinhamentoHorizontal);

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            if (string.Equals(texto.AlinhamentoHorizontal, alinhamentoNormalizado, StringComparison.Ordinal))
                return false;

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<string>(
                _document,
                tipo,
                texto.Id,
                (t, value) => t.AlinhamentoHorizontal = value,
                texto.AlinhamentoHorizontal,
                alinhamentoNormalizado));

            return true;
        }

        private bool AlterarPosicao(ProjectSheetType tipo, ProjectSheetTemplateText texto, double x, double y)
        {
            if (!ValorFinito(x) || !ValorFinito(y))
                return false;

            double deltaSquared = DistanciaQuadrada(texto.X, texto.Y, x, y);

            if (deltaSquared < MinDeltaSquared)
                return false;

            var estadoAnterior = ProjectSheetTemplateTextPositionState.FromText(texto);
            var estadoNovo = new ProjectSheetTemplateTextPositionState(x, y);

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<ProjectSheetTemplateTextPositionState>(
                _document,
                tipo,
                texto.Id,
                (t, value) => value.Aplicar(t),
                estadoAnterior,
                estadoNovo));

            return true;
        }

        private bool TryGetTexto(Guid tipoId, Guid textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto)
        {
            tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId)!;
            texto = tipo?.Textos.FirstOrDefault(t => t.Id == textoId)!;

            return tipo != null && texto != null;
        }

        private static ProjectSheetTemplateTextGraphicTypeState CriarEstadoTipo(TipoTextoAnotativo tipoTexto)
        {
            return new ProjectSheetTemplateTextGraphicTypeState(
                tipoTexto.NomeTipo,
                tipoTexto.Familia,
                tipoTexto.Categoria,
                NormalizarCor(tipoTexto.CorTexto),
                NormalizarFonte(tipoTexto.Fonte),
                NormalizarAltura(tipoTexto.AlturaTexto),
                NormalizarAlinhamento(tipoTexto.AlinhamentoHorizontal));
        }

        private static bool TipoEEstiloIgual(ProjectSheetTemplateText texto, ProjectSheetTemplateTextGraphicTypeState estado)
        {
            return string.Equals(texto.TipoTextoNome, estado.NomeTipo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(texto.TipoTextoFamilia, estado.Familia, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(texto.TipoTextoCategoria, estado.Categoria, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(texto.CorTexto, estado.CorTexto, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(texto.Fonte, estado.Fonte, StringComparison.Ordinal) &&
                Math.Abs(texto.AlturaTexto - estado.AlturaTexto) < 0.000001 &&
                string.Equals(texto.AlinhamentoHorizontal, estado.AlinhamentoHorizontal, StringComparison.Ordinal);
        }

        private static bool TemDeltaValido(double deltaX, double deltaY)
        {
            if (!ValorFinito(deltaX) || !ValorFinito(deltaY))
                return false;

            return deltaX * deltaX + deltaY * deltaY >= MinDeltaSquared;
        }

        private static bool LarguraValida(double value)
        {
            return ValorFinito(value) && value >= ProjectSheetTemplateText.MinBoxWidth;
        }

        private static bool AlturaValida(double value)
        {
            return ValorFinito(value) && value >= ProjectSheetTemplateText.MinTextHeight;
        }

        private static string NormalizarNome(string nome)
        {
            return string.IsNullOrWhiteSpace(nome) ? string.Empty : nome.Trim();
        }

        private static string NormalizarCor(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? ProjectSheetTemplateText.DefaultTextColor : valor.Trim();
        }

        private static string NormalizarFonte(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? ProjectSheetTemplateText.DefaultFont : valor.Trim();
        }

        private static double NormalizarAltura(double valor)
        {
            return !AlturaValida(valor) ? ProjectSheetTemplateText.DefaultTextHeight : valor;
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

        private static double NormalizarRotacao(double valor)
        {
            if (!ValorFinito(valor))
                return 0.0;

            double normalizada = valor % 360.0;

            if (normalizada < 0.0)
                normalizada += 360.0;

            return normalizada >= 360.0 ? 0.0 : normalizada;
        }

        private static bool ValorFinito(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static double DistanciaQuadrada(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return dx * dx + dy * dy;
        }
    }
}