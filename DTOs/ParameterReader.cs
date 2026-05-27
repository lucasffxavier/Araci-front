using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Araci.API;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services;

namespace Araci.DTOs
{
    public class ParameterReader
    {
        private readonly CoreApi _api;
        private readonly ConnectivityService? _connectivity;
        private readonly TopologyValidator? _topology;
        private readonly ElectricGraphBuilder? _graphBuilder;

        public ParameterReader(CoreApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public ParameterReader(EditorContext context)
            : this(
                new CoreApi(context),
                new ConnectivityService(context),
                new TopologyValidator(context),
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
        }

        public TopologyValidationResult? ValidateTopology()
        {
            return _topology?.Validate();
        }

        public IList<LoadData> GetLoads()
        {
            return _api.ObterElementos<Carga>()
                .Select(carga => new LoadData
                {
                    Id = carga.Id.ToString(),
                    Nome = ReadString(carga, "Nome"),
                    Barra = ResolverBarraEquipamento(carga),
                    Fases = ReadInt(carga, "Fases"),
                    R = ReadDouble(carga, "Carga resistencia", "Carga resistência"),
                    X = ReadDouble(carga, "Carga reatancia", "Carga reatância"),
                    PotenciaAtiva = ReadDouble(carga, "PotenciaAtiva"),
                    PotenciaReativa = ReadDouble(carga, "PotenciaReativa"),
                    Tensao = ReadVoltage(carga, "TensaoKV", "Tensao", "TensaoLinha"),
                    Conexao = ReadString(carga, "Carga conexao", "Conexao"),
                    Modelo = ReadInt(carga, "Carga modelo", "ModeloCarga")
                })
                .ToList();
        }

        public IList<LineData> GetLines()
        {
            ElectricGraph? graph = _graphBuilder?.Build();

            return _api.ObterElementos<Cabo>()
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
            return GetElementsByTypeName("Transformador", "Transformer")
                .Select(transformador => new TransformerData
                {
                    Id = transformador.Id.ToString(),
                    Nome = ReadString(transformador, "Nome"),
                    Fases = ReadInt(transformador, "Fases"),
                    Enrolamentos = ReadInt(transformador, "Enrolamentos")
                })
                .ToList();
        }

        public IList<GeneratorData> GetGenerators()
        {
            return _api.ObterElementos<Gerador>()
                .Select(gerador => new GeneratorData
                {
                    Id = gerador.Id.ToString(),
                    Nome = ReadString(gerador, "Nome"),
                    Barra = ResolverBarraEquipamento(gerador),
                    Fases = ReadInt(gerador, "Fases"),
                    Tensao = ReadVoltage(gerador, "TensaoKV", "Tensao", "TensaoLinha"),
                    Potencia = ReadDouble(gerador, "PotenciaAtiva", "Potencia", "PotenciaAparente"),
                    FP = ReadDouble(gerador, "FP", "FatorPotencia")
                })
                .ToList();
        }

        private IList<Elemento> GetElementsByTypeName(params string[] typeNames)
        {
            return _api.ObterElementos()
                .Where(elemento => typeNames.Any(typeName =>
                    string.Equals(elemento.GetType().Name, typeName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(elemento.Tipo?.NomeTipo, typeName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private string ResolverBus1(Cabo cabo)
        {
            return _connectivity?.ResolverBus1Estrito(cabo) ?? string.Empty;
        }

        private string ResolverBus1(Cabo cabo, ElectricGraph? graph)
        {
            return ResolverBusPorGrafo(cabo, graph, origem: true) ?? ResolverBus1(cabo);
        }

        private string ResolverBus2(Cabo cabo)
        {
            return _connectivity?.ResolverBus2Estrito(cabo) ?? string.Empty;
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

            return graph?.FindNode(elementId)?.Name ?? string.Empty;
        }

        private string ResolverBarraEquipamento(ElementoEquipamento equipamento)
        {
            return _connectivity?.ResolverBusNameParaEquipamentoEstrito(equipamento) ?? string.Empty;
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

        private static double ReadVoltage(Elemento elemento, params string[] names)
        {
            object? value = ReadValueObject(elemento, names);

            if (value is double doubleValue)
                return ElectricalValueParser.ToVoltageKv(doubleValue);

            if (value is int intValue)
                return ElectricalValueParser.ToVoltageKv(intValue);

            if (value is string text)
                return ElectricalValueParser.ToVoltageKv(text);

            return 0;
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

                if (elemento.Tipo?.Parametros.TryGetValue(name, out parametro) == true)
                    return parametro.ValorObjeto;
            }

            return null;
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
    }
}
