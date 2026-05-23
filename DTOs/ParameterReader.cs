using System;
using System.Collections.Generic;
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
                    Barra = ReadBarra(carga, "Barra 1"),
                    Fases = ReadInt(carga, "Fases"),
                    R = ReadDouble(carga, "Carga resistência", "Carga resistencia"),
                    X = ReadDouble(carga, "Carga reatância", "Carga reatancia"),
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
                    Barra1 = ReadString(cabo, "Barra 1", "BarraOrigem"),
                    Barra2 = ReadString(cabo, "Barra 2", "BarraDestino"),
                    Fases = ReadInt(cabo, "Fases"),
                    Comprimento = ReadDouble(cabo, "Comprimento"),
                    R1 = ReadDouble(cabo, "R1"),
                    X1 = ReadDouble(cabo, "X1"),
                    R0 = ReadDouble(cabo, "R0"),
                    X0 = ReadDouble(cabo, "X0")
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
                    Barra = ReadBarra(gerador, "Barra 1"),
                    Fases = ReadInt(gerador, "Fases"),
                    Tensao = ReadDouble(gerador, "Tensao", "TensaoLinha"),
                    Potencia = ReadDouble(gerador, "Potencia", "PotenciaAparente"),
                    FP = ReadDouble(gerador, "FP", "FatorPotencia")
                })
                .ToList();
        }

        public SlackData GetSlack()
        {
            Elemento? slack = GetElementsByTypeName("Slack").FirstOrDefault();

            if (slack == null)
                throw new InvalidOperationException("Elemento slack nao encontrado no modelo.");

            return new SlackData
            {
                Id = slack.Id.ToString(),
                Nome = ReadString(slack, "Nome"),
                Tensao = ReadDouble(slack, "Tensao"),
                Fases = ReadInt(slack, "Fases"),
                Barra = ReadString(slack, "Barra", "Nome")
            };
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
            return ReadString(elemento, names);
        }

        private static string ReadString(Elemento elemento, params string[] names)
        {
            return ReadValue<string>(elemento, names) ?? string.Empty;
        }

        private static int ReadInt(Elemento elemento, params string[] names)
        {
            return ReadValue<int>(elemento, names);
        }

        private static double ReadDouble(Elemento elemento, params string[] names)
        {
            return ReadValue<double>(elemento, names);
        }

        private static T? ReadValue<T>(Elemento elemento, params string[] names)
        {
            foreach (string name in names)
            {
                if (elemento.Parametros.TryGetValue(name, out Parameter? parametro) &&
                    parametro is Parameter<T> typed)
                {
                    return typed.Valor;
                }

                if (elemento.Tipo?.Parametros.TryGetValue(name, out parametro) == true &&
                    parametro is Parameter<T> typedTipo)
                {
                    return typedTipo.Valor;
                }
            }

            return default;
        }

        public class LoadData
        {
            public string Id { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public string Barra { get; set; } = string.Empty;

            public int Fases { get; set; }

            public double R { get; set; }

            public double X { get; set; }

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

        public class SlackData
        {
            public string Id { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public double Tensao { get; set; }

            public int Fases { get; set; }

            public string Barra { get; set; } = string.Empty;
        }
    }
}
