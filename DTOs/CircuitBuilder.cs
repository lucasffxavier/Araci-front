using System;
using System.Collections.Generic;
using System.Linq;

namespace Araci.DTOs
{
    public class CircuitBuilder
    {
        private readonly ParameterReader _reader;

        public CircuitBuilder(ParameterReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public CircuitDto Build()
        {
            var dto = new CircuitDto
            {
                Loads = BuildLoads(),
                Lines = BuildLines(),
                Transformers = BuildTransformers(),
                Generators = BuildGenerators(),
                Slack = BuildSlack()
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
                    Nome = load.Nome,
                    Barra = load.Barra,
                    Fases = load.Fases,
                    R = load.R,
                    X = load.X,
                    PotenciaAtiva = load.PotenciaAtiva,
                    PotenciaReativa = load.PotenciaReativa,
                    Conexao = load.Conexao,
                    Modelo = load.Modelo
                })
                .ToList();
        }

        private IList<LineDto> BuildLines()
        {
            return _reader.GetLines()
                .Select(line => new LineDto
                {
                    Id = line.Id,
                    Nome = line.Nome,
                    Barra1 = line.Barra1,
                    Barra2 = line.Barra2,
                    Fases = line.Fases,
                    Comprimento = line.Comprimento,
                    R1 = line.R1,
                    X1 = line.X1,
                    R0 = line.R0,
                    X0 = line.X0
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
                    Fases = transformer.Fases,
                    Enrolamentos = transformer.Enrolamentos
                })
                .ToList();
        }

        private IList<GeneratorDto> BuildGenerators()
        {
            return _reader.GetGenerators()
                .Select(generator => new GeneratorDto
                {
                    Id = generator.Id,
                    Nome = generator.Nome,
                    Barra = generator.Barra,
                    Fases = generator.Fases,
                    Tensao = generator.Tensao,
                    Potencia = generator.Potencia,
                    FP = generator.FP
                })
                .ToList();
        }

        private SlackDto BuildSlack()
        {
            ParameterReader.SlackData slack = _reader.GetSlack();

            return new SlackDto
            {
                Id = slack.Id,
                Nome = slack.Nome,
                Tensao = slack.Tensao,
                Fases = slack.Fases,
                Barra = slack.Barra
            };
        }

        private void Validar(CircuitDto dto)
        {
            if (dto.Slack == null || string.IsNullOrWhiteSpace(dto.Slack.Barra))
                throw new InvalidOperationException("Slack inválido.");

            if (dto.Lines.Any(l => string.IsNullOrWhiteSpace(l.Barra1) || string.IsNullOrWhiteSpace(l.Barra2)))
                throw new InvalidOperationException("Linha sem barras definidas.");

            if (!dto.Loads.Any() && !dto.Generators.Any())
                throw new InvalidOperationException("Circuito sem carga ou geração.");
        }
    }
}
