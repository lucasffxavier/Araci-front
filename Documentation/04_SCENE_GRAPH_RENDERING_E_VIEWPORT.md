# 1. Introdução

Este documento mapeia a engine visual, o scene graph e o viewport do Araci conforme o código atualmente presente na solução. O objetivo é descrever como os elementos do domínio são projetados para uma cena editável em WPF, como essa cena é consultada espacialmente, como os controles visuais são materializados e como a navegação do usuário transforma coordenadas de tela em coordenadas de mundo.

O conteúdo é derivado dos arquivos reais existentes no projeto. Não são descritos módulos, classes ou fluxos que não estejam representados no código analisado. A análise se concentra exclusivamente na camada visual e nos serviços diretamente relacionados a renderização, cena, ViewModels, controles, seleção, hover, drag, viewport, câmera e consultas espaciais.

A engine visual do Araci, no estado atual, está estruturada em torno de uma cadeia principal:

```text
AraciDocument
    |
    v
DocumentSceneSyncService
    |
    v
Scene.Elementos
    |
    v
ElementoViewModel + ElementoNode
    |
    v
ViewportView / ItemsControl / DataTemplate
    |
    v
Controls WPF + SharpVectors SVG
```

Essa cadeia mostra que o desenho não ocorre diretamente a partir dos modelos de domínio. Os modelos são convertidos em ViewModels, os ViewModels encapsulam nós geométricos, e o XAML seleciona controles específicos para cada tipo visual. A câmera e a navegação atuam em camadas visuais do viewport por meio de uma transformação matricial aplicada no WPF.

# 2. Visão Geral da Arquitetura Visual

A arquitetura visual observada é composta por seis grupos de responsabilidade:

| Grupo | Classes e arquivos | Responsabilidade observada |
|---|---|---|
| Documento | `Core/Documents/AraciDocument.cs` | Fonte dos elementos do projeto. |
| Cena | `Core/Scenes/Scene.cs` | Coleção visual observável de `ElementoViewModel`. |
| Sincronização | `Applications/Scene/DocumentSceneSyncService.cs` | Mantém a cena coerente com a coleção de elementos do documento. |
| Nós de cena | `Core/SceneNodes/*.cs` | Calculam geometria, posição, bounds e movimento visual. |
| ViewModels | `ViewModels/*ViewModel.cs` | Adaptam modelos para renderização, propriedades editáveis e estados visuais. |
| Viewport e controles | `Views/ViewportView.xaml`, `Controls/*.cs` | Materializam a cena em WPF e tratam entrada de mouse/teclado. |

O desenho segue uma separação parcial entre domínio, cena e apresentação. O domínio contém entidades como `Barra`, `Cabo`, `Carga`, `Gerador`, `Sin` e `Transformador`. A cena contém `ElementoViewModel`, e cada ViewModel possui um `ElementoNode` que computa a geometria exibida.

```text
+-------------------+       +-----------------------------+
| AraciDocument     |       | Araci.Core.Scenes.Scene     |
| Elementos         +------>+ ObservableCollection<VM>    |
+-------------------+       +--------------+--------------+
                                          |
                                          v
                         +----------------+----------------+
                         | ElementoViewModel               |
                         | Modelo + Node + VisualState     |
                         +----------------+----------------+
                                          |
                                          v
                         +----------------+----------------+
                         | ViewportView.xaml               |
                         | ItemsControl + DataTemplates    |
                         +----------------+----------------+
                                          |
                                          v
                         +----------------+----------------+
                         | Controls WPF                    |
                         | BarraControl, CaboControl, ...  |
                         +---------------------------------+
```

Os namespaces principais dessa área são:

| Namespace | Papel |
|---|---|
| `Araci.Core.Scenes` | Define a cena visual. |
| `Araci.Core.SceneNodes` | Define nós geométricos associados aos modelos. |
| `Araci.Core.Rendering` | Define dados e constantes de geometria para renderização. |
| `Araci.Core.SceneQueries` | Implementa consultas visuais e hit test. |
| `Araci.Core.Spatial` | Implementa índice espacial. |
| `Araci.Core.Viewport` | Define a câmera. |
| `Araci.ViewModels` | Expõe ViewModels usados pelo viewport. |
| `Araci.Controls` e `Araci.Controls.Base` | Implementam controles WPF dos elementos. |
| `Araci.Views` | Contém o viewport WPF. |
| `Araci.Services` | Agrupa serviços de viewport, navegação, layout, atualização visual, snap, hover e movimento. |
| `Araci.Applications.Scene` | Sincroniza documento e cena. |
| `Araci.Applications.Editor` | Roteia eventos de entrada para ferramentas. |
| `Araci.Applications.Editar.Selecionar` | Implementa seleção, drag, box selection e edição de vértices de cabo. |

# 3. Scene

A classe `Scene` está em `Core/Scenes/Scene.cs`, no namespace `Araci.Core.Scenes`. Ela é uma classe simples e central para a camada visual:

| Membro | Tipo | Responsabilidade |
|---|---|---|
| `Elementos` | `ObservableCollection<ElementoViewModel>` | Coleção observável de elementos visuais renderizados pelo viewport. |
| `Scene()` | construtor | Inicializa a coleção `Elementos`. |

O papel arquitetural da `Scene` é funcionar como estado visual agregador. Ela não armazena diretamente `Elemento`, `Barra`, `Cabo` ou outros modelos. Ela armazena `ElementoViewModel`, que por sua vez referenciam os modelos de domínio.

```text
Scene
  |
  +-- ObservableCollection<ElementoViewModel>
        |
        +-- BarraViewModel
        +-- CaboViewModel
        +-- CargaViewModel
        +-- GeradorViewModel
        +-- SinViewModel
        +-- TransformadorViewModel
```

Como `Scene.Elementos` é uma `ObservableCollection`, ela é usada diretamente como fonte de dados no `ItemsControl` chamado `WorldLayer` em `Views/ViewportView.xaml`. O serviço `SceneQueryService`, em `Core/SceneQueries/SceneQueryService.cs`, também observa essa coleção para invalidar e reconstruir o índice espacial quando elementos são adicionados ou removidos.

# 4. Sincronização Documento → Cena

A sincronização entre documento e cena é implementada por `DocumentSceneSyncService`, em `Applications/Scene/DocumentSceneSyncService.cs`, namespace `Araci.Applications.Scene`.

Essa classe recebe no construtor:

| Dependência | Papel no serviço |
|---|---|
| `AraciDocument` | Origem da coleção de modelos. |
| `CoreScene` (`Araci.Core.Scenes.Scene`) | Destino visual onde os ViewModels são registrados. |
| `ElementoFactory` | Criação de `ElementoViewModel` a partir de `Elemento`. |
| `ISelectionService` | Limpeza de seleção ao remover ou reinicializar ViewModels. |
| `CableVertexEditService` | Limpeza e atualização de estado de edição de vértices. |
| `TerminalSnapState` | Limpeza do marcador de snap em eventos de reset. |
| `AlignmentGuideService` | Limpeza de guias de alinhamento. |
| `IHoverService` | Limpeza de hover ao remover ViewModels. |
| `ISceneQueryService` | Invalidação de consultas espaciais após mudanças. |

O serviço mantém o dicionário `_viewModelsPorModelo`, do tipo `Dictionary<Elemento, ElementoViewModel>`, que associa cada modelo do documento a um ViewModel visual. O construtor assina `AraciDocument.Elementos.CollectionChanged` e executa `SincronizarComDocumento()`, criando a cena inicial.

O fluxo observado é:

```text
AraciDocument.Elementos.CollectionChanged
       |
       v
DocumentSceneSyncService.OnDocumentElementosChanged
       |
       +-- Reset       -> LimparViewModels()
       +-- OldItems    -> RemoverViewModel(modelo)
       +-- NewItems    -> AdicionarViewModel(modelo)
       |
       v
Scene.Elementos atualizada
       |
       v
ISceneQueryService.Invalidate()
```

Em `AdicionarViewModel`, o serviço chama `ObterOuCriarViewModel`, que delega a criação para `_elementoFactory.CriarViewModel(modelo)`. Em `RemoverViewModel`, ele remove o ViewModel da cena, limpa seleção, edição de vértices e hover associados, remove o mapeamento e invalida as consultas.

`LimparViewModels()` é mais abrangente: limpa seleção, edição de vértices de cabo, hover, snap de terminal, guias de alinhamento, a coleção da cena e o dicionário de mapeamento. Essa operação é usada principalmente quando a coleção do documento dispara reset.

A classe também expõe:

| Método | Função |
|---|---|
| `RegistrarViewModel(ElementoViewModel vm)` | Registra manualmente um ViewModel se o modelo existir no documento e o VM estiver na cena. |
| `ObterViewModel(Elemento modelo)` | Retorna o VM correspondente se ele ainda estiver válido na cena e no documento. |
| `AtualizarViewModel(Elemento modelo)` | Chama `AtualizarAposModeloAlterado()` no VM associado. |
| `SincronizarComDocumento()` | Reconstrói a cena visual a partir da coleção atual do documento. |

# 5. Scene Nodes

Os nós de cena ficam em `Core/SceneNodes`, namespace `Araci.Core.SceneNodes`. Eles encapsulam geometria visual e operações de movimento, mantendo a relação com o modelo de domínio.

```text
ElementoNode (abstract)
  |
  +-- BarraNode
  |
  +-- CaboNode
  |
  +-- EquipamentoNode
        |
        +-- usado por CargaViewModel
        +-- usado por GeradorViewModel
        +-- usado por SinViewModel
        +-- usado por TransformadorViewModel
```

## ElementoNode

`ElementoNode`, em `Core/SceneNodes/ElementoNode.cs`, é a classe base abstrata. Ela recebe um `Elemento` no construtor e expõe:

| Membro | Responsabilidade |
|---|---|
| `Modelo` | Referência ao modelo de domínio. |
| `Bounds` | Retângulo calculado do elemento. |
| `BoundsAlinhamento` | Retângulo usado para alinhamento, por padrão igual a `Bounds`. |
| `Centro` | Centro calculado a partir de `Bounds`. |
| `Largura`, `Altura` | Dimensões derivadas de `Bounds`. |
| `X`, `Y` | Acessam `Modelo.PosicaoX` e `Modelo.PosicaoY`. |
| `Mover(Vector delta)` | Atualiza posição do modelo e recalcula geometria. |
| `AtualizarGeometria()` | Método abstrato implementado pelos nós derivados. |

## BarraNode

`BarraNode`, em `Core/SceneNodes/BarraNode.cs`, representa a geometria visual de uma `Barra`. Ela usa `ElementGeometryDefaults.BarraLargura` como largura fixa e `Barra.Altura` como altura. O método `AtualizarGeometria()` define:

```text
Bounds = Rect(Modelo.PosicaoX, Modelo.PosicaoY, Largura, Altura)
```

`BoundsAlinhamento` é ajustado com um inset vertical calculado por `Math.Min(Altura / 4.0, BarraLargura / 2.0)`, produzindo uma área útil de alinhamento menor que a altura total.

## CaboNode

`CaboNode`, em `Core/SceneNodes/CaboNode.cs`, representa a geometria de um `Cabo`. Diferente de equipamentos retangulares, ele calcula sua posição e `Bounds` a partir de uma lista de pontos:

| Fonte de pontos | Uso |
|---|---|
| `Cabo.Vertices` | Pontos permanentes do cabo. |
| `Cabo.PreviewPonto` | Ponto temporário usado durante inserção ou preview. |

Se não houver pontos, o node define `Bounds = Rect.Empty` e coordenadas internas `_x` e `_y` iguais a zero. Quando há pontos, calcula `minX`, `minY`, `maxX` e `maxY`, garantindo largura e altura mínimas de 1.

O método `Mover(Vector delta)` chama `_cabo.MoverPreservandoAncoras(delta)`. Isso indica que o movimento visual de cabos considera ancoragens existentes no modelo, mas a implementação dessa regra está no modelo `Cabo`, fora do escopo deste documento visual.

## EquipamentoNode

`EquipamentoNode`, em `Core/SceneNodes/EquipamentoNode.cs`, representa equipamentos retangulares. Seu construtor recebe `ElementoEquipamento` e dimensões opcionais. Por padrão usa:

| Constante | Valor |
|---|---:|
| `ElementGeometryDefaults.EquipamentoLargura` | 70 |
| `ElementGeometryDefaults.EquipamentoAltura` | 70 |

`TransformadorViewModel` instancia `EquipamentoNode` com dimensões específicas de transformador: `ElementGeometryDefaults.TransformadorLargura` igual a 80 e `ElementGeometryDefaults.TransformadorAltura` igual a 140.

# 6. ViewModels

Os ViewModels visuais estão no namespace `Araci.ViewModels`. A classe base é `ElementoViewModel`, em `ViewModels/ElementoViewModel.cs`. Ela é responsável por adaptar um `Elemento` de domínio ao viewport.

## ElementoViewModel

`ElementoViewModel` contém:

| Membro | Função |
|---|---|
| `Modelo` | Modelo de domínio associado. |
| `Node` | `ElementoNode` responsável pela geometria. |
| `Tipo` | Acesso a `Modelo.Tipo`. |
| `TiposDisponiveis` | Coleção abstrata definida nas classes derivadas. |
| `TipoViewModel` | ViewModel de tipo criado por `TipoElementoViewModelFactory.Criar(Tipo)`. |
| `VisualState` | Instância de `ElementoVisualState`. |
| `IsSelecionado`, `IsHover`, `IsPreview` | Estados visuais usados pelos controles e consultas. |
| `Stroke`, `StrokeThickness` | Propriedades derivadas do estado visual. |
| `RenderData` | Dados de renderização compostos por dimensão, pontos locais e stroke. |
| `Rotacao` | Encapsula `Modelo.Rotacao`. |
| `X`, `Y`, `WorldX`, `WorldY` | Coordenadas visuais expostas para binding. |
| `Largura`, `Altura`, `Bounds`, `BoundsAlinhamento`, `Centro` | Geometria exposta aos controles, seleção e índices. |

O método `NotificarGeometria()` dispara `PropertyChanged` para as propriedades geométricas e `RenderData`. Isso aciona tanto os bindings WPF quanto a invalidação observada pelo `SceneQueryService`.

`ElementoViewModel` também implementa `Mover(Vector delta)`, `CapturarEstado()`, `AplicarEstado(ElementoEstado estado)` e `AtualizarAposModeloAlterado()`. Esses métodos conectam interação visual e histórico de comandos.

## ViewModels específicos

| Classe | Arquivo | Node usado | Responsabilidades visuais observadas |
|---|---|---|---|
| `BarraViewModel` | `ViewModels/BarraViewModel.cs` | `BarraNode` | Expõe propriedades de `Barra`, atualiza terminais e aplica altura via `ElementGeometryUpdateService` quando disponível. |
| `CaboViewModel` | `ViewModels/CaboViewModel.cs` | `CaboNode` | Gerencia vértices, preview, origem/destino, movimento de cabo e estado com lista de vértices. |
| `CargaViewModel` | `ViewModels/CargaViewModel.cs` | `EquipamentoNode` | Expõe propriedades de carga e atualiza terminais em movimento ou alteração de geometria. |
| `GeradorViewModel` | `ViewModels/GeradorViewModel.cs` | `EquipamentoNode` | Expõe propriedades de gerador e atualiza terminais. |
| `SinViewModel` | `ViewModels/SinViewModel.cs` | `EquipamentoNode` | Expõe propriedades do SIN e atualiza terminais. |
| `TransformadorViewModel` | `ViewModels/TransformadorViewModel.cs` | `EquipamentoNode` com 80 x 140 | Expõe parâmetros do transformador e atualiza terminais. |

## ViewportViewModel

`ViewportViewModel`, em `ViewModels/ViewportViewModel.cs`, agrega estado visual do viewport:

| Propriedade | Origem ou uso |
|---|---|
| `Document` | `AraciDocument`. |
| `Scene` | Cena visual. |
| `Elementos` | Atalho para `Scene.Elementos`. |
| `SelectionBox` | Estado do retângulo de seleção. |
| `TerminalSnap` | Estado do marcador de snap de terminal. |
| `CableVertexEdit` | Serviço com handles de edição de cabo. |
| `MoveHud` | HUD exibido durante movimento. |
| `AlignmentGuides` | Coleção de linhas de guia de alinhamento. |
| `InverseZoom` e propriedades relacionadas | Ajustam tamanhos visuais de marcadores conforme zoom. |

O método `AtualizarZoomVisual(double zoom)` calcula o inverso do zoom e notifica propriedades usadas por marcadores de terminal e mensagens de snap. Isso evita que esses marcadores cresçam ou diminuam proporcionalmente ao mundo quando a câmera muda.

## Estado visual

`ElementoVisualState`, em `ViewModels/VisualStates/ElementoVisualState.cs`, contém seleção, hover, visibilidade, travamento e stroke. O comportamento atual define:

| Estado | Stroke | Espessura |
|---|---|---:|
| Selecionado | `Brushes.DeepSkyBlue` | 4 |
| Hover | `Brushes.LightSkyBlue` | 3 |
| Normal | `StrokeBase` (`Brushes.DimGray`) | `StrokeThicknessBase` (2) |

`ElementoEstado`, em `ViewModels/ElementoEstado.cs`, captura posição, rotação e, para cabos, lista de vértices. Ele é usado por serviços como `MoveService` e `CableVertexEditService`.

# 7. Rendering

A renderização combina dados geométricos, DataTemplates WPF, controles específicos e SVGs. Os arquivos centrais são:

| Arquivo | Namespace | Papel |
|---|---|---|
| `Core/Rendering/ElementGeometryDefaults.cs` | `Araci.Core.Rendering` | Define dimensões padrão. |
| `Core/Rendering/ElementoRenderData.cs` | `Araci.Core.Rendering` | Agrupa dados de renderização expostos pelo ViewModel. |
| `Controls/Base/ElementoControlBase.cs` | `Araci.Controls.Base` | Classe base para controles visuais. |
| `Controls/*.cs` | `Araci.Controls` | Controles concretos de elementos. |
| `Views/ViewportView.xaml` | `Araci.Views` | Hospeda layers e DataTemplates. |

`ElementGeometryDefaults` define constantes de dimensão:

| Constante | Valor |
|---|---:|
| `EquipamentoLargura` | 70 |
| `EquipamentoAltura` | 70 |
| `TransformadorLargura` | 80 |
| `TransformadorAltura` | 140 |
| `BarraLargura` | 10 |

`ElementoRenderData` contém `Largura`, `Altura`, `PontoLocalInicial`, `PontoLocalFinal`, `Stroke` e `StrokeThickness`. Em `ElementoViewModel`, a propriedade `RenderData` é criada a partir das dimensões do node, dos pontos locais e do estado visual.

## Controles WPF

`ElementoControlBase`, em `Controls/Base/ElementoControlBase.cs`, configura:

| Item | Implementação |
|---|---|
| Cursor padrão | `Cursors.Hand`. |
| Rotação visual | `RotateTransform` com binding em `ElementoViewModel.Rotacao`. |
| Atualização manual | Assinatura de `PropertyChanged` quando `UsaBindings` é falso. |
| Hover local | `MouseEnter` e `MouseLeave` alteram `vm.IsHover`. |

Os controles concretos são:

| Controle | Renderização observada |
|---|---|
| `BarraControl` | Usa `SvgViewbox` com `Resources/Svg/barra.svg`, overlay de seleção/hover, overlay de preview e handles superior/inferior. |
| `CaboControl` | Usa `Canvas`, `Polyline` visível, `Polyline` transparente para hit area e elipses para handles intermediários. |
| `CargaControl` | Usa `SvgViewbox` com `Resources/Svg/carga.svg`, overlay de preview e overlay de seleção/hover. |
| `GeradorControl` | Usa `SvgViewbox` com `Resources/Svg/gerador.svg`, overlay de preview e overlay de seleção/hover. |
| `SinControl` | Usa `SvgViewbox` com `Resources/Svg/sin.svg`, overlay de preview e overlay de seleção/hover. |
| `TransformadorControl` | Usa `SvgViewbox` com `Resources/Svg/transformador.svg`, stretch uniforme e overlays mascarados por `VisualBrush`. |

`CaboControl` é o controle mais especializado. Ele observa `Cabo.Vertices.CollectionChanged`, recalcula a polilinha em coordenadas locais subtraindo `WorldX` e `WorldY`, inclui `PreviewPonto` quando presente e controla a exibição de handles de vértices intermediários quando o cabo está selecionado ou em hover.

# 8. Sistema Espacial

O sistema espacial está distribuído entre `Araci.Core.Spatial` e `Araci.Core.SceneQueries`.

## ISpatialIndex

`ISpatialIndex`, em `Core/Spatial/ISpatialIndex.cs`, define:

| Método | Responsabilidade |
|---|---|
| `Build(IEnumerable<ElementoViewModel>)` | Reconstruir o índice a partir de elementos. |
| `Add(ElementoViewModel)` | Adicionar elemento ao índice. |
| `Remove(ElementoViewModel)` | Remover elemento do índice. |
| `Update(ElementoViewModel)` | Atualizar elemento. |
| `Query(Rect)` | Consultar elementos por área. |
| `Nearby(Point, double)` | Consultar elementos próximos de um ponto. |

## SpatialHashGrid

`SpatialHashGrid`, em `Core/Spatial/SpatialHashGrid.cs`, implementa `ISpatialIndex` com uma grade hash:

```text
Rect Bounds do elemento
       |
       v
GetCells(bounds)
       |
       v
Dictionary<(int, int), List<ElementoViewModel>>
```

O tamanho padrão da célula é 100. O método `Query(Rect area)` percorre as células cobertas pela área, evita duplicidade com `HashSet<ElementoViewModel>` e retorna apenas elementos cujo `Bounds` intersecta a área.

`Nearby(Point point, double radius)` converte o ponto em um retângulo ao redor dele e delega para `Query`.

## SceneQueryService

`SceneQueryService`, em `Core/SceneQueries/SceneQueryService.cs`, é a camada de consulta usada por seleção, hover, snap e interação. Ele recebe uma `Scene`, cria internamente um `SpatialHashGrid` e observa:

| Observação | Efeito |
|---|---|
| `Scene.Elementos.CollectionChanged` | Reobserva ViewModels e invalida índice. |
| `ElementoViewModel.PropertyChanged` | Invalida índice quando mudam propriedades geométricas. |

As propriedades que invalidam o índice incluem `Bounds`, `X`, `Y`, `WorldX`, `WorldY`, `Largura`, `Altura`, `Centro`, `Rotacao` e `RenderData`.

O índice é reconstruído sob demanda em `GarantirIndex()`, somente quando `_indexValido` está falso.

## HitTest, Query e Nearby

`HitTest(Point)` chama `HitTest(Point, double)` com tolerância 6. O método usa:

```text
Nearby(point, max(10, tolerance))
    |
    +-- ignora IsPreview
    +-- cria candidatos
    +-- escolhe melhor por prioridade, distância e ordem
```

Para cabos, `CriarCandidatoCabo` mede a menor distância do ponto aos segmentos definidos por `Cabo.Vertices`. Se houver menos de dois vértices, cai para teste por `Bounds`.

Para elementos não lineares, `ContemPontoElemento` testa `Bounds`. Quando há rotação, o ponto é rotacionado inversamente em torno de `vm.Centro` antes do teste.

`Query(Rect)` e `Nearby(Point,double)` usam o índice espacial e depois fazem uma varredura adicional na cena para elementos com geometria expandida, definida por:

```text
Math.Abs(vm.Rotacao) > 0.000001
    ou
vm.Modelo is ITerminalOwner
```

Essa segunda etapa permite considerar bounds rotacionados e posições de terminais que podem não estar totalmente representados pelo `Bounds` simples.

# 9. Viewport e Câmera

A câmera está em `Core/Viewport/Camera.cs`, namespace `Araci.Core.Viewport`. Ela implementa `INotifyPropertyChanged` e contém:

| Membro | Valor ou função |
|---|---|
| `DefaultZoom` | 1.0 |
| `DefaultWheelZoomFactor` | 1.1 |
| `MinZoom` | 0.1 |
| `MaxZoom` | 8.0 |
| `Zoom` | Zoom atual com clamp. |
| `Offset` | Deslocamento de tela. |
| `Pan(Vector)` | Soma delta ao offset. |
| `ZoomAt(Point,double)` | Altera zoom preservando o ponto de mundo sob o cursor. |
| `SetZoomAt(Point,double)` | Define zoom específico preservando referência. |
| `Fit(Rect, Size, double)` | Ajusta zoom e offset para enquadrar bounds no viewport. |
| `Reset()` | Retorna a zoom 1 e offset zero. |
| `WorldToScreen(Point)` | Aplica zoom e offset. |
| `ScreenToWorld(Point)` | Inverte zoom e offset. |

O serviço `ViewportService`, em `Service/ViewportService.cs`, encapsula `ViewportViewModel` e expõe a `Camera`. Ele também guarda tamanho lógico do viewport (`Largura`, `Altura`), calcula `CentroTela`, converte pontos entre mundo e tela, executa pan, zoom in/out, zoom 100%, reset e `ZoomExtents`.

`ZoomExtents(double margem = 40)` une os `Bounds` não vazios de todos os elementos e chama `Camera.Fit`. Se não houver bounds válidos, chama `Camera.Reset`.

`ViewportNavigationService`, em `Service/ViewportNavigationService.cs`, trata navegação:

| Entrada | Comportamento |
|---|---|
| Botão do meio | Inicia pan, exceto duplo clique. |
| Duplo clique no botão do meio | Executa `ZoomExtents()`. |
| Espaço + botão esquerdo | Inicia pan alternativo. |
| Roda do mouse | Zoom no cursor. |
| Ctrl + `+` | Zoom in no centro. |
| Ctrl + `-` | Zoom out no centro. |
| Ctrl + `0` | Reset da câmera. |
| Ctrl + `1` | Zoom 100% no centro. |

# 10. Fluxo de Renderização

O fluxo de renderização observado combina sincronização de dados, ViewModel, DataTemplate e controle WPF.

```text
AraciDocument.Elementos
       |
       v
DocumentSceneSyncService
       |
       v
ElementoFactory / ElementoViewModelFactory
       |
       v
ElementoViewModel
       |
       +-- Modelo: Elemento
       +-- Node: ElementoNode
       +-- VisualState
       +-- RenderData
       |
       v
Scene.Elementos
       |
       v
ViewportView.WorldLayer ItemsControl
       |
       v
DataTemplate por tipo de ViewModel
       |
       v
BarraControl / CaboControl / CargaControl / GeradorControl / SinControl / TransformadorControl
       |
       v
WPF visual tree
```

Em `Views/ViewportView.xaml`, `WorldLayer` é um `ItemsControl` com `ItemsSource="{Binding Elementos}"`. O painel de itens é um `Canvas`, e cada `ContentPresenter` recebe:

| Propriedade WPF | Binding |
|---|---|
| `Canvas.Left` | `WorldX` |
| `Canvas.Top` | `WorldY` |

Os DataTemplates mapeiam:

| ViewModel | Controle |
|---|---|
| `BarraViewModel` | `BarraControl` |
| `CaboViewModel` | `CaboControl` |
| `CargaViewModel` | `CargaControl` |
| `GeradorViewModel` | `GeradorControl` |
| `SinViewModel` | `SinControl` |
| `TransformadorViewModel` | `TransformadorControl` |

A câmera é aplicada em `Views/ViewportView.xaml.cs`. O método `ConfigurarCamera()` atribui o mesmo `MatrixTransform` a:

| Layer | Efeito |
|---|---|
| `WorldLayer` | Transforma elementos principais. |
| `AlignmentGuideLayer` | Transforma guias de alinhamento. |
| `SelectionLayer` | Transforma retângulo de seleção. |
| `CableVertexHandleLayer` | Transforma camada reservada de handles. |
| `TerminalSnapLayer` | Transforma marcadores de snap. |

`AtualizarCameraTransform()` cria uma matriz:

```text
Matrix(
    camera.Zoom, 0,
    0, camera.Zoom,
    camera.Offset.X, camera.Offset.Y)
```

Depois chama `ViewportViewModel.AtualizarZoomVisual(camera.Zoom)`, atualizando propriedades visuais dependentes do inverso do zoom.

# 11. Fluxo de Interação

O fluxo de interação começa em `ViewportView`, passa por `InputRouter` e chega às ferramentas ativas.

```text
Mouse/Keyboard em ViewportView
       |
       v
ViewportNavigationService
       |
       +-- pan / zoom / atalhos de câmera
       |
       v
InputRouter
       |
       v
ToolService.FerramentaAtual
       |
       v
SelecionarTool / MoverTool / ferramentas de inserção
```

## Mouse

`ViewportView.xaml.cs` trata eventos de preview. Em `OnPreviewMouseLeftButtonDown`, o viewport:

1. Verifica se deve iniciar pan com espaço + botão esquerdo.
2. Garante foco de teclado.
3. Encontra o `ElementoViewModel` sob o mouse por caminhada na árvore visual em `EncontrarElemento`.
4. Converte posição de tela para mundo por `ViewportService.ScreenToWorld`.
5. Cria `ToolInputState`.
6. Chama `_context.Input.MouseDown(vm, worldPosition, inputState)`.
7. Captura o mouse.

Em `OnPreviewMouseMove`, se a navegação está em pan, chama `ViewportNavigationService.TryUpdatePan`. Caso contrário, calcula posição de mundo e chama `_context.Input.MouseMove`.

Em `OnPreviewMouseLeftButtonUp`, encerra pan se necessário, trata supressão de mouse up após pan com espaço, chama `_context.Input.MouseUp` e libera captura.

## Teclado

`InputRouter`, em `Applications/Editor/InputRouter.cs`, trata atalhos globais:

| Entrada | Comportamento |
|---|---|
| Ctrl+Z | `_commands.Undo()` |
| Ctrl+Y | `_commands.Redo()` |
| Ctrl+C | Copiar selecionados |
| Ctrl+V | Colar |
| Delete | `_safeDelete.DeleteActiveHandleOrSelection()` |
| Escape | Cancela ferramenta ocupada ou limpa seleção |
| Space | Pode rotacionar seleção via ferramenta ou permitir pan conforme contexto |

Ele também implementa atalhos de duas teclas sem modificadores. O buffer expira após 1 segundo e aciona definições de elementos via `IElementCatalog.FindByShortcut`, além de `SE`, `MV` e `AL`.

## Seleção, hover e drag

`SelecionarTool`, em `Applications/Editar/Selecionar/SelecionarTool.cs`, orquestra seleção, resize de barra, edição de vértices de cabo, box selection, drag e rotação.

No mouse down, a ordem observada é:

```text
BarraResizeService.TryBegin
    |
Alt + TryRemoveHandle
    |
Ctrl + TryInsertVertex
    |
CableVertexEditService.TryBegin
    |
HitTest/elemento sob mouse
    |
SelectionController.Select
    |
DragMoveController.Begin
    |
SelectionBoxController.Begin
```

`SelectionBoxController`, em `Applications/Editar/Selecionar/SelectionBoxController.cs`, usa `ISceneQueryService.Query(area)` e depois valida interseções com regras específicas para cabos e barras.

`DragMoveController`, em `Applications/Editar/Selecionar/DragMoveController.cs`, usa `MoveService`, `MoveHudService`, `AlignmentGuideService` e `MoveConstraintService`. Durante update, calcula delta pretendido, aplica restrições, aplica snap de alinhamento e chama `_move.MoverVisual(vm, incremento)` para cada ViewModel selecionado.

`HoverService`, em `Service/HoverService.cs`, usa `ISceneQueryService.HitTest(worldPosition)` para definir `IsHover` no ViewModel atual. Quando a ferramenta está ocupada, `InputRouter.MouseMove` chama `_hover.Clear()`.

## Edição de vértices de cabo

`CableVertexEditService`, em `Applications/Editar/Selecionar/CableVertexEditService.cs`, mantém uma coleção `Handles` de `CableVertexHandleViewModel`. Ele permite:

| Operação | Método |
|---|---|
| Recriar handles | `Refresh()` |
| Iniciar drag de handle | `TryBegin(Point)` |
| Inserir vértice em segmento | `TryInsertVertex(Point)` |
| Remover handle por posição | `TryRemoveHandle(Point)` |
| Remover handle ativo | `TryRemoveActive()` |
| Atualizar posição durante drag | `Update(Point, ToolInputState)` |
| Finalizar edição | `End()` |
| Cancelar edição | `Cancel()` |

`CableVertexInteractionController`, em `Applications/Editar/Selecionar/CableVertexInteractionController.cs`, faz hit test dos handles e segmentos, projeta pontos em segmentos e aplica restrição ortogonal quando `Shift` está pressionado.

# 12. Performance

As evidências de performance estão principalmente no índice espacial, na invalidação preguiçosa e na separação de camadas transformadas.

## Índice espacial preguiçoso

`SceneQueryService` não reconstrói o índice imediatamente a cada mudança. Ele marca `_indexValido = false` em `Invalidate()` e reconstrói em `GarantirIndex()` apenas quando uma consulta ocorre. Isso evita reconstruções múltiplas durante sequências de atualização, mas concentra custo no próximo `HitTest`, `Query` ou `Nearby`.

## SpatialHashGrid

`SpatialHashGrid` evita varredura integral para consultas simples. Ele distribui elementos por células de tamanho 100 e usa `HashSet` em `Query` para evitar duplicidades quando um elemento ocupa mais de uma célula.

## Varredura adicional controlada

`SceneQueryService.Query` e `Nearby` fazem uma varredura adicional apenas para ViewModels que usam geometria expandida: elementos rotacionados ou modelos que implementam `ITerminalOwner`. Isso corrige limitações do `Bounds` simples sem varrer todos os elementos em todos os casos, desde que a maioria dos elementos não precise dessa regra expandida.

## Transformação por layer

`ViewportView.xaml.cs` aplica uma única `MatrixTransform` aos layers principais. Essa abordagem deixa o WPF transformar a árvore visual de cada camada, sem recalcular individualmente as coordenadas de todos os controles a cada alteração de zoom ou pan.

## Atualização visual por notificação

`ElementoViewModel.NotificarGeometria()` dispara `PropertyChanged` para propriedades geométricas. Controles com bindings, como `BarraControl`, `CargaControl`, `GeradorControl`, `SinControl` e `TransformadorControl`, atualizam por binding. `CaboControl` usa uma estratégia manual, observando `PropertyChanged` e `Cabo.Vertices.CollectionChanged`.

# 13. Acoplamentos

Os acoplamentos reais mais relevantes são:

| Origem | Destino | Evidência |
|---|---|---|
| `Scene` | `ElementoViewModel` | `Scene.Elementos` armazena ViewModels, não modelos puros. |
| `DocumentSceneSyncService` | Documento, cena, seleção, hover, snap, guias, cabos e consultas | O serviço centraliza limpeza e sincronização visual. |
| `ElementoViewModel` | `ElementoNode`, `TipoElemento`, serviços de nome e propriedades | A ViewModel adapta domínio, geometria e UI de propriedades. |
| `BarraViewModel` | `ElementGeometryUpdateService` | A alteração de altura pode reancorar cabos e atualizar cena. |
| `ViewportView` | `EditorContext` | A view cria ViewModel via contexto e acessa serviços de input, navegação, viewport, hover e snap. |
| `SceneQueryService` | `Scene`, `SpatialHashGrid`, `ElementoViewModel`, `ITerminalOwner`, `CaboViewModel` | Consulta espacial conhece detalhes de cabos, rotação e terminais. |
| `MoveService` | `ConnectivityService`, `TerminalLayoutService`, `ViewportService`, `ISceneQueryService`, `MoverElementoUseCase` | Movimento visual também atualiza conectividade e histórico. |
| `CableVertexEditService` | Seleção, queries, visual updates e use case de edição | Edição visual de vértices executa caso de uso e invalida cena. |
| Controles WPF | SVGs em `Resources/Svg` e ViewModels específicos | Controles carregam assets por URI pack e fazem bindings por nomes de propriedades. |

Um ponto arquitetural importante é que `Scene` depende diretamente de `ElementoViewModel`. Isso torna a cena uma estrutura de apresentação, não um scene graph puramente geométrico ou independente de WPF. Os nós (`ElementoNode`) estão em `Core`, mas são consumidos por ViewModels e não formam uma árvore própria de renderização.

# 14. Dívidas Técnicas

As dívidas listadas abaixo são baseadas em evidências observadas no código.

## Cena acoplada a ViewModels

`Core/Scenes/Scene.cs` define `ObservableCollection<ElementoViewModel>`. Por estar em `Core`, mas depender de `Araci.ViewModels`, a cena mistura uma camada nominalmente central com objetos de apresentação. Isso reduz a independência do scene graph e dificulta a substituição de WPF por outro backend visual.

## Duplicação de cálculo de rotação de bounds

Há lógica de rotação de retângulo em `ElementoViewModel.ObterBoundsRotacionado` e também em `SceneQueryService.ObterBoundsRotacionado`. As duas implementações calculam os quatro cantos, rotacionam em torno do centro e produzem um novo `Rect`. Essa duplicação aumenta risco de divergência futura.

## Índice espacial tem métodos incrementais, mas o serviço usa rebuild preguiçoso

`ISpatialIndex` e `SpatialHashGrid` expõem `Add`, `Remove` e `Update`, mas `SceneQueryService` invalida e reconstrói o índice inteiro em `GarantirIndex()`. Isso é simples e consistente com o código atual, mas pode se tornar custo perceptível em cenas grandes ou durante muitas consultas após alterações frequentes.

## `SpatialHashGrid.Remove` depende dos bounds atuais

`SpatialHashGrid.Remove(ElementoViewModel elemento)` calcula células com `elemento.Bounds` no momento da chamada. Se a remoção incremental fosse usada após a mudança de bounds, poderia remover a partir das células novas, não necessariamente das células antigas. Atualmente esse risco é mitigado pelo rebuild preguiçoso em `SceneQueryService`, mas permanece na API incremental.

## `CableVertexHandleLayer` existe no XAML, mas não apresenta binding próprio

`Views/ViewportView.xaml` declara `CableVertexHandleLayer` como `Canvas` vazio e aplica a transformação da câmera nele. A edição de vértices, porém, é visível principalmente em `CaboControl`, que cria elipses internas para handles intermediários. O serviço `CableVertexEditService` mantém `Handles`, mas a camada dedicada não mostra template ou binding para essa coleção no XAML analisado.

## Estado visual parcialmente não usado

`ElementoVisualState` contém `IsVisivel` e `IsTravado`, mas a renderização analisada usa principalmente seleção, hover, stroke e espessura. Não foi observado binding direto de `IsVisivel` ou `IsTravado` nos controles lidos.

## Mistura de hit test WPF e hit test geométrico

`ViewportView.EncontrarElemento` usa a árvore visual do WPF para localizar o ViewModel sob o mouse. `SelecionarTool`, quando necessário, também usa `ISceneQueryService.HitTest`. A convivência dos dois mecanismos é funcional, mas cria dois caminhos possíveis de identificação visual do elemento.

## Dependência direta de propriedades por string em controles

`ElementoControlBase` mantém um conjunto de nomes de propriedades visuais, incluindo `"X2"` e `"Y2"`. Isso funciona com `PropertyChanged`, mas não é fortemente tipado para todas as entradas, o que aumenta risco de inconsistências em renomeações futuras.

# 15. Comparação com Arquitetura-Alvo

Considerando a arquitetura CAD/BIM pretendida para uma engine visual mais ampla, o código atual apresenta uma base funcional para viewport 2D, mas ainda se encontra em um estágio mais próximo de uma composição MVVM/WPF do que de um scene graph CAD/BIM totalmente independente.

| Aspecto | Estado atual observado | Direção arquitetural compatível com CAD/BIM |
|---|---|---|
| Scene graph | `Scene` contém `ElementoViewModel`. | Scene graph poderia ser independente de ViewModels e WPF. |
| Geometria | `ElementoNode` calcula bounds e movimento. | Geometria poderia evoluir para primitivas e entidades renderizáveis mais ricas. |
| Renderização | DataTemplates WPF e controles específicos. | Backend visual poderia ser desacoplado de WPF, preservando adapters. |
| Consulta espacial | `SceneQueryService` com `SpatialHashGrid`. | Pode evoluir para índice robusto para muitos elementos, layers e tipos de geometria. |
| Câmera | `Camera` 2D com zoom, pan e fit. | Base adequada para CAD 2D; ambiente 3D exigiria outra abstração. |
| Interação | `InputRouter` e ferramentas coordenam mouse/teclado. | Modelo é extensível para novas ferramentas, desde que comandos visuais sejam mantidos coesos. |
| Controles | SVGs e WPF controls por tipo de elemento. | Simbologia pode evoluir para catálogo gráfico e renderização parametrizada. |
| Atualização visual | PropertyChanged e invalidation preguiçosa. | Cenas grandes podem exigir invalidação incremental e batching explícito. |

O núcleo visual existente já cobre os fundamentos de uma aplicação CAD 2D: coordenadas de mundo, câmera, pan, zoom, seleção, hover, drag, box selection, snap por terminal, guias de alinhamento, visualização por elementos e consulta espacial. Também existe suporte específico para cabos com polilinhas, vértices intermediários e preview de inserção.

Ao mesmo tempo, a arquitetura atual ainda não representa uma engine gráfica independente. A cena está vinculada a `ElementoViewModel`, os controles WPF carregam diretamente assets SVG, e parte das regras geométricas de consulta está duplicada entre ViewModel e query service. Para uma arquitetura-alvo CAD/BIM mais madura, a principal evolução seria separar de forma mais clara:

```text
Modelo de domínio
    |
    v
Modelo visual independente
    |
    v
Scene graph geométrico
    |
    v
Adaptador WPF
    |
    v
Controles / Renderização
```

Essa direção não invalida o desenho atual. O código existente oferece uma base coerente para edição 2D em WPF e já contém mecanismos importantes de consulta e navegação. A recomendação arquitetural é tratar a implementação atual como uma camada visual WPF funcional, com oportunidades claras de extração futura caso o Araci avance para múltiplos backends, grande volume de entidades, simbologia parametrizada ou ambiente 3D.
