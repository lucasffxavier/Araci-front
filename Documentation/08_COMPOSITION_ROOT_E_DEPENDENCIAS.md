# 1. Introdução

Este documento descreve a composição de objetos e o gerenciamento de dependências do Araci conforme observado no código atual. O foco é mapear como a aplicação WPF inicializa, cria o contexto principal do editor, instancia serviços, conecta ViewModels, expõe comandos de UI e integra persistência, topologia, simulação e viewport.

A composição atual não usa um container de injeção de dependência externo. O código monta explicitamente objetos por construtores, concentrando a maior parte da criação em `Service/EditorContext.cs` e em classes auxiliares de composição localizadas em `Service/Composition`. Essa abordagem torna a cadeia de dependências visível no código, embora também concentre muitas responsabilidades de montagem em um ponto central.

A pasta `Service` está organizada em grupos por responsabilidade (`Catalog`, `Composition`, `Editing`, `Geometry`, `Interaction`, `Naming`, `Settings`, `Simulation`, `Topology`, `UI` e `Viewport`). A raiz de `Service` mantém apenas `EditorContext.cs` e `EditorState.cs`.

Os principais arquivos analisados para este documento são:

| Arquivo | Papel na composição |
| --- | --- |
| `App.xaml` | Define `StartupUri="MainWindow.xaml"` e recursos globais. |
| `App.xaml.cs` | Ajusta cultura WPF em `OnStartup`. |
| `MainWindow.xaml.cs` | Cria `EditorContext`, inicializa viewport e ribbon. |
| `Service/EditorContext.cs` | Centro principal de composição e estado compartilhado do editor. |
| `Service/Composition/EditorCoreComposition.cs` | Cria núcleo de cena, catálogo, topologia, geometria e serviços básicos. |
| `Service/Composition/EditingComposition.cs` | Cria serviços de edição, seleção, movimentação, clipboard e input. |
| `Service/Composition/ViewportComposition.cs` | Cria `ViewportViewModel`, sincronização documento-cena e navegação. |
| `Service/Composition/PersistenceComposition.cs` | Cria serviços de persistência de projeto. |
| `Service/Composition/SimulationComposition.cs` | Cria pipeline e auxiliares de simulação. |
| `Views/ViewportView.xaml.cs` | Inicializa ViewModel do viewport e conecta eventos de input/câmera. |
| `Ribbon/Tabs/*.xaml.cs` | Consome serviços expostos por `EditorContext`. |

# 2. Visão Geral

O fluxo geral de inicialização e conexão de dependências é:

```text
+----------------------+
| App.xaml             |
| StartupUri           |
+----------+-----------+
           |
           v
+----------------------+
| MainWindow           |
| new EditorContext()  |
+----------+-----------+
           |
           v
+----------------------+
| Composition          |
| EditorContext ctor   |
+----------+-----------+
           |
           v
+----------------------+
| Contexto             |
| EditorContext        |
+----------+-----------+
           |
           v
+----------------------+
| Serviços             |
| factories/use cases  |
+----------+-----------+
           |
           v
+----------------------+
| ViewModels           |
| ViewportViewModel    |
+----------+-----------+
           |
           v
+----------------------+
| UI                   |
| Ribbon / Viewport    |
+----------------------+
```

O `MainWindow` é o primeiro ponto de criação explícita do grafo de objetos. Seu construtor executa:

1. `InitializeComponent()`;
2. `_context = new EditorContext()`;
3. `Viewport.Inicializar(_context)`;
4. `InicializarRibbon()`.

`InicializarRibbon()` atribui `DataContext = _context` para a janela e `ArquivoMenu.DataContext = _context`. Outros componentes de UI recebem o mesmo contexto por herança de `DataContext` ou por busca na janela.

O `EditorContext` é o objeto compartilhado que agrega:

- documento (`AraciDocument`);
- cena (`Scene`);
- consultas espaciais;
- serviços de seleção, hover, snap, movimentação, rotação e exclusão;
- factories de elementos;
- use cases;
- persistência;
- simulação;
- topologia;
- viewport;
- histórico de comandos;
- estados visuais transitórios.

Fluxo resumido de dependências:

```text
MainWindow
    |
    v
EditorContext
    |
    +--> EditorCoreComposition
    |       +--> Scene
    |       +--> ElementRegistryService
    |       +--> ConnectivityService
    |       +--> ElectricGraphBuilder
    |       +--> TopologyValidator
    |
    +--> SimulationComposition
    |       +--> SimulationPipeline
    |
    +--> EditingComposition
    |       +--> SelectionService
    |       +--> MoveService
    |       +--> InputRouter
    |
    +--> PersistenceComposition
    |       +--> ProjectPersistenceService
    |
    +--> ViewportComposition
            +--> ViewportViewModel
            +--> DocumentSceneSyncService
```

# 3. Composition Root

O composition root real está distribuído entre `MainWindow`, `EditorContext` e as classes estáticas de `Service/Composition`. O código não contém registro em container DI; a criação é manual.

## MainWindow

`MainWindow.xaml.cs` é o ponto inicial de composição em runtime da janela principal. A classe `MainWindow` cria uma instância privada de `EditorContext`:

```text
MainWindow()
    InitializeComponent()
    new EditorContext()
    Viewport.Inicializar(_context)
    InicializarRibbon()
```

Responsabilidades observadas:

- criar o contexto do editor;
- inicializar o viewport com esse contexto;
- atribuir o contexto como `DataContext`;
- abrir/fechar popup de arquivo;
- focar o viewport;
- mostrar ou ocultar painel de propriedades.

## EditorCoreComposition

`Service/Composition/EditorCoreComposition.cs` contém `EditorCoreComposition.Create(...)`. Esse método cria o núcleo do editor a partir de `AraciDocument`, `EditorSettings` e `TypeLibraryService`.

Dependências de entrada:

| Parâmetro | Uso observado |
| --- | --- |
| `AraciDocument document` | Base para conectividade, grafo elétrico e validador topológico. |
| `EditorSettings settings` | Usado por `SnapService`. |
| `TypeLibraryService types` | Usado por `ElementDefinitionsProvider`. |

Objetos criados:

| Objeto | Classe | Responsabilidade observada |
| --- | --- | --- |
| `scene` | `Araci.Core.Scenes.Scene` | Cena visual com ViewModels. |
| `sceneQueries` | `SceneQueryService` | Consultas sobre a cena. |
| `hover` | `HoverService` | Estado de hover usando consultas da cena. |
| `snap` | `SnapService` | Snap baseado em consultas da cena e configurações. |
| `typePropertiesDialogs` | `TypePropertiesDialogService` | Diálogos de propriedades de tipos. |
| `dialogs` | `DialogService` | Mensagens ao usuário. |
| `instanceProperties` | `ElementInstancePropertyProvider` | Catálogo de propriedades de instância por elemento. |
| `definitions` | `ElementDefinitionsProvider` | Definições de elementos disponíveis. |
| `elements` | `ElementRegistryService` | Registro/catálogo de elementos. |
| `connectivity` | `ConnectivityService` | Busca e validação de conectividade no documento. |
| `electricGraph` | `ElectricGraphBuilder` | Construção de grafo elétrico a partir do documento. |
| `operationalState` | `OperationalGraphStateBuilder` | Construção de estado operacional a partir de grafo. |
| `topology` | `TopologyValidator` | Validação topológica do documento. |
| `geometry` | `ElementGeometryService` | Tamanho/geometria por elemento registrado. |
| `terminalLayout` | `TerminalLayoutService` | Atualização de terminais baseada em elementos e geometria. |

O método também executa `InstancePropertyCatalog.Configure(elements)`, conectando o catálogo de propriedades ao registro de elementos.

O retorno é um record interno `EditorCoreComponents`, que agrupa os objetos criados para atribuição posterior no `EditorContext`.

## PersistenceComposition

`Service/Composition/PersistenceComposition.cs` contém `PersistenceComposition.CreateProjects(...)`. Esse método cria o serviço de persistência de projeto.

Dependências de entrada:

| Parâmetro | Uso observado |
| --- | --- |
| `AraciDocument document` | Documento persistido e reconstruído. |
| `CommandManager commands` | Histórico limpo em novo projeto e abertura. |
| `ElementRegistryService elements` | Usado pelo serializer para resolver tipos/kinds. |
| `IElementModelFactory modelFactory` | Usado pelo serializer para recriar modelos. |
| `TerminalLayoutService terminalLayout` | Usado para recompor terminais após abertura. |
| `ElementGeometryService geometry` | Usado para restaurar posições visuais de terminais. |
| `DialogService dialogs` | Exibição de erros e avisos. |
| `Action clearTransientState` | Limpeza de estado transitório após novo/abrir. |

Objetos criados:

- `ProjectSerializer`;
- `FileSystemProjectRepository`;
- `ProjectFileDialogService`;
- `ProjectPersistenceService`.

O retorno é `ProjectPersistenceService`, armazenado em `EditorContext.Projects`.

## SimulationComposition

`Service/Composition/SimulationComposition.cs` contém `SimulationComposition.Create(...)`. Esse método cria os componentes usados pela simulação.

Dependências de entrada:

| Parâmetro | Uso observado |
| --- | --- |
| `AraciDocument document` | Fonte para criação de DTO de circuito e aplicação de resultados. |
| `Action notifySimulationResultViewModels` | Callback para atualizar ViewModels após aplicar resultados. |

Objetos criados:

| Objeto | Classe |
| --- | --- |
| `results` | `SimulationResultApplier` |
| `gateway` | `FastApiOpenDssGateway` |
| `circuitDtoBuilder` | `CircuitDtoBuilder` |
| `pipeline` | `SimulationPipeline` |
| `export` | `SimulationExportService` |
| `messages` | `SimulationMessageBuilder` |

O retorno é o record `SimulationComponents`, contendo `Results`, `Pipeline`, `Export` e `Messages`. O gateway e o builder são dependências internas do pipeline e não ficam expostos como propriedades do `EditorContext`.

## EditingComposition

`Service/Composition/EditingComposition.cs` cria grupos de serviços de edição. Os métodos reais são:

| Método | Retorno | Responsabilidade de composição |
| --- | --- | --- |
| `CreateVisualUpdates(...)` | `VisualUpdateService` | Atualização visual com viewport, terminais, conectividade, consultas e snap. |
| `CreateSelection(...)` | `SelectionService` | Seleção com estado do editor, eventos e edição de propriedades. |
| `CreateCableVertexEdit(...)` | `CableVertexEditService` | Edição de vértices de cabo. |
| `CreateSafeDelete(...)` | `SafeDeleteService` | Exclusão segura de seleção ou handle ativo. |
| `CreateClipboard(...)` | `ClipboardService` | Copiar/colar usando use cases e viewport. |
| `CreateMoveServices(...)` | `MoveServices` | Agrupa serviços e use cases de movimento, rotação, resize e alinhamento. |
| `CreateInput(...)` | `InputRouter` | Roteamento de mouse/teclado para ferramentas, comandos, seleção e clipboard. |

`CreateMoveServices(...)` cria um record `MoveServices` com:

- `MoveHudService`;
- `AlignmentGuideService`;
- `MoveConstraintService`;
- `MoverElementoUseCase`;
- `RotacionarElementoUseCase`;
- `RedimensionarBarraUseCase`;
- `MoveService`;
- `BarraResizeService`;
- `RotationService`.

## ViewportComposition

`Service/Composition/ViewportComposition.cs` tem dois métodos:

| Método | Retorno | Responsabilidade |
| --- | --- | --- |
| `CreateViewModel(...)` | `ViewportViewModel` | Cria `DocumentSceneSyncService` e `ViewportViewModel`. |
| `CreateNavigation(...)` | `ViewportNavigationService` | Cria navegação baseada em provider de `ViewportService`. |

`CreateViewModel(...)` recebe o documento, a cena, estados de seleção/snap, serviços de edição, factories e serviços de consulta. Internamente cria `DocumentSceneSyncService`, que sincroniza `AraciDocument` com `Scene` e ViewModels.

# 4. EditorContext

`Service/EditorContext.cs` define `EditorContext`, namespace `Araci.Services`. A classe implementa `IEditorSession`, interface que expõe `AraciDocument Document`, `Scene`, `ISceneQueryService SceneQueries` e `ICommandHistory Commands`.

O `EditorContext` tem dois construtores:

- `EditorContext()`, que chama `this(new EventBus())`;
- `EditorContext(IEventBus eventBus)`, que executa toda a composição.

## Estrutura

O `EditorContext` contém propriedades inicializadas diretamente, propriedades atribuídas a partir das composições e propriedades criadas manualmente no construtor.

Todas as propriedades públicas observadas são:

| Propriedade | Tipo | Origem |
| --- | --- | --- |
| `Events` | `IEventBus` | Parâmetro do construtor ou `new EventBus()`. |
| `Document` | `AraciDocument` | Inicializador de propriedade. |
| `Scene` | `CoreScene` | `EditorCoreComposition`. |
| `SceneQueries` | `ISceneQueryService` | `EditorCoreComposition`. |
| `Hover` | `HoverService` | `EditorCoreComposition`. |
| `Tools` | `ToolService` | Criado no construtor. |
| `Input` | `InputRouter` | `EditingComposition.CreateInput`. |
| `Navigation` | `ViewportNavigationService` | `ViewportComposition.CreateNavigation`. |
| `Viewport` | `ViewportService?` | Criado por `InicializarViewport`. |
| `Editor` | `EditorState` | Inicializador de propriedade. |
| `Settings` | `EditorSettings` | Inicializador de propriedade. |
| `MoveHud` | `MoveHudService` | `EditingComposition.CreateMoveServices`. |
| `AlignmentGuides` | `AlignmentGuideService` | `EditingComposition.CreateMoveServices`. |
| `MoveConstraints` | `MoveConstraintService` | `EditingComposition.CreateMoveServices`. |
| `SelectionBox` | `SelectionBoxViewModel` | Inicializador de propriedade. |
| `TerminalSnap` | `TerminalSnapState` | Inicializador de propriedade. |
| `CableVertexEdit` | `CableVertexEditService` | `EditingComposition.CreateCableVertexEdit`. |
| `Commands` | `CommandManager` | Inicializador de propriedade. |
| `SafeDelete` | `SafeDeleteService` | `EditingComposition.CreateSafeDelete`. |
| `Clipboard` | `ClipboardService` | `EditingComposition.CreateClipboard`. |
| `Projects` | `ProjectPersistenceService` | `PersistenceComposition.CreateProjects`. |
| `VisualUpdates` | `VisualUpdateService` | `EditingComposition.CreateVisualUpdates`. |
| `Selection` | `SelectionService` | `EditingComposition.CreateSelection`. |
| `Move` | `MoveService` | `EditingComposition.CreateMoveServices`. |
| `BarraResize` | `BarraResizeService` | `EditingComposition.CreateMoveServices`. |
| `Rotation` | `RotationService` | `EditingComposition.CreateMoveServices`. |
| `Snap` | `SnapService` | `EditorCoreComposition`. |
| `Names` | `NameService` | Criado no construtor. |
| `Connectivity` | `ConnectivityService` | `EditorCoreComposition`. |
| `ElectricGraph` | `ElectricGraphBuilder` | `EditorCoreComposition`. |
| `OperationalState` | `OperationalGraphStateBuilder` | `EditorCoreComposition`. |
| `Topology` | `TopologyValidator` | `EditorCoreComposition`. |
| `SimulationResults` | `SimulationResultApplier` | `SimulationComposition`. |
| `Simulation` | `SimulationPipeline` | `SimulationComposition`. |
| `SimulationExport` | `SimulationExportService` | `SimulationComposition`. |
| `SimulationMessages` | `SimulationMessageBuilder` | `SimulationComposition`. |
| `TypePropertiesDialogs` | `TypePropertiesDialogService` | `EditorCoreComposition`. |
| `Dialogs` | `DialogService` | `EditorCoreComposition`. |
| `Geometry` | `ElementGeometryService` | `EditorCoreComposition`. |
| `TerminalLayout` | `TerminalLayoutService` | `EditorCoreComposition`. |
| `GeometryUpdates` | `ElementGeometryUpdateService` | Criado no construtor. |
| `Elements` | `ElementRegistryService` | `EditorCoreComposition`. |
| `Types` | `TypeLibraryService` | Inicializador de propriedade. |
| `ElementoFactory` | `ElementoFactory` | Criado no construtor. |
| `InserirElemento` | `InserirElementoUseCase` | Criado no construtor. |
| `InserirCabo` | `InserirCaboUseCase` | Criado no construtor. |
| `CopiarElementos` | `CopiarElementosUseCase` | Criado no construtor. |
| `ColarElementos` | `ColarElementosUseCase` | Criado no construtor. |
| `ExcluirElemento` | `ExcluirElementoUseCase` | Criado no construtor. |
| `EditarPropriedades` | `EditarPropriedadesUseCase` | Criado no construtor. |
| `MoverElemento` | `MoverElementoUseCase` | `EditingComposition.CreateMoveServices`. |
| `RotacionarElemento` | `RotacionarElementoUseCase` | `EditingComposition.CreateMoveServices`. |
| `RedimensionarBarra` | `RedimensionarBarraUseCase` | `EditingComposition.CreateMoveServices`. |
| `EditarVerticesCabo` | `EditarVerticesCaboUseCase` | Criado no construtor. |

## Métodos relevantes

| Método | Responsabilidade |
| --- | --- |
| `CriarViewportViewModel()` | Chama `ViewportComposition.CreateViewModel(...)`. |
| `InicializarViewport(ViewportViewModel viewportViewModel)` | Cria `ViewportService` e atribui a `Viewport`. |
| `BeginTransaction()` | Retorna `Commands.BeginTransaction()`. |
| `NotifySimulationResultViewModels()` | Notifica ViewModels de `Cabo` e `Carga` após aplicação de resultados. |
| `CriarSelecionarTool()` | Cria `SelecionarTool` com dependências de seleção/movimento. |
| `CriarMoverTool()` | Cria `MoverTool`. |
| `CriarAlinharTool()` | Cria `AlinharTool`. |
| `CriarInserirCaboTool()` | Cria `InserirCaboTool`. |
| `CriarInserirElementoGenericoTool(...)` | Cria `InserirElementoGenericoTool`. |
| `ObterDestinoColagem(...)` | Resolve destino de colagem a partir de mouse, viewport ou centro dos copiados. |
| `LimparEstadoTransitorioProjeto()` | Limpa seleção, hover, edição de cabo, snap, HUD, consultas e retorna ferramenta para seleção. |

## Ciclo de vida

O ciclo de vida observado é associado à janela principal. `MainWindow` cria um `EditorContext` no construtor e o mantém em campo privado `_context`. Não foi observado descarte explícito do contexto. O viewport registra eventos de câmera em `ConfigurarCamera()` e remove a assinatura em `OnUnloaded()`.

# 5. Serviços Registrados

A tabela abaixo consolida serviços e objetos compartilhados criados no grafo principal. A palavra "registrado" aqui significa "criado e mantido pelo composition root manual", não registro em container DI.

| Serviço/Objeto | Quem cria | Quem consome | Responsabilidade real |
| --- | --- | --- | --- |
| `AraciDocument` | `EditorContext` | Persistência, topologia, simulação, use cases, viewport | Documento com coleção de elementos. |
| `Scene` | `EditorCoreComposition` | `ViewportViewModel`, ferramentas, consultas | Cena visual com ViewModels. |
| `SceneQueryService` | `EditorCoreComposition` | Hover, snap, seleção, edição, input visual | Consultas sobre elementos da cena. |
| `HoverService` | `EditorCoreComposition` | `InputRouter`, ferramentas, safe delete | Atualização e limpeza de hover. |
| `SnapService` | `EditorCoreComposition` | Ferramentas de inserção | Snap durante inserção. |
| `TypePropertiesDialogService` | `EditorCoreComposition` | Factories/ViewModels | Diálogos de propriedades de tipo. |
| `DialogService` | `EditorCoreComposition` | Persistência, fluxo de corrente | Mensagens de erro, aviso e resultado. |
| `ElementRegistryService` | `EditorCoreComposition` | Factories, ferramentas, serializer, geometria, input | Catálogo de elementos por kind. |
| `ConnectivityService` | `EditorCoreComposition` | Topologia, edição, exclusão, movimentação, rotação | Busca e valida conexões no documento. |
| `ElectricGraphBuilder` | `EditorCoreComposition` | `TopologyValidator`, consumidores via `EditorContext` | Cria grafo elétrico a partir do documento. |
| `OperationalGraphStateBuilder` | `EditorCoreComposition` | Consumidores via `EditorContext` | Cria estado operacional a partir de grafo. |
| `TopologyValidator` | `EditorCoreComposition` | Consumidores via `EditorContext`; simulação cria outro internamente via `ParameterReader` | Valida topologia do documento. |
| `ElementGeometryService` | `EditorCoreComposition` | Terminal layout, persistência, inserção | Resolve tamanhos de elementos. |
| `TerminalLayoutService` | `EditorCoreComposition` | Factories, persistência, edição, inserção | Atualiza terminais por elemento. |
| `SimulationResultApplier` | `SimulationComposition` | `SimulationPipeline`, `EditorContext` | Aplica resultados ao documento e notifica ViewModels. |
| `SimulationPipeline` | `SimulationComposition` | `AnaliseTab` via `FluxoDeCorrenteApplication` | Executa fluxo de corrente. |
| `SimulationExportService` | `SimulationComposition` | `FluxoDeCorrenteApplication` | Exportação de resultados. |
| `SimulationMessageBuilder` | `SimulationComposition` | `FluxoDeCorrenteApplication` | Montagem de mensagens de simulação. |
| `VisualUpdateService` | `EditingComposition` | Edição, movimentação, vértices de cabo | Atualização visual após mudanças. |
| `NameService` | `EditorContext` | Factories, inserção, colagem | Geração/gestão de nomes. |
| `ElementGeometryUpdateService` | `EditorContext` | ViewModel de barra, resize | Atualização geométrica e de terminais. |
| `ElementoModelFactory` | `EditorContext` | `ElementoFactory`, serializer | Cria modelos a partir de kind. |
| `ElementoViewModelFactory` | `EditorContext` | `ElementoFactory`, scene sync | Cria ViewModels a partir de modelos. |
| `ElementoFactory` | `EditorContext` | Inserção, ferramentas, viewport sync | Cria modelos e ViewModels. |
| `InserirElementoUseCase` | `EditorContext` | Ferramentas de inserção | Inserção de elemento. |
| `InserirCaboUseCase` | `EditorContext` | `InserirCaboTool` | Inserção de cabos. |
| `CopiarElementosUseCase` | `EditorContext` | `ClipboardService` | Cópia de elementos. |
| `ColarElementosUseCase` | `EditorContext` | `ClipboardService` | Colagem de elementos. |
| `ExcluirElementoUseCase` | `EditorContext` | `SafeDeleteService` | Exclusão com comando. |
| `EditarPropriedadesUseCase` | `EditorContext` | `SelectionService` | Edição de propriedades via comandos. |
| `SelectionService` | `EditingComposition` | UI, ferramentas, edição | Mantém seleção. |
| `CableVertexEditService` | `EditingComposition` | Seleção, viewport, safe delete | Edição de vértices de cabo. |
| `SafeDeleteService` | `EditingComposition` | `InputRouter`, ferramentas, `EditarTab` | Exclusão segura. |
| `ClipboardService` | `EditingComposition` | `InputRouter`, `EditarTab` | Copiar e colar seleção. |
| `MoveService` | `EditingComposition` | Ferramentas de seleção/mover | Movimento de elementos. |
| `BarraResizeService` | `EditingComposition` | Viewport e ferramentas | Redimensionamento de barra. |
| `RotationService` | `EditingComposition` | Ferramentas/input | Rotação de elementos. |
| `ToolService` | `EditorContext` | Ribbon, input, viewport | Gerencia ferramenta ativa. |
| `InputRouter` | `EditingComposition` | `ViewportView` | Roteia mouse/teclado para ferramenta atual. |
| `ViewportNavigationService` | `ViewportComposition` | `ViewportView` | Pan e zoom via input. |
| `ProjectPersistenceService` | `PersistenceComposition` | `ArquivoMenuView` | Novo, salvar e abrir projeto. |
| `ViewportViewModel` | `ViewportComposition` | `ViewportView` | ViewModel principal do canvas. |
| `ViewportService` | `EditorContext.InicializarViewport` | Navegação, colagem, visual updates | Câmera, transformações e acesso a ViewModels. |

# 6. Fluxo de Inicialização

O fluxo real de inicialização começa em WPF:

```text
Application Startup
    |
    | App.xaml StartupUri="MainWindow.xaml"
    v
MainWindow.InitializeComponent()
    |
    v
new EditorContext()
    |
    +--> EditorCoreComposition.Create(...)
    +--> SimulationComposition.Create(...)
    +--> EditingComposition.CreateVisualUpdates(...)
    +--> factories / use cases / services
    +--> PersistenceComposition.CreateProjects(...)
    +--> EditingComposition.CreateMoveServices(...)
    +--> new ToolService(...)
    +--> EditingComposition.CreateInput(...)
    +--> ViewportComposition.CreateNavigation(...)
    |
    v
Viewport.Inicializar(_context)
    |
    +--> EditorContext.CriarViewportViewModel()
    |       +--> ViewportComposition.CreateViewModel(...)
    |       +--> DocumentSceneSyncService
    |       +--> ViewportViewModel
    |
    +--> DataContext = ViewportViewModel
    +--> EditorContext.InicializarViewport(...)
    +--> new ViewportService(viewportViewModel)
    +--> ConfigurarCamera()
    |
    v
InicializarRibbon()
    |
    +--> MainWindow.DataContext = EditorContext
    +--> ArquivoMenu.DataContext = EditorContext
```

No `ViewportView`, o método `Inicializar(EditorContext context)`:

1. guarda o contexto;
2. cria o `ViewportViewModel` via `_context.CriarViewportViewModel()`;
3. define `DataContext`;
4. chama `_context.InicializarViewport(_viewportViewModel)`;
5. configura câmera;
6. assina `Unloaded`.

A partir desse ponto, os eventos de mouse e teclado do viewport passam a chamar:

- `_context.Navigation` para pan e zoom;
- `_context.Input` para mouse, teclado e ferramentas;
- `_context.Hover`, `_context.TerminalSnap` e `_context.AlignmentGuides` para limpar estados transitórios.

# 7. Dependências Principais

## AraciDocument

`AraciDocument`, em `Core/Documents/AraciDocument.cs`, é criado uma vez pelo inicializador de propriedade de `EditorContext`. Ele é passado para:

- `EditorCoreComposition.Create(...)`;
- `SimulationComposition.Create(...)`;
- `NameService`;
- `InserirElementoUseCase`;
- `InserirCaboUseCase`;
- `ColarElementosUseCase`;
- `ExcluirElementoUseCase`;
- `PersistenceComposition.CreateProjects(...)`;
- `ViewportComposition.CreateViewModel(...)`.

Também é consumido indiretamente por `CircuitDtoBuilder`, `ParameterReader`, `ConnectivityService`, `ElectricGraphBuilder` e `TopologyValidator`.

## ElementRegistryService

`ElementRegistryService` é criado em `EditorCoreComposition` a partir de `ElementDefinitionsProvider.CreateDefaults()`. Ele é uma dependência transversal para:

- criação de modelos (`ElementoModelFactory`);
- criação de ViewModels (`ElementoViewModelFactory`);
- geometria (`ElementGeometryService`);
- persistência (`ProjectSerializer`);
- ferramentas (`ToolService`);
- input por atalhos (`InputRouter`);
- geração de nomes (`NameService`);
- grafo elétrico (`ElectricGraphBuilder`, com registry opcional).

## ConnectivityService

`ConnectivityService` é criado em `EditorCoreComposition` com o `AraciDocument`. Ele é repassado para:

- `TopologyValidator`;
- `VisualUpdateService`;
- `ExcluirElementoUseCase`;
- `MoveServices`;
- `InserirCaboTool`;
- `RotationService`;
- `MoveService`;
- `SafeDeleteService` indiretamente via exclusão.

## ElectricGraphBuilder

`ElectricGraphBuilder` é criado em `EditorCoreComposition` com `document` e `elements`. Ele é armazenado em `EditorContext.ElectricGraph` e passado para `TopologyValidator`. O builder também é criado em outros pontos fora do composition root principal, como em `ParameterReader(AraciDocument)`, mas essa criação ocorre dentro do fluxo de simulação/leitura de parâmetros, não em `EditorContext`.

## SimulationPipeline

`SimulationPipeline` é criado por `SimulationComposition.Create(...)` com:

- `CircuitDtoBuilder`;
- `FastApiOpenDssGateway`;
- `SimulationResultApplier`.

`AnaliseTab.xaml.cs` consome `Context.Simulation`, `Context.SimulationExport`, `Context.SimulationMessages` e `Context.Dialogs` para criar `FluxoDeCorrenteApplication`.

## ProjectPersistenceService

`ProjectPersistenceService` é criado por `PersistenceComposition.CreateProjects(...)` e armazenado em `EditorContext.Projects`. `ArquivoMenuView.xaml.cs` chama:

- `Context?.Projects.Novo()`;
- `Context?.Projects.AbrirComDialogo()`;
- `Context?.Projects.SalvarComDialogo()`.

# 8. Acoplamentos Observados

## Dependências diretas

O acoplamento direto mais forte é `MainWindow -> EditorContext`, pois a janela instancia diretamente o contexto com `new EditorContext()`.

Outro acoplamento direto relevante é `ViewportView -> EditorContext`. `ViewportView.Inicializar(...)` recebe o contexto e seus handlers acessam vários serviços diretamente:

- `Navigation`;
- `Input`;
- `Viewport`;
- `Hover`;
- `TerminalSnap`;
- `AlignmentGuides`;
- `BarraResize`.

Os tabs do ribbon também dependem diretamente de `EditorContext`:

- `ArquivoMenuView` usa `Context.Projects`;
- `DiagramaTab` usa `Context.Tools`;
- `EditarTab` usa `Context.Tools`, `Context.Clipboard` e `Context.Commands`;
- `AnaliseTab` usa `Context.Simulation`, `Context.SimulationExport`, `Context.SimulationMessages` e `Context.Dialogs`;
- `GerenciarTab` chama `MainWindow.MostrarPropriedades()`.

## Dependências indiretas

Algumas dependências passam por factories:

```text
ElementRegistryService
    -> ElementoModelFactory
    -> ElementoFactory
    -> InserirElementoUseCase / InserirCaboUseCase / DocumentSceneSyncService
```

E para ViewModels:

```text
ElementRegistryService
    -> ElementoViewModelFactory
    -> ElementoFactory
    -> DocumentSceneSyncService
    -> Scene
```

Na persistência:

```text
ProjectPersistenceService
    -> ProjectSerializer
        -> ElementRegistryService
        -> ElementoModelFactory
        -> TerminalLayoutService
        -> ElementGeometryService
```

Na simulação:

```text
SimulationPipeline
    -> CircuitDtoBuilder
        -> ParameterReader
            -> CoreApi
            -> ConnectivityService
            -> TopologyValidator
            -> ElectricGraphBuilder
```

## Dependências transitivas

`AraciDocument` é a raiz de várias dependências transitivas. Alterações no documento impactam:

- cena, por meio de `DocumentSceneSyncService`;
- topologia, por meio de `ConnectivityService` e `ElectricGraphBuilder`;
- simulação, por meio de `CircuitDtoBuilder` e `ParameterReader`;
- persistência, por meio de `ProjectSerializer`;
- comandos e use cases de edição.

`ElementRegistryService` também é transitivo: ele define kinds, tipos, criação de modelos, criação de ViewModels, tamanho geométrico, atualização de terminais e metadados de ribbon.

# 9. Ciclo de Vida dos Objetos

## Singleton implícito por janela

Não há singleton formal no código analisado. Porém, dentro de uma instância de `MainWindow`, o campo `_context` funciona como um objeto compartilhado único durante a vida da janela.

Objetos criados uma vez no `EditorContext` e reutilizados:

- `AraciDocument`;
- `Scene`;
- `EditorState`;
- `EditorSettings`;
- `SelectionBoxViewModel`;
- `TerminalSnapState`;
- `CommandManager`;
- `TypeLibraryService`;
- serviços criados pelas composições;
- factories;
- use cases;
- `ProjectPersistenceService`;
- `SimulationPipeline`.

## Objetos compartilhados

O `AraciDocument` e o `Scene` são compartilhados por múltiplos serviços. O `ViewportService` é criado depois do `ViewportViewModel` e passa a ser acessado por callbacks `Func<ViewportService?>`, como `() => Viewport`, usados por serviços criados antes de o viewport existir.

Esse padrão aparece em:

- `VisualUpdateService`;
- `MoveHudService`;
- `ClipboardService`;
- `MoveService`;
- `RotationService`;
- `ViewportNavigationService`.

## Objetos recriados

Alguns objetos são criados sob demanda:

- ferramentas em `ToolService`, como `SelecionarTool`, `MoverTool`, `AlinharTool`, `DeletarTool`, `InserirCaboTool` e `InserirElementoGenericoTool`;
- modelos e ViewModels em `ElementoFactory`;
- `ParameterReader` e `CircuitBuilder` dentro de `CircuitDtoBuilder.Build()`;
- `FluxoDeCorrenteApplication` em `AnaliseTab.FluxoButton_Click`;
- `FluxoDeCorrenteWindow` em `AnaliseTab.FluxoButton_Click`.

## Assinaturas de eventos

Assinaturas observadas:

- `Selection.SelectionChanged += CableVertexEdit.Refresh` em `EditorContext`;
- `Context.Tools.FerramentaAlterada += ...` em `DiagramaTab` e `EditarTab`, com remoção em `Unloaded`;
- `Viewport.Camera.PropertyChanged += OnCameraChanged` em `ViewportView`, com remoção em `OnUnloaded`.

# 10. Relação com MVVM

A aplicação usa ViewModels para o viewport e para elementos, mas a composição é centralizada no `EditorContext` e a UI acessa serviços diretamente em alguns pontos.

Fluxo visual observado:

```text
AraciDocument
    |
    v
DocumentSceneSyncService
    |
    v
Scene
    |
    v
ElementoViewModel / ViewportViewModel
    |
    v
Controls WPF
```

`ViewportComposition.CreateViewModel(...)` cria `DocumentSceneSyncService` e depois `ViewportViewModel`. O `ViewportViewModel` expõe:

- `Document`;
- `Scene`;
- `SelectionBox`;
- `TerminalSnap`;
- `CableVertexEdit`;
- `MoveHud`;
- `AlignmentGuides`;
- `Elementos => Scene.Elementos`;
- propriedades visuais dependentes de zoom.

`ElementoViewModelFactory` cria ViewModels por meio do catálogo de elementos. Ele usa:

- `IElementCatalog`;
- `IElementModelFactory`;
- `NameService`;
- `TypePropertiesDialogService`;
- `TerminalLayoutService`;
- `ElementGeometryUpdateService`.

Quando o ViewModel criado é `BarraViewModel`, `ElementoViewModelFactory.ConfigurarViewModel(...)` atribui `barra.GeometryUpdates = _geometryUpdates`.

A UI do ribbon não é puramente command-binding MVVM. Os code-behinds chamam serviços do contexto diretamente, por exemplo `Context.Tools.AtivarMover()`, `Context.Clipboard.Colar()` e `Context.Projects.SalvarComDialogo()`.

# 11. Relação com Topologia

A topologia é conectada no núcleo do editor por `EditorCoreComposition`:

```text
EditorContext
    |
    v
EditorCoreComposition.Create(Document, Settings, Types)
    |
    +--> ConnectivityService(document)
    |
    +--> ElectricGraphBuilder(document, elements)
    |
    +--> TopologyValidator(document, connectivity, electricGraph)
```

O encadeamento pedido é:

```text
EditorContext
    |
    v
ElectricGraphBuilder
    |
    v
TopologyValidator
```

No código real, `TopologyValidator` recebe simultaneamente:

- `AraciDocument`;
- `ConnectivityService`;
- `ElectricGraphBuilder`.

`ConnectivityService` e `ElectricGraphBuilder` compartilham o mesmo `AraciDocument` criado pelo contexto. `ElectricGraphBuilder` também recebe `ElementRegistryService`, permitindo resolver kind pelo registro quando cria nós.

Além do grafo mantido no contexto, `ParameterReader(AraciDocument)` cria internamente novo `ConnectivityService`, novo `TopologyValidator(document)`, e novo `ElectricGraphBuilder(document)`. Portanto, há duas formas observadas de composição topológica:

- composição compartilhada no `EditorContext`;
- composição local dentro de `ParameterReader` para simulação.

# 12. Relação com Persistência

A persistência é ligada ao contexto por:

```text
EditorContext
    |
    v
PersistenceComposition.CreateProjects(...)
    |
    +--> ProjectSerializer
    +--> FileSystemProjectRepository
    +--> ProjectFileDialogService
    |
    v
ProjectPersistenceService
    |
    v
EditorContext.Projects
```

Dependências reais passadas ao `ProjectPersistenceService`:

- `Document`;
- `Commands`;
- `ProjectSerializer`;
- `FileSystemProjectRepository`;
- `ProjectFileDialogService`;
- `Dialogs`;
- `LimparEstadoTransitorioProjeto`.

`ArquivoMenuView.xaml.cs` é o consumidor direto na UI. Ele resolve o contexto por `DataContext` ou pela janela e chama métodos do serviço de projetos.

O callback `LimparEstadoTransitorioProjeto` limpa:

- seleção;
- hover;
- edição de vértices de cabo;
- estado de snap;
- visibilidade da caixa de seleção;
- HUD de movimento;
- cache de consultas de cena;
- ferramenta atual, retornando para seleção.

# 13. Relação com Simulação

A simulação é ligada ao contexto por:

```text
EditorContext
    |
    v
SimulationComposition.Create(Document, NotifySimulationResultViewModels)
    |
    +--> SimulationResultApplier
    +--> FastApiOpenDssGateway
    +--> CircuitDtoBuilder
    +--> SimulationPipeline
    +--> SimulationExportService
    +--> SimulationMessageBuilder
    |
    v
EditorContext.Simulation
```

`SimulationPipeline`, em `Applications/Simulation/SimulationPipeline.cs`, recebe:

- `CircuitDtoBuilder`;
- `ISimulationGateway`;
- `ISimulationResultApplier`.

O método `ExecutarFluxoDeCorrenteAsync()`:

1. chama `_circuitBuilder.Build()`;
2. chama `_gateway.SimularAsync(dto)`;
3. aplica o resultado com `_simulationResults.Apply(resultado)`;
4. retorna `SimulationResultDto`.

`CircuitDtoBuilder`, em `Applications/Simulation/CircuitDtoBuilder.cs`, recebe `AraciDocument` e, a cada `Build()`, cria:

- `ParameterReader reader = new(_document)`;
- `CircuitBuilder builder = new(reader)`.

Na UI, `AnaliseTab.xaml.cs` cria `FluxoDeCorrenteApplication` sob demanda com:

- `Context.Simulation`;
- `Context.SimulationExport`;
- `Context.SimulationMessages`;
- `Context.Dialogs`.

# 14. Dívidas Técnicas

As dívidas abaixo são baseadas somente no código observado.

## Composition root concentrado em EditorContext

`EditorContext` cria e mantém muitos serviços, use cases, factories, estados e callbacks. Isso torna o grafo de dependências explícito, mas concentra responsabilidades de composição, estado de editor e helpers privados em uma única classe.

## Ausência de container ou módulo formal de DI

Não foi observado container de injeção de dependência. A composição manual é simples e rastreável, mas dificulta substituição sistemática de implementações por ambiente, testes ou perfis de execução.

## UI acoplada diretamente ao EditorContext

Vários code-behinds acessam `EditorContext` e seus serviços diretamente. Exemplos:

- `ArquivoMenuView` chama `Context.Projects`;
- `DiagramaTab` chama `Context.Tools`;
- `EditarTab` chama `Context.Tools`, `Context.Clipboard` e `Context.Commands`;
- `AnaliseTab` constrói aplicação de fluxo com serviços do contexto;
- `ViewportView` chama `Context.Input`, `Context.Navigation`, `Context.Hover` e outros.

## Dependências duplicadas em topologia

Há um conjunto topológico compartilhado em `EditorContext` (`Connectivity`, `ElectricGraph`, `Topology`) e também criação local de `ConnectivityService`, `TopologyValidator` e `ElectricGraphBuilder` em `ParameterReader(AraciDocument)`. Isso é evidência de composições paralelas para o mesmo domínio topológico.

## Viewport inicializado em duas fases

Serviços recebem `Func<ViewportService?>` antes de `Viewport` existir. O `ViewportService` só é criado em `EditorContext.InicializarViewport(...)`, chamado por `ViewportView.Inicializar(...)`. Esse padrão resolve a dependência circular, mas exige que consumidores aceitem `null`.

## Ferramentas criadas por delegates dentro do contexto

`ToolService` recebe delegates privados de `EditorContext` para criar ferramentas. Essa abordagem adia a criação e permite ferramentas novas por ativação, mas mantém a construção concreta das ferramentas acoplada ao contexto.

## Recursos externos instanciados diretamente

`SimulationComposition` instancia `FastApiOpenDssGateway` diretamente. Embora o pipeline dependa de `ISimulationGateway`, a escolha concreta do gateway está fixa na composição atual.

## Persistência concreta instanciada diretamente

`PersistenceComposition` instancia `FileSystemProjectRepository` e `ProjectFileDialogService` diretamente. Isso é coerente com a composição manual, mas não há abstração de configuração para trocar implementação nesse ponto sem alterar código.

# 15. Comparação com Arquitetura-Alvo

A composição atual tem uma característica positiva importante: a maioria das dependências é explícita, criada por construtores e fácil de localizar. `EditorContext` funciona como um composition root manual e torna visível como documento, cena, topologia, persistência, simulação, edição e viewport se conectam.

Em uma arquitetura-alvo baseada em Use Cases, Domain-Centric Design, Service Layer e Composition Root, a implementação atual pode evoluir nos seguintes aspectos:

| Aspecto | Estado atual | Arquitetura-alvo desejada |
| --- | --- | --- |
| Composition Root | Distribuído entre `MainWindow`, `EditorContext` e `Service/Composition`. | Composition root mais isolado, com `EditorContext` menos responsável por construir objetos concretos. |
| Use Cases | Existem use cases de edição e diagrama criados no contexto. | Use cases como fronteiras explícitas de aplicação, com dependências organizadas por módulo. |
| Domain-Centric Design | `AraciDocument`, modelos e topologia são compartilhados diretamente. | Domínio mais independente da UI e com serviços de aplicação fazendo mediação. |
| Service Layer | Serviços são criados manualmente e expostos no contexto. | Camada de serviços com contratos estáveis, menos acesso direto da UI a serviços internos. |
| UI/MVVM | Code-behinds consomem `EditorContext` diretamente. | Maior uso de ViewModels/commands para reduzir acoplamento entre UI e serviços. |
| Topologia | Topologia composta no contexto e recriada localmente no `ParameterReader`. | Composição topológica única ou claramente separada por escopo, com contratos consistentes. |
| Simulação | Pipeline usa contratos, mas gateway concreto é fixado na composição. | Configuração explícita de gateways por ambiente, teste ou backend. |
| Persistência | Serviço de projeto é composto manualmente com repositório e diálogos concretos. | Persistência configurável, com fronteiras claras entre UI, aplicação e infraestrutura. |

O desenho atual já contém blocos compatíveis com a arquitetura-alvo: use cases, abstrações (`IProjectPersistenceService`, `ISimulationPipeline`, `ISimulationGateway`, `IElementCatalog`, `IElementModelFactory`, `IElementViewModelFactory`), serviços de aplicação e composições separadas por área. A principal diferença é que o `EditorContext` ainda acumula o papel de contexto de sessão, service locator para a UI e composition root de alto nível.

Uma evolução natural seria preservar a clareza da composição manual, mas reduzir o acoplamento dos code-behinds ao contexto completo, expondo ViewModels ou facades mais específicos para arquivo, edição, diagrama, análise e viewport. Isso aproximaria a aplicação de um desenho mais modular sem contradizer os contratos e serviços já existentes no código.
