using System;
using System.Collections.Generic;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Core.Rendering;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services;
using Araci.ViewModels;
using Araci.Services.Catalog;

namespace Araci.Applications.Factories
{
    public class ElementDefinitionsProvider
    {
        private readonly TypeLibraryService _types;
        private readonly ElementInstancePropertyProvider _properties;

        public ElementDefinitionsProvider(
            TypeLibraryService types,
            ElementInstancePropertyProvider properties)
        {
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public IEnumerable<ElementDefinition> CreateDefaults()
        {
            yield return new ElementDefinition(
                ElementKinds.Cabo,
                "Cabo",
                "CABO",
                typeof(Cabo),
                typeof(CaboViewModel),
                typeof(TipoCabo),
                CriarCabo,
                (m, c) => new CaboViewModel((Cabo)m, _types, c.Names, c.TypePropertiesDialogs),
                () => _types.TipoCaboPadrao,
                () => _types.TiposCabos,
                _ => Size.Empty,
                e => AtualizarTerminaisCabo((Cabo)e),
                Ribbon("Cabo", "cabo.svg", 10, "CB"),
                true,
                _properties.Cabo());

            yield return new ElementDefinition(
                ElementKinds.Carga,
                "Carga",
                "CARGA",
                typeof(Carga),
                typeof(CargaViewModel),
                typeof(TipoCarga),
                CriarCarga,
                (m, c) => new CargaViewModel((Carga)m, _types, c.Names, c.TypePropertiesDialogs, c.TerminalLayout),
                () => _types.TipoCargaPadrao,
                () => _types.TiposCargas,
                _ => EquipamentoSize(),
                e => ((Carga)e).AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura),
                Ribbon("Carga", "carga.svg", 20, "CG"),
                false,
                _properties.Carga());

            yield return new ElementDefinition(
                ElementKinds.Gerador,
                "Gerador",
                "GERADOR",
                typeof(Gerador),
                typeof(GeradorViewModel),
                typeof(TipoGerador),
                CriarGerador,
                (m, c) => new GeradorViewModel((Gerador)m, _types, c.Names, c.TypePropertiesDialogs, c.TerminalLayout),
                () => _types.TipoGeradorPadrao,
                () => _types.TiposGeradores,
                _ => EquipamentoSize(),
                e => ((Gerador)e).AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura),
                Ribbon("Gerador", "gerador.svg", 30, "GE"),
                false,
                _properties.Gerador());

            yield return new ElementDefinition(
                ElementKinds.Sin,
                "SIN",
                "SIN",
                typeof(Sin),
                typeof(SinViewModel),
                typeof(TipoSin),
                CriarSin,
                (m, c) => new SinViewModel((Sin)m, _types, c.Names, c.TypePropertiesDialogs, c.TerminalLayout),
                () => _types.TipoSinPadrao,
                () => _types.TiposSin,
                _ => EquipamentoSize(),
                e => ((Sin)e).AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura),
                Ribbon("SIN", "sin.svg", 40, "SI"),
                false,
                _properties.Sin());

            yield return new ElementDefinition(
                ElementKinds.Transformador,
                "Transformador",
                "TR",
                typeof(Transformador),
                typeof(TransformadorViewModel),
                typeof(TipoTransformador),
                CriarTransformador,
                (m, c) => new TransformadorViewModel((Transformador)m, _types, c.Names, c.TypePropertiesDialogs, c.TerminalLayout),
                () => _types.TipoTransformadorPadrao,
                () => _types.TiposTransformadores,
                _ => TransformadorSize(),
                e => ((Transformador)e).AtualizarTerminais(ElementGeometryDefaults.TransformadorLargura, ElementGeometryDefaults.TransformadorAltura),
                Ribbon("Trafo", "transformador.svg", 50, "TR"),
                false,
                _properties.Transformador());

            yield return new ElementDefinition(
                ElementKinds.Barra,
                "Barra",
                "BARRA",
                typeof(Barra),
                typeof(BarraViewModel),
                typeof(TipoBarra),
                CriarBarra,
                (m, c) => new BarraViewModel((Barra)m, _types, c.Names, c.TypePropertiesDialogs, c.TerminalLayout),
                () => _types.TipoBarraPadrao,
                () => _types.TiposBarras,
                e => new Size(ElementGeometryDefaults.BarraLargura, ((Barra)e).Altura),
                e => ((Barra)e).AtualizarTerminais(ElementGeometryDefaults.BarraLargura),
                Ribbon("Barra", "barra.svg", 60, "BA"),
                false,
                _properties.Barra());

            yield return new ElementDefinition(
                ElementKinds.LinhaAnotativa,
                "Linha",
                "LINHA",
                typeof(LinhaAnotativa),
                typeof(LinhaAnotativaViewModel),
                typeof(TipoLinhaAnotativa),
                CriarLinhaAnotativa,
                (m, c) => new LinhaAnotativaViewModel((LinhaAnotativa)m, _types, c.Names, c.TypePropertiesDialogs),
                () => _types.TipoLinhaAnotativaPadrao,
                () => _types.TiposLinhasAnotativas,
                _ => Size.Empty,
                _ => { },
                new ElementRibbonMetadata("Linha", "Desenhar", "linha.svg", 0, false, null),
                false,
                _properties.LinhaAnotativa());
        }

        private Barra CriarBarra()
        {
            return new Barra { Tipo = _types.TipoBarraPadrao ?? throw new InvalidOperationException("Nenhum tipo de barra cadastrado.") };
        }

        private Carga CriarCarga()
        {
            return new Carga { Tipo = _types.TipoCargaPadrao ?? throw new InvalidOperationException("Nenhum tipo de carga cadastrado.") };
        }

        private Gerador CriarGerador()
        {
            return new Gerador { Tipo = _types.TipoGeradorPadrao ?? throw new InvalidOperationException("Nenhum tipo de gerador cadastrado.") };
        }

        private Sin CriarSin()
        {
            return new Sin { Tipo = _types.TipoSinPadrao ?? throw new InvalidOperationException("Nenhum tipo de SIN cadastrado.") };
        }

        private Transformador CriarTransformador()
        {
            return new Transformador { Tipo = _types.TipoTransformadorPadrao ?? throw new InvalidOperationException("Nenhum tipo de transformador cadastrado.") };
        }

        private Cabo CriarCabo()
        {
            return new Cabo { Tipo = _types.TipoCaboPadrao ?? throw new InvalidOperationException("Nenhum tipo de cabo cadastrado.") };
        }

        private LinhaAnotativa CriarLinhaAnotativa()
        {
            return new LinhaAnotativa { Tipo = _types.TipoLinhaAnotativaPadrao ?? throw new InvalidOperationException("Nenhum tipo de linha anotativa cadastrado.") };
        }

        private static ElementRibbonMetadata Ribbon(string nome, string icone, int ordem, string atalho)
        {
            return new ElementRibbonMetadata(nome, "Inserir", icone, ordem, true, atalho);
        }

        private static Size EquipamentoSize()
        {
            return new Size(ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura);
        }

        private static Size TransformadorSize()
        {
            return new Size(ElementGeometryDefaults.TransformadorLargura, ElementGeometryDefaults.TransformadorAltura);
        }

        private static void AtualizarTerminaisCabo(Cabo cabo)
        {
            if (cabo.Vertices.Count > 0)
                cabo.DefinirOrigem(cabo.Vertices[0]);

            if (cabo.Vertices.Count > 1)
                cabo.DefinirDestino(cabo.Vertices[^1]);
        }
    }
}
