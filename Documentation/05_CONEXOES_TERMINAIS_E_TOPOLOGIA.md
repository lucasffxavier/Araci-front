# 1. Introdução

Este documento descreve a camada topológica do Araci conforme implementada no código atual. O foco é mapear terminais, endpoints, conectividade, inserção de cabos, construção do grafo elétrico, validação topológica e estado operacional de energização.

A camada topológica do Araci tem como objetivo transformar elementos elétricos armazenados em `AraciDocument` em uma rede interpretável por serviços elétricos. Essa rede é construída a partir de elementos que participam do grafo elétrico, terminais pertencentes a esses elementos e cabos que conectam endpoints por identificadores.

O documento é derivado exclusivamente dos arquivos reais existentes no projeto. As regras descritas correspondem a validações, algoritmos e fluxos observados em classes como `Terminal`, `TerminalEndpoint`, `ConnectivityService`, `InserirCaboUseCase`, `InserirCaboTool`, `ElectricGraphBuilder`, `TopologyValidator` e `OperationalGraphStateBuilder`.

A visão geral pode ser resumida assim:

```text
AraciDocument.Elementos
    |
    +-- Barra, Carga, Gerador, Sin, Transformador
    |       |
    |       +-- ITerminalOwner.Terminais
    |
    +-- Cabo
            |
            +-- OrigemEndpoint
            +-- DestinoEndpoint

Document -> ConnectivityService -> ElectricGraphBuilder -> TopologyValidator
                                            |
                                            v
                                  OperationalGraphStateBuilder
```

# 2. Conceitos Fundamentais

A topologia elétrica no código é organizada em torno de cinco conceitos principais: `Terminal`, `TerminalEndpoint`, `ITerminalOwner`, `ElementoDomainRole` e conectividade por cabos.

## Terminal

`Terminal`, em `Models/Terminal.cs`, representa um ponto de conexão pertencente a um `Elemento`. Ele armazena a posição global, posição local, identificador, tipo, direção e um rótulo opcional de barra.

Cada terminal conhece seu proprietário por meio da propriedade `Dono`. Isso permite formar endpoints estáveis com `Dono.Id` e `Terminal.Id`.

## TerminalEndpoint

`TerminalEndpoint`, em `Models/TerminalEndpoint.cs`, é uma struct imutável que identifica um terminal por valor:

```text
TerminalEndpoint
    ElementId
    TerminalId
```

Ele é usado para comparar conexões de cabos, verificar duplicidade e localizar terminais no grafo.

## ITerminalOwner

`ITerminalOwner`, em `Models/ITerminalOwner.cs`, define:

```csharp
IReadOnlyList<Terminal> Terminais { get; }
```

Qualquer elemento que implementa essa interface pode expor terminais para snap, conexão, grafo elétrico e validação topológica.

## DomainRole

O papel de domínio está definido pelo enum `ElementoDomainRole`, em `Models/ElementoDomainRole.cs`, com valores:

| Valor | Significado no código |
|---|---|
| `Grafico` | Papel padrão de `Elemento`. |
| `Anotacao` | Papel previsto para elementos anotativos. |
| `EletricoTopologico` | Elemento participa da topologia elétrica. |

Em `Models/Elemento.cs`, a propriedade `ParticipaDoGrafoEletrico` retorna `true` quando `DomainRole == ElementoDomainRole.EletricoTopologico`.

## Conectividade

A conectividade é derivada dos campos do `Cabo`, em `Models/Cabo.cs`:

| Campo/propriedade | Papel |
|---|---|
| `OrigemId` | Id do elemento de origem. |
| `OrigemTerminalId` | Id do terminal de origem. |
| `DestinoId` | Id do elemento de destino. |
| `DestinoTerminalId` | Id do terminal de destino. |
| `OrigemEndpoint` | `TerminalEndpoint` formado por origem. |
| `DestinoEndpoint` | `TerminalEndpoint` formado por destino. |

```text
Elemento A                          Elemento B
  Terminal A1                         Terminal B1
      |                                   |
      +----------- Cabo -----------------+
             OrigemEndpoint
             DestinoEndpoint
```

# 3. Sistema de Terminais

O sistema de terminais é composto por `Terminal`, `TerminalPlacement`, `TerminalKind`, `TerminalDirection` e `TerminalEndpoint`.

## Terminal

Arquivo: `Models/Terminal.cs`

Propriedades reais:

| Propriedade | Tipo | Uso |
|---|---|---|
| `Dono` | `Elemento` | Elemento proprietário do terminal. |
| `Id` | `string` | Identificador do terminal no elemento. |
| `Posicao` | `Point` | Coordenada global no mundo do diagrama. |
| `PosicaoLocal` | `Point` | Coordenada local relativa ao proprietário. |
| `Barra` | `string?` | Rótulo de barra associado ao terminal. |
| `Kind` | `TerminalKind` | Tipo do terminal. |
| `Direction` | `TerminalDirection` | Direção nominal do terminal. |
| `Endpoint` | `TerminalEndpoint` | Endpoint derivado por `TerminalEndpoint.FromTerminal(this)`. |

O construtor recebe `Elemento dono`, `Point posicao`, `string? id`, `TerminalKind kind` e `TerminalDirection direction`. Quando `id` é nulo ou vazio, o código gera um GUID sem separadores com `Guid.NewGuid().ToString("N")`.

## TerminalKind

Arquivo: `Models/TerminalKind.cs`

Valores reais:

| Valor | Uso observado |
|---|---|
| `Electrical` | Terminais elétricos de barras e equipamentos. |
| `CableEnd` | Terminais internos das pontas de cabos. |

`SnapService.PrioridadeTerminal`, em `Service/SnapService.cs`, prioriza `TerminalKind.Electrical` sobre `CableEnd` quando dois terminais estão à mesma distância.

## TerminalDirection

Arquivo: `Models/TerminalDirection.cs`

Valores reais:

```text
None
North
South
East
West
Internal
```

As direções são atribuídas por elementos específicos. Exemplos: `Gerador` cria terminais `TOPO`, `BASE`, `ESQUERDA` e `DIREITA` com direções `North`, `South`, `West` e `East`; `Cabo` cria `ORIGEM` com `West` e `DESTINO` com `East`.

## TerminalPlacement

Arquivo: `Models/TerminalPlacement.cs`

`TerminalPlacement` converte coordenadas entre espaço local e espaço global. Ele considera posição, escala, rotação e, quando informado, um tamanho com pivô no centro.

Métodos reais:

| Método | Função |
|---|---|
| `ToWorld(Elemento owner, Point local)` | Converte coordenada local para global sem tamanho. |
| `ToWorld(Elemento owner, Point local, double width, double height)` | Converte usando tamanho. |
| `ToWorld(Elemento owner, Point local, Size size)` | Implementação principal. |
| `ToLocal(Elemento owner, Point world)` | Converte coordenada global para local sem tamanho. |
| `ToLocal(Elemento owner, Point world, double width, double height)` | Converte usando tamanho. |
| `ToLocal(Elemento owner, Point world, Size size)` | Implementação principal. |

O algoritmo de `ToWorld`:

```text
local
  |
  v
subtrai pivô
  |
  v
aplica escala do owner
  |
  v
aplica rotação do owner, se diferente de zero
  |
  v
soma PosicaoX/PosicaoY e pivô
  |
  v
world
```

O pivô é obtido por `ObterPivo(Size size)`. Se o tamanho estiver vazio ou inválido, o pivô é `(0,0)`. Caso contrário, o pivô é `(width / 2, height / 2)`.

`ToLocal` aplica o caminho inverso: remove posição e pivô, aplica rotação negativa, divide por escala e soma o pivô.

## Atualização de posição

`Terminal` expõe quatro métodos para atualizar posição:

| Método | Comportamento |
|---|---|
| `DefinirPosicaoLocal(Point local)` | Define `PosicaoLocal` e recalcula `Posicao` por `TerminalPlacement.ToWorld`. |
| `DefinirPosicaoLocal(Point local, double width, double height)` | Igual, mas com tamanho do elemento. |
| `DefinirPosicaoVisual(Point world)` | Define `Posicao` e recalcula `PosicaoLocal` por `TerminalPlacement.ToLocal`. |
| `DefinirPosicaoVisual(Point world, double width, double height)` | Igual, mas com tamanho do elemento. |

Essa distinção aparece em dois tipos de fluxo:

```text
Layout do elemento:
    coordenada local conhecida -> DefinirPosicaoLocal -> coordenada global

Movimento visual ou cabo:
    coordenada global conhecida -> DefinirPosicaoVisual -> coordenada local
```

# 4. Proprietários de Terminais

Os proprietários de terminais reais são classes que implementam `ITerminalOwner`: `Barra`, `Cabo` e `ElementoEquipamento`. `Carga`, `Gerador`, `Sin` e `Transformador` herdam de `ElementoEquipamento`.

## Barra

Arquivo: `Models/Barra.cs`

`Barra` possui `DomainRole => ElementoDomainRole.EletricoTopologico` e expõe `Terminais` a partir de uma lista interna.

Constantes relevantes:

| Constante | Valor |
|---|---:|
| `ALTURA_PADRAO` | 120 |
| `ALTURA_MINIMA` | 40 |
| `TERMINAIS_PADRAO` | 24 |
| `ESPACAMENTO_TERMINAIS` | `ALTURA_PADRAO / (TERMINAIS_PADRAO - 1)` |

`AtualizarTerminais(double largura, IReadOnlySet<string>? terminaisProtegidos)` calcula a quantidade por `CalcularQuantidadeTerminais(Altura)`, ajusta a lista e posiciona cada terminal na linha vertical da barra.

Terminais criados:

```text
BARRA-01
BARRA-02
...
BARRA-N
```

Todos os terminais criados por `Barra` usam `TerminalKind.Electrical` e `TerminalDirection.East`.

Casos especiais reais:

| Situação | Comportamento |
|---|---|
| Altura menor que mínima, NaN ou infinita | `NormalizarAltura` retorna `ALTURA_MINIMA`. |
| Barra aumenta | Novos slots são criados. |
| Barra reduz | Terminais fora da quantidade nova são removidos, exceto terminais protegidos. |
| Terminal protegido | `AjustarQuantidadeTerminais` preserva ids presentes em `terminaisProtegidos`. |

`ElementGeometryUpdateService.AtualizarElementoECabos`, em `Service/ElementGeometryUpdateService.cs`, usa `ConnectivityService.ObterTerminalIdsOcupados(barra)` para proteger terminais ocupados durante atualização de barra.

## Cabo

Arquivo: `Models/Cabo.cs`

`Cabo` herda de `ElementoLinear`, implementa `ITerminalOwner` e também possui `DomainRole => EletricoTopologico`. Na formação do grafo, ele não vira nó; vira aresta.

Terminais do cabo:

| Id | Kind | Direction | Criado por |
|---|---|---|---|
| `ORIGEM` | `CableEnd` | `West` | `DefinirOrigem(Point p)` |
| `DESTINO` | `CableEnd` | `East` | `DefinirDestino(Point p)` |

`DefinirOrigem` e `DefinirDestino` criam ou atualizam os terminais internos do cabo. Eles também atualizam `Terminal.Barra` com `BarraOrigem` ou `BarraDestino`.

O cabo mantém `Vertices`, `PreviewPonto`, endpoints e propriedades de conexão. `AtualizarTerminaisPelasPontas()` usa o primeiro vértice como origem e o último como destino.

## ElementoEquipamento

Arquivo: `Models/ElementoEquipamento.cs`

`ElementoEquipamento` implementa `ITerminalOwner` e é a base para `Carga`, `Gerador`, `Sin` e `Transformador`. Ele cria inicialmente um terminal `PRINCIPAL`, com `TerminalKind.Electrical` e `TerminalDirection.None`, no método privado `CriarTerminalInicial()`.

A propriedade `Barra` atualiza o parâmetro `Barra` e propaga o valor para todos os terminais internos.

## Carga

Arquivo: `Models/Carga.cs`

`Carga.AtualizarTerminais(double largura, double altura)` usa o primeiro terminal existente, define:

| Terminal | Local | Direction |
|---|---|---|
| `PRINCIPAL` herdado | `(largura / 2, 0)` | `North` |

O terminal recebe `Barra = Carga.Barra`.

## Gerador

Arquivo: `Models/Gerador.cs`

`Gerador.AtualizarTerminais(double largura, double altura)` garante quatro terminais. Se houver menos de quatro, limpa a lista e recria:

| Índice | Id | Local | Direction |
|---:|---|---|---|
| 0 | `TOPO` | `(largura / 2, 0)` | `North` |
| 1 | `BASE` | `(largura / 2, altura)` | `South` |
| 2 | `ESQUERDA` | `(0, altura / 2)` | `West` |
| 3 | `DIREITA` | `(largura, altura / 2)` | `East` |

Todos são `TerminalKind.Electrical`.

## Sin

Arquivo: `Models/Sin.cs`

`Sin` define constantes:

```text
NORTE
SUL
LESTE
OESTE
```

`AtualizarTerminais(double largura, double altura)` verifica se os quatro terminais padrão estão presentes na ordem esperada. Caso contrário, limpa e recria os terminais. As posições locais são:

| Id | Local | Direction |
|---|---|---|
| `NORTE` | `(largura / 2, 0)` | `North` |
| `SUL` | `(largura / 2, altura)` | `South` |
| `LESTE` | `(largura, altura / 2)` | `East` |
| `OESTE` | `(0, altura / 2)` | `West` |

## Transformador

Arquivo: `Models/Transformador.cs`

`Transformador` define:

```text
PRIMARIO
SECUNDARIO
```

`AtualizarTerminais(double largura, double altura)` recria os terminais quando o par padrão não está presente. As posições são:

| Id | Local | Direction |
|---|---|---|
| `PRIMARIO` | `(largura / 2, 0)` | `North` |
| `SECUNDARIO` | `(largura / 2, altura)` | `South` |

# 5. Sistema de Conectividade

O sistema de conectividade é implementado principalmente por `ConnectivityService`, em `Service/ConnectivityService.cs`, e `ConnectionValidationResult`, em `Service/ConnectionValidationResult.cs`.

## ConnectivityService

`ConnectivityService` recebe `AraciDocument` no construtor e opera sobre `_document.Elementos`.

Métodos de busca:

| Método | Responsabilidade |
|---|---|
| `ObterElementoPorId(string id)` | Busca elemento por `Elemento.Id.ToString()` ignorando maiúsculas/minúsculas. |
| `ObterTerminal(TerminalEndpoint endpoint)` | Resolve elemento e terminal a partir de endpoint completo. |
| `ObterCabosConectados(Elemento elemento)` | Retorna cabos cujo `OrigemId` ou `DestinoId` é o id do elemento. |
| `ObterCabosConectados(TerminalEndpoint endpoint)` | Retorna cabos conectados exatamente ao endpoint. |
| `ObterTerminalIdsOcupados(Elemento elemento)` | Retorna ids de terminais do elemento usados por cabos conectados. |

Métodos de barramento:

| Método | Regra real |
|---|---|
| `ResolverBusName(Elemento elemento)` | Usa `Nome.Trim()`; se vazio, gera `BUS-` com prefixo do Guid. |
| `ResolverBusNamePorId(string id)` | Busca elemento e resolve nome; se não achar, retorna vazio. |
| `ResolverBusNamePorIdEstrito(string id, string contexto)` | Lança exceção se id vazio ou inexistente. |
| `ResolverBusNameParaEquipamento(ElementoEquipamento equipamento)` | Tenta `BarraId`; depois `Barra`; depois nome do equipamento. |
| `ResolverBusNameParaEquipamentoEstrito(ElementoEquipamento equipamento)` | Se `BarraId` existir, exige que resolva; caso contrário usa nome do equipamento. |
| `ResolverBus1/ResolverBus2` | Resolve origem/destino de cabo por ids ou fallback textual `BarraOrigem`/`BarraDestino`. |
| `ResolverBus1Estrito/ResolverBus2Estrito` | Exige ids válidos para origem/destino. |

## Reancoragem de cabos

`ReancorarCabosConectados(Elemento elemento)` percorre cabos conectados ao elemento e chama:

| Método privado | Ação |
|---|---|
| `ReancorarOrigem(Cabo cabo, Elemento elemento)` | Localiza terminal de origem e atualiza `Vertices[0]` e `DefinirOrigem`. |
| `ReancorarDestino(Cabo cabo, Elemento elemento)` | Localiza terminal de destino e atualiza último vértice e `DefinirDestino`. |

Essa reancoragem é usada por `MoveService`, `RotationService`, `VisualUpdateService` e `ElementGeometryUpdateService`.

## ConnectionValidationResult

`ConnectionValidationResult` contém:

| Propriedade | Tipo |
|---|---|
| `IsValid` | `bool` |
| `Message` | `string?` |

Métodos estáticos:

```text
ConnectionValidationResult.Valid()
ConnectionValidationResult.Invalid(string message)
```

# 6. Inserção de Cabos

A inserção de cabos é dividida entre caso de uso e ferramenta interativa:

| Classe | Arquivo | Papel |
|---|---|---|
| `InserirCaboUseCase` | `Applications/UseCases/Diagrama/InserirCaboUseCase.cs` | Cria cabo, registra no documento e aplica origem/destino. |
| `InserirCaboTool` | `Applications/Diagrama/InserirCabo/InserirCabo.cs` | Controla interação por mouse, preview, snap, validação e finalização. |

## InserirCaboUseCase

Métodos reais:

| Método | Comportamento |
|---|---|
| `Iniciar(Point pontoOrigem, Terminal terminalOrigem)` | Cria `CaboViewModel`, inicia geometria, executa `AddElementoCommand`, obtém VM da cena e conecta origem. |
| `ConectarOrigem(CaboViewModel cabo, Terminal terminal)` | Define `OrigemId`, `OrigemTerminalId`, `BarraOrigem`, chama `DefinirOrigem` e notifica parâmetros. |
| `FinalizarDestino(CaboViewModel cabo, Terminal terminal, Point pontoDestino)` | Finaliza ponto e conecta destino. |
| `ConectarDestino(CaboViewModel cabo, Terminal terminal)` | Define `DestinoId`, `DestinoTerminalId`, `BarraDestino`, chama `DefinirDestino` e notifica parâmetros. |

O rótulo de barra é obtido por `ObterRotuloTerminal`: se `terminal.Barra` estiver vazio, usa `terminal.Dono.Nome`; caso contrário, usa `terminal.Barra`.

## InserirCaboTool

`InserirCaboTool` controla o fluxo real de interação:

```text
MouseMove sem inserção
    |
    v
AtualizarPreviewInicial(position)
    |
    +-- terminal válido -> mostra snap e preview inicial
    +-- terminal inválido -> mostra mensagem inválida
    +-- sem terminal -> limpa preview

MouseDown inicial
    |
    +-- exige terminal
    +-- ValidarOrigem
    +-- InserirCaboUseCase.Iniciar
    +-- entra em modo _inserindo

MouseMove durante inserção
    |
    +-- procura terminal de destino
    +-- valida destino
    +-- atualiza preview do cabo

MouseDown durante inserção
    |
    +-- sem terminal -> adiciona vértice intermediário
    +-- com terminal válido -> FinalizarDestino
```

`EhElementoConectavel` permite conexão apenas quando o dono do terminal é `ElementoEquipamento` ou `Barra`. Assim, a ferramenta não trata terminais de cabo como destino de novas conexões.

Durante preview, `ObterPreviewInicial()` cria um cabo VM por `_factory.CriarCaboVM()`, marca `IsPreview = true`, adiciona em `Scene.Elementos` e invalida `ISceneQueryService`.

O alinhamento durante inserção é feito por `AplicarAlinhamentoCabo`. Com `Shift`, a ferramenta aplica ortogonalização por `AplicarOrtogonalizacao`: se `|delta.X| >= |delta.Y|`, preserva `Y` da origem; caso contrário, preserva `X` da origem.

# 7. Regras de Validação

As regras reais aparecem em três lugares principais: `ConnectivityService`, `InserirCaboTool` e `ElectricGraphBuilder`. `TopologyValidator` agrega validações de documento, cabos, equipamentos e circuito.

## ConnectivityService.ValidarTerminalDisponivel

Método: `ValidarTerminalDisponivel(Cabo? caboAtual, Terminal? terminal)`

Regras:

| Condição | Resultado |
|---|---|
| `terminal == null` | Inválido: `"Conexão inválida"` |
| endpoint incompleto | Inválido: `"Conexão inválida"` |
| já existe cabo no terminal, exceto `caboAtual` | Inválido: `"Conexão inválida"` |
| nenhuma condição inválida | Válido |

## ConnectivityService.ValidarConexaoCabo

Método: `ValidarConexaoCabo(Cabo? caboAtual, Terminal? origem, Terminal? destino)`

Regras:

| Condição | Mensagem |
|---|---|
| origem ou destino nulos | `"Conexão inválida"` |
| endpoint de origem ou destino incompleto | `"Conexão inválida"` |
| origem e destino pertencem ao mesmo elemento | `"Conexão inválida"` |
| origem e destino são o mesmo endpoint | `"Conexão inválida"` |
| origem ou destino já ocupado por outro cabo | `"Conexão ocupada"` |
| já existe cabo duplicado entre os mesmos terminais | `"Conexão inválida"` |

A duplicidade usa `TerminalEndpoint.PairKey`, que ordena os dois endpoints por texto e gera uma chave independente da direção.

## InserirCaboTool.ValidarOrigem e ValidarDestino

`ValidarOrigem(Terminal terminal)` primeiro chama `OrigemValida`, que exige id de elemento e id de terminal não vazios. Depois chama `ConnectivityService.ValidarTerminalDisponivel(null, terminal)`.

`ValidarDestino(Terminal terminal)` exige `_caboAtual` e `_terminalOrigem`. Depois delega para `ConnectivityService.ValidarConexaoCabo(_caboAtual.Cabo, _terminalOrigem, terminal)`.

## ElectricGraphBuilder.CreateEdge

`CreateEdge` sempre cria uma `ElectricGraphEdge`, mesmo com erros. A diferença é `IsValid = false` e `Error` preenchido.

Regras reais:

| Validação | Mensagem produzida |
|---|---|
| `ElementId` vazio | `Cabo sem OrigemId.` ou `Cabo sem DestinoId.` |
| elemento não existe no grafo | `Cabo com OrigemId inexistente: ...` ou `Cabo com DestinoId inexistente: ...` |
| `TerminalId` vazio | `Cabo sem OrigemTerminalId.` ou `Cabo sem DestinoTerminalId.` |
| terminal não existe no nó | `Cabo com OrigemTerminalId inexistente: ...` ou `Cabo com DestinoTerminalId inexistente: ...` |
| origem e destino no mesmo elemento | `Origem e destino pertencem ao mesmo elemento.` |
| origem e destino no mesmo endpoint | `Origem e destino usam o mesmo terminal.` |
| par de terminais duplicado | `Cabo duplicado entre os mesmos terminais.` |

# 8. ElectricGraph

As classes do grafo estão em `Service/ElectricGraph*.cs`, namespace `Araci.Services`.

## ElectricGraph

Arquivo: `Service/ElectricGraph.cs`

`ElectricGraph` contém:

| Propriedade | Tipo |
|---|---|
| `Nodes` | `IReadOnlyList<ElectricGraphNode>` |
| `Edges` | `IReadOnlyList<ElectricGraphEdge>` |

Ele cria um dicionário interno `_nodesById` indexado por `ElementId`, com comparação case-insensitive.

Métodos principais:

| Método | Função |
|---|---|
| `FindNode(string elementId)` | Busca nó por id textual. |
| `FindNodeByElementId(string elementId)` | Alias de `FindNode`. |
| `FindNodeByElement(Elemento elemento)` | Busca pelo `Id` do elemento. |
| `FindTerminal(TerminalEndpoint endpoint)` | Busca terminal no nó do endpoint. |
| `GetEdgesForElement(string elementId)` | Retorna arestas ligadas ao elemento. |
| `GetEdgesForTerminal(TerminalEndpoint endpoint)` | Retorna arestas ligadas ao endpoint. |
| `GetNeighbors(string elementId)` | Retorna vizinhos por arestas válidas. |
| `GetInvalidEdges()` | Retorna arestas inválidas. |
| `GetValidEdges()` | Retorna arestas válidas. |
| `FindEdgeByCable(Cabo cabo)` | Busca aresta pelo id do cabo. |
| `BreadthFirst(string startElementId)` | Percorre nós por BFS usando arestas válidas. |

## ElectricGraphNode

Arquivo: `Service/ElectricGraphNode.cs`

Campos:

| Propriedade | Origem |
|---|---|
| `ElementId` | `Elemento.Id.ToString()` |
| `ElementGuid` | `Elemento.Id` |
| `Name` | Nome resolvido do elemento |
| `Kind` | `ElementRegistryService.GetKind(elemento)` ou `GetType().Name` |
| `SourceElement` | Elemento original |
| `Terminals` | Lista de `ElectricGraphTerminal` |

## ElectricGraphTerminal

Arquivo: `Service/ElectricGraphTerminal.cs`

Campos:

| Propriedade | Uso |
|---|---|
| `ElementId` | Id do elemento dono. |
| `TerminalId` | Id do terminal. |
| `BusName` | Nome de barra resolvido. |
| `Endpoint` | `TerminalEndpoint(elementId, terminalId)`. |
| `SourceTerminal` | Terminal original. |

## ElectricGraphEdge

Arquivo: `Service/ElectricGraphEdge.cs`

Campos:

| Propriedade | Uso |
|---|---|
| `EdgeId` | Id do cabo. |
| `SourceCable` | Cabo original. |
| `FromElementId`, `FromTerminalId` | Origem textual. |
| `ToElementId`, `ToTerminalId` | Destino textual. |
| `From`, `To` | Endpoints estruturados. |
| `IsValid` | Resultado das validações de aresta. |
| `Error` | Mensagem concatenada quando inválida. |

```text
ElectricGraph
    |
    +-- Nodes
    |     |
    |     +-- ElectricGraphNode
    |             |
    |             +-- ElectricGraphTerminal
    |
    +-- Edges
          |
          +-- ElectricGraphEdge
                  From: TerminalEndpoint
                  To:   TerminalEndpoint
```

# 9. ElectricGraphBuilder

Arquivo: `Service/ElectricGraphBuilder.cs`

`ElectricGraphBuilder` recebe `AraciDocument` e opcionalmente `ElementRegistryService`.

## Algoritmo real de Build

`Build()` executa:

```text
1. _document.Elementos
2. filtra por IsNodeElement
3. cria ElectricGraphNode para cada elemento filtrado
4. indexa nós por ElementId
5. percorre Cabos topológicos
6. cria ElectricGraphEdge para cada cabo
7. retorna ElectricGraph(nodes, edges)
```

## Critérios de nó

`IsNodeElement(Elemento elemento)` retorna verdadeiro quando:

```text
elemento.ParticipaDoGrafoEletrico
    &&
elemento is ITerminalOwner
    &&
elemento is not Cabo
```

Logo, `Barra`, `Carga`, `Gerador`, `Sin` e `Transformador` viram nós. `Cabo` não vira nó.

## Criação de nó

`CreateNode(Elemento elemento)`:

1. Usa `elemento.Id.ToString()` como `elementId`.
2. Itera `((ITerminalOwner)elemento).Terminais`.
3. Cria `ElectricGraphTerminal` para cada terminal.
4. Define `BusName` do terminal como `t.Barra` quando preenchido; caso contrário, usa `ResolveBusName(elemento)`.
5. Define `Kind` via `_registry?.GetKind(elemento) ?? elemento.GetType().Name`.

## Critérios de aresta

`BuildEdges` percorre:

```csharp
_document.Elementos
    .OfType<Cabo>()
    .Where(c => c.ParticipaDoGrafoEletrico)
```

Cada cabo gera uma aresta por `CreateEdge`.

## Validações de aresta

`CreateEdge` chama `ValidateEndpoint` para origem e destino. Depois valida mesmo elemento, mesmo terminal e duplicidade por par de terminais.

Mesmo quando inválida, a aresta entra no grafo:

```text
ElectricGraphEdge
    IsValid = false
    Error = "..."
```

Isso permite que `TopologyValidator` reporte cabos inválidos por `graph.GetInvalidEdges()`.

# 10. TopologyValidator

Arquivo: `Service/TopologyValidator.cs`

`TopologyValidator` recebe `AraciDocument`, `ConnectivityService` e `ElectricGraphBuilder`. O construtor simples cria seus próprios serviços.

`Validate()` executa, nesta ordem:

```text
ValidarNomes(result)
ValidarCabos(result)
ValidarEquipamentos(result)
ValidarCircuito(result)
```

## TopologyValidationResult

Arquivo: `Service/TopologyValidationResult.cs`

Contém lista de `TopologyIssue`. Propriedades:

| Propriedade | Função |
|---|---|
| `Issues` | Lista completa. |
| `Errors` | Issues com `TopologyIssueSeverity.Error`. |
| `Warnings` | Issues com `TopologyIssueSeverity.Warning`. |
| `IsValid` | Verdadeiro quando não há erros. |

Métodos:

| Método | Função |
|---|---|
| `AddError(string message)` | Adiciona erro. |
| `AddWarning(string message)` | Adiciona aviso. |
| `FormatErrors()` | Formata erros como linhas iniciadas por `-`. |

`TopologyIssue`, em `Service/TopologyIssue.cs`, possui `Severity`, `Message` e `Elemento?`. No código analisado, `TopologyValidator` usa mensagens, mas não associa `Elemento` ao criar erros.

## Validações observadas

### ValidarNomes

Regras:

| Condição | Mensagem |
|---|---|
| Elemento elétrico sem `Nome` | `Elemento {id} sem Nome.` |
| Dois ou mais elementos elétricos com mesmo nome | `Nome duplicado no documento: '{nome}'.` |

Os elementos avaliados são `ElementosEletricos()`, isto é, `_document.Elementos.Where(e => e.ParticipaDoGrafoEletrico)`.

### ValidarCabos

Constrói `ElectricGraph` e para cada aresta inválida adiciona:

```text
Cabo '{Nome(edge.SourceCable)}': {edge.Error}
```

### ValidarEquipamentos

Regras:

| Elemento | Regra |
|---|---|
| `ElementoEquipamento` | Se `BarraId` estiver preenchido, deve apontar para elemento existente. |
| `Carga` | Deve ter conexão topológica utilizável por Id. |
| `Gerador` | Deve ter conexão topológica utilizável por Id. |

`TemConexaoTopologica(ElementoEquipamento equipamento)` retorna verdadeiro quando:

1. `BarraId` está preenchido e resolve para elemento existente; ou
2. existe `Cabo` topológico cujo `OrigemId` ou `DestinoId` é o id do equipamento.

Mensagens reais:

| Condição | Mensagem |
|---|---|
| `BarraId` inválido | `Equipamento '{Nome}' com BarraId invalido: {BarraId}.` |
| `Carga` sem conexão por Id | `Carga '{Nome}' sem conexao topologica utilizavel por Id.` |
| `Gerador` sem conexão por Id | `Gerador '{Nome}' sem conexao topologica utilizavel por Id.` |

### ValidarCircuito

Regras:

| Condição | Mensagem |
|---|---|
| Não há `Sin` nem `Gerador` elétrico | `Circuito sem fonte slack.` |
| Há mais de um `ElementoEquipamento` e não há `Cabo` elétrico | `Circuito com mais de um equipamento e sem cabo.` |

# 11. OperationalGraphState

Arquivos:

| Classe | Arquivo |
|---|---|
| `OperationalGraphState` | `Service/OperationalGraphState.cs` |
| `OperationalGraphStateBuilder` | `Service/OperationalGraphStateBuilder.cs` |

## OperationalGraphState

`OperationalGraphState` armazena conjuntos de ids:

| Propriedade | Conteúdo |
|---|---|
| `EnergizedNodeIds` | Nós alcançados por fontes. |
| `DeenergizedNodeIds` | Nós não alcançados. |
| `EnergizedEdgeIds` | Arestas válidas percorridas. |
| `DeenergizedEdgeIds` | Arestas não percorridas. |
| `SourceNodeIds` | Ids das fontes usadas. |

Métodos:

| Método | Resultado |
|---|---|
| `IsNodeEnergized(string elementId)` | Verifica se id está em `EnergizedNodeIds`. |
| `IsEdgeEnergized(string edgeId)` | Verifica se id está em `EnergizedEdgeIds`. |

Os conjuntos são criados por `ToSet`, que remove strings vazias, aplica `Trim()` e usa comparação case-insensitive.

## OperationalGraphStateBuilder

`Build(ElectricGraph graph)` lança `ArgumentNullException` se `graph` for nulo.

O algoritmo:

```text
GetSourceNodeIds(graph)
    |
    +-- se houver nós cujo SourceElement is Sin:
    |       usa todos os SIN
    |
    +-- caso contrário:
            usa o primeiro nó cujo SourceElement is Gerador

Inicializa fila BFS com fontes
    |
    v
Enquanto fila não vazia:
    - remove nó atual
    - percorre graph.GetEdgesForElement(currentId).Where(e => e.IsValid)
    - marca aresta como energizada
    - identifica outro endpoint
    - se outro nó existir e ainda não foi energizado, adiciona na fila

Calcula desenergizados por diferença entre todos e energizados
```

Diagrama:

```text
SIN(s) presentes?
    |
    +-- sim -> fontes = todos os nós Sin
    |
    +-- não -> fonte = primeiro Gerador, se existir

fontes
  |
  v
BFS por arestas válidas
  |
  +-- EnergizedNodeIds
  +-- EnergizedEdgeIds
  +-- DeenergizedNodeIds
  +-- DeenergizedEdgeIds
```

O método não usa arestas inválidas na energização, pois filtra `e.IsValid`.

# 12. Fluxo Completo

O fluxo completo real, combinando terminal, cabo, conectividade, grafo, validação e estado operacional, pode ser descrito assim:

```text
Terminal
  |
  | TerminalEndpoint.FromTerminal
  v
TerminalEndpoint
  |
  | InserirCaboUseCase.ConectarOrigem/ConectarDestino
  v
Cabo
  |
  +-- OrigemId
  +-- OrigemTerminalId
  +-- DestinoId
  +-- DestinoTerminalId
  |
  | ConnectivityService
  |   - busca elementos
  |   - busca terminais
  |   - valida ocupação
  |   - reancora cabos
  v
ElectricGraphBuilder
  |
  +-- cria nós para ITerminalOwner topológicos, exceto Cabo
  +-- cria arestas para Cabo
  +-- marca arestas inválidas com erro
  |
  v
TopologyValidator
  |
  +-- valida nomes
  +-- valida cabos via ElectricGraph
  +-- valida BarraId/conexão de cargas e geradores
  +-- valida fonte slack e presença de cabos
  |
  v
OperationalGraphStateBuilder
  |
  +-- escolhe fontes Sin ou primeiro Gerador
  +-- executa BFS por arestas válidas
  +-- separa energizados e desenergizados
```

Na inserção visual de cabos, o fluxo concreto é:

```text
InserirCaboTool
  |
  +-- SnapService localiza Terminal
  +-- ConnectivityService valida
  +-- InserirCaboUseCase cria e conecta Cabo
  +-- Cabo armazena endpoints por Id
  +-- ElectricGraphBuilder interpreta endpoints como arestas
```

# 13. Casos Especiais

## Barras grandes e barras redimensionadas

`Barra.CalcularQuantidadeTerminais` calcula a quantidade de terminais a partir da altura. Aumentar a altura cria novos slots. Reduzir a altura remove terminais fora do novo limite, exceto quando seus ids estão no conjunto de terminais protegidos.

Esse conjunto é fornecido em `ElementGeometryUpdateService.AtualizarElementoECabos`, que chama `ConnectivityService.ObterTerminalIdsOcupados(barra)` antes de atualizar terminais da barra.

## Transformadores

`Transformador` possui dois terminais topológicos, `PRIMARIO` e `SECUNDARIO`. Em `DTOs/ParameterReader.cs`, há tratamento específico para barramentos de transformador:

| Método | Regra |
|---|---|
| `ResolverBarraTransformador` | Delega para `ResolverBarraTerminalTransformador`. |
| `ResolverBarraTerminalTransformador` | Retorna `{NomeBarramento(transformador)}_{terminal}`. |
| `EhTerminalTransformador` | Reconhece `PRIMARIO` e `SECUNDARIO`. |

Assim, para DTOs, terminais de transformador recebem nomes de barra diferenciados por terminal.

## Múltiplos terminais

`Gerador` e `Sin` expõem quatro terminais. `Barra` expõe quantidade variável. `Transformador` expõe dois. `Carga` expõe um terminal principal. O grafo cria `ElectricGraphTerminal` para todos os terminais expostos por cada nó.

## Cabos inválidos

`ElectricGraphBuilder` não remove cabos inválidos. Ele cria `ElectricGraphEdge` com `IsValid = false` e mensagem em `Error`. `TopologyValidator.ValidarCabos` transforma esses erros em `TopologyValidationResult`.

`OperationalGraphStateBuilder` ignora cabos inválidos na BFS porque percorre apenas `Where(e => e.IsValid)`.

## Elementos ignorados

`ElectricGraphBuilder.IsNodeElement` ignora qualquer elemento que não tenha `ParticipaDoGrafoEletrico == true`, qualquer elemento que não implemente `ITerminalOwner` e também ignora `Cabo` como nó.

`TopologyValidator.ElementosEletricos()` também usa `ParticipaDoGrafoEletrico`, portanto elementos com `DomainRole` diferente de `EletricoTopologico` não entram nessas validações.

## Colagem de cabos

`ColarElementosUseCase.LimparConexoesSeNecessario`, em `Applications/UseCases/Editar/ColarElementosUseCase.cs`, limpa `OrigemId`, `DestinoId`, `OrigemTerminalId`, `DestinoTerminalId`, `BarraOrigem` e `BarraDestino` quando o elemento colado é `Cabo`. Isso evita que um cabo clonado mantenha conexões topológicas para elementos originais.

## Exclusão de elementos

`ExcluirElementoUseCase`, em `Applications/UseCases/Editar/ExcluirElementoUseCase.cs`, coleta cabos conectados quando um elemento não cabo é excluído. A ordem final coloca cabos antes de outros elementos:

```text
OrderBy(e => e is Cabo ? 0 : 1)
```

Isso mostra que exclusão topológica remove cabos associados ao elemento selecionado.

# 14. Dívidas Técnicas

As dívidas abaixo são evidências diretas do código atual.

## Regras topológicas distribuídas

As regras de conexão aparecem em `ConnectivityService`, `InserirCaboTool`, `ElectricGraphBuilder`, `TopologyValidator` e `ParameterReader`. Não há uma única política central que concentre todas as regras topológicas.

## Mensagens de validação com acentuação corrompida em alguns pontos

Em arquivos como `Service/ConnectivityService.cs` e `Applications/Diagrama/InserirCabo/InserirCabo.cs`, algumas mensagens aparecem como `"ConexÃ£o invÃ¡lida"` e `"ConexÃ£o ocupada"`. Isso indica inconsistência de encoding textual em mensagens exibidas ou propagadas.

## TopologyIssue.Elemento pouco utilizado

`TopologyIssue` possui propriedade `Elemento? Elemento`, mas `TopologyValidator` chama `AddError(string message)` e não associa o elemento nos erros observados. Isso limita navegação futura do erro para o elemento no diagrama.

## ElectricGraphBuilder inclui arestas inválidas

`CreateEdge` sempre retorna `ElectricGraphEdge`, mesmo quando há erros. Isso é útil para diagnóstico, mas exige que consumidores filtrem `IsValid`. `OperationalGraphStateBuilder` filtra, mas outros consumidores precisam manter a mesma disciplina.

## Ocupação de terminal é restrita a um cabo

`ConnectivityService.ExisteCaboNoTerminal` considera terminal ocupado quando qualquer cabo diferente do atual usa o endpoint. Isso implementa uma conexão por terminal. Caso a arquitetura-alvo exija múltiplas conexões em um mesmo terminal, o modelo atual precisará evoluir.

## Transformador tem tratamento especial no ParameterReader

O grafo possui `ElectricGraphTerminal.BusName`, mas `ParameterReader` ainda possui lógica própria para resolver barras de transformador por terminal. Isso mostra que parte da semântica de barramento ainda está fora do grafo.

## Snap e topologia estão parcialmente acoplados ao estado visual

`SnapService` usa `ISceneQueryService.Nearby` e `ElementoViewModel` para localizar terminais. A inserção de cabo depende desse fluxo visual para encontrar terminais próximos. A topologia persistida é por Id, mas a captura inicial da conexão é visual/espacial.

# 15. Comparação com Arquitetura-Alvo

A implementação atual já possui conceitos essenciais para uma arquitetura elétrica CAD/BIM: terminais explícitos, endpoints por Id, cabos como arestas, nós derivados de elementos topológicos, validação de conectividade e estado operacional por BFS.

| Aspecto | Estado atual | Arquitetura elétrica pretendida |
|---|---|---|
| Identidade topológica | `TerminalEndpoint(ElementId, TerminalId)`. | Base adequada para persistência e simulação. |
| Nós elétricos | Elementos topológicos com `ITerminalOwner`, exceto `Cabo`. | Modelo coerente para grafo. |
| Arestas | `Cabo` gera `ElectricGraphEdge`. | Adequado para redes por conexões físicas. |
| Validação | Distribuída entre serviços. | Poderia ser centralizada em uma camada de regras topológicas. |
| Estado operacional | BFS por fontes `Sin` ou primeiro `Gerador`. | Base inicial; futuras regras podem considerar chaves, proteções e estados operacionais. |
| Barramentos | Resolvidos por nome, barra textual, terminal e casos especiais. | Poderiam ser entidades de domínio explícitas. |
| Transformadores | Nós com dois terminais e tratamento especial em DTO. | Poderiam modelar múltiplos enrolamentos e barras internas no grafo. |
| Snap/conexão | Captura visual via `SnapService` e validação por `ConnectivityService`. | Boa integração CAD; poderia separar serviço topológico puro da camada visual. |

O ponto mais forte da implementação atual é que a conexão não depende apenas de coordenadas. O cabo armazena ids de elemento e terminal, permitindo que `ConnectivityService`, `ElectricGraphBuilder`, `TopologyValidator`, `ParameterReader` e `OperationalGraphStateBuilder` interpretem a rede como topologia elétrica.

O ponto de evolução mais importante é consolidar a semântica elétrica em torno do grafo. Hoje, o grafo representa nós, terminais e arestas, mas algumas regras ainda estão em serviços adjacentes. Uma arquitetura-alvo mais madura poderia separar:

```text
Documento
  |
  v
Modelo topológico puro
  |
  v
Validador de regras elétricas
  |
  v
Estado operacional
  |
  v
Simulação / exportação / UI
```

No estágio atual, a camada topológica do Araci é suficiente para identificar conexões por terminal, bloquear conexões inválidas durante inserção, formar grafo elétrico, validar inconsistências essenciais e determinar energização simples por alcance a partir de fontes. Para um ambiente CAD/BIM elétrico mais completo, as próximas evoluções arquiteturais tenderiam a envolver barramentos explícitos, estados de equipamentos, validações por tipo elétrico, múltiplas conexões controladas por regra e maior centralização das políticas topológicas.
