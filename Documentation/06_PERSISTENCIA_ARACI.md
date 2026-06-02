# 1. Introdução

A persistência do Araci tem como objetivo transformar o estado atual de um `AraciDocument` em uma representação serializável, armazená-la em arquivo e reconstruir posteriormente o mesmo documento de trabalho. No código atual, essa responsabilidade está concentrada na camada de aplicação e infraestrutura, com integração direta com o documento de domínio, com o histórico de comandos e com serviços auxiliares de interface para seleção de arquivos e exibição de mensagens.

O formato persistido observado no código é um arquivo textual JSON, normalmente salvo com extensão `.araci`. A implementação atual usa DTOs genéricos de projeto, elemento, tipo, parâmetro, terminal e ponto, em vez de DTOs especializados por cada classe de domínio. Assim, elementos como `Barra`, `Cabo`, `Carga`, `Gerador`, `Sin` e `Transformador` são persistidos por meio de um `ElementDto`, distinguido principalmente pela propriedade `Kind`, pelas referências de tipo, pelos parâmetros, pelos terminais e, no caso de cabos, pelos vértices.

Os principais arquivos envolvidos são:

| Arquivo | Responsabilidade observada |
| --- | --- |
| `Applications/Projects/ProjectPersistenceService.cs` | Orquestra novo projeto, salvar, abrir, diálogos, tratamento de erros, limpeza de estado transitório e histórico. |
| `Infrastructure/Persistence/ProjectSerializer.cs` | Converte `AraciDocument` para DTOs, serializa JSON, desserializa JSON e reconstrói elementos. |
| `Infrastructure/Persistence/ProjectFileDto.cs` | Define os DTOs persistidos no JSON. |
| `Infrastructure/Persistence/IProjectRepository.cs` | Abstrai leitura e escrita textual do arquivo. |
| `Infrastructure/Persistence/FileSystemProjectRepository.cs` | Implementa leitura e escrita via sistema de arquivos. |
| `Infrastructure/Persistence/ProjectFileDialogService.cs` | Abre diálogos WPF para arquivos `.araci`, `.json` e outros. |
| `Applications/Abstractions/IProjectPersistenceService.cs` | Contrato de persistência usado pela aplicação. |
| `Applications/Abstractions/IProjectFileDialogService.cs` | Contrato para seleção de arquivos. |
| `Service/Composition/PersistenceComposition.cs` | Monta os serviços de persistência no contexto da aplicação. |
| `Araci.TechnicalChecks/Program.cs` | Contém verificações técnicas de reload, estabilidade de IDs, topologia e DTOs de simulação após persistência. |

A persistência atual não é apenas uma operação de arquivo. Ela participa diretamente da continuidade topológica e operacional do projeto, pois preserva identificadores, parâmetros de conexão, terminais e vértices que são posteriormente usados por serviços como `ConnectivityService`, `ElectricGraphBuilder`, `ParameterReader` e `CircuitBuilder`.

# 2. Visão Geral

O fluxo principal de persistência é:

```text
+----------------+
| AraciDocument  |
+-------+--------+
        |
        | ProjectSerializer.CreateFileDto(...)
        v
+----------------+
| ProjectFileDto |
+-------+--------+
        |
        | System.Text.Json
        v
+----------------+
| JSON textual   |
+-------+--------+
        |
        | FileSystemProjectRepository.WriteAllText(...)
        v
+----------------+
| Arquivo .araci |
+----------------+
```

O fluxo inverso observado no código é:

```text
+----------------+
| Arquivo .araci |
+-------+--------+
        |
        | FileSystemProjectRepository.ReadAllText(...)
        v
+----------------+
| JSON textual   |
+-------+--------+
        |
        | ProjectSerializer.Deserialize(...)
        v
+----------------+
| ProjectFileDto |
+-------+--------+
        |
        | ProjectSerializer.CreateElements(...)
        v
+----------------+
| Elementos      |
+-------+--------+
        |
        | AraciDocument.Limpar()
        | AraciDocument.AdicionarElemento(...)
        v
+----------------+
| AraciDocument  |
+----------------+
```

O documento de domínio persistido é `Core/Documents/AraciDocument.cs`, no namespace `Araci.Core.Documents`. Ele contém a coleção `ObservableCollection<Elemento> Elementos` e expõe os métodos `AdicionarElemento`, `RemoverElemento` e `Limpar`. A persistência serializa o conteúdo de `Elementos` e, ao abrir um arquivo, recria a coleção do documento por meio de modelos construídos pelo `IElementModelFactory`.

O serviço de aplicação `ProjectPersistenceService`, no namespace `Araci.Applications.Projects`, é o ponto de orquestração. Ele não serializa diretamente o documento. Em vez disso, delega a conversão para `ProjectSerializer`, a leitura e gravação para `IProjectRepository`, os diálogos para `IProjectFileDialogService` e as mensagens ao usuário para `IUserDialogService`.

# 3. Arquitetura da Persistência

A arquitetura de persistência atual está organizada em três blocos principais: contrato de aplicação, orquestração e infraestrutura de arquivo/serialização.

```text
+---------------------------------------------+
| Applications.Abstractions                    |
|                                             |
| IProjectPersistenceService                   |
| IProjectFileDialogService                    |
+----------------------+----------------------+
                       |
                       v
+---------------------------------------------+
| Applications.Projects                        |
|                                             |
| ProjectPersistenceService                    |
+----------------------+----------------------+
                       |
       +---------------+----------------+
       |                                |
       v                                v
+-----------------------+      +--------------------------+
| ProjectSerializer     |      | IProjectRepository       |
| ProjectFileDto        |      | FileSystemProjectRepository |
+-----------------------+      +--------------------------+
       |
       v
+-----------------------+
| JSON / arquivo .araci |
+-----------------------+
```

## ProjectPersistenceService

`Applications/Projects/ProjectPersistenceService.cs` define a classe `ProjectPersistenceService`. Ela implementa `IProjectPersistenceService` e concentra os casos de uso de projeto:

| Método | Responsabilidade real |
| --- | --- |
| `Novo()` | Limpa o documento, limpa estado transitório, limpa histórico de comandos, zera caminho atual e redefine metadados. |
| `SalvarComDialogo()` | Solicita caminho pelo diálogo e chama `Salvar(path)` se houver seleção. |
| `AbrirComDialogo()` | Solicita caminho pelo diálogo e chama `Abrir(path)` se houver seleção. |
| `Salvar(string path)` | Prepara metadados, cria DTO, serializa JSON e grava arquivo. |
| `Abrir(string path)` | Lê arquivo, desserializa DTO, recria elementos, substitui documento e atualiza metadados. |

O construtor recebe:

| Dependência | Papel no serviço |
| --- | --- |
| `AraciDocument document` | Documento em memória que será salvo ou substituído ao abrir arquivo. |
| `ICommandHistory commands` | Histórico de comandos a ser limpo em novo projeto e após abertura. |
| `ProjectSerializer serializer` | Conversão entre documento, DTOs e JSON. |
| `IProjectRepository repository` | Leitura e escrita textual. |
| `IProjectFileDialogService fileDialog` | Seleção de caminho de abertura/salvamento. |
| `IUserDialogService dialogs` | Exibição de erros e avisos. |
| `Action limparEstadoTransitorio` | Limpeza de estado visual/transitório da aplicação. |

A classe mantém `_metadata` e `_currentPath`. O valor inicial de `_metadata` vem de `ProjectMetadataDto.CreateNew(ProjectSerializer.UntitledProjectName)`. O caminho atual é atualizado em `Salvar(path)` e `Abrir(path)`.

## ProjectSerializer

`Infrastructure/Persistence/ProjectSerializer.cs` define a classe `ProjectSerializer`, no namespace `Araci.Infrastructure.Persistence`. Ela é responsável por:

- criar `ProjectFileDto` a partir de `AraciDocument`;
- serializar `ProjectFileDto` como JSON;
- desserializar JSON para `ProjectFileDto`;
- recriar elementos a partir de `ElementDto`;
- preparar metadados para salvamento;
- criar metadados a partir de arquivo aberto;
- converter parâmetros entre valores tipados e `JsonElement`;
- restaurar terminais, vértices, tipos e parâmetros.

As constantes reais observadas são:

| Constante | Valor |
| --- | --- |
| `CurrentVersion` | `1` |
| `AppName` | `"Araci Engine"` |
| `UntitledProjectName` | `"Sem titulo"` |

O serializer usa `JsonSerializerOptions` com:

| Opção | Valor observado |
| --- | --- |
| `WriteIndented` | `true` |
| `DefaultIgnoreCondition` | `JsonIgnoreCondition.WhenWritingNull` |

Não há política de nomenclatura JSON configurada. Portanto, as propriedades serializadas mantêm os nomes PascalCase dos DTOs C#, como `Version`, `AppName`, `ProjectName`, `Elements`, `Kind`, `DomainRole`, `Parameters`, `Terminals` e `Vertices`.

## ProjectFileDto

O documento persistido não possui uma classe chamada `ProjectDto` no código analisado. A classe real equivalente é `ProjectFileDto`, definida em `Infrastructure/Persistence/ProjectFileDto.cs`. Ela representa a raiz do arquivo JSON.

## Serviços auxiliares

`IProjectRepository` e `FileSystemProjectRepository` separam o mecanismo de persistência textual do serviço de aplicação. A implementação concreta usa:

- `File.ReadAllText(path)`;
- `File.WriteAllText(path, content)`.

`ProjectFileDialogService` define o filtro:

```text
Projeto Araci (*.araci)|*.araci|JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*
```

No salvamento, o diálogo usa `DefaultExt = ".araci"` e `AddExtension = true`. Na abertura, usa `DefaultExt = ".araci"` e `CheckFileExists = true`.

`Service/Composition/PersistenceComposition.cs` possui o método `CreateProjects(...)`, que instancia `ProjectSerializer`, `FileSystemProjectRepository`, `ProjectFileDialogService` e `ProjectPersistenceService`, conectando a persistência ao contexto da aplicação.

# 4. Formato .araci

O formato `.araci` observado no código é JSON textual indentado. A extensão é tratada por `ProjectFileDialogService` como extensão padrão de projeto, mas o filtro também permite `.json` e todos os arquivos. A gravação em si é feita por `FileSystemProjectRepository.WriteAllText`, sem validação explícita da extensão.

A raiz JSON corresponde a `ProjectFileDto`. A estrutura real é:

| Campo JSON | Origem no DTO | Tipo C# |
| --- | --- | --- |
| `Version` | `ProjectFileDto.Version` | `int` |
| `AppName` | `ProjectFileDto.AppName` | `string?` |
| `ProjectName` | `ProjectFileDto.ProjectName` | `string?` |
| `CreatedAt` | `ProjectFileDto.CreatedAt` | `DateTimeOffset?` |
| `SavedAt` | `ProjectFileDto.SavedAt` | `DateTimeOffset?` |
| `Generator` | `ProjectFileDto.Generator` | `string?` |
| `Notes` | `ProjectFileDto.Notes` | `string?` |
| `Elements` | `ProjectFileDto.Elements` | `List<ElementDto>` |

Como `DefaultIgnoreCondition` é `WhenWritingNull`, propriedades nulas podem ser omitidas do JSON gerado.

Exemplo estrutural baseado nos DTOs reais:

```json
{
  "Version": 1,
  "AppName": "Araci Engine",
  "ProjectName": "Projeto",
  "CreatedAt": "2026-06-01T12:00:00+00:00",
  "SavedAt": "2026-06-01T12:00:00+00:00",
  "Generator": "Araci Engine",
  "Elements": [
    {
      "Kind": "Cabo",
      "DomainRole": "EletricoTopologico",
      "Id": "00000000-0000-0000-0000-000000000000",
      "X": 0,
      "Y": 0,
      "Rotation": 0,
      "Scale": 1,
      "Type": {
        "NomeTipo": "Tipo",
        "Familia": "Familia",
        "Categoria": "Categoria"
      },
      "Parameters": [
        {
          "Name": "OrigemId",
          "Type": "System.Guid",
          "Value": "00000000-0000-0000-0000-000000000000"
        }
      ],
      "Terminals": [
        {
          "Id": "ORIGEM",
          "X": 0,
          "Y": 0,
          "Barra": "barra"
        }
      ],
      "Vertices": [
        {
          "X": 0,
          "Y": 0
        }
      ]
    }
  ]
}
```

O exemplo acima representa apenas a forma estrutural suportada pelos DTOs. O conteúdo concreto de `Kind`, `Parameters`, `Terminals`, `Type` e `Vertices` depende dos elementos criados pelo domínio e dos parâmetros existentes em cada modelo.

Não há no código analisado um empacotamento binário, manifesto separado, compressão, checksum, assinatura, schema JSON externo ou armazenamento segmentado por entidade. O arquivo é um JSON único contendo metadados e lista de elementos.

# 5. DTOs de Persistência

Os DTOs de persistência estão em `Infrastructure/Persistence/ProjectFileDto.cs`, no namespace `Araci.Infrastructure.Persistence`.

A varredura do código identificou os seguintes DTOs reais de persistência:

| DTO real | Arquivo | Responsabilidade |
| --- | --- | --- |
| `ProjectFileDto` | `Infrastructure/Persistence/ProjectFileDto.cs` | Raiz do arquivo persistido. |
| `ProjectMetadataDto` | `Infrastructure/Persistence/ProjectFileDto.cs` | Metadados mantidos pelo serviço durante a sessão e aplicados ao arquivo. |
| `ElementDto` | `Infrastructure/Persistence/ProjectFileDto.cs` | Representação genérica de elemento persistido. |
| `TypeRefDto` | `Infrastructure/Persistence/ProjectFileDto.cs` | Referência ao tipo associado ao elemento. |
| `ParameterDto` | `Infrastructure/Persistence/ProjectFileDto.cs` | Representação serializável de parâmetros do modelo. |
| `TerminalDto` | `Infrastructure/Persistence/ProjectFileDto.cs` | Representação serializável de terminais. |
| `PointDto` | `Infrastructure/Persistence/ProjectFileDto.cs` | Representação serializável de pontos geométricos. |

Também foram procurados os nomes solicitados `ProjectDto`, `BarraDto` e `CaboDto`. Eles não existem como classes, records ou structs de persistência no código analisado. O papel de `ProjectDto` é exercido pela classe real `ProjectFileDto`. `Barra` e `Cabo` são persistidos por `ElementDto`, com distinção por `Kind`, parâmetros, terminais e, para `Cabo`, `Vertices`.

## ProjectDto

Não há uma classe real chamada `ProjectDto` na persistência. O DTO raiz encontrado é `ProjectFileDto`. Assim, qualquer documentação do conceito de projeto persistido precisa apontar para `ProjectFileDto`, não para um `ProjectDto` inexistente.

## ProjectFileDto

`ProjectFileDto` é a raiz do arquivo persistido.

| Propriedade | Tipo | Observação |
| --- | --- | --- |
| `Version` | `int` | Versão do formato. O serializer atual usa `CurrentVersion = 1`. |
| `AppName` | `string?` | Nome da aplicação gravado no arquivo. |
| `ProjectName` | `string?` | Nome do projeto. |
| `CreatedAt` | `DateTimeOffset?` | Data de criação lógica do arquivo/projeto. |
| `SavedAt` | `DateTimeOffset?` | Data da gravação. |
| `Generator` | `string?` | Gerador do arquivo. |
| `Notes` | `string?` | Observações opcionais. |
| `Elements` | `List<ElementDto>` | Lista de elementos persistidos. |

## ProjectMetadataDto

`ProjectMetadataDto` representa os metadados mantidos pelo serviço durante a sessão.

| Propriedade | Tipo | Valor padrão observado |
| --- | --- | --- |
| `AppName` | `string` | `ProjectSerializer.AppName` |
| `ProjectName` | `string` | `ProjectSerializer.UntitledProjectName` |
| `CreatedAt` | `DateTimeOffset?` | `null` |
| `SavedAt` | `DateTimeOffset?` | `null` |
| `Generator` | `string` | `ProjectSerializer.AppName` |
| `Notes` | `string?` | `null` |

O método estático `CreateNew(string projectName)` usa `ProjectSerializer.AppName` e aplica `UntitledProjectName` quando o nome informado é vazio.

## ElementDto

`ElementDto` é o DTO genérico para todos os elementos. Não foram encontradas classes `CaboDto`, `BarraDto`, `CargaDto`, `GeradorDto`, `SinDto` ou `TransformadorDto` na persistência.

| Propriedade | Tipo | Papel |
| --- | --- | --- |
| `Kind` | `string` | Identificador de tipo de elemento usado por `IElementModelFactory.CreateModel(dto.Kind)`. |
| `DomainRole` | `string?` | Papel de domínio serializado a partir de `elemento.DomainRole.ToString()`. |
| `Id` | `Guid` | Identidade persistida do elemento. |
| `X` | `double` | Posição X (`PosicaoX`). |
| `Y` | `double` | Posição Y (`PosicaoY`). |
| `Rotation` | `double` | Rotação (`Rotacao`). |
| `Scale` | `double` | Escala (`Escala`). Valor padrão `1`. |
| `Type` | `TypeRefDto?` | Referência de tipo do elemento. |
| `Parameters` | `List<ParameterDto>` | Parâmetros do elemento. |
| `Terminals` | `List<TerminalDto>` | Terminais, quando o elemento implementa `ITerminalOwner`. |
| `Vertices` | `List<PointDto>` | Vértices, usados pelo `Cabo`. |

## BarraDto

Não há `BarraDto` real no código de persistência. Uma instância de `Models/Barra.cs` é salva como `ElementDto`. Os dados específicos observáveis da barra são preservados por:

- `Kind`, obtido por `ElementRegistryService.GetKind(elemento)`;
- `Id`, `X`, `Y`, `Rotation` e `Scale`;
- `Type`, quando existe `TipoBarra`;
- `Parameters`, incluindo parâmetros reais do modelo como `Altura` e `Tensao`;
- `Terminals`, contendo os terminais da barra com `Id`, `X`, `Y` e `Barra`.

Na abertura, a normalização específica observada para barra ocorre em `ProjectSerializer.NormalizarParametros(...)`, que executa `barra.Altura = barra.Altura`, acionando a normalização da propriedade `Altura` definida em `Models/Barra.cs`.

## CaboDto

Não há `CaboDto` real no código de persistência. Uma instância de `Models/Cabo.cs` é salva como `ElementDto`. Os dados específicos observáveis do cabo são preservados por:

- `Kind`, obtido por `ElementRegistryService.GetKind(elemento)`;
- `Id`, `X`, `Y`, `Rotation` e `Scale`;
- `Type`, quando existe `TipoCabo`;
- `Parameters`, incluindo `OrigemId`, `DestinoId`, `OrigemTerminalId`, `DestinoTerminalId`, `BarraOrigem`, `BarraDestino`, `Comprimento`, `Ampacidade`, tensões e correntes definidas no modelo;
- `Terminals`, contendo os terminais `ORIGEM` e `DESTINO` quando existentes;
- `Vertices`, contendo os pontos da polilinha do cabo.

Na abertura, `ProjectSerializer.AplicarVertices(...)` é a rotina específica que recompõe a geometria do cabo a partir de `PointDto`, limpa `PreviewPonto` e chama `DefinirOrigem(...)` e `DefinirDestino(...)` quando há vértices suficientes.

## TypeRefDto

`TypeRefDto` persiste a referência ao tipo do elemento:

| Propriedade | Tipo |
| --- | --- |
| `NomeTipo` | `string?` |
| `Familia` | `string?` |
| `Categoria` | `string?` |

Durante a abertura, `ProjectSerializer.ResolverTipo(...)` usa `_elements.ResolveType(kind, dto?.NomeTipo, dto?.Familia, dto?.Categoria)`.

## ParameterDto

`ParameterDto` representa cada entrada de `elemento.Parametros.Values`.

| Propriedade | Tipo | Origem |
| --- | --- | --- |
| `Name` | `string` | `parameter.Nome` |
| `Type` | `string` | `parameter.Tipo.FullName ?? parameter.Tipo.Name` |
| `Value` | `JsonElement` | `JsonSerializer.SerializeToElement(parameter.ValorObjeto, parameter.Tipo)` |

Na abertura, a restauração procura um parâmetro existente com o mesmo nome em `elemento.Parametros`. Se o nome não existir no modelo recriado, o parâmetro persistido é ignorado.

## TerminalDto

`TerminalDto` persiste dados visuais e de identificação dos terminais:

| Propriedade | Tipo | Origem |
| --- | --- | --- |
| `Id` | `string` | `terminal.Id` |
| `X` | `double` | `terminal.Posicao.X` |
| `Y` | `double` | `terminal.Posicao.Y` |
| `Barra` | `string?` | `terminal.Barra` |

Na abertura, a restauração procura um terminal existente no elemento recriado com o mesmo `Id`. Terminais persistidos sem correspondente não são recriados dinamicamente pelo serializer.

## PointDto

`PointDto` representa pontos geométricos:

| Propriedade | Tipo |
| --- | --- |
| `X` | `double` |
| `Y` | `double` |

Ele é usado principalmente em `ElementDto.Vertices` para preservar os vértices de `Cabo`.

# 6. Processo de Salvamento

O salvamento com diálogo começa na camada de apresentação. Em `Ribbon/Tabs/ArquivoMenuView.xaml.cs`, o método `Salvar_Click` chama `Context?.Projects.SalvarComDialogo()`. O contexto contém o serviço criado em `PersistenceComposition.CreateProjects(...)`.

O fluxo de salvamento é:

```text
ArquivoMenuView.Salvar_Click
        |
        v
IProjectPersistenceService.SalvarComDialogo()
        |
        v
ProjectFileDialogService.ShowSaveDialog()
        |
        v
ProjectPersistenceService.Salvar(path)
        |
        +--> ProjectSerializer.PrepareMetadataForSave(...)
        |
        +--> ProjectSerializer.CreateFileDto(document, metadata)
        |
        +--> ProjectSerializer.Serialize(dto)
        |
        +--> FileSystemProjectRepository.WriteAllText(path, json)
        |
        +--> atualiza _currentPath e _metadata
```

Em `ProjectPersistenceService.Salvar(string path)`, o serviço:

1. captura `savedAt` com `DateTimeOffset.UtcNow`;
2. chama `_serializer.PrepareMetadataForSave(_metadata, path, savedAt)`;
3. chama `_serializer.CreateFileDto(_document, metadata)`;
4. serializa o DTO com `_serializer.Serialize(dto)`;
5. grava o texto com `_repository.WriteAllText(path, json)`;
6. atualiza `_currentPath`;
7. atualiza `_metadata`.

`ProjectSerializer.PrepareMetadataForSave(...)` define o nome do projeto a partir dos metadados atuais quando existe um nome válido. Caso contrário, usa `Path.GetFileNameWithoutExtension(path)`. A data `CreatedAt` é preservada quando já existe; se não existir, recebe `savedAt`. `SavedAt` sempre recebe o horário do salvamento. `AppName` e `Generator` são definidos como `ProjectSerializer.AppName`. `Notes` é preservado.

`ProjectSerializer.CreateFileDto(...)` monta a raiz `ProjectFileDto` com:

- `Version = CurrentVersion`;
- `AppName`;
- `ProjectName`;
- `CreatedAt`;
- `SavedAt`;
- `Generator`;
- `Notes`;
- `Elements = document.Elementos.Select(CriarElementoDto).ToList()`.

Para cada elemento, `CriarElementoDto(...)` salva:

- `Kind`, obtido por `_elements.GetKind(elemento)`;
- `DomainRole`, obtido por `elemento.DomainRole.ToString()`;
- `Id`;
- `PosicaoX` e `PosicaoY`, como `X` e `Y`;
- `Rotacao`, como `Rotation`;
- `Escala`, como `Scale`;
- `Tipo`, como `TypeRefDto`;
- parâmetros, como `ParameterDto`;
- terminais, quando o elemento implementa `ITerminalOwner`;
- vértices, quando o elemento é `Cabo`.

O tratamento de erro no salvamento captura `IOException`, `UnauthorizedAccessException`, `JsonException`, `NotSupportedException` e `ArgumentException`. Em caso de falha, `ProjectPersistenceService` chama `_dialogs.ShowError("Salvar projeto", $"Nao foi possivel salvar o projeto.\n{ex.Message}")`.

# 7. Processo de Abertura

A abertura com diálogo começa em `Ribbon/Tabs/ArquivoMenuView.xaml.cs`, no método `Abrir_Click`, que chama `Context?.Projects.AbrirComDialogo()`.

O fluxo observado é:

```text
ArquivoMenuView.Abrir_Click
        |
        v
IProjectPersistenceService.AbrirComDialogo()
        |
        v
ProjectFileDialogService.ShowOpenDialog()
        |
        v
ProjectPersistenceService.Abrir(path)
        |
        +--> FileSystemProjectRepository.ReadAllText(path)
        |
        +--> ProjectSerializer.Deserialize(json)
        |
        +--> ProjectSerializer.GetVersion(dto)
        |
        +--> aviso se version > CurrentVersion
        |
        +--> ProjectSerializer.CreateElements(dto)
        |
        +--> AraciDocument.Limpar()
        |
        +--> AraciDocument.AdicionarElemento(...)
        |
        +--> ProjectSerializer.CreateMetadataFromFile(dto, path)
        |
        +--> limpar estado transitório
        |
        +--> ICommandHistory.Clear()
```

Em `ProjectPersistenceService.Abrir(string path)`, a substituição do documento ocorre somente depois da leitura, desserialização, verificação de versão e criação da lista de elementos. Isso reduz o risco de perda do documento atual quando uma falha ocorre antes da reconstrução dos elementos.

`ProjectSerializer.Deserialize(string json)` usa `JsonSerializer.Deserialize<ProjectFileDto>(json, _jsonOptions)`. Se o retorno for nulo, cria um novo `ProjectFileDto`.

`ProjectSerializer.GetVersion(ProjectFileDto dto)` retorna `1` quando `dto.Version <= 0`; caso contrário, retorna o valor persistido. Se a versão lida for maior que `CurrentVersion`, o serviço mostra aviso com o título `"Abrir projeto"` e a mensagem:

```text
Este projeto foi salvo em uma versao futura ({version}). O Araci tentara abrir de forma conservadora.
```

A reconstrução de elementos é feita por `ProjectSerializer.CreateElements(ProjectFileDto dto)`, que executa `dto.Elements.Select(CriarElemento).Where(e != null).Cast<Elemento>().ToList()`.

Para cada `ElementDto`, `CriarElemento(ElementDto dto)`:

1. cria o modelo com `_modelFactory.CreateModel(dto.Kind)`;
2. restaura `Id`, usando novo `Guid` apenas quando o `Id` persistido é vazio;
3. restaura `PosicaoX`, `PosicaoY`, `Rotacao` e `Escala`;
4. restaura `Tipo` com `ResolverTipo(...)`;
5. aplica parâmetros com `AplicarParametros(...)`;
6. normaliza parâmetros específicos em `NormalizarParametros(...)`;
7. aplica vértices quando o elemento é `Cabo`;
8. atualiza terminais por `TerminalLayoutService.AtualizarTerminais(elemento)`;
9. restaura posições e barras dos terminais persistidos.

Ao final da abertura, `_metadata` recebe `ProjectSerializer.CreateMetadataFromFile(dto, path)`, `_currentPath` é atualizado, o estado transitório é limpo e `_commands.Clear()` remove o histórico anterior.

O tratamento de erro de abertura captura `JsonException`, além de `IOException`, `UnauthorizedAccessException`, `InvalidOperationException`, `NotSupportedException` e `ArgumentException`. A mensagem exibida é:

```text
Nao foi possivel abrir o projeto. O projeto atual foi mantido.
{ex.Message}
```

# 8. Preservação de Identidade

A preservação de identidade é um aspecto central da persistência atual, porque a topologia elétrica depende de referências entre elementos, cabos e terminais.

## Identidade dos elementos

Cada `ElementDto` persiste `Id` como `Guid`. Na abertura, `ProjectSerializer.CriarElemento(...)` executa:

- se `dto.Id == Guid.Empty`, cria um novo `Guid`;
- caso contrário, atribui `elemento.Id = dto.Id`.

Isso preserva a identidade dos elementos salvos, exceto quando o arquivo contém explicitamente `Guid.Empty`.

## Identidade dos terminais

Os terminais são persistidos como `TerminalDto`, contendo `Id`, coordenadas e `Barra`. Na abertura, `RestaurarTerminais(...)`:

1. verifica se o elemento implementa `ITerminalOwner`;
2. obtém o tamanho visual com `_geometry.ObterTamanho(elemento)`;
3. procura no elemento recriado um terminal cujo `terminal.Id` seja igual ao `TerminalDto.Id`;
4. restaura a posição visual;
5. restaura `terminal.Barra`.

Para elementos `Cabo` ou quando o tamanho visual é vazio, a posição é restaurada diretamente por `terminal.DefinirPosicaoVisual(new Point(dto.X, dto.Y))`. Para os demais casos, usa `terminal.DefinirPosicaoVisual(new Point(dto.X, dto.Y), tamanho.Width, tamanho.Height)`.

O serializer não cria terminais arbitrários a partir do arquivo. Ele depende de `TerminalLayoutService.AtualizarTerminais(elemento)` para recompor os terminais esperados do modelo e depois restaura os dados persistidos por correspondência de `Id`.

## Identidade de conexões

Não há um DTO específico chamado `ConnectionDto` no código de persistência analisado. As referências de conexão são preservadas dentro dos `ParameterDto` de cada elemento, especialmente cabos, porque `ProjectSerializer.CriarParameterDto(...)` serializa todos os parâmetros existentes em `elemento.Parametros.Values`.

Assim, propriedades de domínio armazenadas como parâmetros, como identificadores de origem, destino e terminais de cabos, são preservadas quando existem no modelo. Na abertura, `AplicarParametros(...)` restaura apenas os parâmetros cujo nome exista no elemento criado por `_modelFactory.CreateModel(dto.Kind)`.

## Identidade geométrica dos cabos

Quando o elemento é `Cabo`, `CriarElementoDto(...)` preenche `Vertices` com `cabo.Vertices.Select(CriarPointDto).ToList()`. Na abertura, `AplicarVertices(...)`:

- limpa a lista `cabo.Vertices`;
- adiciona cada ponto persistido;
- limpa `cabo.PreviewPonto`;
- quando há pelo menos um vértice, chama `cabo.DefinirOrigem(cabo.Vertices[0])`;
- quando há mais de um vértice, chama `cabo.DefinirDestino(cabo.Vertices[^1])`.

Essa restauração preserva a geometria polilinha do cabo e também recompõe origem/destino geométricos a partir do primeiro e último vértice persistidos.

## Referência de tipo

O campo `Type` preserva uma referência lógica por `NomeTipo`, `Familia` e `Categoria`. A abertura não instancia tipos diretamente do JSON. Ela chama `_elements.ResolveType(kind, dto?.NomeTipo, dto?.Familia, dto?.Categoria)`, de modo que a resolução depende do catálogo de tipos registrado em `ElementRegistryService`.

## DomainRole

`DomainRole` é serializado em `ElementDto.DomainRole`, mas não foi observado código que atribua esse valor de volta ao elemento durante a abertura. Na reconstrução, o papel de domínio efetivo vem do modelo criado por `IElementModelFactory.CreateModel(dto.Kind)` e da classe concreta resultante.

# 9. Metadados e Versionamento

O versionamento atual é simples e concentrado em `ProjectSerializer`.

| Campo | Origem | Comportamento observado |
| --- | --- | --- |
| `Version` | `ProjectFileDto.Version` | Gravado como `CurrentVersion`, atualmente `1`. |
| `AppName` | `ProjectFileDto.AppName` / `ProjectMetadataDto.AppName` | Gravado como `"Araci Engine"` no salvamento normal. |
| `ProjectName` | `ProjectFileDto.ProjectName` | Preservado dos metadados ou derivado do nome do arquivo. |
| `CreatedAt` | `ProjectFileDto.CreatedAt` | Preservado se já existe; caso contrário, recebe `SavedAt` no salvamento. |
| `SavedAt` | `ProjectFileDto.SavedAt` | Atualizado a cada salvamento com `DateTimeOffset.UtcNow`. |
| `Generator` | `ProjectFileDto.Generator` | Gravado como `"Araci Engine"` no salvamento normal. |
| `Notes` | `ProjectFileDto.Notes` | Preservado quando existe. |

`ProjectSerializer.GetVersion(...)` trata `Version <= 0` como versão `1`. Isso fornece compatibilidade mínima com arquivos em que a versão esteja ausente, zerada ou inválida numericamente para baixo.

Quando `ProjectPersistenceService.Abrir(...)` encontra versão maior que `ProjectSerializer.CurrentVersion`, ele não bloqueia a abertura. O serviço mostra um aviso ao usuário e tenta abrir de forma conservadora.

Não foram observados migradores de versão, tabela de conversão de schema, validação formal de versão por intervalo, nem estratégia explícita de downgrade. A compatibilidade atual é baseada em:

- valores padrão dos DTOs;
- tolerância do `System.Text.Json` a campos desconhecidos;
- restauração parcial quando parâmetros persistidos não existem no modelo atual;
- aviso para arquivos de versão futura;
- fallback de versão para `1` quando o valor é menor ou igual a zero.

# 10. Integridade do Documento

A integridade na persistência atual é composta por tratamento de exceções, ordem conservadora de substituição do documento e reconstrução baseada em modelos conhecidos.

## Salvamento

Durante o salvamento, as falhas capturadas são:

- `IOException`;
- `UnauthorizedAccessException`;
- `JsonException`;
- `NotSupportedException`;
- `ArgumentException`.

Quando ocorre falha, o serviço exibe erro e não atualiza `_currentPath` nem `_metadata`, pois essas atribuições ocorrem somente depois da gravação.

## Abertura

Durante a abertura, as falhas capturadas são:

- `JsonException`;
- `IOException`;
- `UnauthorizedAccessException`;
- `InvalidOperationException`;
- `NotSupportedException`;
- `ArgumentException`.

O serviço lê, desserializa e cria a lista de elementos antes de chamar `_document.Limpar()`. Portanto, se uma falha ocorrer nessas fases iniciais, a intenção implementada é manter o projeto atual. A própria mensagem de erro informa que o projeto atual foi mantido.

## Reconstrução de elementos

A criação de cada elemento depende de `_modelFactory.CreateModel(dto.Kind)`. Isso significa que o `Kind` persistido precisa corresponder a um modelo conhecido pela aplicação. Depois de criado, o elemento recebe valores persistidos de identidade, posição, rotação, escala, tipo, parâmetros, vértices e terminais.

## Conversão de parâmetros

`ProjectSerializer.ConverterValor(...)` trata explicitamente:

- `string`;
- `int`;
- `double`;
- `bool`;
- `Guid`.

Para outros tipos, usa `dto.Value.ToString()`. Assim, a persistência atual tem suporte explícito a tipos primitivos e `Guid`, mas não evidencia conversão especializada para estruturas complexas fora desses casos.

## Parâmetros desconhecidos

Em `AplicarParametros(...)`, se `elemento.Parametros.TryGetValue(dto.Name, out var parameter)` falhar, o parâmetro persistido é ignorado. Esse comportamento evita falha por parâmetro antigo ou não reconhecido, mas também significa que dados fora do modelo atual podem ser descartados silenciosamente na abertura.

## Terminais desconhecidos

Em `RestaurarTerminais(...)`, se um `TerminalDto` não encontrar terminal correspondente pelo `Id`, ele é ignorado. A persistência não cria terminais novos com base apenas no arquivo.

## Ausências observadas

Não foram observados no código:

- checksum do arquivo;
- assinatura;
- schema JSON formal;
- validação estrutural completa antes da abertura;
- recuperação parcial por elemento;
- relatório detalhado de quais elementos foram descartados;
- transação de gravação com arquivo temporário e substituição atômica.

# 11. Relação com Topologia

A persistência se relaciona com a topologia porque preserva os dados usados para reconstrução do grafo elétrico. O fluxo conceitual observado é:

```text
+-----------------------+
| Arquivo .araci        |
+-----------+-----------+
            |
            v
+-----------------------+
| ProjectSerializer     |
| ElementDto -> Elemento|
+-----------+-----------+
            |
            v
+-----------------------+
| AraciDocument         |
| Elementos restaurados |
+-----------+-----------+
            |
            v
+-----------------------+
| Cabos com parâmetros  |
| Origem/Destino        |
| TerminalIds           |
+-----------+-----------+
            |
            v
+-----------------------+
| ConnectivityService   |
| ElectricGraphBuilder  |
+-----------+-----------+
            |
            v
+-----------------------+
| ElectricGraph         |
+-----------------------+
```

O código de persistência não constrói `ElectricGraph` diretamente. Ele preserva as informações necessárias para que os serviços topológicos façam isso depois da abertura.

Os dados preservados que impactam a topologia incluem:

- `ElementDto.Id`, usado como identidade dos elementos;
- parâmetros de conexão serializados em `ParameterDto`;
- `TerminalDto.Id`, usado para localizar terminais restaurados;
- `TerminalDto.Barra`, usado como dado associado ao terminal;
- `PointDto` em `Vertices`, usado para geometria dos cabos;
- tipo e parâmetros específicos de cada elemento.

As verificações em `Araci.TechnicalChecks/Program.cs` confirmam essa relação. Métodos como `PersistenciaPreservaTopologiaSimples`, `PersistenciaPreservaRamificacao`, `IdsPermanecemEstaveisAposReload`, `CabosConectadosAosTerminaisDoSinPreservamConexoes`, `TransformadorPreservaConexoesAposReload`, `CaboPermaneceAncoradoAposAlturaRotacaoMovimentoEReload` e `BarraRotacionadaPersisteAposReload` exercitam cenários em que o documento é salvo, reaberto e depois usado para reconstruir conexões, terminais e grafo elétrico.

Em particular, `PersistenciaPreservaRamificacao` monta um cenário ramificado, faz reload e executa `ElectricGraphBuilder(loaded).Build()`, verificando quantidade de nós, arestas e ausência de arestas inválidas. Isso demonstra que a persistência atual é usada como base para reconstrução topológica após abertura.

# 12. Relação com Simulação

A persistência não executa simulação, mas influencia diretamente a entrada da simulação porque restaura o `AraciDocument` usado pelo pipeline de leitura de parâmetros e construção de DTOs elétricos.

O fluxo observado é:

```text
+-----------------------+
| Arquivo .araci        |
+-----------+-----------+
            |
            v
+-----------------------+
| AraciDocument         |
| restaurado            |
+-----------+-----------+
            |
            v
+-----------------------+
| ParameterReader       |
+-----------+-----------+
            |
            v
+-----------------------+
| DTOs de simulação     |
+-----------+-----------+
            |
            v
+-----------------------+
| CircuitBuilder        |
+-----------+-----------+
            |
            v
+-----------------------+
| Circuito / OpenDSS    |
+-----------------------+
```

O relacionamento mais importante é que `ParameterReader` consome o documento já reconstruído. Se a persistência preservar corretamente IDs, parâmetros, terminais, tipos e vértices, o leitor de parâmetros consegue derivar os mesmos dados elétricos após reload.

`Araci.TechnicalChecks/Program.cs` contém verificações que ligam persistência e simulação:

- `DtoPermaneceEquivalenteAposReload` cria DTOs com `CircuitBuilder(new ParameterReader(...)).Build()` antes e depois do reload e compara slack, cargas e linhas.
- `ReloadComSinMantemSlackBaseadoNoSin` verifica preservação do comportamento de slack baseado em `Sin` após reload.
- `ReloadPreservaDtoDetalhadoTransformador` verifica dados detalhados de transformador no DTO após abertura.
- `OperationalGraphAposReloadPreservaResultado` verifica preservação de resultado operacional depois do reload.

Esses checks indicam que a persistência atual é tratada como uma fronteira relevante para simulação: salvar e abrir não deve alterar a interpretação elétrica do documento.

# 13. Testes de Persistência

O projeto contém verificações técnicas em `Araci.TechnicalChecks/Program.cs`. Esse arquivo está fora da compilação principal do projeto `Araci.csproj`, pois o `.csproj` remove `Araci.TechnicalChecks\**` de `Compile`, `EmbeddedResource` e `None`. Ainda assim, o código existe como suíte de validações técnicas e documenta garantias esperadas pela implementação.

O helper `SaveAndLoad(AraciDocument document)` executa o ciclo real de persistência:

1. cria um caminho temporário com extensão `.araci`;
2. cria um `EditorContext` de origem;
3. adiciona os elementos do documento de entrada em `source.Document`;
4. chama `source.Projects.Salvar(path)`;
5. cria um `EditorContext` de destino;
6. chama `target.Projects.Abrir(path)`;
7. retorna `target.Document`;
8. apaga o arquivo temporário ao final quando possível.

As principais verificações relacionadas à persistência são:

| Check | Garantia observada |
| --- | --- |
| `PersistenciaPreservaTopologiaSimples` | Preserva quantidade de elementos, IDs, nomes, cabo, vértices, comprimento e potências de carga/gerador. |
| `PersistenciaPreservaRamificacao` | Após reload, o `ElectricGraphBuilder` reconstrói nós e arestas de uma ramificação sem arestas inválidas. |
| `DtoPermaneceEquivalenteAposReload` | DTOs gerados por `CircuitBuilder` antes e depois do reload permanecem equivalentes em campos críticos. |
| `IdsPermanecemEstaveisAposReload` | IDs de cabo, origem, destino e terminais permanecem estáveis. |
| `BuildsRepetidosAposReloadNaoAlteramDocument` | Construções repetidas do grafo após reload não alteram o documento nem contagens de nós/arestas. |
| `SinPreservaIdAposReload` | `Sin` preserva ID, nome, barra e terminais. |
| `CabosConectadosAosTerminaisDoSinPreservamConexoes` | Cabos ligados a terminais do `Sin` preservam conexões e não geram arestas inválidas. |
| `ReloadComSinMantemSlackBaseadoNoSin` | O slack baseado em `Sin` é preservado após reload. |
| `OperationalGraphAposReloadPreservaResultado` | O estado operacional preserva resultado esperado após reload. |
| `TransformadorPreservaConexoesAposReload` | Transformador preserva terminais e conexões por cabos. |
| `ReloadPreservaDtoDetalhadoTransformador` | DTO detalhado de transformador preserva barramentos, tensões, potência, impedância e ligações. |
| `ElementoRotacionadoPersisteAposReload` | Rotação de equipamento persiste após reload. |
| `BarraComAlturaAlteradaPersisteAposReload` | Altura alterada de barra persiste e mantém terminal/cabo coerentes. |
| `CaboPermaneceAncoradoAposAlturaRotacaoMovimentoEReload` | Cabo permanece ancorado após alteração de altura, rotação, movimento e reload. |
| `BarraRotacionadaPersisteAposReload` | Barra rotacionada preserva rotação e ponto inicial de cabo. |

Essas verificações cobrem aspectos importantes:

- estabilidade de `Guid`;
- preservação de parâmetros;
- preservação de terminais;
- preservação de geometria;
- preservação de tipos e dados elétricos;
- reconstrução topológica;
- equivalência de DTOs de simulação;
- manutenção de comportamento operacional após abertura.

# 14. Dívidas Técnicas

As dívidas abaixo são baseadas apenas em evidências observadas no código.

## DTO genérico para todos os elementos

A persistência não possui DTOs específicos como `CaboDto`, `BarraDto`, `CargaDto`, `GeradorDto`, `SinDto` ou `TransformadorDto`. Todos os elementos são persistidos por `ElementDto`. Isso simplifica a serialização, mas torna o contrato do arquivo menos explícito para cada tipo de elemento.

## Ausência de schema formal

Não foi encontrado schema JSON formal para o arquivo `.araci`. A validação estrutural depende da desserialização por `System.Text.Json`, da criação de modelos por `IElementModelFactory` e da existência de parâmetros nos elementos recriados.

## Migração de versão ainda limitada

O código possui `CurrentVersion = 1`, grava `Version` e avisa quando abre arquivo de versão futura. Porém, não foram observados migradores, adaptadores de versões antigas, histórico de mudanças de schema ou etapas formais de upgrade/downgrade.

## Parâmetros desconhecidos são ignorados

`AplicarParametros(...)` ignora parâmetros persistidos que não existem no modelo atual. Esse comportamento favorece tolerância a mudanças, mas pode descartar dados sem relatório.

## Terminais desconhecidos são ignorados

`RestaurarTerminais(...)` só restaura terminais cujo `Id` já exista no elemento recriado. Terminais extras no arquivo são ignorados.

## DomainRole é persistido, mas não restaurado diretamente

`DomainRole` é gravado em `ElementDto`, mas não foi encontrado uso desse valor na reconstrução do elemento. O papel de domínio após abertura depende do modelo criado pelo `Kind`.

## Acoplamento ao nome das propriedades C#

Como não há `PropertyNamingPolicy` em `JsonSerializerOptions`, o formato JSON usa os nomes das propriedades C#. Mudanças de nomes em DTOs podem alterar o contrato persistido se não forem acompanhadas por atributos ou compatibilidade explícita.

## Escrita direta no arquivo final

`FileSystemProjectRepository.WriteAllText(...)` grava diretamente no caminho final. Não foi observada escrita em arquivo temporário seguida de substituição atômica, o que seria uma estratégia mais robusta para falhas durante gravação.

## Caminho atual armazenado, mas sem comando explícito de salvar atual observado

`ProjectPersistenceService` mantém `_currentPath`, mas o contrato `IProjectPersistenceService` expõe `SalvarComDialogo()` e `Salvar(string path)`. Não foi observado método público como `SalvarAtual()` usando `_currentPath` sem diálogo.

## Filtro de arquivo permite JSON e todos os arquivos

O diálogo de arquivo usa `.araci` como extensão padrão, mas também permite `.json` e qualquer extensão. Isso é útil para inspeção, mas reduz a rigidez do contrato de arquivo de projeto.

## Recuperação parcial não observada

`CreateElements(...)` tenta criar todos os elementos. Não foi observado mecanismo para abrir parcialmente um arquivo quando um elemento individual falha, nem relatório por elemento inválido.

# 15. Comparação com Arquitetura-Alvo

A persistência atual é adequada para um estágio inicial de plataforma CAD/BIM elétrica 2D: é legível, simples, baseada em JSON, preserva IDs e mantém dados suficientes para reconstrução visual, topológica e de simulação. O código também já separa responsabilidades importantes entre orquestração de aplicação, serialização, repositório de arquivo e seleção de caminho.

Em relação a uma arquitetura-alvo de CAD/BIM mais madura, o estado atual pode ser entendido como uma base funcional, ainda não como um sistema completo de persistência BIM.

| Aspecto | Implementação atual | Arquitetura-alvo esperada para CAD/BIM |
| --- | --- | --- |
| Formato | JSON único em arquivo `.araci`. | Formato versionado com schema explícito, validação e possível organização por seções. |
| DTOs | DTO genérico `ElementDto`. | DTOs ou contratos mais explícitos por família de entidade, mantendo compatibilidade. |
| Versionamento | `Version = 1`, fallback e aviso de versão futura. | Migrações formais, histórico de schema e validação de compatibilidade. |
| Integridade | Tratamento de exceções e abertura conservadora. | Checksum, escrita atômica, recuperação parcial e diagnóstico estruturado. |
| Topologia | Preservada via IDs, parâmetros, terminais e vértices. | Persistência topológica mais explícita, auditável e validável antes da simulação. |
| Simulação | Reconstituída a partir do documento aberto. | Contratos estáveis entre persistência, modelo elétrico e pipeline de simulação. |
| Metadados | `AppName`, `ProjectName`, `CreatedAt`, `SavedAt`, `Generator`, `Notes`. | Metadados de autoria, revisão, unidades, normas, histórico e ambiente de execução. |
| Escalabilidade | Lista única de elementos. | Suporte a múltiplos diagramas, referências externas, catálogos e modelos maiores. |

A principal qualidade arquitetural da implementação atual é preservar a identidade e os dados essenciais do domínio em uma estrutura simples. Isso permite que checks técnicos validem reload, topologia e simulação com baixo custo operacional.

A principal limitação é que o contrato persistido ainda é implícito: ele depende dos nomes de propriedades C#, da existência de parâmetros em modelos recriados e da resolução de tipos pelo registro de elementos. Para uma evolução CAD/BIM, a tendência natural seria tornar o contrato de arquivo mais explícito, versionado e validável, sem perder a simplicidade que hoje facilita inspeção e depuração.
