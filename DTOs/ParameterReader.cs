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

        public ParameterReader(CoreApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public ParameterReader(EditorContext context)
            : this(new CoreApi(context))
        {
        }

        public ParameterReader(AraciDocument document)
            : this(new CoreApi(document))
        {
        }

        public IList<LoadData> GetLoads()
        {
            return _api.ObterElementos<Carga>()
                .Select(carga => new LoadData
                {
                    Id = carga.Id.ToString(),
                    Nome = ReadString(carga, "Nome"),
                    Barra = ReadBarra(carga, "Barra", "Barra 1"),
                    Fases = ReadInt(carga, "Fases"),
                    R = ReadDouble(carga, "Carga resistencia", "Carga resistÃªncia"),
                    X = ReadDouble(carga, "Carga reatancia", "Carga reatÃ¢ncia"),
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
            return _api.ObterElementos<Cabo>()
                .Select(cabo => new LineData
                {
                    Id = cabo.Id.ToString(),
                    Nome = ReadString(cabo, "Nome"),
                    Barra1 = ReadBarra(cabo, "BarraOrigem", "Barra 1"),
                    Barra2 = ReadBarra(cabo, "BarraDestino", "Barra 2"),
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
                    Barra = ReadBarra(gerador, "Barra", "Barra 1"),
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
