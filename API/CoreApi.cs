using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services;

namespace Araci.API
{
    public class CoreApi
    {
        private readonly EditorContext? _context;
        private readonly AraciDocument _document;

        public CoreApi(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _document = context.Document;
        }

        public CoreApi(AraciDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public IList<Elemento> ObterElementos()
        {
            return _document.Elementos.ToList();
        }

        public IList<TElemento> ObterElementos<TElemento>()
            where TElemento : Elemento
        {
            return _document.Elementos.OfType<TElemento>().ToList();
        }

        public IList<Elemento> ObterElementosPorTipo(string nomeTipo)
        {
            if (string.IsNullOrWhiteSpace(nomeTipo))
                throw new ArgumentException("Nome do tipo invalido.", nameof(nomeTipo));

            return _document.Elementos
                .Where(e =>
                    string.Equals(e.GetType().Name, nomeTipo, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(e.Tipo?.NomeTipo, nomeTipo, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IList<Parameter> ObterParametros(Elemento elemento)
        {
            ValidarElemento(elemento);

            return elemento.Parametros.Values.ToList();
        }

        public Parameter ObterParametro(Elemento elemento, string nomeParametro)
        {
            ValidarElemento(elemento);

            if (string.IsNullOrWhiteSpace(nomeParametro))
                throw new ArgumentException("Nome do parametro invalido.", nameof(nomeParametro));

            if (!elemento.Parametros.TryGetValue(nomeParametro, out var parametro))
                throw new InvalidOperationException($"Parametro nao encontrado: {nomeParametro}");

            return parametro;
        }

        public object? ObterValorParametro(Elemento elemento, string nomeParametro)
        {
            return ObterParametro(elemento, nomeParametro).ValorObjeto;
        }

        public T ObterValorParametro<T>(Elemento elemento, string nomeParametro)
        {
            ValidarElemento(elemento);

            return elemento.Obter<T>(nomeParametro);
        }

        public int ObterValorParametroInteiro(Elemento elemento, string nomeParametro)
        {
            if (elemento == null || string.IsNullOrWhiteSpace(nomeParametro))
                return 0;

            if (!elemento.Parametros.TryGetValue(nomeParametro, out var parametro))
                return 0;

            try
            {
                if (parametro is Parameter<int> inteiro)
                    return inteiro.Valor;

                return parametro.ValorObjeto is int valor
                    ? valor
                    : 0;
            }
            catch
            {
                return 0;
            }
        }

        public string ObterValorParametroTexto(Elemento elemento, string nomeParametro)
        {
            return ObterValorParametro(elemento, nomeParametro)?.ToString() ?? string.Empty;
        }

        public void MostrarMensagem(string titulo, string mensagem)
        {
            MessageBox.Show(
                mensagem ?? string.Empty,
                string.IsNullOrWhiteSpace(titulo) ? "Araci" : titulo,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void ImprimirTexto(string texto)
        {
            MostrarMensagem("Araci", texto);
        }

        private static void ValidarElemento(Elemento? elemento)
        {
            if (elemento == null)
                throw new ArgumentNullException(nameof(elemento), "Elemento nulo.");
        }
    }
}
