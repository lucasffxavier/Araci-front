using System;
using System.Collections.Generic;
using System.Windows;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ElementDefinition
    {
        public ElementDefinition(
            string kind,
            string nomeAmigavel,
            string prefixoNome,
            Type modelType,
            Type viewModelType,
            Type typeModelType,
            Func<Elemento> criarModelo,
            Func<Elemento, NameService, TypePropertiesDialogService, TerminalLayoutService, ElementoViewModel?> criarViewModel,
            Func<TipoElemento?> obterTipoPadrao,
            Func<IEnumerable<TipoElemento>> obterTipos,
            Func<Elemento, Size> obterTamanho,
            Action<Elemento> atualizarTerminais)
        {
            Kind = kind ?? throw new ArgumentNullException(nameof(kind));
            NomeAmigavel = nomeAmigavel ?? throw new ArgumentNullException(nameof(nomeAmigavel));
            PrefixoNome = prefixoNome ?? throw new ArgumentNullException(nameof(prefixoNome));
            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            ViewModelType = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
            TypeModelType = typeModelType ?? throw new ArgumentNullException(nameof(typeModelType));
            CriarModelo = criarModelo ?? throw new ArgumentNullException(nameof(criarModelo));
            CriarViewModel = criarViewModel ?? throw new ArgumentNullException(nameof(criarViewModel));
            ObterTipoPadrao = obterTipoPadrao ?? throw new ArgumentNullException(nameof(obterTipoPadrao));
            ObterTipos = obterTipos ?? throw new ArgumentNullException(nameof(obterTipos));
            ObterTamanho = obterTamanho ?? throw new ArgumentNullException(nameof(obterTamanho));
            AtualizarTerminais = atualizarTerminais ?? throw new ArgumentNullException(nameof(atualizarTerminais));
        }

        public string Kind { get; }
        public string NomeAmigavel { get; }
        public string PrefixoNome { get; }
        public Type ModelType { get; }
        public Type ViewModelType { get; }
        public Type TypeModelType { get; }
        public Func<Elemento> CriarModelo { get; }
        public Func<Elemento, NameService, TypePropertiesDialogService, TerminalLayoutService, ElementoViewModel?> CriarViewModel { get; }
        public Func<TipoElemento?> ObterTipoPadrao { get; }
        public Func<IEnumerable<TipoElemento>> ObterTipos { get; }
        public Func<Elemento, Size> ObterTamanho { get; }
        public Action<Elemento> AtualizarTerminais { get; }

        public bool AceitaModelo(Elemento elemento)
        {
            return ModelType.IsInstanceOfType(elemento);
        }
    }
}
