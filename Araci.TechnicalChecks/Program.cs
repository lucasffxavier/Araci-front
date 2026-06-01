using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Araci.API;
using Araci.Applications.Diagrama;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Rendering;
using Araci.DTOs;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services;
using Araci.ViewModels;

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
                ("Elementos existentes permanecem eletricos", ElementosExistentesPermanecemEletricos),
                ("ElectricGraph Build nao altera Document", ElectricGraphBuildNaoAlteraDocument),
                ("ElectricGraph inclui eletricos e ignora anotativo", ElectricGraphIncluiEletricosEIgnoraAnotativo),
                ("DTO permanece identico com anotativo no Document", DtoPermaneceIdenticoComAnotativoNoDocument),
                ("OperationalGraph ignora anotativo", OperationalGraphIgnoraAnotativo),
                ("TopologyValidator ignora anotativo", TopologyValidatorIgnoraAnotativo),
                ("Classificacao eletrica nao depende de nome tipo ou SVG", ClassificacaoEletricaNaoDependeDeNomeTipoOuSvg),
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
                ("Cabos conectados aos terminais do SIN preservam conexoes", CabosConectadosAosTerminaisDoSinPreservamConexoes),
                ("DTOs sem SIN mantem gerador como slack", DtosSemSinMantemGeradorComoSlack),
                ("SIN com gerador vira slack preferencial", SinComGeradorViraSlackPreferencial),
                ("SIN com gerador preserva GeneratorDto real", SinComGeradorPreservaGeneratorDtoReal),
                ("SIN com multiplos geradores preserva todos em Generators", SinComMultiplosGeradoresPreservaTodosGenerators),
                ("Reload preserva GeneratorDto real com SIN", ReloadPreservaGeneratorDtoRealComSin),
                ("Circuito eolico simplificado preserva GeneratorDto", CircuitoEolicoSimplificadoPreservaGeneratorDto),
                ("Multiplos SIN usam primeiro do Document como slack", MultiplosSinUsamPrimeiroDoDocumentComoSlack),
                ("Reload com SIN mantem slack baseado no SIN", ReloadComSinMantemSlackBaseadoNoSin),
                ("OperationalGraph energiza SIN cabo e carga", OperationalGraphEnergizaSinCaboECarga),
                ("OperationalGraph mantem carga isolada desenergizada", OperationalGraphMantemCargaIsoladaDesenergizada),
                ("OperationalGraph energiza ramificacao com barra", OperationalGraphEnergizaRamificacaoComBarra),
                ("OperationalGraph nao propaga por cabo invalido", OperationalGraphNaoPropagaPorCaboInvalido),
                ("OperationalGraph usa gerador como fallback sem SIN", OperationalGraphUsaGeradorComoFallbackSemSin),
                ("OperationalGraph sem fonte nao energiza nos", OperationalGraphSemFonteNaoEnergizaNos),
                ("OperationalGraph rebuild repetido nao altera Document", OperationalGraphRebuildRepetidoNaoAlteraDocument),
                ("OperationalGraph apos reload preserva resultado", OperationalGraphAposReloadPreservaResultado),
                ("Transformador minimo possui terminais primario e secundario", TransformadorMinimoPossuiTerminais),
                ("Transformador aparece no ElectricGraph", TransformadorApareceNoElectricGraph),
                ("Transformador preserva conexoes apos reload", TransformadorPreservaConexoesAposReload),
                ("Transformador entra no DTO minimo", TransformadorEntraNoDtoMinimo),
                ("Transformador usa centro com geometria propria", TransformadorUsaCentroComGeometriaPropria),
                ("Reload preserva DTO detalhado do transformador", ReloadPreservaDtoDetalhadoTransformador),
                ("CircuitDto preserva parametros reais de SIN transformador e carga", CircuitDtoPreservaParametrosReaisSinTransformadorCarga),
                ("DTOs antigos/default preservam SIN e carga", DtosAntigosDefaultPreservamSinECarga),
                ("TopologyValidator aceita SIN transformador e carga sem gerador", TopologyValidatorAceitaSinTransformadorCargaSemGerador),
                ("TopologyValidator aceita gerador legado sem SIN", TopologyValidatorAceitaGeradorLegadoSemSin),
                ("TopologyValidator sem fonte slack falha com mensagem clara", TopologyValidatorSemFonteSlackFalhaComMensagemClara),
                ("TerminalEndpoint identifica conexao por valor", TerminalEndpointIdentificaConexaoPorValor),
                ("TerminalPlacement usa pivo central", TerminalPlacementUsaPivoCentral),
                ("TerminalPlacement ToLocal inverte ToWorld", TerminalPlacementToLocalInverteToWorld),
                ("Rotacao recalcula terminal por posicao local", RotacaoRecalculaTerminalPorPosicaoLocal),
                ("Carga rotacionada alinha terminal com pivo central", CargaRotacionadaAlinhaTerminalComPivoCentral),
                ("Gerador rotacionado alinha terminais com pivo central", GeradorRotacionadoAlinhaTerminaisComPivoCentral),
                ("SIN rotacionado alinha terminais com pivo central", SinRotacionadoAlinhaTerminaisComPivoCentral),
                ("Transformador rotacionado alinha terminais com pivo central", TransformadorRotacionadoAlinhaTerminaisComPivoCentral),
                ("Barra rotacionada alinha terminais com pivo central", BarraRotacionadaAlinhaTerminaisComPivoCentral),
                ("ElectricGraph BFS percorre por conexoes validas", ElectricGraphBfsPercorreConexoesValidas),
                ("Rotacao +90 atualiza modelo", RotacaoMaisNoventaAtualizaModelo),
                ("Rotacao cicla quadrantes", RotacaoCiclaQuadrantes),
                ("Preview preserva rotacao em modelo real", PreviewPreservaRotacaoEmModeloReal),
                ("Preview armazena rotacao antes de existir", PreviewArmazenaRotacaoAntesDeExistir),
                ("Preview existente rotaciona visualmente", PreviewExistenteRotacionaVisualmente),
                ("Update do preview nao reseta rotacao", UpdateDoPreviewNaoResetaRotacao),
                ("Modelo real recebe rotacao do preview", ModeloRealRecebeRotacaoDoPreview),
                ("InputRouter envia Space para insercao sem preview", InputRouterEnviaSpaceParaInsercaoSemPreview),
                ("Botoes da Ribbon nao capturam foco", BotoesDaRibbonNaoCapturamFoco),
                ("Viewport continua focavel", ViewportContinuaFocavel),
                ("Elemento rotacionado persiste apos reload", ElementoRotacionadoPersisteAposReload),
                ("Terminais mudam posicao e preservam IDs", TerminaisMudamPosicaoEPreservamIds),
                ("Cabo preserva TerminalId apos rotacao", CaboPreservaTerminalIdAposRotacao),
                ("Cabo reancora visualmente apos rotacao", CaboReancoraVisualmenteAposRotacao),
                ("Undo Redo da rotacao restaura elemento e cabos", UndoRedoRotacaoRestauraElementoECabos),
                ("Rotacao reancora Carga com cabo conectado", RotacaoReancoraCargaComCaboConectado),
                ("Rotacao reancora Gerador com cabo conectado", RotacaoReancoraGeradorComCaboConectado),
                ("Rotacao reancora SIN em todos terminais", RotacaoReancoraSinEmTodosTerminais),
                ("Rotacao reancora Transformador primario e secundario", RotacaoReancoraTransformadorPrimarioSecundario),
                ("Rotacao reancora Barra em dois terminais", RotacaoReancoraBarraEmDoisTerminais),
                ("Undo Redo da rotacao reancora terminais e cabos", UndoRedoRotacaoReancoraTerminaisECabos),
                ("Snap encontra terminal apos rotacao com cabo", SnapEncontraTerminalAposRotacaoComCabo),
                ("ElectricGraph build repetido apos rotacao nao altera Document", ElectricGraphBuildAposRotacaoNaoAlteraDocument),
                ("DTO nao muda por causa da rotacao", DtoNaoMudaPorCausaDaRotacao),
                ("RotationService aceita Barra", RotationServiceAceitaBarra),
                ("Barra nova possui altura padrao", BarraNovaPossuiAlturaPadrao),
                ("Barra padrao mantem 24 terminais com pitch fixo", BarraPadraoMantemVinteQuatroTerminaisComPitchFixo),
                ("Alterar altura da Barra muda Bounds", AlterarAlturaDaBarraMudaBounds),
                ("Crescer Barra aumenta conectores preservando IDs", CrescerBarraAumentaConectoresPreservandoIds),
                ("Reduzir Barra remove terminais livres excedentes", ReduzirBarraRemoveTerminaisLivresExcedentes),
                ("Reduzir Barra preserva terminal ocupado", ReduzirBarraPreservaTerminalOcupado),
                ("Resize da Barra reancora cabo conectado", ResizeDaBarraReancoraCaboConectado),
                ("Undo Redo de resize da Barra preserva cabo", UndoRedoResizeBarraPreservaCabo),
                ("Connectivity retorna terminais ocupados da Barra", ConnectivityRetornaTerminaisOcupadosDaBarra),
                ("Cabo conectado a Barra reancora apos alterar altura", CaboConectadoABarraReancoraAposAlterarAltura),
                ("Barra com altura alterada persiste apos reload", BarraComAlturaAlteradaPersisteAposReload),
                ("ElectricGraph continua valido apos altura da Barra", ElectricGraphContinuaValidoAposAlturaDaBarra),
                ("DTO nao muda por causa da altura da Barra", DtoNaoMudaPorCausaDaAlturaDaBarra),
                ("Rotacao da Barra funciona apos altura alterada", RotacaoDaBarraFuncionaAposAlturaAlterada),
                ("Cabo permanece ancorado apos altura rotacao movimento e reload", CaboPermaneceAncoradoAposAlturaRotacaoMovimentoEReload),
                ("Altura invalida da Barra normaliza para minimo", AlturaInvalidaDaBarraNormalizaParaMinimo),
                ("Barra selecionada rotaciona 0 para 90", BarraSelecionadaRotacionaZeroParaNoventa),
                ("Barra cicla quadrantes", BarraCiclaQuadrantes),
                ("Preview de Barra preserva rotacao", PreviewDeBarraPreservaRotacao),
                ("Barra preserva 24 TerminalIds apos rotacao", BarraPreservaVinteQuatroTerminalIdsAposRotacao),
                ("Terminais da Barra mudam posicao visual apos rotacao", TerminaisDaBarraMudamPosicaoVisualAposRotacao),
                ("Cabo conectado a Barra preserva TerminalId apos rotacao", CaboConectadoABarraPreservaTerminalIdAposRotacao),
                ("Cabo conectado a Barra reancora visualmente apos rotacao", CaboConectadoABarraReancoraVisualmenteAposRotacao),
                ("Undo Redo da rotacao da Barra restaura cabos", UndoRedoRotacaoDaBarraRestauraCabos),
                ("Barra rotacionada persiste apos reload", BarraRotacionadaPersisteAposReload),
                ("ElectricGraph apos rotacao da Barra mantem arestas validas", ElectricGraphAposRotacaoDaBarraMantemArestasValidas),
                ("DTO nao muda por causa da rotacao da Barra", DtoNaoMudaPorCausaDaRotacaoDaBarra),
                ("Hit-test encontra Barra rotacionada", HitTestEncontraBarraRotacionada),
                ("Snap encontra terminal de Barra rotacionada", SnapEncontraTerminalDeBarraRotacionada)
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

        private static void ElementosExistentesPermanecemEletricos()
        {
            Elemento[] elementos =
            {
                new Cabo(),
                new Barra(),
                new Carga(),
                new Gerador(),
                new Sin(),
                new Transformador()
            };

            foreach (Elemento elemento in elementos)
            {
                AssertEqual(
                    ElementoDomainRole.EletricoTopologico,
                    elemento.DomainRole,
                    $"{elemento.GetType().Name}.DomainRole");
                Assert(elemento.ParticipaDoGrafoEletrico, $"{elemento.GetType().Name} deve participar do grafo eletrico.");
            }
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

        private static void ElectricGraphIncluiEletricosEIgnoraAnotativo()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-GRAFO");
            Transformador transformer = CreateTransformador("TR-GRAFO");
            Barra bar = CreateBar("BARRA-GRAFO");
            Gerador generator = CreateGenerator("GER-GRAFO", 900, 0.95);
            Carga load = CreateLoad("CARGA-GRAFO", 300, 100);
            Cabo cable = CreateCable(generator, load, "L-GRAFO", 1);
            Elemento annotation = CreateAnnotation("CARGA-GRAFO");

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformer);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);
            document.AdicionarElemento(annotation);

            ElectricGraph graph = new ElectricGraphBuilder(document).Build();

            AssertEqual(5, graph.Nodes.Count, "Quantidade de nos eletricos");
            AssertEqual(1, graph.Edges.Count, "Quantidade de cabos eletricos");
            AssertContainsNode(graph.Nodes, sin, "SIN no grafo");
            AssertContainsNode(graph.Nodes, transformer, "Transformador no grafo");
            AssertContainsNode(graph.Nodes, bar, "Barra no grafo");
            AssertContainsNode(graph.Nodes, generator, "Gerador no grafo");
            AssertContainsNode(graph.Nodes, load, "Carga no grafo");
            Assert(graph.FindNode(annotation.Id.ToString()) == null, "Anotativo nao deve virar no do grafo.");
        }

        private static void DtoPermaneceIdenticoComAnotativoNoDocument()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            string dtoBefore = SerializeCircuitDto(new CircuitBuilder(new ParameterReader(circuit.Document)).Build());

            circuit.Document.AdicionarElemento(CreateAnnotation(circuit.Generator.Nome));

            string dtoAfter = SerializeCircuitDto(new CircuitBuilder(new ParameterReader(circuit.Document)).Build());
            ParameterReader reader = new(circuit.Document);

            AssertEqual(dtoBefore, dtoAfter, "CircuitDto serializado");
            AssertEqual(1, reader.GetGenerators().Count, "ParameterReader.Generators");
            AssertEqual(1, reader.GetLoads().Count, "ParameterReader.Loads");
            AssertEqual(1, reader.GetLines().Count, "ParameterReader.Lines");
            AssertEqual(0, reader.GetSins().Count, "ParameterReader.Sins");
            AssertEqual(0, reader.GetTransformers().Count, "ParameterReader.Transformers");
        }

        private static void OperationalGraphIgnoraAnotativo()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-ANOT");
            Carga load = CreateLoad("CARGA-ANOT", 300, 100);
            Cabo cable = CreateCable(sin, 1, load, 0, "L-ANOT", 1.0);
            Elemento annotation = CreateAnnotation("SIN-ANOT");

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);
            document.AdicionarElemento(annotation);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN energizado com anotativo");
            AssertEnergized(state, load, "Carga energizada com anotativo");
            AssertEdgeEnergized(state, cable, "Cabo energizado com anotativo");
            Assert(!state.EnergizedNodeIds.Contains(annotation.Id.ToString()), "Anotativo nao deve energizar.");
            Assert(!state.DeenergizedNodeIds.Contains(annotation.Id.ToString()), "Anotativo nao deve aparecer desenergizado.");
        }

        private static void TopologyValidatorIgnoraAnotativo()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            circuit.Document.AdicionarElemento(CreateAnnotation(circuit.Generator.Nome));

            TopologyValidationResult result = new TopologyValidator(circuit.Document).Validate();

            Assert(result.IsValid, "Anotativo com nome duplicado deve ser ignorado pelo validador topologico.");
            AssertEqual(4, circuit.Document.Elementos.Count, "Anotativo deve permanecer no Document.");
        }

        private static void ClassificacaoEletricaNaoDependeDeNomeTipoOuSvg()
        {
            var carga = new Carga
            {
                Nome = "texto livre",
                Tipo = new TipoCarga()
            };

            var annotation = new FakeAnnotationElement
            {
                Nome = "Carga",
                Tipo = new TipoCarga()
            };

            Assert(carga.ParticipaDoGrafoEletrico, "Carga deve ser eletrica apesar do nome livre.");
            AssertEqual(ElementoDomainRole.Grafico, annotation.DomainRole, "Anotativo.DomainRole");
            Assert(!annotation.ParticipaDoGrafoEletrico, "Anotativo nao deve virar eletrico por nome ou Tipo.");
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
            AssertSinTerminals(sin, "SIN criado");
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
            AssertEqual(4, node.Terminals.Count, "Terminais do no SIN");
            AssertGraphTerminal(node, Sin.TERMINAL_NORTE, "Terminal NORTE no grafo");
            AssertGraphTerminal(node, Sin.TERMINAL_SUL, "Terminal SUL no grafo");
            AssertGraphTerminal(node, Sin.TERMINAL_LESTE, "Terminal LESTE no grafo");
            AssertGraphTerminal(node, Sin.TERMINAL_OESTE, "Terminal OESTE no grafo");
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
            AssertSinTerminals(loadedSin, "SIN apos reload");
        }

        private static void CabosConectadosAosTerminaisDoSinPreservamConexoes()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-CABO");
            Carga loadNorte = CreateLoad("CARGA-SIN-N", 350, 120);
            Carga loadSul = CreateLoad("CARGA-SIN-S", 351, 121);
            Carga loadLeste = CreateLoad("CARGA-SIN-L", 352, 122);
            Carga loadOeste = CreateLoad("CARGA-SIN-O", 353, 123);
            Cabo cableNorte = CreateCable(sin, 0, loadNorte, 0, "L-SIN-N", 1.5);
            Cabo cableSul = CreateCable(sin, 1, loadSul, 0, "L-SIN-S", 1.6);
            Cabo cableLeste = CreateCable(sin, 2, loadLeste, 0, "L-SIN-L", 1.7);
            Cabo cableOeste = CreateCable(sin, 3, loadOeste, 0, "L-SIN-O", 1.8);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(loadNorte);
            document.AdicionarElemento(loadSul);
            document.AdicionarElemento(loadLeste);
            document.AdicionarElemento(loadOeste);
            document.AdicionarElemento(cableNorte);
            document.AdicionarElemento(cableSul);
            document.AdicionarElemento(cableLeste);
            document.AdicionarElemento(cableOeste);

            AraciDocument loaded = SaveAndLoad(document);
            ElectricGraph graph = new ElectricGraphBuilder(loaded).Build();
            Sin loadedSin = FindById<Sin>(loaded, sin.Id);

            AssertSinTerminals(loadedSin, "SIN conectado apos reload");
            AssertCableEndpoint(loaded, cableNorte, sin, Sin.TERMINAL_NORTE, loadNorte, "Cabo NORTE");
            AssertCableEndpoint(loaded, cableSul, sin, Sin.TERMINAL_SUL, loadSul, "Cabo SUL");
            AssertCableEndpoint(loaded, cableLeste, sin, Sin.TERMINAL_LESTE, loadLeste, "Cabo LESTE");
            AssertCableEndpoint(loaded, cableOeste, sin, Sin.TERMINAL_OESTE, loadOeste, "Cabo OESTE");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Grafo com SIN nao deve ter arestas invalidas");
        }

        private static void DtosSemSinMantemGeradorComoSlack()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();

            CircuitDto dto = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();

            Assert(dto.Slack != null, "SlackDto deve continuar existindo.");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Slack!.Id, "Slack deve continuar usando gerador");
            AssertEqual(0, dto.Generators.Count, "Primeiro gerador legado nao deve aparecer em Generators.");
            AssertEqual(1, dto.Loads.Count, "Quantidade de cargas sem SIN");
            AssertEqual(1, dto.Lines.Count, "Quantidade de linhas sem SIN");
        }

        private static void SinComGeradorViraSlackPreferencial()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            Sin sin = CreateSin("SIN-PREFERENCIAL");
            circuit.Document.Elementos.Insert(0, sin);

            CircuitDto dto = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack deve usar SIN");
            AssertEqual(sin.Nome, dto.Slack.Nome, "Slack.Nome SIN");
            AssertEqual(sin.Barra, dto.Slack.Barra, "Slack.Barra SIN");
            AssertEqual(1, dto.Generators.Count, "Gerador deve permanecer em Generators");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Generators[0].Id, "GeneratorDto.Id com SIN");
            AssertEqual(13.8, dto.Generators[0].Tensao, "GeneratorDto.Tensao com SIN");
            AssertEqual(circuit.Generator.PotenciaAtiva, dto.Generators[0].Potencia, "GeneratorDto.Potencia com SIN");
            AssertEqual(0.93, dto.Generators[0].FP, "GeneratorDto.FP com SIN");
            AssertEqual(1, dto.Loads.Count, "Cargas com SIN");
            AssertEqual(1, dto.Lines.Count, "Linhas com SIN");
        }

        private static void SinComGeradorPreservaGeneratorDtoReal()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-GER-REAL");
            Gerador generator = CreateGenerator("GERADOR-REAL", 2750, 0.96);
            Carga load = CreateLoad("CARGA-GER-REAL", 300, 100);

            generator.TensaoLinha = "0.69";
            generator.TipoGerador.TensaoKV = 34.5;

            document.AdicionarElemento(sin);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-SIN-LOAD-REAL", 1.0));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-GER-LOAD-REAL", 1.0));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();
            GeneratorDto generatorDto = dto.Generators.Single();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack deve usar SIN no circuito com gerador real");
            AssertEqual(generator.Id.ToString(), generatorDto.Id, "GeneratorDto real.Id");
            AssertEqual(generator.Nome, generatorDto.Nome, "GeneratorDto real.Nome");
            AssertEqual(generator.Nome, generatorDto.Barra, "GeneratorDto real.Barra");
            AssertEqual(3, generatorDto.Fases, "GeneratorDto real.Fases");
            AssertEqual(0.69, generatorDto.Tensao, "GeneratorDto real.Tensao");
            AssertEqual(2750, generatorDto.Potencia, "GeneratorDto real.Potencia");
            AssertEqual(0.96, generatorDto.FP, "GeneratorDto real.FP");
        }

        private static void SinComMultiplosGeradoresPreservaTodosGenerators()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-MULTI-GER");
            Gerador generatorA = CreateGenerator("GERADOR-SIN-A", 1100, 0.91);
            Gerador generatorB = CreateGenerator("GERADOR-SIN-B", 730, 0.87);
            Carga load = CreateLoad("CARGA-SIN-MULTI", 510, 170);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(generatorA);
            document.AdicionarElemento(generatorB);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-SIN-M", 1.1));
            document.AdicionarElemento(CreateCable(generatorA, 0, load, 0, "L-GA-SIN", 1.2));
            document.AdicionarElemento(CreateCable(generatorB, 0, load, 0, "L-GB-SIN", 1.3));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack deve usar SIN com multiplos geradores");
            AssertEqual(2, dto.Generators.Count, "Todos os geradores devem permanecer em Generators");
            Assert(dto.Generators.Any(g => g.Id == generatorA.Id.ToString()), "Gerador A deve estar em Generators.");
            Assert(dto.Generators.Any(g => g.Id == generatorB.Id.ToString()), "Gerador B deve estar em Generators.");
            AssertEqual(1100, dto.Generators.Single(g => g.Id == generatorA.Id.ToString()).Potencia, "GeneratorDto A.Potencia");
            AssertEqual(730, dto.Generators.Single(g => g.Id == generatorB.Id.ToString()).Potencia, "GeneratorDto B.Potencia");
        }

        private static void ReloadPreservaGeneratorDtoRealComSin()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RELOAD-GER-REAL");
            Gerador generator = CreateGenerator("GERADOR-RELOAD-REAL", 3150, 0.94);
            Carga load = CreateLoad("CARGA-RELOAD-GER-REAL", 300, 100);

            generator.TensaoLinha = "34.5";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-SIN-LOAD-RELOAD-GER", 1.0));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-GER-LOAD-RELOAD-GER", 1.0));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(SaveAndLoad(document))).Build();
            GeneratorDto generatorDto = dto.Generators.Single();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack SIN apos reload com gerador real");
            AssertEqual(generator.Id.ToString(), generatorDto.Id, "GeneratorDto reload.Id");
            AssertEqual(34.5, generatorDto.Tensao, "GeneratorDto reload.Tensao");
            AssertEqual(3150, generatorDto.Potencia, "GeneratorDto reload.Potencia");
            AssertEqual(0.94, generatorDto.FP, "GeneratorDto reload.FP");
        }

        private static void CircuitoEolicoSimplificadoPreservaGeneratorDto()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-EOLICO");
            Transformador trSe = CreateTransformador("TR-SE-EOLICO");
            Transformador trAerogerador = CreateTransformador("TR-AERO-EOLICO");
            Gerador generator = CreateGenerator("AEROGERADOR-001", 4200, 0.97);
            Carga load = CreateLoad("CARGA-AUX-EOLICA", 120, 40);

            sin.TensaoLinha = "138";

            trSe.TensaoPrimarioKV = 138.0;
            trSe.TensaoSecundarioKV = 34.5;
            trSe.PotenciaAparente = 65000.0;

            trAerogerador.TensaoPrimarioKV = 34.5;
            trAerogerador.TensaoSecundarioKV = 0.69;
            trAerogerador.PotenciaAparente = 5000.0;

            generator.TensaoLinha = "0.69";
            load.TensaoLinha = "0.69";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(trSe);
            document.AdicionarElemento(trAerogerador);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, trSe, 0, "L-EOLICO-138", 1.0));
            document.AdicionarElemento(CreateCable(trSe, 1, trAerogerador, 0, "L-EOLICO-34", 1.0));
            document.AdicionarElemento(CreateCable(trAerogerador, 1, generator, 0, "L-EOLICO-069", 1.0));
            document.AdicionarElemento(CreateCable(generator, 1, load, 0, "L-EOLICO-AUX", 0.1));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();
            GeneratorDto generatorDto = dto.Generators.Single();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack eolico deve usar SIN");
            AssertEqual(2, dto.Transformers.Count, "Circuito eolico deve preservar dois transformadores");
            AssertEqual(generator.Id.ToString(), generatorDto.Id, "GeneratorDto eolico.Id");
            AssertEqual(generator.Nome, generatorDto.Nome, "GeneratorDto eolico.Nome");
            AssertEqual(generator.Nome, generatorDto.Barra, "GeneratorDto eolico.Barra");
            AssertEqual(0.69, generatorDto.Tensao, "GeneratorDto eolico.Tensao");
            AssertEqual(4200, generatorDto.Potencia, "GeneratorDto eolico.Potencia");
            AssertEqual(0.97, generatorDto.FP, "GeneratorDto eolico.FP");
        }

        private static void MultiplosSinUsamPrimeiroDoDocumentComoSlack()
        {
            var document = new AraciDocument();
            Sin sinA = CreateSin("SIN-PRIMEIRO");
            Sin sinB = CreateSin("SIN-SEGUNDO");
            Gerador generator = CreateGenerator("GERADOR-COM-SIN", 900, 0.95);
            Carga load = CreateLoad("CARGA-MULTI-SIN", 250, 80);

            document.AdicionarElemento(sinA);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(sinB);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sinB, 2, load, 0, "L-MULTI-SIN", 1.4));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-G-MULTI-SIN", 1.5));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(sinA.Id.ToString(), dto.Slack.Id, "Primeiro SIN deve virar slack");
            AssertEqual(sinA.Nome, dto.Slack.Nome, "Nome do primeiro SIN slack");
            AssertEqual(1, dto.Generators.Count, "Gerador deve permanecer em Generators com multiplos SIN");
        }

        private static void ReloadComSinMantemSlackBaseadoNoSin()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            Sin sin = CreateSin("SIN-RELOAD-SLACK");
            circuit.Document.Elementos.Insert(0, sin);

            AraciDocument loaded = SaveAndLoad(circuit.Document);
            CircuitDto dto = new CircuitBuilder(new ParameterReader(loaded)).Build();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack SIN apos reload");
            AssertEqual(sin.Nome, dto.Slack.Nome, "Slack.Nome SIN apos reload");
            AssertEqual(1, dto.Generators.Count, "Gerador preservado apos reload com SIN");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Generators[0].Id, "GeneratorDto.Id apos reload com SIN");
        }

        private static void OperationalGraphEnergizaSinCaboECarga()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-OP");
            Carga load = CreateLoad("CARGA-OP", 300, 100);
            Cabo cable = CreateCable(sin, 1, load, 0, "L-OP", 1.0);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN energizado");
            AssertEnergized(state, load, "Carga energizada");
            AssertEdgeEnergized(state, cable, "Cabo energizado");
            AssertEqual(1, state.SourceNodeIds.Count, "Quantidade de fontes operacionais");
            AssertEqual(sin.Id.ToString(), state.SourceNodeIds[0], "Fonte operacional SIN");
        }

        private static void OperationalGraphMantemCargaIsoladaDesenergizada()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-ISOLADO");
            Carga load = CreateLoad("CARGA-ISOLADA", 300, 100);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN isolado energizado");
            AssertDeenergized(state, load, "Carga isolada desenergizada");
        }

        private static void OperationalGraphEnergizaRamificacaoComBarra()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RAMO-OP");
            Barra bar = CreateBar("BARRA-RAMO-OP");
            Carga load1 = CreateLoad("CARGA-OP-R1", 320, 90);
            Carga load2 = CreateLoad("CARGA-OP-R2", 280, 85);
            Cabo cable1 = CreateCable(sin, 1, bar, 0, "L-OP-01", 1.0);
            Cabo cable2 = CreateCable(bar, 1, load1, 0, "L-OP-02", 1.1);
            Cabo cable3 = CreateCable(bar, 2, load2, 0, "L-OP-03", 1.2);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(load1);
            document.AdicionarElemento(load2);
            document.AdicionarElemento(cable1);
            document.AdicionarElemento(cable2);
            document.AdicionarElemento(cable3);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN ramificado");
            AssertEnergized(state, bar, "Barra ramificada");
            AssertEnergized(state, load1, "Carga ramo 1");
            AssertEnergized(state, load2, "Carga ramo 2");
            AssertEdgeEnergized(state, cable1, "Cabo ramo 1");
            AssertEdgeEnergized(state, cable2, "Cabo ramo 2");
            AssertEdgeEnergized(state, cable3, "Cabo ramo 3");
        }

        private static void OperationalGraphNaoPropagaPorCaboInvalido()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-INVALIDO");
            Carga load = CreateLoad("CARGA-BLOQUEADA", 300, 100);
            Cabo cable = CreateCable(sin, 1, load, 0, "L-INVALIDO", 1.0);
            cable.DestinoTerminalId = "NAO_EXISTE";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN com cabo invalido");
            AssertDeenergized(state, load, "Carga atras de cabo invalido");
            AssertEdgeDeenergized(state, cable, "Cabo invalido desenergizado");
        }

        private static void OperationalGraphUsaGeradorComoFallbackSemSin()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            OperationalGraphState state = BuildOperationalState(circuit.Document);

            AssertEnergized(state, circuit.Generator, "Gerador fallback");
            AssertEnergized(state, circuit.Load, "Carga via gerador fallback");
            AssertEdgeEnergized(state, circuit.Cable, "Cabo via gerador fallback");
            AssertEqual(circuit.Generator.Id.ToString(), state.SourceNodeIds[0], "Fonte fallback gerador");
        }

        private static void OperationalGraphSemFonteNaoEnergizaNos()
        {
            var document = new AraciDocument();
            Carga load = CreateLoad("CARGA-SEM-FONTE", 300, 100);

            document.AdicionarElemento(load);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEqual(0, state.SourceNodeIds.Count, "Sem fontes operacionais");
            AssertEqual(0, state.EnergizedNodeIds.Count, "Nos energizados sem fonte");
            AssertDeenergized(state, load, "Carga sem fonte");
        }

        private static void OperationalGraphRebuildRepetidoNaoAlteraDocument()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-REBUILD-OP");
            Carga load = CreateLoad("CARGA-REBUILD-OP", 300, 100);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-REBUILD-OP", 1.0));

            int countBefore = document.Elementos.Count;

            OperationalGraphState state1 = BuildOperationalState(document);
            OperationalGraphState state2 = BuildOperationalState(document);
            OperationalGraphState state3 = BuildOperationalState(document);

            AssertEqual(countBefore, document.Elementos.Count, "Contagem apos rebuild operacional");
            AssertEqual(state1.EnergizedNodeIds.Count, state2.EnergizedNodeIds.Count, "Operational nodes 1/2");
            AssertEqual(state2.EnergizedNodeIds.Count, state3.EnergizedNodeIds.Count, "Operational nodes 2/3");
            AssertEqual(state1.EnergizedEdgeIds.Count, state2.EnergizedEdgeIds.Count, "Operational edges 1/2");
            AssertEqual(state2.EnergizedEdgeIds.Count, state3.EnergizedEdgeIds.Count, "Operational edges 2/3");
        }

        private static void OperationalGraphAposReloadPreservaResultado()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RELOAD-OP");
            Carga load = CreateLoad("CARGA-RELOAD-OP", 300, 100);
            Cabo cable = CreateCable(sin, 1, load, 0, "L-RELOAD-OP", 1.0);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            AraciDocument loaded = SaveAndLoad(document);
            OperationalGraphState state = BuildOperationalState(loaded);

            Assert(state.IsNodeEnergized(sin.Id.ToString()), "SIN deve continuar energizado apos reload.");
            Assert(state.IsNodeEnergized(load.Id.ToString()), "Carga deve continuar energizada apos reload.");
            Assert(state.IsEdgeEnergized(cable.Id.ToString()), "Cabo deve continuar energizado apos reload.");
        }

        private static void TransformadorMinimoPossuiTerminais()
        {
            Transformador transformador = CreateTransformador("TR-TESTE");

            AssertTransformadorTerminals(transformador, "Transformador minimo");
            AssertEqual(120, transformador.Terminais[0].Posicao.X, "Primario.X");
            AssertEqual(80, transformador.Terminais[0].Posicao.Y, "Primario.Y");
            AssertEqual(120, transformador.Terminais[1].Posicao.X, "Secundario.X");
            AssertEqual(220, transformador.Terminais[1].Posicao.Y, "Secundario.Y");
        }

        private static void TransformadorApareceNoElectricGraph()
        {
            var document = new AraciDocument();
            Transformador transformador = CreateTransformador("TR-GRAFO");

            document.AdicionarElemento(transformador);

            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            ElectricGraphNode? node = graph.FindNode(transformador.Id.ToString());

            Assert(node != null, "Transformador deve aparecer como no do ElectricGraph.");
            AssertEqual(2, node!.Terminals.Count, "Terminais do transformador no grafo");
            AssertGraphTerminal(node, Transformador.TERMINAL_PRIMARIO, "Terminal PRIMARIO no grafo");
            AssertGraphTerminal(node, Transformador.TERMINAL_SECUNDARIO, "Terminal SECUNDARIO no grafo");
        }

        private static void TransformadorPreservaConexoesAposReload()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-TR");
            Transformador transformador = CreateTransformador("TR-RELOAD");
            Carga load = CreateLoad("CARGA-TR", 300, 100);
            Cabo primaryCable = CreateCable(sin, 1, transformador, 0, "L-TR-P", 1.0);
            Cabo secondaryCable = CreateCable(transformador, 1, load, 0, "L-TR-S", 1.1);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(load);
            document.AdicionarElemento(primaryCable);
            document.AdicionarElemento(secondaryCable);

            AraciDocument loaded = SaveAndLoad(document);
            Transformador loadedTransformador = FindById<Transformador>(loaded, transformador.Id);
            Cabo loadedPrimary = FindById<Cabo>(loaded, primaryCable.Id);
            Cabo loadedSecondary = FindById<Cabo>(loaded, secondaryCable.Id);
            ElectricGraph graph = new ElectricGraphBuilder(loaded).Build();

            AssertTransformadorTerminals(loadedTransformador, "Transformador apos reload");
            AssertEqual(Transformador.TERMINAL_PRIMARIO, loadedPrimary.DestinoTerminalId, "Primario apos reload");
            AssertEqual(Transformador.TERMINAL_SECUNDARIO, loadedSecondary.OrigemTerminalId, "Secundario apos reload");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Grafo com transformador nao deve ter arestas invalidas");
        }

        private static void TransformadorEntraNoDtoMinimo()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-DTO-TR");
            Transformador transformador = CreateTransformador("TR-DTO");
            Gerador generator = CreateGenerator("GERADOR-DTO-TR", 900, 0.95);
            Carga load = CreateLoad("CARGA-DTO-TR", 300, 100);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, transformador, 0, "L-DTO-TR-P", 1.0));
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-DTO-TR-S", 1.1));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-DTO-TR-G", 1.2));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "SlackDto.Id");
            AssertEqual(13.8, dto.Slack.Tensao, "SlackDto.Tensao");
            AssertEqual(1, dto.Transformers.Count, "Quantidade de transformadores no DTO");
            AssertEqual(transformador.Id.ToString(), dto.Transformers[0].Id, "TransformerDto.Id");
            AssertEqual(transformador.Nome, dto.Transformers[0].Nome, "TransformerDto.Nome");
            AssertEqual(3, dto.Transformers[0].Fases, "TransformerDto.Fases");
            AssertEqual(2, dto.Transformers[0].Enrolamentos, "TransformerDto.Enrolamentos");
            AssertEqual($"{transformador.Nome}_PRIMARIO", dto.Transformers[0].BarraPrimario, "TransformerDto.BarraPrimario");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", dto.Transformers[0].BarraSecundario, "TransformerDto.BarraSecundario");
            AssertEqual(13.8, dto.Transformers[0].TensaoPrimarioKV, "TransformerDto.TensaoPrimarioKV");
            AssertEqual(0.38, dto.Transformers[0].TensaoSecundarioKV, "TransformerDto.TensaoSecundarioKV");
            AssertEqual(500, dto.Transformers[0].PotenciaKVA, "TransformerDto.PotenciaKVA");
            AssertEqual(1, dto.Transformers[0].RPercentual, "TransformerDto.RPercentual");
            AssertEqual(5, dto.Transformers[0].XPercentual, "TransformerDto.XPercentual");
            AssertEqual("Wye", dto.Transformers[0].LigacaoPrimario, "TransformerDto.LigacaoPrimario");
            AssertEqual("Wye", dto.Transformers[0].LigacaoSecundario, "TransformerDto.LigacaoSecundario");
            AssertEqual(sin.Nome, dto.Lines[0].Barra1, "LineDto primario.Barra1");
            AssertEqual($"{transformador.Nome}_PRIMARIO", dto.Lines[0].Barra2, "LineDto primario.Barra2");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", dto.Lines[1].Barra1, "LineDto secundario.Barra1");
            AssertEqual(load.Nome, dto.Lines[1].Barra2, "LineDto secundario.Barra2");
        }

        private static void TransformadorUsaCentroComGeometriaPropria()
        {
            var context = new EditorContext();
            Transformador transformador = context.ElementoFactory.CriarTransformador();
            Point centro = new Point(500, 400);
            Point topoEsquerdo = context.Geometry.CalcularTopoEsquerdoPorCentro(transformador, centro);
            TransformadorViewModel vm = context.ElementoFactory.CriarTransformadorVM();

            AssertEqual(460, topoEsquerdo.X, "Transformador.PosicaoX por centro");
            AssertEqual(330, topoEsquerdo.Y, "Transformador.PosicaoY por centro");
            AssertEqual(ElementGeometryDefaults.TransformadorLargura, vm.Largura, "TransformadorViewModel.Largura");
            AssertEqual(ElementGeometryDefaults.TransformadorAltura, vm.Altura, "TransformadorViewModel.Altura");
        }

        private static void ReloadPreservaDtoDetalhadoTransformador()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RELOAD-DTO-TR");
            Transformador transformador = CreateTransformador("TR-RELOAD-DTO");
            Gerador generator = CreateGenerator("GERADOR-RELOAD-DTO-TR", 900, 0.95);
            Carga load = CreateLoad("CARGA-RELOAD-DTO-TR", 300, 100);

            transformador.TensaoPrimarioKV = 34.5;
            transformador.TensaoSecundarioKV = 0.69;
            transformador.PotenciaAparente = 1500.0;
            transformador.RPercentual = 0.75;
            transformador.XPercentual = 6.5;
            transformador.LigacaoPrimario = "Delta";
            transformador.LigacaoSecundario = "Wye";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, transformador, 0, "L-RELOAD-TR-P", 1.0));
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-RELOAD-TR-S", 1.1));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-RELOAD-TR-G", 1.2));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(SaveAndLoad(document))).Build();
            TransformerDto transformerDto = dto.Transformers.Single();

            AssertEqual($"{transformador.Nome}_PRIMARIO", transformerDto.BarraPrimario, "Reload TransformerDto.BarraPrimario");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", transformerDto.BarraSecundario, "Reload TransformerDto.BarraSecundario");
            AssertEqual(34.5, transformerDto.TensaoPrimarioKV, "Reload TransformerDto.TensaoPrimarioKV");
            AssertEqual(0.69, transformerDto.TensaoSecundarioKV, "Reload TransformerDto.TensaoSecundarioKV");
            AssertEqual(1500, transformerDto.PotenciaKVA, "Reload TransformerDto.PotenciaKVA");
            AssertEqual(0.75, transformerDto.RPercentual, "Reload TransformerDto.RPercentual");
            AssertEqual(6.5, transformerDto.XPercentual, "Reload TransformerDto.XPercentual");
            AssertEqual("Delta", transformerDto.LigacaoPrimario, "Reload TransformerDto.LigacaoPrimario");
            AssertEqual("Wye", transformerDto.LigacaoSecundario, "Reload TransformerDto.LigacaoSecundario");
            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "SIN deve continuar slack preferencial");
            AssertEqual(sin.Nome, dto.Lines[0].Barra1, "Reload LineDto primario.Barra1");
            AssertEqual($"{transformador.Nome}_PRIMARIO", dto.Lines[0].Barra2, "Reload LineDto primario.Barra2");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", dto.Lines[1].Barra1, "Reload LineDto secundario.Barra1");
            AssertEqual(load.Nome, dto.Lines[1].Barra2, "Reload LineDto secundario.Barra2");
        }

        private static void CircuitDtoPreservaParametrosReaisSinTransformadorCarga()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-138");
            Transformador transformador = CreateTransformador("TR-65MVA");
            Carga load = CreateLoad("CARGA-34KV", 5000, 1000);

            sin.TensaoLinha = "138";

            transformador.TensaoPrimarioKV = 138.0;
            transformador.TensaoSecundarioKV = 34.5;
            transformador.PotenciaAparente = 65000.0;
            transformador.RPercentual = 1.0;
            transformador.XPercentual = 8.0;
            transformador.LigacaoPrimario = "Wye";
            transformador.LigacaoSecundario = "Wye";

            load.TensaoLinha = "34.5";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, transformador, 0, "L-REAL-P", 1.0));
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-REAL-S", 1.0));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();
            AssertCircuitDtoParametrosReais(dto, sin, transformador, load, "DTO real");

            CircuitDto reloadedDto = new CircuitBuilder(new ParameterReader(SaveAndLoad(document))).Build();
            AssertCircuitDtoParametrosReais(reloadedDto, sin, transformador, load, "DTO real apos reload");
        }

        private static void DtosAntigosDefaultPreservamSinECarga()
        {
            var document = new AraciDocument();
            Sin sin = new Sin
            {
                Nome = "SIN-DEFAULT",
                Barra = "SIN-DEFAULT",
                PosicaoX = 80,
                PosicaoY = 80,
                Tipo = new TipoSin()
            };
            Carga load = new Carga
            {
                Nome = "CARGA-DEFAULT",
                Barra = "CARGA-DEFAULT",
                PosicaoX = 300,
                PosicaoY = 100,
                Tipo = new TipoCarga()
            };

            sin.AtualizarTerminais(80, 80);
            load.AtualizarTerminais(80);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-DEFAULT", 1.0));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(12.47, dto.Slack.Tensao, "Default Slack.Tensao");
            AssertEqual(12.47, dto.Loads.Single().Tensao, "Default Load.Tensao");
            AssertEqual(800, dto.Loads.Single().PotenciaAtiva, "Default Load.PotenciaAtiva");
            AssertEqual(300, dto.Loads.Single().PotenciaReativa, "Default Load.PotenciaReativa");
        }

        private static void TopologyValidatorAceitaSinTransformadorCargaSemGerador()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-TOPO-TR");
            Transformador transformador = CreateTransformador("TR-TOPO");
            Carga load = CreateLoad("CARGA-TOPO-TR", 300, 100);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, transformador, 0, "L-TOPO-TR-P", 1.0));
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-TOPO-TR-S", 1.1));

            TopologyValidationResult result = new TopologyValidator(document).Validate();
            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            Assert(result.IsValid, $"Topologia com SIN deve ser valida. Erros: {result.FormatErrors()}");
            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "SIN deve virar SlackDto sem gerador");
            AssertEqual(0, dto.Generators.Count, "Sem geradores no DTO");
        }

        private static void TopologyValidatorAceitaGeradorLegadoSemSin()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            TopologyValidationResult result = new TopologyValidator(circuit.Document).Validate();
            CircuitDto dto = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();

            Assert(result.IsValid, $"Topologia com gerador legado deve ser valida. Erros: {result.FormatErrors()}");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Slack.Id, "Gerador legado deve virar SlackDto sem SIN");
        }

        private static void TopologyValidatorSemFonteSlackFalhaComMensagemClara()
        {
            var document = new AraciDocument();
            Transformador transformador = CreateTransformador("TR-SEM-FONTE");
            Carga load = CreateLoad("CARGA-SEM-FONTE", 300, 100);

            document.AdicionarElemento(transformador);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-SEM-FONTE", 1.0));

            TopologyValidationResult result = new TopologyValidator(document).Validate();
            string errors = result.FormatErrors();

            Assert(!result.IsValid, "Topologia sem SIN e sem gerador deve falhar.");
            AssertContains(errors, "fonte slack", "Erro sem fonte slack");
            Assert(
                !errors.Contains("sem gerador", StringComparison.OrdinalIgnoreCase),
                $"Erro sem fonte nao deve mencionar apenas gerador. Texto: {errors}");
        }

        private static void TerminalEndpointIdentificaConexaoPorValor()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            Terminal origem = GetTerminal(circuit.Generator, 0);
            Terminal destino = GetTerminal(circuit.Load, 0);
            TerminalEndpoint endpointOrigem = TerminalEndpoint.FromTerminal(origem);
            TerminalEndpoint endpointDestino = new(circuit.Load.Id.ToString(), destino.Id);
            ConnectivityService connectivity = new(circuit.Document);

            Assert(endpointOrigem.IsComplete, "Endpoint de origem deve estar completo.");
            AssertEqual(origem.Id, endpointOrigem.TerminalId, "Endpoint.TerminalId");
            AssertEqual(origem, connectivity.ObterTerminal(endpointOrigem), "Resolver endpoint origem");
            AssertEqual(circuit.Cable, connectivity.ObterCabosConectados(endpointOrigem).Single(), "Cabo conectado ao endpoint origem");
            AssertEqual(circuit.Cable, connectivity.ObterCabosConectados(endpointDestino).Single(), "Cabo conectado ao endpoint destino");
        }

        private static void RotacaoRecalculaTerminalPorPosicaoLocal()
        {
            var generator = new Gerador
            {
                PosicaoX = 100,
                PosicaoY = 100,
                Rotacao = 90
            };

            generator.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);
            Terminal topo = generator.Terminais.Single(t => t.Id == "TOPO");
            Terminal direita = generator.Terminais.Single(t => t.Id == "DIREITA");

            AssertEqual(170, topo.Posicao.X, "Topo rotacionado X");
            AssertEqual(135, topo.Posicao.Y, "Topo rotacionado Y");
            AssertEqual(135, direita.Posicao.X, "Direita rotacionada X");
            AssertEqual(170, direita.Posicao.Y, "Direita rotacionada Y");
        }

        private static void TerminalPlacementUsaPivoCentral()
        {
            var elemento = new Carga
            {
                PosicaoX = 100,
                PosicaoY = 100,
                Rotacao = 90
            };

            Point world = TerminalPlacement.ToWorld(elemento, new Point(35, 0), 70, 70);

            AssertEqual(170, world.X, "Terminal central 90 X");
            AssertEqual(135, world.Y, "Terminal central 90 Y");
        }

        private static void TerminalPlacementToLocalInverteToWorld()
        {
            var elemento = new Carga
            {
                PosicaoX = 123,
                PosicaoY = 77
            };

            var locals = new[]
            {
                new Point(35, 0),
                new Point(70, 35),
                new Point(35, 70),
                new Point(0, 35),
                new Point(10, 20)
            };

            foreach (double rotation in new double[] { 0, 90, 180, 270 })
            {
                elemento.Rotacao = rotation;

                foreach (Point local in locals)
                {
                    Point world = TerminalPlacement.ToWorld(elemento, local, 70, 70);
                    Point actual = TerminalPlacement.ToLocal(elemento, world, 70, 70);

                    AssertEqual(local.X, actual.X, $"ToLocal inverso {rotation}.X");
                    AssertEqual(local.Y, actual.Y, $"ToLocal inverso {rotation}.Y");
                }
            }
        }

        private static void CargaRotacionadaAlinhaTerminalComPivoCentral()
        {
            Carga load = CreateLoad("CARGA-CENTRAL", 300, 100);
            load.Rotacao = 90;
            load.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            AssertTerminalsUseCentralPivot(
                load,
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura,
                "Carga");
        }

        private static void GeradorRotacionadoAlinhaTerminaisComPivoCentral()
        {
            Gerador generator = CreateGenerator("GER-CENTRAL", 1000, 0.95);
            generator.Rotacao = 90;
            generator.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            AssertTerminalsUseCentralPivot(
                generator,
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura,
                "Gerador");
        }

        private static void SinRotacionadoAlinhaTerminaisComPivoCentral()
        {
            Sin sin = CreateSin("SIN-CENTRAL");
            sin.Rotacao = 90;
            sin.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            AssertTerminalsUseCentralPivot(
                sin,
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura,
                "SIN");
        }

        private static void TransformadorRotacionadoAlinhaTerminaisComPivoCentral()
        {
            Transformador transformador = CreateTransformador("TR-CENTRAL");
            transformador.Rotacao = 90;
            transformador.AtualizarTerminais(
                ElementGeometryDefaults.TransformadorLargura,
                ElementGeometryDefaults.TransformadorAltura);

            AssertTerminalsUseCentralPivot(
                transformador,
                ElementGeometryDefaults.TransformadorLargura,
                ElementGeometryDefaults.TransformadorAltura,
                "Transformador");
        }

        private static void BarraRotacionadaAlinhaTerminaisComPivoCentral()
        {
            Barra bar = CreateBar("BARRA-CENTRAL");
            bar.Rotacao = 90;
            bar.AtualizarTerminais(ElementGeometryDefaults.BarraLargura);

            AssertTerminalsUseCentralPivot(
                bar,
                ElementGeometryDefaults.BarraLargura,
                bar.Altura,
                "Barra");
        }

        private static void ElectricGraphBfsPercorreConexoesValidas()
        {
            AraciDocument document = CreateBranchDocument();
            Gerador generator = document.Elementos.OfType<Gerador>().Single();
            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            IReadOnlyList<ElectricGraphNode> visited = graph.BreadthFirst(generator.Id.ToString());

            AssertEqual(4, visited.Count, "Quantidade de nos no BFS");
            Assert(visited.All(n => graph.FindNode(n.ElementId) != null), "BFS deve retornar apenas nos do grafo.");
        }

        private static void RotacaoMaisNoventaAtualizaModelo()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-ROT", 300, 100);
            context.Document.AdicionarElemento(load);
            ElementoViewModel vm = GetVm(context, load);

            context.Selection.Selecionar(vm);
            bool rotated = context.Rotation.RotateSelectionClockwise();

            Assert(rotated, "Rotacao deve ser aplicada.");
            AssertEqual(90, load.Rotacao, "Rotacao da carga");
        }

        private static void RotacaoCiclaQuadrantes()
        {
            double value = 0;

            value = RotationService.RotateClockwise(value);
            AssertEqual(90, value, "Rotacao 0 -> 90");
            value = RotationService.RotateClockwise(value);
            AssertEqual(180, value, "Rotacao 90 -> 180");
            value = RotationService.RotateClockwise(value);
            AssertEqual(270, value, "Rotacao 180 -> 270");
            value = RotationService.RotateClockwise(value);
            AssertEqual(0, value, "Rotacao 270 -> 0");
        }

        private static void PreviewPreservaRotacaoEmModeloReal()
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<CargaViewModel, Carga>(
                context,
                context.ElementoFactory.CriarCargaVM,
                vm => (Carga)vm.Modelo);

            controller.Update(new Point(240, 180));
            controller.RotateClockwise();
            controller.RotateClockwise();

            Carga real = context.ElementoFactory.CriarCarga();
            real.Rotacao = controller.CurrentRotation;

            AssertEqual(180, real.Rotacao, "Rotacao copiada do preview");
        }

        private static void PreviewArmazenaRotacaoAntesDeExistir()
        {
            AssertPreviewArmazenaRotacaoAntesDeExistir<CargaViewModel, Carga>(
                "Carga",
                context => context.ElementoFactory.CriarCargaVM(),
                vm => (Carga)vm.Modelo);

            AssertPreviewArmazenaRotacaoAntesDeExistir<TransformadorViewModel, Transformador>(
                "Transformador",
                context => context.ElementoFactory.CriarTransformadorVM(),
                vm => (Transformador)vm.Modelo);

            AssertPreviewArmazenaRotacaoAntesDeExistir<BarraViewModel, Barra>(
                "Barra",
                context => context.ElementoFactory.CriarBarraVM(),
                vm => vm.Barra);
        }

        private static void PreviewExistenteRotacionaVisualmente()
        {
            AssertPreviewExistenteRotacionaVisualmente<CargaViewModel, Carga>(
                "Carga",
                context => context.ElementoFactory.CriarCargaVM(),
                vm => (Carga)vm.Modelo);

            AssertPreviewExistenteRotacionaVisualmente<TransformadorViewModel, Transformador>(
                "Transformador",
                context => context.ElementoFactory.CriarTransformadorVM(),
                vm => (Transformador)vm.Modelo);

            AssertPreviewExistenteRotacionaVisualmente<BarraViewModel, Barra>(
                "Barra",
                context => context.ElementoFactory.CriarBarraVM(),
                vm => vm.Barra);
        }

        private static void UpdateDoPreviewNaoResetaRotacao()
        {
            AssertUpdateDoPreviewNaoResetaRotacao<CargaViewModel, Carga>(
                "Carga",
                context => context.ElementoFactory.CriarCargaVM(),
                vm => (Carga)vm.Modelo);

            AssertUpdateDoPreviewNaoResetaRotacao<TransformadorViewModel, Transformador>(
                "Transformador",
                context => context.ElementoFactory.CriarTransformadorVM(),
                vm => (Transformador)vm.Modelo);

            AssertUpdateDoPreviewNaoResetaRotacao<BarraViewModel, Barra>(
                "Barra",
                context => context.ElementoFactory.CriarBarraVM(),
                vm => vm.Barra);
        }

        private static void ModeloRealRecebeRotacaoDoPreview()
        {
            AssertModeloRealRecebeRotacaoDoPreview<CargaViewModel, Carga>(
                "Carga",
                context => context.ElementoFactory.CriarCargaVM(),
                context => context.ElementoFactory.CriarCarga(),
                vm => (Carga)vm.Modelo);

            AssertModeloRealRecebeRotacaoDoPreview<TransformadorViewModel, Transformador>(
                "Transformador",
                context => context.ElementoFactory.CriarTransformadorVM(),
                context => context.ElementoFactory.CriarTransformador(),
                vm => (Transformador)vm.Modelo);

            AssertModeloRealRecebeRotacaoDoPreview<BarraViewModel, Barra>(
                "Barra",
                context => context.ElementoFactory.CriarBarraVM(),
                context => context.ElementoFactory.CriarBarra(),
                vm => vm.Barra);
        }

        private static void InputRouterEnviaSpaceParaInsercaoSemPreview()
        {
            RunSta(() =>
            {
                EditorContext context = CreateContextWithViewport();
                Assert(
                    context.Tools.AtivarInsercaoElemento(ElementRegistryService.KindCarga),
                    "Ferramenta de insercao de Carga deve ser ativada pelo ToolService.");

                Assert(context.Input.KeyDown(Key.Space), "InputRouter deve consumir Space na ferramenta de insercao sem preview.");
            });
        }

        private static void BotoesDaRibbonNaoCapturamFoco()
        {
            AssertButtonsNotFocusable("Ribbon/Tabs/DiagramaTab.xaml", "DiagramaTab");
            AssertButtonsNotFocusable("Ribbon/Tabs/EditarTab.xaml", "EditarTab");
            AssertButtonsNotFocusable("Ribbon/RibbonView.xaml", "RibbonView");
        }

        private static void ViewportContinuaFocavel()
        {
            string xaml = File.ReadAllText(FindProjectFile("Views/ViewportView.xaml"));

            AssertContains(xaml, "Focusable=\"True\"", "ViewportView.Focusable");
        }

        private static void AssertPreviewArmazenaRotacaoAntesDeExistir<TViewModel, TModel>(
            string name,
            Func<EditorContext, TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<TViewModel, TModel>(
                context,
                () => criarPreview(context),
                obterModelo);

            Assert(controller.RotateClockwise(), $"{name}: RotateClockwise antes do preview");
            AssertEqual(90, controller.CurrentRotation, $"{name}: CurrentRotation antes do preview");

            controller.Update(new Point(240, 180));

            Assert(controller.Preview != null, $"{name}: preview deve existir apos Update.");
            AssertEqual(90, obterModelo(controller.Preview!).Rotacao, $"{name}: Modelo.Rotacao do preview");
        }

        private static void AssertPreviewExistenteRotacionaVisualmente<TViewModel, TModel>(
            string name,
            Func<EditorContext, TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<TViewModel, TModel>(
                context,
                () => criarPreview(context),
                obterModelo);

            controller.Update(new Point(240, 180));
            Assert(controller.RotateClockwise(), $"{name}: RotateClockwise com preview");

            Assert(controller.Preview != null, $"{name}: preview deve existir.");
            AssertEqual(90, controller.CurrentRotation, $"{name}: CurrentRotation");
            AssertEqual(90, controller.Preview!.Rotacao, $"{name}: Preview.Rotacao");
            AssertEqual(90, obterModelo(controller.Preview).Rotacao, $"{name}: Preview.Modelo.Rotacao");
        }

        private static void AssertUpdateDoPreviewNaoResetaRotacao<TViewModel, TModel>(
            string name,
            Func<EditorContext, TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<TViewModel, TModel>(
                context,
                () => criarPreview(context),
                obterModelo);

            controller.RotateClockwise();
            controller.RotateClockwise();
            controller.Update(new Point(240, 180));
            controller.Update(new Point(260, 190));
            controller.Update(new Point(280, 200));

            Assert(controller.Preview != null, $"{name}: preview deve existir.");
            AssertEqual(180, controller.CurrentRotation, $"{name}: CurrentRotation apos Updates");
            AssertEqual(180, obterModelo(controller.Preview!).Rotacao, $"{name}: Modelo.Rotacao apos Updates");
        }

        private static void AssertModeloRealRecebeRotacaoDoPreview<TViewModel, TModel>(
            string name,
            Func<EditorContext, TViewModel> criarPreview,
            Func<EditorContext, TModel> criarModeloReal,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<TViewModel, TModel>(
                context,
                () => criarPreview(context),
                obterModelo);

            controller.RotateClockwise();
            controller.RotateClockwise();
            controller.RotateClockwise();

            TModel real = criarModeloReal(context);
            real.Rotacao = controller.CurrentRotation;

            AssertEqual(270, controller.CurrentRotation, $"{name}: CurrentRotation");
            AssertEqual(270, real.Rotacao, $"{name}: Rotacao do modelo real");
        }

        private static void ElementoRotacionadoPersisteAposReload()
        {
            var document = new AraciDocument();
            Carga load = CreateLoad("CARGA-PERSIST-ROT", 300, 100);
            load.Rotacao = 270;
            document.AdicionarElemento(load);

            AraciDocument loaded = SaveAndLoad(document);
            Carga loadedLoad = FindById<Carga>(loaded, load.Id);

            AssertEqual(270, loadedLoad.Rotacao, "Rotacao apos reload");
        }

        private static void TerminaisMudamPosicaoEPreservamIds()
        {
            Gerador generator = CreateGenerator("GER-TERM-ROT", 1000, 0.95);
            var before = generator.Terminais
                .Select(t => (t.Id, t.Posicao))
                .ToList();

            generator.Rotacao = 90;
            generator.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            for (int i = 0; i < before.Count; i++)
                AssertEqual(before[i].Id, generator.Terminais[i].Id, $"Terminal {i}.Id");

            Assert(
                before.Any(item => generator.Terminais.Single(t => t.Id == item.Id).Posicao != item.Posicao),
                "Ao menos um terminal deve mudar de posicao apos rotacao.");
        }

        private static void CaboPreservaTerminalIdAposRotacao()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            string origemTerminalId = circuit.Cable.OrigemTerminalId;
            string destinoTerminalId = circuit.Cable.DestinoTerminalId;

            RotateSelected(circuit.Context, circuit.Generator);

            AssertEqual(origemTerminalId, circuit.Cable.OrigemTerminalId, "OrigemTerminalId");
            AssertEqual(destinoTerminalId, circuit.Cable.DestinoTerminalId, "DestinoTerminalId");
        }

        private static void CaboReancoraVisualmenteAposRotacao()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            Point before = circuit.Cable.Vertices[0];

            RotateSelected(circuit.Context, circuit.Generator);

            Terminal terminal = GetTerminal(circuit.Generator, 0);
            Assert(before != circuit.Cable.Vertices[0], "Vertice do cabo deve mudar apos rotacao.");
            AssertEqual(terminal.Posicao.X, circuit.Cable.Vertices[0].X, "Cabo.Vertices[0].X");
            AssertEqual(terminal.Posicao.Y, circuit.Cable.Vertices[0].Y, "Cabo.Vertices[0].Y");
        }

        private static void UndoRedoRotacaoRestauraElementoECabos()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            Point beforeVertex = circuit.Cable.Vertices[0];

            RotateSelected(circuit.Context, circuit.Generator);
            Point afterVertex = circuit.Cable.Vertices[0];

            circuit.Context.Commands.Undo();
            AssertEqual(0, circuit.Generator.Rotacao, "Rotacao apos undo");
            AssertEqual(beforeVertex.X, circuit.Cable.Vertices[0].X, "Cabo X apos undo");
            AssertEqual(beforeVertex.Y, circuit.Cable.Vertices[0].Y, "Cabo Y apos undo");

            circuit.Context.Commands.Redo();
            AssertEqual(90, circuit.Generator.Rotacao, "Rotacao apos redo");
            AssertEqual(afterVertex.X, circuit.Cable.Vertices[0].X, "Cabo X apos redo");
            AssertEqual(afterVertex.Y, circuit.Cable.Vertices[0].Y, "Cabo Y apos redo");
        }

        private static void RotacaoReancoraCargaComCaboConectado()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            string origemTerminalId = circuit.Cable.OrigemTerminalId;
            string destinoTerminalId = circuit.Cable.DestinoTerminalId;
            Point before = circuit.Cable.Vertices[^1];
            Point middle = new Point(220, 140);
            circuit.Cable.Vertices.Insert(1, middle);

            RotateSelected(circuit.Context, circuit.Load);

            AssertEqual(origemTerminalId, circuit.Cable.OrigemTerminalId, "OrigemTerminalId");
            AssertEqual(destinoTerminalId, circuit.Cable.DestinoTerminalId, "DestinoTerminalId");
            Assert(before != circuit.Cable.Vertices[^1], "Destino do cabo deve mover com a Carga.");
            AssertCableEndpointAtTerminal(circuit.Cable, false, circuit.Load, 0, "Carga destino");
            AssertEqual(middle.X, circuit.Cable.Vertices[1].X, "Intermediario X");
            AssertEqual(middle.Y, circuit.Cable.Vertices[1].Y, "Intermediario Y");
        }

        private static void RotacaoReancoraGeradorComCaboConectado()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            string origemTerminalId = circuit.Cable.OrigemTerminalId;
            Point before = circuit.Cable.Vertices[0];

            RotateSelected(circuit.Context, circuit.Generator);

            AssertEqual(origemTerminalId, circuit.Cable.OrigemTerminalId, "OrigemTerminalId");
            Assert(before != circuit.Cable.Vertices[0], "Origem do cabo deve mover com o Gerador.");
            AssertCableEndpointAtTerminal(circuit.Cable, true, circuit.Generator, 0, "Gerador origem");
        }

        private static void RotacaoReancoraSinEmTodosTerminais()
        {
            EditorContext context = CreateContextWithViewport();
            Sin sin = CreateSin("SIN-ROT-ANCHORS");
            var loads = Enumerable.Range(1, 4)
                .Select(i => CreateLoad($"CARGA-SIN-ROT-{i}", 100 + i, 50 + i))
                .ToList();
            var cables = new List<Cabo>();

            context.Document.AdicionarElemento(sin);

            for (int i = 0; i < loads.Count; i++)
            {
                context.Document.AdicionarElemento(loads[i]);
                Cabo cable = CreateCable(sin, i, loads[i], 0, $"L-SIN-ROT-{i}", 1.0 + i);
                cables.Add(cable);
                context.Document.AdicionarElemento(cable);
            }

            var before = cables.Select(c => c.Vertices[0]).ToList();
            var terminalIds = cables.Select(c => c.OrigemTerminalId).ToList();

            RotateSelected(context, sin);

            for (int i = 0; i < cables.Count; i++)
            {
                AssertEqual(terminalIds[i], cables[i].OrigemTerminalId, $"SIN cabo {i}.OrigemTerminalId");
                Assert(before[i] != cables[i].Vertices[0], $"SIN cabo {i} deve mover.");
                AssertCableEndpointAtTerminal(cables[i], true, sin, i, $"SIN terminal {i}");
            }
        }

        private static void RotacaoReancoraTransformadorPrimarioSecundario()
        {
            EditorContext context = CreateContextWithViewport();
            Sin sin = CreateSin("SIN-TR-ROT");
            Transformador transformador = CreateTransformador("TR-ROT-ANCHORS");
            Carga load = CreateLoad("CARGA-TR-ROT", 300, 100);
            Cabo primary = CreateCable(sin, 1, transformador, 0, "L-TR-ROT-P", 1.0);
            Cabo secondary = CreateCable(transformador, 1, load, 0, "L-TR-ROT-S", 1.1);

            context.Document.AdicionarElemento(sin);
            context.Document.AdicionarElemento(transformador);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(primary);
            context.Document.AdicionarElemento(secondary);

            Point primaryBefore = primary.Vertices[^1];
            Point secondaryBefore = secondary.Vertices[0];

            RotateSelected(context, transformador);

            Assert(primaryBefore != primary.Vertices[^1], "Primario deve reancorar.");
            Assert(secondaryBefore != secondary.Vertices[0], "Secundario deve reancorar.");
            AssertCableEndpointAtTerminal(primary, false, transformador, 0, "Transformador primario");
            AssertCableEndpointAtTerminal(secondary, true, transformador, 1, "Transformador secundario");
        }

        private static void RotacaoReancoraBarraEmDoisTerminais()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            var terminalIds = circuit.Bar.Terminais.Select(t => t.Id).ToList();
            Point incomingBefore = circuit.Incoming.Vertices[^1];
            Point outgoingBefore = circuit.Outgoing.Vertices[0];
            Point middle = new Point(230, 150);
            circuit.Outgoing.Vertices.Insert(1, middle);

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(24, circuit.Bar.Terminais.Count, "Barra.Terminais.Count");

            for (int i = 0; i < terminalIds.Count; i++)
                AssertEqual(terminalIds[i], circuit.Bar.Terminais[i].Id, $"Barra.Terminal[{i}].Id");

            Assert(incomingBefore != circuit.Incoming.Vertices[^1], "Entrada da Barra deve mover.");
            Assert(outgoingBefore != circuit.Outgoing.Vertices[0], "Saida da Barra deve mover.");
            AssertCableEndpointAtTerminal(circuit.Incoming, false, circuit.Bar, 0, "Barra entrada");
            AssertCableEndpointAtTerminal(circuit.Outgoing, true, circuit.Bar, 1, "Barra saida");
            AssertEqual(middle.X, circuit.Outgoing.Vertices[1].X, "Barra intermediario X");
            AssertEqual(middle.Y, circuit.Outgoing.Vertices[1].Y, "Barra intermediario Y");
        }

        private static void UndoRedoRotacaoReancoraTerminaisECabos()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            Point terminalBefore = GetTerminal(circuit.Load, 0).Posicao;
            Point vertexBefore = circuit.Cable.Vertices[^1];

            RotateSelected(circuit.Context, circuit.Load);
            Point terminalAfter = GetTerminal(circuit.Load, 0).Posicao;
            Point vertexAfter = circuit.Cable.Vertices[^1];

            circuit.Context.Commands.Undo();
            AssertEqual(0, circuit.Load.Rotacao, "Carga.Rotacao undo");
            AssertEqual(terminalBefore.X, GetTerminal(circuit.Load, 0).Posicao.X, "Terminal X undo");
            AssertEqual(terminalBefore.Y, GetTerminal(circuit.Load, 0).Posicao.Y, "Terminal Y undo");
            AssertEqual(vertexBefore.X, circuit.Cable.Vertices[^1].X, "Cabo X undo");
            AssertEqual(vertexBefore.Y, circuit.Cable.Vertices[^1].Y, "Cabo Y undo");

            circuit.Context.Commands.Redo();
            AssertEqual(90, circuit.Load.Rotacao, "Carga.Rotacao redo");
            AssertEqual(terminalAfter.X, GetTerminal(circuit.Load, 0).Posicao.X, "Terminal X redo");
            AssertEqual(terminalAfter.Y, GetTerminal(circuit.Load, 0).Posicao.Y, "Terminal Y redo");
            AssertEqual(vertexAfter.X, circuit.Cable.Vertices[^1].X, "Cabo X redo");
            AssertEqual(vertexAfter.Y, circuit.Cable.Vertices[^1].Y, "Cabo Y redo");
        }

        private static void SnapEncontraTerminalAposRotacaoComCabo()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();

            RotateSelected(circuit.Context, circuit.Load);

            Terminal expected = GetTerminal(circuit.Load, 0);
            Terminal? snapped = circuit.Context.Snap.ObterTerminalMaisProximo(expected.Posicao);

            Assert(snapped != null, "Snap deve encontrar terminal rotacionado.");
            AssertEqual(expected.Id, snapped!.Id, "Snap.TerminalId");
            AssertEqual(expected.Dono.Id, snapped.Dono.Id, "Snap.Dono");
        }

        private static void ElectricGraphBuildAposRotacaoNaoAlteraDocument()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            RotateSelected(circuit.Context, circuit.Generator);
            int count = circuit.Context.Document.Elementos.Count;

            _ = new ElectricGraphBuilder(circuit.Context.Document).Build();
            _ = new ElectricGraphBuilder(circuit.Context.Document).Build();

            AssertEqual(count, circuit.Context.Document.Elementos.Count, "Quantidade de elementos apos builds");
        }

        private static void DtoNaoMudaPorCausaDaRotacao()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            CircuitDto before = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            RotateSelected(circuit.Context, circuit.Generator);
            CircuitDto after = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            AssertEqual(before.Slack!.Id, after.Slack!.Id, "Slack.Id");
            AssertEqual(before.Lines.Single().Barra1, after.Lines.Single().Barra1, "Line.Barra1");
            AssertEqual(before.Lines.Single().Barra2, after.Lines.Single().Barra2, "Line.Barra2");
            AssertEqual(before.Loads.Single().Barra, after.Loads.Single().Barra, "Load.Barra");
        }

        private static void RotationServiceAceitaBarra()
        {
            EditorContext context = CreateContextWithViewport();
            Barra bar = CreateBar("BARRA-PODE-ROT");
            context.Document.AdicionarElemento(bar);

            Assert(RotationService.PodeRotacionar(GetVm(context, bar)), "Barra deve ser aceita para rotacao.");
        }

        private static void BarraNovaPossuiAlturaPadrao()
        {
            Barra bar = new();

            AssertEqual(Barra.ALTURA_PADRAO, bar.Altura, "Altura padrao da Barra");
            AssertEqual(24, bar.Terminais.Count, "Quantidade de terminais da Barra");
        }

        private static void AlterarAlturaDaBarraMudaBounds()
        {
            EditorContext context = CreateContextWithViewport();
            Barra bar = CreateBar("BARRA-ALT-BOUNDS");
            context.Document.AdicionarElemento(bar);
            BarraViewModel vm = GetBarVm(context, bar);

            double before = vm.Bounds.Height;
            vm.Altura = 220;

            AssertEqual(220, bar.Altura, "Barra.Altura");
            AssertEqual(220, vm.Bounds.Height, "Bounds.Height");
            Assert(before != vm.Bounds.Height, "Bounds deve mudar apos alterar altura.");
        }

        private static void BarraPadraoMantemVinteQuatroTerminaisComPitchFixo()
        {
            Barra bar = CreateBar("BARRA-PITCH-PADRAO");

            AssertEqual(24, bar.Terminais.Count, "Quantidade de terminais padrao");
            AssertNoDuplicateTerminalIds(bar, "Barra padrao");
            AssertEqual("BARRA-01", bar.Terminais[0].Id, "Primeiro terminal");
            AssertEqual("BARRA-24", bar.Terminais[^1].Id, "Ultimo terminal");
            AssertEqual(0, bar.Terminais[0].PosicaoLocal.Y, "Primeiro terminal local Y");
            AssertEqual(Barra.ALTURA_PADRAO, bar.Terminais[^1].PosicaoLocal.Y, "Ultimo terminal local Y");
            AssertTerminaisDaBarraSeguemPitchFixo(bar, "Barra padrao");
        }

        private static void CrescerBarraAumentaConectoresPreservandoIds()
        {
            Barra bar = CreateBar("BARRA-PITCH-CRESCE");
            var idsIniciais = bar.Terminais.Select(t => t.Id).ToList();

            bar.Altura = 240;
            bar.AtualizarTerminais();

            Assert(bar.Terminais.Count > idsIniciais.Count, "Quantidade deve aumentar.");
            AssertNoDuplicateTerminalIds(bar, "Barra aumentada");

            for (int i = 0; i < idsIniciais.Count; i++)
                AssertEqual(idsIniciais[i], bar.Terminais[i].Id, $"Terminal existente {i}.Id");

            AssertEqual("BARRA-25", bar.Terminais[24].Id, "Primeiro terminal novo");
            AssertTerminaisDaBarraSeguemPitchFixo(bar, "Barra aumentada");
        }

        private static void ReduzirBarraRemoveTerminaisLivresExcedentes()
        {
            Barra bar = CreateBar("BARRA-PITCH-REDUZ");
            bar.Altura = 240;
            bar.AtualizarTerminais();
            int quantidadeAumentada = bar.Terminais.Count;

            bar.Altura = Barra.ALTURA_MINIMA;
            bar.AtualizarTerminais();

            Assert(bar.Terminais.Count < quantidadeAumentada, "Quantidade deve reduzir.");
            AssertNoDuplicateTerminalIds(bar, "Barra reduzida");
            AssertTerminaisDaBarraSeguemPitchFixo(bar, "Barra reduzida");

            foreach (Terminal terminal in bar.Terminais)
                Assert(terminal.PosicaoLocal.Y <= bar.Altura + 0.000001, $"{terminal.Id}: Y deve ficar dentro da altura.");
        }

        private static void ReduzirBarraPreservaTerminalOcupado()
        {
            BarResizeCircuit circuit = CreateBarResizeCircuit();
            Terminal terminalAlto = GetTerminal(circuit.Bar, "BARRA-30");
            Cabo cable = CreateCable(circuit.Bar, terminalAlto, circuit.OtherBar, circuit.OtherBar.Terminais[0], "L-BARRA-ALTA", 1.0);
            circuit.Document.AdicionarElemento(cable);

            circuit.Context.GeometryUpdates.AplicarAlturaBarra(circuit.Bar, Barra.ALTURA_MINIMA);

            Terminal preservado = AssertTerminalExists(circuit.Bar, terminalAlto.Id);
            AssertNoDuplicateTerminalIds(circuit.Bar, "Barra reduzida com cabo");
            Assert(preservado.PosicaoLocal.Y <= circuit.Bar.Altura + 0.000001, "Terminal ocupado deve ficar em posicao local valida.");
            AssertEqual(terminalAlto.Id, cable.OrigemTerminalId, "OrigemTerminalId preservado");
            AssertEqual(preservado.Posicao.X, cable.Vertices[0].X, "Cabo origem X reancorado");
            AssertEqual(preservado.Posicao.Y, cable.Vertices[0].Y, "Cabo origem Y reancorado");
        }

        private static void ResizeDaBarraReancoraCaboConectado()
        {
            BarResizeCircuit circuit = CreateBarResizeCircuit();
            Terminal terminal = GetTerminal(circuit.Bar, "BARRA-24");
            Cabo cable = CreateCable(circuit.Bar, terminal, circuit.OtherBar, circuit.OtherBar.Terminais[0], "L-BARRA-REANCORA", 1.0);
            circuit.Document.AdicionarElemento(cable);
            string endpoint = cable.OrigemTerminalId;

            circuit.Context.GeometryUpdates.AplicarAlturaBarra(circuit.Bar, 240);

            Terminal atual = AssertTerminalExists(circuit.Bar, endpoint);
            AssertEqual(endpoint, cable.OrigemTerminalId, "Endpoint preservado");
            AssertEqual(atual.Posicao.X, cable.Vertices[0].X, "Cabo origem X reancorado");
            AssertEqual(atual.Posicao.Y, cable.Vertices[0].Y, "Cabo origem Y reancorado");
        }

        private static void UndoRedoResizeBarraPreservaCabo()
        {
            BarResizeCircuit circuit = CreateBarResizeCircuit();
            Terminal terminal = GetTerminal(circuit.Bar, "BARRA-30");
            Cabo cable = CreateCable(circuit.Bar, terminal, circuit.OtherBar, circuit.OtherBar.Terminais[0], "L-BARRA-UNDO", 1.0);
            circuit.Document.AdicionarElemento(cable);
            string terminalId = terminal.Id;
            var command = new ResizeBarraCommand(
                circuit.Bar,
                240,
                circuit.Bar.PosicaoX,
                circuit.Bar.PosicaoY,
                Barra.ALTURA_MINIMA,
                circuit.Bar.PosicaoX,
                circuit.Bar.PosicaoY,
                circuit.Context.GeometryUpdates);

            command.Execute();
            AssertResizePreservaCabo(circuit.Bar, cable, terminalId, Barra.ALTURA_MINIMA, "Execute");

            command.Undo();
            AssertResizePreservaCabo(circuit.Bar, cable, terminalId, 240, "Undo");

            command.Redo();
            AssertResizePreservaCabo(circuit.Bar, cable, terminalId, Barra.ALTURA_MINIMA, "Redo");
        }

        private static void ConnectivityRetornaTerminaisOcupadosDaBarra()
        {
            BarResizeCircuit circuit = CreateBarResizeCircuit();
            Terminal ocupado = GetTerminal(circuit.Bar, "BARRA-30");
            Terminal livre = GetTerminal(circuit.Bar, "BARRA-29");
            Cabo cable = CreateCable(circuit.Bar, ocupado, circuit.OtherBar, circuit.OtherBar.Terminais[0], "L-BARRA-OCUPADOS", 1.0);
            circuit.Document.AdicionarElemento(cable);

            IReadOnlySet<string> ocupados = circuit.Context.Connectivity.ObterTerminalIdsOcupados(circuit.Bar);

            Assert(ocupados.Contains(ocupado.Id), "Terminal ocupado deve aparecer no conjunto.");
            Assert(!ocupados.Contains(livre.Id), "Terminal livre nao deve aparecer no conjunto.");
        }

        private static void CaboConectadoABarraReancoraAposAlterarAltura()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            string origemId = circuit.Outgoing.OrigemId;
            string destinoId = circuit.Outgoing.DestinoId;
            string origemTerminalId = circuit.Outgoing.OrigemTerminalId;
            string destinoTerminalId = circuit.Outgoing.DestinoTerminalId;
            Point before = circuit.Outgoing.Vertices[0];
            Point middle = new Point(230, 150);
            circuit.Outgoing.Vertices.Insert(1, middle);

            SetBarHeight(circuit.Context, circuit.Bar, 240);

            AssertEqual(GetTerminal(circuit.Bar, 1).Posicao.X, circuit.Outgoing.Vertices[0].X, "Ponta conectada X");
            AssertEqual(GetTerminal(circuit.Bar, 1).Posicao.Y, circuit.Outgoing.Vertices[0].Y, "Ponta conectada Y");
            AssertCableEndpointAtTerminal(circuit.Outgoing, true, circuit.Bar, 1, "Barra saida apos altura");
            AssertEqual(middle.X, circuit.Outgoing.Vertices[1].X, "Intermediario X preservado");
            AssertEqual(middle.Y, circuit.Outgoing.Vertices[1].Y, "Intermediario Y preservado");
            AssertEqual(origemId, circuit.Outgoing.OrigemId, "OrigemId preservado");
            AssertEqual(destinoId, circuit.Outgoing.DestinoId, "DestinoId preservado");
            AssertEqual(origemTerminalId, circuit.Outgoing.OrigemTerminalId, "OrigemTerminalId preservado");
            AssertEqual(destinoTerminalId, circuit.Outgoing.DestinoTerminalId, "DestinoTerminalId preservado");
        }

        private static void BarraComAlturaAlteradaPersisteAposReload()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            SetBarHeight(circuit.Context, circuit.Bar, 260);

            AraciDocument loaded = SaveAndLoad(circuit.Context.Document);
            Barra loadedBar = FindById<Barra>(loaded, circuit.Bar.Id);

            AssertEqual(260, loadedBar.Altura, "Altura apos reload");
            Assert(loadedBar.Terminais[^1].PosicaoLocal.Y <= loadedBar.Altura + 0.000001, "Ultimo terminal apos reload deve ficar dentro da altura.");
            AssertTerminaisDaBarraSeguemPitchFixo(loadedBar, "Barra apos reload");
            AssertCableEndpointAtTerminal(
                FindById<Cabo>(loaded, circuit.Outgoing.Id),
                true,
                loadedBar,
                1,
                "Cabo saida apos reload");
        }

        private static void ElectricGraphContinuaValidoAposAlturaDaBarra()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            SetBarHeight(circuit.Context, circuit.Bar, 240);

            ElectricGraph graph = new ElectricGraphBuilder(circuit.Context.Document).Build();

            AssertEqual(2, graph.Edges.Count, "Quantidade de arestas");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Arestas invalidas");
            AssertEqual(2, graph.GetEdgesForElement(circuit.Bar.Id.ToString()).Count, "Arestas da Barra");
        }

        private static void DtoNaoMudaPorCausaDaAlturaDaBarra()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            CircuitDto before = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            SetBarHeight(circuit.Context, circuit.Bar, 240);
            CircuitDto after = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            AssertEqual(before.Slack!.Id, after.Slack!.Id, "Slack.Id");
            AssertEqual(before.Lines.Count, after.Lines.Count, "Lines.Count");
            AssertEqual(before.Loads.Count, after.Loads.Count, "Loads.Count");
            AssertEqual(before.Lines[0].Barra1, after.Lines[0].Barra1, "Line[0].Barra1");
            AssertEqual(before.Lines[0].Barra2, after.Lines[0].Barra2, "Line[0].Barra2");
            AssertEqual(before.Lines[1].Barra1, after.Lines[1].Barra1, "Line[1].Barra1");
            AssertEqual(before.Lines[1].Barra2, after.Lines[1].Barra2, "Line[1].Barra2");
            AssertEqual(before.Loads.Single().Barra, after.Loads.Single().Barra, "Load.Barra");
        }

        private static void RotacaoDaBarraFuncionaAposAlturaAlterada()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            SetBarHeight(circuit.Context, circuit.Bar, 240);
            Point before = circuit.Outgoing.Vertices[0];

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(90, circuit.Bar.Rotacao, "Rotacao da Barra");
            Assert(before != circuit.Outgoing.Vertices[0], "Cabo deve reancorar apos rotacao com altura alterada.");
            AssertCableEndpointAtTerminal(circuit.Outgoing, true, circuit.Bar, 1, "Barra saida apos altura e rotacao");
        }

        private static void CaboPermaneceAncoradoAposAlturaRotacaoMovimentoEReload()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            Point middle = new Point(230, 150);
            circuit.Outgoing.Vertices.Insert(1, middle);

            SetBarHeight(circuit.Context, circuit.Bar, 240);
            RotateSelected(circuit.Context, circuit.Bar);
            MoveElement(circuit.Context, circuit.Bar, new Vector(35, 20));

            AraciDocument loaded = SaveAndLoad(circuit.Context.Document);
            Barra loadedBar = FindById<Barra>(loaded, circuit.Bar.Id);
            Cabo loadedOutgoing = FindById<Cabo>(loaded, circuit.Outgoing.Id);

            AssertEqual(240, loadedBar.Altura, "Altura apos sequencia e reload");
            AssertEqual(90, loadedBar.Rotacao, "Rotacao apos sequencia e reload");
            AssertCableEndpointAtTerminal(loadedOutgoing, true, loadedBar, 1, "Cabo apos sequencia e reload");
            AssertEqual(middle.X, loadedOutgoing.Vertices[1].X, "Intermediario X apos sequencia e reload");
            AssertEqual(middle.Y, loadedOutgoing.Vertices[1].Y, "Intermediario Y apos sequencia e reload");
        }

        private static void AlturaInvalidaDaBarraNormalizaParaMinimo()
        {
            Barra bar = CreateBar("BARRA-ALT-MIN");

            bar.Altura = -10;
            bar.AtualizarTerminais();

            AssertEqual(Barra.ALTURA_MINIMA, bar.Altura, "Altura minima");
            Assert(bar.Terminais[^1].PosicaoLocal.Y <= bar.Altura + 0.000001, "Ultimo terminal com altura minima deve ficar dentro da altura.");
            AssertTerminaisDaBarraSeguemPitchFixo(bar, "Barra com altura minima");
        }

        private static void BarraSelecionadaRotacionaZeroParaNoventa()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(90, circuit.Bar.Rotacao, "Rotacao da Barra");
        }

        private static void BarraCiclaQuadrantes()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);
            AssertEqual(90, circuit.Bar.Rotacao, "Rotacao 0 -> 90");
            RotateSelected(circuit.Context, circuit.Bar);
            AssertEqual(180, circuit.Bar.Rotacao, "Rotacao 90 -> 180");
            RotateSelected(circuit.Context, circuit.Bar);
            AssertEqual(270, circuit.Bar.Rotacao, "Rotacao 180 -> 270");
            RotateSelected(circuit.Context, circuit.Bar);
            AssertEqual(0, circuit.Bar.Rotacao, "Rotacao 270 -> 0");
        }

        private static void PreviewDeBarraPreservaRotacao()
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<BarraViewModel, Barra>(
                context,
                context.ElementoFactory.CriarBarraVM,
                vm => vm.Barra);

            controller.Update(new Point(240, 180));
            controller.RotateClockwise();

            Barra real = context.ElementoFactory.CriarBarra();
            real.Rotacao = controller.CurrentRotation;

            AssertEqual(90, real.Rotacao, "Rotacao copiada do preview da Barra");
        }

        private static void BarraPreservaVinteQuatroTerminalIdsAposRotacao()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            var before = circuit.Bar.Terminais.Select(t => t.Id).ToList();

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(24, circuit.Bar.Terminais.Count, "Quantidade de terminais");

            for (int i = 0; i < 24; i++)
            {
                string expected = $"BARRA-{i + 1:00}";
                AssertEqual(expected, circuit.Bar.Terminais[i].Id, $"Terminal {i}.Id padrao");
                AssertEqual(before[i], circuit.Bar.Terminais[i].Id, $"Terminal {i}.Id preservado");
            }
        }

        private static void TerminaisDaBarraMudamPosicaoVisualAposRotacao()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            var before = circuit.Bar.Terminais
                .Select(t => (t.Id, t.Posicao))
                .ToList();

            RotateSelected(circuit.Context, circuit.Bar);

            Assert(
                before.Any(item => circuit.Bar.Terminais.Single(t => t.Id == item.Id).Posicao != item.Posicao),
                "Ao menos um terminal da Barra deve mudar de posicao visual apos rotacao.");
        }

        private static void CaboConectadoABarraPreservaTerminalIdAposRotacao()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            string origemTerminalId = circuit.Outgoing.OrigemTerminalId;
            string destinoTerminalId = circuit.Incoming.DestinoTerminalId;

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(origemTerminalId, circuit.Outgoing.OrigemTerminalId, "Cabo saida OrigemTerminalId");
            AssertEqual(destinoTerminalId, circuit.Incoming.DestinoTerminalId, "Cabo entrada DestinoTerminalId");
        }

        private static void CaboConectadoABarraReancoraVisualmenteAposRotacao()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            Point before = circuit.Outgoing.Vertices[0];
            Point middle = new Point(230, 150);
            circuit.Outgoing.Vertices.Insert(1, middle);

            RotateSelected(circuit.Context, circuit.Bar);

            Terminal terminal = GetTerminal(circuit.Bar, 1);
            Assert(before != circuit.Outgoing.Vertices[0], "Vertice inicial deve mudar apos rotacao da Barra.");
            AssertEqual(terminal.Posicao.X, circuit.Outgoing.Vertices[0].X, "Cabo.Vertices[0].X");
            AssertEqual(terminal.Posicao.Y, circuit.Outgoing.Vertices[0].Y, "Cabo.Vertices[0].Y");
            AssertEqual(middle.X, circuit.Outgoing.Vertices[1].X, "Vertice intermediario X preservado");
            AssertEqual(middle.Y, circuit.Outgoing.Vertices[1].Y, "Vertice intermediario Y preservado");
        }

        private static void UndoRedoRotacaoDaBarraRestauraCabos()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            Point beforeVertex = circuit.Outgoing.Vertices[0];

            RotateSelected(circuit.Context, circuit.Bar);
            Point afterVertex = circuit.Outgoing.Vertices[0];

            circuit.Context.Commands.Undo();
            AssertEqual(0, circuit.Bar.Rotacao, "Rotacao da Barra apos undo");
            AssertEqual(beforeVertex.X, circuit.Outgoing.Vertices[0].X, "Cabo X apos undo");
            AssertEqual(beforeVertex.Y, circuit.Outgoing.Vertices[0].Y, "Cabo Y apos undo");

            circuit.Context.Commands.Redo();
            AssertEqual(90, circuit.Bar.Rotacao, "Rotacao da Barra apos redo");
            AssertEqual(afterVertex.X, circuit.Outgoing.Vertices[0].X, "Cabo X apos redo");
            AssertEqual(afterVertex.Y, circuit.Outgoing.Vertices[0].Y, "Cabo Y apos redo");
        }

        private static void BarraRotacionadaPersisteAposReload()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);
            AraciDocument loaded = SaveAndLoad(circuit.Context.Document);

            Barra loadedBar = FindById<Barra>(loaded, circuit.Bar.Id);
            Cabo loadedOutgoing = FindById<Cabo>(loaded, circuit.Outgoing.Id);

            AssertEqual(90, loadedBar.Rotacao, "Rotacao da Barra apos reload");
            AssertEqual(circuit.Outgoing.OrigemTerminalId, loadedOutgoing.OrigemTerminalId, "OrigemTerminalId apos reload");
            AssertEqual(circuit.Outgoing.Vertices[0].X, loadedOutgoing.Vertices[0].X, "Vertice X apos reload");
            AssertEqual(circuit.Outgoing.Vertices[0].Y, loadedOutgoing.Vertices[0].Y, "Vertice Y apos reload");
        }

        private static void ElectricGraphAposRotacaoDaBarraMantemArestasValidas()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);
            ElectricGraph graph = new ElectricGraphBuilder(circuit.Context.Document).Build();

            AssertEqual(2, graph.Edges.Count, "Quantidade de arestas");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Arestas invalidas");
            AssertEqual(2, graph.GetEdgesForElement(circuit.Bar.Id.ToString()).Count, "Arestas da Barra");
        }

        private static void DtoNaoMudaPorCausaDaRotacaoDaBarra()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            CircuitDto before = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            RotateSelected(circuit.Context, circuit.Bar);
            CircuitDto after = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            AssertEqual(before.Slack!.Id, after.Slack!.Id, "Slack.Id");
            AssertEqual(before.Lines.Count, after.Lines.Count, "Lines.Count");
            AssertEqual(before.Loads.Count, after.Loads.Count, "Loads.Count");
            AssertEqual(before.Lines[0].Barra1, after.Lines[0].Barra1, "Line[0].Barra1");
            AssertEqual(before.Lines[0].Barra2, after.Lines[0].Barra2, "Line[0].Barra2");
            AssertEqual(before.Lines[1].Barra1, after.Lines[1].Barra1, "Line[1].Barra1");
            AssertEqual(before.Lines[1].Barra2, after.Lines[1].Barra2, "Line[1].Barra2");
            AssertEqual(before.Loads.Single().Barra, after.Loads.Single().Barra, "Load.Barra");
        }

        private static void HitTestEncontraBarraRotacionada()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            ElementoViewModel vm = GetVm(circuit.Context, circuit.Bar);

            RotateSelected(circuit.Context, circuit.Bar);

            Point visualPoint = RotateAround(
                new Point(circuit.Bar.PosicaoX + 5, circuit.Bar.PosicaoY + 8),
                vm.Centro,
                circuit.Bar.Rotacao);

            ElementoViewModel? hit = circuit.Context.SceneQueries.HitTest(visualPoint)?.Elemento;

            Assert(ReferenceEquals(vm, hit), "Hit-test deve retornar a Barra rotacionada.");
        }

        private static void SnapEncontraTerminalDeBarraRotacionada()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);

            Terminal expected = GetTerminal(circuit.Bar, 23);
            Terminal? snapped = circuit.Context.Snap.ObterTerminalMaisProximo(expected.Posicao);

            Assert(snapped != null, "Snap deve encontrar terminal da Barra rotacionada.");
            AssertEqual(expected.Id, snapped!.Id, "TerminalId do snap");
            AssertEqual(circuit.Bar.Id, snapped.Dono.Id, "Dono do terminal do snap");
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

        private static Elemento CreateAnnotation(string name)
        {
            return new FakeAnnotationElement
            {
                Nome = name,
                PosicaoX = 40,
                PosicaoY = 40
            };
        }

        private static string SerializeCircuitDto(CircuitDto dto)
        {
            return JsonSerializer.Serialize(dto);
        }

        private static RotatedCircuit CreateRotatedCircuit()
        {
            EditorContext context = CreateContextWithViewport();
            Gerador generator = CreateGenerator("GER-ROT-CABO", 1000, 0.95);
            Carga load = CreateLoad("CARGA-ROT-CABO", 300, 100);
            Cabo cable = CreateCable(generator, 0, load, 0, "L-ROT-CABO", 1.0);

            context.Document.AdicionarElemento(generator);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(cable);

            return new RotatedCircuit(context, generator, load, cable);
        }

        private static BarRotationCircuit CreateBarRotationCircuit()
        {
            EditorContext context = CreateContextWithViewport();
            Gerador generator = CreateGenerator("GER-BARRA-ROT", 1000, 0.95);
            Barra bar = CreateBar("BARRA-ROT");
            Carga load = CreateLoad("CARGA-BARRA-ROT", 300, 100);
            Cabo incoming = CreateCable(generator, 0, bar, 0, "L-BARRA-IN", 1.0);
            Cabo outgoing = CreateCable(bar, 1, load, 0, "L-BARRA-OUT", 1.0);

            context.Document.AdicionarElemento(generator);
            context.Document.AdicionarElemento(bar);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(incoming);
            context.Document.AdicionarElemento(outgoing);

            return new BarRotationCircuit(context, generator, bar, load, incoming, outgoing);
        }

        private static BarResizeCircuit CreateBarResizeCircuit()
        {
            EditorContext context = CreateContextWithViewport();
            Barra bar = CreateBar("BARRA-RESIZE-A");
            Barra otherBar = CreateBar("BARRA-RESIZE-B");
            otherBar.PosicaoX = 300;
            context.Document.AdicionarElemento(bar);
            context.Document.AdicionarElemento(otherBar);
            context.GeometryUpdates.AplicarAlturaBarra(bar, 240);
            return new BarResizeCircuit(context, context.Document, bar, otherBar);
        }

        private static EditorContext CreateContextWithViewport()
        {
            var context = new EditorContext();
            var viewport = context.CriarViewportViewModel();

            context.InicializarViewport(viewport);
            return context;
        }

        private static InsertPreviewController<TViewModel, TModel> CriarPreviewController<TViewModel, TModel>(
            EditorContext context,
            Func<TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            return new InsertPreviewController<TViewModel, TModel>(
                criarPreview,
                obterModelo,
                context.Snap,
                context.Geometry,
                context.TerminalLayout,
                context.AlignmentGuides,
                context.Scene,
                context.SceneQueries);
        }

        private static void RotateSelected(EditorContext context, Elemento elemento)
        {
            ElementoViewModel vm = GetVm(context, elemento);

            context.Selection.Selecionar(vm);
            Assert(context.Rotation.RotateSelectionClockwise(), "Rotacao da selecao deve ser aplicada.");
        }

        private static void SetBarHeight(EditorContext context, Barra bar, double height)
        {
            GetBarVm(context, bar).Altura = height;
        }

        private static void MoveElement(EditorContext context, Elemento elemento, Vector delta)
        {
            ElementoViewModel vm = GetVm(context, elemento);

            context.Move.BeginMove(new[] { vm });
            context.Move.MoverVisual(vm, delta);
            context.Move.EndMove(new[] { vm });
        }

        private static BarraViewModel GetBarVm(EditorContext context, Barra bar)
        {
            if (GetVm(context, bar) is not BarraViewModel vm)
                throw new InvalidOperationException($"ViewModel da Barra '{bar.Nome}' nao encontrado.");

            return vm;
        }

        private static ElementoViewModel GetVm(EditorContext context, Elemento elemento)
        {
            ElementoViewModel? vm = context.Viewport?.ObterViewModel(elemento);

            if (vm == null)
                throw new InvalidOperationException($"ViewModel de '{elemento.Nome}' nao encontrado.");

            return vm;
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
                FatorPotencia = fp,
                TensaoLinha = "13.8"
            };

            generator.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

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

            load.AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura);

            return load;
        }

        private static Barra CreateBar(string name)
        {
            var bar = new Barra
            {
                Nome = name,
                PosicaoX = 200,
                PosicaoY = 100
            };

            bar.AtualizarTerminais();

            return bar;
        }

        private static Sin CreateSin(string name)
        {
            var sin = new Sin
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoSin
                {
                    Fases = 3,
                    PotenciaCurtoMVA = 500,
                    RelacaoXR = 10
                },
                PosicaoX = 80,
                PosicaoY = 80,
                TensaoLinha = "13.8"
            };

            sin.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            return sin;
        }

        private static Transformador CreateTransformador(string name)
        {
            var transformador = new Transformador
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoTransformador
                {
                    Fases = 3,
                    Enrolamentos = 2
                },
                PosicaoX = 80,
                PosicaoY = 80,
                TensaoLinha = "13.8"
            };

            transformador.TensaoPrimarioKV = 13.8;
            transformador.TensaoSecundarioKV = 0.38;
            transformador.PotenciaAparente = 500;

            transformador.AtualizarTerminais(
                ElementGeometryDefaults.TransformadorLargura,
                ElementGeometryDefaults.TransformadorAltura);

            return transformador;
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

        private static Cabo CreateCable(
            Elemento from,
            Terminal fromTerminal,
            Elemento to,
            Terminal toTerminal,
            string name,
            double length)
        {
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

        private static Terminal GetTerminal(Barra barra, string terminalId)
        {
            return barra.Terminais.First(t =>
                string.Equals(t.Id, terminalId, StringComparison.OrdinalIgnoreCase));
        }

        private static void AssertCableEndpointAtTerminal(
            Cabo cable,
            bool origin,
            Elemento elemento,
            int terminalIndex,
            string name)
        {
            Terminal terminal = GetTerminal(elemento, terminalIndex);
            Point vertex = origin
                ? cable.Vertices[0]
                : cable.Vertices[^1];
            string terminalId = origin
                ? cable.OrigemTerminalId
                : cable.DestinoTerminalId;

            AssertEqual(terminal.Id, terminalId, $"{name}.TerminalId");
            AssertEqual(terminal.Posicao.X, vertex.X, $"{name}.Vertice.X");
            AssertEqual(terminal.Posicao.Y, vertex.Y, $"{name}.Vertice.Y");
        }

        private static void AssertNoDuplicateTerminalIds(Barra barra, string name)
        {
            int idsUnicos = barra.Terminais
                .Select(t => t.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            AssertEqual(barra.Terminais.Count, idsUnicos, $"{name}.TerminalIds unicos");
        }

        private static Terminal AssertTerminalExists(Barra barra, string terminalId)
        {
            Terminal? terminal = barra.Terminais.FirstOrDefault(t =>
                string.Equals(t.Id, terminalId, StringComparison.OrdinalIgnoreCase));

            Assert(terminal != null, $"Terminal '{terminalId}' deve existir na barra '{barra.Nome}'.");
            return terminal!;
        }

        private static void AssertTerminaisDaBarraSeguemPitchFixo(Barra barra, string name)
        {
            double pitch = Barra.ALTURA_PADRAO / 23;

            foreach (Terminal terminal in barra.Terminais)
            {
                int slot = int.Parse(terminal.Id["BARRA-".Length..]) - 1;
                double expected = Math.Min(slot * pitch, barra.Altura);
                AssertEqual(expected, terminal.PosicaoLocal.Y, $"{name}.{terminal.Id}.Y");
            }
        }

        private static void AssertResizePreservaCabo(
            Barra barra,
            Cabo cable,
            string terminalId,
            double alturaEsperada,
            string name)
        {
            AssertEqual(alturaEsperada, barra.Altura, $"{name}.Altura");
            Terminal terminal = AssertTerminalExists(barra, terminalId);
            AssertNoDuplicateTerminalIds(barra, $"{name}.Barra");
            AssertEqual(terminalId, cable.OrigemTerminalId, $"{name}.OrigemTerminalId");
            AssertEqual(terminal.Posicao.X, cable.Vertices[0].X, $"{name}.Cabo.X");
            AssertEqual(terminal.Posicao.Y, cable.Vertices[0].Y, $"{name}.Cabo.Y");
        }

        private static void AssertTerminalsUseCentralPivot(
            Elemento elemento,
            double width,
            double height,
            string name)
        {
            if (elemento is not ITerminalOwner owner)
                throw new InvalidOperationException($"{name}: elemento sem terminais.");

            foreach (Terminal terminal in owner.Terminais)
            {
                Point expected = ExpectedCentralWorld(
                    elemento,
                    terminal.PosicaoLocal,
                    width,
                    height);

                AssertEqual(expected.X, terminal.Posicao.X, $"{name}.{terminal.Id}.X");
                AssertEqual(expected.Y, terminal.Posicao.Y, $"{name}.{terminal.Id}.Y");
            }
        }

        private static Point ExpectedCentralWorld(
            Elemento owner,
            Point local,
            double width,
            double height)
        {
            double scale = owner.Escala == 0 ? 1 : owner.Escala;
            double pivotX = width / 2;
            double pivotY = height / 2;
            double x = (local.X - pivotX) * scale;
            double y = (local.Y - pivotY) * scale;
            double radians = owner.Rotacao * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            return new Point(
                owner.PosicaoX + pivotX + x * cos - y * sin,
                owner.PosicaoY + pivotY + x * sin + y * cos);
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

        private static void AssertSinTerminals(Sin sin, string name)
        {
            AssertEqual(4, sin.Terminais.Count, $"{name}.Terminais.Count");
            AssertEqual(Sin.TERMINAL_NORTE, sin.Terminais[0].Id, $"{name}.Terminal[0]");
            AssertEqual(Sin.TERMINAL_SUL, sin.Terminais[1].Id, $"{name}.Terminal[1]");
            AssertEqual(Sin.TERMINAL_LESTE, sin.Terminais[2].Id, $"{name}.Terminal[2]");
            AssertEqual(Sin.TERMINAL_OESTE, sin.Terminais[3].Id, $"{name}.Terminal[3]");

            foreach (Terminal terminal in sin.Terminais)
                AssertEqual(sin.Barra, terminal.Barra ?? string.Empty, $"{name}.{terminal.Id}.Barra");
        }

        private static void AssertTransformadorTerminals(Transformador transformador, string name)
        {
            AssertEqual(2, transformador.Terminais.Count, $"{name}.Terminais.Count");
            AssertEqual(Transformador.TERMINAL_PRIMARIO, transformador.Terminais[0].Id, $"{name}.Terminal[0]");
            AssertEqual(Transformador.TERMINAL_SECUNDARIO, transformador.Terminais[1].Id, $"{name}.Terminal[1]");

            foreach (Terminal terminal in transformador.Terminais)
                AssertEqual(transformador.Barra, terminal.Barra ?? string.Empty, $"{name}.{terminal.Id}.Barra");
        }

        private static void AssertGraphTerminal(ElectricGraphNode node, string terminalId, string name)
        {
            bool exists = node.Terminals.Any(t =>
                string.Equals(t.TerminalId, terminalId, StringComparison.OrdinalIgnoreCase));

            Assert(exists, $"{name}: terminal '{terminalId}' nao encontrado.");
        }

        private static void AssertCableEndpoint(
            AraciDocument loaded,
            Cabo expectedCable,
            Sin expectedSin,
            string expectedSinTerminalId,
            Carga expectedLoad,
            string name)
        {
            Cabo loadedCable = FindById<Cabo>(loaded, expectedCable.Id);

            AssertEqual(expectedSin.Id.ToString(), loadedCable.OrigemId, $"{name}.OrigemId");
            AssertEqual(expectedLoad.Id.ToString(), loadedCable.DestinoId, $"{name}.DestinoId");
            AssertEqual(expectedSinTerminalId, loadedCable.OrigemTerminalId, $"{name}.OrigemTerminalId");
            AssertEqual(expectedCable.DestinoTerminalId, loadedCable.DestinoTerminalId, $"{name}.DestinoTerminalId");
        }

        private static OperationalGraphState BuildOperationalState(AraciDocument document)
        {
            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            return new OperationalGraphStateBuilder().Build(graph);
        }

        private static void AssertEnergized(
            OperationalGraphState state,
            Elemento elemento,
            string name)
        {
            Assert(
                state.IsNodeEnergized(elemento.Id.ToString()),
                $"{name}: elemento deveria estar energizado.");
        }

        private static void AssertDeenergized(
            OperationalGraphState state,
            Elemento elemento,
            string name)
        {
            Assert(
                !state.IsNodeEnergized(elemento.Id.ToString()) &&
                state.DeenergizedNodeIds.Contains(elemento.Id.ToString()),
                $"{name}: elemento deveria estar desenergizado.");
        }

        private static void AssertEdgeEnergized(
            OperationalGraphState state,
            Cabo cabo,
            string name)
        {
            Assert(
                state.IsEdgeEnergized(cabo.Id.ToString()),
                $"{name}: cabo deveria estar energizado.");
        }

        private static void AssertEdgeDeenergized(
            OperationalGraphState state,
            Cabo cabo,
            string name)
        {
            Assert(
                !state.IsEdgeEnergized(cabo.Id.ToString()) &&
                state.DeenergizedEdgeIds.Contains(cabo.Id.ToString()),
                $"{name}: cabo deveria estar desenergizado.");
        }

        private static void AssertCircuitDtoParametrosReais(
            CircuitDto dto,
            Sin sin,
            Transformador transformador,
            Carga load,
            string name)
        {
            TransformerDto transformerDto = dto.Transformers.Single();
            LoadDto loadDto = dto.Loads.Single();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, $"{name}.Slack.Id");
            AssertEqual(138, dto.Slack.Tensao, $"{name}.Slack.Tensao");
            AssertEqual($"{transformador.Nome}_PRIMARIO", transformerDto.BarraPrimario, $"{name}.Transformer.BarraPrimario");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", transformerDto.BarraSecundario, $"{name}.Transformer.BarraSecundario");
            AssertEqual(138, transformerDto.TensaoPrimarioKV, $"{name}.Transformer.TensaoPrimarioKV");
            AssertEqual(34.5, transformerDto.TensaoSecundarioKV, $"{name}.Transformer.TensaoSecundarioKV");
            AssertEqual(65000, transformerDto.PotenciaKVA, $"{name}.Transformer.PotenciaKVA");
            AssertEqual(1, transformerDto.RPercentual, $"{name}.Transformer.RPercentual");
            AssertEqual(8, transformerDto.XPercentual, $"{name}.Transformer.XPercentual");
            AssertEqual("Wye", transformerDto.LigacaoPrimario, $"{name}.Transformer.LigacaoPrimario");
            AssertEqual("Wye", transformerDto.LigacaoSecundario, $"{name}.Transformer.LigacaoSecundario");
            AssertEqual(34.5, loadDto.Tensao, $"{name}.Load.Tensao");
            AssertEqual(5000, loadDto.PotenciaAtiva, $"{name}.Load.PotenciaAtiva");
            AssertEqual(1000, loadDto.PotenciaReativa, $"{name}.Load.PotenciaReativa");
            AssertEqual(sin.Nome, dto.Lines[0].Barra1, $"{name}.LinePrimario.Barra1");
            AssertEqual($"{transformador.Nome}_PRIMARIO", dto.Lines[0].Barra2, $"{name}.LinePrimario.Barra2");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", dto.Lines[1].Barra1, $"{name}.LineSecundario.Barra1");
            AssertEqual(load.Nome, dto.Lines[1].Barra2, $"{name}.LineSecundario.Barra2");
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

        private static void AssertButtonsNotFocusable(string relativePath, string name)
        {
            string xaml = File.ReadAllText(FindProjectFile(relativePath));
            IReadOnlyList<string> buttons = ExtractButtonTags(xaml);

            Assert(buttons.Count > 0, $"{name}: nenhum Button encontrado.");

            for (int i = 0; i < buttons.Count; i++)
            {
                string button = buttons[i];

                if (button.Contains("Style=\"{StaticResource RibbonToolButton}\"", StringComparison.OrdinalIgnoreCase))
                    continue;

                AssertContains(button, "Focusable=\"False\"", $"{name}.Button[{i}].Focusable");
                AssertContains(button, "IsTabStop=\"False\"", $"{name}.Button[{i}].IsTabStop");
            }
        }

        private static IReadOnlyList<string> ExtractButtonTags(string xaml)
        {
            var buttons = new List<string>();
            int index = 0;

            while (index < xaml.Length)
            {
                int start = xaml.IndexOf("<Button", index, StringComparison.OrdinalIgnoreCase);

                if (start < 0)
                    break;

                int end = xaml.IndexOf('>', start);

                if (end < 0)
                    break;

                buttons.Add(xaml[start..(end + 1)]);
                index = end + 1;
            }

            return buttons;
        }

        private static string FindProjectFile(string relativePath)
        {
            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            DirectoryInfo? directory = new(Directory.GetCurrentDirectory());

            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, normalized);

                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            throw new FileNotFoundException($"Arquivo de projeto nao encontrado: {relativePath}");
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

        private static void RunSta(Action action)
        {
            Exception? exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
                throw exception;
        }

        private static Point RotateAround(Point point, Point center, double angle)
        {
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double x = point.X - center.X;
            double y = point.Y - center.Y;

            return new Point(
                center.X + x * cos - y * sin,
                center.Y + x * sin + y * cos);
        }

        private sealed record SimpleCircuit(
            AraciDocument Document,
            Gerador Generator,
            Carga Load,
            Cabo Cable);

        private sealed record RotatedCircuit(
            EditorContext Context,
            Gerador Generator,
            Carga Load,
            Cabo Cable);

        private sealed record BarRotationCircuit(
            EditorContext Context,
            Gerador Generator,
            Barra Bar,
            Carga Load,
            Cabo Incoming,
            Cabo Outgoing);

        private sealed record BarResizeCircuit(
            EditorContext Context,
            AraciDocument Document,
            Barra Bar,
            Barra OtherBar);

        private sealed class FakeAnnotationElement : Elemento
        {
            public override Elemento Clonar()
            {
                var clone = new FakeAnnotationElement();
                CopiarBasePara(clone);
                return clone;
            }
        }
    }
}
