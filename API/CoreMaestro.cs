using System;
using System.Collections.Generic;
using System.Linq;
using Araci.API;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services;

namespace Araci.Maestro
{
    public class CoreMaestro
    {
        private readonly CoreApi _api;

        public CoreMaestro(EditorContext context)
        {
            _api = new CoreApi(context);
        }

        public CoreMaestro(AraciDocument document)
        {
            _api = new CoreApi(document);
        }

        public CoreMaestro(CoreApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public IList<Elemento> ObterElementos()
        {
            return _api.ObterElementos();
        }

        public IList<TElemento> ObterElementos<TElemento>()
            where TElemento : Elemento
        {
            return _api.ObterElementos<TElemento>();
        }

        public IList<Elemento> ObterElementosPorTipo(string nomeTipo)
        {
            return _api.ObterElementosPorTipo(nomeTipo);
        }

        public IList<Parameter> ObterParametros(Elemento elemento)
        {
            return _api.ObterParametros(elemento);
        }

        public IList<IList<Parameter>> ObterParametros(IList<Elemento> elementos)
        {
            if (elementos == null)
                throw new ArgumentNullException(nameof(elementos));

            return elementos
                .Select(elemento => _api.ObterParametros(elemento))
                .ToList();
        }

        public Parameter ObterParametro(Elemento elemento, string nomeParametro)
        {
            return _api.ObterParametro(elemento, nomeParametro);
        }

        public IList<Parameter> ObterParametrosPorNome(IList<Elemento> elementos, string nomeParametro)
        {
            if (elementos == null)
                throw new ArgumentNullException(nameof(elementos));

            return elementos
                .Select(elemento => _api.ObterParametro(elemento, nomeParametro))
                .ToList();
        }

        public IList<object?> ObterValoresParametro(IList<Elemento> elementos, string nomeParametro)
        {
            if (elementos == null)
                throw new ArgumentNullException(nameof(elementos));

            return elementos
                .Select(elemento => _api.ObterValorParametro(elemento, nomeParametro))
                .ToList();
        }

        public IList<int> ObterValoresParametroInteiro(IList<Elemento> elementos, string nomeParametro)
        {
            if (elementos == null)
                throw new ArgumentNullException(nameof(elementos));

            return elementos
                .Select(elemento => _api.ObterValorParametroInteiro(elemento, nomeParametro))
                .ToList();
        }

        public IList<string> ObterValoresParametroTexto(IList<Elemento> elementos, string nomeParametro)
        {
            if (elementos == null)
                throw new ArgumentNullException(nameof(elementos));

            return elementos
                .Select(elemento => _api.ObterValorParametroTexto(elemento, nomeParametro))
                .ToList();
        }

        public void JanelaMensagem(string titulo, string mensagem)
        {
            _api.MostrarMensagem(titulo, mensagem);
        }

        public void ImprimirTexto(string texto)
        {
            _api.ImprimirTexto(texto);
        }
    }
}
