using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Araci.API;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services;

namespace Araci.DTOs
{
    public class CircuitBuilder
    {
        private readonly CoreApi _api;

        public CircuitBuilder(CoreApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public CircuitBuilder(EditorContext context)
            : this(new CoreApi(context))
        {
        }

        public CircuitBuilder(AraciDocument document)
            : this(new CoreApi(document))
        {
        }

        public CircuitDto Build()
        {
            return new CircuitDto
            {
                Loads = BuildLoads(),
                Lines = BuildLines(),
                Transformers = BuildTransformers(),
                Generators = BuildGenerators(),
                Slack = BuildSlack()
            };
        }

        private IList<LoadDto> BuildLoads()
        {
            return _api.ObterElementos<Carga>()
                .Select(carga => new LoadDto
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

        private IList<LineDto> BuildLines()
        {
            return _api.ObterElementos<Cabo>()
                .Select(cabo => new LineDto
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

        private IList<TransformerDto> BuildTransformers()
        {
            return GetElementsByTypeName("Transformador", "Transformer")
                .Select(transformador => new TransformerDto
                {
                    Id = transformador.Id.ToString(),
                    Nome = ReadString(transformador, "Nome"),
                    Fases = ReadInt(transformador, "Fases"),
                    Enrolamentos = ReadInt(transformador, "Enrolamentos")
                })
                .ToList();
        }

        private IList<GeneratorDto> BuildGenerators()
        {
            return _api.ObterElementos<Gerador>()
                .Select(gerador => new GeneratorDto
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

        private SlackDto BuildSlack()
        {
            Elemento? slack = GetElementsByTypeName("Slack").FirstOrDefault()
                ?? _api.ObterElementos<Barra>().FirstOrDefault();

            if (slack == null)
                return new SlackDto();

            return new SlackDto
            {
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
            string valor = ReadString(elemento, names);

            if (!string.IsNullOrWhiteSpace(valor))
                return valor;

            return elemento is ITerminalOwner owner
                ? owner.Terminais.FirstOrDefault()?.Barra ?? string.Empty
                : string.Empty;
        }

        private static string ReadString(Elemento elemento, params string[] names)
        {
            object? value = ReadValue(elemento, names);

            return value?.ToString() ?? string.Empty;
        }

        private static int ReadInt(Elemento elemento, params string[] names)
        {
            object? value = ReadValue(elemento, names);

            if (value == null)
                return 0;

            if (value is int intValue)
                return intValue;

            return int.TryParse(
                value.ToString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int result)
                ? result
                : 0;
        }

        private static double ReadDouble(Elemento elemento, params string[] names)
        {
            object? value = ReadValue(elemento, names);

            if (value == null)
                return 0;

            if (value is double doubleValue)
                return doubleValue;

            return double.TryParse(
                value.ToString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double result)
                ? result
                : 0;
        }

        private static object? ReadValue(Elemento elemento, params string[] names)
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
    }
}
