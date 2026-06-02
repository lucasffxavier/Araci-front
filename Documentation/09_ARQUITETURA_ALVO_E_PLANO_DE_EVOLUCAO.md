# 1. Introdução

Este documento consolida a comparação entre a arquitetura atual implementada no Araci e a arquitetura alvo definida para a evolução do projeto. O objetivo é orientar decisões técnicas futuras sem confundir capacidades existentes com capacidades planejadas.

A **Arquitetura Atual** corresponde ao que existe no código analisado: uma aplicação WPF/.NET 8 com `EditorContext` como centro de composição, `AraciDocument` como documento de trabalho, elementos elétricos estruturados, scene graph 2D, comandos com Undo/Redo, persistência `.araci`, topologia por terminais e simulação via pipeline OpenDSS/FastAPI.

A **Arquitetura Alvo** corresponde à direção já documentada para o projeto: plataforma CAD/BIM elétrica 2D evoluindo para maior modularidade, separação entre UI, ViewModels, Use Cases, Domain, Services e Infrastructure, com base em MVVM, Domain-Centric Design, Scene Graph, Command Pattern, Service Layer, Application Layer e Composition Root. Essa direção aparece em `README.md`, `Documentation/01_VISAO_GERAL_DO_PRODUTO.md`, `Documentation/04_MAPEAMENTO_DO_CODIGO_ATUAL.md`, `Documentation/13_PERSISTENCIA_ARACI.md`, `Documentation/14_SIMULACAO_OPENDSS_FASTAPI.md` e `Documentation/15_COMPOSITION_ROOT_E_DEPENDENCIAS.md`.

Quando este documento mencionar evolução futura, serão usados explicitamente os rótulos **Arquitetura Alvo** ou **Planejado**. Esses itens não devem ser interpretados como funcionalidades já implementadas.

# 2. Visão Geral da Evolução

A evolução arquitetural pode ser entendida como uma transição gradual, não como substituição abrupta do código atual. A base existente já contém conceitos centrais do produto: documento, elementos, terminais, cabos, grafo elétrico, comandos, scene graph, persistência e simulação. A evolução desejada é tornar essas fronteiras mais explícitas, menos acopladas à UI e mais preparadas para CAD/BIM, interoperabilidade e escalabilidade.

```text
+------------------------------------------------+
| ARQUITETURA ATUAL                              |
|                                                |
| WPF + EditorContext central                    |
| AraciDocument com elementos                    |
| Use cases de edição/inserção                   |
| Serviços compartilhados                        |
| Persistência JSON .araci                       |
| Simulação via FastAPI/OpenDSS                  |
+-----------------------+------------------------+
                        |
                        v
+------------------------------------------------+
| TRANSIÇÃO                                      |
|                                                |
| Separar responsabilidades                      |
| Reduzir acoplamento UI -> EditorContext        |
| Formalizar Use Cases de projeto/simulação      |
| Fortalecer contratos de domínio                |
| Evoluir persistência e topologia               |
+-----------------------+------------------------+
                        |
                        v
+------------------------------------------------+
| ARQUITETURA ALVO                               |
|                                                |
| UI / ViewModels / Use Cases                    |
| Domain central e menos dependente de WPF       |
| Service Layer com contratos claros             |
| Infrastructure configurável                    |
| Núcleo CAD/BIM extensível                      |
+------------------------------------------------+
```

Visão em camadas:

```text
Atual:

Views / Ribbon / Viewport
        |
        v
EditorContext
        |
        +--> Use Cases
        +--> Services
        +--> Domain
        +--> Infrastructure

Arquitetura Alvo:

Views
  |
ViewModels / Presentation Facades
  |
Use Cases / Application Services
  |
Domain Model / Domain Services
  |
Infrastructure Adapters
```

# 3. Arquitetura Atual

A arquitetura atual foi mapeada nos documentos anteriores:

| Documento | Síntese |
| --- | --- |
| `Documentation/04_MAPEAMENTO_DO_CODIGO_ATUAL.md` | Mapeia projetos, pastas, namespaces, domínio, aplicação, apresentação, persistência, simulação e dívidas técnicas. |
| `Documentation/05_DOMINIO_E_MODELO_DO_PROJETO.md` | Detalha `AraciDocument`, hierarquia de elementos, terminais, conexões, tipos e topologia. |
| `Documentation/08_SCENE_GRAPH_RENDERING_E_VIEWPORT.md` | Descreve cena, nodes, ViewModels, rendering, viewport, câmera e interação. |
| `Documentation/10_CONEXOES_TERMINAIS_E_TOPOLOGIA.md` | Detalha terminais, conectividade, cabos, `ElectricGraphBuilder`, `TopologyValidator` e `OperationalGraphState`. |
| `Documentation/13_PERSISTENCIA_ARACI.md` | Documenta persistência `.araci`, DTOs reais, `ProjectSerializer` e `ProjectPersistenceService`. |
| `Documentation/14_SIMULACAO_OPENDSS_FASTAPI.md` | Documenta `SimulationPipeline`, `ParameterReader`, DTOs, `CircuitBuilder`, FastAPI e OpenDSS. |
| `Documentation/15_COMPOSITION_ROOT_E_DEPENDENCIAS.md` | Documenta `EditorContext`, composition root, serviços registrados, ciclo de vida e acoplamentos. |

Resumo consolidado da arquitetura atual:

```text
App.xaml
  |
  v
MainWindow
  |
  v
EditorContext
  |
  +--> AraciDocument
  +--> Scene
  +--> ElementRegistryService
  +--> TerminalLayoutService
  +--> ConnectivityService
  +--> ElectricGraphBuilder
  +--> TopologyValidator
  +--> ProjectPersistenceService
  +--> SimulationPipeline
  +--> ToolService / InputRouter
  +--> Use Cases de edição e diagrama
```

O código atual possui separação física parcial:

- `Models`: elementos de domínio, terminais, parâmetros e tipos;
- `Core`: documento, comandos, scene, rendering, spatial e viewport;
- `Applications`: use cases, factories, editor, simulação e projetos;
- `Service`: serviços de edição, topologia, composição, geometria, seleção e viewport;
- `Infrastructure`: persistência e simulação externa;
- `DTOs`: DTOs e lógica de leitura/construção de circuito para simulação;
- `ViewModels`, `Views`, `Controls`, `Ribbon`, `Properties`: apresentação WPF.

Pontos fortes atuais:

- `AraciDocument` centraliza os elementos do projeto.
- Há elementos elétricos reais: `Barra`, `Cabo`, `Carga`, `Gerador`, `Sin` e `Transformador`.
- `Terminal`, `TerminalEndpoint` e `ITerminalOwner` permitem conectividade por terminais.
- `ElectricGraphBuilder` transforma o documento em grafo elétrico.
- `TopologyValidator` valida inconsistências topológicas.
- `OperationalGraphStateBuilder` calcula energização operacional.
- `CommandManager` e comandos concretos suportam Undo/Redo.
- `ProjectPersistenceService` e `ProjectSerializer` oferecem persistência `.araci`.
- `SimulationPipeline` integra `CircuitDtoBuilder`, `FastApiOpenDssGateway` e `SimulationResultApplier`.
- `ElementRegistryService` e `ElementDefinitionsProvider` formam um catálogo interno inicial de elementos.

Limitações atuais recorrentes:

- `EditorContext` concentra composição, estado e exposição de serviços.
- Code-behinds acessam serviços diretamente por `EditorContext`.
- Alguns serviços manipulam ViewModels diretamente.
- O domínio usa tipos WPF como `Point` e `Vector`.
- Topologia é composta no contexto e também recriada em `ParameterReader`.
- A persistência é JSON simples, sem schema formal ou migração estruturada.
- A simulação fixa o gateway concreto em `SimulationComposition`.

# 4. Arquitetura Alvo

A **Arquitetura Alvo** mantém a identidade do Araci como plataforma CAD/BIM elétrica 2D, mas organiza melhor as fronteiras entre interface, aplicação, domínio, serviços e infraestrutura.

```text
+------------------------------------------------+
| UI                                             |
| Views, Ribbon, Controls, Viewport WPF          |
+-----------------------+------------------------+
                        |
                        v
+------------------------------------------------+
| ViewModels / Presentation                      |
| Estado visual, comandos de tela, seleção       |
+-----------------------+------------------------+
                        |
                        v
+------------------------------------------------+
| Use Cases / Application Layer                  |
| Inserir, mover, rotacionar, cabear, salvar,    |
| abrir, simular, editar propriedades            |
+-----------------------+------------------------+
                        |
                        v
+------------------------------------------------+
| Domain / Domain Services                       |
| AraciDocument, elementos, terminais, tipos,    |
| conexões, topologia elétrica                   |
+-----------------------+------------------------+
                        |
                        v
+------------------------------------------------+
| Services / Infrastructure                      |
| Persistência, OpenDSS/FastAPI, import/export,  |
| catálogos externos, integrações futuras        |
+------------------------------------------------+
```

## UI

Arquitetura Atual: a UI WPF existe em `Views`, `Controls`, `Ribbon` e `Properties`. `ViewportView.xaml.cs`, `ArquivoMenuView.xaml.cs`, `DiagramaTab.xaml.cs`, `EditarTab.xaml.cs` e `AnaliseTab.xaml.cs` acessam diretamente o `EditorContext` e serviços como `InputRouter`, `ToolService`, `ProjectPersistenceService` e `SimulationPipeline`.

Arquitetura Alvo: a UI deve continuar sendo a camada de interação WPF, mas com menor conhecimento do contexto completo. A direção desejada é expor comandos e facades de apresentação mais específicos, reduzindo o acesso direto a serviços internos.

## ViewModels

Arquitetura Atual: `ViewportViewModel` encapsula `Document`, `Scene`, `SelectionBox`, `TerminalSnap`, `CableVertexEdit`, `MoveHud` e `AlignmentGuides`. Elementos possuem ViewModels como `BarraViewModel`, `CaboViewModel`, `CargaViewModel`, `GeradorViewModel`, `SinViewModel` e `TransformadorViewModel`.

Arquitetura Alvo: ViewModels devem permanecer como estado e comportamento de apresentação, sem absorver regras de domínio ou infraestrutura. A evolução desejada é manter regras elétricas, topológicas e de persistência fora da apresentação.

## Use Cases

Arquitetura Atual: existem use cases reais em `Applications/UseCases`, como `InserirElementoUseCase`, `InserirCaboUseCase`, `MoverElementoUseCase`, `RotacionarElementoUseCase`, `EditarPropriedadesUseCase`, `ExcluirElementoUseCase`, `CopiarElementosUseCase`, `ColarElementosUseCase`, `EditarVerticesCaboUseCase` e `RedimensionarBarraUseCase`.

Arquitetura Alvo: os use cases devem se tornar fronteiras mais explícitas para ações de aplicação. Operações hoje concentradas em serviços de projeto e simulação podem evoluir para use cases nomeados de projeto e análise, desde que isso seja feito sem duplicar responsabilidades.

## Domain

Arquitetura Atual: o domínio vive principalmente em `Models` e `Core/Documents`. `AraciDocument` contém a coleção de elementos. `Elemento`, `ElementoEquipamento`, `ElementoLinear`, `Barra`, `Cabo`, `Carga`, `Gerador`, `Sin` e `Transformador` carregam propriedades, parâmetros e parte do comportamento geométrico/topológico.

Arquitetura Alvo: o domínio deve continuar sendo o centro semântico do produto, mas com menor dependência de conceitos visuais e WPF quando possível. Regras de terminais, conectividade, elementos, tipos e topologia devem ser preservadas como núcleo estável.

## Services

Arquitetura Atual: serviços como `TerminalLayoutService`, `ElementRegistryService`, `ConnectivityService`, `ElectricGraphBuilder`, `TopologyValidator`, `MoveService`, `SelectionService`, `VisualUpdateService`, `SimulationResultApplier` e `ProjectPersistenceService` são criados e expostos pelo `EditorContext`.

Arquitetura Alvo: a Service Layer deve ter contratos mais claros e escopos mais específicos. Serviços de domínio, aplicação, apresentação e infraestrutura devem ser distinguíveis por responsabilidade, reduzindo dependências cruzadas com ViewModels.

## Infrastructure

Arquitetura Atual: `Infrastructure/Persistence` contém persistência de arquivo e `Infrastructure/Simulation/FastApiOpenDssGateway.cs` contém gateway externo para simulação. `SimulationComposition` instancia diretamente `FastApiOpenDssGateway`.

Arquitetura Alvo: infraestrutura deve funcionar como camada adaptadora. Persistência, gateways de simulação, importação/exportação e integrações futuras devem depender de contratos definidos pela aplicação, com configuração explícita quando necessário.

# 5. Papel do AraciDocument

## Estado atual

`Core/Documents/AraciDocument.cs` define `AraciDocument` como uma classe com `ObservableCollection<Elemento> Elementos` e métodos `AdicionarElemento`, `RemoverElemento` e `Limpar`.

Ele é usado por:

- `EditorContext`;
- `ConnectivityService`;
- `ElectricGraphBuilder`;
- `TopologyValidator`;
- `ProjectPersistenceService`;
- `ProjectSerializer`;
- `CircuitDtoBuilder`;
- `ParameterReader`;
- use cases de inserção, exclusão e colagem;
- `ViewportComposition` e `DocumentSceneSyncService`.

No estado atual, `AraciDocument` é simultaneamente:

- documento de edição;
- fonte de persistência;
- fonte de topologia;
- fonte de simulação;
- base para sincronização visual.

## Arquitetura Alvo

Na Arquitetura Alvo, `AraciDocument` deve permanecer como raiz do modelo de projeto, mas com responsabilidades mais bem delimitadas.

Responsabilidades desejadas:

- representar o estado persistível do projeto;
- conter entidades de domínio e suas relações;
- servir de entrada para topologia, persistência e simulação;
- não depender diretamente de detalhes de UI;
- suportar evolução para múltiplos diagramas quando esse recurso for planejado e implementado;
- preservar identidade e rastreabilidade de elementos.

Planejado: caso o projeto evolua para múltiplos diagramas, catálogo externo, IFC ou 3D, `AraciDocument` provavelmente precisará deixar de ser apenas uma coleção plana de elementos e passar a representar um projeto com unidades internas mais explícitas. Essa mudança não existe no código atual.

# 6. Evolução dos Use Cases

## Use cases existentes

| Use case real | Arquivo | Estado atual | Evolução desejada |
| --- | --- | --- | --- |
| `InserirElementoUseCase` | `Applications/UseCases/Diagrama/InserirElementoUseCase.cs` | Cria modelo por `ElementoFactory`, posiciona, atualiza terminais e executa `AddElementoCommand`. | Arquitetura Alvo: manter como fronteira de inserção, reduzindo dependência direta de detalhes visuais quando possível. |
| `InserirCaboUseCase` | `Applications/UseCases/Diagrama/InserirCaboUseCase.cs` | Cria `CaboViewModel`, adiciona modelo ao documento e grava origem/destino por terminal. | Arquitetura Alvo: separar regra de conexão do ViewModel; manter UI como consumidora, não como parte do caso de uso. |
| `MoverElementoUseCase` | `Applications/UseCases/Editar/MoverElementoUseCase.cs` | Compara estados e cria `MoveElementoCommand` em transação. | Arquitetura Alvo: continuar baseado em comandos, com entradas independentes de ViewModel. |
| `RotacionarElementoUseCase` | `Applications/UseCases/Editar/RotacionarElementoUseCase.cs` | Compara estados e executa `RotateElementoCommand`. | Arquitetura Alvo: manter rotação como caso de uso de aplicação, preservando callback visual fora do domínio. |
| `EditarPropriedadesUseCase` | `Applications/UseCases/Editar/EditarPropriedadesUseCase.cs` | Usa reflexão em `ElementoViewModel` e executa `BulkPropertyChangeCommand`. | Arquitetura Alvo: migrar progressivamente para descritores de propriedades mais explícitos e menos dependentes de ViewModel. |
| `ExcluirElementoUseCase` | `Applications/UseCases/Editar/ExcluirElementoUseCase.cs` | Remove elementos com apoio de `ConnectivityService` e comandos. | Arquitetura Alvo: manter validações de conectividade e exclusão em camada de aplicação/domínio. |
| `CopiarElementosUseCase` | `Applications/UseCases/Editar/CopiarElementosUseCase.cs` | Use case de cópia. | Arquitetura Alvo: preservar identidade de cópia e regras de duplicação em aplicação. |
| `ColarElementosUseCase` | `Applications/UseCases/Editar/ColarElementosUseCase.cs` | Cola elementos no documento usando nomes, comandos e destino. | Arquitetura Alvo: suportar regras futuras de múltiplos diagramas e blocos quando planejadas. |
| `EditarVerticesCaboUseCase` | `Applications/UseCases/Editar/EditarVerticesCaboUseCase.cs` | Edita geometria de cabo por comando. | Arquitetura Alvo: manter geometria de edição separada da topologia elétrica. |
| `RedimensionarBarraUseCase` | `Applications/UseCases/Editar/RedimensionarBarraUseCase.cs` | Redimensiona barra com comando e atualização geométrica. | Arquitetura Alvo: preservar regra de altura/terminais em serviço de domínio ou aplicação. |

## Nomes solicitados como Arquitetura Alvo ou Planejado

Alguns nomes mencionados na estrutura obrigatória não existem como classes reais no código atual:

| Nome | Estado atual | Tratamento neste documento |
| --- | --- | --- |
| `CriarCaboUseCase` | Não encontrado. Existe `InserirCaboUseCase`. | Arquitetura Alvo: pode representar uma separação futura entre criação de cabo e inserção/interação. |
| `SalvarProjetoUseCase` | Não encontrado. A responsabilidade está em `ProjectPersistenceService.Salvar(...)` e `SalvarComDialogo()`. | Planejado: se criado, deve encapsular salvamento sem misturar diálogo, serialização e escrita. |
| `AbrirProjetoUseCase` | Não encontrado. A responsabilidade está em `ProjectPersistenceService.Abrir(...)` e `AbrirComDialogo()`. | Planejado: se criado, deve encapsular abertura e reconstrução de projeto. |
| `ExecutarSimulacaoUseCase` | Não encontrado com esse nome. O fluxo atual usa `SimulationPipeline.ExecutarFluxoDeCorrenteAsync()` e `FluxoDeCorrenteApplication`. | Planejado: se criado, deve orquestrar validação, DTOs, gateway, aplicação de resultados e mensagens. |

Arquitetura Alvo para use cases:

```text
View / ViewModel
      |
      v
Use Case
      |
      +--> Domain
      +--> Services
      +--> Commands
      +--> Infrastructure Port
```

# 7. Evolução dos Serviços

## TerminalLayoutService

Estado atual: `TerminalLayoutService` atualiza terminais usando `ElementRegistryService` e `ElementGeometryService`. É usado por inserção, factories, persistência, visual updates e geometria.

Arquitetura Alvo: manter como serviço especializado em layout de terminais, mas separar claramente regras elétricas de regras visuais. Para CAD/BIM, terminais precisam ser estáveis como identidade topológica, mesmo quando sua apresentação muda.

## ElementRegistryService

Estado atual: `ElementRegistryService` implementa `IElementCatalog`, registra `ElementDefinition`, resolve kind, atalhos, tipos, tamanhos, atualização de terminais e propriedades de instância.

Arquitetura Alvo: evoluir de catálogo interno inicial para base extensível de elementos e tipos. Planejado: o catálogo futuro citado em `README.md` e `Documentation/01_VISAO_GERAL_DO_PRODUTO.md` pode se apoiar nesse serviço, mas ainda não existe como catálogo completo de fabricantes ou biblioteca externa.

## ElectricGraphBuilder

Estado atual: transforma `AraciDocument` em `ElectricGraph`, criando nós para elementos topológicos que implementam `ITerminalOwner` e arestas para `Cabo`. Valida endpoints inexistentes, mesmo elemento, mesmo terminal e cabo duplicado.

Arquitetura Alvo: fortalecer como serviço de domínio/topologia, com regras elétricas mais explícitas e contratos estáveis para simulação, validação, energização e futuras análises.

## ParameterReader

Estado atual: `DTOs/ParameterReader.cs` lê elementos de `AraciDocument`, cria/usa `ConnectivityService`, `TopologyValidator` e `ElectricGraphBuilder`, resolve barras e produz dados intermediários para cargas, linhas, transformadores, geradores e SIN.

Arquitetura Alvo: separar melhor leitura de parâmetros, resolução topológica e montagem de DTOs. A dívida observada em `Documentation/04_MAPEAMENTO_DO_CODIGO_ATUAL.md` é que `DTOs/ParameterReader.cs` contém lógica significativa, não apenas DTO.

## CircuitBuilder

Estado atual: `DTOs/CircuitBuilder.cs` monta `CircuitDto`, escolhe slack preferindo SIN quando existe, constrói cargas, linhas, transformadores e geradores, aplica defaults e valida barras de linhas.

Arquitetura Alvo: manter a construção de circuito como etapa de aplicação/simulação, com regras elétricas e defaults documentados, testáveis e menos misturados à pasta `DTOs`.

## SimulationPipeline

Estado atual: `Applications/Simulation/SimulationPipeline.cs` implementa `ISimulationPipeline`, chama `CircuitDtoBuilder.Build()`, envia para `ISimulationGateway.SimularAsync(dto)` e aplica resultado via `ISimulationResultApplier`.

Arquitetura Alvo: consolidar como orquestrador da simulação. Planejado: permitir configuração mais clara de gateway e ambiente de simulação, mantendo OpenDSS/FastAPI como adaptador de infraestrutura.

## ProjectPersistenceService

Estado atual: `Applications/Projects/ProjectPersistenceService.cs` orquestra novo, salvar, abrir, diálogos, serialização, repositório e limpeza de estado transitório.

Arquitetura Alvo: separar melhor use cases de projeto, diálogo de arquivo, serialização e escrita. Planejado: possíveis `SalvarProjetoUseCase` e `AbrirProjetoUseCase`, caso a equipe decida formalizar essas operações como Application Layer.

## Demais serviços relevantes

| Serviço | Estado atual | Evolução desejada |
| --- | --- | --- |
| `ConnectivityService` | Resolve elementos, cabos e endpoints no documento. | Arquitetura Alvo: consolidar como serviço topológico de domínio ou aplicação. |
| `TopologyValidator` | Valida topologia a partir de documento, conectividade e grafo. | Arquitetura Alvo: ampliar validações elétricas sem acoplar UI. |
| `OperationalGraphStateBuilder` | Calcula energização operacional por grafo. | Arquitetura Alvo: base para estados elétricos mais ricos. |
| `DocumentSceneSyncService` | Sincroniza documento, cena e ViewModels. | Arquitetura Alvo: manter como ponte apresentação-cena, evitando invadir domínio. |
| `VisualUpdateService` | Atualiza visualmente elementos após mudanças. | Arquitetura Alvo: permanecer na camada de apresentação/serviços visuais. |
| `ToolService` e `InputRouter` | Gerenciam ferramentas e eventos de input. | Arquitetura Alvo: reduzir dependência direta da UI ao contexto completo. |

# 8. Evolução da Simulação

## Estado atual

O fluxo atual de simulação é:

```text
AnaliseTab
   |
   v
FluxoDeCorrenteApplication
   |
   v
SimulationPipeline
   |
   +--> CircuitDtoBuilder
   |       +--> ParameterReader
   |       +--> CircuitBuilder
   |
   +--> FastApiOpenDssGateway
   |
   +--> SimulationResultApplier
```

Classes reais:

- `SimulationPipeline`;
- `CircuitDtoBuilder`;
- `ParameterReader`;
- `CircuitBuilder`;
- `FastApiOpenDssGateway`;
- `SimulationApiClient`;
- `SimulationResultApplier`;
- `SimulationExportService`;
- `SimulationMessageBuilder`;
- DTOs como `CircuitDto`, `LoadDto`, `LineDto`, `GeneratorDto`, `TransformerDto`, `SlackDto` e `SimulationResultDto`.

## Arquitetura Alvo

A simulação deve ser organizada como pipeline de aplicação com contratos claros:

```text
ExecutarSimulacaoUseCase (Planejado)
        |
        v
Validação Topológica
        |
        v
Leitura de Parâmetros
        |
        v
DTO de Circuito
        |
        v
Gateway de Simulação
        |
        v
Aplicação de Resultados
        |
        v
Notificação Visual
```

Planejado: `ExecutarSimulacaoUseCase` não existe hoje como classe. O papel equivalente é dividido entre `FluxoDeCorrenteApplication` e `SimulationPipeline`. A evolução desejada é deixar a orquestração de simulação mais explícita, configurável e testável.

# 9. Evolução da Persistência

## Estado atual

A persistência atual é:

```text
AraciDocument
    |
    v
ProjectSerializer
    |
    v
ProjectFileDto / ElementDto / ParameterDto / TerminalDto / PointDto
    |
    v
JSON
    |
    v
Arquivo .araci
```

Classes reais:

- `ProjectPersistenceService`;
- `ProjectSerializer`;
- `ProjectFileDto`;
- `ProjectMetadataDto`;
- `ElementDto`;
- `TypeRefDto`;
- `ParameterDto`;
- `TerminalDto`;
- `PointDto`;
- `FileSystemProjectRepository`;
- `ProjectFileDialogService`.

O documento `Documentation/13_PERSISTENCIA_ARACI.md` registra que não existem `ProjectDto`, `BarraDto` ou `CaboDto` específicos; o DTO raiz real é `ProjectFileDto` e elementos são persistidos por `ElementDto`.

## Arquitetura Alvo

Arquitetura Alvo para persistência:

- contrato de arquivo mais explícito;
- versionamento com migrações quando necessário;
- validação estrutural;
- escrita mais robusta;
- separação entre diálogo, use case, serializer e repositório;
- preparação para múltiplos diagramas e metadados CAD/BIM quando planejados.

Planejado: `SalvarProjetoUseCase` e `AbrirProjetoUseCase` podem surgir como fronteiras de aplicação. Hoje, essas responsabilidades estão em `ProjectPersistenceService`.

# 10. Evolução da Topologia

## Estado atual

A topologia atual é baseada em:

- `Terminal`;
- `TerminalEndpoint`;
- `ITerminalOwner`;
- `Cabo` como aresta;
- elementos topológicos como nós;
- `ConnectivityService`;
- `ElectricGraphBuilder`;
- `TopologyValidator`;
- `OperationalGraphStateBuilder`.

Fluxo:

```text
AraciDocument
    |
    v
ConnectivityService
    |
    v
ElectricGraphBuilder
    |
    v
ElectricGraph
    |
    v
TopologyValidator
    |
    v
OperationalGraphStateBuilder
```

O documento `Documentation/10_CONEXOES_TERMINAIS_E_TOPOLOGIA.md` mostra que a base atual já contém conceitos essenciais para uma arquitetura elétrica CAD/BIM.

## Arquitetura Alvo

A topologia deve evoluir para ser um núcleo elétrico ainda mais explícito:

- regras por tipo elétrico;
- validações por terminal, fase, nível de tensão e compatibilidade quando essas regras forem implementadas;
- maior centralização de políticas topológicas;
- melhor separação entre geometria visual e conectividade elétrica;
- suporte futuro a múltiplos diagramas sem perda de identidade topológica.

Planejado: validações elétricas avançadas não devem ser descritas como existentes se não estiverem implementadas. Elas são direção de evolução compatível com a plataforma CAD/BIM elétrica.

# 11. Evolução para CAD/BIM

Esta seção documenta itens planejados já citados em `README.md` e `Documentation/01_VISAO_GERAL_DO_PRODUTO.md`. Eles não são funcionalidades completas no código atual.

## Catálogo

Estado atual: existe um catálogo interno inicial composto por `ElementDefinitionsProvider`, `ElementRegistryService` e `TypeLibraryService`.

Planejado: evoluir para catálogo mais amplo de componentes, tipos, propriedades técnicas e possivelmente bibliotecas de fabricantes. A arquitetura deve preservar `ElementRegistryService` como ponto de extensão ou evoluí-lo para contrato mais amplo.

## Anotações

Estado atual: `DomainRole` contém papel de anotação no domínio documentado anteriormente, mas não há um sistema completo de anotações documentais implementado.

Planejado: anotações devem entrar como entidades documentais sem contaminar a topologia elétrica. A separação por `DomainRole` é uma base conceitual relevante.

## Blocos

Estado atual: não foi identificado sistema completo de blocos reutilizáveis.

Planejado: blocos devem funcionar como composição reutilizável de elementos gráficos/técnicos. Arquiteturalmente, isso exigirá regras de identidade, cópia, persistência e eventual parametrização.

## Múltiplos Diagramas

Estado atual: `AraciDocument` contém uma coleção única `Elementos`.

Planejado: múltiplos diagramas exigem que o projeto represente mais de uma folha ou visão. Isso impacta `AraciDocument`, persistência, scene graph, seleção, comandos e navegação.

## DXF

Estado atual: não foi identificado importador/exportador DXF completo no código analisado.

Planejado: DXF pertence à interoperabilidade CAD. Deve ser tratado como infraestrutura/adaptador, sem reduzir o modelo interno a geometria semântica pobre.

## DWG

Estado atual: não foi identificado suporte DWG completo no código analisado.

Planejado: DWG pertence à compatibilidade com fluxos CAD consolidados. Sua implementação deverá respeitar contratos de importação/exportação e limites de licenciamento/tecnologia quando for planejada.

## IFC

Estado atual: não foi identificado suporte IFC completo no código analisado.

Planejado: IFC pertence à interoperabilidade BIM. A arquitetura deve evitar tratar IFC como simples exportação gráfica; ele envolve semântica, entidades e relações.

## 3D

Estado atual: a engine visual atual é 2D, com `ViewportView`, `Camera`, scene graph e controles WPF.

Planejado: ambiente 3D é visão de longo prazo. Conforme `Documentation/01_VISAO_GERAL_DO_PRODUTO.md`, 3D exige mudanças em geometria, navegação, representação espacial, persistência e interoperabilidade.

# 12. Extensibilidade

O mecanismo atual para adicionar novos elementos passa por `ElementDefinitionsProvider` e `ElementRegistryService`.

Fluxo atual de extensão:

```text
Novo modelo Elemento
      |
      v
Novo TipoElemento
      |
      v
Novo ViewModel
      |
      v
ElementDefinition
      |
      v
ElementRegistryService
      |
      +--> Ribbon
      +--> Factories
      +--> Persistência
      +--> Geometria
      +--> Terminais
```

Classes reais envolvidas:

- `ElementDefinition`;
- `ElementDefinitionsProvider`;
- `ElementRegistryService`;
- `ElementoModelFactory`;
- `ElementoViewModelFactory`;
- `ElementoFactory`;
- `TypeLibraryService`;
- `TerminalLayoutService`;
- `ElementGeometryService`.

Arquitetura Alvo:

- novos elementos devem ser registrados por contratos claros;
- criação de modelo e criação de ViewModel devem permanecer separadas;
- tipo, geometria, terminais, propriedades e metadados de ribbon devem ser definidos de forma coesa;
- persistência deve conseguir reconstruir elementos por `Kind`;
- topologia deve distinguir elementos elétricos, gráficos e anotativos;
- simulação deve consumir apenas elementos que participam do grafo elétrico.

# 13. Roadmap Arquitetural

O roadmap abaixo é baseado apenas em decisões e direções já presentes em `README.md`, `Documentation/01_VISAO_GERAL_DO_PRODUTO.md` e nos documentos técnicos existentes.

## Curto prazo

- Consolidar `EditorContext` como composição, reduzindo gradualmente responsabilidades não relacionadas à sessão.
- Fortalecer use cases existentes como fronteiras de aplicação.
- Evoluir persistência `.araci` com contrato mais explícito e validações.
- Melhorar separação entre ViewModels e regras de domínio.
- Consolidar topologia por terminais e checks técnicos.
- Organizar a simulação para reduzir concentração de lógica em `DTOs`.
- Avançar catálogo interno de elementos e propriedades.

## Médio prazo

- Planejado: sistema de anotações.
- Planejado: blocos reutilizáveis.
- Planejado: importação/exportação DXF.
- Planejado: IFC.
- Planejado: bibliotecas ou catálogos mais ricos.
- Evoluir composição para reduzir acesso direto da UI ao `EditorContext`.
- Separar melhor serviços de apresentação, aplicação, domínio e infraestrutura.

## Longo prazo

- Planejado: múltiplos diagramas.
- Planejado: ambiente BIM elétrico.
- Planejado: GIS.
- Planejado: integrações corporativas/ERP.
- Planejado: ambiente 3D.
- Evoluir `AraciDocument` para representar projetos maiores, múltiplas visões e relações semânticas mais amplas.

# 14. Riscos Arquiteturais

Riscos reais observados no código e nos documentos técnicos:

| Risco | Evidência |
| --- | --- |
| Concentração no `EditorContext` | `Service/EditorContext.cs` cria e expõe grande parte dos serviços, use cases e estados. |
| UI acoplada ao contexto completo | `ViewportView`, `ArquivoMenuView`, `DiagramaTab`, `EditarTab` e `AnaliseTab` acessam `EditorContext` diretamente. |
| Domínio acoplado a WPF | Modelos usam `System.Windows.Point` e `Vector`, conforme registrado em `Documentation/04_MAPEAMENTO_DO_CODIGO_ATUAL.md`. |
| Serviços com dependência de ViewModels | Serviços como seleção, movimento e edição interagem com `ElementoViewModel`. |
| Topologia composta em múltiplos pontos | `EditorContext` cria `ElectricGraphBuilder`/`TopologyValidator`, e `ParameterReader(AraciDocument)` cria instâncias próprias. |
| Persistência sem schema formal | `Documentation/13_PERSISTENCIA_ARACI.md` registra JSON simples, sem schema e sem migrações estruturadas. |
| Simulação com gateway fixo na composição | `SimulationComposition` instancia `FastApiOpenDssGateway` diretamente. |
| DTOs com lógica significativa | `ParameterReader` e `CircuitBuilder` contêm leitura, resolução, defaults e validação. |
| Evolução CAD/BIM pode pressionar o modelo atual | Múltiplos diagramas, IFC e 3D exigem mudanças além da UI. |

# 15. Conclusão

A arquitetura atual do Araci já implementa uma base funcional relevante para uma plataforma CAD/BIM elétrica 2D. O código possui documento central, elementos elétricos estruturados, terminais, conectividade, grafo elétrico, scene graph, comandos, persistência e simulação. Esses elementos demonstram que a direção arquitetural não é apenas conceitual; ela já está parcialmente materializada.

A Arquitetura Alvo não exige descartar essa base. Ela propõe tornar as fronteiras mais claras: UI e ViewModels focados em apresentação, Use Cases como entrada da camada de aplicação, domínio como núcleo semântico, serviços com responsabilidades delimitadas e infraestrutura como adaptador para arquivo, simulação e integrações.

Essa evolução é especialmente importante porque a visão do Araci vai além de um editor gráfico. Catálogo, anotações, blocos, DXF, DWG, IFC, múltiplos diagramas, GIS, ERP e 3D só podem ser incorporados de forma sustentável se reforçarem o núcleo do produto: modelagem elétrica estruturada, persistível, editável, validável e preparada para simulação.

Portanto, a arquitetura alvo suporta a evolução para uma plataforma CAD/BIM elétrica completa porque preserva o centro de domínio já existente e organiza a expansão em torno de contratos, use cases, serviços e adaptadores. O caminho recomendado é incremental: consolidar o que já existe, reduzir acoplamentos reais e introduzir novas capacidades planejadas sem quebrar a coerência do modelo elétrico.

