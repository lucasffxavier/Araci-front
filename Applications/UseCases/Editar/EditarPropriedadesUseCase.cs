using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.UseCases.Editar
{
    public class EditarPropriedadesUseCase
    {
        private readonly ICommandHistory _commands;

        public EditarPropriedadesUseCase(EditorContext context)
            : this(context?.Commands ?? throw new ArgumentNullException(nameof(context)))
        {
        }

        public EditarPropriedadesUseCase(ICommandHistory commands)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Executar(IEnumerable<ElementoViewModel> elementos, string propertyName, object? novoValor)
        {
            ArgumentNullException.ThrowIfNull(elementos);

            if (string.IsNullOrWhiteSpace(propertyName))
                return false;

            var items = new List<BulkPropertyChangeCommand.Item>();

            foreach (ElementoViewModel elemento in elementos.Where(e => e != null))
            {
                PropertyInfo? prop = ObterPropriedadeEditavel(elemento, propertyName);

                if (prop == null)
                    continue;

                object? valorAntes = prop.GetValue(elemento);

                if (ValoresIguais(valorAntes, novoValor))
                    continue;

                items.Add(new BulkPropertyChangeCommand.Item(elemento, propertyName, valorAntes, novoValor));
            }

            if (items.Count == 0)
                return false;

            var command = new BulkPropertyChangeCommand(items);

            if (command.IsEmpty)
                return false;

            _commands.Execute(command);
            return true;
        }

        private static PropertyInfo? ObterPropriedadeEditavel(ElementoViewModel elemento, string propertyName)
        {
            PropertyInfo? prop = elemento.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            if (prop == null || !prop.CanWrite || prop.GetIndexParameters().Length > 0)
                return null;

            return prop;
        }

        private static bool ValoresIguais(object? a, object? b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            if (a is double da && b is double db)
                return Math.Abs(da - db) < 0.000001;

            if (a is float fa && b is float fb)
                return Math.Abs(fa - fb) < 0.000001f;

            return Equals(a, b);
        }
    }
}
