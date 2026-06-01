# 1. Introdução

Este documento descreve a arquitetura de simulação do Araci conforme o código atualmente existente na solução. O foco é mapear o fluxo real que transforma o `AraciDocument` em DTOs de circuito, envia esses DTOs para uma API HTTP de simulação, recebe resultados e aplica correntes calculadas ao modelo.

O documento é baseado exclusivamente nos arquivos reais encontrados no projeto. Não são descritos endpoints, DTOs, serviços, scripts OpenDSS ou processos FastAPI que não estejam representados no código. Quando um item solicitado não existe com o nome esperado, isso é registrado explicitamente. Por exemplo, não foi encontrada uma classe `SinDto`; o código representa a fonte slack por `SlackDto` e lê elementos `Sin` por `ParameterReader.ExternalSourceData`.

A arquitetura implementada é uma integração cliente WPF/.NET 8 com uma API HTTP externa. O código local não contém implementação de servidor FastAPI nem chamadas diretas a uma biblioteca OpenDSS. A evidência de OpenDSS aparece no retorno de `SimulationResultDto.Script`, na exportação de arquivo `.dss` e no nome do gateway `FastApiOpenDssGateway`.

# 2. Visão Geral

O fluxo principal de simulação observado é:

```text
AraciDocument
    |
    v
ParameterReader
    |
    +-- usa TopologyValidator
    +-- usa ElectricGraphBuilder
    +-- usa ConnectivityService
    |
    v
CircuitBuilder
    |
    v
CircuitDto
    |
    v
FastApiOpenDssGateway
    |
    v
SimulationApiClient
    |
    v
POST http://127.0.0.1:8000/simular
    |
    v
SimulationResultDto
    |
    v
SimulationResultApplier
    |
    v
Cabo / Carga atualizados com correntes
```

O fluxo requisitado `AraciDocument -> ElectricGraph -> ParameterReader -> DTOs -> CircuitBuilder -> OpenDSS -> Resultados` aparece no código com uma nuance importante: `ElectricGraph` não é passado como objeto externo para o pipeline; ele é construído internamente por `ParameterReader` quando necessário, por meio de `ElectricGraphBuilder`.

```text
CircuitDtoBuilder.Build()
    |
    v
new ParameterReader(_document)
    |
    v
new CircuitBuilder(reader).Build()
    |
    +-- reader.ValidateTopology()
    +-- reader.GetSins()
    +-- reader.GetGenerators()
    +-- reader.GetLoads()
    +-- reader.GetLines()
    +-- reader.GetTransformers()
```

Arquivos principais:

| Área | Arquivos |
|---|---|
| Pipeline | `Applications/Simulation/SimulationPipeline.cs`, `Applications/Simulation/CircuitDtoBuilder.cs` |
| Contratos | `Applications/Abstractions/ISimulationPipeline.cs`, `ISimulationGateway.cs`, `ISimulationResultApplier.cs` |
| DTOs | `DTOs/CircuitDto.cs`, `LoadDto.cs`, `LineDto.cs`, `GeneratorDto.cs`, `TransformerDto.cs`, `SlackDto.cs`, `SimulationResultDto.cs` |
| Leitura | `DTOs/ParameterReader.cs`, `DTOs/ElectricalValueParser.cs` |
| Construção | `DTOs/CircuitBuilder.cs` |
| Integração HTTP | `Infrastructure/Simulation/FastApiOpenDssGateway.cs`, `DTOs/SimulationApiClient.cs` |
| Resultado | `Service/SimulationResultApplier.cs`, `SimulationMessageBuilder.cs`, `SimulationExportService.cs` |
| Composição | `Service/Composition/SimulationComposition.cs`, `Service/EditorContext.cs` |
| UI | `Ribbon/Tabs/AnaliseTab.xaml.cs`, `Applications/Analisar/FluxoDeCorrente/*` |

# 3. Arquitetura da Simulação

## SimulationPipeline

Arquivo: `Applications/Simulation/SimulationPipeline.cs`

`SimulationPipeline` implementa `ISimulationPipeline`. O método real exposto é:

```csharp
Task<SimulationResultDto> ExecutarFluxoDeCorrenteAsync()
```

Responsabilidades:

| Etapa | Implementação |
|---|---|
| Construir DTO | `_circuitBuilder.Build()` |
| Chamar integração | `_gateway.SimularAsync(dto)` |
| Aplicar resultado | `_simulationResults.Apply(resultado)` |
| Retornar resultado | Retorna `SimulationResultDto` recebido |

O pipeline não contém lógica própria de OpenDSS, serialização HTTP ou atualização de campos de corrente. Ele apenas orquestra os serviços especializados.

## CircuitDtoBuilder

Arquivo: `Applications/Simulation/CircuitDtoBuilder.cs`

`CircuitDtoBuilder` recebe `AraciDocument` no construtor. Seu método `Build()` instancia:

```csharp
ParameterReader reader = new(_document);
CircuitBuilder builder = new(reader);
return builder.Build();
```

Portanto, o DTO de simulação é sempre derivado do documento atual.

## SimulationResult

Não foi encontrada uma classe chamada exatamente `SimulationResult`. O resultado tipado existente é `SimulationResultDto`, em `DTOs/SimulationResultDto.cs`. Ele contém status, mensagem, avisos, script DSS e resultados por linhas e cargas.

## SimulationResultApplier

Arquivo: `Service/SimulationResultApplier.cs`

`SimulationResultApplier` implementa `ISimulationResultApplier`. Ele recebe `AraciDocument` e um callback opcional `Action? notifyViewModels`.

Responsabilidades:

| Entrada | Ação |
|---|---|
| `SimulationResultDto.Lines` | Localiza `Cabo` por id e aplica correntes. |
| `SimulationResultDto.Loads` | Localiza `Carga` por id e aplica correntes. |
| Callback | Chama `_notifyViewModels?.Invoke()` ao final. |

## SimulationMessageBuilder

Arquivo: `Service/SimulationMessageBuilder.cs`

`SimulationMessageBuilder.Build(SimulationResultDto resultado, string? dssPath)` cria uma mensagem para diálogo WPF. A mensagem inclui:

| Campo | Uso |
|---|---|
| `resultado.Sucesso` | Define texto inicial e ícone. |
| `resultado.Mensagem` | Incluída se não estiver vazia. |
| `dssPath` | Inclui caminho do DSS salvo. |
| `resultado.Avisos` | Lista avisos. |
| `resultado.Script` | Inclui o script DSS gerado. |

O objeto retornado é `SimulationMessage`, com `Title`, `Text` e `MessageBoxImage Icon`.

## SimulationExportService

Arquivo: `Service/SimulationExportService.cs`

`SimulationExportService` salva arquivos de saída:

| Método | Responsabilidade |
|---|---|
| `GetPaths(FluxoDeCorrenteOptions options)` | Gera caminhos `.dss` e `_resultado.json`. |
| `Exists(SimulationExportPaths paths)` | Verifica se algum arquivo já existe. |
| `Save(FluxoDeCorrenteOptions options, SimulationResultDto resultado)` | Salva script DSS e JSON do resultado. |

`Save` escreve:

```text
{NomeArquivo}.dss
{NomeArquivo}_resultado.json
```

O script salvo no `.dss` é `resultado.Script`.

## Composição

Arquivo: `Service/Composition/SimulationComposition.cs`

`SimulationComposition.Create` instancia:

```text
SimulationResultApplier
FastApiOpenDssGateway
CircuitDtoBuilder
SimulationPipeline
SimulationExportService
SimulationMessageBuilder
```

`EditorContext`, em `Service/EditorContext.cs`, guarda esses objetos em propriedades:

```text
SimulationResults
Simulation
SimulationExport
SimulationMessages
```

# 4. ParameterReader

Arquivo: `DTOs/ParameterReader.cs`

`ParameterReader` é o componente que lê elementos do domínio e converte dados para estruturas intermediárias usadas por `CircuitBuilder`.

Construtores reais:

| Construtor | Serviços internos |
|---|---|
| `ParameterReader(CoreApi api)` | Cria `ConnectivityService`. |
| `ParameterReader(EditorContext context)` | Usa `CoreApi`, `ConnectivityService`, `TopologyValidator` e `context.ElectricGraph`. |
| `ParameterReader(AraciDocument document)` | Cria `CoreApi`, `ConnectivityService`, `TopologyValidator` e `ElectricGraphBuilder`. |

## ValidateTopology

`ValidateTopology()` retorna `_topology?.Validate()`. Essa validação é chamada por `CircuitBuilder.ValidarTopologia()` antes de construir o `CircuitDto`.

## GetLoads

`GetLoads()` constrói um `ElectricGraph` se `_graphBuilder` existir e percorre:

```csharp
_api.ObterElementos<Carga>()
    .Where(carga => carga.ParticipaDoGrafoEletrico)
```

Mapeamento para `LoadData`:

| Propriedade | Origem |
|---|---|
| `Id` | `carga.Id.ToString()` |
| `Nome` | `ReadString(carga, "Nome")` |
| `Barra` | `ResolverBarraEquipamento(carga, graph)` |
| `Fases` | `ReadInt(carga, "Fases")` |
| `R` | `ReadDouble(carga, "Carga resistencia", "Carga resistência")` |
| `X` | `ReadDouble(carga, "Carga reatancia", "Carga reatância")` |
| `PotenciaAtiva` | `ReadDouble(carga, "PotenciaAtiva")` |
| `PotenciaReativa` | `ReadDouble(carga, "PotenciaReativa")` |
| `Tensao` | `ReadKvWithDefault(carga, 12.47, "TensaoKV", "Tensao", "TensaoLinha")` |
| `Conexao` | `ReadString(carga, "Carga conexao", "Conexao")` |
| `Modelo` | `ReadInt(carga, "Carga modelo", "ModeloCarga", "Modelo")` |

## GetLines

`GetLines()` constrói `ElectricGraph` e percorre cabos topológicos:

```csharp
_api.ObterElementos<Cabo>()
    .Where(cabo => cabo.ParticipaDoGrafoEletrico)
```

Mapeamento para `LineData`:

| Propriedade | Origem |
|---|---|
| `Id` | `cabo.Id.ToString()` |
| `Nome` | `ReadString(cabo, "Nome")` |
| `Barra1` | `ResolverBus1(cabo, graph)` |
| `Barra2` | `ResolverBus2(cabo, graph)` |
| `Fases` | `ReadInt(cabo, "Fases")` |
| `Comprimento` | `ReadDouble(cabo, "Comprimento")` |
| `R1` | `ReadDouble(cabo, "R1", "Resistencia")` |
| `X1` | `ReadDouble(cabo, "X1", "Reatancia")` |
| `R0` | `ReadDouble(cabo, "R0")` |
| `X0` | `ReadDouble(cabo, "X0")` |
| `C1` | `ReadDouble(cabo, "C1")` |
| `C0` | `ReadDouble(cabo, "C0")` |

`ResolverBus1` e `ResolverBus2` tentam usar o grafo por `ResolverBusPorGrafo`. Se a aresta estiver inválida, retornam string vazia. Caso contrário, buscam o nó conectado e usam `ResolverBarraPorTerminal` para casos especiais de transformador ou `node.Name`.

## GetTransformers

`GetTransformers()` percorre `Transformador` topológicos. Diferente de cargas, linhas, geradores e SIN, este método não constrói grafo; ele resolve barras do transformador diretamente por terminal.

Mapeamento para `TransformerData`:

| Propriedade | Origem |
|---|---|
| `Id` | `transformador.Id.ToString()` |
| `Nome` | `ReadString(transformador, "Nome")` |
| `Fases` | `ReadIntWithDefault(transformador, 3, "Fases")` |
| `Enrolamentos` | `ReadIntWithDefault(transformador, 2, "Enrolamentos")` |
| `BarraPrimario` | `ResolverBarraTransformador(transformador, "PRIMARIO")` |
| `BarraSecundario` | `ResolverBarraTransformador(transformador, "SECUNDARIO")` |
| `TensaoPrimarioKV` | `ReadKvFromInstanceWithDefault(..., 13.8, ...)` |
| `TensaoSecundarioKV` | `ReadKvFromInstanceWithDefault(..., 0.38, ...)` |
| `PotenciaKVA` | `ReadPowerKvaFromInstance(transformador)` |
| `RPercentual` | `ReadDoubleWithDefault(..., 1, ...)` |
| `XPercentual` | `ReadDoubleWithDefault(..., 5, ...)` |
| `LigacaoPrimario` | `ReadStringWithDefault(..., "Wye", ...)` |
| `LigacaoSecundario` | `ReadStringWithDefault(..., "Wye", ...)` |

`ResolverBarraTerminalTransformador` retorna:

```text
{NomeBarramento(transformador)}_{PRIMARIO ou SECUNDARIO}
```

## GetGenerators

`GetGenerators()` constrói grafo e percorre `Gerador` topológicos.

Mapeamento para `GeneratorData`:

| Propriedade | Origem |
|---|---|
| `Id` | `gerador.Id.ToString()` |
| `Nome` | `ReadString(gerador, "Nome")` |
| `Barra` | `ResolverBarraEquipamento(gerador, graph)` |
| `Fases` | `ReadIntWithDefault(gerador, 3, "Fases")` |
| `Tensao` | `ReadKvWithDefault(gerador, 12.47, "TensaoKV", "Tensao", "TensaoLinha")` |
| `Potencia` | `ReadPositiveDoubleByPriority(gerador, "PotenciaAtiva", "Potencia", "PotenciaAparente")` |
| `FP` | `ReadDoubleWithDefault(gerador, 0.98, "FP", "FatorPotencia")` |

## GetSins

`GetSins()` constrói grafo e percorre `Sin` topológicos.

Mapeamento para `ExternalSourceData`:

| Propriedade | Origem |
|---|---|
| `Id` | `sin.Id.ToString()` |
| `Nome` | `ReadString(sin, "Nome")` |
| `Barra` | `ResolverBarraEquipamento(sin, graph)` |
| `Fases` | `ReadIntWithDefault(sin, 3, "Fases")` |
| `Tensao` | `ReadKvWithDefault(sin, 12.47, "TensaoKV", "Tensao", "TensaoLinha", "TensaoBaseKV")` |
| `PotenciaCurtoMVA` | `ReadDouble(sin, "PotenciaCurtoMVA", "PotenciaCurtoCircuitoMva")` |
| `RelacaoXR` | `ReadDouble(sin, "RelacaoXR", "X/R")` |

## Demais métodos encontrados

Métodos auxiliares relevantes:

| Método | Função |
|---|---|
| `ResolverBarraEquipamento` | Resolve barramento por grafo ou por `ConnectivityService`. |
| `ResolverBusPorGrafo` | Resolve barra da ponta de cabo por `ElectricGraphEdge`. |
| `ResolverBarraPorTerminal` | Trata terminais de `Transformador`. |
| `ReadString`, `ReadInt`, `ReadDouble`, `ReadKv` | Lê parâmetros de instância/tipo. |
| `ReadDoubleWithDefault`, `ReadKvWithDefault`, `ReadStringWithDefault` | Aplica fallback quando valor não existe ou não é positivo. |
| `ReadPositiveDoubleByPriority` | Lê o primeiro valor positivo entre nomes. |
| `ReadPowerKvaFromInstance` | Lê potência aparente/KVA do transformador com fallback 500. |

`ElectricalValueParser`, em `DTOs/ElectricalValueParser.cs`, normaliza texto numérico, troca vírgula por ponto, remove partes de ângulo polar e extrai o primeiro número por regex.

# 5. DTOs

## CircuitDto

Arquivo: `DTOs/CircuitDto.cs`

Propriedades:

| Propriedade | Tipo |
|---|---|
| `Loads` | `IList<LoadDto>` |
| `Lines` | `IList<LineDto>` |
| `Transformers` | `IList<TransformerDto>` |
| `Generators` | `IList<GeneratorDto>` |
| `Slack` | `SlackDto` |

## LoadDto

Arquivo: `DTOs/LoadDto.cs`

| Propriedade | Tipo |
|---|---|
| `Id` | `string` |
| `Nome` | `string` |
| `Barra` | `string` |
| `Fases` | `int` |
| `R` | `double` |
| `X` | `double` |
| `PotenciaAtiva` | `double` |
| `PotenciaReativa` | `double` |
| `Tensao` | `double` |
| `Conexao` | `string` |
| `Modelo` | `int` |

## GeneratorDto

Arquivo: `DTOs/GeneratorDto.cs`

| Propriedade | Tipo |
|---|---|
| `Id` | `string` |
| `Nome` | `string` |
| `Barra` | `string` |
| `Fases` | `int` |
| `Tensao` | `double` |
| `Potencia` | `double` |
| `FP` | `double` |

## TransformerDto

Arquivo: `DTOs/TransformerDto.cs`

| Propriedade | Tipo |
|---|---|
| `Id` | `string` |
| `Nome` | `string` |
| `Fases` | `int` |
| `Enrolamentos` | `int` |
| `BarraPrimario` | `string` |
| `BarraSecundario` | `string` |
| `TensaoPrimarioKV` | `double` |
| `TensaoSecundarioKV` | `double` |
| `PotenciaKVA` | `double` |
| `RPercentual` | `double` |
| `XPercentual` | `double` |
| `LigacaoPrimario` | `string` |
| `LigacaoSecundario` | `string` |

## LineDto

Arquivo: `DTOs/LineDto.cs`

| Propriedade | Tipo |
|---|---|
| `Id` | `string` |
| `Nome` | `string` |
| `Barra1` | `string` |
| `Barra2` | `string` |
| `Fases` | `int` |
| `Comprimento` | `double` |
| `R1` | `double` |
| `X1` | `double` |
| `R0` | `double` |
| `X0` | `double` |
| `C1` | `double` |
| `C0` | `double` |

## SinDto

Não foi encontrada classe `SinDto` no código. O elemento `Sin` é lido por `ParameterReader.GetSins()` como `ExternalSourceData` e, no `CircuitBuilder`, é convertido para `SlackDto` por `BuildSlack(ParameterReader.ExternalSourceData source)`.

## SlackDto

Arquivo: `DTOs/SlackDto.cs`

| Propriedade | Tipo |
|---|---|
| `Id` | `string` |
| `Nome` | `string` |
| `Tensao` | `double` |
| `Fases` | `int` |
| `Barra` | `string` |

## SimulationResultDto

Arquivo: `DTOs/SimulationResultDto.cs`

| Propriedade | Tipo |
|---|---|
| `Sucesso` | `bool` |
| `Script` | `string` |
| `Mensagem` | `string` |
| `Avisos` | `IList<string>` |
| `Lines` | `IList<LineResultDto>` |
| `Loads` | `IList<LoadResultDto>` |

`LineResultDto` e `LoadResultDto` possuem `Id`, `Nome`, `Corrente`, correntes por fase e ângulos por fase.

# 6. CircuitBuilder

Arquivo: `DTOs/CircuitBuilder.cs`

`CircuitBuilder` recebe `ParameterReader` e constrói `CircuitDto`. O método principal é `Build()`.

## Algoritmo de construção

```text
Build()
  |
  +-- ValidarTopologia()
  |
  +-- sins = _reader.GetSins()
  +-- generators = _reader.GetGenerators()
  |
  +-- slackSin = sins.FirstOrDefault()
  +-- slackGenerator = generators.FirstOrDefault()
  |
  +-- se não há SIN nem gerador:
  |       throw "Nenhuma fonte slack encontrada no circuito."
  |
  +-- CircuitDto
        Loads = BuildLoads()
        Lines = BuildLines()
        Transformers = BuildTransformers()
        Generators = BuildGenerators(...)
        Slack = BuildSlack(SIN ou Gerador)
  |
  +-- Validar(dto)
```

## Escolha do slack

Regra real:

| Condição | Resultado |
|---|---|
| Existe ao menos um `Sin` | O primeiro `Sin` vira `SlackDto`. |
| Não existe `Sin`, mas existe `Gerador` | O primeiro `Gerador` vira `SlackDto`. |
| Não existe `Sin` nem `Gerador` | Exceção `InvalidOperationException`. |

Quando existe `Sin`, todos os geradores lidos continuam em `Generators`. Quando não existe `Sin`, o primeiro gerador é usado como slack e os demais entram em `Generators` por `generators.Skip(1)`.

## Regras de DTO

`BuildLoads()` aplica:

| Campo | Fallback |
|---|---|
| `Nome` | `"Carga"` |
| `Barra` | fallback no nome da carga |
| `Fases` | 3 |
| `PotenciaAtiva` | 800 se não positiva |
| `PotenciaReativa` | 300 se negativa |
| `Tensao` | 12.47 se não positiva |
| `Conexao` | `"Wye"` |
| `Modelo` | 1 |

`BuildLines()` aplica:

| Campo | Fallback |
|---|---|
| `Nome` | `"L1"` |
| `Fases` | 3 |
| `Comprimento` | 1 |
| `R1` | 0.1 |
| `X1` | 0.2 |
| `R0` | `3 * r1` |
| `X0` | `3 * x1` |
| `C1` | 3.4 |
| `C0` | 1.6 |

`BuildTransformers()` aplica:

| Campo | Fallback |
|---|---|
| `Fases` | 3 |
| `Enrolamentos` | 2 |
| `TensaoPrimarioKV` | 12.47 via `SafeKv`, embora `ParameterReader` use 13.8 antes |
| `TensaoSecundarioKV` | 12.47 via `SafeKv`, embora `ParameterReader` use 0.38 antes |
| `PotenciaKVA` | 500 |
| `RPercentual` | 1 se negativo |
| `XPercentual` | 5 se negativo |
| `LigacaoPrimario` | `"Wye"` |
| `LigacaoSecundario` | `"Wye"` |

`BuildGenerators()` aplica:

| Campo | Fallback |
|---|---|
| `Nome` | `"Gerador"` |
| `Barra` | fallback no nome |
| `Fases` | 3 |
| `Tensao` | 12.47 |
| `Potencia` | 1000 |
| `FP` | 0.98 |

## Validação final

`Validar(CircuitDto dto)` verifica:

| Condição | Exceção |
|---|---|
| `dto.Slack` nulo ou `Slack.Barra` vazio | `"Nenhuma fonte slack encontrada no circuito."` |
| Linha sem `Barra1` ou `Barra2` | `"Cabo '{line.Nome}' sem barra origem/destino definida."` |

`ValidarTopologia()` chama `ParameterReader.ValidateTopology()` e lança:

```text
Topologia invalida para simulacao:
{result.FormatErrors()}
```

# 7. Integração OpenDSS

A integração com OpenDSS não aparece como chamada direta a biblioteca local. O código local envia `CircuitDto` para uma API HTTP e recebe um resultado que pode conter script DSS.

Classes reais:

| Classe | Arquivo | Papel |
|---|---|---|
| `FastApiOpenDssGateway` | `Infrastructure/Simulation/FastApiOpenDssGateway.cs` | Implementa `ISimulationGateway` delegando para `SimulationApiClient`. |
| `SimulationApiClient` | `DTOs/SimulationApiClient.cs` | Serializa `CircuitDto`, faz POST HTTP e desserializa resposta. |

`FastApiOpenDssGateway.SimularAsync(CircuitDto circuit)` chama:

```csharp
_client.SimularTipadoAsync(circuit)
```

`FastApiOpenDssGateway.SimularTextoAsync(CircuitDto circuit)` chama:

```csharp
_client.SimularAsync(circuit)
```

Evidências OpenDSS:

| Evidência | Código |
|---|---|
| Nome do gateway | `FastApiOpenDssGateway` |
| Script DSS retornado | `SimulationResultDto.Script` |
| Campos de resposta aceitos | `script`, `dss_script` |
| Exportação DSS | `SimulationExportService.Save` grava `resultado.Script` em `.dss` |
| Janela de fluxo | `FluxoDeCorrenteWindow` pede pasta para salvar arquivo DSS |

# 8. FastAPI

Não foi encontrada implementação FastAPI local no projeto. O que existe é o cliente HTTP que presume um endpoint.

## Endpoint utilizado

Em `DTOs/SimulationApiClient.cs`:

```csharp
private const string DefaultSimulationUrl = "http://127.0.0.1:8000/simular";
```

O método `SimularAsync(CircuitDto dto)` executa:

```text
POST http://127.0.0.1:8000/simular
Content-Type: application/json
Body: CircuitDto serializado em snake_case
```

## Payload de request

`JsonSerializerOptions` usado no request:

```text
PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
```

Portanto, propriedades como `PotenciaAtiva` são serializadas em `potencia_ativa`.

## Payload de response

`SimulationApiClient.DeserializeResult` aceita formatos flexíveis:

| Informação | Nomes aceitos |
|---|---|
| Status textual | `status` com `ok` ou `success` |
| Status booleano | `sucesso`, `success`, `converged` |
| Script | `script`, `dss_script` |
| Mensagem | `mensagem`, `message`, `erro`, `error` |
| Avisos | `avisos`, `warnings`, `mensagens`, `messages` |
| Resultado aninhado | `resultado`, `result` |
| Linhas | `lines`, `linhas`, `lineResults`, `line_results` |
| Cargas | `loads`, `cargas`, `loadResults`, `load_results` |
| Elementos genéricos | `elementos`, `elements` |

Não há no código local definição de rotas FastAPI, modelos Pydantic, serviço Python ou execução do OpenDSS.

# 9. Resultados

## SimulationResultDto

`SimulationResultDto` é preenchido por `SimulationApiClient.DeserializeResult`. O método:

1. Lê sucesso por `ReadStatus`.
2. Lê script por `ReadString(root, "script", "dss_script")`.
3. Grava script em `C:\Temp\araci_script_debug.txt` por `WriteScriptDebug`.
4. Lê mensagem e avisos.
5. Se existir `resultado` ou `result`, lê também dentro desse objeto.
6. Tenta ler resultados de linhas e cargas na raiz.
7. Se necessário, tenta ler linhas e cargas dentro de `resultRoot`.
8. Se ainda faltar linhas ou cargas, tenta ler resultados por `elementos`/`elements`.

## Aplicação dos resultados

`SimulationResultApplier.Apply(SimulationResultDto resultado)`:

```text
resultado.Lines
    |
    v
procura Cabo por Id
    |
    v
aplica CorrenteLinha, CorrenteFaseA/B/C

resultado.Loads
    |
    v
procura Carga por Id
    |
    v
aplica CorrenteLinha, CorrenteFaseA/B/C
```

O formato aplicado é polar:

```text
{magnitude:0.##}∠{angle:0.##}°
```

Fallbacks de ângulo:

| Campo | Fallback |
|---|---|
| Linha | 0 |
| Fase A | 0 |
| Fase B | -120 |
| Fase C | 120 |

## Atualização visual

`SimulationComposition.Create` cria `SimulationResultApplier(document, notifySimulationResultViewModels)`. Em `EditorContext.NotifySimulationResultViewModels`, quando há `Viewport`, o código percorre `Viewport.Elementos` e, para VMs cujo modelo é `Cabo` ou `Carga`, chama:

```csharp
vm.NotificarPropriedades(
    "CorrenteLinha",
    "CorrenteFaseA",
    "CorrenteFaseB",
    "CorrenteFaseC");
```

Assim, a atualização visual ocorre por notificação de propriedades nos ViewModels após alteração do modelo.

# 10. Fluxo Completo

Fluxo completo do botão de análise até os resultados:

```text
Ribbon/Tabs/AnaliseTab.xaml
    |
    v
FluxoButton_Click
    |
    +-- abre FluxoDeCorrenteWindow
    +-- coleta PastaSaida e NomeArquivo
    |
    v
FluxoDeCorrenteApplication.ExecutarAsync(options)
    |
    v
SimulationPipeline.ExecutarFluxoDeCorrenteAsync()
    |
    v
CircuitDtoBuilder.Build()
    |
    v
ParameterReader + CircuitBuilder
    |
    +-- TopologyValidator
    +-- ElectricGraphBuilder
    +-- ConnectivityService
    |
    v
CircuitDto
    |
    v
FastApiOpenDssGateway.SimularAsync()
    |
    v
SimulationApiClient.SimularTipadoAsync()
    |
    v
POST /simular
    |
    v
JSON de resposta
    |
    v
SimulationResultDto
    |
    v
SimulationResultApplier.Apply()
    |
    +-- atualiza Cabo
    +-- atualiza Carga
    +-- notifica ViewModels
    |
    v
SimulationExportService.Save()
    |
    +-- grava .dss
    +-- grava _resultado.json
    |
    v
SimulationMessageBuilder.Build()
    |
    v
DialogService.Show()
```

Fluxo técnico simplificado:

```text
AraciDocument
  -> ElectricGraphBuilder (dentro do ParameterReader)
  -> ParameterReader
  -> CircuitBuilder
  -> CircuitDto
  -> FastAPI HTTP /simular
  -> OpenDSS externo inferido pelo gateway/script
  -> SimulationResultDto
  -> SimulationResultApplier
  -> Modelos e ViewModels
```

# 11. Regras Elétricas

## Slack

Regras reais em `CircuitBuilder.Build()`:

| Condição | Slack |
|---|---|
| Há `Sin` | Primeiro `Sin` retornado por `GetSins()`. |
| Não há `Sin`, há `Gerador` | Primeiro `Gerador` retornado por `GetGenerators()`. |
| Não há fonte | Exceção. |

`TopologyValidator` também exige que haja `Sin` ou `Gerador`; caso contrário, adiciona erro `"Circuito sem fonte slack."`.

## Barras

As barras usadas nos DTOs são strings. Elas são resolvidas por:

| Contexto | Regra |
|---|---|
| Equipamento | `ResolverBarraEquipamento` tenta grafo e `ConnectivityService`. |
| Cabo | `ResolverBusPorGrafo`; fallback por endpoint/`ConnectivityService`. |
| Transformador | `{NomeBarramento}_{PRIMARIO}` e `{NomeBarramento}_{SECUNDARIO}`. |
| Fallback em DTO | `SafeBus(value, fallback)` usa nome ou `"BARRA-001"`. |

## Conexões

Antes do DTO final, `CircuitBuilder.ValidarTopologia()` usa `TopologyValidator`. Cabos inválidos, ids inexistentes, terminais inexistentes, duplicidades e outras falhas topológicas são reportadas por esse validador e bloqueiam a simulação com exceção.

Além disso, `CircuitBuilder.Validar(dto)` rejeita linhas sem barra de origem ou destino.

## Transformadores

Transformadores geram `TransformerDto` com primário e secundário separados. As barras são determinadas por terminal:

```text
{NomeBarramento(transformador)}_PRIMARIO
{NomeBarramento(transformador)}_SECUNDARIO
```

Parâmetros lidos incluem fases, enrolamentos, tensões, potência, resistência percentual, reatância percentual e ligações.

## Cargas

Cargas viram `LoadDto`. Se potência ativa não for positiva, `CircuitBuilder` usa 800. Se potência reativa for negativa, usa 300. Conexão vazia vira `"Wye"` e modelo vazio/não positivo vira `1`.

## Geradores

Quando não há `Sin`, o primeiro gerador vira slack. Quando há `Sin`, todos os geradores permanecem em `GeneratorDto`. Quando o primeiro gerador vira slack, os demais são enviados como geradores por `generators.Skip(1)`.

# 12. Tratamento de Erros

## Validação topológica

`CircuitBuilder.ValidarTopologia()` lança `InvalidOperationException` quando `TopologyValidationResult.IsValid` é falso:

```text
Topologia invalida para simulacao:
- ...
```

## Ausência de slack

Se `CircuitBuilder` não encontra `Sin` nem `Gerador`, lança:

```text
Nenhuma fonte slack encontrada no circuito.
```

## Linhas sem barra

`CircuitBuilder.Validar` lança:

```text
Cabo '{line.Nome}' sem barra origem/destino definida.
```

## Falhas HTTP

`SimulationApiClient.SimularAsync` trata:

| Condição | Exceção |
|---|---|
| HTTP sem sucesso | `HttpRequestException` com status, reason phrase e resposta. |
| Timeout | `InvalidOperationException("Tempo limite excedido ao chamar a API de simulacao.", ex)` |
| Outras exceções | `InvalidOperationException("Erro ao enviar circuito para a API de simulacao.", ex)` |

O `HttpClient` padrão tem timeout de 30 segundos.

## Falhas de UI/exportação

`FluxoDeCorrenteApplication.ExecuteCoreAsync` captura exceções gerais e chama:

```csharp
_dialogs.ShowWarning("Fluxo de corrente", ex.Message)
```

`SalvarArquivos` também captura exceções e mostra:

```text
A simulação foi aplicada, mas não foi possível salvar os arquivos.
```

## Validação de opções de exportação

`FluxoDeCorrenteWindow` rejeita:

| Condição | Mensagem |
|---|---|
| Pasta vazia ou inexistente | `"Selecione uma pasta de saída válida."` |
| Nome de arquivo vazio | `"Informe o nome do arquivo DSS."` |

Algumas mensagens em arquivos lidos aparecem com encoding corrompido, como `"saÃ­da"`.

# 13. Dívidas Técnicas

As dívidas abaixo são baseadas em evidência direta do código.

## Não há implementação FastAPI/OpenDSS no repositório analisado

O front-end presume `http://127.0.0.1:8000/simular`, mas não há servidor FastAPI, modelo Python ou script OpenDSS local no código lido. Isso torna a integração dependente de um serviço externo não versionado neste workspace.

## URL fixa

`SimulationApiClient` define `DefaultSimulationUrl` como constante. Não foi observado uso de configuração de ambiente, arquivo de settings ou UI para alterar o endpoint.

## Escrita fixa de debug

`SimulationApiClient.WriteScriptDebug` grava sempre em:

```text
C:\Temp\araci_script_debug.txt
```

Isso cria dependência local de caminho e efeito colateral em toda desserialização de resultado.

## Contrato de resposta muito flexível

`DeserializeResult` aceita muitos nomes alternativos para os mesmos campos. Isso aumenta tolerância, mas também torna o contrato menos explícito e dificulta saber qual é o formato canônico da API.

## `SinDto` não existe

Embora o domínio tenha `Sin`, o DTO público de circuito não possui `SinDto`. O `Sin` é convertido em `SlackDto`. Isso é coerente com a escolha de slack, mas deve ser conhecido por quem procurar um DTO específico de SIN.

## Regras de simulação distribuídas

Parte das regras está em `ParameterReader`, parte em `CircuitBuilder`, parte em `TopologyValidator`, parte em `SimulationApiClient`. A simulação funciona por composição, mas as regras elétricas e defaults não estão centralizados em um único módulo.

## Fallbacks elétricos embutidos

Defaults como 12.47 kV, 800 kW, 300 kvar, R1 0.1, X1 0.2, C1 3.4 e C0 1.6 estão embutidos em `CircuitBuilder`. Não foi observado catálogo externo ou configuração para esses valores.

## Tratamento parcial de resultados

`SimulationResultApplier` aplica resultados apenas em `Cabo` e `Carga`. Não foi observado código aplicando resultados de tensão, potência, transformadores, geradores ou `Sin`.

# 14. Comparação com Arquitetura-Alvo

A implementação atual entrega uma integração funcional para fluxo de corrente por API externa. Ela constrói um DTO de circuito, valida topologia, chama um gateway HTTP, recebe resultados e atualiza correntes no modelo. Para uma arquitetura de simulação CAD/BIM elétrica mais madura, alguns limites ficam claros.

| Aspecto | Estado atual | Arquitetura desejada |
|---|---|---|
| Construção do circuito | `ParameterReader` + `CircuitBuilder`. | Separação mais explícita entre leitura, validação, normalização e montagem. |
| Topologia | Validada antes do DTO por `TopologyValidator`. | Integração mais direta entre `ElectricGraph` e DTO final. |
| OpenDSS | Acessado indiretamente por API HTTP; script retornado. | Contrato versionado e documentado, com servidor versionado junto ao projeto ou pacote definido. |
| FastAPI | Apenas endpoint cliente `/simular` é conhecido. | Rotas, schemas e erros compartilhados entre front-end e backend. |
| Resultados | Correntes aplicadas a cabos e cargas. | Aplicação estruturada para tensões, perdas, carregamentos, fontes e transformadores. |
| Configuração | URL fixa e defaults embutidos. | Configuração por ambiente/projeto e defaults por catálogo elétrico. |
| Exportação | Salva script DSS e JSON de resultado. | Exportação rastreável com metadados, versão de modelo e parâmetros de simulação. |

O desenho atual tem uma qualidade importante: a simulação consome a topologia por ids e terminais, não apenas por nomes textuais. `ParameterReader` usa `ElectricGraphBuilder` e `ConnectivityService` para resolver barramentos, e `CircuitBuilder` bloqueia a simulação quando a topologia é inválida.

O principal ponto de evolução é tornar o contrato com FastAPI/OpenDSS explícito e versionado. Hoje o código do cliente aceita vários formatos de resposta e presume uma URL local. Para a arquitetura-alvo, o ideal seria estabilizar o contrato de request/response, centralizar defaults elétricos, remover caminhos fixos de debug e ampliar a aplicação de resultados para além de correntes em cabos e cargas.
