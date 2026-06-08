using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Araci.API;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services;
using Araci.Services.Topology;

namespace Araci.DTOs
{
    public class ParameterReader
    {
        private readonly CoreApi _api;
        private readonly ConnectivityService? _connectivity;
        private readonly TopologyValidator? _topology;
        private readonly ElectricGraphBuilder? _graphBuilder;
        private readonly IReadOnlyList<Elemento> _elementosOperacionais;

        public ParameterReader(CoreApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _connectivity = new ConnectivityService(api.Document);
            _graphBuilder = new ElectricGraphBuilder(api.Document);
            _topology = new TopologyValidator(api.Document, _connectivity, _graphBuilder);
            _elementosOperacionais = api.Document.ObterElementosDaVistaAtiva().ToList();
        }

        public ParameterReader(EditorContext context)
            : this(
                new CoreApi(context),
                new ConnectivityService(context.Document),
                new TopologyValidator(
                    context.Document,
                    new ConnectivityService(context.Document),
                    context.ElectricGraph),
                context.ElectricGraph)
        {
        }

        public ParameterReader(AraciDocument document)
            : this(
                new CoreApi(document),
                new ConnectivityService(document),
                new TopologyValidator(document),
                new ElectricGraphBuilder(document))
        {
        }

        private ParameterReader(
            CoreApi api,
            ConnectivityService connectivity,
            TopologyValidator topology,
            ElectricGraphBuilder? graphBuilder)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _topology = topology ?? throw new ArgumentNullException(nameof(topology));
            _graphBuilder = graphBuilder;
            _elementosOperacionais = api.Document.ObterElementosDaVistaAtiva().ToList();
        }

        public TopologyValidationResult? ValidateTopology()
        {
            return _topology?.Validate();
        }

        public IList<LoadData> GetLoads()
        {
            ElectricGraph? graph = _graphBuilder?.Build();

            return _elementosOperacionais.OfType<Carga>()
                .Where(carga => carga.ParticipaDoGrafoEletrico)
                .Select(carga => new LoadData
                {
                    Id = carga.Id.ToString(),
                    Nome = ReadString(carga, "Nome"),
                    Barra = ResolverBarraEquipamento(carga, graph),
                    Fases = ReadInt(carga, "Fases"),
                    R = ReadDouble(carga, "Carga resistencia", "Carga resistência"),
                    X = ReadDouble(carga, "Carga reatancia", "Carga reatância"),
                    PotenciaAtiva = ReadDouble(carga, "PotenciaAtiva"),
                    PotenciaReativa = ReadDouble(carga, "PotenciaReativa"),
                    Tensao = ReadKvWithDefault(carga, 12.47, "TensaoKV", "Tensao", "TensaoLinha"),
                    Conexao = ReadString(carga, "Carga conexao", "Conexao"),
                    Modelo = ReadInt(carga, "Carga modelo", "ModeloCarga", "Modelo")
                })
                .ToList();
        }

        public IList<LineData> GetLines()
        {
            ElectricGraph? graph = _graphBuilder?.Build();

            return _elementosOperacionais.OfType<Cabo>()
                .Where(cabo => cabo.ParticipaDoGrafoEletrico)
                .Select(cabo => new LineData
                {
                    Id = cabo.Id.ToString(),
                    Nome = ReadString(cabo, "Nome"),
                    Barra1 = ResolverBus1(cabo, graph),
                    Barra2 = ResolverBus2(cabo, graph),
                    Fases = ReadInt(cabo, "Fases"),
                    Comprimento = ReadDouble(cabo, "Comprimento"),
                    R1 = ReadDouble(cabo, "R1", "Resistencia"),
                    X1 = ReadDouble(cabo, "X1", "Reatancia"),
                    R0 = ReadDouble(cabo, "R0"),
                    X0 = ReadDouble(cabo, "X0"),
                    C1 = ReadDouble(cabo, "C1"),
                    C0 = ReadDouble(cabo, "C0")
                })
                .ToList();
        }

        public IList<TransformerData> GetTransformers()
        {
            return _elementosOperacionais.OfType<Transformador>()
                .Where(transformador => transformador.ParticipaDoGrafoEletrico)
                .Select(transformador => new TransformerData
                {
                    Id = transformador.Id.ToString(),
                    Nome = ReadString(transformador, "Nome"),
                    Fases = ReadIntWithDefault(transformador, 3, "Fases"),
                    Enrolamentos = ReadIntWithDefault(transformador, 2, "Enrolamentos"),
                    BarraPrimario = ResolverBarraTransformador(transformador, Transformador.TERMINAL_PRIMARIO),
                    BarraSecundario = ResolverBarraTransformador(transformador, Transformador.TERMINAL_SECUNDARIO),
                    TensaoPrimarioKV = ReadKvFromInstanceWithDefault(transformador, 13.8, "TensaoPrimarioKV", "TensaoPrimariaKV", "TensaoAltaKV", "TensaoATKV"),
                    TensaoSecundarioKV = ReadKvFromInstanceWithDefault(transformador, 0.38, "TensaoSecundarioKV", "TensaoSecundariaKV", "TensaoBaixaKV", "TensaoBTKV"),
                    PotenciaKVA = ReadPowerKvaFromInstance(transformador),
                    RPercentual = ReadDoubleWithDefault(transformador, 1, "RPercentual", "ResistenciaPercentual", "PercentR"),
                    XPercentual = ReadDoubleWithDefault(transformador, 5, "XPercentual", "ReatanciaPercentual", "PercentX"),
                    LigacaoPrimario = ReadStringWithDefault(transformador, "Wye", "LigacaoPrimario", "LigacaoPrimaria", "ConexaoPrimario", "ConexaoPrimaria"),
                    LigacaoSecundario = ReadStringWithDefault(transformador, "Wye", "LigacaoSecundario", "LigacaoSecundaria", "ConexaoSecundario", "ConexaoSecundaria")
                })
                .ToList();
        }

        private static double ReadPowerKvaFromInstance(Elemento elemento)
        {
            double aparente = ReadDoubleFrom(elemento.Parametros, "PotenciaAparente", "PotenciaAparenteKVA", "PotenciaKVA", "PotenciaNominalKVA");
            return aparente > 0 ? aparente : 500;
        }

        private static double ReadKvFromInstanceWithDefault(Elemento elemento, double defaultValue, params string[] names)
        {
            double modelValue = ReadKvFrom(elemento.Parametros, names);
            return modelValue > 0 ? modelValue : defaultValue;
        }

        public IList<GeneratorData> GetGenerators()
        {
            ElectricGraph? graph = _graphBuilder?.Build();

            return _elementosOperacionais.OfType<Gerador>()
                .Where(gerador => gerador.ParticipaDoGrafoEletrico)
                .Select(gerador => new GeneratorData
                {
                    Id = gerador.Id.ToString(),
                    Nome = ReadString(gerador, "Nome"),
                    Barra = ResolverBarraEquipamento(gerador, graph),
                    Fases = ReadIntWithDefault(gerador, 3, "Fases"),
                    Tensao = ReadKvWithDefault(gerador, 12.47, "TensaoKV", "Tensao", "TensaoLinha"),
                    Potencia = ReadPositiveDoubleByPriority(gerador, "PotenciaAtiva", "Potencia", "PotenciaAparente"),
                    FP = ReadDoubleWithDefault(gerador, 0.98, "FP", "FatorPotencia")
                })
                .ToList();
        }

        public IList<ExternalSourceData> GetSins()
        {
            ElectricGraph? graph = _graphBuilder?.Build();

            return _elementosOperacionais.OfType<Sin>()
                .Where(sin => sin.ParticipaDoGrafoEletrico)
                .Select(sin => new ExternalSourceData
                {
                    Id = sin.Id.ToString(),
                    Nome = ReadString(sin, "Nome"),
                    Barra = ResolverBarraEquipamento(sin, graph),
                    Fases = ReadIntWithDefault(sin, 3, "Fases"),
                    Tensao = ReadKvWithDefault(sin, 12.47, "TensaoKV", "Tensao", "TensaoLinha", "TensaoBaseKV"),
                    PotenciaCurtoMVA = ReadDouble(sin, "PotenciaCurtoMVA", "PotenciaCurtoCircuitoMva"),
                    RelacaoXR = ReadDouble(sin, "RelacaoXR", "X/R")
                })
                .ToList();
        }

        private IList<Elemento> GetElementsByTypeName(params string[] typeNames)
        {
            return _elementosOperacionais
                .Where(elemento => typeNames.Any(typeName =>
                    string.Equals(elemento.GetType().Name, typeName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(elemento.Tipo?.NomeTipo, typeName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private string ResolverBus1(Cabo cabo)
        {
            return ResolverBusPorElementoETerminal(cabo.OrigemId, cabo.OrigemTerminalId) ??
                _connectivity?.ResolverBus1Estrito(cabo) ??
                string.Empty;
        }

        private string ResolverBus1(Cabo cabo, ElectricGraph? graph)
        {
            return ResolverBusPorGrafo(cabo, graph, origem: true) ?? ResolverBus1(cabo);
        }

        private string ResolverBus2(Cabo cabo)
        {
            return ResolverBusPorElementoETerminal(cabo.DestinoId, cabo.DestinoTerminalId) ??
                _connectivity?.ResolverBus2Estrito(cabo) ??
                string.Empty;
        }

        private string ResolverBus2(Cabo cabo, ElectricGraph? graph)
        {
            return ResolverBusPorGrafo(cabo, graph, origem: false) ?? ResolverBus2(cabo);
        }

        private static string? ResolverBusPorGrafo(
            Cabo cabo,
            ElectricGraph? graph,
            bool origem)
        {
            ElectricGraphEdge? edge = graph?.FindEdgeByCable(cabo);

            if (edge == null)
                return null;

            if (!edge.IsValid)
                return string.Empty;

            string elementId = origem ? edge.FromElementId : edge.ToElementId;
            string terminalId = origem ? edge.FromTerminalId : edge.ToTerminalId;
            ElectricGraphNode? node = graph?.FindNode(elementId);

            return ResolverBarraPorTerminal(node?.SourceElement, terminalId) ??
                node?.Name ??
                string.Empty;
        }

        private string ResolverBarraEquipamento(ElementoEquipamento equipamento)
        {
            return _connectivity?.ResolverBusNameParaEquipamentoEstrito(equipamento) ?? string.Empty;
        }

        private string ResolverBarraEquipamento(
            ElementoEquipamento equipamento,
            ElectricGraph? graph)
        {
            return ResolverBarraPorGrafo(equipamento, graph) ??
                ResolverBarraEquipamento(equipamento);
        }

        private static string? ResolverBarraPorGrafo(
            ElementoEquipamento equipamento,
            ElectricGraph? graph)
        {
            return graph?.FindNodeByElement(equipamento)?.Name;
        }

        private static string ResolverBarraTransformador(
            Transformador transformador,
            string terminalId)
        {
            return ResolverBarraTerminalTransformador(transformador, terminalId);
        }

        private string? ResolverBusPorElementoETerminal(string elementId, string terminalId)
        {
            Elemento? elemento = ObterElementoOperacionalPorId(elementId);
            return ResolverBarraPorTerminal(elemento, terminalId);
        }

        private Elemento? ObterElementoOperacionalPorId(string elementId)
        {
            if (string.IsNullOrWhiteSpace(elementId))
                return null;

            return _elementosOperacionais.FirstOrDefault(e =>
                string.Equals(e.Id.ToString(), elementId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string? ResolverBarraPorTerminal(Elemento? elemento, string terminalId)
        {
            return elemento is Transformador transformador &&
                EhTerminalTransformador(terminalId)
                    ? ResolverBarraTerminalTransformador(transformador, terminalId)
                    : null;
        }

        private static string ResolverBarraTerminalTransformador(
            Transformador transformador,
            string terminalId)
        {
            string nome = NomeBarramento(transformador);
            string terminal = string.Equals(terminalId, Transformador.TERMINAL_PRIMARIO, StringComparison.OrdinalIgnoreCase)
                ? Transformador.TERMINAL_PRIMARIO
                : Transformador.TERMINAL_SECUNDARIO;

            return $"{nome}_{terminal}";
        }

        private static bool EhTerminalTransformador(string terminalId)
        {
            return string.Equals(terminalId, Transformador.TERMINAL_PRIMARIO, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(terminalId, Transformador.TERMINAL_SECUNDARIO, StringComparison.OrdinalIgnoreCase);
        }

        private static string NomeBarramento(Elemento elemento)
        {
            if (!string.IsNullOrWhiteSpace(elemento.Nome))
                return elemento.Nome.Trim();

            string id = elemento.Id.ToString("N");
            return id.Length >= 8 ? $"BUS-{id[..8]}" : $"BUS-{id}";
        }

        private static string ReadBarra(Elemento elemento, params string[] names)
        {
            string valor = ReadString(elemento, names);

            if (!string.IsNullOrWhiteSpace(valor))
                return valor;

            if (elemento is ITerminalOwner owner)
                return owner.Terminais.FirstOrDefault()?.Barra ?? string.Empty;

            return string.Empty;
        }

        private static string ReadString(Elemento elemento, params string[] names)
        {
            return ReadValueAsString(elemento, names) ?? string.Empty;
        }

        private static int ReadInt(Elemento elemento, params string[] names)
        {
            return (int)ReadDouble(elemento, names);
        }

        private static double ReadDouble(Elemento elemento, params string[] names)
        {
            object? value = ReadValueObject(elemento, names);

            if (value is double doubleValue)
                return ElectricalValueParser.ToNumber(doubleValue);

            if (value is int intValue)
                return intValue;

            if (value is string text)
                return ElectricalValueParser.ToNumber(text);

            return 0;
        }

        private static double ReadKv(Elemento elemento, params string[] names)
        {
            object? value = ReadValueObject(elemento, names);

            if (value is double doubleValue)
                return ElectricalValueParser.ToNumber(doubleValue);

            if (value is int intValue)
                return intValue;

            if (value is string text)
                return ElectricalValueParser.ToNumber(text);

            return 0;
        }

        private static double ReadPowerKva(Elemento elemento)
        {
            double modelMva = ReadDoubleFrom(elemento.Parametros, "PotenciaMVA", "PotenciaNominalMVA");

            if (modelMva > 0)
                return modelMva * 1000;

            double typeMva = elemento.Tipo == null
                ? 0
                : ReadDoubleFrom(elemento.Tipo.Parametros, "PotenciaMVA", "PotenciaNominalMVA");

            if (typeMva > 0)
                return typeMva * 1000;

            double modelKva = ReadDoubleFrom(elemento.Parametros, "PotenciaKVA", "PotenciaNominalKVA");
            double typeKva = elemento.Tipo == null
                ? 0
                : ReadDoubleFrom(elemento.Tipo.Parametros, "PotenciaKVA", "PotenciaNominalKVA");

            if (modelKva > 0 && !NearlyEqual(modelKva, 500))
                return modelKva;

            if (typeKva > 0)
                return typeKva;

            return modelKva;
        }

        private static int ReadIntWithDefault(Elemento elemento, int defaultValue, params string[] names)
        {
            return (int)ReadDoubleWithDefault(elemento, defaultValue, names);
        }

        private static double ReadDoubleWithDefault(
            Elemento elemento,
            double defaultValue,
            params string[] names)
        {
            double modelValue = ReadDoubleFrom(elemento.Parametros, names);
            double typeValue = elemento.Tipo == null
                ? 0
                : ReadDoubleFrom(elemento.Tipo.Parametros, names);

            if (modelValue > 0)
                return modelValue;

            if (typeValue > 0)
                return typeValue;

            return defaultValue;
        }

        private static double ReadPositiveDoubleByPriority(Elemento elemento, params string[] names)
        {
            double modelValue = ReadPositiveDoubleFrom(elemento.Parametros, names);

            if (modelValue > 0)
                return modelValue;

            return elemento.Tipo == null
                ? 0
                : ReadPositiveDoubleFrom(elemento.Tipo.Parametros, names);
        }

        private static double ReadKvWithDefault(
            Elemento elemento,
            double defaultValue,
            params string[] names)
        {
            double modelValue = ReadKvFrom(elemento.Parametros, names);
            double typeValue = elemento.Tipo == null
                ? 0
                : ReadKvFrom(elemento.Tipo.Parametros, names);

            if (modelValue > 0)
                return modelValue;

            if (typeValue > 0)
                return typeValue;

            return defaultValue;
        }

        private static string ReadStringWithDefault(
            Elemento elemento,
            string defaultValue,
            params string[] names)
        {
            string modelValue = ReadStringFrom(elemento.Parametros, names);
            string typeValue = elemento.Tipo == null
                ? string.Empty
                : ReadStringFrom(elemento.Tipo.Parametros, names);

            if (!string.IsNullOrWhiteSpace(modelValue))
                return modelValue;

            if (!string.IsNullOrWhiteSpace(typeValue))
                return typeValue;

            return defaultValue;
        }

        private static string? ReadValueAsString(Elemento elemento, params string[] names)
        {
            object? value = ReadValueObject(elemento, names);

            return value switch
            {
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value?.ToString()
            };
        }

        private static object? ReadValueObject(Elemento elemento, params string[] names)
        {
            foreach (string name in names)
            {
                if (elemento.Parametros.TryGetValue(name, out Parameter? parametro))
                    return parametro.ValorObjeto;
            }

            foreach (string name in names)
            {
                Parameter? parametro;
                if (elemento.Tipo?.Parametros.TryGetValue(name, out parametro) == true)
                    return parametro.ValorObjeto;
            }

            return null;
        }

        private static double ReadDoubleFrom(
            IReadOnlyDictionary<string, Parameter> parametros,
            params string[] names)
        {
            object? value = ReadValueObjectFrom(parametros, names);

            if (value is double doubleValue)
                return ElectricalValueParser.ToNumber(doubleValue);

            if (value is int intValue)
                return intValue;

            if (value is string text)
                return ElectricalValueParser.ToNumber(text);

            return 0;
        }

        private static double ReadPositiveDoubleFrom(
            IReadOnlyDictionary<string, Parameter> parametros,
            params string[] names)
        {
            foreach (string name in names)
            {
                double value = ReadDoubleFrom(parametros, name);

                if (value > 0)
                    return value;
            }

            return 0;
        }

        private static double ReadKvFrom(
            IReadOnlyDictionary<string, Parameter> parametros,
            params string[] names)
        {
            object? value = ReadValueObjectFrom(parametros, names);

            if (value is double doubleValue)
                return ElectricalValueParser.ToNumber(doubleValue);

            if (value is int intValue)
                return intValue;

            if (value is string text)
                return ElectricalValueParser.ToNumber(text);

            return 0;
        }

        private static string ReadStringFrom(
            IReadOnlyDictionary<string, Parameter> parametros,
            params string[] names)
        {
            object? value = ReadValueObjectFrom(parametros, names);

            return value switch
            {
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value?.ToString() ?? string.Empty
            };
        }

        private static object? ReadValueObjectFrom(
            IReadOnlyDictionary<string, Parameter> parametros,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (parametros.TryGetValue(name, out Parameter? parametro))
                    return parametro.ValorObjeto;
            }

            return null;
        }

        private static bool NearlyEqual(double left, double right)
        {
            return Math.Abs(left - right) < 0.000001;
        }

        public class LoadData
        {
            public string Id { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public string Barra { get; set; } = string.Empty;

            public int Fases { get; set; }

            public double R { get; set; }

            public double X { get; set; }

            public double PotenciaAtiva { get; set; }

            public double PotenciaReativa { get; set; }

            public double Tensao { get; set; }

            public string Conexao { get; set; } = string.Empty;

            public int Modelo { get; set; }
        }

        public class LineData
        {
            public string Id { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public string Barra1 { get; set; } = string.Empty;

            public string Barra2 { get; set; } = string.Empty;

            public int Fases { get; set; }

            public double Comprimento { get; set; }

            public double R1 { get; set; }

            public double X1 { get; set; }

            public double R0 { get; set; }

            public double X0 { get; set; }

            public double C1 { get; set; }

            public double C0 { get; set; }
        }

        public class TransformerData
        {
            public string Id { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public int Fases { get; set; }

            public int Enrolamentos { get; set; }

            public string BarraPrimario { get; set; } = string.Empty;

            public string BarraSecundario { get; set; } = string.Empty;

            public double TensaoPrimarioKV { get; set; }

            public double TensaoSecundarioKV { get; set; }

            public double PotenciaKVA { get; set; }

            public double RPercentual { get; set; }

            public double XPercentual { get; set; }

            public string LigacaoPrimario { get; set; } = string.Empty;

            public string LigacaoSecundario { get; set; } = string.Empty;
        }

        public class GeneratorData
        {
            public string Id { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public string Barra { get; set; } = string.Empty;

            public int Fases { get; set; }

            public double Tensao { get; set; }

            public double Potencia { get; set; }

            public double FP { get; set; }
        }

        public class ExternalSourceData
        {
            public string Id { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public string Barra { get; set; } = string.Empty;

            public int Fases { get; set; }

            public double Tensao { get; set; }

            public double PotenciaCurtoMVA { get; set; }

            public double RelacaoXR { get; set; }
        }
    }
}
