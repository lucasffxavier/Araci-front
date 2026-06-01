# 1. Introdução

Este documento descreve o domínio e o modelo do projeto Araci com base exclusiva no código existente na solução. O foco está nas classes, propriedades, relações e regras observadas nos arquivos reais do repositório, especialmente em `Core/Documents`, `Models`, `Models/Tipos`, `Service`, `DTOs` e `Infrastructure/Persistence`.

O domínio atual do Araci tem como objetivo representar um documento elétrico composto por elementos posicionáveis, parametrizáveis, tipados e, quando aplicável, conectáveis por terminais. Esses elementos formam uma topologia elétrica quando possuem `DomainRole` igual a `EletricoTopologico`. A partir dessa topologia, o código constrói um grafo elétrico, valida conexões e prepara dados para simulação.

O núcleo do domínio está concentrado em:

| Área | Classes principais | Arquivos |
| --- | --- | --- |
| Documento | `AraciDocument` | `Core/Documents/AraciDocument.cs` |
| Elementos | `Elemento`, `ElementoEquipamento`, `ElementoLinear` | `Models/Elemento.cs`, `Models/ElementoEquipamento.cs`, `Models/ElementoLinear.cs` |
| Elementos concretos | `Barra`, `Cabo`, `Carga`, `Gerador`, `Sin`, `Transformador` | `Models/*.cs` |
| Terminais | `Terminal`, `TerminalEndpoint`, `ITerminalOwner` | `Models/Terminal.cs`, `Models/TerminalEndpoint.cs`, `Models/ITerminalOwner.cs` |
| Parâmetros | `Parameter`, `Parameter<T>` | `Models/Parameter.cs` |
| Tipos | `TipoElemento` e derivados | `Models/Tipos/*.cs` |
| Conectividade | `ConnectivityService`, `ConnectionValidationResult` | `Service/ConnectivityService.cs`, `Service/ConnectionValidationResult.cs` |
| Topologia | `ElectricGraph`, `ElectricGraphBuilder`, `TopologyValidator` | `Service/ElectricGraph*.cs`, `Service/TopologyValidator.cs` |
| Estado operacional | `OperationalGraphState`, `OperationalGraphStateBuilder` | `Service/OperationalGraphState*.cs` |

O domínio atual não é apenas uma lista de símbolos. Cada elemento possui identidade (`Id`), posição, rotação, escala, tipo técnico, parâmetros e, em vários casos, terminais. Os cabos armazenam referências por Id para origem e destino, formando conexões persistíveis entre elementos.

# 2. Visão Geral do Modelo de Domínio

O modelo do domínio é organizado em torno de um documento (`AraciDocument`) que contém uma coleção observável de `Elemento`. Cada elemento pode ter um tipo (`TipoElemento`), parâmetros (`Parameter`) e um papel de domínio (`ElementoDomainRole`). Elementos que implementam `ITerminalOwner` expõem terminais. Cabos conectam endpoints de terminais por Id. O conjunto de elementos elétricos é transformado em grafo por `ElectricGraphBuilder`.

```text
+-----------------------------+
|        AraciDocument        |
| ObservableCollection<Elemento>
+--------------+--------------+
               |
               v
+-----------------------------+
|          Elemento           |
| Id, Nome, Posicao, Rotacao  |
| Escala, Tipo, Parametros    |
| DomainRole                  |
+--------------+--------------+
               |
     +---------+----------+----------------+
     |                    |                |
     v                    v                v
+----------+      +------------------+  +----------------+
|  Barra   |      | ElementoEquipamento | ElementoLinear |
+----------+      +------------------+  +----------------+
     |                    |                |
     |                    |                v
     |        +-----------+----------+  +----------+
     |        |           |          |  |   Cabo   |
     |        v           v          v  +----------+
     |     Carga      Gerador      Sin
     |                    |
     |                    v
     |             Transformador
     |
     v
ITerminalOwner
```

A topologia elétrica é derivada apenas dos elementos cujo `ParticipaDoGrafoEletrico` retorna verdadeiro. Essa propriedade é definida em `Models/Elemento.cs` como:

```text
ParticipaDoGrafoEletrico = DomainRole == ElementoDomainRole.EletricoTopologico
```

No código atual, `Elemento` tem `DomainRole` padrão igual a `Grafico`. `ElementoEquipamento`, `Barra` e `Cabo` sobrescrevem ou definem `DomainRole` como `EletricoTopologico`.

```text
Documento
  |
  | elementos com DomainRole = EletricoTopologico
  v
ElectricGraphBuilder
  |
  +-- ITerminalOwner exceto Cabo -> ElectricGraphNode
  |
  +-- Cabo -> ElectricGraphEdge
  |
  v
ElectricGraph
```

Relações principais:

| Conceito | Relação real no código |
| --- | --- |
| Documento e elementos | `AraciDocument.Elementos` contém `Elemento`. |
| Elemento e tipo | `Elemento.Tipo` referencia `TipoElemento?`. |
| Elemento e parâmetros | `Elemento.Parametros` expõe dicionário de `Parameter`. |
| Elemento e terminal | Elementos que implementam `ITerminalOwner` expõem `IReadOnlyList<Terminal>`. |
| Cabo e conexão | `Cabo` guarda `OrigemId`, `OrigemTerminalId`, `DestinoId`, `DestinoTerminalId`. |
| Terminal e endpoint | `Terminal.Endpoint` cria `TerminalEndpoint` por `Dono.Id` e `Terminal.Id`. |
| Grafo e documento | `ElectricGraphBuilder` percorre `AraciDocument.Elementos`. |

# 3. AraciDocument

`AraciDocument` está em `Core/Documents/AraciDocument.cs`, namespace `Araci.Core.Documents`.

## 3.1 Responsabilidades

Responsabilidades observadas:

- Manter a coleção de elementos do documento.
- Adicionar elementos sem duplicar a mesma referência.
- Remover elementos existentes.
- Limpar o documento.

## 3.2 Propriedades

| Propriedade | Tipo | Descrição |
| --- | --- | --- |
| `Elementos` | `ObservableCollection<Elemento>` | Coleção observável de elementos do documento. |

## 3.3 Comportamento

Métodos reais:

| Método | Comportamento |
| --- | --- |
| `AdicionarElemento(Elemento elemento)` | Adiciona o elemento somente se `Elementos.Contains(elemento)` for falso. |
| `RemoverElemento(Elemento elemento)` | Remove o elemento somente se ele estiver na coleção. |
| `Limpar()` | Executa `Elementos.Clear()`. |

`AraciDocument` não valida topologia, não cria elementos, não aplica regras de conexão e não serializa dados. Essas responsabilidades ficam em serviços como `ConnectivityService`, `ElectricGraphBuilder`, `TopologyValidator` e `ProjectSerializer`.

## 3.4 Papel arquitetural

Arquiteturalmente, `AraciDocument` é o contêiner do estado persistente do domínio. Ele é consumido por:

- `ConnectivityService`, para localizar elementos, cabos e terminais.
- `ElectricGraphBuilder`, para construir nós e arestas.
- `TopologyValidator`, para validar nomes, cabos, equipamentos e circuito.
- `ProjectSerializer`, para criar `ProjectFileDto` e reconstruir elementos.
- `ParameterReader`, para extrair dados de simulação por meio de `CoreApi`.

O documento é simples por design atual: a inteligência do domínio está fora dele.

# 4. Hierarquia de Elementos

A hierarquia real de elementos está nos arquivos:

- `Models/Elemento.cs`
- `Models/ElementoEquipamento.cs`
- `Models/ElementoLinear.cs`
- `Models/Barra.cs`
- `Models/Cabo.cs`
- `Models/Carga.cs`
- `Models/Gerador.cs`
- `Models/Sin.cs`
- `Models/Transformador.cs`

## 4.1 Diagrama de herança

```text
IElementoClonavel
        ^
        |
+------------------+
|     Elemento     |
+------------------+
| Id               |
| PosicaoX         |
| PosicaoY         |
| Rotacao          |
| Escala           |
| Tipo             |
| Nome             |
| Parametros       |
| DomainRole       |
+---+----------+---+
    |          |
    |          +----------------------+
    |                                 |
    v                                 v
+---------+              +----------------------+
|  Barra  |              |   ElementoLinear     |
+---------+              +----------------------+
| ITerminalOwner         | PosicaoX2            |
| DomainRole eletrico    | PosicaoY2            |
                         | Comprimento          |
                         +----------+-----------+
                                    |
                                    v
                              +-----------+
                              |   Cabo    |
                              +-----------+
                              | ITerminalOwner
                              | DomainRole eletrico

+----------------------+
| ElementoEquipamento  |
+----------------------+
| ITerminalOwner       |
| DomainRole eletrico  |
| Barra, BarraId       |
| Alimentador          |
| Potencias            |
| Tensoes              |
| Correntes            |
+----------+-----------+
           |
   +-------+-----------+-------------+----------------+
   |                   |             |                |
   v                   v             v                v
+--------+       +----------+     +------+      +---------------+
| Carga  |       | Gerador  |     | Sin  |      | Transformador |
+--------+       +----------+     +------+      +---------------+
```

## 4.2 Elemento

`Elemento` é uma classe abstrata em `Models/Elemento.cs`, namespace `Araci.Models`.

Propriedades reais:

| Propriedade | Tipo |
| --- | --- |
| `PosicaoX` | `double` |
| `PosicaoY` | `double` |
| `Id` | `Guid` |
| `Rotacao` | `double` |
| `Escala` | `double` |
| `Tipo` | `TipoElemento?` |
| `DomainRole` | `ElementoDomainRole` |
| `ParticipaDoGrafoEletrico` | `bool` |
| `Nome` | `string` |
| `Parametros` | `IReadOnlyDictionary<string, Parameter>` |

Métodos reais:

- `PossuiParametro`
- `Obter<T>`
- `Definir<T>`
- `Clonar`
- `CopiarBasePara`

`Elemento` cria o parâmetro `Nome` no construtor com `Parameter<string>`.

## 4.3 ElementoEquipamento

`ElementoEquipamento` está em `Models/ElementoEquipamento.cs`. É uma classe abstrata que herda `Elemento` e implementa `ITerminalOwner`.

Propriedades e parâmetros reais:

| Propriedade | Parâmetro |
| --- | --- |
| `Barra` | `Barra` |
| `BarraId` | `BarraId` |
| `Alimentador` | `Alimentador` |
| `PotenciaAtiva` | `PotenciaAtiva` |
| `PotenciaReativa` | `PotenciaReativa` |
| `TensaoLinha` | `TensaoLinha` |
| `TensaoFaseA` | `TensaoFaseA` |
| `TensaoFaseB` | `TensaoFaseB` |
| `TensaoFaseC` | `TensaoFaseC` |
| `CorrenteLinha` | `CorrenteLinha` |
| `CorrenteFaseA` | `CorrenteFaseA` |
| `CorrenteFaseB` | `CorrenteFaseB` |
| `CorrenteFaseC` | `CorrenteFaseC` |

Comportamento:

- `DomainRole` retorna `EletricoTopologico`.
- Mantém lista interna de `Terminal`.
- Cria inicialmente um terminal `PRINCIPAL`.
- Ao alterar `Barra`, atualiza `terminal.Barra` de todos os terminais internos.
- `CopiarEquipamentoPara` clona base e terminais.

## 4.4 ElementoLinear

`ElementoLinear` está em `Models/ElementoLinear.cs`. É uma classe abstrata que herda `Elemento`.

Propriedades reais:

- `PosicaoX2`
- `PosicaoY2`
- `Comprimento`

Comportamento:

- `CopiarLinearPara` copia base, segunda posição e comprimento.

No código atual, `Cabo` herda `ElementoLinear`, mas também redefine a propriedade `Comprimento` com `new double Comprimento`, armazenada como parâmetro `PARAM_COMPRIMENTO`.

## 4.5 Classes derivadas completas

| Classe | Base | Implementa `ITerminalOwner` | `DomainRole` |
| --- | --- | --- | --- |
| `Barra` | `Elemento` | Sim | `EletricoTopologico` |
| `Cabo` | `ElementoLinear` | Sim | `EletricoTopologico` |
| `Carga` | `ElementoEquipamento` | Herdado | `EletricoTopologico` herdado |
| `Gerador` | `ElementoEquipamento` | Herdado | `EletricoTopologico` herdado |
| `Sin` | `ElementoEquipamento` | Herdado | `EletricoTopologico` herdado |
| `Transformador` | `ElementoEquipamento` | Herdado | `EletricoTopologico` herdado |

# 5. Elementos Elétricos

## 5.1 Barra

Arquivo: `Models/Barra.cs`  
Namespace: `Araci.Models`  
Classe: `Barra`

Responsabilidade real:

- Representar uma barra elétrica topológica.
- Expor múltiplos terminais elétricos distribuídos ao longo da altura.
- Participar do grafo elétrico.

Propriedades e constantes:

| Nome | Tipo/valor |
| --- | --- |
| `PARAM_ALTURA` | `"Altura"` |
| `PARAM_TENSAO` | `"Tensao"` |
| `ALTURA_PADRAO` | `120` |
| `ALTURA_MINIMA` | `40` |
| `Terminais` | `IReadOnlyList<Terminal>` |
| `DomainRole` | `EletricoTopologico` |
| `TipoBarra` | `(TipoBarra)Tipo!` |
| `Altura` | `double`, normalizada |
| `Tensao` | `string` |

Parâmetros criados no construtor:

| Parâmetro | Valor inicial |
| --- | --- |
| `Nome` | `"BARRA-001"` |
| `Altura` | `120` |
| `Tensao` | `"13,8âˆ 0Â°"` |

Terminais:

- A barra mantém lista interna `_terminais`.
- `AtualizarTerminais` calcula a quantidade pela altura.
- Os Ids seguem o padrão `BARRA-01`, `BARRA-02`, etc.
- Cada terminal é criado com `TerminalKind.Electrical` e `TerminalDirection.East`.
- A posição local usa `centroX = largura / 2` e `y` calculado por espaçamento.

Comportamento elétrico e topológico:

- `Barra` participa do grafo por retornar `EletricoTopologico`.
- `ElectricGraphBuilder` transforma `Barra` em `ElectricGraphNode`, pois ela implementa `ITerminalOwner` e não é `Cabo`.
- Cabos podem apontar para terminais da barra via `OrigemId/DestinoId` e `OrigemTerminalId/DestinoTerminalId`.

Regras observadas:

- Altura inválida, infinita, `NaN` ou menor que `ALTURA_MINIMA` é normalizada para `40`.
- O número de terminais aumenta conforme a altura.
- Terminais protegidos podem impedir remoção durante ajuste de quantidade em `AtualizarTerminais(double largura, IReadOnlySet<string>? terminaisProtegidos)`.

## 5.2 Cabo

Arquivo: `Models/Cabo.cs`  
Namespace: `Araci.Models`  
Classe: `Cabo`

Responsabilidade real:

- Representar uma conexão elétrica entre dois endpoints de terminais.
- Armazenar vértices de geometria do cabo.
- Armazenar origem/destino por Id de elemento e Id de terminal.
- Participar do grafo elétrico como aresta.

Propriedades principais:

| Propriedade | Tipo |
| --- | --- |
| `Terminais` | `IReadOnlyList<Terminal>` |
| `DomainRole` | `EletricoTopologico` |
| `Vertices` | `ObservableCollection<Point>` |
| `PreviewPonto` | `Point?` |
| `TipoCabo` | `TipoCabo` |
| `Origem` | `Terminal?` |
| `Destino` | `Terminal?` |
| `OrigemId` | `string` |
| `DestinoId` | `string` |
| `OrigemTerminalId` | `string` |
| `DestinoTerminalId` | `string` |
| `OrigemEndpoint` | `TerminalEndpoint` |
| `DestinoEndpoint` | `TerminalEndpoint` |
| `BarraOrigem` | `string` |
| `BarraDestino` | `string` |
| `Comprimento` | `double` |
| `Ampacidade` | `double` |

Parâmetros criados no construtor:

| Parâmetro | Valor inicial |
| --- | --- |
| `OrigemId` | `string.Empty` |
| `DestinoId` | `string.Empty` |
| `OrigemTerminalId` | `string.Empty` |
| `DestinoTerminalId` | `string.Empty` |
| `BarraOrigem` | `"GERADOR-001"` |
| `BarraDestino` | `"CARGA-001"` |
| `Comprimento` | `1` |
| `Ampacidade` | `520` |
| `TensaoLinha` | `"12,47âˆ 0Â°"` |
| `TensaoFaseA` | `"7,2âˆ 0Â°"` |
| `TensaoFaseB` | `"7,2âˆ -120Â°"` |
| `TensaoFaseC` | `"7,2âˆ 120Â°"` |
| `CorrenteLinha` | `"0âˆ 0Â°"` |
| `CorrenteFaseA` | `"0âˆ 0Â°"` |
| `CorrenteFaseB` | `"0âˆ -120Â°"` |
| `CorrenteFaseC` | `"0âˆ 120Â°"` |
| `Nome` | `"L1"` |

Terminais:

- Origem é criada ou atualizada por `DefinirOrigem(Point p)`.
- Destino é criado ou atualizado por `DefinirDestino(Point p)`.
- Terminal de origem usa Id `"ORIGEM"`, `TerminalKind.CableEnd` e `TerminalDirection.West`.
- Terminal de destino usa Id `"DESTINO"`, `TerminalKind.CableEnd` e `TerminalDirection.East`.
- `AtualizarTerminaisPelasPontas` usa `Vertices[0]` como origem e `Vertices[^1]` como destino.

Comportamento elétrico e topológico:

- `Cabo` participa do grafo elétrico.
- Em `ElectricGraphBuilder`, cabos não viram nós; viram `ElectricGraphEdge`.
- A origem e o destino são avaliados por `TerminalEndpoint`.
- Um cabo válido precisa apontar para elementos existentes e terminais existentes.

Comportamentos específicos:

- `PossuiOrigemConectada`, `PossuiDestinoConectado` e `PossuiDuasPontasConectadas` dependem de endpoints completos.
- `MoverPreservandoAncoras` move apenas vértices intermediários quando origem e/ou destino estão conectados.
- `Clonar` copia endpoints, barras, parâmetros elétricos, vértices, preview e terminais.

## 5.3 Carga

Arquivo: `Models/Carga.cs`  
Namespace: `Araci.Models`  
Classe: `Carga`

Responsabilidade real:

- Representar uma carga elétrica topológica.
- Herdar parâmetros comuns de `ElementoEquipamento`.
- Expor terminal elétrico para conexão.

Propriedades:

| Propriedade | Origem |
| --- | --- |
| `TipoCarga` | `Carga` |
| `Barra`, `BarraId`, `Alimentador` | `ElementoEquipamento` |
| `PotenciaAtiva`, `PotenciaReativa` | `ElementoEquipamento` |
| `TensaoLinha`, `TensaoFaseA/B/C` | `ElementoEquipamento` |
| `CorrenteLinha`, `CorrenteFaseA/B/C` | `ElementoEquipamento` |

Valores iniciais:

| Campo | Valor |
| --- | --- |
| `Nome` | `"CARGA-001"` |
| `Barra` | `"CARGA-001"` |
| `Alimentador` | `1` |
| `PotenciaAtiva` | `800` |
| `PotenciaReativa` | `300` |
| `PosicaoX` | `500` |
| `PosicaoY` | `250` |

Terminais:

- Herda terminal inicial `PRINCIPAL`.
- `AtualizarTerminais(double largura, double altura)` posiciona o primeiro terminal no topo, em `(largura / 2, 0)`.
- Define `Direction` como `TerminalDirection.North`.
- Atribui `terminal.Barra = Barra`.

Comportamento elétrico e topológico:

- Participa do grafo por herdar `DomainRole` de `ElementoEquipamento`.
- `TopologyValidator` exige conexão topológica utilizável por Id para cargas elétricas.
- `ParameterReader.GetLoads` extrai cargas que participam do grafo elétrico.

## 5.4 Gerador

Arquivo: `Models/Gerador.cs`  
Namespace: `Araci.Models`  
Classe: `Gerador`

Responsabilidade real:

- Representar um gerador elétrico topológico.
- Atuar como fonte possível para slack quando não há `Sin`.
- Expor quatro terminais elétricos.

Propriedades adicionais:

| Propriedade | Parâmetro |
| --- | --- |
| `PotenciaAparente` | `PotenciaAparente` |
| `FatorPotencia` | `FatorPotencia` |
| `TipoGerador` | `TipoGerador` |

Valores iniciais:

| Campo | Valor |
| --- | --- |
| `Nome` | `"GERADOR-001"` |
| `Barra` | `"GERADOR-001"` |
| `Alimentador` | `1` |
| `PotenciaAparente` | `1020` |
| `PotenciaAtiva` | `1000` |
| `PotenciaReativa` | `203` |
| `PosicaoX` | `300` |
| `PosicaoY` | `200` |

Terminais:

`AtualizarTerminais(double largura, double altura)` garante quatro terminais:

| Id | Direção | Posição local |
| --- | --- | --- |
| `TOPO` | `North` | `(largura / 2, 0)` |
| `BASE` | `South` | `(largura / 2, altura)` |
| `ESQUERDA` | `West` | `(0, altura / 2)` |
| `DIREITA` | `East` | `(largura, altura / 2)` |

Comportamento elétrico e topológico:

- Participa do grafo por herdar `EletricoTopologico`.
- `TopologyValidator` exige conexão topológica utilizável por Id para geradores elétricos.
- `OperationalGraphStateBuilder` usa o primeiro `Gerador` como fonte se não houver nós `Sin`.
- `DTOs/CircuitBuilder.cs` usa gerador como slack quando `ParameterReader.GetSins()` não retorna fonte externa.

## 5.5 SIN

Arquivo: `Models/Sin.cs`  
Namespace: `Araci.Models`  
Classe: `Sin`

Responsabilidade real:

- Representar uma fonte externa/rede externa do sistema.
- Atuar como fonte preferencial de energização e slack.
- Expor quatro terminais elétricos.

Constantes:

| Constante | Valor |
| --- | --- |
| `TERMINAL_NORTE` | `"NORTE"` |
| `TERMINAL_SUL` | `"SUL"` |
| `TERMINAL_LESTE` | `"LESTE"` |
| `TERMINAL_OESTE` | `"OESTE"` |

Valores iniciais:

| Campo | Valor |
| --- | --- |
| `Nome` | `"SIN-001"` |
| `Barra` | `"SIN-001"` |
| `Alimentador` | `1` |
| `TensaoLinha` | `"12.47"` |
| `TensaoFaseA/B/C` | `"7.2"` |
| `CorrenteLinha/FaseA/FaseB/FaseC` | `"0"` |
| `PosicaoX` | `200` |
| `PosicaoY` | `160` |

Terminais:

`AtualizarTerminais(double largura, double altura)` garante terminais:

| Id | Direção | Posição local |
| --- | --- | --- |
| `NORTE` | `North` | `(largura / 2, 0)` |
| `SUL` | `South` | `(largura / 2, altura)` |
| `LESTE` | `East` | `(largura, altura / 2)` |
| `OESTE` | `West` | `(0, altura / 2)` |

Comportamento elétrico e topológico:

- Participa do grafo por herdar `EletricoTopologico`.
- `OperationalGraphStateBuilder` prioriza todos os nós cujo `SourceElement is Sin` como fontes.
- `DTOs/CircuitBuilder.cs` prioriza o primeiro `Sin` como slack em relação ao gerador.
- `ParameterReader.GetSins` extrai `ExternalSourceData`.

## 5.6 Transformador

Arquivo: `Models/Transformador.cs`  
Namespace: `Araci.Models`  
Classe: `Transformador`

Responsabilidade real:

- Representar transformador elétrico topológico.
- Expor terminal primário e secundário.
- Carregar parâmetros elétricos próprios.

Constantes e parâmetros:

| Nome | Valor |
| --- | --- |
| `TERMINAL_PRIMARIO` | `"PRIMARIO"` |
| `TERMINAL_SECUNDARIO` | `"SECUNDARIO"` |
| `PARAM_FASES` | `"Fases"` |
| `PARAM_ENROLAMENTOS` | `"Enrolamentos"` |
| `PARAM_TENSAO_PRIMARIO_KV` | `"TensaoPrimarioKV"` |
| `PARAM_TENSAO_SECUNDARIO_KV` | `"TensaoSecundarioKV"` |
| `PARAM_POTENCIA_APARENTE` | `"PotenciaAparente"` |
| `PARAM_R_PERCENTUAL` | `"RPercentual"` |
| `PARAM_X_PERCENTUAL` | `"XPercentual"` |
| `PARAM_LIGACAO_PRIMARIO` | `"LigacaoPrimario"` |
| `PARAM_LIGACAO_SECUNDARIO` | `"LigacaoSecundario"` |

Propriedades:

- `Fases`
- `Enrolamentos`
- `TensaoPrimarioKV`
- `TensaoSecundarioKV`
- `PotenciaAparente`
- `RPercentual`
- `XPercentual`
- `LigacaoPrimario`
- `LigacaoSecundario`
- `TipoTransformador`

Valores iniciais:

| Campo | Valor |
| --- | --- |
| `Nome` | `"TR-001"` |
| `Barra` | `"TR-001"` |
| `Alimentador` | `1` |
| `Fases` | `3` |
| `Enrolamentos` | `2` |
| `TensaoPrimarioKV` | `13.8` |
| `TensaoSecundarioKV` | `0.38` |
| `PotenciaAparente` | `500` |
| `RPercentual` | `1` |
| `XPercentual` | `5` |
| `LigacaoPrimario` | `"Wye"` |
| `LigacaoSecundario` | `"Wye"` |
| `PosicaoX` | `260` |
| `PosicaoY` | `180` |

Terminais:

| Id | Direção | Posição local |
| --- | --- | --- |
| `PRIMARIO` | `North` | `(largura / 2, 0)` |
| `SECUNDARIO` | `South` | `(largura / 2, altura)` |

Comportamento elétrico e topológico:

- Participa do grafo por herdar `EletricoTopologico`.
- `ParameterReader.GetTransformers` extrai `TransformerData`.
- `ParameterReader` resolve barras específicas de transformador por terminal, produzindo nomes no formato `NomeBarramento(transformador)_PRIMARIO` ou `NomeBarramento(transformador)_SECUNDARIO`.

# 6. Sistema de Terminais

O sistema de terminais é formado por:

- `ITerminalOwner`
- `Terminal`
- `TerminalEndpoint`
- `TerminalKind`
- `TerminalDirection`
- `TerminalPlacement`

Todos estão no namespace `Araci.Models`.

## 6.1 ITerminalOwner

Arquivo: `Models/ITerminalOwner.cs`

Contrato:

```text
IReadOnlyList<Terminal> Terminais { get; }
```

Implementações reais:

- `Barra`
- `Cabo`
- `ElementoEquipamento`, e por herança `Carga`, `Gerador`, `Sin`, `Transformador`

## 6.2 Terminal

Arquivo: `Models/Terminal.cs`

Propriedades:

| Propriedade | Tipo |
| --- | --- |
| `Dono` | `Elemento` |
| `Id` | `string` |
| `Posicao` | `Point` |
| `PosicaoLocal` | `Point` |
| `Barra` | `string?` |
| `Kind` | `TerminalKind` |
| `Direction` | `TerminalDirection` |
| `Endpoint` | `TerminalEndpoint` |

Comportamento:

- Se o construtor recebe `id` nulo ou vazio, cria `Guid.NewGuid().ToString("N")`.
- `DefinirPosicaoLocal` atualiza `PosicaoLocal` e calcula `Posicao` por `TerminalPlacement.ToWorld`.
- `DefinirPosicaoVisual` atualiza `Posicao` e calcula `PosicaoLocal` por `TerminalPlacement.ToLocal`.

## 6.3 TerminalEndpoint

Arquivo: `Models/TerminalEndpoint.cs`

`TerminalEndpoint` é um `readonly struct` composto por:

- `ElementId`
- `TerminalId`

Comportamento:

- Normaliza strings com `Trim`.
- `IsComplete` exige `ElementId` e `TerminalId` não vazios.
- `FromTerminal` cria endpoint por `terminal.Dono.Id.ToString()` e `terminal.Id`.
- Implementa igualdade case-insensitive.
- `ToString` retorna `"ElementId:TerminalId"` se completo.
- `PairKey` produz chave de par independente de ordem para detectar duplicidade.

## 6.4 TerminalKind

Arquivo: `Models/TerminalKind.cs`

Valores:

| Valor | Uso observado |
| --- | --- |
| `Electrical` | Terminais de equipamentos e barras. |
| `CableEnd` | Terminais internos de origem/destino do cabo. |

## 6.5 TerminalDirection

Arquivo: `Models/TerminalDirection.cs`

Valores:

- `None`
- `North`
- `South`
- `East`
- `West`
- `Internal`

Usos observados:

- `Carga` usa `North`.
- `Gerador` usa `North`, `South`, `West`, `East`.
- `Sin` usa `North`, `South`, `East`, `West`.
- `Transformador` usa `North` e `South`.
- `Cabo` usa `West` para origem e `East` para destino.
- `ElementoEquipamento` cria terminal inicial com `None`.

## 6.6 TerminalPlacement

Arquivo: `Models/TerminalPlacement.cs`

Responsabilidade:

- Converter posição local em posição de mundo.
- Converter posição de mundo em posição local.
- Considerar `Elemento.PosicaoX`, `Elemento.PosicaoY`, `Elemento.Rotacao`, `Elemento.Escala` e pivô calculado por tamanho.

Fluxo conceitual:

```text
local point
  -> desloca em relacao ao pivo
  -> aplica escala
  -> aplica rotacao
  -> soma posicao do elemento e pivo
  -> world point
```

Se o tamanho é vazio ou inválido, o pivô é `(0, 0)`. Se o tamanho é válido, o pivô é o centro: `(width / 2, height / 2)`.

# 7. Sistema de Conexões

As conexões são implementadas por referência de Id, não por referência direta a objetos conectados. O principal portador da conexão é `Cabo`.

## 7.1 Conexões por Id

`Cabo` possui:

| Propriedade | Significado |
| --- | --- |
| `OrigemId` | Id do elemento de origem. |
| `OrigemTerminalId` | Id do terminal de origem. |
| `DestinoId` | Id do elemento de destino. |
| `DestinoTerminalId` | Id do terminal de destino. |
| `OrigemEndpoint` | `TerminalEndpoint(OrigemId, OrigemTerminalId)`. |
| `DestinoEndpoint` | `TerminalEndpoint(DestinoId, DestinoTerminalId)`. |

A conexão entre elementos é recuperada por busca no documento. `ConnectivityService.ObterElementoPorId` percorre `AraciDocument.Elementos` e compara `Elemento.Id.ToString()` com o Id recebido.

## 7.2 Origem e destino

`Applications/UseCases/Diagrama/InserirCaboUseCase.cs` mostra como uma conexão é preenchida:

- `ConectarOrigem` define `OrigemId`, `OrigemTerminalId`, `BarraOrigem` e chama `Cabo.DefinirOrigem`.
- `ConectarDestino` define `DestinoId`, `DestinoTerminalId`, `BarraDestino` e chama `Cabo.DefinirDestino`.
- `FinalizarDestino` finaliza a geometria no ponto e conecta destino.

```text
Terminal origem
  -> Elemento.Id
  -> Terminal.Id
  -> Cabo.OrigemId
  -> Cabo.OrigemTerminalId

Terminal destino
  -> Elemento.Id
  -> Terminal.Id
  -> Cabo.DestinoId
  -> Cabo.DestinoTerminalId
```

## 7.3 Terminais ocupados

`ConnectivityService` identifica ocupação por endpoints:

- `ObterTerminalIdsOcupados(Elemento elemento)` retorna os `TerminalId` já usados por cabos conectados ao elemento.
- `TerminalEstaOcupado(Terminal terminal, Cabo? caboIgnorado = null)` verifica se existe cabo usando o endpoint.
- `ExisteCaboNoTerminal` ignora opcionalmente um cabo atual.

## 7.4 Validações

`ConnectivityService.ValidarTerminalDisponivel`:

- rejeita terminal nulo;
- rejeita endpoint incompleto;
- rejeita terminal já ocupado.

`ConnectivityService.ValidarConexaoCabo`:

- rejeita origem ou destino nulos;
- rejeita endpoint incompleto;
- rejeita origem e destino no mesmo elemento;
- rejeita mesmo endpoint;
- rejeita terminais ocupados;
- rejeita cabo duplicado entre o mesmo par de terminais.

`ElectricGraphBuilder` também valida ao construir arestas:

- cabo sem `OrigemId`;
- cabo com `OrigemId` inexistente;
- cabo sem `OrigemTerminalId`;
- cabo com `OrigemTerminalId` inexistente;
- cabo sem `DestinoId`;
- cabo com `DestinoId` inexistente;
- cabo sem `DestinoTerminalId`;
- cabo com `DestinoTerminalId` inexistente;
- origem e destino no mesmo elemento;
- origem e destino no mesmo terminal;
- cabo duplicado entre os mesmos terminais.

# 8. Sistema de Parâmetros

O sistema de parâmetros está em `Models/Parameter.cs` e é usado por `Elemento` e `TipoElemento`.

## 8.1 Parameter

`Parameter` é classe abstrata com:

| Propriedade | Tipo |
| --- | --- |
| `Nome` | `string` |
| `Tipo` | `Type` |
| `ValorObjeto` | `object?` |

Método:

- `Clonar()`

## 8.2 Parameter<T>

`Parameter<T>` armazena:

- campo privado `_valor`;
- propriedade `Valor`;
- implementação de `ValorObjeto`.

Comportamento real:

- Se `ValorObjeto` recebe `null`, `_valor` recebe `default!`.
- Se o valor é do tipo `T`, é atribuído diretamente.
- Caso contrário, tenta `Convert.ChangeType`.
- Se a conversão falhar, lança `InvalidCastException`.

## 8.3 Armazenamento

`Elemento` mantém:

```text
Dictionary<string, Parameter> _parametros
```

`TipoElemento` também mantém:

```text
Dictionary<string, Parameter> _parametros
```

Ambos expõem `Parametros` como `IReadOnlyDictionary<string, Parameter>`.

## 8.4 Leitura

Leitura direta:

- `Elemento.Obter<T>(string nome)`
- `TipoElemento.Obter<T>(string nome)`

Escrita direta:

- `Elemento.Definir<T>(string nome, T valor)`
- `TipoElemento.Definir<T>(string nome, T valor)`

Leitura para simulação:

- `DTOs/ParameterReader.cs` lê parâmetros de instância e, em alguns casos, parâmetros de tipo.
- `ReadValueObject` primeiro procura em `elemento.Parametros`; depois procura em `elemento.Tipo?.Parametros`.

## 8.5 Persistência

`Infrastructure/Persistence/ProjectSerializer.cs` serializa parâmetros por:

```text
elemento.Parametros.Values.Select(CriarParameterDto)
```

`ParameterDto`, em `ProjectFileDto.cs`, contém:

- `Name`
- `Type`
- `Value`

Na abertura:

- `AplicarParametros` percorre `ParameterDto`.
- Ignora parâmetros que o elemento atual não possui.
- Converte `JsonElement` para o tipo do parâmetro de destino.
- Atribui em `parameter.ValorObjeto`.

Isso significa que o formato persiste os valores de parâmetros existentes, mas não cria parâmetros inexistentes em elementos carregados.

# 9. Sistema de Tipos

O sistema de tipos está em `Models/Tipos`.

## 9.1 TipoElemento

Arquivo: `Models/Tipos/TipoElemento.cs`

Propriedades base:

| Propriedade | Parâmetro |
| --- | --- |
| `NomeTipo` | `NomeTipo` |
| `Familia` | `Familia` |
| `Categoria` | `Categoria` |
| `Parametros` | Dicionário de `Parameter` |

Métodos:

- `PossuiParametro`
- `Obter<T>`
- `Definir<T>`

## 9.2 Tipos concretos

| Classe | Arquivo | Parâmetros principais |
| --- | --- | --- |
| `TipoBarra` | `Models/Tipos/TipoBarra.cs` | `ClasseTensao`, `Fases`; propriedades simples `AlturaPadrao`, `NumeroConexoes`. |
| `TipoCabo` | `Models/Tipos/TipoCabo.cs` | `Fases`, `R1`, `X1`, `R0`, `X0`, `C1`, `C0`, `Secao`. |
| `TipoCarga` | `Models/Tipos/TipoCarga.cs` | `ModeloCarga`, `Conexao`, `Tensao`, `Fases`, `FatorPotencia`. |
| `TipoGerador` | `Models/Tipos/TipoGerador.cs` | `Fases`, `TensaoKV`, `ModeloFonte`, `FatorPotencia`. |
| `TipoSin` | `Models/Tipos/TipoSin.cs` | `Fases`, `PotenciaCurtoMVA`, `RelacaoXR`. |
| `TipoTransformador` | `Models/Tipos/TipoTransformador.cs` | `Fases`, `Enrolamentos`, `RPercentual`, `XPercentual`, `LigacaoPrimario`, `LigacaoSecundario`. |

## 9.3 Biblioteca de tipos

`Service/TypeLibraryService.cs` cria coleções observáveis:

- `TiposCabos`
- `TiposCargas`
- `TiposGeradores`
- `TiposSin`
- `TiposTransformadores`
- `TiposBarras`

Também expõe os tipos padrão como o primeiro item de cada coleção.

## 9.4 Tipo versus instância

No código atual:

- `TipoElemento` representa dados de tipo/catálogo.
- `Elemento.Tipo` referencia o tipo atribuído a uma instância.
- `Elemento.Parametros` representa parâmetros da instância.

Exemplo real:

```text
TipoCabo
  R1, X1, R0, X0, C1, C0, Secao

Cabo
  OrigemId, DestinoId, Comprimento, Ampacidade,
  Tensoes, Correntes, Vertices
```

`ParameterReader` pode ler primeiro parâmetros da instância e depois parâmetros do tipo, conforme `ReadValueObject`.

## 9.5 Registro de elementos

`Applications/Factories/ElementDefinitionsProvider.cs` registra tipos, modelos e atualização de terminais para:

- `ElementKinds.Cabo`
- `ElementKinds.Carga`
- `ElementKinds.Gerador`
- `ElementKinds.Sin`
- `ElementKinds.Transformador`
- `ElementKinds.Barra`

`Applications/Abstractions/ElementKinds.cs` define os kinds reais:

| Constante | Valor |
| --- | --- |
| `Barra` | `"Barra"` |
| `Carga` | `"Carga"` |
| `Gerador` | `"Gerador"` |
| `Sin` | `"Sin"` |
| `Transformador` | `"Transformador"` |
| `Cabo` | `"Cabo"` |

`Service/ElementRegistryService.cs` resolve kind, tipo padrão, tipos disponíveis, tamanho e atualização de terminais.

# 10. DomainRole

`ElementoDomainRole` está em `Models/ElementoDomainRole.cs`.

Valores reais:

| Valor | Código |
| --- | --- |
| `Grafico` | `0` |
| `Anotacao` | `1` |
| `EletricoTopologico` | `2` |

`Elemento.DomainRole` retorna `Grafico` por padrão. `Elemento.ParticipaDoGrafoEletrico` retorna verdadeiro apenas quando `DomainRole == EletricoTopologico`.

## 10.1 Grafico

`Grafico` é o valor padrão de `Elemento`. Um elemento com esse papel não participa do grafo elétrico, pois `ParticipaDoGrafoEletrico` retorna falso.

Não há classe concreta de produção encontrada que sobrescreva `DomainRole` para `Grafico`; esse é o comportamento base.

## 10.2 Anotacao

`Anotacao` existe no enum, mas não há classe de domínio concreta de produção encontrada com esse papel. Há evidência em `Araci.TechnicalChecks/Program.cs` de classe de teste `FakeAnnotationElement`, usada para verificar que elementos anotativos são ignorados por grafo/topologia/DTO.

## 10.3 EletricoTopologico

`EletricoTopologico` é usado por:

- `ElementoEquipamento`
- `Barra`
- `Cabo`

Impactos reais:

- `ElectricGraphBuilder` só considera elementos com `ParticipaDoGrafoEletrico`.
- `TopologyValidator` só valida elementos retornados por `ElementosEletricos`.
- `ParameterReader` filtra cargas, cabos, transformadores, geradores e SIN por `ParticipaDoGrafoEletrico`.

```text
Elemento.DomainRole
        |
        v
ParticipaDoGrafoEletrico
        |
        +-- false -> ignorado por ElectricGraphBuilder/TopologyValidator
        |
        +-- true  -> elegível para grafo, topologia e simulação
```

# 11. Formação da Topologia Elétrica

A topologia elétrica é formada por `ElectricGraphBuilder`, a partir dos elementos do `AraciDocument`.

Classes:

- `ElectricGraph`
- `ElectricGraphNode`
- `ElectricGraphEdge`
- `ElectricGraphTerminal`
- `ElectricGraphBuilder`
- `TopologyValidator`

Todos estão no namespace `Araci.Services`.

## 11.1 ElectricGraph

Arquivo: `Service/ElectricGraph.cs`

Propriedades:

| Propriedade | Tipo |
| --- | --- |
| `Nodes` | `IReadOnlyList<ElectricGraphNode>` |
| `Edges` | `IReadOnlyList<ElectricGraphEdge>` |

Métodos principais:

- `FindNode`
- `FindNodeByElementId`
- `FindNodeByElement`
- `FindTerminal`
- `GetEdgesForElement`
- `GetEdgesForTerminal`
- `GetNeighbors`
- `GetInvalidEdges`
- `GetValidEdges`
- `FindEdgeByCableId`
- `FindEdgeByCable`
- `BreadthFirst`

## 11.2 ElectricGraphNode

Arquivo: `Service/ElectricGraphNode.cs`

Propriedades:

- `ElementId`
- `ElementGuid`
- `Name`
- `Kind`
- `SourceElement`
- `Terminals`

Um nó representa um elemento elétrico topológico que implementa `ITerminalOwner` e não é `Cabo`.

## 11.3 ElectricGraphEdge

Arquivo: `Service/ElectricGraphEdge.cs`

Propriedades:

- `EdgeId`
- `SourceCable`
- `FromElementId`
- `FromTerminalId`
- `ToElementId`
- `ToTerminalId`
- `From`
- `To`
- `IsValid`
- `Error`

Uma aresta representa um `Cabo`.

## 11.4 ElectricGraphTerminal

Arquivo: `Service/ElectricGraphTerminal.cs`

Propriedades:

- `ElementId`
- `TerminalId`
- `BusName`
- `Endpoint`
- `SourceTerminal`

O terminal de grafo preserva referência ao `Terminal` de domínio em `SourceTerminal`.

## 11.5 ElectricGraphBuilder

Arquivo: `Service/ElectricGraphBuilder.cs`

Fluxo real:

```text
AraciDocument.Elementos
  |
  +-- Where(IsNodeElement)
  |     criterio:
  |       ParticipaDoGrafoEletrico == true
  |       elemento is ITerminalOwner
  |       elemento is not Cabo
  |
  +-- Select(CreateNode)
  |
  +-- OfType<Cabo>().Where(ParticipaDoGrafoEletrico)
        |
        v
      CreateEdge
```

Diagrama:

```text
+--------------------+
|   AraciDocument    |
+---------+----------+
          |
          v
+--------------------+
| Elementos eletricos|
+----+----------+----+
     |          |
     |          |
     v          v
ITerminalOwner  Cabo
not Cabo        |
     |          |
     v          v
ElectricGraphNode  ElectricGraphEdge
```

`CreateNode`:

- usa `elemento.Id.ToString()` como `ElementId`;
- usa `ResolveBusName(elemento)` como `Name`;
- usa `_registry?.GetKind(elemento) ?? elemento.GetType().Name` como `Kind`;
- cria `ElectricGraphTerminal` para cada terminal do elemento.

`CreateEdge`:

- usa `Cabo.OrigemEndpoint` e `Cabo.DestinoEndpoint`;
- valida endpoints;
- valida mesmo elemento;
- valida mesmo terminal;
- valida duplicidade por `TerminalEndpoint.PairKey`;
- cria `ElectricGraphEdge` com `IsValid` e `Error`.

## 11.6 TopologyValidator

Arquivo: `Service/TopologyValidator.cs`

`TopologyValidator.Validate()` executa:

```text
Validate
  -> ValidarNomes
  -> ValidarCabos
  -> ValidarEquipamentos
  -> ValidarCircuito
```

Validações reais:

| Método | Regras |
| --- | --- |
| `ValidarNomes` | Elementos elétricos sem `Nome`; nomes duplicados. |
| `ValidarCabos` | Arestas inválidas retornadas por `ElectricGraph.GetInvalidEdges()`. |
| `ValidarEquipamentos` | `BarraId` inválido; carga sem conexão topológica por Id; gerador sem conexão topológica por Id. |
| `ValidarCircuito` | Circuito sem `Sin` e sem `Gerador`; circuito com mais de um equipamento e sem cabo. |

`TopologyValidationResult` armazena `TopologyIssue`, separa `Errors` e `Warnings`, e considera válido quando não há erros.

# 12. OperationalGraphState

`OperationalGraphState` e `OperationalGraphStateBuilder` estão em:

- `Service/OperationalGraphState.cs`
- `Service/OperationalGraphStateBuilder.cs`

## 12.1 OperationalGraphState

Propriedades:

| Propriedade | Tipo |
| --- | --- |
| `EnergizedNodeIds` | `IReadOnlySet<string>` |
| `DeenergizedNodeIds` | `IReadOnlySet<string>` |
| `EnergizedEdgeIds` | `IReadOnlySet<string>` |
| `DeenergizedEdgeIds` | `IReadOnlySet<string>` |
| `SourceNodeIds` | `IReadOnlyList<string>` |

Métodos:

- `IsNodeEnergized(string elementId)`
- `IsEdgeEnergized(string edgeId)`

## 12.2 Energização

`OperationalGraphStateBuilder.Build(ElectricGraph graph)`:

1. Obtém fontes por `GetSourceNodeIds`.
2. Adiciona fontes à fila.
3. Percorre arestas válidas conectadas ao nó atual.
4. Energiza arestas válidas.
5. Energiza nós alcançáveis por arestas válidas.
6. Calcula nós e arestas desenergizados por diferença.

```text
Fontes
  |
  v
Queue BFS
  |
  +-- arestas validas do no atual
  |       |
  |       v
  |   energiza aresta
  |       |
  |       v
  |   encontra outro no
  |       |
  |       v
  |   energiza e enfileira
  |
  v
OperationalGraphState
```

## 12.3 Slack, SIN e Gerador

`GetSourceNodeIds` implementa a regra:

- Se existirem nós cujo `SourceElement is Sin`, todos esses nós são fontes.
- Se não houver `Sin`, usa o primeiro nó cujo `SourceElement is Gerador`.
- Se não houver `Sin` nem `Gerador`, retorna lista vazia.

Essa regra é coerente com a escolha de slack em `DTOs/CircuitBuilder.cs`, onde `Sin` é preferido e gerador é fallback.

# 13. Regras de Negócio do Domínio

Regras reais identificadas no código:

1. `AraciDocument` não adiciona a mesma referência de elemento duas vezes.

2. Todo `Elemento` possui `Id`, posição, rotação, escala, tipo opcional e parâmetro `Nome`.

3. Um elemento só participa do grafo elétrico quando `DomainRole == EletricoTopologico`.

4. `ElementoEquipamento` é sempre topológico e possui parâmetros comuns de barra, alimentador, potência, tensão e corrente.

5. `Barra` normaliza altura para no mínimo `40`.

6. `Barra` recalcula quantidade e posição dos terminais conforme altura.

7. `Cabo` conecta elementos por `OrigemId/DestinoId` e `OrigemTerminalId/DestinoTerminalId`.

8. `Cabo` só tem origem ou destino conectados quando os respectivos endpoints estão completos.

9. `Cabo.MoverPreservandoAncoras` não move vértices ancorados em endpoints conectados.

10. `TerminalEndpoint` compara endpoints ignorando caixa de texto.

11. `TerminalEndpoint.PairKey` permite detectar pares duplicados independentemente da ordem.

12. `ConnectivityService.ValidarConexaoCabo` impede conexão no mesmo elemento.

13. `ConnectivityService.ValidarConexaoCabo` impede conexão no mesmo endpoint.

14. `ConnectivityService.ValidarConexaoCabo` impede conexão em terminal já ocupado.

15. `ConnectivityService.ValidarConexaoCabo` impede cabo duplicado entre os mesmos terminais.

16. `ElectricGraphBuilder` cria nós para elementos topológicos que implementam `ITerminalOwner`, exceto cabos.

17. `ElectricGraphBuilder` cria arestas para cabos topológicos.

18. `TopologyValidator` exige nome para elementos elétricos.

19. `TopologyValidator` rejeita nomes duplicados entre elementos elétricos.

20. `TopologyValidator` rejeita circuitos sem `Sin` e sem `Gerador`.

21. `TopologyValidator` rejeita circuito com mais de um equipamento e sem cabo.

22. `TopologyValidator` exige conexão topológica por Id para cargas e geradores.

23. `OperationalGraphStateBuilder` energiza a partir de `Sin` quando há SIN.

24. `OperationalGraphStateBuilder` usa o primeiro `Gerador` quando não há SIN.

25. `ProjectSerializer` ignora parâmetros persistidos que não existem no modelo atual.

26. `ProjectSerializer` normaliza escala zero para `1` ao reconstruir elemento.

27. `ProjectSerializer` normaliza altura de `Barra` ao carregar.

28. `ProjectSerializer` restaura terminais por correspondência de `Terminal.Id`.

29. `TypeLibraryService` cria pelo menos um tipo padrão para cada família suportada.

30. `ElementRegistryService` impede registro duplicado de `ElementDefinition.Kind`.

# 14. Limitações e Dívidas Técnicas do Domínio

As limitações abaixo são baseadas somente em evidências do código real.

| Limitação/Dívida | Evidência |
| --- | --- |
| Domínio acoplado a WPF | `Models` usa `System.Windows.Point`, `Vector` e `Size` indiretamente em terminais e posicionamento. |
| `Cabo` redefine `Comprimento` | `ElementoLinear` tem `Comprimento`, mas `Cabo` declara `new double Comprimento` baseado em parâmetro. |
| `DomainRole.Anotacao` sem classe concreta de produção | O enum existe, mas só foi observado uso real em verificação técnica com `FakeAnnotationElement`. |
| `TopologyIssue.Elemento` pouco explorado | `TopologyValidator` adiciona erros por texto e não associa elemento nos erros observados. |
| `TopologyValidator` valida conexão topológica de `Carga` e `Gerador`, mas não há regra equivalente explícita para `Sin` e `Transformador` no mesmo formato. |
| Valores textuais com encoding corrompido | Parâmetros iniciais exibem strings como `"13,8âˆ 0Â°"` e mensagens como `"ConexÃ£o invÃ¡lida"`. |
| Tipos misturam parâmetros e propriedades simples | `TipoBarra` possui `AlturaPadrao` e `NumeroConexoes` fora do dicionário `Parametros`, enquanto outros dados ficam em `Parameter`. |
| Persistência salva `DomainRole`, mas não usa esse campo na reconstrução | `ProjectSerializer.CriarElementoDto` grava `DomainRole`, mas `CriarElemento` não aplica `dto.DomainRole`. |
| Persistência de parâmetros é conservadora | `ProjectSerializer.AplicarParametros` ignora parâmetros desconhecidos em vez de preservá-los. |
| Bus de transformador é regra especial em `ParameterReader` | `ResolverBarraTerminalTransformador` cria nomes por terminal (`Nome_PRIMARIO`, `Nome_SECUNDARIO`) fora do modelo `Transformador`. |
| `ElectricGraphBuilder` marca aresta inválida, mas ainda inclui a aresta em `Edges` | `CreateEdge` sempre retorna `ElectricGraphEdge`, com `IsValid=false` e `Error`. |

# 15. Comparação com Arquitetura-Alvo

A arquitetura-alvo do domínio, considerando a direção já documentada do projeto, aponta para um modelo elétrico estruturado, persistível, topológico e adequado à simulação. O domínio atual já contém a maior parte dos conceitos essenciais, mas ainda mistura responsabilidades de domínio, geometria visual e infraestrutura de leitura.

## 15.1 Pontos alinhados

| Expectativa de domínio | Evidência no código atual |
| --- | --- |
| Documento como agregado | `AraciDocument.Elementos`. |
| Elementos com identidade própria | `Elemento.Id`. |
| Elementos tipados | `Elemento.Tipo` e `TipoElemento`. |
| Parâmetros estruturados | `Parameter`, `Parameter<T>`, dicionários de parâmetros. |
| Terminais explícitos | `ITerminalOwner`, `Terminal`, `TerminalEndpoint`. |
| Conexões persistíveis por Id | `Cabo.OrigemId`, `DestinoId`, `OrigemTerminalId`, `DestinoTerminalId`. |
| Separação entre elemento e tipo | `Elemento` versus `TipoElemento`. |
| Topologia derivada do documento | `ElectricGraphBuilder`. |
| Validação topológica | `TopologyValidator`. |
| Energização operacional | `OperationalGraphStateBuilder`. |
| Papel de domínio | `ElementoDomainRole`. |

## 15.2 Pontos parcialmente alinhados

| Área | Estado atual |
| --- | --- |
| Domínio independente de UI | Parcial; há dependência de `System.Windows.Point`, `Vector` e geometria WPF. |
| Modelo topológico puro | Parcial; topologia existe, mas posição visual e terminal elétrico compartilham o mesmo objeto. |
| Tipos como catálogo | Parcial; `TypeLibraryService` cria tipos padrão em memória, sem persistência própria de catálogo. |
| Persistência completa do domínio | Parcial; salva parâmetros, tipo, terminais e vértices, mas ignora `DomainRole` na reconstrução e parâmetros desconhecidos. |
| Regras elétricas centralizadas | Parcial; regras aparecem em `ConnectivityService`, `ElectricGraphBuilder`, `TopologyValidator`, `ParameterReader` e `CircuitBuilder`. |

## 15.3 Lacunas observadas

```text
Arquitetura-alvo desejada
  Modelo de dominio eletrico puro
  Topologia eletrica
  Persistencia
  Simulacao
  Apresentacao

Estado atual observado
  Modelo + geometria WPF
  Topologia em Services
  Persistencia dependente de geometria/catalogo
  Leitura de simulacao em DTOs
  DomainRole persistido mas nao reidratado
```

Lacunas principais:

- O domínio ainda não é independente de WPF.
- O sistema de parâmetros não preserva parâmetros desconhecidos no carregamento.
- `DomainRole` é salvo no DTO, mas não há aplicação desse valor na reconstrução do elemento.
- A formação de dados de simulação está distribuída entre domínio, services e `DTOs`.
- Tipos padrão existem em memória por `TypeLibraryService`, mas não há modelo persistente de biblioteca de tipos no domínio atual.

## 15.4 Síntese arquitetural

O domínio atual do Araci já representa de forma concreta os conceitos centrais de um modelo elétrico 2D: documento, elemento, tipo, parâmetro, terminal, cabo, conexão, grafo, validação e energização. A estrutura é suficiente para construir topologia elétrica a partir do documento e diferenciar elementos topológicos de elementos não topológicos por `DomainRole`.

O principal avanço em relação a um desenho puramente gráfico é que conexões são armazenadas por Id de elemento e Id de terminal, e não apenas por coordenadas. Isso permite que `ElectricGraphBuilder`, `ConnectivityService`, `TopologyValidator`, `ParameterReader` e `CircuitBuilder` interpretem o documento como rede elétrica.

O principal limite arquitetural é que o domínio ainda carrega conceitos de geometria WPF e parte das regras está distribuída fora das entidades. A evolução para uma arquitetura-alvo mais limpa exigiria separar progressivamente modelo elétrico, geometria visual, persistência, leitura para simulação e apresentação, mantendo as relações reais já estabelecidas por `AraciDocument`, `Elemento`, `TerminalEndpoint` e `ElectricGraph`.
