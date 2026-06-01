# Araci

CAD/BIM Elétrico 2D para modelagem, documentação e simulação de sistemas elétricos de potência.

![Status](https://img.shields.io/badge/status-em%20desenvolvimento-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![WPF](https://img.shields.io/badge/WPF-Desktop-blue)
![License](https://img.shields.io/badge/license-Apache%202.0-green)

---

# Visão Geral

O Araci é uma plataforma CAD/BIM elétrica desenvolvida em C# e WPF com foco em:

* Modelagem de diagramas unifilares
* Representação topológica de redes elétricas
* Simulação de fluxo de potência
* Integração com OpenDSS
* Evolução para ambiente BIM elétrico

O projeto nasceu com o objetivo de fornecer uma arquitetura moderna para análise, documentação e simulação de sistemas elétricos de distribuição, subestações, parques solares e parques eólicos.

---

# Principais Funcionalidades

## Modelagem Elétrica

* Barras
* Cabos
* Cargas
* Geradores
* Sistema Interligado Nacional (SIN)
* Transformadores de dois enrolamentos

## Edição CAD

* Inserção de elementos
* Snap em terminais
* Conexões topológicas
* Seleção simples e múltipla
* Movimentação
* Rotação
* Resize de barras
* Undo/Redo

## Topologia Elétrica

* Sistema baseado em terminais
* Conectividade por identificadores
* Construção automática do grafo elétrico
* Validação topológica
* Detecção de conexões inválidas

## Simulação

* Integração OpenDSS
* Pipeline de simulação desacoplado
* Exportação DSS
* Aplicação automática de resultados
* Atualização visual de correntes

---

# Arquitetura

O Araci utiliza uma arquitetura híbrida baseada em:

* MVVM
* Domain-Centric Design
* Scene Graph
* Command Pattern
* Service Layer
* Application Layer
* Composition Root

Fluxo arquitetural simplificado:

```text
Usuário
   ↓
Views
   ↓
ViewModels
   ↓
Application
   ↓
Domain
   ↓
Infrastructure
   ↓
OpenDSS
```

---

# Arquitetura Elétrica

```text
AraciDocument
      ↓
ElectricGraphBuilder
      ↓
ElectricGraph
      ↓
TopologyValidator
      ↓
ParameterReader
      ↓
CircuitBuilder
      ↓
OpenDSS
```

---

# Tecnologias

## Front-End

* C#
* .NET 8
* WPF
* XAML

## Arquitetura

* MVVM
* Scene Graph
* Command Pattern
* Dependency Composition

## Simulação

* OpenDSS
* FastAPI
* py-dss-interface

---

# Estrutura do Projeto

```text
Applications/
Core/
Controls/
DTOs/
Infrastructure/
Models/
Properties/
Resources/
Ribbon/
Service/
ViewModels/
Views/
Araci.TechnicalChecks/
Documentation/
```

---

# Documentação

A documentação arquitetural completa está disponível em:

```text
Documentation/
```

Documentos principais:

* Visão Geral do Produto
* Mapeamento do Código Atual
* Domínio e Modelo do Projeto
* Scene Graph e Rendering
* Conexões, Terminais e Topologia
* Persistência
* Simulação OpenDSS

---

# Estado Atual

Implementado:

* Diagramas unifilares
* Engine gráfica própria
* Sistema de terminais
* Grafo elétrico derivado
* Persistência .araci
* Fluxo de corrente via OpenDSS
* Undo/Redo
* Edição em massa
* Simulação integrada

Em desenvolvimento:

* Catálogo de elementos
* Sistema de anotações
* Blocos reutilizáveis
* DXF/DWG
* IFC
* Múltiplos diagramas
* Ambiente 3D

---

# Roadmap

Curto prazo:

* Catálogo de elementos
* Sistema de anotações
* Evolução da persistência
* Melhorias de produtividade CAD

Médio prazo:

* Importação e exportação DXF
* IFC
* Bibliotecas de fabricantes

Longo prazo:

* Ambiente BIM elétrico
* Múltiplos diagramas
* GIS
* Integrações corporativas

---

# Objetivos do Projeto

O objetivo do Araci não é apenas ser um editor de diagramas.

A visão de longo prazo é evoluir para uma plataforma integrada de engenharia elétrica capaz de unir:

* Modelagem
* Documentação
* Simulação
* Análise
* Integração BIM

em uma única arquitetura.

---

# Autor

Lucas Xavier

Engenheiro Eletricista e Desenvolvedor de Software.

Áreas de interesse:

* Sistemas Elétricos de Potência
* CAD/BIM
* Arquitetura de Software
* Simulação Elétrica
* OpenDSS
* Engenharia Assistida por Computador

---

# Licença

Este projeto está licenciado sob os termos da Apache License 2.0.

Consulte o arquivo LICENSE para mais informações.
