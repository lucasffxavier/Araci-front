# 1. Introdução

O Araci é uma plataforma CAD/BIM elétrica 2D desenvolvida em WPF/.NET 8 para modelagem, documentação e simulação de sistemas elétricos. Seu propósito central é oferecer um ambiente técnico especializado para representação de redes elétricas, com foco em diagramas unifilares, elementos elétricos fundamentais e integração com simulação por OpenDSS.

Como produto, o Araci se posiciona entre o desenho técnico assistido por computador, a modelagem orientada a dados e a análise elétrica. Essa combinação permite que o projeto elétrico deixe de ser apenas uma representação gráfica e passe a carregar informações estruturadas sobre os componentes modelados. Em vez de tratar linhas, símbolos e textos como entidades isoladas, o Araci busca organizar o modelo como um conjunto de objetos elétricos reconhecíveis, persistíveis e passíveis de simulação.

O projeto nasce no contexto de aplicações técnicas em que a documentação elétrica precisa ser compreensível para pessoas e, ao mesmo tempo, suficientemente estruturada para processamento computacional. A representação 2D continua sendo essencial para a comunicação técnica, principalmente em diagramas unifilares. Contudo, a evolução dos processos de engenharia exige que essa representação seja acompanhada de dados, validações, histórico de alterações e possibilidade de interoperabilidade com mecanismos externos de cálculo e análise.

O Araci adota WPF como tecnologia de interface e .NET 8 como plataforma de execução. Essa escolha é compatível com aplicações desktop de engenharia que exigem interação gráfica, desenho em canvas, manipulação precisa de objetos, resposta visual imediata e organização de funcionalidades em uma arquitetura evolutiva. O ambiente desktop também favorece cenários em que o usuário precisa trabalhar com arquivos de projeto, documentação local e fluxos técnicos intensivos.

Este documento apresenta a visão geral do produto. Seu público inclui novos desenvolvedores, arquitetos de software, gestores técnicos, parceiros de integração e usuários interessados em compreender o papel do Araci dentro de um processo de engenharia elétrica. O objetivo não é documentar detalhes de implementação nem prometer funcionalidades não existentes. A intenção é estabelecer uma visão arquitetural, acadêmica e corporativa sobre o produto, seus objetivos, seu escopo atual, seu escopo futuro e sua direção de longo prazo.

Ao longo do documento, as funcionalidades são separadas em duas categorias:

| Categoria | Significado |
| --- | --- |
| Escopo atual | Recursos que fazem parte da visão funcional já definida para o Araci conforme o contexto fornecido. |
| Escopo futuro | Capacidades planejadas ou desejadas para evolução do produto, sem caracterizá-las como disponíveis. |

Essa distinção é importante para preservar precisão técnica. O Araci deve ser entendido como uma plataforma em evolução, com uma base voltada à modelagem elétrica 2D e com potencial de expansão para catálogos, interoperabilidade, múltiplos diagramas, integração geográfica, integração corporativa e ambiente tridimensional.

# 2. Motivação do Projeto

A engenharia elétrica depende de documentação clara, consistente e tecnicamente rastreável. Em muitos fluxos de trabalho, diagramas unifilares, listas de componentes, informações de cabos, cargas, transformadores e geradores são produzidos e mantidos em ferramentas diferentes. Essa fragmentação tende a criar retrabalho, inconsistência e dificuldade de atualização. Uma alteração no desenho pode não ser refletida nos dados; uma alteração nos dados pode não ser refletida no desenho; uma simulação pode depender de informações que não estão organizadas de forma diretamente reutilizável.

O Araci é motivado pela necessidade de aproximar representação gráfica, informação técnica e análise elétrica. A proposta é criar um ambiente em que os elementos de um sistema elétrico sejam modelados como componentes reconhecíveis, e não apenas como primitivas gráficas. Barras, cabos, cargas, geradores, transformadores e referências ao SIN podem ser tratados como entidades do domínio elétrico, permitindo que o modelo seja interpretado, salvo, restaurado e exportado para simulação.

Outro fator motivador é a necessidade de ferramentas mais específicas para o domínio elétrico. Aplicações CAD genéricas são muito flexíveis, mas frequentemente delegam ao usuário a responsabilidade de manter coerência semântica entre símbolos, textos e conexões. Ferramentas de análise elétrica, por outro lado, podem ser robustas para simulação, mas nem sempre oferecem uma experiência de desenho 2D voltada à documentação visual de engenharia no mesmo fluxo. O Araci busca ocupar esse espaço intermediário: um produto visual, orientado a objetos elétricos e preparado para interação com simulação.

A motivação também é arquitetural. Uma plataforma CAD/BIM elétrica precisa nascer com separação clara entre interface, modelo, persistência, comandos e integração com mecanismos externos. Essa separação favorece evolução gradual do produto. Recursos como Undo/Redo, persistência em arquivo `.araci` e simulação OpenDSS exigem organização interna consistente, pois não podem depender apenas da aparência gráfica apresentada ao usuário. Eles dependem de um estado de aplicação que possa ser manipulado, serializado, revertido e transformado em representação adequada para análise.

Em termos corporativos, o Araci responde à demanda por uma ferramenta capaz de consolidar conhecimento técnico dentro de um ambiente próprio. Projetos elétricos são ativos de informação. Quando representados de forma estruturada, eles podem se tornar insumos para documentação, auditoria, revisão, simulação, planejamento e integração com outros sistemas. Ainda que o escopo atual seja concentrado na modelagem 2D e na simulação por OpenDSS, a visão mais ampla considera o Araci como base para um ecossistema técnico.

Em termos acadêmicos, o projeto também é relevante por tratar da convergência entre CAD, BIM, modelagem de sistemas elétricos e simulação computacional. O desafio não está apenas em Anotar, mas em representar conceitos de engenharia de modo que possam ser compreendidos por usuários e por algoritmos. Esse equilíbrio entre expressividade visual e estrutura de dados é um dos fundamentos da motivação do produto.

# 3. Problemas que o Araci Resolve

O Araci resolve problemas relacionados à criação, organização, persistência e análise de modelos elétricos 2D. Esses problemas não devem ser entendidos apenas como dificuldades operacionais de desenho, mas como desafios de gestão da informação técnica.

O primeiro problema é a separação entre desenho e significado. Em ferramentas genéricas, uma barra pode ser apenas um conjunto de linhas, uma carga pode ser apenas um símbolo e um cabo pode ser apenas uma conexão visual. Essa abordagem permite liberdade gráfica, mas dificulta o processamento posterior. O Araci aborda esse problema ao tratar elementos como barras, cabos, cargas, geradores e transformadores dentro de um vocabulário elétrico explícito.

O segundo problema é a dificuldade de manter coerência em diagramas unifilares. Diagramas unifilares são representações sintéticas de sistemas elétricos. Eles reduzem a complexidade visual, mas exigem precisão semântica. Um erro de conexão, uma carga mal posicionada, uma relação incorreta entre componentes ou uma alteração não registrada pode comprometer a qualidade da documentação. Ao estruturar o ambiente em torno desses elementos, o Araci cria uma base mais adequada para controle e evolução do modelo.

O terceiro problema é a persistência do projeto. Um modelo técnico precisa ser salvo e recuperado mantendo sua estrutura. A persistência `.araci` atende a essa necessidade ao estabelecer um formato próprio de projeto. O objetivo da persistência não é apenas armazenar uma imagem ou um desenho final, mas preservar o estado técnico necessário para continuidade do trabalho.

O quarto problema é a ausência de integração natural entre modelagem e simulação. A simulação OpenDSS representa uma capacidade relevante porque permite que o modelo elétrico seja associado a um mecanismo de análise. O Araci, ao considerar a simulação OpenDSS em seu escopo atual, aponta para um fluxo em que a modelagem 2D pode alimentar processos computacionais de avaliação elétrica.

O quinto problema é a reversibilidade das ações do usuário. Em ambientes CAD, operações de edição são frequentes, incrementais e muitas vezes exploratórias. O suporte a Undo/Redo é fundamental para produtividade e segurança operacional. Ele permite que o usuário experimente alterações, corrija erros e retorne a estados anteriores sem comprometer o projeto.

O sexto problema é a necessidade de uma plataforma extensível. Mesmo que o escopo atual seja delimitado, o produto precisa ser capaz de evoluir para catálogo, anotações, blocos, interoperabilidade com DXF, DWG e IFC, múltiplos diagramas, integração GIS, integração ERP e ambiente 3D. A existência de um escopo futuro explícito reduz ambiguidade estratégica e orienta decisões arquiteturais desde o início.

De forma resumida, os problemas tratados pelo Araci podem ser organizados da seguinte forma:

| Problema | Abordagem do Araci |
| --- | --- |
| Desenho sem semântica elétrica | Modelagem por elementos elétricos reconhecíveis. |
| Diagramas difíceis de manter | Organização do modelo em torno de entidades do domínio. |
| Falta de continuidade entre sessões | Persistência em arquivo `.araci`. |
| Distância entre desenho e análise | Integração com simulação OpenDSS. |
| Edição sem reversibilidade | Suporte a Undo/Redo. |
| Evolução limitada | Visão futura para interoperabilidade, integrações e ambiente 3D. |

# 4. Objetivos do Projeto

O objetivo principal do Araci é fornecer uma plataforma CAD/BIM elétrica 2D para criação e manipulação de modelos elétricos com valor documental e computacional. Esse objetivo combina três dimensões: representação gráfica, estruturação de dados e preparação para análise.

No plano gráfico, o Araci deve permitir a construção de diagramas unifilares e a manipulação de elementos elétricos essenciais. A interface precisa apoiar o usuário na composição visual do sistema, respeitando a natureza técnica do desenho elétrico. O diagrama é a superfície de comunicação entre o usuário e o modelo.

No plano de dados, o Araci deve representar os componentes como entidades compreensíveis para a aplicação. Barras, cabos, cargas, geradores, transformadores e SIN não devem ser apenas formas visuais. Eles devem existir como elementos do modelo, com identidade suficiente para serem persistidos, restaurados, manipulados e utilizados em processos posteriores.

No plano analítico, o Araci deve conectar o modelo a simulações OpenDSS. Essa integração é relevante porque aproxima a documentação elétrica de processos de análise. O modelo criado no ambiente gráfico pode servir como base para simulação, reduzindo a distância entre o desenho e o cálculo.

Entre os objetivos específicos do projeto, destacam-se:

- Disponibilizar um ambiente 2D especializado para modelagem elétrica.
- Apoiar a elaboração de diagramas unifilares.
- Representar elementos fundamentais de sistemas elétricos, como barras, cabos, cargas, geradores, SIN e transformadores.
- Persistir projetos em arquivos `.araci`.
- Suportar simulação por OpenDSS.
- Oferecer Undo/Redo para operações de edição.
- Estabelecer uma base arquitetural que possa evoluir para catálogo, anotações, blocos e interoperabilidade.
- Preparar o produto para futuras integrações com formatos, sistemas corporativos, dados geográficos e ambiente 3D.

Esses objetivos não implicam que todas as capacidades futuras estejam presentes no produto atual. Eles definem direção técnica e estratégica. A distinção entre base atual e evolução futura é parte da maturidade do projeto.

Do ponto de vista de arquitetura de software, o Araci deve favorecer separação de responsabilidades. A camada visual não deve concentrar todo o significado do modelo. A persistência não deve depender apenas de coordenadas visuais. A simulação não deve ser tratada como uma ação isolada desconectada do modelo. O Undo/Redo não deve ser um complemento superficial, mas parte da lógica de manipulação de estado.

Do ponto de vista de gestão de produto, o Araci busca criar uma linguagem comum entre usuários técnicos, desenvolvedores e arquitetos. Essa linguagem comum é essencial para que o produto possa crescer sem perder coerência. Quando todos compreendem quais elementos pertencem ao escopo atual e quais pertencem ao escopo futuro, as decisões de implementação, priorização e integração tornam-se mais objetivas.

# 5. Escopo Atual

O escopo atual do Araci compreende a modelagem elétrica 2D, a representação de elementos fundamentais, a persistência em formato `.araci`, a simulação OpenDSS e o suporte a Undo/Redo. Esse escopo deve ser entendido como a base funcional da plataforma.

## 5.1 Diagramas unifilares

Diagramas unifilares são a principal forma de representação abordada no escopo atual. Eles descrevem sistemas elétricos de maneira sintética, utilizando uma única linha ou representação simplificada para comunicar conexões, relações e fluxo lógico entre componentes.

No contexto do Araci, o diagrama unifilar é mais do que um desenho. Ele é a representação visual de um modelo elétrico estruturado. A tela 2D serve como ambiente para organizar elementos como barras, cabos, cargas, geradores, SIN e transformadores. Essa abordagem permite que o usuário trabalhe com uma linguagem gráfica familiar à engenharia elétrica, enquanto a aplicação mantém uma estrutura interna capaz de sustentar persistência e simulação.

O diagrama unifilar também funciona como ponto de convergência entre documentação e análise. Ele comunica o sistema para pessoas e, ao mesmo tempo, fornece uma base para transformação em dados de simulação.

## 5.2 Barras

Barras representam pontos ou estruturas de conexão dentro do sistema elétrico. Em um modelo elétrico, elas são elementos fundamentais para organizar a topologia do sistema, pois servem como referências de conexão entre fontes, cargas, transformadores e cabos.

No Araci, barras fazem parte do vocabulário de modelagem atual. A presença desse elemento indica que o produto considera a topologia elétrica como parte central do modelo, e não apenas como efeito visual. Barras são relevantes para interpretação de conexões, persistência do projeto e preparação do modelo para simulação.

## 5.3 Cabos

Cabos representam ligações elétricas entre elementos. Em diagramas unifilares, eles expressam relações de conexão e continuidade. No Araci, cabos compõem o escopo atual como entidades necessárias para estruturar o sistema modelado.

A inclusão de cabos no modelo permite que o diagrama represente não apenas a presença de equipamentos, mas também suas relações elétricas. Isso é essencial para qualquer fluxo que pretenda evoluir de desenho para análise. Sem conexões reconhecíveis, o modelo não teria topologia suficiente para persistência estruturada ou simulação.

## 5.4 Cargas

Cargas representam pontos de consumo ou demanda elétrica. Elas são componentes essenciais em sistemas elétricos e aparecem no escopo atual do Araci como entidades modeláveis.

A presença de cargas permite que o diagrama represente a finalidade operacional do sistema. Em conjunto com barras, cabos, fontes e transformadores, as cargas ajudam a compor a estrutura lógica necessária para documentação e simulação. No contexto do produto, cargas devem ser entendidas como elementos do domínio elétrico, e não apenas como símbolos gráficos.

## 5.5 Geradores

Geradores representam fontes de energia no modelo elétrico. Sua inclusão no escopo atual permite representar sistemas em que a geração faz parte da configuração analisada ou documentada.

No Araci, geradores aparecem como componentes relevantes para a modelagem de sistemas elétricos que não dependem exclusivamente de uma referência externa de suprimento. A presença desse elemento amplia a capacidade do diagrama unifilar de representar diferentes arranjos elétricos.

## 5.6 SIN

O SIN, no contexto do escopo atual, é tratado como elemento relacionado ao sistema elétrico modelado. A sua presença no conjunto de entidades indica a necessidade de representar uma referência sistêmica ou conexão associada ao Sistema Interligado Nacional, conforme a terminologia fornecida.

No Araci, a inclusão do SIN deve ser compreendida de forma controlada: trata-se de um componente previsto no escopo atual do produto, sem extrapolar funcionalidades específicas não informadas. Sua relevância está em permitir que o modelo considere uma referência de sistema externo ou interligado dentro da representação elétrica.

## 5.7 Transformadores

Transformadores são componentes essenciais em sistemas elétricos, pois representam mudança de nível de tensão e acoplamento entre partes do sistema. No Araci, eles fazem parte do escopo atual de modelagem.

A inclusão de transformadores é importante para que diagramas unifilares possam representar sistemas elétricos mais realistas. Transformadores conectam segmentos do sistema, relacionam barras, cabos, fontes e cargas, e são elementos significativos para processos de documentação e simulação.

## 5.8 Persistência `.araci`

A persistência em arquivo `.araci` é um elemento fundamental do escopo atual. Ela permite salvar e restaurar projetos dentro de um formato próprio da plataforma.

Um formato de persistência próprio é relevante porque o Araci não trabalha apenas com uma imagem estática. O arquivo precisa preservar o modelo de trabalho, incluindo os elementos elétricos, suas relações e as informações necessárias para continuidade do projeto. A extensão `.araci` identifica o arquivo como artefato nativo da plataforma.

A persistência também estabelece uma fronteira importante entre sessão de trabalho e projeto técnico. O usuário pode interromper, retomar, revisar e evoluir o modelo sem depender de reconstrução manual. Para desenvolvedores e arquitetos, esse recurso exige atenção à compatibilidade, à integridade dos dados e à evolução do formato ao longo do tempo.

## 5.9 Simulação OpenDSS

A simulação OpenDSS faz parte do escopo atual do Araci. O OpenDSS é considerado aqui como mecanismo externo de simulação associado ao modelo elétrico criado na plataforma.

A integração com simulação permite que o modelo não seja apenas documental, mas também analítico. O objetivo é aproximar o processo de desenho e modelagem da capacidade de avaliar o comportamento elétrico por meio de uma ferramenta especializada.

Do ponto de vista arquitetural, a simulação exige transformação do modelo interno em uma representação adequada para o OpenDSS. Essa transformação depende da qualidade do modelo e da clareza das entidades elétricas. Por isso, a existência de barras, cabos, cargas, geradores, SIN e transformadores como elementos do domínio é relevante para o fluxo de simulação.

## 5.10 Undo/Redo

Undo/Redo é uma funcionalidade essencial para ambientes de edição técnica. Em uma plataforma CAD/BIM elétrica 2D, o usuário realiza operações frequentes de criação, movimentação, ajuste e reorganização de elementos. A possibilidade de desfazer e refazer ações aumenta a segurança do processo de trabalho e reduz o custo de correção.

No Araci, Undo/Redo deve ser entendido como parte do controle de estado do modelo. Não se trata apenas de uma conveniência de interface. A reversibilidade das operações precisa respeitar a estrutura do projeto, preservando coerência entre representação visual, entidades elétricas, persistência e demais fluxos associados.

## 5.11 Síntese do escopo atual

| Item | Papel no produto |
| --- | --- |
| Diagramas unifilares | Representação principal do sistema elétrico em 2D. |
| Barras | Estruturas de conexão e referência topológica. |
| Cabos | Relações de ligação entre elementos elétricos. |
| Cargas | Pontos de consumo ou demanda elétrica. |
| Geradores | Fontes de energia representadas no modelo. |
| SIN | Referência sistêmica associada ao sistema interligado. |
| Transformadores | Elementos de acoplamento e transformação elétrica. |
| Persistência `.araci` | Formato nativo para salvar e restaurar projetos. |
| Simulação OpenDSS | Integração analítica para avaliação elétrica. |
| Undo/Redo | Controle de reversibilidade das operações de edição. |

# 6. Escopo Futuro

O escopo futuro do Araci descreve direções de evolução. Os itens desta seção não devem ser interpretados como funcionalidades disponíveis no escopo atual, mas como capacidades planejadas, desejadas ou estrategicamente relevantes para a maturidade da plataforma.

## 6.1 Catálogo

Um catálogo futuro poderá organizar componentes, equipamentos, propriedades técnicas e padrões reutilizáveis. Em uma plataforma CAD/BIM elétrica, um catálogo é importante porque reduz inconsistências, acelera a modelagem e aproxima o projeto de dados padronizados.

No contexto do Araci, o catálogo pode funcionar como base para selecionar elementos de forma mais estruturada. Ele também pode apoiar a evolução da persistência, da documentação e da integração com simulação. Entretanto, no presente documento, catálogo é tratado apenas como escopo futuro.

## 6.2 Anotações

Anotações futuras podem ampliar a capacidade documental do produto. Elas podem permitir que o usuário acrescente observações, textos técnicos, referências, comentários e informações complementares ao diagrama.

Em diagramas elétricos, anotações são relevantes porque nem toda informação necessária ao projeto está contida apenas na topologia. Comentários, notas de projeto e indicações de revisão podem ajudar na comunicação entre equipes. No Araci, anotações aparecem como evolução prevista para fortalecer a documentação.

## 6.3 Blocos

Blocos representam um mecanismo comum em ambientes CAD para reutilização de conjuntos gráficos ou técnicos. Como escopo futuro, blocos podem permitir que partes recorrentes de um diagrama sejam organizadas como unidades reutilizáveis.

Para o Araci, blocos podem ser relevantes tanto para produtividade quanto para padronização. Eles podem reduzir repetição manual e apoiar consistência visual. Ainda assim, esta capacidade deve ser entendida como futura, não como parte confirmada do escopo atual.

## 6.4 DXF

DXF é um formato associado à interoperabilidade CAD. A possibilidade futura de trabalhar com DXF pode ampliar a capacidade do Araci de trocar informações com outros ambientes de desenho.

Essa evolução teria relevância para integração com fluxos existentes, migração de desenhos e exportação de documentação. No entanto, como escopo futuro, DXF deve ser tratado como objetivo de interoperabilidade, não como funcionalidade presente.

## 6.5 DWG

DWG é outro formato amplamente relacionado a fluxos CAD. A inclusão de DWG no escopo futuro aponta para a necessidade de compatibilidade com ecossistemas de desenho técnico já utilizados no mercado.

Para o Araci, suporte a DWG poderia facilitar comunicação com parceiros, reaproveitamento de documentação e entrega de arquivos em formatos aceitos por outros ambientes. Neste documento, porém, DWG é apenas uma direção futura.

## 6.6 IFC

IFC é um formato associado à interoperabilidade BIM. A presença de IFC no escopo futuro indica a intenção de aproximar o Araci de fluxos BIM mais amplos.

Como o Araci é descrito como uma plataforma CAD/BIM elétrica 2D, a interoperabilidade com IFC pode representar um caminho para conectar modelos elétricos a outros domínios de projeto. Essa possibilidade deve ser avaliada arquiteturalmente com cuidado, pois IFC envolve estrutura semântica e relações de informação que vão além da exportação gráfica.

## 6.7 Múltiplos diagramas

Múltiplos diagramas representam a capacidade futura de um projeto conter mais de uma representação ou folha lógica. Essa evolução pode ser importante para projetos maiores, nos quais um único diagrama não é suficiente para representar todas as partes do sistema.

No Araci, múltiplos diagramas podem exigir organização de projeto, navegação, consistência entre diagramas e persistência estruturada. O recurso tem impacto arquitetural relevante porque altera a noção de projeto de uma única área de trabalho para um conjunto de representações relacionadas.

## 6.8 GIS

GIS, ou sistemas de informação geográfica, aparece como escopo futuro para integração com dados espaciais. Em sistemas elétricos, a dimensão geográfica pode ser relevante para planejamento, análise territorial, infraestrutura distribuída e relação entre ativos e localização.

Para o Araci, uma integração GIS futura poderia aproximar diagramas elétricos de informações georreferenciadas. Como essa capacidade não pertence ao escopo atual, ela deve ser considerada uma direção estratégica de expansão.

## 6.9 ERP

ERP representa integração com sistemas corporativos de gestão. No escopo futuro, essa integração poderia aproximar o modelo elétrico de processos empresariais, como cadastro de ativos, materiais, custos, planejamento ou controle organizacional.

Para o Araci, ERP é uma possibilidade de evolução corporativa. Sua inclusão indica que o produto pode, no longo prazo, dialogar com sistemas além do ambiente técnico de desenho e simulação. Essa integração exigiria definição de contratos, dados mestres e governança de informação.

## 6.10 Ambiente 3D

O ambiente 3D aparece como visão futura. Embora o escopo atual seja 2D, a evolução para 3D poderia ampliar a capacidade de visualização, coordenação e integração com fluxos BIM.

Essa transição teria impacto significativo. Um ambiente 3D não é apenas uma camada visual adicional; ele exige mudanças em geometria, navegação, representação espacial, persistência e interoperabilidade. Por isso, no Araci, o 3D deve ser tratado como visão de longo prazo ou expansão estruturada, não como extensão trivial do canvas 2D.

## 6.11 Síntese do escopo futuro

| Item futuro | Natureza da evolução |
| --- | --- |
| Catálogo | Padronização e reutilização de componentes. |
| Anotações | Enriquecimento documental dos diagramas. |
| Blocos | Reutilização de conjuntos gráficos ou técnicos. |
| DXF | Interoperabilidade CAD. |
| DWG | Compatibilidade com fluxos CAD consolidados. |
| IFC | Interoperabilidade BIM. |
| Múltiplos diagramas | Organização de projetos com várias representações. |
| GIS | Integração com dados geográficos. |
| ERP | Integração com sistemas corporativos. |
| Ambiente 3D | Expansão espacial e BIM de longo prazo. |

# 7. Público-Alvo

O Araci é destinado a um público técnico e multidisciplinar. Por ser uma plataforma CAD/BIM elétrica 2D, o produto interessa a usuários que trabalham com modelagem, documentação, revisão, análise e gestão de sistemas elétricos.

O primeiro grupo é composto por desenvolvedores que atuarão na evolução da aplicação. Para esse público, o Araci deve ser compreendido como um sistema de software com domínio técnico específico. O conhecimento de WPF/.NET 8 é relevante, mas não suficiente. É necessário compreender que a aplicação representa elementos elétricos e que esses elementos se relacionam com persistência, simulação e comandos de edição.

O segundo grupo é formado por arquitetos de software. Para eles, o interesse está na estrutura da plataforma, na separação de responsabilidades, na evolução do formato `.araci`, na integração com OpenDSS e na preparação para funcionalidades futuras. O Araci exige decisões arquiteturais que equilibrem interface rica, modelo de domínio, serialização, histórico de comandos e interoperabilidade.

O terceiro grupo é composto por gestores técnicos e gestores de produto. Para esse público, o Araci deve ser visto como uma plataforma em construção com valor estratégico. O escopo atual entrega uma base de modelagem e simulação, enquanto o escopo futuro indica caminhos de expansão para catálogo, interoperabilidade, integração corporativa e ambiente 3D. A distinção entre atual e futuro é essencial para planejamento realista.

O quarto grupo inclui parceiros. Parceiros podem ter interesse em integração com formatos, sistemas externos, catálogos, GIS, ERP ou fluxos de simulação. Para esse público, o Araci oferece uma base conceitual de produto que pode evoluir para interoperabilidade controlada.

O quinto grupo inclui usuários finais. Esses usuários precisam de uma ferramenta para criar e manipular diagramas unifilares, representar componentes elétricos, salvar projetos, desfazer e refazer operações e utilizar simulação OpenDSS. Para eles, o valor do produto está na combinação entre desenho técnico e inteligência de modelo.

| Público | Interesse principal |
| --- | --- |
| Desenvolvedores | Evoluir a aplicação respeitando o domínio elétrico e a arquitetura. |
| Arquitetos | Definir estruturas robustas para modelo, persistência, comandos e integrações. |
| Gestores | Planejar produto, escopo, evolução e priorização técnica. |
| Parceiros | Avaliar possibilidades de integração e interoperabilidade. |
| Usuários | Modelar, documentar, persistir e simular sistemas elétricos em 2D. |

Essa diversidade de público exige que a comunicação do produto seja clara. O Araci não deve ser apresentado apenas como editor gráfico, nem apenas como simulador, nem apenas como ferramenta BIM. Ele deve ser compreendido como uma plataforma CAD/BIM elétrica 2D que combina desenho, estruturação de dados e integração analítica.

# 8. Diferenciais Técnicos

Os diferenciais técnicos do Araci devem ser entendidos conceitualmente, sem benchmarking comercial. A comparação com ferramentas como AutoCAD Electrical, ETAP, DigSilent, EPLAN, Bentley e Revit serve apenas para situar o espaço técnico ocupado pelo produto. Não se trata de afirmar superioridade, equivalência ou substituição direta.

O Araci se diferencia por buscar uma síntese específica: ambiente 2D especializado, elementos elétricos estruturados, persistência nativa `.araci`, simulação OpenDSS e base arquitetural para evolução CAD/BIM. Essa combinação define uma identidade própria.

## 8.1 AutoCAD Electrical

AutoCAD Electrical é associado ao universo CAD elétrico e à documentação técnica. Conceitualmente, ele representa uma categoria de ferramenta orientada à criação e gestão de desenhos elétricos em ambiente CAD.

O Araci se aproxima desse universo por também trabalhar com representação elétrica 2D. Entretanto, sua proposta, conforme o contexto fornecido, está centrada em uma plataforma própria desenvolvida em WPF/.NET 8, com foco em diagramas unifilares, persistência `.araci` e simulação OpenDSS. A comparação conceitual mostra que o Araci busca construir uma base própria para modelagem elétrica, em vez de ser apenas uma adaptação de um CAD genérico.

## 8.2 ETAP

ETAP é associado a análise e engenharia de sistemas elétricos. Conceitualmente, esse tipo de ferramenta destaca a dimensão analítica e de simulação.

O Araci se relaciona com esse campo por incluir simulação OpenDSS em seu escopo atual. A diferença conceitual é que o Araci parte de uma plataforma CAD/BIM elétrica 2D, com modelagem visual de diagramas unifilares e persistência própria. Assim, sua identidade está na ponte entre modelagem gráfica e análise, não exclusivamente em análise.

## 8.3 DigSilent

DigSilent é associado à análise de sistemas elétricos de potência. Como referência conceitual, representa ferramentas com forte vocação para estudos elétricos.

O Araci não deve ser descrito como equivalente a esse tipo de solução. Sua proposta, conforme o contexto fornecido, é fornecer um ambiente 2D de modelagem, documentação e simulação com OpenDSS. O diferencial está em estruturar o desenho elétrico como modelo persistível e simulado, mantendo foco em uma plataforma própria.

## 8.4 EPLAN

EPLAN é associado a engenharia elétrica, documentação e processos estruturados de projeto. Conceitualmente, esse universo enfatiza padronização, dados e documentação técnica.

O Araci se aproxima dessa direção ao buscar que os elementos do diagrama sejam entidades do domínio elétrico. No escopo futuro, catálogo, anotações e blocos reforçam uma possível evolução para maior padronização documental. No escopo atual, a base é mais delimitada: diagramas unifilares, componentes elétricos fundamentais, persistência `.araci`, OpenDSS e Undo/Redo.

## 8.5 Bentley

Bentley é associado a soluções de engenharia e infraestrutura, frequentemente com forte relação com modelos, dados e interoperabilidade em ambientes corporativos.

A comparação conceitual com o Araci destaca a importância da visão de longo prazo. GIS, ERP, IFC e ambiente 3D aparecem como possibilidades futuras que dialogam com integração, infraestrutura e dados corporativos. Entretanto, o escopo atual do Araci permanece concentrado em CAD/BIM elétrica 2D e simulação OpenDSS.

## 8.6 Revit

Revit é associado ao BIM e à modelagem de informações de construção. Conceitualmente, representa uma abordagem baseada em objetos, relações e dados de projeto, frequentemente em ambiente 3D.

O Araci compartilha a ambição de tratar elementos como objetos informacionais, mas seu escopo atual é 2D e elétrico. A eventual evolução para IFC e ambiente 3D faz parte do escopo futuro, não da base atual. Portanto, o Araci deve ser entendido como uma plataforma CAD/BIM elétrica 2D com potencial de expansão, e não como uma ferramenta BIM 3D já consolidada.

## 8.7 Síntese comparativa conceitual

| Referência | Ênfase conceitual | Relação conceitual com o Araci |
| --- | --- | --- |
| AutoCAD Electrical | CAD elétrico e documentação 2D. | O Araci também atua em 2D, mas com plataforma própria e persistência `.araci`. |
| ETAP | Análise e engenharia elétrica. | O Araci se conecta à análise por OpenDSS, partindo de modelagem visual. |
| DigSilent | Estudos de sistemas elétricos de potência. | O Araci não se posiciona como equivalente; usa simulação como parte de um fluxo CAD/BIM 2D. |
| EPLAN | Documentação elétrica estruturada. | O Araci compartilha a direção de estruturar informação elétrica, com escopo atual mais delimitado. |
| Bentley | Engenharia, infraestrutura e interoperabilidade. | O Araci aponta para integrações futuras como GIS, ERP, IFC e 3D. |
| Revit | BIM orientado a objetos, usualmente 3D. | O Araci adota visão CAD/BIM elétrica 2D, com 3D como escopo futuro. |

Os diferenciais técnicos do Araci podem ser resumidos nos seguintes pontos:

- Plataforma própria em WPF/.NET 8.
- Foco em CAD/BIM elétrica 2D.
- Modelagem por elementos elétricos do domínio.
- Diagramas unifilares como representação principal.
- Persistência nativa em `.araci`.
- Integração com simulação OpenDSS.
- Undo/Redo como parte do fluxo de edição.
- Visão futura para interoperabilidade CAD, BIM, GIS, ERP e 3D.

# 9. Arquitetura em Alto Nível

A arquitetura em alto nível do Araci pode ser compreendida como uma organização em camadas e fluxos. Como este documento não tem o objetivo de detalhar implementação, os diagramas abaixo representam uma visão conceitual, baseada apenas no contexto fornecido.

## 9.1 Camadas conceituais

```text
+--------------------------------------------------------------+
|                        Usuario                               |
+------------------------------+-------------------------------+
                               |
                               v
+--------------------------------------------------------------+
|                  Interface WPF / Ambiente 2D                 |
|  - Desenho e manipulacao de diagramas unifilares             |
|  - Interacao com elementos eletricos                         |
|  - Operacoes de edicao                                       |
+------------------------------+-------------------------------+
                               |
                               v
+--------------------------------------------------------------+
|                    Modelo Eletrico                           |
|  - Barras                                                    |
|  - Cabos                                                     |
|  - Cargas                                                    |
|  - Geradores                                                 |
|  - SIN                                                       |
|  - Transformadores                                           |
+------------------------------+-------------------------------+
                               |
              +----------------+----------------+
              |                                 |
              v                                 v
+-----------------------------+   +-----------------------------+
|       Persistencia .araci   |   |       Simulacao OpenDSS     |
|  - Salvar projeto           |   |  - Preparar dados           |
|  - Restaurar projeto        |   |  - Executar fluxo analitico |
+-----------------------------+   +-----------------------------+
```

Essa visão destaca a separação entre interação visual, modelo elétrico, persistência e simulação. O usuário interage com a interface WPF, mas a aplicação precisa manter um modelo elétrico coerente. Esse modelo é a base para salvar arquivos `.araci` e para preparar simulações OpenDSS.

## 9.2 Fluxo de edição e Undo/Redo

```text
+------------------+
| Acao do usuario  |
+---------+--------+
          |
          v
+------------------+
| Comando de edicao|
+---------+--------+
          |
          v
+------------------+        +------------------+
| Atualiza modelo  +-------> | Atualiza tela 2D |
+---------+--------+        +------------------+
          |
          v
+------------------+
| Historico        |
| Undo / Redo      |
+---------+--------+
          |
          v
+------------------+
| Estado reversivel|
+------------------+
```

O suporte a Undo/Redo implica que ações de edição devem ser tratadas como operações reversíveis. Conceitualmente, isso significa que a aplicação precisa controlar alterações no modelo e permitir retorno a estados anteriores. O histórico de ações não é apenas um recurso de interface; ele faz parte da integridade do processo de edição.

## 9.3 Fluxo de persistência

```text
+----------------------+
| Modelo eletrico      |
| em memoria           |
+----------+-----------+
           |
           v
+----------------------+
| Serializacao         |
| para formato .araci  |
+----------+-----------+
           |
           v
+----------------------+
| Arquivo de projeto   |
| .araci               |
+----------+-----------+
           |
           v
+----------------------+
| Abertura futura      |
| e reconstrucao       |
+----------------------+
```

A persistência `.araci` representa o ciclo de vida do projeto fora da sessão ativa da aplicação. Ela permite que o modelo seja salvo e posteriormente reconstruído. Para uma plataforma CAD/BIM elétrica, a persistência deve preservar informações suficientes para que o projeto continue sendo editável, compreensível e útil para simulação.

## 9.4 Fluxo de simulação OpenDSS

```text
+----------------------+
| Diagrama unifilar    |
| e modelo eletrico    |
+----------+-----------+
           |
           v
+----------------------+
| Interpretacao dos    |
| elementos do dominio |
+----------+-----------+
           |
           v
+----------------------+
| Preparacao para      |
| OpenDSS              |
+----------+-----------+
           |
           v
+----------------------+
| Simulacao OpenDSS    |
+----------+-----------+
           |
           v
+----------------------+
| Resultado analitico  |
| associado ao modelo  |
+----------------------+
```

Esse fluxo conceitual mostra que a simulação depende da qualidade do modelo. Barras, cabos, cargas, geradores, SIN e transformadores precisam ser reconhecidos como entidades para que a preparação da simulação seja possível. A integração com OpenDSS reforça o caráter analítico do produto.

## 9.5 Visão de evolução arquitetural

```text
                         +--------------------+
                         |   Escopo atual     |
                         +----------+---------+
                                    |
                                    v
+------------------+    +--------------------+    +------------------+
| CAD eletrico 2D  +--> | Modelo eletrico    +--> | OpenDSS          |
+------------------+    +--------------------+    +------------------+
                                    |
                                    v
                         +--------------------+
                         | Persistencia       |
                         | .araci             |
                         +----------+---------+
                                    |
                                    v
                         +--------------------+
                         | Escopo futuro      |
                         | Catalogo           |
                         | Anotacoes          |
                         | Blocos             |
                         | DXF / DWG / IFC    |
                         | Multiplos diagramas|
                         | GIS / ERP / 3D     |
                         +--------------------+
```

A arquitetura deve ser capaz de sustentar evolução sem perder coerência. O escopo futuro inclui capacidades que podem impactar profundamente o produto. Por isso, mesmo que elas não estejam no escopo atual, sua existência como visão orienta escolhas estruturais: modelo bem definido, persistência evolutiva, comandos controlados, separação entre visual e domínio, e interfaces claras para integração.

# 10. Visão de Longo Prazo

A visão de longo prazo do Araci é tornar-se uma plataforma elétrica integrada, capaz de unir modelagem 2D, documentação técnica, simulação, interoperabilidade e integração com ecossistemas corporativos e de engenharia. Essa visão deve ser construída de forma incremental, respeitando a maturidade do produto e a separação entre funcionalidades atuais e futuras.

No curto e médio prazo, a consolidação do escopo atual é fundamental. Diagramas unifilares, barras, cabos, cargas, geradores, SIN, transformadores, persistência `.araci`, simulação OpenDSS e Undo/Redo formam a base sobre a qual as demais capacidades poderão ser construídas. Se essa base for consistente, o produto terá melhores condições de evoluir sem acumular complexidade desnecessária.

No longo prazo, o catálogo pode transformar a forma como componentes são inseridos e padronizados. Anotações e blocos podem enriquecer a documentação e aumentar a produtividade. DXF e DWG podem ampliar a comunicação com fluxos CAD. IFC pode aproximar o Araci do ecossistema BIM. Múltiplos diagramas podem permitir organização de projetos mais complexos. GIS pode conectar o modelo elétrico a localização e território. ERP pode ligar a engenharia a processos corporativos. O ambiente 3D pode expandir a visualização e a coordenação espacial.

Essa evolução sugere que o Araci pode deixar de ser apenas uma ferramenta de desenho e tornar-se um ambiente de informação elétrica. Essa expressão é importante: um ambiente de informação elétrica contém desenho, mas não se limita ao desenho. Ele contém dados, relações, arquivos, históricos, simulações e integrações.

A visão de longo prazo também exige governança arquitetural. A cada nova capacidade, o produto deverá preservar a consistência do modelo. Uma funcionalidade de catálogo, por exemplo, não deve ser apenas uma lista visual de símbolos; ela precisa dialogar com o modelo elétrico. Uma exportação DXF ou DWG não deve comprometer a semântica interna. Uma integração IFC precisa respeitar a diferença entre representação 2D e modelos BIM mais amplos. Uma integração ERP precisa considerar dados corporativos e responsabilidades de atualização. Um ambiente 3D precisa nascer conectado ao modelo, não como visualização isolada.

Em uma perspectiva corporativa, o Araci pode apoiar ciclos de projeto mais controlados. A persistência nativa permite continuidade. Undo/Redo melhora segurança operacional. Simulação OpenDSS aproxima documentação de análise. Interoperabilidade futura pode reduzir barreiras entre equipes e sistemas. Integrações futuras podem transformar o projeto elétrico em parte de uma cadeia maior de informação.

Em uma perspectiva acadêmica, a visão de longo prazo do Araci está ligada à representação computacional do conhecimento elétrico. O produto pode evoluir como um estudo aplicado de como diagramas, modelos, formatos e simulações se relacionam. A tensão produtiva entre desenho 2D, BIM, simulação e integração corporativa é um campo rico de pesquisa e desenvolvimento.

O sucesso dessa visão depende de disciplina. É necessário evitar que o produto se transforme em um conjunto de funcionalidades desconectadas. Cada nova capacidade deve reforçar o núcleo: modelagem elétrica estruturada. O Araci deve crescer a partir de um centro conceitual claro. Esse centro é a representação de sistemas elétricos em um ambiente CAD/BIM 2D, com dados suficientes para persistência, edição reversível e simulação.

# 11. Conclusão

O Araci é uma plataforma CAD/BIM elétrica 2D desenvolvida em WPF/.NET 8 para modelagem, documentação e simulação de sistemas elétricos. Sua proposta combina representação visual, estruturação de dados e integração analítica. O produto parte de um escopo atual definido: diagramas unifilares, barras, cabos, cargas, geradores, SIN, transformadores, persistência `.araci`, simulação OpenDSS e Undo/Redo.

Essa base permite que o Araci seja compreendido como mais do que um editor gráfico. O diagrama unifilar é a face visual do sistema, mas o valor arquitetural está na capacidade de tratar elementos elétricos como entidades do domínio. Essa abordagem favorece persistência, reversibilidade, simulação e evolução futura.

O escopo futuro aponta para uma plataforma mais ampla, com catálogo, anotações, blocos, DXF, DWG, IFC, múltiplos diagramas, GIS, ERP e ambiente 3D. Esses recursos devem ser tratados como visão de evolução, não como funcionalidades atuais. Sua presença no planejamento, porém, é importante para orientar decisões arquiteturais desde a base.

Conceitualmente, o Araci dialoga com categorias representadas por AutoCAD Electrical, ETAP, DigSilent, EPLAN, Bentley e Revit, mas não deve ser avaliado por benchmarking comercial neste documento. A comparação serve para situar seu espaço técnico: uma plataforma própria, elétrica, 2D, orientada a modelo, com persistência nativa e integração com simulação OpenDSS.

Para novos desenvolvedores, o Araci exige compreensão do domínio elétrico e da arquitetura de aplicações desktop técnicas. Para arquitetos, exige atenção à separação entre interface, modelo, persistência, comandos e simulação. Para gestores, oferece uma base clara de produto e uma visão de evolução. Para parceiros, indica possibilidades futuras de integração. Para usuários, propõe um ambiente dedicado à modelagem, documentação e simulação de sistemas elétricos.

Em síntese, o Araci representa uma iniciativa de construção de uma plataforma elétrica especializada. Seu valor está na convergência entre CAD, BIM, domínio elétrico e simulação. Sua evolução dependerá da manutenção de um princípio central: cada recurso deve fortalecer a capacidade do produto de representar sistemas elétricos de forma clara, estruturada, persistente e tecnicamente útil.
