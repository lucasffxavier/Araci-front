using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public class EditarPropriedadesVistaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public EditarPropriedadesVistaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool AlterarEscala(Guid id, string escala)
        {
            string normalizado = NormalizarTexto(escala, "1:100");
            return Alterar(id, v => v.Escala, (v, valor) => v.Escala = valor, normalizado);
        }

        public bool AlterarDisciplina(Guid id, ProjectViewDiscipline disciplina)
        {
            return Alterar(id, v => v.Disciplina, (v, valor) => v.Disciplina = valor, disciplina);
        }

        public bool AlterarRecortarVista(Guid id, bool recortarVista)
        {
            return Alterar(id, v => v.RecortarVista, (v, valor) => v.RecortarVista = valor, recortarVista);
        }

        public bool AlterarRegiaoRecorteVisivel(Guid id, bool regiaoRecorteVisivel)
        {
            return Alterar(id, v => v.RegiaoRecorteVisivel, (v, valor) => v.RegiaoRecorteVisivel = valor, regiaoRecorteVisivel);
        }

        private bool Alterar<T>(
            Guid id,
            Func<ProjectView, T> obter,
            Action<ProjectView, T> aplicar,
            T valorNovo)
        {
            ProjectView? vista = _document.Vistas.FirstOrDefault(v => v.Id == id);

            if (vista == null)
                return false;

            T valorAnterior = obter(vista);

            if (Equals(valorAnterior, valorNovo))
                return true;

            _commands.Execute(new UpdateProjectViewPropertyCommand<T>(
                _document,
                vista,
                aplicar,
                valorAnterior,
                valorNovo));

            return true;
        }

        private static string NormalizarTexto(string valor, string fallback)
        {
            return string.IsNullOrWhiteSpace(valor) ? fallback : valor.Trim();
        }
    }
}
