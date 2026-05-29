using System;
using System.Collections.Generic;
using System.Linq;

namespace Araci.ViewModels
{
    public static class InstancePropertyCatalog
    {
        private static readonly Dictionary<Type, IReadOnlyList<InstancePropertyDescriptor>> _properties = new()
        {
            [typeof(BarraViewModel)] = new[]
            {
                Prop<BarraViewModel>("Nome", "Nome", 10),
                Prop<BarraViewModel>("Tensao", "Tensão (kV)", 20),
                Prop<BarraViewModel>("Altura", "Altura (m)", 30)
            },
            [typeof(CaboViewModel)] = new[]
            {
                Prop<CaboViewModel>("Nome", "Nome", 10),
                Prop<CaboViewModel>("BarraOrigem", "Barra origem", 20),
                Prop<CaboViewModel>("BarraDestino", "Barra destino", 30),
                Prop<CaboViewModel>("Comprimento", "Comprimento (m)", 40),
                Prop<CaboViewModel>("Ampacidade", "Ampacidade (A)", 50),
                Prop<CaboViewModel>("TensaoLinha", "Tensão linha (kV)", 60, allowMixedTypeEdit: true),
                Prop<CaboViewModel>("TensaoFaseA", "Tensão fase A (kV)", 70, allowMixedTypeEdit: true),
                Prop<CaboViewModel>("TensaoFaseB", "Tensão fase B (kV)", 80, allowMixedTypeEdit: true),
                Prop<CaboViewModel>("TensaoFaseC", "Tensão fase C (kV)", 90, allowMixedTypeEdit: true),
                Prop<CaboViewModel>("CorrenteLinha", "Corrente linha (A)", 100, allowMixedTypeEdit: true),
                Prop<CaboViewModel>("CorrenteFaseA", "Corrente fase A (A)", 110, allowMixedTypeEdit: true),
                Prop<CaboViewModel>("CorrenteFaseB", "Corrente fase B (A)", 120, allowMixedTypeEdit: true),
                Prop<CaboViewModel>("CorrenteFaseC", "Corrente fase C (A)", 130, allowMixedTypeEdit: true)
            },
            [typeof(CargaViewModel)] = new[]
            {
                Prop<CargaViewModel>("Nome", "Nome", 10),
                Prop<CargaViewModel>("PotenciaAtiva", "Potência ativa (kW)", 20, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("PotenciaReativa", "Potência reativa (kVAr)", 30, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("Alimentador", "Alimentador", 40, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("CorrenteLinha", "Corrente linha (A)", 50, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("CorrenteFaseA", "Corrente fase A (A)", 60, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("CorrenteFaseB", "Corrente fase B (A)", 70, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("CorrenteFaseC", "Corrente fase C (A)", 80, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("TensaoLinha", "Tensão linha (kV)", 90, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("TensaoFaseA", "Tensão fase A (kV)", 100, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("TensaoFaseB", "Tensão fase B (kV)", 110, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("TensaoFaseC", "Tensão fase C (kV)", 120, allowMixedTypeEdit: true)
            },
            [typeof(GeradorViewModel)] = new[]
            {
                Prop<GeradorViewModel>("Nome", "Nome", 10),
                Prop<GeradorViewModel>("PotenciaAparente", "Potência aparente (kVA)", 20),
                Prop<GeradorViewModel>("PotenciaAtiva", "Potência ativa (kW)", 30, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("PotenciaReativa", "Potência reativa (kVAr)", 40, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("TensaoLinha", "Tensão linha (kV)", 50, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("TensaoFaseA", "Tensão fase A (kV)", 60, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("TensaoFaseB", "Tensão fase B (kV)", 70, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("TensaoFaseC", "Tensão fase C (kV)", 80, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("CorrenteLinha", "Corrente linha (A)", 90, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("CorrenteFaseA", "Corrente fase A (A)", 100, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("CorrenteFaseB", "Corrente fase B (A)", 110, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("CorrenteFaseC", "Corrente fase C (A)", 120, allowMixedTypeEdit: true)
            },
            [typeof(SinViewModel)] = new[]
            {
                Prop<SinViewModel>("Nome", "Nome", 10),
                Prop<SinViewModel>("TensaoLinha", "Tensão linha (kV)", 20, allowMixedTypeEdit: true)
            },
            [typeof(TransformadorViewModel)] = new[]
            {
                Prop<TransformadorViewModel>("Nome", "Nome", 10),
                Prop<TransformadorViewModel>("Barra", "Barra", 20),
                Prop<TransformadorViewModel>("Alimentador", "Alimentador", 30, allowMixedTypeEdit: true),
                Prop<TransformadorViewModel>("Fases", "Fases", 40),
                Prop<TransformadorViewModel>("Enrolamentos", "Enrolamentos", 50),
                Prop<TransformadorViewModel>("TensaoPrimarioKV", "Tensão primário (kV)", 60),
                Prop<TransformadorViewModel>("TensaoSecundarioKV", "Tensão secundário (kV)", 70),
                Prop<TransformadorViewModel>("PotenciaAparente", "Potência aparente (kVA)", 80),
                Prop<TransformadorViewModel>("RPercentual", "R (%)", 90),
                Prop<TransformadorViewModel>("XPercentual", "X (%)", 100),
                Prop<TransformadorViewModel>("LigacaoPrimario", "Ligação primário", 110),
                Prop<TransformadorViewModel>("LigacaoSecundario", "Ligação secundário", 120)
            }
        };

        public static IReadOnlyList<InstancePropertyDescriptor> GetFor(ElementoViewModel elemento)
        {
            return elemento == null ? Array.Empty<InstancePropertyDescriptor>() : GetFor(elemento.GetType());
        }

        public static IReadOnlyList<InstancePropertyDescriptor> GetFor(Type type)
        {
            return _properties.TryGetValue(type, out var properties) ? properties : Array.Empty<InstancePropertyDescriptor>();
        }

        public static IReadOnlyList<InstancePropertyDescriptor> GetCommonFor(IReadOnlyList<ElementoViewModel> elementos)
        {
            if (elementos.Count == 0)
                return Array.Empty<InstancePropertyDescriptor>();

            var commonNames = new HashSet<string>(GetFor(elementos[0]).Select(p => p.PropertyName));

            foreach (var elemento in elementos.Skip(1))
                commonNames.IntersectWith(GetFor(elemento).Select(p => p.PropertyName));

            return GetFor(elementos[0])
                .Where(p => commonNames.Contains(p.PropertyName))
                .OrderBy(p => p.Order)
                .ThenBy(p => p.DisplayName)
                .ToList();
        }

        public static bool CanEditAcrossMixedTypes(IReadOnlyList<ElementoViewModel> elementos, string propertyName)
        {
            if (elementos.Count == 0 || string.IsNullOrWhiteSpace(propertyName))
                return false;

            return elementos.All(e => GetFor(e).Any(p => p.PropertyName == propertyName && p.IsEditable && p.AllowMixedTypeEdit));
        }

        private static InstancePropertyDescriptor Prop<T>(string propertyName, string displayName, int order, bool isEditable = true, bool allowMixedTypeEdit = false)
            where T : ElementoViewModel
        {
            return new InstancePropertyDescriptor(typeof(T), propertyName, displayName, order, isEditable, allowMixedTypeEdit);
        }
    }
}