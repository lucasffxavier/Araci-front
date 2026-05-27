using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.API;
using Araci.Core.Documents;
using Araci.DTOs;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services;

namespace Araci.TechnicalChecks
{
    internal static class Program
    {
        private static int Main()
        {
            var tests = new (string Name, Action Run)[]
            {
                ("Circuito simples preserva DTOs via ElectricGraph", CircuitoSimplesPreservaDtos),
                ("ParameterReader CoreApi usa fallback sem ElectricGraph", CoreApiUsaFallbackSemElectricGraph),
                ("Cabo invalido bloqueia DTO final", CaboInvalidoBloqueiaDto),
                ("Cabo duplicado gera erro topologico", CaboDuplicadoGeraErro),
                ("ElectricGraph Build nao altera Document", ElectricGraphBuildNaoAlteraDocument)
            };

            var failures = new List<string>();

            foreach ((string name, Action run) in tests)
            {
                try
                {
                    run();
                    Console.WriteLine($"PASS {name}");
                }
                catch (Exception ex)
                {
                    failures.Add($"{name}: {ex.Message}");
                    Console.WriteLine($"FAIL {name}: {ex.Message}");
                }
            }

            if (failures.Count == 0)
                return 0;

            Console.WriteLine();
            Console.WriteLine("Falhas:");

            foreach (string failure in failures)
                Console.WriteLine($"- {failure}");

            return 1;
        }

        private static void CircuitoSimplesPreservaDtos()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            ParameterReader reader = new(circuit.Document);
            CircuitDto dto = new CircuitBuilder(reader).Build();

            Assert(dto.Slack != null, "SlackDto deve existir.");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Slack!.Id, "SlackDto.Id");
            AssertEqual(circuit.Generator.Nome, dto.Slack.Nome, "SlackDto.Nome");
            AssertEqual(circuit.Generator.Nome, dto.Slack.Barra, "SlackDto.Barra");
            AssertEqual(3, dto.Slack.Fases, "SlackDto.Fases");
            AssertEqual(13.8, dto.Slack.Tensao, "SlackDto.Tensao");

            AssertEqual(1, dto.Loads.Count, "Quantidade de cargas");
            LoadDto load = dto.Loads[0];
            AssertEqual(circuit.Load.Id.ToString(), load.Id, "LoadDto.Id");
            AssertEqual(circuit.Load.Nome, load.Nome, "LoadDto.Nome");
            AssertEqual(circuit.Load.Nome, load.Barra, "LoadDto.Barra");
            AssertEqual(3, load.Fases, "LoadDto.Fases");
            AssertEqual(650, load.PotenciaAtiva, "LoadDto.PotenciaAtiva");
            AssertEqual(210, load.PotenciaReativa, "LoadDto.PotenciaReativa");
            AssertEqual(13.8, load.Tensao, "LoadDto.Tensao");
            AssertEqual("Wye", load.Conexao, "LoadDto.Conexao");
            AssertEqual(1, load.Modelo, "LoadDto.Modelo");

            AssertEqual(1, dto.Lines.Count, "Quantidade de cabos");
            LineDto line = dto.Lines[0];
            AssertEqual(circuit.Cable.Id.ToString(), line.Id, "LineDto.Id");
            AssertEqual(circuit.Cable.Nome, line.Nome, "LineDto.Nome");
            AssertEqual(circuit.Generator.Nome, line.Barra1, "LineDto.Barra1");
            AssertEqual(circuit.Load.Nome, line.Barra2, "LineDto.Barra2");
            AssertEqual(2.75, line.Comprimento, "LineDto.Comprimento");

            IList<ParameterReader.GeneratorData> generators = reader.GetGenerators();
            AssertEqual(1, generators.Count, "Quantidade de GeneratorData");
            AssertEqual(circuit.Generator.Id.ToString(), generators[0].Id, "GeneratorData.Id");
            AssertEqual(circuit.Generator.Nome, generators[0].Nome, "GeneratorData.Nome");
            AssertEqual(circuit.Generator.Nome, generators[0].Barra, "GeneratorData.Barra");
            AssertEqual(1250, generators[0].Potencia, "GeneratorData.Potencia");
            AssertEqual(0.93, generators[0].FP, "GeneratorData.FP");
        }

        private static void CoreApiUsaFallbackSemElectricGraph()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            ParameterReader reader = new(new CoreApi(circuit.Document));

            ParameterReader.LoadData load = reader.GetLoads().Single();
            ParameterReader.GeneratorData generator = reader.GetGenerators().Single();
            ParameterReader.LineData line = reader.GetLines().Single();

            AssertEqual(circuit.Load.Nome, load.Barra, "LoadData.Barra fallback");
            AssertEqual(circuit.Generator.Nome, generator.Barra, "GeneratorData.Barra fallback");
            AssertEqual(circuit.Generator.Nome, line.Barra1, "LineData.Barra1 fallback");
            AssertEqual(circuit.Load.Nome, line.Barra2, "LineData.Barra2 fallback");
        }

        private static void CaboInvalidoBloqueiaDto()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            circuit.Cable.DestinoId = Guid.NewGuid().ToString();

            ParameterReader reader = new(circuit.Document);
            TopologyValidationResult? result = reader.ValidateTopology();

            Assert(result != null && !result.IsValid, "Validador deve detectar cabo invalido.");
            AssertContains(result!.FormatErrors(), "DestinoId inexistente", "Erro de cabo invalido");

            AssertThrows<InvalidOperationException>(
                () => new CircuitBuilder(reader).Build(),
                "CircuitBuilder deve bloquear DTO final invalido.");
        }

        private static void CaboDuplicadoGeraErro()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            Cabo duplicate = CreateCable(circuit.Generator, circuit.Load, "L-DUP", 3.1);
            circuit.Document.AdicionarElemento(duplicate);

            TopologyValidationResult result = new TopologyValidator(circuit.Document).Validate();

            Assert(!result.IsValid, "Validador deve reprovar cabo duplicado.");
            AssertContains(result.FormatErrors(), "duplicado", "Erro de duplicidade");
            AssertEqual(4, circuit.Document.Elementos.Count, "Cabos duplicados nao devem ser removidos.");
        }

        private static void ElectricGraphBuildNaoAlteraDocument()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            int countBefore = circuit.Document.Elementos.Count;

            ElectricGraph graph = new ElectricGraphBuilder(circuit.Document).Build();

            AssertEqual(countBefore, circuit.Document.Elementos.Count, "Contagem do Document");
            AssertEqual(2, graph.Nodes.Count, "Quantidade de nos do grafo");
            AssertEqual(1, graph.Edges.Count, "Quantidade de arestas do grafo");
        }

        private static SimpleCircuit CreateSimpleCircuit()
        {
            var document = new AraciDocument();
            var generator = new Gerador
            {
                Nome = "GERADOR-TESTE",
                Barra = "GERADOR-TESTE",
                Tipo = new TipoGerador
                {
                    TensaoKV = 13.8,
                    FatorPotencia = 0.93
                },
                PosicaoX = 100,
                PosicaoY = 100,
                PotenciaAtiva = 1250,
                TensaoLinha = "13.8"
            };

            var load = new Carga
            {
                Nome = "CARGA-TESTE",
                Barra = "CARGA-TESTE",
                Tipo = new TipoCarga
                {
                    Tensao = "13.8",
                    Conexao = "Wye",
                    ModeloCarga = 1
                },
                PosicaoX = 300,
                PosicaoY = 100,
                PotenciaAtiva = 650,
                PotenciaReativa = 210,
                TensaoLinha = "13.8"
            };

            Cabo cable = CreateCable(generator, load, "L-TESTE", 2.75);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            return new SimpleCircuit(document, generator, load, cable);
        }

        private static Cabo CreateCable(
            Gerador generator,
            Carga load,
            string name,
            double length)
        {
            Terminal generatorTerminal = generator.Terminais[0];
            Terminal loadTerminal = load.Terminais[0];
            var cable = new Cabo
            {
                Nome = name,
                OrigemId = generator.Id.ToString(),
                OrigemTerminalId = generatorTerminal.Id,
                DestinoId = load.Id.ToString(),
                DestinoTerminalId = loadTerminal.Id,
                Comprimento = length
            };

            cable.DefinirOrigem(new Point(100, 100));
            cable.DefinirDestino(new Point(300, 100));
            cable.Vertices.Add(new Point(100, 100));
            cable.Vertices.Add(new Point(300, 100));

            return cable;
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void AssertEqual<T>(T expected, T actual, string name)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new InvalidOperationException($"{name}: esperado '{expected}', obtido '{actual}'.");
        }

        private static void AssertEqual(double expected, double actual, string name)
        {
            if (Math.Abs(expected - actual) > 0.000001)
                throw new InvalidOperationException($"{name}: esperado '{expected}', obtido '{actual}'.");
        }

        private static void AssertContains(string text, string expected, string name)
        {
            if (!text.Contains(expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"{name}: texto nao contem '{expected}'. Texto: {text}");
        }

        private static void AssertThrows<TException>(Action action, string name)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }

            throw new InvalidOperationException($"{name}: excecao {typeof(TException).Name} nao foi lancada.");
        }

        private sealed record SimpleCircuit(
            AraciDocument Document,
            Gerador Generator,
            Carga Load,
            Cabo Cable);
    }
}
