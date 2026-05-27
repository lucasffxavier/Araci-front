using System;
using System.Collections.Generic;
using System.IO;
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
                ("ElectricGraph Build nao altera Document", ElectricGraphBuildNaoAlteraDocument),
                ("Multiplos geradores preservam slack e restantes", MultiplosGeradoresPreservamSlack),
                ("Cabos em serie preservam orientacao", CabosEmSeriePreservamOrientacao),
                ("Ramificacao simples valida grafo e DTO", RamificacaoSimplesValidaGrafoEDto),
                ("Topologia maior nao altera Document", TopologiaMaiorNaoAlteraDocument),
                ("Ordem de linhas segue ordem do Document", OrdemDeLinhasSegueDocument),
                ("Persistencia preserva topologia simples", PersistenciaPreservaTopologiaSimples),
                ("Persistencia preserva ramificacao", PersistenciaPreservaRamificacao),
                ("DTO permanece equivalente apos reload", DtoPermaneceEquivalenteAposReload),
                ("IDs permanecem estaveis apos reload", IdsPermanecemEstaveisAposReload),
                ("Builds repetidos apos reload nao alteram Document", BuildsRepetidosAposReloadNaoAlteramDocument),
                ("SIN pode ser criado e entra no Document", SinPodeSerCriadoEEntraNoDocument),
                ("SIN aparece no ElectricGraph", SinApareceNoElectricGraph),
                ("SIN preserva Id apos reload", SinPreservaIdAposReload),
                ("Cabo conectado ao SIN preserva conexoes", CaboConectadoAoSinPreservaConexoes),
                ("DTOs existentes com gerador continuam funcionando", DtosComGeradorContinuamFuncionando)
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

        private static void MultiplosGeradoresPreservamSlack()
        {
            var document = new AraciDocument();
            Gerador generatorA = CreateGenerator("GERADOR-A", 1100, 0.91);
            Gerador generatorB = CreateGenerator("GERADOR-B", 730, 0.87);
            Carga load = CreateLoad("CARGA-MULTI", 510, 170);

            document.AdicionarElemento(generatorA);
            document.AdicionarElemento(generatorB);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(generatorA, load, "L-A", 1.1));
            document.AdicionarElemento(CreateCable(generatorB, load, "L-B", 1.2));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            Assert(dto.Slack != null, "SlackDto deve existir.");
            AssertEqual(generatorA.Id.ToString(), dto.Slack!.Id, "Slack deve usar primeiro gerador");
            AssertEqual(generatorA.Nome, dto.Slack.Nome, "SlackDto.Nome");
            AssertEqual(generatorA.Nome, dto.Slack.Barra, "SlackDto.Barra");
            AssertEqual(1, dto.Generators.Count, "Geradores restantes");

            GeneratorDto generator = dto.Generators[0];
            AssertEqual(generatorB.Id.ToString(), generator.Id, "GeneratorDto.Id");
            AssertEqual(generatorB.Nome, generator.Nome, "GeneratorDto.Nome");
            AssertEqual(generatorB.Nome, generator.Barra, "GeneratorDto.Barra");
            AssertEqual(730, generator.Potencia, "GeneratorDto.Potencia");
            AssertEqual(0.87, generator.FP, "GeneratorDto.FP");
            Assert(!dto.Generators.Any(g => g.Id == generatorA.Id.ToString()), "Slack nao deve aparecer em Generators.");
        }

        private static void CabosEmSeriePreservamOrientacao()
        {
            var document = new AraciDocument();
            Gerador generator = CreateGenerator("GERADOR-SERIE", 1200, 0.95);
            Barra bar = CreateBar("BARRA-SERIE");
            Carga load = CreateLoad("CARGA-SERIE", 430, 140);
            Cabo line1 = CreateCable(generator, 0, bar, 0, "L-S01", 4.1);
            Cabo line2 = CreateCable(bar, 1, load, 0, "L-S02", 5.2);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(load);
            document.AdicionarElemento(line1);
            document.AdicionarElemento(line2);

            IList<ParameterReader.LineData> lines = new ParameterReader(document).GetLines();

            AssertEqual(2, lines.Count, "Quantidade de linhas em serie");
            AssertLine(lines[0], line1, generator.Nome, bar.Nome, "Linha serie 1");
            AssertLine(lines[1], line2, bar.Nome, load.Nome, "Linha serie 2");
        }

        private static void RamificacaoSimplesValidaGrafoEDto()
        {
            var document = new AraciDocument();
            Gerador generator = CreateGenerator("GERADOR-RAMO", 1300, 0.94);
            Barra bar = CreateBar("BARRA-RAMO");
            Carga load1 = CreateLoad("CARGA-R1", 320, 90);
            Carga load2 = CreateLoad("CARGA-R2", 280, 85);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(load1);
            document.AdicionarElemento(load2);
            document.AdicionarElemento(CreateCable(generator, 0, bar, 0, "L-01", 1.0));
            document.AdicionarElemento(CreateCable(bar, 1, load1, 0, "L-02", 1.1));
            document.AdicionarElemento(CreateCable(bar, 2, load2, 0, "L-03", 1.2));

            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            IReadOnlyList<ElectricGraphNode> neighbors = graph.GetNeighbors(bar.Id.ToString());
            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(4, graph.Nodes.Count, "Quantidade de nos da ramificacao");
            AssertEqual(3, graph.Edges.Count, "Quantidade de arestas da ramificacao");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Arestas invalidas da ramificacao");
            AssertContainsNode(neighbors, generator, "Vizinho gerador");
            AssertContainsNode(neighbors, load1, "Vizinho carga 1");
            AssertContainsNode(neighbors, load2, "Vizinho carga 2");
            AssertEqual(2, dto.Loads.Count, "Cargas no DTO ramificado");
            AssertEqual(3, dto.Lines.Count, "Linhas no DTO ramificado");
        }

        private static void TopologiaMaiorNaoAlteraDocument()
        {
            AraciDocument document = CreateBranchDocument();
            int countBefore = document.Elementos.Count;

            ElectricGraph graph1 = new ElectricGraphBuilder(document).Build();
            ElectricGraph graph2 = new ElectricGraphBuilder(document).Build();
            ElectricGraph graph3 = new ElectricGraphBuilder(document).Build();

            AssertEqual(countBefore, document.Elementos.Count, "Contagem apos builds repetidos");
            AssertEqual(graph1.Nodes.Count, graph2.Nodes.Count, "Nodes build 1/2");
            AssertEqual(graph2.Nodes.Count, graph3.Nodes.Count, "Nodes build 2/3");
            AssertEqual(graph1.Edges.Count, graph2.Edges.Count, "Edges build 1/2");
            AssertEqual(graph2.Edges.Count, graph3.Edges.Count, "Edges build 2/3");
        }

        private static void OrdemDeLinhasSegueDocument()
        {
            AraciDocument document = CreateBranchDocument();
            IList<ParameterReader.LineData> lines = new ParameterReader(document).GetLines();

            AssertEqual(3, lines.Count, "Quantidade de linhas ordenadas");
            AssertEqual("L-01", lines[0].Nome, "Linha 1 por ordem do Document");
            AssertEqual("L-02", lines[1].Nome, "Linha 2 por ordem do Document");
            AssertEqual("L-03", lines[2].Nome, "Linha 3 por ordem do Document");
        }

        private static void PersistenciaPreservaTopologiaSimples()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            AraciDocument loaded = SaveAndLoad(circuit.Document);

            AssertEqual(3, loaded.Elementos.Count, "Quantidade de elementos recarregados");

            Gerador generator = FindById<Gerador>(loaded, circuit.Generator.Id);
            Carga load = FindById<Carga>(loaded, circuit.Load.Id);
            Cabo cable = FindById<Cabo>(loaded, circuit.Cable.Id);

            AssertEqual(circuit.Generator.Nome, generator.Nome, "Nome do gerador");
            AssertEqual(circuit.Load.Nome, load.Nome, "Nome da carga");
            AssertCablePersisted(circuit.Cable, cable, "Cabo simples");
            AssertEqual(circuit.Load.PotenciaAtiva, load.PotenciaAtiva, "Potencia ativa da carga");
            AssertEqual(circuit.Load.PotenciaReativa, load.PotenciaReativa, "Potencia reativa da carga");
            AssertEqual(circuit.Generator.PotenciaAtiva, generator.PotenciaAtiva, "Potencia ativa do gerador");
        }

        private static void PersistenciaPreservaRamificacao()
        {
            AraciDocument loaded = SaveAndLoad(CreateBranchDocument());
            Barra bar = loaded.Elementos.OfType<Barra>().Single();
            ElectricGraph graph = new ElectricGraphBuilder(loaded).Build();
            IReadOnlyList<ElectricGraphNode> neighbors = graph.GetNeighbors(bar.Id.ToString());

            AssertEqual(4, graph.Nodes.Count, "Nodes apos reload");
            AssertEqual(3, graph.Edges.Count, "Edges apos reload");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Edges invalidas apos reload");
            AssertContainsNode(neighbors, loaded.Elementos.OfType<Gerador>().Single(), "Vizinho gerador apos reload");

            foreach (Carga load in loaded.Elementos.OfType<Carga>())
                AssertContainsNode(neighbors, load, $"Vizinho {load.Nome} apos reload");
        }

        private static void DtoPermaneceEquivalenteAposReload()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            CircuitDto before = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();
            CircuitDto after = new CircuitBuilder(new ParameterReader(SaveAndLoad(circuit.Document))).Build();

            AssertEqual(before.Slack!.Id, after.Slack!.Id, "Slack.Id apos reload");
            AssertEqual(before.Slack.Nome, after.Slack.Nome, "Slack.Nome apos reload");
            AssertEqual(before.Slack.Barra, after.Slack.Barra, "Slack.Barra apos reload");
            AssertEqual(before.Loads.Count, after.Loads.Count, "Quantidade de cargas apos reload");
            AssertEqual(before.Lines.Count, after.Lines.Count, "Quantidade de linhas apos reload");
            AssertEqual(before.Loads[0].Id, after.Loads[0].Id, "Load.Id apos reload");
            AssertEqual(before.Loads[0].Nome, after.Loads[0].Nome, "Load.Nome apos reload");
            AssertEqual(before.Loads[0].Barra, after.Loads[0].Barra, "Load.Barra apos reload");
            AssertEqual(before.Lines[0].Id, after.Lines[0].Id, "Line.Id apos reload");
            AssertEqual(before.Lines[0].Nome, after.Lines[0].Nome, "Line.Nome apos reload");
            AssertEqual(before.Lines[0].Barra1, after.Lines[0].Barra1, "Line.Barra1 apos reload");
            AssertEqual(before.Lines[0].Barra2, after.Lines[0].Barra2, "Line.Barra2 apos reload");
            AssertEqual(before.Lines[0].Comprimento, after.Lines[0].Comprimento, "Line.Comprimento apos reload");
        }

        private static void IdsPermanecemEstaveisAposReload()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            AraciDocument loaded = SaveAndLoad(circuit.Document);
            Cabo cable = FindById<Cabo>(loaded, circuit.Cable.Id);

            AssertEqual(circuit.Cable.Id, cable.Id, "Id do cabo");
            AssertEqual(circuit.Generator.Id.ToString(), cable.OrigemId, "OrigemId apos reload");
            AssertEqual(circuit.Load.Id.ToString(), cable.DestinoId, "DestinoId apos reload");
            AssertEqual(circuit.Cable.OrigemTerminalId, cable.OrigemTerminalId, "OrigemTerminalId apos reload");
            AssertEqual(circuit.Cable.DestinoTerminalId, cable.DestinoTerminalId, "DestinoTerminalId apos reload");
            _ = FindById<Gerador>(loaded, circuit.Generator.Id);
            _ = FindById<Carga>(loaded, circuit.Load.Id);
        }

        private static void BuildsRepetidosAposReloadNaoAlteramDocument()
        {
            AraciDocument loaded = SaveAndLoad(CreateBranchDocument());
            int countBefore = loaded.Elementos.Count;

            ElectricGraph graph1 = new ElectricGraphBuilder(loaded).Build();
            ElectricGraph graph2 = new ElectricGraphBuilder(loaded).Build();
            ElectricGraph graph3 = new ElectricGraphBuilder(loaded).Build();

            AssertEqual(countBefore, loaded.Elementos.Count, "Contagem apos reload e builds");
            AssertEqual(graph1.Nodes.Count, graph2.Nodes.Count, "Nodes reload build 1/2");
            AssertEqual(graph2.Nodes.Count, graph3.Nodes.Count, "Nodes reload build 2/3");
            AssertEqual(graph1.Edges.Count, graph2.Edges.Count, "Edges reload build 1/2");
            AssertEqual(graph2.Edges.Count, graph3.Edges.Count, "Edges reload build 2/3");
        }

        private static void SinPodeSerCriadoEEntraNoDocument()
        {
            var context = new EditorContext();
            Sin sin = context.ElementoFactory.CriarSin();

            context.Document.AdicionarElemento(sin);

            AssertEqual(1, context.Document.Elementos.Count, "Quantidade no Document");
            Assert(context.Document.Elementos.Contains(sin), "SIN deve estar no Document.");
            AssertEqual("Sin", context.Elements.GetKind(sin), "Kind do SIN");
            Assert(sin.Terminais.Count > 0, "SIN deve possuir terminal conectavel.");
        }

        private static void SinApareceNoElectricGraph()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-GRAFO");

            document.AdicionarElemento(sin);

            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            ElectricGraphNode? node = graph.FindNode(sin.Id.ToString());

            Assert(node != null, "SIN deve aparecer como no do ElectricGraph.");
            AssertEqual(sin.Nome, node!.Name, "Nome do no SIN");
            AssertEqual(1, node.Terminals.Count, "Terminais do no SIN");
        }

        private static void SinPreservaIdAposReload()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RELOAD");

            document.AdicionarElemento(sin);

            AraciDocument loaded = SaveAndLoad(document);
            Sin loadedSin = FindById<Sin>(loaded, sin.Id);

            AssertEqual(sin.Id, loadedSin.Id, "Id do SIN");
            AssertEqual(sin.Nome, loadedSin.Nome, "Nome do SIN");
            AssertEqual(sin.Barra, loadedSin.Barra, "Barra do SIN");
            AssertEqual(sin.Terminais[0].Id, loadedSin.Terminais[0].Id, "Terminal do SIN");
        }

        private static void CaboConectadoAoSinPreservaConexoes()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-CABO");
            Carga load = CreateLoad("CARGA-SIN", 350, 120);
            Cabo cable = CreateCable(sin, 0, load, 0, "L-SIN", 1.5);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            AraciDocument loaded = SaveAndLoad(document);
            Cabo loadedCable = FindById<Cabo>(loaded, cable.Id);
            ElectricGraph graph = new ElectricGraphBuilder(loaded).Build();

            AssertEqual(sin.Id.ToString(), loadedCable.OrigemId, "OrigemId SIN apos reload");
            AssertEqual(load.Id.ToString(), loadedCable.DestinoId, "DestinoId carga apos reload");
            AssertEqual(cable.OrigemTerminalId, loadedCable.OrigemTerminalId, "OrigemTerminalId SIN apos reload");
            AssertEqual(cable.DestinoTerminalId, loadedCable.DestinoTerminalId, "DestinoTerminalId carga apos reload");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Grafo com SIN nao deve ter arestas invalidas");
        }

        private static void DtosComGeradorContinuamFuncionando()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            circuit.Document.AdicionarElemento(CreateSin("SIN-COMPAT"));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();

            Assert(dto.Slack != null, "SlackDto deve continuar existindo.");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Slack!.Id, "Slack deve continuar usando gerador");
            AssertEqual(1, dto.Loads.Count, "Quantidade de cargas com SIN adicional");
            AssertEqual(1, dto.Lines.Count, "Quantidade de linhas com SIN adicional");
        }

        private static SimpleCircuit CreateSimpleCircuit()
        {
            var document = new AraciDocument();
            Gerador generator = CreateGenerator("GERADOR-TESTE", 1250, 0.93);
            Carga load = CreateLoad("CARGA-TESTE", 650, 210);

            Cabo cable = CreateCable(generator, load, "L-TESTE", 2.75);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            return new SimpleCircuit(document, generator, load, cable);
        }

        private static AraciDocument CreateBranchDocument()
        {
            var document = new AraciDocument();
            Gerador generator = CreateGenerator("GERADOR-BRANCH", 1300, 0.94);
            Barra bar = CreateBar("BARRA-BRANCH");
            Carga load1 = CreateLoad("CARGA-B1", 320, 90);
            Carga load2 = CreateLoad("CARGA-B2", 280, 85);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(load1);
            document.AdicionarElemento(load2);
            document.AdicionarElemento(CreateCable(generator, 0, bar, 0, "L-01", 1.0));
            document.AdicionarElemento(CreateCable(bar, 1, load1, 0, "L-02", 1.1));
            document.AdicionarElemento(CreateCable(bar, 2, load2, 0, "L-03", 1.2));

            return document;
        }

        private static Gerador CreateGenerator(string name, double power, double fp)
        {
            var generator = new Gerador
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoGerador
                {
                    TensaoKV = 13.8,
                    FatorPotencia = fp
                },
                PosicaoX = 100,
                PosicaoY = 100,
                PotenciaAtiva = power,
                TensaoLinha = "13.8"
            };

            generator.AtualizarTerminais(80, 80);

            return generator;
        }

        private static Carga CreateLoad(string name, double activePower, double reactivePower)
        {
            var load = new Carga
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoCarga
                {
                    Tensao = "13.8",
                    Conexao = "Wye",
                    ModeloCarga = 1
                },
                PosicaoX = 300,
                PosicaoY = 100,
                PotenciaAtiva = activePower,
                PotenciaReativa = reactivePower,
                TensaoLinha = "13.8"
            };

            load.AtualizarTerminais(80);

            return load;
        }

        private static Barra CreateBar(string name)
        {
            return new Barra
            {
                Nome = name,
                PosicaoX = 200,
                PosicaoY = 100
            };
        }

        private static Sin CreateSin(string name)
        {
            var sin = new Sin
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoSin
                {
                    TensaoKV = 13.8,
                    Fases = 3,
                    PotenciaCurtoMVA = 500,
                    RelacaoXR = 10
                },
                PosicaoX = 80,
                PosicaoY = 80,
                TensaoLinha = "13.8"
            };

            sin.AtualizarTerminais(80, 80);

            return sin;
        }

        private static Cabo CreateCable(
            Gerador generator,
            Carga load,
            string name,
            double length)
        {
            return CreateCable(generator, 0, load, 0, name, length);
        }

        private static Cabo CreateCable(
            Elemento from,
            int fromTerminalIndex,
            Elemento to,
            int toTerminalIndex,
            string name,
            double length)
        {
            Terminal fromTerminal = GetTerminal(from, fromTerminalIndex);
            Terminal toTerminal = GetTerminal(to, toTerminalIndex);
            var cable = new Cabo
            {
                Nome = name,
                OrigemId = from.Id.ToString(),
                OrigemTerminalId = fromTerminal.Id,
                DestinoId = to.Id.ToString(),
                DestinoTerminalId = toTerminal.Id,
                Comprimento = length
            };

            cable.DefinirOrigem(fromTerminal.Posicao);
            cable.DefinirDestino(toTerminal.Posicao);
            cable.Vertices.Add(fromTerminal.Posicao);
            cable.Vertices.Add(toTerminal.Posicao);

            return cable;
        }

        private static Terminal GetTerminal(Elemento elemento, int index)
        {
            if (elemento is not ITerminalOwner owner)
                throw new InvalidOperationException($"Elemento '{elemento.Nome}' nao possui terminais.");

            return owner.Terminais[index];
        }

        private static void AssertLine(
            ParameterReader.LineData line,
            Cabo cable,
            string expectedFrom,
            string expectedTo,
            string name)
        {
            AssertEqual(cable.Id.ToString(), line.Id, $"{name}.Id");
            AssertEqual(cable.Nome, line.Nome, $"{name}.Nome");
            AssertEqual(expectedFrom, line.Barra1, $"{name}.Barra1");
            AssertEqual(expectedTo, line.Barra2, $"{name}.Barra2");
        }

        private static void AssertContainsNode(
            IEnumerable<ElectricGraphNode> nodes,
            Elemento expected,
            string name)
        {
            bool contains = nodes.Any(n =>
                string.Equals(n.ElementId, expected.Id.ToString(), StringComparison.OrdinalIgnoreCase));

            Assert(contains, $"{name}: no '{expected.Nome}' nao encontrado.");
        }

        private static AraciDocument SaveAndLoad(AraciDocument document)
        {
            string path = Path.Combine(Path.GetTempPath(), $"araci-check-{Guid.NewGuid():N}.araci");

            try
            {
                var source = new EditorContext();

                foreach (Elemento elemento in document.Elementos)
                    source.Document.AdicionarElemento(elemento);

                source.Projects.Salvar(path);

                var target = new EditorContext();
                target.Projects.Abrir(path);

                return target.Document;
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private static T FindById<T>(AraciDocument document, Guid id)
            where T : Elemento
        {
            T? elemento = document.Elementos.OfType<T>().FirstOrDefault(e => e.Id == id);

            if (elemento == null)
                throw new InvalidOperationException($"Elemento {typeof(T).Name} '{id}' nao encontrado.");

            return elemento;
        }

        private static void AssertCablePersisted(Cabo expected, Cabo actual, string name)
        {
            AssertEqual(expected.Nome, actual.Nome, $"{name}.Nome");
            AssertEqual(expected.OrigemId, actual.OrigemId, $"{name}.OrigemId");
            AssertEqual(expected.DestinoId, actual.DestinoId, $"{name}.DestinoId");
            AssertEqual(expected.OrigemTerminalId, actual.OrigemTerminalId, $"{name}.OrigemTerminalId");
            AssertEqual(expected.DestinoTerminalId, actual.DestinoTerminalId, $"{name}.DestinoTerminalId");
            AssertEqual(expected.Comprimento, actual.Comprimento, $"{name}.Comprimento");
            AssertEqual(expected.Vertices.Count, actual.Vertices.Count, $"{name}.Vertices.Count");

            for (int i = 0; i < expected.Vertices.Count; i++)
            {
                AssertEqual(expected.Vertices[i].X, actual.Vertices[i].X, $"{name}.Vertices[{i}].X");
                AssertEqual(expected.Vertices[i].Y, actual.Vertices[i].Y, $"{name}.Vertices[{i}].Y");
            }
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
