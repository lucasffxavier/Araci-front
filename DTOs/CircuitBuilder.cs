using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Services;

namespace Araci.DTOs
{
    public class CircuitBuilder
    {
        private const double DefaultBaseKv = 12.47;

        private readonly ParameterReader _reader;

        public CircuitBuilder(ParameterReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public CircuitDto Build()
        {
            ValidarTopologia();

            IList<ParameterReader.ExternalSourceData> sins = _reader.GetSins();
            IList<ParameterReader.GeneratorData> generators = _reader.GetGenerators();
            ParameterReader.ExternalSourceData? slackSin = sins.FirstOrDefault();
            ParameterReader.GeneratorData? slackGenerator = generators.FirstOrDefault();

            if (slackSin == null && slackGenerator == null)
                throw new InvalidOperationException("Nenhuma fonte slack encontrada no circuito.");

            var dto = new CircuitDto
            {
                Loads = BuildLoads(),
                Lines = BuildLines(),
                Transformers = BuildTransformers(),
                Generators = BuildGenerators(slackSin != null ? generators : generators.Skip(1)),
                Slack = slackSin != null ? BuildSlack(slackSin) : BuildSlack(slackGenerator!)
            };

            Validar(dto);

            return dto;
        }

        private IList<LoadDto> BuildLoads()
        {
            return _reader.GetLoads()
                .Select(load => new LoadDto
                {
                    Id = load.Id,
                    Nome = SafeName(load.Nome, "Carga"),
                    Barra = SafeBus(load.Barra, load.Nome),
                    Fases = SafePhases(load.Fases),
                    R = load.R,
                    X = load.X,
                    PotenciaAtiva = load.PotenciaAtiva > 0 ? load.PotenciaAtiva : 800,
                    PotenciaReativa = load.PotenciaReativa >= 0 ? load.PotenciaReativa : 300,
                    Tensao = SafeKv(load.Tensao),
                    Conexao = string.IsNullOrWhiteSpace(load.Conexao) ? "Wye" : load.Conexao,
                    Modelo = load.Modelo > 0 ? load.Modelo : 1
                })
                .ToList();
        }

        private IList<LineDto> BuildLines()
        {
            return _reader.GetLines()
                .Select(line =>
                {
                    double r1 = line.R1 > 0 ? line.R1 : 0.1;
                    double x1 = line.X1 > 0 ? line.X1 : 0.2;

                    return new LineDto
                    {
                        Id = line.Id,
                        Nome = SafeName(line.Nome, "L1"),
                        Barra1 = line.Barra1?.Trim() ?? string.Empty,
                        Barra2 = line.Barra2?.Trim() ?? string.Empty,
                        Fases = SafePhases(line.Fases),
                        Comprimento = line.Comprimento > 0 ? line.Comprimento : 1,
                        R1 = r1,
                        X1 = x1,
                        R0 = line.R0 > 0 ? line.R0 : 3 * r1,
                        X0 = line.X0 > 0 ? line.X0 : 3 * x1,
                        C1 = line.C1 > 0 ? line.C1 : 3.4,
                        C0 = line.C0 > 0 ? line.C0 : 1.6
                    };
                })
                .ToList();
        }

        private IList<TransformerDto> BuildTransformers()
        {
            return _reader.GetTransformers()
                .Select(transformer => new TransformerDto
                {
                    Id = transformer.Id,
                    Nome = transformer.Nome,
                    Fases = SafePhases(transformer.Fases),
                    Enrolamentos = transformer.Enrolamentos > 0 ? transformer.Enrolamentos : 2,
                    BarraPrimario = transformer.BarraPrimario?.Trim() ?? string.Empty,
                    BarraSecundario = transformer.BarraSecundario?.Trim() ?? string.Empty,
                    TensaoPrimarioKV = SafeKv(transformer.TensaoPrimarioKV),
                    TensaoSecundarioKV = SafeKv(transformer.TensaoSecundarioKV),
                    PotenciaKVA = transformer.PotenciaKVA > 0 ? transformer.PotenciaKVA : 500,
                    RPercentual = transformer.RPercentual >= 0 ? transformer.RPercentual : 1,
                    XPercentual = transformer.XPercentual >= 0 ? transformer.XPercentual : 5,
                    LigacaoPrimario = string.IsNullOrWhiteSpace(transformer.LigacaoPrimario) ? "Wye" : transformer.LigacaoPrimario,
                    LigacaoSecundario = string.IsNullOrWhiteSpace(transformer.LigacaoSecundario) ? "Wye" : transformer.LigacaoSecundario
                })
                .ToList();
        }

        private static IList<GeneratorDto> BuildGenerators(IEnumerable<ParameterReader.GeneratorData> generators)
        {
            return generators
                .Select(generator => new GeneratorDto
                {
                    Id = generator.Id,
                    Nome = SafeName(generator.Nome, "Gerador"),
                    Barra = SafeBus(generator.Barra, generator.Nome),
                    Fases = SafePhases(generator.Fases),
                    Tensao = SafeKv(generator.Tensao),
                    Potencia = generator.Potencia > 0 ? generator.Potencia : 1000,
                    FP = generator.FP > 0 ? generator.FP : 0.98
                })
                .ToList();
        }

        private static SlackDto BuildSlack(ParameterReader.GeneratorData generator)
        {
            string nome = SafeName(generator.Nome, "GERADOR-001");

            return new SlackDto
            {
                Id = generator.Id,
                Nome = nome,
                Tensao = SafeKv(generator.Tensao),
                Fases = SafePhases(generator.Fases),
                Barra = SafeBus(generator.Barra, nome)
            };
        }

        private static SlackDto BuildSlack(ParameterReader.ExternalSourceData source)
        {
            string nome = SafeName(source.Nome, "SIN-001");

            return new SlackDto
            {
                Id = source.Id,
                Nome = nome,
                Tensao = SafeKv(source.Tensao),
                Fases = SafePhases(source.Fases),
                Barra = SafeBus(source.Barra, nome)
            };
        }

        private static void Validar(CircuitDto dto)
        {
            if (dto.Slack == null || string.IsNullOrWhiteSpace(dto.Slack.Barra))
                throw new InvalidOperationException("Nenhuma fonte slack encontrada no circuito.");

            foreach (LineDto line in dto.Lines)
            {
                if (string.IsNullOrWhiteSpace(line.Barra1) || string.IsNullOrWhiteSpace(line.Barra2))
                    throw new InvalidOperationException($"Cabo '{line.Nome}' sem barra origem/destino definida.");
            }

            if (!dto.Loads.Any())
                throw new InvalidOperationException("Nenhuma carga encontrada para simular o fluxo.");
        }

        private void ValidarTopologia()
        {
            TopologyValidationResult? result = _reader.ValidateTopology();

            if (result == null || result.IsValid)
                return;

            throw new InvalidOperationException(
                "Topologia invalida para simulacao:" +
                Environment.NewLine +
                result.FormatErrors());
        }

        private static string SafeName(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string SafeBus(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? SafeName(fallback, "BARRA-001")
                : value.Trim();
        }

        private static int SafePhases(int value)
        {
            return value > 0 ? value : 3;
        }

        private static double SafeKv(double value)
        {
            return value > 0 ? value : DefaultBaseKv;
        }
    }
}
