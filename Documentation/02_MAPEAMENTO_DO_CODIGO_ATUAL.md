# 1. Visão Geral da Solução

Este documento mapeia a solução atual do Araci a partir do código existente no repositório. O objetivo é registrar a estrutura real do sistema, seus namespaces, classes, módulos, fluxos e acoplamentos observáveis, sem introduzir módulos, dependências ou responsabilidades não presentes no código.

A solução contém dois projetos declarados em `Araci.slnx`:

| Projeto | Arquivo | Tipo observado |
| --- | --- | --- |
| Araci | `Araci.csproj` | Aplicação WPF desktop em `.NET 8` com `UseWPF` e `UseWindowsForms`. |
| Araci.TechnicalChecks | `Araci.TechnicalChecks/Araci.TechnicalChecks.csproj` | Executável de verificações técnicas que referencia `Araci.csproj`. |

O projeto principal `Araci.csproj` define `OutputType` como `WinExe`, `TargetFramework` como `net8.0-windows`, `RootNamespace` como `Araci`, `Nullable` habilitado, `ImplicitUsings` desabilitado e referência ao pacote `SharpVectors.Wpf` versão `1.8.5`. O uso de `SharpVectors.Wpf` aparece nos controles visuais baseados em SVG, como `BarraControl`, `CargaControl`, `GeradorControl`, `SinControl` e `TransformadorControl`.

A aplicação é iniciada por `App.xaml` e `App.xaml.cs`, com janela principal em `MainWindow.xaml` e `MainWindow.xaml.cs`. A classe `MainWindow`, no namespace `Araci`, cria um `EditorContext`, inicializa o `ViewportView` e associa o mesmo contexto ao Ribbon e ao menu de arquivo.

O ponto central de orquestração da aplicação é `Service/EditorContext.cs`, classe `EditorContext`, namespace `Araci.Services`. Essa classe monta o documento (`AraciDocument`), a cena (`Scene`), serviços de seleção, hover, snap, conectividade, topologia, simulação, persistência, factories, comandos, tools, viewport e navegação. A composição é parcialmente separada em classes internas de `Service/Composition`.

Em alto nível, a solução atual pode ser representada assim:

```text
+---------------------------+
|        MainWindow         |
|  Ribbon + Viewport + UI   |
+-------------+-------------+
              |
              v
+---------------------------+
|       EditorContext       |
| Composicao e orquestracao |
+------+------+------+------+ 
       |      |      |
       |      |      +------------------------+
       |      |                               |
       v      v                               v
+----------+  +----------------+   +-------------------------+
| Document |  | Scene/ViewModel|   | Services / UseCases     |
| Core     |  | Presentation   |   | Tools / Commands        |
+----+-----+  +-------+--------+   +-----------+-------------+
     |                |                        |
     v                v                        v
+----------+  +----------------+   +-------------------------+
| Models   |  | Controls/Ribbon|   | Persistence/Simulation  |
+----------+  +----------------+   +-------------------------+
```

A arquitetura atual mistura padrões de camadas com composição manual. Há separação física entre `Core`, `Models`, `Applications`, `Service`, `Infrastructure`, `DTOs`, `ViewModels`, `Views`, `Controls` e `Ribbon`, mas o `EditorContext` concentra muitas conexões entre essas partes.

# 2. Estrutura Física

A estrutura física observada, desconsiderando `bin`, `obj`, `.git` e `.vs`, é a seguinte:

```text
Araci Engine/
|-- API/
|-- Application/
|   |-- Abstractions/
|-- Applications/
|   |-- Abstractions/
|   |-- Analisar/
|   |   |-- FluxoDeCorrente/
|   |   |-- FluxoPotencia/
|   |-- Commands/
|   |-- Diagrama/
|   |   |-- InserirCabo/
|   |   |-- InserirElemento/
|   |-- Editar/
|   |   |-- Alinhar/
|   |   |-- Base/
|   |   |-- Deletar/
|   |   |-- Mover/
|   |   |-- Selecionar/
|   |-- Editor/
|   |-- Factories/
|   |-- Projects/
|   |-- Scene/
|   |-- Simulation/
|   |-- UseCases/
|       |-- Diagrama/
|       |-- Editar/
|-- Araci.TechnicalChecks/
|-- Controls/
|   |-- Base/
|   |-- Converters/
|   |-- Interfaces/
|-- Core/
|   |-- Commands/
|   |-- Documents/
|   |-- Events/
|   |-- Features/
|   |-- Rendering/
|   |-- SceneNodes/
|   |-- SceneQueries/
|   |-- Scenes/
|   |-- Spatial/
|   |-- Viewport/
|-- Documentation/
|-- DTOs/
|-- Infrastructure/
|   |-- Persistence/
|   |-- Simulation/
|-- Models/
|   |-- Interfaces/
|   |-- Tipos/
|-- Properties/
|   |-- Types/
|-- Resources/
|   |-- Icons/
|   |-- Svg/
|   |-- Icons/
|   |-- Styles/
|   |-- Templates/
|-- Ribbon/
|   |-- Tabs/
|-- Service/
|   |-- Composition/
|-- ViewModels/
|   |-- Base/
|   |-- VisualStates/
|-- Views/
```

Observações sobre a estrutura:

- `Application/Abstractions` existe fisicamente, mas não contém arquivos encontrados no levantamento.
- `Applications` concentra abstrações, use cases, tools, comandos, factories, sincronização de cena, projetos e simulação.
- `Service` concentra serviços de domínio/aplicação e composição manual.
- `Core` contém documento, comandos, eventos, scene graph, queries espaciais, rendering e viewport.
- `Infrastructure` contém persistência em arquivo e gateway de simulação.
- `DTOs` contém contratos de simulação e leitores/construtores de circuito.
- `Models` contém o modelo de domínio elétrico e tipos de elementos.
- `Views`, `Controls`, `Ribbon`, `Properties` e `ViewModels` compõem a apresentação WPF.

# 3. Estrutura de Namespaces

Os namespaces reais encontrados no código são:

| Namespace | Localização principal | Responsabilidade observada |
| --- | --- | --- |
| `Araci` | Raiz, `MainWindow.xaml.cs`, `App.xaml.cs` | Aplicação WPF e janela principal. |
| `Araci.API` | `API/CoreApi.cs` | API interna para leitura de documento, elementos e parâmetros. |
| `Araci.Maestro` | `API/CoreMaestro.cs` | Fachada sobre `CoreApi`. |
| `Araci.Applications.Abstractions` | `Applications/Abstractions` | Contratos de aplicação, catálogo, sessão, persistência, simulação e seleção. |
| `Araci.Applications.Analisar.FluxoDeCorrente` | `Applications/Analisar/FluxoDeCorrente` | Aplicação e janela de fluxo de corrente. |
| `Araci.Applications.Analisar.FluxoPotencia` | `Applications/Analisar/FluxoPotencia` | Aplicação de fluxo de potência textual. |
| `Araci.Applications.Commands` e `Araci.Core.Commands` | `Applications/Commands`, `Core/Commands` | Comandos undoable e histórico de comandos. |
| `Araci.Applications.Diagrama` | `Applications/Diagrama` | Preview de inserção e ferramentas de inserção. |
| `Araci.Applications.Editar.*` | `Applications/Editar` | Tools de edição, seleção, mover, deletar e alinhar. |
| `Araci.Applications.Editor` | `Applications/Editor` | `InputRouter` e `ToolService`. |
| `Araci.Applications.Factories` | `Applications/Factories` | Factories e definições de elementos. |
| `Araci.Applications.Projects` | `Applications/Projects` | Serviço de persistência de projeto. |
| `Araci.Applications.Scene` | `Applications/Scene` | Sincronização entre documento e cena. |
| `Araci.Applications.Simulation` | `Applications/Simulation` | Pipeline e builder de DTO de circuito. |
| `Araci.Applications.UseCases.*` | `Applications/UseCases` | Casos de uso de diagrama e edição. |
| `Araci.Controls.*` | `Controls` | Controles WPF dos elementos, conversores e interface visual. |
| `Araci.Core.*` | `Core` | Documento, eventos, comandos, scene graph, spatial index e viewport. |
| `Araci.DTOs` | `DTOs` | DTOs de simulação, `ParameterReader`, `CircuitBuilder`, cliente HTTP. |
| `Araci.Infrastructure.Persistence` | `Infrastructure/Persistence` | Serialização, repositório e diálogos de arquivo. |
| `Araci.Infrastructure.Simulation` | `Infrastructure/Simulation` | Gateway FastAPI/OpenDSS. |
| `Araci.Models` | `Models` | Elementos de domínio, terminais, parâmetros, papéis de domínio. |
| `Araci.Models.Tipos` | `Models/Tipos` | Tipos técnicos de barra, cabo, carga, gerador, SIN e transformador. |
| `Araci.Properties.*` | `Properties` | Views de propriedades de instância e tipo. |
| `Araci.Ribbon.*` | `Ribbon` | Ribbon e abas. |
| `Araci.Services` | `Service` | Serviços de editor, topologia, conectividade, geometria, seleção e simulação. |
| `Araci.Services.Composition` | `Service/Composition` | Composição manual de subsistemas. |
| `Araci.TechnicalChecks` | `Araci.TechnicalChecks/Program.cs` | Verificações técnicas automatizadas. |
| `Araci.ViewModels.*` | `ViewModels` | ViewModels de elementos, tipos, viewport e propriedades. |
| `Araci.Views` | `Views` | Views principais do editor. |

# 4. Principais Módulos

## 4.1 Inicialização e composição

Arquivos principais:

- `MainWindow.xaml`
- `MainWindow.xaml.cs`
- `Service/EditorContext.cs`
- `Service/Composition/EditorCoreComposition.cs`
- `Service/Composition/EditingComposition.cs`
- `Service/Composition/PersistenceComposition.cs`
- `Service/Composition/SimulationComposition.cs`
- `Service/Composition/ViewportComposition.cs`

Responsabilidade real:

- Criar o contexto de edição.
- Inicializar viewport, ribbon e painel de propriedades.
- Instanciar serviços, factories, use cases, comandos, simulação e persistência.
- Conectar eventos e callbacks entre serviços.

`EditorContext` é a classe mais central da aplicação. Ela instancia `AraciDocument`, `Scene`, `SceneQueryService`, `HoverService`, `SnapService`, `ConnectivityService`, `ElectricGraphBuilder`, `TopologyValidator`, `SimulationPipeline`, `ProjectPersistenceService`, `ToolService`, `InputRouter`, entre outros.

## 4.2 Domínio elétrico

Arquivos principais:

- `Core/Documents/AraciDocument.cs`
- `Models/Elemento.cs`
- `Models/ElementoEquipamento.cs`
- `Models/ElementoLinear.cs`
- `Models/Barra.cs`
- `Models/Cabo.cs`
- `Models/Carga.cs`
- `Models/Gerador.cs`
- `Models/Sin.cs`
- `Models/Transformador.cs`
- `Models/Terminal.cs`
- `Models/TerminalEndpoint.cs`
- `Models/Parameter.cs`

Responsabilidade real:

- Representar os elementos elétricos e seus parâmetros.
- Representar terminais e endpoints de conexão.
- Diferenciar elementos gráficos, anotativos e elétricos topológicos via `ElementoDomainRole`.
- Fornecer clonagem de elementos.

## 4.3 Aplicação e edição

Arquivos principais:

- `Applications/Editor/InputRouter.cs`
- `Applications/Editor/ToolService.cs`
- `Applications/Editar/Base/ITool.cs`
- `Applications/Editar/Selecionar/SelecionarTool.cs`
- `Applications/Diagrama/InserirElemento/InserirElementoGenerico.cs`
- `Applications/Diagrama/InserirCabo/InserirCabo.cs`
- `Applications/UseCases/Diagrama/InserirElementoUseCase.cs`
- `Applications/UseCases/Diagrama/InserirCaboUseCase.cs`
- `Applications/UseCases/Editar/*`

Responsabilidade real:

- Roteamento de mouse, teclado e atalhos.
- Ativação de ferramentas.
- Inserção de elementos e cabos.
- Seleção, drag, box selection, edição de vértices de cabo, movimentação, rotação, exclusão, cópia e colagem.
- Execução de comandos com Undo/Redo.

## 4.4 Persistência

Arquivos principais:

- `Applications/Projects/ProjectPersistenceService.cs`
- `Infrastructure/Persistence/ProjectSerializer.cs`
- `Infrastructure/Persistence/ProjectFileDto.cs`
- `Infrastructure/Persistence/FileSystemProjectRepository.cs`
- `Infrastructure/Persistence/ProjectFileDialogService.cs`

Responsabilidade real:

- Criar DTO de projeto.
- Serializar e desserializar JSON.
- Ler e escrever arquivos.
- Abrir e salvar projetos por diálogos WPF/Win32.
- Preservar elementos, parâmetros, tipos, terminais e vértices.

## 4.5 Simulação

Arquivos principais:

- `Applications/Simulation/SimulationPipeline.cs`
- `Applications/Simulation/CircuitDtoBuilder.cs`
- `DTOs/ParameterReader.cs`
- `DTOs/CircuitBuilder.cs`
- `DTOs/CircuitDto.cs`
- `DTOs/SimulationApiClient.cs`
- `Infrastructure/Simulation/FastApiOpenDssGateway.cs`
- `Service/SimulationResultApplier.cs`
- `Service/SimulationExportService.cs`
- `Service/SimulationMessageBuilder.cs`
- `Applications/Analisar/FluxoDeCorrente/FluxoDeCorrenteApplication.cs`

Responsabilidade real:

- Ler parâmetros do documento.
- Validar topologia antes da construção do DTO.
- Construir `CircuitDto`.
- Chamar API HTTP em `http://127.0.0.1:8000/simular`.
- Aplicar resultados de corrente em cabos e cargas.
- Exportar script DSS e JSON de resultado quando configurado.

## 4.6 Apresentação

Arquivos principais:

- `Views/ViewportView.xaml`
- `Views/ViewportView.xaml.cs`
- `ViewModels/ViewportViewModel.cs`
- `ViewModels/ElementoViewModel.cs`
- `Controls/*Control.cs`
- `Ribbon/RibbonView.xaml`
- `Ribbon/Tabs/*.xaml.cs`
- `Properties/*.xaml`

Responsabilidade real:

- Renderizar elementos no canvas WPF.
- Aplicar transformações de câmera.
- Exibir seleção, hover, preview, guias de alinhamento, snap de terminal e HUD de movimento.
- Conectar Ribbon a tools, comandos, persistência e simulação.
- Exibir e editar propriedades.

# 5. Modelo de Domínio

## 5.1 AraciDocument

`Core/Documents/AraciDocument.cs`, namespace `Araci.Core.Documents`, define a classe `AraciDocument`.

Responsabilidade real:

- Manter `ObservableCollection<Elemento> Elementos`.
- Adicionar elemento se ele ainda não estiver na coleção.
- Remover elemento se ele existir na coleção.
- Limpar todos os elementos.

`AraciDocument` é o agregado de documento usado por persistência, simulação, conectividade, topologia e sincronização de cena.

## 5.2 Elementos

`Models/Elemento.cs` define a base abstrata `Elemento`. Ela contém:

- `Guid Id`.
- `PosicaoX`, `PosicaoY`.
- `Rotacao`.
- `Escala`.
- `Tipo`.
- Dicionário de `Parameter`.
- Propriedade `Nome`.
- `DomainRole`.
- `ParticipaDoGrafoEletrico`.
- Método abstrato `Clonar`.

`Models/ElementoDomainRole.cs` define:

| Valor | Significado no código |
| --- | --- |
| `Grafico` | Papel padrão da classe base. |
| `Anotacao` | Papel previsto no enum; há uso em verificações técnicas com elemento fake. |
| `EletricoTopologico` | Elemento participa do grafo elétrico. |

As classes concretas observadas são:

| Classe | Arquivo | Base | Observações reais |
| --- | --- | --- | --- |
| `Barra` | `Models/Barra.cs` | `Elemento`, `ITerminalOwner` | Possui altura, tensão e múltiplos terminais calculados por altura. |
| `Cabo` | `Models/Cabo.cs` | `ElementoLinear`, `ITerminalOwner` | Possui origem/destino, terminais, vértices, preview e parâmetros elétricos. |
| `Carga` | `Models/Carga.cs` | `ElementoEquipamento` | Define terminal superior e parâmetros de potência, tensão e corrente herdados. |
| `Gerador` | `Models/Gerador.cs` | `ElementoEquipamento` | Adiciona potência aparente e fator de potência; cria quatro terminais. |
| `Sin` | `Models/Sin.cs` | `ElementoEquipamento` | Representa fonte externa; cria terminais norte, sul, leste e oeste. |
| `Transformador` | `Models/Transformador.cs` | `ElementoEquipamento` | Define primário, secundário, tensões, potência, R/X percentual e ligações. |

`ElementoEquipamento`, em `Models/ElementoEquipamento.cs`, centraliza parâmetros comuns de equipamentos, como `Barra`, `BarraId`, `Alimentador`, potências, tensões e correntes. Ele sobrescreve `DomainRole` para `EletricoTopologico` e mantém lista de `Terminal`.

`ElementoLinear`, em `Models/ElementoLinear.cs`, adiciona `PosicaoX2`, `PosicaoY2` e `Comprimento`, mas `Cabo` utiliza principalmente `Vertices`, endpoints e parâmetros próprios.

## 5.3 Tipos

Os tipos ficam em `Models/Tipos`:

- `TipoElemento`
- `TipoBarra`
- `TipoCabo`
- `TipoCarga`
- `TipoGerador`
- `TipoSin`
- `TipoTransformador`

`Service/TypeLibraryService.cs` instancia coleções padrão para cabos, cargas, geradores, SIN, transformadores e barras. Exemplos reais:

- `TipoCabo` padrão `LC-500MCM`.
- `TipoCarga` padrão `Carga MT`.
- `TipoGerador` padrão `Gerador Eolico`.
- `TipoSin` padrão `Rede Externa`.
- `TipoTransformador` padrão `Transformador 2 Enrolamentos`.
- `TipoBarra` padrão `Barra Vertical`.

## 5.4 Conexões

Conexões são representadas principalmente por:

- `Models/Terminal.cs`
- `Models/TerminalEndpoint.cs`
- `Models/ITerminalOwner.cs`
- `Models/TerminalKind.cs`
- `Models/TerminalDirection.cs`
- `Models/TerminalPlacement.cs`
- `Models/Cabo.cs`
- `Service/ConnectivityService.cs`

`Terminal` contém:

- `Dono`, do tipo `Elemento`.
- `Id`.
- `Posicao`.
- `PosicaoLocal`.
- `Barra`.
- `Kind`.
- `Direction`.

`TerminalEndpoint` é um `readonly struct` com `ElementId` e `TerminalId`, comparação por valor, normalização e `PairKey` para comparar pares de conexão sem depender da ordem.

`Cabo` armazena:

- `OrigemId`
- `DestinoId`
- `OrigemTerminalId`
- `DestinoTerminalId`
- `BarraOrigem`
- `BarraDestino`
- `Vertices`
- terminais `ORIGEM` e `DESTINO`

`ConnectivityService` resolve elementos por Id, terminais, barramentos, cabos conectados, terminais ocupados, reancoragem de cabos e validação de conexões.

## 5.5 Topologia

A topologia elétrica é tratada por:

- `Service/ElectricGraph.cs`
- `Service/ElectricGraphBuilder.cs`
- `Service/ElectricGraphNode.cs`
- `Service/ElectricGraphEdge.cs`
- `Service/ElectricGraphTerminal.cs`
- `Service/TopologyValidator.cs`
- `Service/TopologyValidationResult.cs`
- `Service/TopologyIssue.cs`
- `Service/OperationalGraphState.cs`
- `Service/OperationalGraphStateBuilder.cs`

`ElectricGraphBuilder` cria nós para elementos que participam do grafo elétrico e são `ITerminalOwner`, exceto `Cabo`. Cabos viram arestas. A validação de aresta verifica origem, destino, existência de elemento, existência de terminal, conexão com o mesmo elemento, mesmo terminal e duplicidade.

`TopologyValidator` valida nomes, cabos, equipamentos e circuito. O código verifica, entre outros pontos:

- elementos elétricos sem `Nome`;
- nomes duplicados;
- cabos inválidos retornados por `ElectricGraph`;
- equipamentos com `BarraId` inválido;
- cargas e geradores sem conexão topológica utilizável por Id;
- circuito sem fonte slack;
- circuito com mais de um equipamento e sem cabo.

`OperationalGraphStateBuilder` executa uma busca a partir de fontes. Ele prioriza nós cujo `SourceElement` é `Sin`; se não houver SIN, usa o primeiro `Gerador`. O resultado separa nós e arestas energizados e desenergizados.

# 6. Camada de Aplicação

## 6.1 Tools

O contrato `ITool` está em `Applications/Editar/Base/ITool.cs`, namespace `Araci.Applications.Editar.Base`. Ele define:

- `Nome`
- `MantemBotaoAtivado`
- `IsBusy`
- `Ativar`
- `Desativar`
- `Cancelar`
- handlers de mouse
- `HandlesKey`
- `OnKeyDown`

Tools reais:

| Tool | Arquivo | Responsabilidade |
| --- | --- | --- |
| `SelecionarTool` | `Applications/Editar/Selecionar/SelecionarTool.cs` | Seleção, box selection, drag move, resize de barra, edição de vértices de cabo e rotação por espaço. |
| `MoverTool` | `Applications/Editar/Mover/MoverTool.cs` | Modo de movimentação baseado na seleção. |
| `AlinharTool` | `Applications/Editar/Alinhar/Alinhar.cs` | Alinhamento de elementos com referência. |
| `DeletarTool` | `Applications/Editar/Deletar/DeletarTool.cs` | Exclusão via `SafeDeleteService`. |
| `InserirElementoGenericoTool` | `Applications/Diagrama/InserirElemento/InserirElementoGenerico.cs` | Inserção de elemento com preview, snap, rotação por espaço e retorno à seleção. |
| `InserirCaboTool` | `Applications/Diagrama/InserirCabo/InserirCabo.cs` | Inserção de cabo por terminais, validação de conexão, vértices intermediários, preview e alinhamento. |

`ToolService`, em `Applications/Editor/ToolService.cs`, cria e alterna ferramentas. Ele também ativa inserção por `kind`, usando `IElementCatalog`.

## 6.2 Commands

Há comandos em duas pastas:

- `Applications/Commands`
- `Core/Commands`

Classes reais:

| Classe | Arquivo | Responsabilidade |
| --- | --- | --- |
| `IUndoableCommand` | `Applications/Commands/IUndoableCommand.cs` | Contrato `Execute`, `Undo`, `Redo`. |
| `CommandManager` | `Applications/Commands/CommandManager.cs` | Pilhas de Undo/Redo, execução e transações. Namespace declarado: `Araci.Core.Commands`. |
| `CompositeCommand` | `Applications/Commands/CompositeCommand.cs` | Agrupa comandos. |
| `TransactionScope` | `Applications/Commands/TransactionScope.cs` | Transação manual, commit explícito. Namespace declarado: `Araci.Core.Transactions`. |
| `AddElementoCommand` | `Applications/Commands/AddElementoCommand.cs` | Adiciona elemento ao documento com nome único. Namespace declarado: `Araci.Core.Commands`. |
| `DeleteElementCommand` | `Applications/Commands/DeleteElementCommand.cs` | Remove e restaura elemento no documento. Namespace declarado: `Araci.Core.Commands`. |
| `MoveElementoCommand` | `Core/Commands/MoveElementoCommand.cs` | Aplica estados antes/depois a elemento. |
| `RotateElementoCommand` | `Core/Commands/RotateElementoCommand.cs` | Aplica estados de rotação em lote. |
| `ResizeBarraCommand` | `Core/Commands/ResizeBarraCommand.cs` | Comando de redimensionamento de barra. |
| `BulkPropertyChangeCommand` | `Core/Commands/BulkPropertyChangeCommand.cs` | Alteração de propriedades em lote. |
| `AlignElementCommand` | `Core/Commands/AlignElementCommand.cs` | Alinhamento com Undo/Redo. |

Há uma inconsistência física relevante: alguns arquivos em `Applications/Commands` declaram namespace `Araci.Core.Commands` ou `Araci.Core.Transactions`.

## 6.3 Services

Serviços principais em `Service`:

- `SelectionService`
- `SafeDeleteService`
- `MoveService`
- `RotationService`
- `BarraResizeService`
- `ConnectivityService`
- `SnapService`
- `AlignmentGuideService`
- `HoverService`
- `VisualUpdateService`
- `ElementGeometryService`
- `ElementGeometryUpdateService`
- `TerminalLayoutService`
- `NameService`
- `TypeLibraryService`
- `ElementRegistryService`
- `TopologyValidator`
- `ElectricGraphBuilder`
- `OperationalGraphStateBuilder`
- `SimulationResultApplier`
- `SimulationExportService`
- `SimulationMessageBuilder`
- `ViewportService`
- `ViewportNavigationService`
- `ClipboardService`
- `DialogService`
- `TypePropertiesDialogService`

Esses serviços estão no namespace `Araci.Services`.

## 6.4 Orquestração

A orquestração ocorre principalmente em `EditorContext`. O fluxo de montagem é:

```text
EditorContext
|-- EditorCoreComposition.Create(...)
|-- SimulationComposition.Create(...)
|-- EditingComposition.CreateVisualUpdates(...)
|-- Factories e UseCases
|-- EditingComposition.CreateSelection(...)
|-- PersistenceComposition.CreateProjects(...)
|-- EditingComposition.CreateMoveServices(...)
|-- ToolService
|-- EditingComposition.CreateInput(...)
|-- ViewportComposition.CreateNavigation(...)
```

Essa composição manual torna explícitas as dependências, mas concentra muitas responsabilidades em uma única classe.

# 7. Camada de Apresentação

## 7.1 Views

`Views/ViewportView.xaml` define o canvas principal. Ele contém:

- `WorldLayer`, um `ItemsControl` com `Canvas` para elementos.
- DataTemplates para `BarraViewModel`, `CaboViewModel`, `CargaViewModel`, `GeradorViewModel`, `SinViewModel` e `TransformadorViewModel`.
- `AlignmentGuideLayer`.
- `SelectionLayer`.
- `CableVertexHandleLayer`.
- `TerminalSnapLayer`.
- HUD de movimento.

`Views/ViewportView.xaml.cs` recebe eventos de mouse, teclado, roda do mouse e navegação. Ele converte coordenadas de tela para mundo via `ViewportService`, cria `ToolInputState` e encaminha eventos ao `InputRouter`.

## 7.2 Controls

Controles reais:

| Controle | Arquivo | Rendering |
| --- | --- | --- |
| `BarraControl` | `Controls/BarraControl.cs` | `SvgViewbox` com `barra.svg`, overlays e handles superior/inferior. |
| `CaboControl` | `Controls/CaboControl.cs` | `Canvas`, `Polyline`, hit area transparente e handles de vértices. |
| `CargaControl` | `Controls/CargaControl.cs` | `SvgViewbox` com `carga.svg`. |
| `GeradorControl` | `Controls/GeradorControl.cs` | `SvgViewbox` com `gerador.svg`. |
| `SinControl` | `Controls/SinControl.cs` | `SvgViewbox` com `sin.svg`. |
| `TransformadorControl` | `Controls/TransformadorControl.cs` | `SvgViewbox` com `transformador.svg`. |

`Controls/Base/ElementoControlBase.cs` aplica rotação visual via binding em `ElementoViewModel.Rotacao`, gerencia hover e atualizações visuais.

## 7.3 Ribbon

Arquivos principais:

- `Ribbon/RibbonView.xaml`
- `Ribbon/RibbonView.xaml.cs`
- `Ribbon/Tabs/DiagramaTab.xaml.cs`
- `Ribbon/Tabs/EditarTab.xaml.cs`
- `Ribbon/Tabs/AnaliseTab.xaml.cs`
- `Ribbon/Tabs/ArquivoMenuView.xaml.cs`
- `Ribbon/Tabs/ArquivoTab.xaml.cs`
- `Ribbon/Tabs/GerenciarTab.xaml.cs`

Responsabilidades reais:

- `RibbonView` alterna visibilidade de abas e abre o menu Arquivo.
- `DiagramaTab` ativa inserção de elementos por `kind` e atualiza botões conforme ferramenta atual.
- `EditarTab` ativa selecionar, mover, alinhar, deletar, copiar, colar, desfazer e refazer.
- `AnaliseTab` abre `FluxoDeCorrenteWindow` e executa `FluxoDeCorrenteApplication`.
- `ArquivoMenuView` chama `Context.Projects.Novo`, `AbrirComDialogo` e `SalvarComDialogo`.

## 7.4 ViewModels

Classes principais:

- `ViewportViewModel`
- `ElementoViewModel`
- `BarraViewModel`
- `CaboViewModel`
- `CargaViewModel`
- `GeradorViewModel`
- `SinViewModel`
- `TransformadorViewModel`
- `TipoElementoViewModel` e derivados
- `PropertiesViewModel`
- `SelectionBoxViewModel`
- `TerminalSnapState`
- `MoveHudService` atua como view model de HUD, embora esteja em `Service`

`ElementoViewModel` encapsula um `ElementoNode`, expõe propriedades visuais, seleção, hover, preview, tipo, parâmetros, posição, bounds e render data. `CaboViewModel` especializa geometria por vértices e preview. `BarraViewModel` especializa altura e atualização de terminais.

# 8. Persistência

A persistência atual usa JSON e extensão `.araci` nos diálogos de arquivo.

## 8.1 Serviços

`Applications/Projects/ProjectPersistenceService.cs` coordena:

- novo projeto;
- salvar com diálogo;
- abrir com diálogo;
- salvar em caminho;
- abrir de caminho;
- limpar estado transitório;
- limpar histórico de comandos após novo/abrir.

Dependências reais:

- `AraciDocument`
- `ICommandHistory`
- `ProjectSerializer`
- `IProjectRepository`
- `IProjectFileDialogService`
- `IUserDialogService`

`Infrastructure/Persistence/FileSystemProjectRepository.cs` lê e escreve texto com `File.ReadAllText` e `File.WriteAllText`.

`Infrastructure/Persistence/ProjectFileDialogService.cs` usa `Microsoft.Win32.SaveFileDialog` e `OpenFileDialog`, com filtro:

```text
Projeto Araci (*.araci)|*.araci|JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*
```

## 8.2 DTOs de persistência

`Infrastructure/Persistence/ProjectFileDto.cs` define:

- `ProjectFileDto`
- `ProjectMetadataDto`
- `ElementDto`
- `TypeRefDto`
- `ParameterDto`
- `TerminalDto`
- `PointDto`

Campos de `ElementDto`:

- `Kind`
- `DomainRole`
- `Id`
- `X`
- `Y`
- `Rotation`
- `Scale`
- `Type`
- `Parameters`
- `Terminals`
- `Vertices`

## 8.3 Serialização

`Infrastructure/Persistence/ProjectSerializer.cs` define:

- `CurrentVersion = 1`
- `AppName = "Araci Engine"`
- `UntitledProjectName = "Sem titulo"`

Responsabilidades reais:

- Converter `AraciDocument` em `ProjectFileDto`.
- Converter `ProjectFileDto` em lista de `Elemento`.
- Serializar com `System.Text.Json`.
- Desserializar com `System.Text.Json`.
- Preparar metadados para salvar.
- Criar metadados a partir de arquivo.
- Resolver tipos por `ElementRegistryService`.
- Restaurar parâmetros, vértices e terminais.

Fluxo de persistência:

```text
Salvar:
AraciDocument
  -> ProjectSerializer.CreateFileDto
  -> ProjectSerializer.Serialize
  -> FileSystemProjectRepository.WriteAllText
  -> arquivo .araci/.json

Abrir:
arquivo .araci/.json
  -> FileSystemProjectRepository.ReadAllText
  -> ProjectSerializer.Deserialize
  -> ProjectSerializer.CreateElements
  -> AraciDocument.Limpar + AdicionarElemento
  -> limpar estado transitorio + CommandManager.Clear
```

# 9. Simulação

## 9.1 ParameterReader

`DTOs/ParameterReader.cs`, namespace `Araci.DTOs`, lê dados do documento. Possui construtores para:

- `CoreApi`
- `EditorContext`
- `AraciDocument`

Métodos públicos relevantes:

- `GetLoads`
- `GetLines`
- `GetTransformers`
- `GetGenerators`
- `GetSins`

Classes internas de dados:

- `LoadData`
- `LineData`
- `TransformerData`
- `GeneratorData`
- `ExternalSourceData`

O leitor usa `CoreApi`, `ConnectivityService`, `TopologyValidator` e `ElectricGraphBuilder` conforme disponível. Ele resolve barramentos por grafo, por endpoints e por serviços de conectividade.

## 9.2 CircuitBuilder

`DTOs/CircuitBuilder.cs` recebe `ParameterReader` e constrói `CircuitDto`.

Responsabilidades reais:

- Validar topologia via `ValidateTopology`.
- Escolher slack preferencialmente por `Sin`; se não houver, usa primeiro `Gerador`.
- Construir listas de `LoadDto`, `LineDto`, `TransformerDto`, `GeneratorDto`.
- Aplicar defaults quando valores não existem ou são inválidos.
- Validar slack e cabos sem barra origem/destino.

## 9.3 Pipeline

`Applications/Simulation/CircuitDtoBuilder.cs` cria `ParameterReader` a partir de `AraciDocument` e usa `DTOs/CircuitBuilder`.

`Applications/Simulation/SimulationPipeline.cs` executa:

```text
CircuitDtoBuilder.Build()
  -> ISimulationGateway.SimularAsync(dto)
  -> ISimulationResultApplier.Apply(resultado)
  -> retorna SimulationResultDto
```

## 9.4 FastAPI e OpenDSS

`Infrastructure/Simulation/FastApiOpenDssGateway.cs` implementa `ISimulationGateway` e delega para `DTOs/SimulationApiClient`.

`DTOs/SimulationApiClient.cs` define:

- URL padrão `http://127.0.0.1:8000/simular`.
- Timeout de 30 segundos.
- Serialização JSON com `JsonNamingPolicy.SnakeCaseLower`.
- POST HTTP para a API.
- Desserialização flexível de resposta.
- Escrita de script em `C:\Temp\araci_script_debug.txt`.

O código trata a API como endpoint FastAPI/OpenDSS, mas o serviço Python/FastAPI não aparece como projeto dentro da solução mapeada. A integração existente no repositório é o cliente HTTP e gateway.

## 9.5 Aplicação de resultados

`Service/SimulationResultApplier.cs` aplica resultados:

- `LineResultDto` atualiza `Cabo.CorrenteLinha`, `CorrenteFaseA`, `CorrenteFaseB`, `CorrenteFaseC`.
- `LoadResultDto` atualiza `Carga.CorrenteLinha`, `CorrenteFaseA`, `CorrenteFaseB`, `CorrenteFaseC`.
- O formato usado é polar, com símbolo de ângulo e grau.

`Service/SimulationExportService.cs` exporta:

- script `.dss`;
- JSON de resultado.

`Service/SimulationMessageBuilder.cs` monta mensagem de sucesso/falha, avisos, caminho DSS e script gerado.

# 10. Scene Graph e Rendering

## 10.1 Scene

`Core/Scenes/Scene.cs` define `Scene`, com `ObservableCollection<ElementoViewModel> Elementos`. Essa cena é a coleção visual usada pelo viewport.

`Applications/Scene/DocumentSceneSyncService.cs` sincroniza `AraciDocument.Elementos` com `Scene.Elementos`, criando ou removendo ViewModels conforme mudanças no documento.

## 10.2 Scene Nodes

Classes reais em `Core/SceneNodes`:

| Classe | Responsabilidade |
| --- | --- |
| `ElementoNode` | Base de nó visual; mantém `Modelo`, `Bounds`, posição, centro e movimentação. |
| `BarraNode` | Calcula bounds de barra considerando largura padrão e altura da barra. |
| `CaboNode` | Calcula bounds por vértices e preview de cabo; move preservando âncoras. |
| `EquipamentoNode` | Calcula bounds retangulares para equipamentos. |

## 10.3 Rendering

Arquivos em `Core/Rendering`:

- `ElementGeometryDefaults.cs`
- `ElementoRenderData.cs`

`ElementGeometryDefaults` contém dimensões padrão de elementos. `ElementoRenderData` encapsula largura, altura, pontos locais e stroke para uso visual.

## 10.4 Queries e índice espacial

`Core/SceneQueries/SceneQueryService.cs` implementa `ISceneQueryService`. Ele:

- mantém um `ISpatialIndex`;
- usa `SpatialHashGrid`;
- invalida índice quando propriedades visuais mudam;
- executa `HitTest`, `Query` por retângulo e `Nearby`;
- trata cabos por distância ponto-segmento;
- considera rotação em bounds.

`Core/Spatial/SpatialHashGrid.cs` implementa índice espacial para acelerar consultas sobre `ElementoViewModel`.

## 10.5 Viewport e câmera

`Core/Viewport/Camera.cs` mantém zoom e offset com `INotifyPropertyChanged`.

`Service/ViewportService.cs` e `Service/ViewportNavigationService.cs` coordenam tamanho de viewport, conversão tela/mundo, pan e zoom. `ViewportView.xaml.cs` aplica a câmera por `MatrixTransform` nas camadas principais.

# 11. Fluxos Principais

## 11.1 Inserção de elemento

```text
Ribbon DiagramaTab ou atalho
  -> ToolService.AtivarInsercaoElemento(kind)
  -> InserirElementoGenericoTool
  -> InsertPreviewController atualiza preview
  -> clique no viewport
  -> InserirElementoUseCase.Executar(kind, x, y, rotacao)
  -> ElementoFactory.CriarModelo
  -> TerminalLayoutService.AtualizarTerminais
  -> CommandManager.Execute(AddElementoCommand)
  -> AraciDocument.AdicionarElemento
  -> DocumentSceneSyncService cria ViewModel na Scene
```

Arquivos envolvidos:

- `Ribbon/Tabs/DiagramaTab.xaml.cs`
- `Applications/Editor/ToolService.cs`
- `Applications/Diagrama/InserirElemento/InserirElementoGenerico.cs`
- `Applications/UseCases/Diagrama/InserirElementoUseCase.cs`
- `Applications/Factories/ElementoFactory.cs`
- `Applications/Commands/AddElementoCommand.cs`
- `Core/Documents/AraciDocument.cs`
- `Applications/Scene/DocumentSceneSyncService.cs`

## 11.2 Seleção

```text
ViewportView recebe mouse down
  -> InputRouter.MouseDown
  -> SelecionarTool.OnMouseDown
  -> SceneQueryService.HitTest se necessário
  -> SelectionController.Select
  -> SelectionService.Selecionar/Toggle/Limpar
  -> EditorState.ElementoSelecionado
  -> SelecaoAlteradaEvent
```

Arquivos envolvidos:

- `Views/ViewportView.xaml.cs`
- `Applications/Editor/InputRouter.cs`
- `Applications/Editar/Selecionar/SelecionarTool.cs`
- `Applications/Editar/Selecionar/SelectionController.cs`
- `Service/SelectionService.cs`
- `Core/SceneQueries/SceneQueryService.cs`

## 11.3 Movimentação

```text
SelecionarTool detecta elemento selecionado
  -> DragMoveController.Begin
  -> MoveService.BeginMove captura estados
  -> MouseMove
  -> MoveConstraintService + AlignmentGuideService
  -> MoveService.MoverVisual
  -> ElementoViewModel.Mover
  -> ConnectivityService.ReancorarCabosConectados
  -> MouseUp
  -> MoveService.EndMove
  -> MoverElementoUseCase.Executar
  -> TransactionScope
  -> MoveElementoCommand
```

Arquivos envolvidos:

- `Applications/Editar/Selecionar/DragMoveController.cs`
- `Service/MoveService.cs`
- `Applications/UseCases/Editar/MoverElementoUseCase.cs`
- `Core/Commands/MoveElementoCommand.cs`
- `Service/ConnectivityService.cs`

## 11.4 Rotação

```text
Tecla Space em seleção
  -> InputRouter.KeyDown
  -> SelecionarTool.OnKeyDown
  -> RotationService.RotateSelectionClockwise
  -> coleta elementos e cabos afetados
  -> atualiza Modelo.Rotacao
  -> VisualUpdateService.AtualizarElementoRotacionado
  -> RotacionarElementoUseCase.Executar
  -> RotateElementoCommand
```

Arquivos envolvidos:

- `Applications/Editor/InputRouter.cs`
- `Applications/Editar/Selecionar/SelecionarTool.cs`
- `Service/RotationService.cs`
- `Applications/UseCases/Editar/RotacionarElementoUseCase.cs`
- `Core/Commands/RotateElementoCommand.cs`

## 11.5 Inserção de cabo

```text
ToolService ativa InserirCaboTool
  -> mouse sobre terminal mostra preview inicial
  -> clique origem valida terminal
  -> InserirCaboUseCase.Iniciar
  -> AddElementoCommand adiciona Cabo
  -> ConectarOrigem
  -> mouse move atualiza preview e snap
  -> cliques sem terminal adicionam vértices intermediários
  -> clique em terminal destino valida conexão
  -> InserirCaboUseCase.FinalizarDestino
  -> ConectarDestino
  -> Finalizar e voltar para seleção
```

Arquivos envolvidos:

- `Applications/Diagrama/InserirCabo/InserirCabo.cs`
- `Applications/UseCases/Diagrama/InserirCaboUseCase.cs`
- `Service/SnapService.cs`
- `Service/ConnectivityService.cs`
- `ViewModels/CaboViewModel.cs`
- `Models/Cabo.cs`

## 11.6 Persistência

```text
ArquivoMenuView
  -> ProjectPersistenceService.SalvarComDialogo/AbrirComDialogo
  -> ProjectFileDialogService
  -> ProjectSerializer
  -> FileSystemProjectRepository
```

No abrir, o documento atual é limpo e os elementos desserializados são adicionados. Em seguida, estado transitório e histórico de comandos são limpos.

## 11.7 Simulação

```text
AnaliseTab.FluxoButton_Click
  -> FluxoDeCorrenteWindow
  -> FluxoDeCorrenteApplication
  -> SimulationPipeline.ExecutarFluxoDeCorrenteAsync
  -> CircuitDtoBuilder.Build
  -> ParameterReader
  -> DTOs.CircuitBuilder
  -> FastApiOpenDssGateway
  -> SimulationApiClient POST /simular
  -> SimulationResultApplier.Apply
  -> SimulationExportService opcional
  -> SimulationMessageBuilder
  -> DialogService
```

# 12. Dependências

## 12.1 Dependências externas

| Dependência | Origem | Uso observado |
| --- | --- | --- |
| `.NET 8 Windows` | `Araci.csproj` | Plataforma da aplicação. |
| `WPF` | `UseWPF=true` | UI desktop. |
| `WindowsForms` | `UseWindowsForms=true` | Habilitado no projeto; uso direto não foi identificado nos arquivos lidos. |
| `SharpVectors.Wpf 1.8.5` | `Araci.csproj` | Renderização de SVG via `SvgViewbox`. |
| `System.Text.Json` | Persistência e simulação | Serialização de `.araci`, DTOs e resposta da API. |
| `System.Net.Http.HttpClient` | `DTOs/SimulationApiClient.cs` | Chamada HTTP para API de simulação. |
| `Microsoft.Win32` | `ProjectFileDialogService.cs` | Diálogos de abrir/salvar. |

## 12.2 Matriz interna

| Módulo | Depende de | Consumido por |
| --- | --- | --- |
| `Models` | WPF `Point` em terminais/geometria | Core, Services, Applications, DTOs, ViewModels, Persistence |
| `Core.Documents` | `Models` | Services, Applications, Persistence, Simulation |
| `Core.SceneNodes` | `Models`, `Core.Rendering` | ViewModels |
| `Core.SceneQueries` | `Core.Scenes`, `Core.Spatial`, `ViewModels` | Tools, Services, Viewport |
| `Applications.UseCases` | `Core.Commands`, `Models`, `Services` | Tools, EditorContext |
| `Applications.Editor` | Tools, abstrações, serviços | Viewport e Ribbon via EditorContext |
| `Service` | Models, Core, Applications, ViewModels | EditorContext, DTOs, UI |
| `Infrastructure.Persistence` | Models, Services, Applications abstractions | ProjectPersistenceService |
| `Infrastructure.Simulation` | DTOs, Applications abstractions | SimulationComposition |
| `DTOs` | Models, Services, API, HttpClient | SimulationPipeline, FastApiOpenDssGateway |
| `ViewModels` | Models, Services, SceneNodes | Views, Controls, Scene |
| `Controls` | ViewModels, SharpVectors | ViewportView |
| `Ribbon` | EditorContext, Applications | MainWindow |

# 13. Acoplamentos

Acoplamentos importantes observados:

1. `EditorContext` é um hub de composição amplo. Ele conhece documento, cena, serviços, use cases, factories, simulação, persistência, tools e viewport.

2. `DTOs/ParameterReader.cs` depende de `Araci.API`, `Araci.Core.Documents`, `Araci.Models` e `Araci.Services`. A camada de DTOs contém lógica relevante de domínio e topologia, não apenas contratos de transporte.

3. `Models` usa tipos WPF como `System.Windows.Point`, especialmente em `Terminal`, `Cabo`, `Barra`, `Carga`, `Gerador`, `Sin` e `Transformador`. Isso aproxima o domínio de detalhes de apresentação/geometria WPF.

4. `Service` depende de `ViewModels` em vários pontos, como `SelectionService`, `MoveService`, `RotationService`, `BarraResizeService`, `VisualUpdateService` e outros. Isso cria acoplamento entre serviços de aplicação e camada de apresentação.

5. `Applications/Commands` contém arquivos com namespace `Araci.Core.Commands` e `Araci.Core.Transactions`, indicando desalinhamento entre estrutura física e namespace.

6. `SimulationApiClient` escreve sempre em `C:\Temp\araci_script_debug.txt`, criando dependência fixa de caminho local fora do projeto.

7. `ProjectSerializer` depende de `ElementRegistryService`, `IElementModelFactory`, `TerminalLayoutService` e `ElementGeometryService`. A persistência reconstrói elementos usando catálogo, factory e geometria.

8. `Ribbon` chama diretamente métodos do `EditorContext`, como `Context.Tools`, `Context.Commands`, `Context.Clipboard`, `Context.Projects` e `Context.Simulation`.

9. `ViewportView.xaml.cs` conhece `EditorContext`, `ViewportService`, `InputRouter`, `Navigation`, `Hover`, `TerminalSnap`, `AlignmentGuides` e `BarraResize`.

# 14. Dívidas Técnicas

As dívidas abaixo são derivadas de observações no código atual.

| Dívida | Evidência |
| --- | --- |
| Composição centralizada demais | `Service/EditorContext.cs` instancia e conecta a maior parte dos componentes. |
| Desalinhamento entre pasta e namespace | Arquivos em `Applications/Commands` declaram `Araci.Core.Commands` e `Araci.Core.Transactions`. |
| Diretório vazio | `Application/Abstractions` existe sem arquivos, enquanto `Applications/Abstractions` é usado. |
| DTOs com lógica de domínio | `DTOs/ParameterReader.cs` resolve topologia, lê parâmetros, usa `ConnectivityService` e `ElectricGraphBuilder`. |
| Domínio acoplado a WPF | `Models` usa `System.Windows.Point`, `Vector` e lógica de posição visual/local. |
| Serviços acoplados a ViewModels | `MoveService`, `RotationService`, `SelectionService` e outros manipulam `ElementoViewModel`. |
| Caminho fixo de debug | `SimulationApiClient` escreve `C:\Temp\araci_script_debug.txt`. |
| URL fixa de simulação | `SimulationApiClient` usa `http://127.0.0.1:8000/simular` como padrão sem configuração externa observada. |
| Caracteres corrompidos em strings | Há textos como `ConexÃ£o invÃ¡lida` e valores como `12,47âˆ 0Â°` em arquivos de modelo/ferramentas. |
| Projeto de checks muito grande | `Araci.TechnicalChecks/Program.cs` concentra grande quantidade de verificações em um único arquivo. |
| `UseWindowsForms` habilitado sem uso claro | `Araci.csproj` habilita Windows Forms, mas o uso direto não foi identificado nos arquivos analisados. |

# 15. Comparação com Arquitetura-Alvo

A arquitetura-alvo do projeto, conforme a direção documentada para o Araci, aponta para uma plataforma CAD/BIM elétrica 2D com separação clara entre interface, modelo, persistência, comandos, simulação e evolução futura. O código atual já contém partes importantes dessa direção, mas ainda apresenta concentração de responsabilidades e acoplamentos entre camadas.

## 15.1 Pontos alinhados

| Arquitetura-alvo | Evidência no código atual |
| --- | --- |
| Modelo elétrico estruturado | `Models/Elemento.cs`, `Barra`, `Cabo`, `Carga`, `Gerador`, `Sin`, `Transformador`. |
| Documento central | `Core/Documents/AraciDocument.cs`. |
| Persistência nativa | `ProjectSerializer`, `ProjectFileDto`, `ProjectPersistenceService`, extensão `.araci` nos diálogos. |
| Undo/Redo | `CommandManager`, `IUndoableCommand`, `AddElementoCommand`, `MoveElementoCommand`, `RotateElementoCommand`, `DeleteElementCommand`. |
| Simulação OpenDSS via API | `SimulationPipeline`, `FastApiOpenDssGateway`, `SimulationApiClient`, `CircuitBuilder`, `ParameterReader`. |
| Scene graph/rendering | `Scene`, `ElementoNode`, `BarraNode`, `CaboNode`, `EquipamentoNode`, `SceneQueryService`, `SpatialHashGrid`. |
| Catálogo interno inicial | `ElementDefinitionsProvider`, `ElementRegistryService`, `TypeLibraryService`. |
| Separação física parcial | Pastas `Core`, `Models`, `Applications`, `Infrastructure`, `ViewModels`, `Views`, `Controls`. |

## 15.2 Pontos parcialmente alinhados

| Área | Estado atual |
| --- | --- |
| Camada de aplicação | Há use cases e tools, mas muitos serviços ainda dependem de ViewModels e WPF. |
| Persistência | Existe serialização estruturada, mas depende de serviços de geometria e catálogo para reconstrução. |
| Simulação | Existe pipeline, mas `DTOs` concentra lógica de leitura, defaults, topologia e transporte. |
| Composição | Há classes de composição, mas `EditorContext` ainda é o agregador dominante. |
| Domínio | O modelo elétrico existe, mas usa tipos WPF e carrega estado visual/topológico no mesmo objeto. |

## 15.3 Lacunas em relação à arquitetura-alvo

| Lacuna | Evidência |
| --- | --- |
| Separação rígida domínio/apresentação | `Models` usa `Point`; `Services` usa `ElementoViewModel`; `ViewportView` conhece `EditorContext`. |
| Configuração externa de simulação | URL e path de debug estão fixos em `SimulationApiClient`. |
| Modularização de checks | `Araci.TechnicalChecks/Program.cs` centraliza muitas verificações. |
| Organização física consistente | `Application` vazio e namespaces em pastas diferentes indicam necessidade de limpeza estrutural. |
| Camada de DTO puramente contratual | `DTOs/ParameterReader.cs` e `DTOs/CircuitBuilder.cs` contêm lógica significativa de domínio/simulação. |

## 15.4 Síntese

O código atual já implementa um núcleo funcional coerente para editor elétrico 2D: documento, elementos, terminais, cabos, grafo elétrico, seleção, movimentação, rotação, persistência, simulação e rendering. A base arquitetural existe, mas ainda está em fase de consolidação.

O principal ponto forte é a existência de conceitos explícitos do domínio elétrico no código: `AraciDocument`, `Elemento`, `Terminal`, `TerminalEndpoint`, `Cabo`, `ElectricGraph`, `TopologyValidator`, `CircuitDto` e `SimulationPipeline`. O principal risco arquitetural é a concentração de orquestração e o acoplamento entre domínio, serviços e apresentação. Para evoluir em direção à arquitetura-alvo, a solução tende a se beneficiar de maior separação entre modelo puro, estado visual, serviços de aplicação, infraestrutura e UI.
