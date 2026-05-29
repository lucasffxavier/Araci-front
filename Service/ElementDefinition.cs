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
            Action<Elemento> atualizarTerminais,
            string? nomeRibbon = null,
            string? categoriaRibbon = null,
            string? icone = null,
            int ordemRibbon = 0,
            bool exibirNoRibbon = true,
            string? atalho = null,
            bool usaFerramentaEspecial = false)
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
            NomeRibbon = string.IsNullOrWhiteSpace(nomeRibbon) ? nomeAmigavel : nomeRibbon;
            CategoriaRibbon = string.IsNullOrWhiteSpace(categoriaRibbon) ? "Inserir" : categoriaRibbon;
            Icone = NormalizarIcone(icone);
            OrdemRibbon = ordemRibbon;
            ExibirNoRibbon = exibirNoRibbon;
            Atalho = string.IsNullOrWhiteSpace(atalho) ? string.Empty : atalho.Trim().ToUpperInvariant();
            UsaFerramentaEspecial = usaFerramentaEspecial;
        }

        public string Kind { get; }
        public string NomeAmigavel { get; }
        public string PrefixoNome { get; }
        public Type ModelType { get; }
        public Type ViewModelType { get; }
        public Type TypeModelType { get; }
        public string NomeRibbon { get; }
        public string CategoriaRibbon { get; }
        public string Icone { get; }
        public int OrdemRibbon { get; }
        public bool ExibirNoRibbon { get; }
        public string Atalho { get; }
        public bool UsaFerramentaEspecial { get; }
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

        public bool AceitaViewModel(ElementoViewModel viewModel)
        {
            return ViewModelType.IsInstanceOfType(viewModel);
        }

        private static string NormalizarIcone(string? icone)
        {
            if (string.IsNullOrWhiteSpace(icone))
                return string.Empty;

            string valor = icone.Trim();

            if (valor.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                return valor;

            return $"pack://application:,,,/Resources/Icons/{valor}";
        }
    }
}