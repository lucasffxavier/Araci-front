using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public class RenomearItemProjetoUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public RenomearItemProjetoUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool RenomearVista(Guid id, string nome)
        {
            string normalizado = NormalizarNome(nome);

            if (string.IsNullOrWhiteSpace(normalizado))
                return false;

            ProjectView? vista = _document.Vistas.FirstOrDefault(v => v.Id == id);

            if (vista == null || NomeIgual(vista.Nome, normalizado))
                return vista != null;

            if (_document.Vistas.Any(v => v.Id != id && NomeIgual(v.Nome, normalizado)))
                return false;

            _commands.Execute(RenameProjectItemCommand.Vista(_document, vista, normalizado));
            return true;
        }

        public bool RenomearTabela(Guid id, string nome)
        {
            string normalizado = NormalizarNome(nome);

            if (string.IsNullOrWhiteSpace(normalizado))
                return false;

            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null || NomeIgual(tabela.Nome, normalizado))
                return tabela != null;

            if (_document.Tabelas.Any(t => t.Id != id && NomeIgual(t.Nome, normalizado)))
                return false;

            _commands.Execute(RenameProjectItemCommand.Tabela(_document, tabela, normalizado));
            return true;
        }

        public bool RenomearPrancha(Guid id, string nome)
        {
            string normalizado = NormalizarNome(nome);

            if (string.IsNullOrWhiteSpace(normalizado))
                return false;

            ProjectSheet? prancha = _document.Pranchas.FirstOrDefault(p => p.Id == id);

            if (prancha == null || NomeIgual(prancha.Nome, normalizado))
                return prancha != null;

            if (_document.Pranchas.Any(p => p.Id != id && NomeIgual(p.Nome, normalizado)))
                return false;

            _commands.Execute(RenameProjectItemCommand.Prancha(_document, prancha, normalizado));
            return true;
        }

        public bool RenomearTipoPrancha(Guid id, string nome)
        {
            string normalizado = NormalizarNome(nome);

            if (string.IsNullOrWhiteSpace(normalizado))
                return false;

            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == id);

            if (tipo == null || NomeIgual(tipo.Nome, normalizado))
                return tipo != null;

            if (_document.TiposPrancha.Any(t => t.Id != id && NomeIgual(t.Nome, normalizado)))
                return false;

            _commands.Execute(RenameProjectItemCommand.TipoPrancha(_document, tipo, normalizado));
            return true;
        }

        private static string NormalizarNome(string nome)
        {
            return string.IsNullOrWhiteSpace(nome) ? string.Empty : nome.Trim();
        }

        private static bool NomeIgual(string atual, string novo)
        {
            return string.Equals(NormalizarNome(atual), novo, StringComparison.OrdinalIgnoreCase);
        }
    }
}
