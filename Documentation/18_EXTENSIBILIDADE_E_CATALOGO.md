# 1. Introdução

Este documento descreve o mecanismo atual de extensibilidade e catálogo do Araci, com base exclusivamente no código real analisado. O foco é explicar como novos tipos de elementos entram no sistema, quais classes participam do fluxo, quais integrações são acionadas e quais limites arquiteturais existem hoje.

A extensibilidade é importante porque o Araci é uma plataforma CAD/BIM elétrica 2D em evolução. Mesmo no escopo atual, elementos como `Barra`, `Cabo`, `Carga`, `Gerador`, `Sin` e `Transformador` precisam ser criados, renderizados, inseridos no Ribbon, persistidos, consultados, conectados e eventualmente lidos pela simulação. Um novo elemento não é apenas uma classe de modelo: ele precisa participar de um conjunto coordenado de serviços.

Este documento separa explicitamente:

- **Implementado**: mecanismos encontrados no código atual.
- **Planejado**: direções documentadas em `README.md`, `Documentation/01_VISAO_GERAL_DO_PRODUTO.md` e `Documentation/16_ARQUITETURA_ALVO_E_PLANO_DE_EVOLUCAO.md`, sem tratá-las como existentes.

Não foram encontrados mecanismos de plugin dinâmico, carregamento externo de extensões, catálogo de fabricantes persistente ou biblioteca externa de componentes. O mecanismo implementado hoje é um catálogo interno em memória, baseado em `ElementDefinition`, `ElementDefinitionsProvider`, `ElementRegistryService`, factories, tipos padrão e metadados de Ribbon.

# 2. Visão Geral

## Implementado

Novos elementos entram no sistema por meio de definições registradas em `ElementRegistryService`. Cada `ElementDefinition` descreve o `Kind`, o modelo, o ViewModel, o tipo de elemento, a criação de instâncias, o tamanho, a atualização de terminais, os metadados de Ribbon e as propriedades de instância.

Fluxo implementado:

```text
+----------------------------+
| TypeLibraryService         |
| Tipos padrão em memória    |
+-------------+--------------+
              |
              v
+----------------------------+
| ElementDefinitionsProvider |
| CreateDefaults()           |
+-------------+--------------+
              |
              v
+----------------------------+
| ElementDefinition          |
| Kind + Modelo + VM + Tipo  |
+-------------+--------------+
              |
              v
+----------------------------+
| ElementRegistryService     |
| Registro por Kind          |
+-------------+--------------+
              |
      +-------+--------+-------------------+
      |                |                   |
      v                v                   v
+-----------+   +-------------+    +----------------+
| Ribbon    |   | Factories   |    | Persistência   |
| botões    |   | modelos/VMs |    | Kind/Type      |
+-----------+   +-------------+    +----------------+
      |                |                   |
      v                v                   v
+---------------------------------------------------+
| Inserção, Scene, Topologia, Simulação, Propriedades|
+---------------------------------------------------+
```

O composition root cria esse conjunto em `Service/Composition/EditorCoreComposition.cs`:

```text
TypeLibraryService
    |
    v
ElementInstancePropertyProvider
    |
    v
ElementDefinitionsProvider
    |
    v
ElementRegistryService(definitions.CreateDefaults())
    |
    v
InstancePropertyCatalog.Configure(elements)
```

## Planejado

O projeto documenta evolução futura para catálogo de componentes, bibliotecas e fabricantes. Essa capacidade não está implementada como mecanismo externo no código atual. Hoje, o catálogo é interno, fixo em código e inicializado em memória.

# 3. ElementDefinition

## Implementado

`Applications/Abstractions/ElementDefinition.cs` define a classe `ElementDefinition`, no namespace `Araci.Applications.Abstractions`. Ela é o contrato central de descrição de um elemento registrável no sistema.

Responsabilidades observadas:

- identificar um elemento por `Kind`;
- associar nome amigável e prefixo de nome;
- vincular tipo de modelo, ViewModel e tipo técnico;
- fornecer delegates para criar modelo e ViewModel;
- fornecer tipo padrão e lista de tipos;
- fornecer tamanho geométrico;
- fornecer ação de atualização de terminais;
- fornecer metadados de Ribbon;
- indicar se usa ferramenta especial;
- declarar propriedades de instância.

Propriedades reais:

| Propriedade | Tipo | Papel |
| --- | --- | --- |
| `Kind` | `string` | Identificador usado por catálogo, factories, Ribbon, persistência e ferramentas. |
| `NomeAmigavel` | `string` | Nome legível do elemento. |
| `PrefixoNome` | `string` | Prefixo usado por `NameService`. |
| `ModelType` | `Type` | Tipo da classe de modelo. |
| `ViewModelType` | `Type` | Tipo da classe ViewModel. |
| `TypeModelType` | `Type` | Tipo da classe de tipo técnico, derivada de `TipoElemento`. |
| `Ribbon` | `ElementRibbonMetadata` | Metadados de exibição no Ribbon. |
| `NomeRibbon` | `string` | Atalho para `Ribbon.Nome`. |
| `CategoriaRibbon` | `string` | Atalho para `Ribbon.Categoria`. |
| `Icone` | `string` | Caminho normalizado do ícone. |
| `OrdemRibbon` | `int` | Ordem de exibição. |
| `ExibirNoRibbon` | `bool` | Indica se aparece no Ribbon. |
| `Atalho` | `string` | Atalho textual normalizado. |
| `UsaFerramentaEspecial` | `bool` | Indica uso de ferramenta específica. |
| `PropriedadesInstancia` | `IReadOnlyList<InstancePropertyDescriptor>` | Propriedades exibidas/editáveis por instância. |
| `CriarModelo` | `Func<Elemento>` | Delegate de criação do modelo. |
| `CriarViewModel` | `Func<Elemento, ElementViewModelFactoryContext, ElementoViewModel?>` | Delegate de criação do ViewModel. |
| `ObterTipoPadrao` | `Func<TipoElemento?>` | Obtém tipo padrão. |
| `ObterTipos` | `Func<IEnumerable<TipoElemento>>` | Obtém tipos disponíveis. |
| `ObterTamanho` | `Func<Elemento, Size>` | Obtém tamanho geométrico. |
| `AtualizarTerminais` | `Action<Elemento>` | Atualiza terminais do elemento. |

Métodos reais:

- `AceitaModelo(Elemento elemento)`: verifica `ModelType.IsInstanceOfType(elemento)`.
- `AceitaViewModel(ElementoViewModel viewModel)`: verifica `ViewModelType.IsInstanceOfType(viewModel)`.

## Papel arquitetural

`ElementDefinition` funciona como a unidade de extensão implementada. Ao registrar uma definição, o sistema passa a saber:

- como criar o modelo;
- como criar a apresentação;
- como listar no Ribbon;
- qual tipo padrão usar;
- quais terminais atualizar;
- como calcular tamanho;
- quais propriedades de instância expor;
- como mapear o elemento para persistência por `Kind`.

# 4. ElementRegistryService

## Implementado

`Service/ElementRegistryService.cs` define `ElementRegistryService`, no namespace `Araci.Services`. Ele implementa `IElementCatalog`, definido em `Applications/Abstractions/IElementCatalog.cs`.

O serviço mantém:

- `_porKind`: `Dictionary<string, ElementDefinition>` com `StringComparer.OrdinalIgnoreCase`;
- `_definitions`: `List<ElementDefinition>`.

Constantes reais:

| Constante | Valor |
| --- | --- |
| `KindBarra` | `ElementKinds.Barra` |
| `KindCarga` | `ElementKinds.Carga` |
| `KindGerador` | `ElementKinds.Gerador` |
| `KindSin` | `ElementKinds.Sin` |
| `KindTransformador` | `ElementKinds.Transformador` |
| `KindCabo` | `ElementKinds.Cabo` |

`ElementKinds`, em `Applications/Abstractions/ElementKinds.cs`, define:

- `Barra = "Barra"`;
- `Carga = "Carga"`;
- `Gerador = "Gerador"`;
- `Sin = "Sin"`;
- `Transformador = "Transformador"`;
- `Cabo = "Cabo"`.

## Registro

O construtor `ElementRegistryService(IEnumerable<ElementDefinition> definitions)` percorre as definições recebidas e chama `Register(definition)`.

`Register(ElementDefinition definition)`:

- rejeita `null`;
- impede `Kind` duplicado;
- adiciona a definição em `_porKind`;
- adiciona a definição em `_definitions`.

Se um `Kind` já existir, lança `InvalidOperationException` com a mensagem `"Elemento ja registrado: {definition.Kind}."`.

## Consulta e resolução por Kind

Métodos reais:

| Método | Responsabilidade |
| --- | --- |
| `FindByKind(string kind)` | Resolve definição pelo `Kind`, ignorando maiúsculas/minúsculas. |
| `FindByShortcut(string shortcut)` | Resolve definição pelo atalho do Ribbon. |
| `FindByModel(Elemento elemento)` | Resolve definição pelo tipo do modelo. |
| `FindByModelType<T>()` | Resolve definição por tipo de modelo. |
| `FindByViewModel(ElementoViewModel viewModel)` | Resolve definição pelo tipo do ViewModel. |
| `FindByViewModelType(Type viewModelType)` | Resolve definição por tipo de ViewModel. |
| `GetKind(Elemento elemento)` | Retorna `Kind` da definição ou `elemento.GetType().Name`. |
| `GetNamePrefix(Elemento elemento)` | Retorna `PrefixoNome` ou `"ELM"`. |
| `GetTypes(string kind)` | Retorna tipos associados ao `Kind`. |
| `GetDefaultType(string kind)` | Retorna tipo padrão associado ao `Kind`. |
| `ResolveType(string kind, string? nomeTipo, string? familia, string? categoria)` | Resolve tipo por nome/família/categoria ou retorna tipo padrão. |
| `GetSize(Elemento elemento)` | Usa `ElementDefinition.ObterTamanho` ou fallback. |
| `UpdateTerminals(Elemento elemento)` | Usa `ElementDefinition.AtualizarTerminais`. |
| `GetInstanceProperties(...)` | Retorna propriedades de instância por ViewModel ou tipo. |
| `GetCommonInstanceProperties(...)` | Calcula propriedades comuns entre ViewModels. |
| `CanEditAcrossMixedTypes(...)` | Verifica edição comum em seleção mista. |

## Integração com demais sistemas

`ElementRegistryService` é consumido por:

- `ElementoModelFactory`, para criar modelo por `Kind`;
- `ElementoViewModelFactory`, para criar ViewModel por modelo;
- `ToolService`, para ativar ferramenta por `Kind`;
- `InputRouter`, para resolver atalhos de teclado;
- `ElementGeometryService`, para obter tamanho;
- `TerminalLayoutService`, para atualizar terminais;
- `ProjectSerializer`, para persistir `Kind` e resolver tipos;
- `ElectricGraphBuilder`, para registrar `Kind` em nós;
- `NameService`, para prefixos de nome;
- `InstancePropertyCatalog`, para propriedades de instância.

# 5. ElementDefinitionsProvider

## Implementado

`Applications/Factories/ElementDefinitionsProvider.cs` define `ElementDefinitionsProvider`. Ele recebe:

- `TypeLibraryService`;
- `ElementInstancePropertyProvider`.

O método `CreateDefaults()` usa `yield return` para criar as definições padrão do catálogo interno atual.

## Catálogo padrão atual

| Kind | Nome | Prefixo | Modelo | ViewModel | Tipo | Ícone | Ordem | Atalho | Ferramenta especial |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Cabo` | `Cabo` | `CABO` | `Cabo` | `CaboViewModel` | `TipoCabo` | `cabo.png` | 10 | `CB` | `true` |
| `Carga` | `Carga` | `CARGA` | `Carga` | `CargaViewModel` | `TipoCarga` | `carga.png` | 20 | `CG` | `false` |
| `Gerador` | `Gerador` | `GERADOR` | `Gerador` | `GeradorViewModel` | `TipoGerador` | `gerador.png` | 30 | `GE` | `false` |
| `Sin` | `SIN` | `SIN` | `Sin` | `SinViewModel` | `TipoSin` | `sin.png` | 40 | `SI` | `false` |
| `Transformador` | `Transformador` | `TR` | `Transformador` | `TransformadorViewModel` | `TipoTransformador` | `transformador.png` | 50 | `TR` | `false` |
| `Barra` | `Barra` | `BARRA` | `Barra` | `BarraViewModel` | `TipoBarra` | `barra.png` | 60 | `BA` | `false` |

## Processo de criação das definições

Cada definição informa:

- função privada de criação do modelo, como `CriarCabo()`, `CriarCarga()` ou `CriarBarra()`;
- delegate de criação do ViewModel;
- função para tipo padrão, como `_types.TipoCaboPadrao`;
- função para lista de tipos, como `_types.TiposCabos`;
- função de tamanho;
- ação de atualização de terminais;
- metadados de Ribbon criados pelo helper `Ribbon(...)`;
- propriedades de instância vindas de `ElementInstancePropertyProvider`.

As funções privadas de criação de modelos atribuem o tipo padrão e lançam `InvalidOperationException` quando o tipo padrão não existe. Exemplo: `CriarCabo()` retorna `new Cabo { Tipo = _types.TipoCaboPadrao ?? throw new InvalidOperationException("Nenhum tipo de cabo cadastrado.") }`.

# 6. Factories

## ElementoFactory

`Applications/Factories/ElementoFactory.cs` define `ElementoFactory`. Ela combina:

- `IElementModelFactory`;
- `IElementViewModelFactory`.

Responsabilidades:

- criar modelos por `Kind`;
- criar modelos tipados;
- criar ViewModel a partir de modelo;
- criar ViewModel tipado por `Kind`;
- criar conveniências específicas: `CriarCabo`, `CriarCaboVM`, `CriarCarga`, `CriarCargaVM`, `CriarGerador`, `CriarGeradorVM`, `CriarSin`, `CriarSinVM`, `CriarTransformador`, `CriarTransformadorVM`, `CriarBarra`, `CriarBarraVM`.

## ElementoModelFactory

`Applications/Factories/ElementoModelFactory.cs` implementa `IElementModelFactory`.

Fluxo de `CreateModel(string kind)`:

```text
kind
  |
  v
IElementCatalog.FindByKind(kind)
  |
  v
ElementDefinition.CriarModelo()
  |
  v
ElementDefinition.AtualizarTerminais(elemento)
  |
  v
Elemento
```

Se o `Kind` não estiver registrado, lança `InvalidOperationException` com `"Elemento nao registrado: {kind}."`.

`CreateModel<TModel>(string kind)` valida se o modelo criado é do tipo esperado.

## ElementoViewModelFactory

`Applications/Factories/ElementoViewModelFactory.cs` implementa `IElementViewModelFactory`.

Dependências:

- `IElementCatalog`;
- `IElementModelFactory`;
- `NameService`;
- `TypePropertiesDialogService`;
- `TerminalLayoutService`;
- `ElementGeometryUpdateService`.

O construtor cria `ElementViewModelFactoryContext`, que contém:

- `NameService Names`;
- `TypePropertiesDialogService TypePropertiesDialogs`;
- `TerminalLayoutService TerminalLayout`.

`CreateViewModel(Elemento modelo)` resolve o `Kind` com `_catalog.GetKind(modelo)`, encontra a definição e chama `definition.CriarViewModel(modelo, _context)`.

`ConfigurarViewModel(...)` possui tratamento específico: quando o ViewModel é `BarraViewModel`, atribui `barra.GeometryUpdates = _geometryUpdates`.

## Fluxo conjunto

```text
ElementoFactory.CriarViewModel(kind)
      |
      v
ElementoModelFactory.CreateModel(kind)
      |
      v
ElementDefinition.CriarModelo()
      |
      v
ElementDefinition.AtualizarTerminais()
      |
      v
ElementoViewModelFactory.CreateViewModel(modelo)
      |
      v
ElementDefinition.CriarViewModel(modelo, context)
```

# 7. TypeLibraryService

## Implementado

`Service/TypeLibraryService.cs` define `TypeLibraryService`. Ele mantém coleções em memória de tipos técnicos por família de elemento.

Coleções reais:

| Propriedade | Tipo |
| --- | --- |
| `TiposCabos` | `ObservableCollection<TipoCabo>` |
| `TiposCargas` | `ObservableCollection<TipoCarga>` |
| `TiposGeradores` | `ObservableCollection<TipoGerador>` |
| `TiposSin` | `ObservableCollection<TipoSin>` |
| `TiposTransformadores` | `ObservableCollection<TipoTransformador>` |
| `TiposBarras` | `ObservableCollection<TipoBarra>` |

Tipos padrão:

- `TipoCaboPadrao`;
- `TipoCargaPadrao`;
- `TipoGeradorPadrao`;
- `TipoSinPadrao`;
- `TipoTransformadorPadrao`;
- `TipoBarraPadrao`.

O construtor chama:

- `InicializarCabos()`;
- `InicializarCargas()`;
- `InicializarGeradores()`;
- `InicializarSin()`;
- `InicializarTransformadores()`;
- `InicializarBarras()`.

## Tipos e referências

Os tipos derivam de `Models/Tipos/TipoElemento.cs`, que define parâmetros base:

- `NomeTipo`;
- `Familia`;
- `Categoria`;
- dicionário `Parametros`.

`ElementDefinitionsProvider` usa `TypeLibraryService` para:

- atribuir tipo padrão ao criar modelos;
- fornecer lista de tipos em `ElementDefinition.ObterTipos`;
- fornecer `TypeModelType`.

`ProjectSerializer` persiste a referência de tipo por `TypeRefDto`, contendo `NomeTipo`, `Familia` e `Categoria`, e reconstitui com `ElementRegistryService.ResolveType(...)`.

## Limite atual

Não foi observado catálogo externo, persistência própria da biblioteca de tipos ou importação de bibliotecas de fabricantes. Os tipos padrão são inicializados em memória no código.

# 8. TerminalLayoutService

## Implementado

`Service/TerminalLayoutService.cs` define `TerminalLayoutService`. Ele possui dois construtores:

- `TerminalLayoutService(ElementGeometryService geometry)`;
- `TerminalLayoutService(ElementRegistryService registry, ElementGeometryService geometry)`.

Quando possui registry, `AtualizarTerminais(Elemento elemento)` chama `_registry.UpdateTerminals(elemento)`. Se o registry conseguir atualizar, o método retorna. Caso contrário, usa fallback por `switch`:

- `Barra`: `barra.AtualizarTerminais(...)`;
- `Carga`: `carga.AtualizarTerminais(...)`;
- `Gerador`: `gerador.AtualizarTerminais(...)`;
- `Sin`: `sin.AtualizarTerminais(...)`;
- `Transformador`: `transformador.AtualizarTerminais(...)`;
- `Cabo`: atualiza origem/destino com base nos vértices.

Há também `AtualizarTerminais(Barra barra, IReadOnlySet<string>? terminaisProtegidos)`.

## Papel na extensibilidade

Para um novo elemento, a atualização de terminais deve ser definida na `ElementDefinition`. Isso permite que `TerminalLayoutService` delegue a regra ao catálogo, em vez de depender apenas do `switch` interno.

Impactos:

- inserção de elementos chama atualização de terminais;
- persistência chama atualização antes de restaurar terminais;
- geometria e edição dependem de terminais consistentes;
- topologia depende de `ITerminalOwner` e terminais estáveis.

# 9. ElementGeometryService

## Implementado

`Service/ElementGeometryService.cs` define `ElementGeometryService`. Ele pode operar com ou sem `ElementRegistryService`.

`ObterTamanho(Elemento elemento)`:

- se há registry, retorna `_registry.GetSize(elemento)`;
- caso contrário, usa fallback:
  - `Barra`: `ElementGeometryDefaults.BarraLargura` e `barra.Altura`;
  - `ElementoEquipamento`: `ElementGeometryDefaults.EquipamentoLargura` e `ElementGeometryDefaults.EquipamentoAltura`;
  - `ElementoLinear`: `Size.Empty`;
  - demais: tamanho de equipamento.

`CalcularTopoEsquerdoPorCentro(Elemento elemento, Point centro)` calcula posição de topo esquerdo a partir do centro e do tamanho.

## Papel geométrico

Para novos elementos, a geometria entra pelo delegate `ElementDefinition.ObterTamanho`. Esse delegate alimenta:

- layout de terminais;
- persistência de posição visual de terminais;
- inserção;
- consultas visuais;
- atualização geométrica.

# 10. Fluxo Completo de Inclusão

## Implementado

Fluxo técnico atual para um novo elemento no padrão existente:

```text
Novo Modelo
    |
    v
Novo Tipo
    |
    v
Novo ViewModel
    |
    v
Nova ElementDefinition
    |
    v
ElementRegistryService
    |
    v
Ribbon
    |
    v
Factories
    |
    v
Persistência
    |
    v
Topologia
    |
    v
Simulação
```

Passo a passo baseado no código:

1. Criar uma classe de modelo derivada de `Elemento`, `ElementoEquipamento` ou `ElementoLinear`.
2. Definir `DomainRole` conforme participação no domínio: `Grafico`, `Anotacao` ou `EletricoTopologico`.
3. Se o elemento possuir terminais, implementar `ITerminalOwner`.
4. Criar ou reutilizar uma classe de tipo derivada de `TipoElemento`.
5. Adicionar coleção e tipo padrão em `TypeLibraryService`, caso o elemento tenha tipos próprios.
6. Criar ViewModel derivado de `ElementoViewModel`.
7. Criar descritores de propriedade em `ElementInstancePropertyProvider`, se houver propriedades de instância.
8. Criar `ElementDefinition` em `ElementDefinitionsProvider.CreateDefaults()`.
9. Definir `Kind` em `ElementKinds`, se o elemento deve ser usado de forma padronizada.
10. Definir metadados de Ribbon com `ElementRibbonMetadata`.
11. Definir função de tamanho em `ObterTamanho`.
12. Definir função de atualização de terminais em `AtualizarTerminais`.
13. Garantir que `ElementoModelFactory` consiga criar o modelo pelo `Kind`.
14. Garantir que `ElementoViewModelFactory` consiga criar o ViewModel.
15. Verificar persistência por `ProjectSerializer`, que usa `Kind` e `_modelFactory.CreateModel(dto.Kind)`.
16. Se o elemento participar da topologia, garantir `ParticipaDoGrafoEletrico`, `ITerminalOwner` e terminais.
17. Se o elemento participar da simulação, atualizar `ParameterReader` e `CircuitBuilder`, pois hoje eles leem tipos concretos.

Diagrama de impacto:

```text
ElementDefinition
      |
      +--> RibbonDefinitions
      |
      +--> ElementoModelFactory
      |
      +--> ElementoViewModelFactory
      |
      +--> ElementGeometryService
      |
      +--> TerminalLayoutService
      |
      +--> ProjectSerializer
      |
      +--> ElectricGraphBuilder
      |
      +--> ParameterReader / CircuitBuilder
```

# 11. Impacto em Persistência

## Implementado

A persistência está em `Infrastructure/Persistence/ProjectSerializer.cs` e usa `Infrastructure/Persistence/ProjectFileDto.cs`.

O DTO genérico `ElementDto` contém:

- `Kind`;
- `DomainRole`;
- `Id`;
- `X`;
- `Y`;
- `Rotation`;
- `Scale`;
- `Type`;
- `Parameters`;
- `Terminals`;
- `Vertices`.

Durante salvamento:

- `ProjectSerializer.CriarElementoDto(...)` chama `_elements.GetKind(elemento)`;
- cria `TypeRefDto` a partir de `elemento.Tipo`;
- serializa parâmetros;
- serializa terminais quando o elemento implementa `ITerminalOwner`;
- serializa vértices quando o elemento é `Cabo`.

Durante abertura:

- `ProjectSerializer.CriarElemento(ElementDto dto)` chama `_modelFactory.CreateModel(dto.Kind)`;
- restaura identidade, posição, rotação, escala e tipo;
- aplica parâmetros;
- chama `_terminalLayout.AtualizarTerminais(elemento)`;
- restaura terminais por `Id`.

## Implicação para novos elementos

Um novo elemento precisa ter `Kind` resolvível pelo registry e modelo criável por `ElementoModelFactory`. Se o `Kind` não estiver registrado, a abertura falha por criação de modelo inexistente.

Se o elemento possuir parâmetros novos, eles serão persistidos genericamente desde que existam em `elemento.Parametros`. Na abertura, `AplicarParametros(...)` ignora parâmetros que não existam no modelo reconstruído.

# 12. Impacto em Topologia

## Implementado

A topologia é afetada pela extensibilidade por meio de:

- `Elemento.DomainRole`;
- `Elemento.ParticipaDoGrafoEletrico`;
- `ITerminalOwner`;
- `TerminalLayoutService`;
- `ConnectivityService`;
- `ElectricGraphBuilder`.

`ElectricGraphBuilder.IsNodeElement(...)` considera nó elétrico quando:

```text
elemento.ParticipaDoGrafoEletrico
    &&
elemento is ITerminalOwner
    &&
elemento is not Cabo
```

Cabos são tratados como arestas:

```text
_document.Elementos
    .OfType<Cabo>()
    .Where(c => c.ParticipaDoGrafoEletrico)
```

Isso significa que, no estado atual, apenas `Cabo` é reconhecido como aresta elétrica pelo builder. Um novo elemento linear topológico não seria automaticamente tratado como aresta, a menos que o código do `ElectricGraphBuilder` fosse alterado.

## Implicações

Para um novo elemento topológico de equipamento:

- deve implementar `ITerminalOwner`;
- deve ter `DomainRole == EletricoTopologico`;
- deve ter terminais criados de forma estável;
- deve ser reconhecido pelo registry.

Para um novo elemento com comportamento de cabo/linha:

- o código atual está especializado em `Cabo`;
- `ParameterReader.GetLines()` também usa `ObterElementos<Cabo>()`;
- a simulação não absorve automaticamente outro tipo linear.

# 13. Impacto em Simulação

## Implementado

A simulação atual não usa `ElementDefinition` para descobrir tipos simuláveis. Ela usa tipos concretos em `DTOs/ParameterReader.cs`:

- `GetLoads()` lê `Carga`;
- `GetLines()` lê `Cabo`;
- `GetTransformers()` lê `Transformador`;
- `GetGenerators()` lê `Gerador`;
- `GetSins()` lê `Sin`.

`DTOs/CircuitBuilder.cs` monta:

- `LoadDto`;
- `LineDto`;
- `TransformerDto`;
- `GeneratorDto`;
- `SlackDto`;
- `CircuitDto`.

## Implicações

Um novo elemento aparece no catálogo, no Ribbon, nas factories, na persistência e possivelmente na topologia, mas não entra automaticamente na simulação. Para participar da simulação, o código atual exige alteração explícita no `ParameterReader` e, se necessário, no `CircuitBuilder` e nos DTOs.

Essa é uma fronteira importante: o catálogo interno atual resolve criação e apresentação, mas a simulação ainda é baseada em classes concretas conhecidas.

# 14. Catálogo Futuro

## Planejado

O `README.md` e os documentos de arquitetura indicam evolução para:

- catálogo de elementos;
- bibliotecas;
- bibliotecas de fabricantes;
- componentes e propriedades técnicas mais ricos.

Esses itens são direções arquiteturais, não mecanismos implementados no código atual.

O que existe hoje:

- catálogo interno em memória;
- tipos padrão inicializados em código;
- `ElementDefinition` como contrato de definição;
- `ElementRegistryService` como registry;
- metadados de Ribbon;
- propriedades de instância.

O que não foi encontrado:

- carregamento dinâmico de catálogos externos;
- plugins de elementos;
- arquivos de biblioteca de fabricantes;
- persistência própria de biblioteca de tipos;
- marketplace;
- importação automática de famílias ou símbolos;
- versionamento de catálogo.

Direção arquitetural compatível:

```text
Planejado:

Catálogo de componentes
    |
    +--> tipos técnicos
    +--> símbolos
    +--> propriedades
    +--> fabricantes
    +--> validações
    +--> simulação
```

Essa direção deve preservar o contrato central do modelo: elementos continuam precisando de identidade, parâmetros, tipo, geometria, terminais quando aplicável, persistência e participação controlada em topologia/simulação.

# 15. Boas Práticas para Novos Elementos

Checklist baseado no mecanismo implementado:

- Definir um `Kind` único e estável.
- Adicionar o `Kind` em `ElementKinds` quando o elemento for parte do catálogo padrão.
- Criar modelo derivado de `Elemento`, `ElementoEquipamento` ou `ElementoLinear`.
- Definir `DomainRole` corretamente.
- Implementar `ITerminalOwner` se o elemento tiver terminais.
- Garantir IDs de terminais estáveis.
- Criar tipo técnico derivado de `TipoElemento` quando o elemento possuir tipo próprio.
- Registrar coleção e tipo padrão em `TypeLibraryService` quando necessário.
- Criar ViewModel correspondente.
- Criar `ElementDefinition` em `ElementDefinitionsProvider`.
- Informar `ModelType`, `ViewModelType` e `TypeModelType`.
- Informar `CriarModelo` com tipo padrão válido.
- Informar `CriarViewModel`.
- Informar `ObterTipoPadrao` e `ObterTipos`.
- Informar `ObterTamanho`.
- Informar `AtualizarTerminais`.
- Informar `ElementRibbonMetadata`, com nome, ícone, ordem e atalho.
- Definir `UsaFerramentaEspecial` somente quando houver ferramenta especial real.
- Adicionar propriedades de instância em `ElementInstancePropertyProvider`, se necessário.
- Verificar criação por `ElementoModelFactory.CreateModel(kind)`.
- Verificar criação por `ElementoViewModelFactory.CreateViewModel(modelo)`.
- Verificar exibição no Ribbon por `Elements.RibbonDefinitions`.
- Verificar persistência e abertura por `ProjectSerializer`.
- Verificar topologia se o elemento for elétrico.
- Verificar simulação se o elemento deve gerar DTO.
- Adicionar checks técnicos compatíveis quando a mudança impactar topologia, persistência ou simulação.

# 16. Riscos Arquiteturais

Riscos baseados em evidências reais:

| Risco | Evidência |
| --- | --- |
| Catálogo interno fixo em código | `ElementDefinitionsProvider.CreateDefaults()` define os elementos padrão diretamente. |
| Ausência de plugin real | Não foram encontradas implementações ou uso de `IFeatureModule`; também não há carregador de plugins. |
| Tipos padrão apenas em memória | `TypeLibraryService` inicializa coleções no construtor. |
| Simulação especializada em classes concretas | `ParameterReader` usa `ObterElementos<Carga>`, `Cabo`, `Transformador`, `Gerador` e `Sin`. |
| Topologia trata apenas `Cabo` como aresta | `ElectricGraphBuilder` usa `_document.Elementos.OfType<Cabo>()`. |
| Persistência depende de `Kind` registrado | `ProjectSerializer` abre arquivo com `_modelFactory.CreateModel(dto.Kind)`. |
| ViewModel ainda participa de alguns use cases | `InserirCaboUseCase` cria `CaboViewModel`; `EditarPropriedadesUseCase` opera sobre `ElementoViewModel`. |
| Registry impede duplicidade, mas não há isolamento por módulo | `Register` bloqueia `Kind` duplicado globalmente. |
| Propriedades de instância acopladas a ViewModel | `InstancePropertyDescriptor.OwnerType` aponta para tipos de ViewModel. |
| Caracteres corrompidos em textos | Textos como `TensÃ£o` aparecem em `ElementInstancePropertyProvider`. |

# 17. Conclusão

## Implementado

O mecanismo atual já fornece uma base sólida para crescimento porque centraliza a descrição dos elementos em `ElementDefinition` e distribui essa informação para registry, factories, Ribbon, geometria, terminais, propriedades, persistência e parte da topologia. Para o escopo atual, isso permite que os elementos principais do Araci sejam tratados de forma relativamente uniforme.

A combinação de `ElementDefinitionsProvider`, `ElementRegistryService`, `ElementoModelFactory`, `ElementoViewModelFactory`, `ElementoFactory`, `TypeLibraryService`, `TerminalLayoutService` e `ElementGeometryService` forma o núcleo real de extensibilidade implementado.

## Planejado

A evolução para catálogo de componentes, bibliotecas e fabricantes ainda não está implementada. Quando essa evolução ocorrer, ela deve respeitar os contratos já existentes: `Kind`, modelo, ViewModel, tipo, geometria, terminais, propriedades, persistência, topologia e simulação. O desenho atual é uma boa fundação, mas ainda precisará de contratos mais explícitos, persistência de catálogo, versionamento e integração controlada para suportar uma plataforma CAD/BIM elétrica mais ampla.

