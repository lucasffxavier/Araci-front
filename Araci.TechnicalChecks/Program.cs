using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Araci.API;
using Araci.Applications.Abstractions;
using Araci.Applications.Analisar.FluxoDeCorrente;
using Araci.Applications.Diagrama;
using Araci.Applications.Editar.Base;
using Araci.Applications.Factories;
using Araci.Applications.Projects.Tables;
using Araci.Applications.UseCases.Analise;
using Araci.Applications.UseCases.Editar;
using Araci.Applications.UseCases.Projeto;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Core.Rendering;
using Araci.Core.SceneQueries;
using Araci.DTOs;
using Araci.Infrastructure.Persistence;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Properties;
using Araci.Services;
using Araci.ViewModels;
using Araci.Services.Topology;
using Araci.Services.Editing;
using Araci.Services.Catalog;
using Araci.Services.Settings;
using Araci.Services.Simulation;
using Araci.Views;

namespace Araci.TechnicalChecks
{
    internal static class Program
    {
        private static int Main()
        {
            var tests = new (string Name, Action Run)[]
            {
                ("Circuito simples preserva DTOs via ElectricGraph", CircuitoSimplesPreservaDtos),
                ("ParameterReader CoreApi usa fallback sem ElectricGraph", CoreApiUsaFallbackSemElectricGraph),
                ("Cabo invalido bloqueia DTO final", CaboInvalidoBloqueiaDto),
                ("Cabo duplicado gera erro topologico", CaboDuplicadoGeraErro),
                ("Elementos existentes permanecem eletricos", ElementosExistentesPermanecemEletricos),
                ("ElementoAnotativo nao participa do grafo eletrico", ElementoAnotativoNaoParticipaDoGrafoEletrico),
                ("ElementoAnotativoRetangular preserva base anotativa", ElementoAnotativoRetangularPreservaBaseAnotativa),
                ("LinhaAnotativa preserva dominio anotativo", LinhaAnotativaPreservaDominioAnotativo),
                ("TipoLinhaAnotativa preserva estilo e biblioteca", TipoLinhaAnotativaPreservaEstiloEBiblioteca),
                ("ElectricGraph Build nao altera Document", ElectricGraphBuildNaoAlteraDocument),
                ("ElectricGraph inclui eletricos e ignora anotativo", ElectricGraphIncluiEletricosEIgnoraAnotativo),
                ("DTO permanece identico com anotativo no Document", DtoPermaneceIdenticoComAnotativoNoDocument),
                ("OperationalGraph ignora anotativo", OperationalGraphIgnoraAnotativo),
                ("TopologyValidator ignora anotativo", TopologyValidatorIgnoraAnotativo),
                ("Classificacao eletrica nao depende de nome tipo ou SVG", ClassificacaoEletricaNaoDependeDeNomeTipoOuSvg),
                ("Multiplos geradores preservam slack e restantes", MultiplosGeradoresPreservamSlack),
                ("Cabos em serie preservam orientacao", CabosEmSeriePreservamOrientacao),
                ("Ramificacao simples valida grafo e DTO", RamificacaoSimplesValidaGrafoEDto),
                ("Topologia maior nao altera Document", TopologiaMaiorNaoAlteraDocument),
                ("Ordem de linhas segue ordem do Document", OrdemDeLinhasSegueDocument),
                ("Persistencia preserva topologia simples", PersistenciaPreservaTopologiaSimples),
                ("Persistencia preserva ramificacao", PersistenciaPreservaRamificacao),
                ("Tabela remove filtros de campos removidos com undo redo", TabelaRemoveFiltrosCamposRemovidosComUndoRedo),
                ("Excluir vista limpa filtro de vista da tabela com undo redo", ExcluirVistaLimpaFiltroTabelaComUndoRedo),
                ("Tabela altera multiplas ordenacoes com undo redo", TabelaAlteraMultiplasOrdenacoesComUndoRedo),
                ("Tabela limita ordenacao a cinco regras", TabelaLimitaOrdenacaoACincoRegras),
                ("Tabela remove ordenacoes duplicadas", TabelaRemoveOrdenacoesDuplicadas),
                ("Tabela limpa apenas ordenacao de campo removido", TabelaLimpaApenasOrdenacaoDeCampoRemovido),
                ("Tabela duplica e persiste multiplas ordenacoes", TabelaDuplicaEPersisteMultiplasOrdenacoes),
                ("Tabela converte ordenacao unica legada", TabelaConverteOrdenacaoUnicaLegada),
                ("Tabela data builder gera colunas por campos selecionados", TabelaDataBuilderGeraColunasPorCamposSelecionados),
                ("Tabela data builder gera linhas por categorias", TabelaDataBuilderGeraLinhasPorCategorias),
                ("Tabela data builder respeita filtro de vista", TabelaDataBuilderRespeitaFiltroVista),
                ("Tabela data builder aplica filtro todas", TabelaDataBuilderAplicaFiltroTodas),
                ("Tabela data builder aplica filtro qualquer", TabelaDataBuilderAplicaFiltroQualquer),
                ("Tabela data builder aplica ordenacao multipla", TabelaDataBuilderAplicaOrdenacaoMultipla),
                ("Tabela data builder sem ordenacao preserva ordem", TabelaDataBuilderSemOrdenacaoPreservaOrdem),
                ("Tabela data builder ignora ordenacao invalida sem alterar tabela", TabelaDataBuilderIgnoraOrdenacaoInvalidaSemAlterarTabela),
                ("Tabela data builder geracao repetida read only", TabelaDataBuilderGeracaoRepetidaReadOnly),
                ("Tabela data builder ignora categoria nao selecionada", TabelaDataBuilderIgnoraCategoriaNaoSelecionada),
                ("Tabela CSV exporta cabecalhos linhas e display value", TabelaCsvExportaCabecalhosLinhasEDisplayValue),
                ("Tabela CSV escapa delimitador aspas e quebra", TabelaCsvEscapaDelimitadorAspasEQuebra),
                ("Tabela CSV exportacao respeita builder e nao altera estado", TabelaCsvExportacaoRespeitaBuilderENaoAlteraEstado),
                ("Tabela CSV use case cancela sem escrever", TabelaCsvUseCaseCancelaSemEscrever),
                ("Tabela CSV use case avisa sem tabela", TabelaCsvUseCaseAvisaSemTabela),
                ("Tabela CSV use case mostra erro de escrita", TabelaCsvUseCaseMostraErroDeEscrita),
                ("Prancha nova inicia sem instancias de tabela", PranchaNovaIniciaSemInstanciasTabela),
                ("ProjectSheet possui defaults validos de folha", ProjectSheetPossuiDefaultsValidosFolha),
                ("ProjectSheetType possui defaults validos", ProjectSheetTypePossuiDefaultsValidos),
                ("AraciDocument novo possui tipo padrao de prancha", AraciDocumentNovoPossuiTipoPadraoPrancha),
                ("CriarPrancha associa prancha ao tipo padrao", CriarPranchaAssociaPranchaAoTipoPadrao),
                ("Prancha persiste instancia de tabela", PranchaPersisteInstanciaTabela),
                ("Persistencia preserva propriedades da prancha", PersistenciaPreservaPropriedadesPrancha),
                ("Persistencia preserva tipos de prancha", PersistenciaPreservaTiposPrancha),
                ("Persistencia preserva associacao prancha tipo", PersistenciaPreservaAssociacaoPranchaTipo),
                ("Criar tipo de prancha adiciona item com defaults", CriarTipoPranchaAdicionaItemComDefaults),
                ("Criar tipo de prancha undo redo", CriarTipoPranchaUndoRedo),
                ("Duplicar tipo de prancha copia propriedades", DuplicarTipoPranchaCopiaPropriedades),
                ("Duplicar tipo de prancha undo redo", DuplicarTipoPranchaUndoRedo),
                ("Renomear tipo de prancha altera nome", RenomearTipoPranchaAlteraNome),
                ("Renomear tipo de prancha bloqueia nome vazio", RenomearTipoPranchaBloqueiaNomeVazio),
                ("Renomear tipo de prancha bloqueia duplicado case insensitive", RenomearTipoPranchaBloqueiaDuplicadoCaseInsensitive),
                ("Excluir tipo de prancha remove tipo nao usado", ExcluirTipoPranchaRemoveTipoNaoUsado),
                ("Excluir tipo de prancha undo redo", ExcluirTipoPranchaUndoRedo),
                ("Excluir ultimo tipo de prancha bloqueado", ExcluirUltimoTipoPranchaBloqueado),
                ("Excluir tipo de prancha em uso bloqueado", ExcluirTipoPranchaEmUsoBloqueado),
                ("Excluir tipo em uso nao altera SheetTypeId", ExcluirTipoEmUsoNaoAlteraSheetTypeId),
                ("Project Browser lista tipos de prancha", ProjectBrowserListaTiposPrancha),
                ("ProjectSheetTypeViewModel usa dimensoes do tipo", ProjectSheetTypeViewModelUsaDimensoesTipo),
                ("ProjectSheetTypeViewModel atualiza apos propriedades do tipo", ProjectSheetTypeViewModelAtualizaAposPropriedadesTipo),
                ("ProjectSheetTypeView possui superficie de template", ProjectSheetTypeViewPossuiSuperficieTemplate),
                ("ProjectSheetTypeView mantem bindings dimensoes do tipo", ProjectSheetTypeViewMantemBindingsDimensoesTipo),
                ("Project Browser seleciona tipo abre visualizacao de tipo", ProjectBrowserSelecionaTipoAbreVisualizacaoTipo),
                ("Project Browser seleciona tipo nao altera vista ativa", ProjectBrowserSelecionaTipoNaoAlteraVistaAtiva),
                ("Project Browser seleciona tipo nao altera tabelas ou pranchas", ProjectBrowserSelecionaTipoNaoAlteraTabelasOuPranchas),
                ("Selecao tipo exibe ProjectSheetTypePropertiesViewModel", SelecaoTipoExibeProjectSheetTypePropertiesViewModel),
                ("Propriedades tipo prancha editaveis undo redo", PropriedadesTipoPranchaEditaveisUndoRedo),
                ("Persistencia salva reabre tipos criados e editados", PersistenciaSalvaReabreTiposCriadosEditados),
                ("Operacoes tipo prancha nao alteram tabelas ou pranchas", OperacoesTipoPranchaNaoAlteramTabelasOuPranchas),
                ("Prancha duplica instancias de tabela com copia profunda", PranchaDuplicaInstanciasTabelaComCopiaProfunda),
                ("Duplicar prancha preserva associacao ao tipo", DuplicarPranchaPreservaAssociacaoTipo),
                ("Operacoes de prancha nao alteram tipos indevidamente", OperacoesPranchaNaoAlteramTiposIndevidamente),
                ("Excluir tabela limpa instancias em pranchas com undo redo", ExcluirTabelaLimpaInstanciasPranchaComUndoRedo),
                ("Prancha carrega arquivo antigo sem instancias", PranchaCarregaArquivoAntigoSemInstancias),
                ("Arquivo antigo sem propriedades de prancha usa defaults", ArquivoAntigoSemPropriedadesPranchaUsaDefaults),
                ("Arquivo antigo sem tipos de prancha usa tipo padrao", ArquivoAntigoSemTiposPranchaUsaTipoPadrao),
                ("Prancha ignora instancia orfa no load", PranchaIgnoraInstanciaOrfaNoLoad),
                ("Editar propriedades prancha undo redo", EditarPropriedadesPranchaUndoRedo),
                ("Inserir tabela na prancha cria instancia undo redo", InserirTabelaNaPranchaCriaInstanciaUndoRedo),
                ("Inserir tabela na prancha ignora ids invalidos", InserirTabelaNaPranchaIgnoraIdsInvalidos),
                ("Inserir tabela na prancha multiplas instancias independentes", InserirTabelaNaPranchaMultiplasInstanciasIndependentes),
                ("Inserir multiplas tabelas na prancha em uma operacao", InserirMultiplasTabelasNaPranchaEmUmaOperacao),
                ("Inserir multiplas tabelas undo redo agrupado", InserirMultiplasTabelasUndoRedoAgrupado),
                ("Inserir multiplas tabelas ignora ids invalidos", InserirMultiplasTabelasIgnoraIdsInvalidos),
                ("Inserir multiplas tabelas remove duplicidades", InserirMultiplasTabelasRemoveDuplicidades),
                ("Inserir multiplas tabelas distribui posicoes", InserirMultiplasTabelasDistribuiPosicoes),
                ("InserirTabelaPranchaDialogResult transporta multiplos ids", InserirTabelaPranchaDialogResultTransportaMultiplosIds),
                ("InserirTabelaPranchaWindow seleciona multiplas sem duplicidade", InserirTabelaPranchaWindowSelecionaMultiplasSemDuplicidade),
                ("EditorContext expoe inserir tabela na prancha", EditorContextExpoeInserirTabelaNaPrancha),
                ("ProjectSheetViewModel expoe instancias de tabela", ProjectSheetViewModelExpoeInstanciasTabela),
                ("ProjectSheetViewModel resolve nome da tabela", ProjectSheetViewModelResolveNomeTabela),
                ("ProjectSheetViewModel trata prancha vazia", ProjectSheetViewModelTrataPranchaVazia),
                ("ProjectSheetViewModel trata tabela inexistente", ProjectSheetViewModelTrataTabelaInexistente),
                ("ProjectSheetViewModel refresh nao altera modelo", ProjectSheetViewModelRefreshNaoAlteraModelo),
                ("Mover tabela na prancha move instancia valida", MoverTabelaNaPranchaMoveInstanciaValida),
                ("Mover tabela na prancha undo redo", MoverTabelaNaPranchaUndoRedo),
                ("Mover tabela na prancha ids invalidos nao alteram estado", MoverTabelaNaPranchaIdsInvalidosNaoAlteramEstado),
                ("Mover tabela na prancha sem alteracao nao cria comando", MoverTabelaNaPranchaSemAlteracaoNaoCriaComando),
                ("Mover tabela na prancha move apenas instancia alvo", MoverTabelaNaPranchaMoveApenasInstanciaAlvo),
                ("ProjectSheetViewModel seleciona instancia", ProjectSheetViewModelSelecionaInstancia),
                ("ProjectSheetViewModel limpa selecao", ProjectSheetViewModelLimpaSelecao),
                ("ProjectSheetViewModel refresh preserva selecao existente", ProjectSheetViewModelRefreshPreservaSelecaoExistente),
                ("ProjectSheetViewModel refresh remove selecao inexistente", ProjectSheetViewModelRefreshRemoveSelecaoInexistente),
                ("ProjectSheetViewModel usa dimensoes da prancha", ProjectSheetViewModelUsaDimensoesPrancha),
                ("ProjectSheetView possui superficie de prancha", ProjectSheetViewPossuiSuperficiePrancha),
                ("ProjectSheetViewModel permite instancia fora da folha", ProjectSheetViewModelPermiteInstanciaForaDaFolha),
                ("ProjectSheetViewModel mantem workspace estavel durante preview negativo", ProjectSheetViewModelMantemWorkspaceEstavelDurantePreviewNegativo),
                ("ProjectSheetViewModel mantem workspace estavel durante preview de resize", ProjectSheetViewModelMantemWorkspaceEstavelDurantePreviewResize),
                ("ProjectSheetViewModel zoom altera percentual", ProjectSheetViewModelZoomAlteraPercentual),
                ("ProjectSheetView aplica zoom no workspace", ProjectSheetViewAplicaZoomNoWorkspace),
                ("ProjectSheetView possui folha nomeada para centralizacao", ProjectSheetViewPossuiFolhaNomeadaParaCentralizacao),
                ("ProjectSheetView possui estilos visuais basicos de tabela", ProjectSheetViewPossuiEstilosVisuaisBasicosTabela),
                ("ProjectSheetTableInstanceViewModel expoe dimensoes tabulares basicas", ProjectSheetTableInstanceViewModelExpoeDimensoesTabularesBasicas),
                ("ProjectSheetView usa bindings para dimensoes tabulares", ProjectSheetViewUsaBindingsDimensoesTabulares),
                ("ProjectSheetView mantem bindings dimensoes folha", ProjectSheetViewMantemBindingsDimensoesFolha),
                ("ProjectSheetPropertiesViewModel expoe propriedades editaveis", ProjectSheetPropertiesViewModelExpoePropriedadesEditaveis),
                ("Dividir tabela na prancha cria instancia independente", DividirTabelaNaPranchaCriaInstanciaIndependente),
                ("Dividir tabela na prancha undo redo", DividirTabelaNaPranchaUndoRedo),
                ("ProjectSheetViewModel recorta linhas por faixa da instancia", ProjectSheetViewModelRecortaLinhasPorFaixaInstancia),
                ("ProjectSheetViewModel divide tabela selecionando nova instancia", ProjectSheetViewModelDivideTabelaSelecionandoNovaInstancia),
                ("ProjectSheetView usa instancia real para divisao de tabela", ProjectSheetViewUsaInstanciaRealParaDivisaoTabela),
                ("ProjectSheetViewModel instancia renderiza dados reais da tabela", ProjectSheetViewModelInstanciaRenderizaDadosReaisTabela),
                ("ProjectSheetViewModel instancia trata tabela sem campos", ProjectSheetViewModelInstanciaTrataTabelaSemCampos),
                ("ProjectSheetViewModel instancia trata tabela sem linhas", ProjectSheetViewModelInstanciaTrataTabelaSemLinhas),
                ("Tabela data view model expoe colunas linhas e celulas", TabelaDataViewModelExpoeColunasLinhasECelulas),
                ("Tabela data view model trata tabela sem campos", TabelaDataViewModelTrataTabelaSemCampos),
                ("Tabela data view model trata tabela sem linhas", TabelaDataViewModelTrataTabelaSemLinhas),
                ("Tabela data view model refresh atualiza dados", TabelaDataViewModelRefreshAtualizaDados),
                ("Tabela data view model refresh reativo apos use cases", TabelaDataViewModelRefreshReativoAposUseCases),
                ("Filtros tabela window permite sem filtro", FiltrosTabelaWindowPermiteSemFiltro),
                ("Tabela remove filtro com undo redo", TabelaRemoveFiltroComUndoRedo),
                ("Project Browser seleciona tabela e solicita visualizacao", ProjectBrowserSelecionaTabelaESolicitaVisualizacao),
                ("Project Browser seleciona prancha e solicita visualizacao", ProjectBrowserSelecionaPranchaESolicitaVisualizacao),
                ("Project Browser seleciona vista e restaura viewport", ProjectBrowserSelecionaVistaERestauraViewport),
                ("Project Browser seleciona vista depois de prancha", ProjectBrowserSelecionaVistaDepoisDePrancha),
                ("Project Browser seleciona tabela depois de prancha", ProjectBrowserSelecionaTabelaDepoisDePrancha),
                ("ProjectTableGridView recria colunas dinamicas", ProjectTableGridViewRecriaColunasDinamicas),
                ("DTO permanece equivalente apos reload", DtoPermaneceEquivalenteAposReload),
                ("IDs permanecem estaveis apos reload", IdsPermanecemEstaveisAposReload),
                ("Builds repetidos apos reload nao alteram Document", BuildsRepetidosAposReloadNaoAlteramDocument),
                ("SIN pode ser criado e entra no Document", SinPodeSerCriadoEEntraNoDocument),
                ("SIN aparece no ElectricGraph", SinApareceNoElectricGraph),
                ("SIN preserva Id apos reload", SinPreservaIdAposReload),
                ("Cabos conectados aos terminais do SIN preservam conexoes", CabosConectadosAosTerminaisDoSinPreservamConexoes),
                ("DTOs sem SIN mantem gerador como slack", DtosSemSinMantemGeradorComoSlack),
                ("SIN com gerador vira slack preferencial", SinComGeradorViraSlackPreferencial),
                ("SIN com gerador preserva GeneratorDto real", SinComGeradorPreservaGeneratorDtoReal),
                ("SIN com multiplos geradores preserva todos em Generators", SinComMultiplosGeradoresPreservaTodosGenerators),
                ("Reload preserva GeneratorDto real com SIN", ReloadPreservaGeneratorDtoRealComSin),
                ("Circuito eolico simplificado preserva GeneratorDto", CircuitoEolicoSimplificadoPreservaGeneratorDto),
                ("Multiplos SIN usam primeiro do Document como slack", MultiplosSinUsamPrimeiroDoDocumentComoSlack),
                ("Reload com SIN mantem slack baseado no SIN", ReloadComSinMantemSlackBaseadoNoSin),
                ("OperationalGraph energiza SIN cabo e carga", OperationalGraphEnergizaSinCaboECarga),
                ("OperationalGraph mantem carga isolada desenergizada", OperationalGraphMantemCargaIsoladaDesenergizada),
                ("OperationalGraph energiza ramificacao com barra", OperationalGraphEnergizaRamificacaoComBarra),
                ("OperationalGraph nao propaga por cabo invalido", OperationalGraphNaoPropagaPorCaboInvalido),
                ("OperationalGraph usa gerador como fallback sem SIN", OperationalGraphUsaGeradorComoFallbackSemSin),
                ("OperationalGraph sem fonte nao energiza nos", OperationalGraphSemFonteNaoEnergizaNos),
                ("OperationalGraph rebuild repetido nao altera Document", OperationalGraphRebuildRepetidoNaoAlteraDocument),
                ("OperationalGraph apos reload preserva resultado", OperationalGraphAposReloadPreservaResultado),
                ("Transformador minimo possui terminais primario e secundario", TransformadorMinimoPossuiTerminais),
                ("Transformador aparece no ElectricGraph", TransformadorApareceNoElectricGraph),
                ("Transformador preserva conexoes apos reload", TransformadorPreservaConexoesAposReload),
                ("Transformador entra no DTO minimo", TransformadorEntraNoDtoMinimo),
                ("Transformador usa centro com geometria propria", TransformadorUsaCentroComGeometriaPropria),
                ("Reload preserva DTO detalhado do transformador", ReloadPreservaDtoDetalhadoTransformador),
                ("CircuitDto preserva parametros reais de SIN transformador e carga", CircuitDtoPreservaParametrosReaisSinTransformadorCarga),
                ("DTOs antigos/default preservam SIN e carga", DtosAntigosDefaultPreservamSinECarga),
                ("TopologyValidator aceita SIN transformador e carga sem gerador", TopologyValidatorAceitaSinTransformadorCargaSemGerador),
                ("TopologyValidator aceita gerador legado sem SIN", TopologyValidatorAceitaGeradorLegadoSemSin),
                ("TopologyValidator sem fonte slack falha com mensagem clara", TopologyValidatorSemFonteSlackFalhaComMensagemClara),
                ("TerminalEndpoint identifica conexao por valor", TerminalEndpointIdentificaConexaoPorValor),
                ("TerminalPlacement usa pivo central", TerminalPlacementUsaPivoCentral),
                ("TerminalPlacement ToLocal inverte ToWorld", TerminalPlacementToLocalInverteToWorld),
                ("Rotacao recalcula terminal por posicao local", RotacaoRecalculaTerminalPorPosicaoLocal),
                ("Carga rotacionada alinha terminal com pivo central", CargaRotacionadaAlinhaTerminalComPivoCentral),
                ("Gerador rotacionado alinha terminais com pivo central", GeradorRotacionadoAlinhaTerminaisComPivoCentral),
                ("SIN rotacionado alinha terminais com pivo central", SinRotacionadoAlinhaTerminaisComPivoCentral),
                ("Transformador rotacionado alinha terminais com pivo central", TransformadorRotacionadoAlinhaTerminaisComPivoCentral),
                ("Barra rotacionada alinha terminais com pivo central", BarraRotacionadaAlinhaTerminaisComPivoCentral),
                ("ElectricGraph BFS percorre por conexoes validas", ElectricGraphBfsPercorreConexoesValidas),
                ("Rotacao +90 atualiza modelo", RotacaoMaisNoventaAtualizaModelo),
                ("Rotacao cicla quadrantes", RotacaoCiclaQuadrantes),
                ("Preview preserva rotacao em modelo real", PreviewPreservaRotacaoEmModeloReal),
                ("Preview armazena rotacao antes de existir", PreviewArmazenaRotacaoAntesDeExistir),
                ("Preview existente rotaciona visualmente", PreviewExistenteRotacionaVisualmente),
                ("Update do preview nao reseta rotacao", UpdateDoPreviewNaoResetaRotacao),
                ("Modelo real recebe rotacao do preview", ModeloRealRecebeRotacaoDoPreview),
                ("InputRouter envia Space para insercao sem preview", InputRouterEnviaSpaceParaInsercaoSemPreview),
                ("Ferramenta LinhaAnotativa cria preview segmentos e undo redo", FerramentaLinhaAnotativaCriaPreviewSegmentosEUndoRedo),
                ("LinhaAnotativa inclinada move preservando ancoras", LinhaAnotativaInclinadaMovePreservandoAncoras),
                ("Botoes da Ribbon nao capturam foco", BotoesDaRibbonNaoCapturamFoco),
                ("Viewport continua focavel", ViewportContinuaFocavel),
                ("Catalogo preserva Ribbon ordem e atalhos", CatalogoPreservaRibbonOrdemEAtalhos),
                ("Catalogo registra LinhaAnotativa com ViewModel minima", CatalogoRegistraLinhaAnotativaComViewModelMinima),
                ("LinhaAnotativa expoe propriedades cor e hit-test", LinhaAnotativaExpoePropriedadesCorEHitTest),
                ("Catalogo preserva propriedades nao editaveis", CatalogoPreservaPropriedadesNaoEditaveis),
                ("Catalogo preserva edicao mista", CatalogoPreservaEdicaoMista),
                ("UnitFormatter converte kV e V", UnitFormatterConverteKvEVolt),
                ("UnitFormatter converte m e km", UnitFormatterConverteMetroEKilometro),
                ("PropertiesViewModel default mantem kV", PropertiesViewModelDefaultMantemKv),
                ("PropertiesViewModel exibe tensao em V", PropertiesViewModelExibeTensaoEmVolt),
                ("PropertiesViewModel edita tensao em V e salva kV", PropertiesViewModelEditaTensaoEmVoltESalvaKv),
                ("PropertiesViewModel edita comprimento em km e salva m", PropertiesViewModelEditaComprimentoEmKmESalvaMetro),
                ("UnitsSettingsViewModel copia defaults", UnitsSettingsViewModelCopiaDefaults),
                ("UnitsSettingsViewModel ApplyTo altera settings", UnitsSettingsViewModelApplyToAlteraSettings),
                ("UnitsSettingsViewModel aplica tensao V", UnitsSettingsViewModelAplicaTensaoVolt),
                ("UnitsSettingsViewModel ToUnitDisplaySettings copia selecoes", UnitsSettingsViewModelToUnitDisplaySettingsCopiaSelecoes),
                ("AlterarUnidadesProjetoUseCase aplica VoltageVolt", AlterarUnidadesProjetoUseCaseAplicaVoltageVolt),
                ("AlterarUnidadesProjetoUseCase chama refresh", AlterarUnidadesProjetoUseCaseChamaRefresh),
                ("AlterarUnidadesProjetoUseCase preserva copia completa", AlterarUnidadesProjetoUseCasePreservaCopiaCompleta),
                ("ExecutarSimulacaoUseCase chama pipeline", ExecutarSimulacaoUseCaseChamaPipeline),
                ("ExecutarSimulacaoUseCase atualiza Resultado", ExecutarSimulacaoUseCaseAtualizaResultado),
                ("ExecutarSimulacaoUseCase mostra mensagem", ExecutarSimulacaoUseCaseMostraMensagem),
                ("FluxoDeCorrenteApplication delega para use case", FluxoDeCorrenteApplicationDelegaParaUseCase),
                ("ExecutarSimulacaoUseCase mostra warning em excecao", ExecutarSimulacaoUseCaseMostraWarningEmExcecao),
                ("ExecutarSimulacaoUseCase sem options nao confirma exportacao", ExecutarSimulacaoUseCaseSemOptionsNaoConfirmaExportacao),
                ("NovoProjetoUseCase chama Novo", NovoProjetoUseCaseChamaNovo),
                ("AbrirProjetoUseCase chama AbrirComDialogo", AbrirProjetoUseCaseChamaAbrirComDialogo),
                ("AbrirProjetoUseCase chama Abrir path", AbrirProjetoUseCaseChamaAbrirPath),
                ("SalvarProjetoUseCase chama SalvarComDialogo", SalvarProjetoUseCaseChamaSalvarComDialogo),
                ("SalvarProjetoUseCase chama Salvar path", SalvarProjetoUseCaseChamaSalvarPath),
                ("EditorContext expoe use cases de projeto", EditorContextExpoeUseCasesDeProjeto),
                ("NovoProjetoUseCase real reseta units default", NovoProjetoUseCaseRealResetaUnitsDefault),
                ("Salvar Abrir via use case preserva Units", SalvarAbrirViaUseCasePreservaUnits),
                ("AtualizarPropriedadesSelecionadasUseCase preserva selecao", AtualizarPropriedadesSelecionadasUseCasePreservaSelecao),
                ("SelecionarElementosUseCase seleciona elemento", SelecionarElementosUseCaseSelecionaElemento),
                ("Selecionar LinhaAnotativa usa painel generico", SelecionarLinhaAnotativaUsaPainelGenerico),
                ("SelecionarElementosUseCase limpa selecao", SelecionarElementosUseCaseLimpaSelecao),
                ("SelecionarElementosUseCase cria painel propriedades multipla selecao", SelecionarElementosUseCaseCriaPainelPropriedadesMultiplaSelecao),
                ("EditorContext expoe use cases de selecao", EditorContextExpoeUseCasesDeSelecao),
                ("EditorContext RefreshProperties preserva selecao", EditorContextRefreshPropertiesPreservaSelecao),
                ("UnitValueConverter usa settings em runtime", UnitValueConverterUsaSettingsEmRuntime),
                ("UnitValueConverter converte edicao para unidade base", UnitValueConverterConverteEdicaoParaUnidadeBase),
                ("Persistencia salva Units no JSON", PersistenciaSalvaUnitsNoJson),
                ("Persistencia reabre Voltage em V", PersistenciaReabreVoltageEmVolt),
                ("Persistencia reabre Length em km", PersistenciaReabreLengthEmKm),
                ("Novo projeto reseta units default", NovoProjetoResetaUnitsDefault),
                ("Arquivo antigo sem Units abre defaults", ArquivoAntigoSemUnitsAbreDefaults),
                ("Arquivo com Units invalido usa fallback", ArquivoComUnitsInvalidoUsaFallback),
                ("Units persistidas nao alteram DTO eletrico", UnitsPersistidasNaoAlteramDtoEletrico),
                ("Persistencia preserva LinhaAnotativa", PersistenciaPreservaLinhaAnotativa),
                ("DocumentSceneSync cria ViewModel ao adicionar elemento", DocumentSceneSyncCriaViewModelAoAdicionarElemento),
                ("DocumentSceneSync remove ViewModel ao remover elemento", DocumentSceneSyncRemoveViewModelAoRemoverElemento),
                ("DocumentSceneSync limpa Scene ao limpar Document", DocumentSceneSyncLimpaSceneAoLimparDocument),
                ("DocumentSceneSync preserva CaboViewModel", DocumentSceneSyncPreservaCaboViewModel),
                ("Elemento rotacionado persiste apos reload", ElementoRotacionadoPersisteAposReload),
                ("Terminais mudam posicao e preservam IDs", TerminaisMudamPosicaoEPreservamIds),
                ("Cabo preserva TerminalId apos rotacao", CaboPreservaTerminalIdAposRotacao),
                ("Cabo reancora visualmente apos rotacao", CaboReancoraVisualmenteAposRotacao),
                ("Undo Redo da rotacao restaura elemento e cabos", UndoRedoRotacaoRestauraElementoECabos),
                ("CableVertexEdit cria handles intermediarios", CableVertexEditCriaHandlesIntermediarios),
                ("CableVertexEdit insere vertice no segmento", CableVertexEditInsereVerticeNoSegmento),
                ("CableVertexEdit remove handle intermediario", CableVertexEditRemoveHandleIntermediario),
                ("CableVertexEdit remove handle ativo", CableVertexEditRemoveHandleAtivo),
                ("CableVertexEdit arrasta vertice intermediario", CableVertexEditArrastaVerticeIntermediario),
                ("CableVertexEdit Shift restringe arraste ortogonal", CableVertexEditShiftRestringeArrasteOrtogonal),
                ("CableVertexEdit Cancel restaura estado inicial", CableVertexEditCancelRestauraEstadoInicial),
                ("CableVertexEdit nao insere longe de segmento", CableVertexEditNaoInsereLongeDeSegmento),
                ("CableVertexEdit nao remove longe de handle", CableVertexEditNaoRemoveLongeDeHandle),
                ("CableVertexEdit Clear limpa handles", CableVertexEditClearLimpaHandles),
                ("Rotacao reancora Carga com cabo conectado", RotacaoReancoraCargaComCaboConectado),
                ("Rotacao reancora Gerador com cabo conectado", RotacaoReancoraGeradorComCaboConectado),
                ("Rotacao reancora SIN em todos terminais", RotacaoReancoraSinEmTodosTerminais),
                ("Rotacao reancora Transformador primario e secundario", RotacaoReancoraTransformadorPrimarioSecundario),
                ("Rotacao reancora Barra em dois terminais", RotacaoReancoraBarraEmDoisTerminais),
                ("Undo Redo da rotacao reancora terminais e cabos", UndoRedoRotacaoReancoraTerminaisECabos),
                ("Snap encontra terminal apos rotacao com cabo", SnapEncontraTerminalAposRotacaoComCabo),
                ("ElectricGraph build repetido apos rotacao nao altera Document", ElectricGraphBuildAposRotacaoNaoAlteraDocument),
                ("DTO nao muda por causa da rotacao", DtoNaoMudaPorCausaDaRotacao),
                ("RotationService aceita Barra", RotationServiceAceitaBarra),
                ("Barra nova possui altura padrao", BarraNovaPossuiAlturaPadrao),
                ("Barra padrao mantem 24 terminais com pitch fixo", BarraPadraoMantemVinteQuatroTerminaisComPitchFixo),
                ("Alterar altura da Barra muda Bounds", AlterarAlturaDaBarraMudaBounds),
                ("Crescer Barra aumenta conectores preservando IDs", CrescerBarraAumentaConectoresPreservandoIds),
                ("Reduzir Barra remove terminais livres excedentes", ReduzirBarraRemoveTerminaisLivresExcedentes),
                ("Reduzir Barra preserva terminal ocupado", ReduzirBarraPreservaTerminalOcupado),
                ("Resize da Barra reancora cabo conectado", ResizeDaBarraReancoraCaboConectado),
                ("Undo Redo de resize da Barra preserva cabo", UndoRedoResizeBarraPreservaCabo),
                ("Connectivity retorna terminais ocupados da Barra", ConnectivityRetornaTerminaisOcupadosDaBarra),
                ("Cabo conectado a Barra reancora apos alterar altura", CaboConectadoABarraReancoraAposAlterarAltura),
                ("Barra com altura alterada persiste apos reload", BarraComAlturaAlteradaPersisteAposReload),
                ("ElectricGraph continua valido apos altura da Barra", ElectricGraphContinuaValidoAposAlturaDaBarra),
                ("DTO nao muda por causa da altura da Barra", DtoNaoMudaPorCausaDaAlturaDaBarra),
                ("Rotacao da Barra funciona apos altura alterada", RotacaoDaBarraFuncionaAposAlturaAlterada),
                ("Cabo permanece ancorado apos altura rotacao movimento e reload", CaboPermaneceAncoradoAposAlturaRotacaoMovimentoEReload),
                ("Altura invalida da Barra normaliza para minimo", AlturaInvalidaDaBarraNormalizaParaMinimo),
                ("Barra selecionada rotaciona 0 para 90", BarraSelecionadaRotacionaZeroParaNoventa),
                ("Barra cicla quadrantes", BarraCiclaQuadrantes),
                ("Preview de Barra preserva rotacao", PreviewDeBarraPreservaRotacao),
                ("Barra preserva 24 TerminalIds apos rotacao", BarraPreservaVinteQuatroTerminalIdsAposRotacao),
                ("Terminais da Barra mudam posicao visual apos rotacao", TerminaisDaBarraMudamPosicaoVisualAposRotacao),
                ("Cabo conectado a Barra preserva TerminalId apos rotacao", CaboConectadoABarraPreservaTerminalIdAposRotacao),
                ("Cabo conectado a Barra reancora visualmente apos rotacao", CaboConectadoABarraReancoraVisualmenteAposRotacao),
                ("Undo Redo da rotacao da Barra restaura cabos", UndoRedoRotacaoDaBarraRestauraCabos),
                ("Barra rotacionada persiste apos reload", BarraRotacionadaPersisteAposReload),
                ("ElectricGraph apos rotacao da Barra mantem arestas validas", ElectricGraphAposRotacaoDaBarraMantemArestasValidas),
                ("DTO nao muda por causa da rotacao da Barra", DtoNaoMudaPorCausaDaRotacaoDaBarra),
                ("Hit-test encontra Barra rotacionada", HitTestEncontraBarraRotacionada),
                ("Snap encontra terminal de Barra rotacionada", SnapEncontraTerminalDeBarraRotacionada)
            };

            var failures = new List<string>();

            foreach ((string name, Action run) in tests)
            {
                try
                {
                    run();
                    Console.WriteLine($"PASS {name}");
                }
                catch (Exception ex)
                {
                    failures.Add($"{name}: {ex.Message}");
                    Console.WriteLine($"FAIL {name}: {ex.Message}");
                }
            }

            if (failures.Count == 0)
                return 0;

            Console.WriteLine();
            Console.WriteLine("Falhas:");

            foreach (string failure in failures)
                Console.WriteLine($"- {failure}");

            return 1;
        }

        private static void CircuitoSimplesPreservaDtos()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            ParameterReader reader = new(circuit.Document);
            CircuitDto dto = new CircuitBuilder(reader).Build();

            Assert(dto.Slack != null, "SlackDto deve existir.");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Slack!.Id, "SlackDto.Id");
            AssertEqual(circuit.Generator.Nome, dto.Slack.Nome, "SlackDto.Nome");
            AssertEqual(circuit.Generator.Nome, dto.Slack.Barra, "SlackDto.Barra");
            AssertEqual(3, dto.Slack.Fases, "SlackDto.Fases");
            AssertEqual(13.8, dto.Slack.Tensao, "SlackDto.Tensao");

            AssertEqual(1, dto.Loads.Count, "Quantidade de cargas");
            LoadDto load = dto.Loads[0];
            AssertEqual(circuit.Load.Id.ToString(), load.Id, "LoadDto.Id");
            AssertEqual(circuit.Load.Nome, load.Nome, "LoadDto.Nome");
            AssertEqual(circuit.Load.Nome, load.Barra, "LoadDto.Barra");
            AssertEqual(3, load.Fases, "LoadDto.Fases");
            AssertEqual(650, load.PotenciaAtiva, "LoadDto.PotenciaAtiva");
            AssertEqual(210, load.PotenciaReativa, "LoadDto.PotenciaReativa");
            AssertEqual(13.8, load.Tensao, "LoadDto.Tensao");
            AssertEqual("Wye", load.Conexao, "LoadDto.Conexao");
            AssertEqual(1, load.Modelo, "LoadDto.Modelo");

            AssertEqual(1, dto.Lines.Count, "Quantidade de cabos");
            LineDto line = dto.Lines[0];
            AssertEqual(circuit.Cable.Id.ToString(), line.Id, "LineDto.Id");
            AssertEqual(circuit.Cable.Nome, line.Nome, "LineDto.Nome");
            AssertEqual(circuit.Generator.Nome, line.Barra1, "LineDto.Barra1");
            AssertEqual(circuit.Load.Nome, line.Barra2, "LineDto.Barra2");
            AssertEqual(2.75, line.Comprimento, "LineDto.Comprimento");

            IList<ParameterReader.GeneratorData> generators = reader.GetGenerators();
            AssertEqual(1, generators.Count, "Quantidade de GeneratorData");
            AssertEqual(circuit.Generator.Id.ToString(), generators[0].Id, "GeneratorData.Id");
            AssertEqual(circuit.Generator.Nome, generators[0].Nome, "GeneratorData.Nome");
            AssertEqual(circuit.Generator.Nome, generators[0].Barra, "GeneratorData.Barra");
            AssertEqual(1250, generators[0].Potencia, "GeneratorData.Potencia");
            AssertEqual(0.93, generators[0].FP, "GeneratorData.FP");
        }

        private static void CoreApiUsaFallbackSemElectricGraph()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            ParameterReader reader = new(new CoreApi(circuit.Document));

            ParameterReader.LoadData load = reader.GetLoads().Single();
            ParameterReader.GeneratorData generator = reader.GetGenerators().Single();
            ParameterReader.LineData line = reader.GetLines().Single();

            AssertEqual(circuit.Load.Nome, load.Barra, "LoadData.Barra fallback");
            AssertEqual(circuit.Generator.Nome, generator.Barra, "GeneratorData.Barra fallback");
            AssertEqual(circuit.Generator.Nome, line.Barra1, "LineData.Barra1 fallback");
            AssertEqual(circuit.Load.Nome, line.Barra2, "LineData.Barra2 fallback");
        }

        private static void CaboInvalidoBloqueiaDto()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            circuit.Cable.DestinoId = Guid.NewGuid().ToString();

            ParameterReader reader = new(circuit.Document);
            TopologyValidationResult? result = reader.ValidateTopology();

            Assert(result != null && !result.IsValid, "Validador deve detectar cabo invalido.");
            AssertContains(result!.FormatErrors(), "DestinoId inexistente", "Erro de cabo invalido");

            AssertThrows<InvalidOperationException>(
                () => new CircuitBuilder(reader).Build(),
                "CircuitBuilder deve bloquear DTO final invalido.");
        }

        private static void CaboDuplicadoGeraErro()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            Cabo duplicate = CreateCable(circuit.Generator, circuit.Load, "L-DUP", 3.1);
            circuit.Document.AdicionarElemento(duplicate);

            TopologyValidationResult result = new TopologyValidator(circuit.Document).Validate();

            Assert(!result.IsValid, "Validador deve reprovar cabo duplicado.");
            AssertContains(result.FormatErrors(), "duplicado", "Erro de duplicidade");
            AssertEqual(4, circuit.Document.Elementos.Count, "Cabos duplicados nao devem ser removidos.");
        }

        private static void ElementosExistentesPermanecemEletricos()
        {
            Elemento[] elementos =
            {
                new Cabo(),
                new Barra(),
                new Carga(),
                new Gerador(),
                new Sin(),
                new Transformador()
            };

            foreach (Elemento elemento in elementos)
            {
                AssertEqual(
                    ElementoDomainRole.EletricoTopologico,
                    elemento.DomainRole,
                    $"{elemento.GetType().Name}.DomainRole");
                Assert(elemento.ParticipaDoGrafoEletrico, $"{elemento.GetType().Name} deve participar do grafo eletrico.");
            }
        }

        private static void ElementoAnotativoNaoParticipaDoGrafoEletrico()
        {
            ElementoAnotativo annotation = new FakeAnnotationElement();
            var carga = new Carga();

            AssertEqual(ElementoDomainRole.Anotacao, annotation.DomainRole, "ElementoAnotativo.DomainRole");
            Assert(!annotation.ParticipaDoGrafoEletrico, "ElementoAnotativo nao deve participar do grafo eletrico.");
            Assert(annotation.PossuiParametro(Elemento.PARAM_NOME), "ElementoAnotativo deve possuir parametro Nome.");
            Assert(annotation.PossuiParametro(ElementoAnotativo.PARAM_COR_LINHA), "ElementoAnotativo deve possuir parametro CorLinha.");
            Assert(annotation.PossuiParametro(ElementoAnotativo.PARAM_ESPESSURA_LINHA), "ElementoAnotativo deve possuir parametro EspessuraLinha.");
            Assert(annotation.PossuiParametro(ElementoAnotativo.PARAM_VISIVEL), "ElementoAnotativo deve possuir parametro Visivel.");
            AssertEqual("#FF000000", annotation.CorLinha, "ElementoAnotativo.CorLinha default");
            AssertEqual(1.0, annotation.EspessuraLinha, "ElementoAnotativo.EspessuraLinha default");
            AssertEqual(true, annotation.Visivel, "ElementoAnotativo.Visivel default");

            annotation.CorLinha = "#FF112233";
            annotation.EspessuraLinha = 2.5;
            annotation.Visivel = false;

            AssertEqual("#FF112233", annotation.Obter<string>(ElementoAnotativo.PARAM_COR_LINHA), "ElementoAnotativo.CorLinha alterada");
            AssertEqual(2.5, annotation.Obter<double>(ElementoAnotativo.PARAM_ESPESSURA_LINHA), "ElementoAnotativo.EspessuraLinha alterada");
            AssertEqual(false, annotation.Obter<bool>(ElementoAnotativo.PARAM_VISIVEL), "ElementoAnotativo.Visivel alterada");
            Assert(!carga.PossuiParametro(ElementoAnotativo.PARAM_COR_LINHA), "Carga nao deve possuir parametro CorLinha.");
            Assert(!carga.PossuiParametro(ElementoAnotativo.PARAM_ESPESSURA_LINHA), "Carga nao deve possuir parametro EspessuraLinha.");
            Assert(!carga.PossuiParametro(ElementoAnotativo.PARAM_VISIVEL), "Carga nao deve possuir parametro Visivel.");
        }

        private static void ElementoAnotativoRetangularPreservaBaseAnotativa()
        {
            ElementoAnotativoRetangular annotation = new FakeRectangularAnnotationElement();
            var carga = new Carga();

            Assert(typeof(ElementoAnotativoRetangular).IsAbstract, "ElementoAnotativoRetangular deve ser abstrata.");
            Assert(annotation is ElementoAnotativo, "ElementoAnotativoRetangular deve herdar de ElementoAnotativo.");
            AssertEqual(ElementoDomainRole.Anotacao, annotation.DomainRole, "ElementoAnotativoRetangular.DomainRole");
            Assert(!annotation.ParticipaDoGrafoEletrico, "ElementoAnotativoRetangular nao deve participar do grafo eletrico.");
            Assert(annotation.PossuiParametro(Elemento.PARAM_NOME), "ElementoAnotativoRetangular deve possuir parametro Nome.");
            Assert(annotation.PossuiParametro(ElementoAnotativo.PARAM_COR_LINHA), "ElementoAnotativoRetangular deve possuir parametro CorLinha.");
            Assert(annotation.PossuiParametro(ElementoAnotativo.PARAM_ESPESSURA_LINHA), "ElementoAnotativoRetangular deve possuir parametro EspessuraLinha.");
            Assert(annotation.PossuiParametro(ElementoAnotativo.PARAM_VISIVEL), "ElementoAnotativoRetangular deve possuir parametro Visivel.");
            Assert(annotation.PossuiParametro(ElementoAnotativoRetangular.PARAM_LARGURA), "ElementoAnotativoRetangular deve possuir parametro Largura.");
            Assert(annotation.PossuiParametro(ElementoAnotativoRetangular.PARAM_ALTURA), "ElementoAnotativoRetangular deve possuir parametro Altura.");
            AssertEqual(100.0, annotation.Largura, "ElementoAnotativoRetangular.Largura default");
            AssertEqual(50.0, annotation.Altura, "ElementoAnotativoRetangular.Altura default");

            annotation.Largura = 220.0;
            annotation.Altura = 90.0;

            AssertEqual(220.0, annotation.Obter<double>(ElementoAnotativoRetangular.PARAM_LARGURA), "ElementoAnotativoRetangular.Largura alterada");
            AssertEqual(90.0, annotation.Obter<double>(ElementoAnotativoRetangular.PARAM_ALTURA), "ElementoAnotativoRetangular.Altura alterada");
            Assert(!carga.PossuiParametro(ElementoAnotativoRetangular.PARAM_LARGURA), "Carga nao deve possuir parametro Largura.");
            Assert(!carga.PossuiParametro(ElementoAnotativoRetangular.PARAM_ALTURA), "Carga nao deve possuir parametro Altura.");
        }

        private static void LinhaAnotativaPreservaDominioAnotativo()
        {
            var linha = new LinhaAnotativa();
            var carga = new Carga();

            Assert(linha is ElementoAnotativo, "LinhaAnotativa deve herdar de ElementoAnotativo.");
            AssertEqual(ElementoDomainRole.Anotacao, linha.DomainRole, "LinhaAnotativa.DomainRole");
            Assert(!linha.ParticipaDoGrafoEletrico, "LinhaAnotativa nao deve participar do grafo eletrico.");
            Assert(linha.PossuiParametro(Elemento.PARAM_NOME), "LinhaAnotativa deve possuir parametro Nome.");
            Assert(linha.PossuiParametro(ElementoAnotativo.PARAM_COR_LINHA), "LinhaAnotativa deve possuir parametro CorLinha.");
            Assert(linha.PossuiParametro(ElementoAnotativo.PARAM_ESPESSURA_LINHA), "LinhaAnotativa deve possuir parametro EspessuraLinha.");
            Assert(linha.PossuiParametro(ElementoAnotativo.PARAM_VISIVEL), "LinhaAnotativa deve possuir parametro Visivel.");
            Assert(linha.PossuiParametro(LinhaAnotativa.PARAM_X2), "LinhaAnotativa deve possuir parametro X2.");
            Assert(linha.PossuiParametro(LinhaAnotativa.PARAM_Y2), "LinhaAnotativa deve possuir parametro Y2.");
            Assert(!linha.PossuiParametro(TipoLinhaAnotativa.PARAM_ESTILO_LINHA), "LinhaAnotativa nao deve possuir parametro de instancia EstiloLinha.");
            AssertEqual(100.0, linha.X2, "LinhaAnotativa.X2 default");
            AssertEqual(0.0, linha.Y2, "LinhaAnotativa.Y2 default");
            AssertEqual("#FF000000", linha.CorLinha, "LinhaAnotativa.CorLinha default");

            linha.Nome = "Linha teste";
            linha.CorLinha = "#FF102030";
            linha.EspessuraLinha = 3.5;
            linha.Visivel = false;
            linha.X2 = 250.0;
            linha.Y2 = 75.0;

            AssertEqual(250.0, linha.Obter<double>(LinhaAnotativa.PARAM_X2), "LinhaAnotativa.X2 alterado");
            AssertEqual(75.0, linha.Obter<double>(LinhaAnotativa.PARAM_Y2), "LinhaAnotativa.Y2 alterado");

            Elemento cloneElemento = linha.Clonar();
            var clone = cloneElemento as LinhaAnotativa;

            if (clone == null)
                throw new InvalidOperationException("Clonar deve criar LinhaAnotativa.");

            Assert(linha.Id != clone.Id, "Clone deve receber novo Id.");
            AssertEqual(linha.Nome, clone.Nome, "Clone.Nome");
            AssertEqual(linha.CorLinha, clone.CorLinha, "Clone.CorLinha");
            AssertEqual(linha.EspessuraLinha, clone.EspessuraLinha, "Clone.EspessuraLinha");
            AssertEqual(linha.Visivel, clone.Visivel, "Clone.Visivel");
            AssertEqual(linha.X2, clone.X2, "Clone.X2");
            AssertEqual(linha.Y2, clone.Y2, "Clone.Y2");
            Assert(!carga.PossuiParametro(LinhaAnotativa.PARAM_X2), "Carga nao deve possuir parametro X2.");
            Assert(!carga.PossuiParametro(LinhaAnotativa.PARAM_Y2), "Carga nao deve possuir parametro Y2.");
            AssertEqual("LinhaAnotativa", ElementKinds.LinhaAnotativa, "ElementKinds.LinhaAnotativa");
        }

        private static void TipoLinhaAnotativaPreservaEstiloEBiblioteca()
        {
            var tipo = new TipoLinhaAnotativa();

            Assert(tipo is TipoElemento, "TipoLinhaAnotativa deve herdar de TipoElemento.");
            Assert(tipo.PossuiParametro(TipoLinhaAnotativa.PARAM_ESTILO_LINHA), "TipoLinhaAnotativa deve possuir parametro EstiloLinha.");
            AssertEqual("Linha contínua", tipo.NomeTipo, "TipoLinhaAnotativa.NomeTipo default");
            AssertEqual("Anotações", tipo.Familia, "TipoLinhaAnotativa.Familia default");
            AssertEqual("Linhas", tipo.Categoria, "TipoLinhaAnotativa.Categoria default");
            AssertEqual("Contínuo", tipo.EstiloLinha, "TipoLinhaAnotativa.EstiloLinha default");

            tipo.EstiloLinha = "Tracejado";
            AssertEqual("Tracejado", tipo.EstiloLinha, "TipoLinhaAnotativa.EstiloLinha tracejado");

            tipo.EstiloLinha = "Valor invalido";
            AssertEqual("Contínuo", tipo.EstiloLinha, "TipoLinhaAnotativa.EstiloLinha invalido");

            var types = new TypeLibraryService();
            AssertEqual(4, types.TiposLinhasAnotativas.Count, "TiposLinhasAnotativas.Count");
            Assert(types.TipoLinhaAnotativaPadrao != null, "TipoLinhaAnotativaPadrao nao deve ser null.");
            AssertEqual("Contínuo", types.TiposLinhasAnotativas[0].EstiloLinha, "Tipo linha[0].EstiloLinha");
            AssertEqual("Tracejado", types.TiposLinhasAnotativas[1].EstiloLinha, "Tipo linha[1].EstiloLinha");
            AssertEqual("Traço ponto", types.TiposLinhasAnotativas[2].EstiloLinha, "Tipo linha[2].EstiloLinha");
            AssertEqual("Traço dois pontos", types.TiposLinhasAnotativas[3].EstiloLinha, "Tipo linha[3].EstiloLinha");

            TipoElementoViewModel? vm = TipoElementoViewModelFactory.Criar(types.TiposLinhasAnotativas[1]);
            Assert(vm is TipoLinhaAnotativaViewModel, "TipoElementoViewModelFactory deve criar TipoLinhaAnotativaViewModel.");

            var tipoVm = (TipoLinhaAnotativaViewModel)vm!;
            tipoVm.EstiloLinha = "Traço ponto";

            AssertEqual("Traço ponto", types.TiposLinhasAnotativas[1].EstiloLinha, "TipoLinhaAnotativaViewModel.EstiloLinha altera tipo");

            string xaml = File.ReadAllText(FindProjectFile("Properties/Types/TipoLinhaAnotativaPropertiesView.xaml"));
            Assert(!xaml.Contains("<ComboBox", StringComparison.OrdinalIgnoreCase), "TipoLinhaAnotativaPropertiesView nao deve conter ComboBox.");
            AssertContains(xaml, "Text=\"{Binding EstiloLinha}\"", "TipoLinhaAnotativaPropertiesView.EstiloLinha.Binding");
            AssertContains(xaml, "PropertyReadOnlyTextBoxStyle", "TipoLinhaAnotativaPropertiesView.EstiloLinha.ReadOnly");
        }

        private static void ElectricGraphBuildNaoAlteraDocument()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            int countBefore = circuit.Document.Elementos.Count;

            ElectricGraph graph = new ElectricGraphBuilder(circuit.Document).Build();

            AssertEqual(countBefore, circuit.Document.Elementos.Count, "Contagem do Document");
            AssertEqual(2, graph.Nodes.Count, "Quantidade de nos do grafo");
            AssertEqual(1, graph.Edges.Count, "Quantidade de arestas do grafo");
        }

        private static void ElectricGraphIncluiEletricosEIgnoraAnotativo()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-GRAFO");
            Transformador transformer = CreateTransformador("TR-GRAFO");
            Barra bar = CreateBar("BARRA-GRAFO");
            Gerador generator = CreateGenerator("GER-GRAFO", 900, 0.95);
            Carga load = CreateLoad("CARGA-GRAFO", 300, 100);
            Cabo cable = CreateCable(generator, load, "L-GRAFO", 1);
            Elemento annotation = CreateAnnotation("CARGA-GRAFO");

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformer);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);
            document.AdicionarElemento(annotation);

            ElectricGraph graph = new ElectricGraphBuilder(document).Build();

            AssertEqual(5, graph.Nodes.Count, "Quantidade de nos eletricos");
            AssertEqual(1, graph.Edges.Count, "Quantidade de cabos eletricos");
            AssertContainsNode(graph.Nodes, sin, "SIN no grafo");
            AssertContainsNode(graph.Nodes, transformer, "Transformador no grafo");
            AssertContainsNode(graph.Nodes, bar, "Barra no grafo");
            AssertContainsNode(graph.Nodes, generator, "Gerador no grafo");
            AssertContainsNode(graph.Nodes, load, "Carga no grafo");
            Assert(graph.FindNode(annotation.Id.ToString()) == null, "Anotativo nao deve virar no do grafo.");
        }

        private static void DtoPermaneceIdenticoComAnotativoNoDocument()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            string dtoBefore = SerializeCircuitDto(new CircuitBuilder(new ParameterReader(circuit.Document)).Build());

            circuit.Document.AdicionarElemento(CreateAnnotation(circuit.Generator.Nome));

            string dtoAfter = SerializeCircuitDto(new CircuitBuilder(new ParameterReader(circuit.Document)).Build());
            ParameterReader reader = new(circuit.Document);

            AssertEqual(dtoBefore, dtoAfter, "CircuitDto serializado");
            AssertEqual(1, reader.GetGenerators().Count, "ParameterReader.Generators");
            AssertEqual(1, reader.GetLoads().Count, "ParameterReader.Loads");
            AssertEqual(1, reader.GetLines().Count, "ParameterReader.Lines");
            AssertEqual(0, reader.GetSins().Count, "ParameterReader.Sins");
            AssertEqual(0, reader.GetTransformers().Count, "ParameterReader.Transformers");
        }

        private static void OperationalGraphIgnoraAnotativo()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-ANOT");
            Carga load = CreateLoad("CARGA-ANOT", 300, 100);
            Cabo cable = CreateCable(sin, 1, load, 0, "L-ANOT", 1.0);
            Elemento annotation = CreateAnnotation("SIN-ANOT");

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);
            document.AdicionarElemento(annotation);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN energizado com anotativo");
            AssertEnergized(state, load, "Carga energizada com anotativo");
            AssertEdgeEnergized(state, cable, "Cabo energizado com anotativo");
            Assert(!state.EnergizedNodeIds.Contains(annotation.Id.ToString()), "Anotativo nao deve energizar.");
            Assert(!state.DeenergizedNodeIds.Contains(annotation.Id.ToString()), "Anotativo nao deve aparecer desenergizado.");
        }

        private static void TopologyValidatorIgnoraAnotativo()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            circuit.Document.AdicionarElemento(CreateAnnotation(circuit.Generator.Nome));

            TopologyValidationResult result = new TopologyValidator(circuit.Document).Validate();

            Assert(result.IsValid, "Anotativo com nome duplicado deve ser ignorado pelo validador topologico.");
            AssertEqual(4, circuit.Document.Elementos.Count, "Anotativo deve permanecer no Document.");
        }

        private static void ClassificacaoEletricaNaoDependeDeNomeTipoOuSvg()
        {
            var carga = new Carga
            {
                Nome = "texto livre",
                Tipo = new TipoCarga()
            };

            var annotation = new FakeAnnotationElement
            {
                Nome = "Carga",
                Tipo = new TipoCarga()
            };

            Assert(carga.ParticipaDoGrafoEletrico, "Carga deve ser eletrica apesar do nome livre.");
            AssertEqual(ElementoDomainRole.Anotacao, annotation.DomainRole, "Anotativo.DomainRole");
            Assert(!annotation.ParticipaDoGrafoEletrico, "Anotativo nao deve virar eletrico por nome ou Tipo.");
        }

        private static void MultiplosGeradoresPreservamSlack()
        {
            var document = new AraciDocument();
            Gerador generatorA = CreateGenerator("GERADOR-A", 1100, 0.91);
            Gerador generatorB = CreateGenerator("GERADOR-B", 730, 0.87);
            Carga load = CreateLoad("CARGA-MULTI", 510, 170);

            document.AdicionarElemento(generatorA);
            document.AdicionarElemento(generatorB);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(generatorA, load, "L-A", 1.1));
            document.AdicionarElemento(CreateCable(generatorB, load, "L-B", 1.2));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            Assert(dto.Slack != null, "SlackDto deve existir.");
            AssertEqual(generatorA.Id.ToString(), dto.Slack!.Id, "Slack deve usar primeiro gerador");
            AssertEqual(generatorA.Nome, dto.Slack.Nome, "SlackDto.Nome");
            AssertEqual(generatorA.Nome, dto.Slack.Barra, "SlackDto.Barra");
            AssertEqual(1, dto.Generators.Count, "Geradores restantes");

            GeneratorDto generator = dto.Generators[0];
            AssertEqual(generatorB.Id.ToString(), generator.Id, "GeneratorDto.Id");
            AssertEqual(generatorB.Nome, generator.Nome, "GeneratorDto.Nome");
            AssertEqual(generatorB.Nome, generator.Barra, "GeneratorDto.Barra");
            AssertEqual(730, generator.Potencia, "GeneratorDto.Potencia");
            AssertEqual(0.87, generator.FP, "GeneratorDto.FP");
            Assert(!dto.Generators.Any(g => g.Id == generatorA.Id.ToString()), "Slack nao deve aparecer em Generators.");
        }

        private static void CabosEmSeriePreservamOrientacao()
        {
            var document = new AraciDocument();
            Gerador generator = CreateGenerator("GERADOR-SERIE", 1200, 0.95);
            Barra bar = CreateBar("BARRA-SERIE");
            Carga load = CreateLoad("CARGA-SERIE", 430, 140);
            Cabo line1 = CreateCable(generator, 0, bar, 0, "L-S01", 4.1);
            Cabo line2 = CreateCable(bar, 1, load, 0, "L-S02", 5.2);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(load);
            document.AdicionarElemento(line1);
            document.AdicionarElemento(line2);

            IList<ParameterReader.LineData> lines = new ParameterReader(document).GetLines();

            AssertEqual(2, lines.Count, "Quantidade de linhas em serie");
            AssertLine(lines[0], line1, generator.Nome, bar.Nome, "Linha serie 1");
            AssertLine(lines[1], line2, bar.Nome, load.Nome, "Linha serie 2");
        }

        private static void RamificacaoSimplesValidaGrafoEDto()
        {
            var document = new AraciDocument();
            Gerador generator = CreateGenerator("GERADOR-RAMO", 1300, 0.94);
            Barra bar = CreateBar("BARRA-RAMO");
            Carga load1 = CreateLoad("CARGA-R1", 320, 90);
            Carga load2 = CreateLoad("CARGA-R2", 280, 85);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(load1);
            document.AdicionarElemento(load2);
            document.AdicionarElemento(CreateCable(generator, 0, bar, 0, "L-01", 1.0));
            document.AdicionarElemento(CreateCable(bar, 1, load1, 0, "L-02", 1.1));
            document.AdicionarElemento(CreateCable(bar, 2, load2, 0, "L-03", 1.2));

            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            IReadOnlyList<ElectricGraphNode> neighbors = graph.GetNeighbors(bar.Id.ToString());
            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(4, graph.Nodes.Count, "Quantidade de nos da ramificacao");
            AssertEqual(3, graph.Edges.Count, "Quantidade de arestas da ramificacao");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Arestas invalidas da ramificacao");
            AssertContainsNode(neighbors, generator, "Vizinho gerador");
            AssertContainsNode(neighbors, load1, "Vizinho carga 1");
            AssertContainsNode(neighbors, load2, "Vizinho carga 2");
            AssertEqual(2, dto.Loads.Count, "Cargas no DTO ramificado");
            AssertEqual(3, dto.Lines.Count, "Linhas no DTO ramificado");
        }

        private static void TopologiaMaiorNaoAlteraDocument()
        {
            AraciDocument document = CreateBranchDocument();
            int countBefore = document.Elementos.Count;

            ElectricGraph graph1 = new ElectricGraphBuilder(document).Build();
            ElectricGraph graph2 = new ElectricGraphBuilder(document).Build();
            ElectricGraph graph3 = new ElectricGraphBuilder(document).Build();

            AssertEqual(countBefore, document.Elementos.Count, "Contagem apos builds repetidos");
            AssertEqual(graph1.Nodes.Count, graph2.Nodes.Count, "Nodes build 1/2");
            AssertEqual(graph2.Nodes.Count, graph3.Nodes.Count, "Nodes build 2/3");
            AssertEqual(graph1.Edges.Count, graph2.Edges.Count, "Edges build 1/2");
            AssertEqual(graph2.Edges.Count, graph3.Edges.Count, "Edges build 2/3");
        }

        private static void OrdemDeLinhasSegueDocument()
        {
            AraciDocument document = CreateBranchDocument();
            IList<ParameterReader.LineData> lines = new ParameterReader(document).GetLines();

            AssertEqual(3, lines.Count, "Quantidade de linhas ordenadas");
            AssertEqual("L-01", lines[0].Nome, "Linha 1 por ordem do Document");
            AssertEqual("L-02", lines[1].Nome, "Linha 2 por ordem do Document");
            AssertEqual("L-03", lines[2].Nome, "Linha 3 por ordem do Document");
        }

        private static void PersistenciaPreservaTopologiaSimples()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            AraciDocument loaded = SaveAndLoad(circuit.Document);

            AssertEqual(3, loaded.Elementos.Count, "Quantidade de elementos recarregados");

            Gerador generator = FindById<Gerador>(loaded, circuit.Generator.Id);
            Carga load = FindById<Carga>(loaded, circuit.Load.Id);
            Cabo cable = FindById<Cabo>(loaded, circuit.Cable.Id);

            AssertEqual(circuit.Generator.Nome, generator.Nome, "Nome do gerador");
            AssertEqual(circuit.Load.Nome, load.Nome, "Nome da carga");
            AssertCablePersisted(circuit.Cable, cable, "Cabo simples");
            AssertEqual(circuit.Load.PotenciaAtiva, load.PotenciaAtiva, "Potencia ativa da carga");
            AssertEqual(circuit.Load.PotenciaReativa, load.PotenciaReativa, "Potencia reativa da carga");
            AssertEqual(circuit.Generator.PotenciaAtiva, generator.PotenciaAtiva, "Potencia ativa do gerador");
        }

        private static void PersistenciaPreservaRamificacao()
        {
            AraciDocument loaded = SaveAndLoad(CreateBranchDocument());
            Barra bar = loaded.Elementos.OfType<Barra>().Single();
            ElectricGraph graph = new ElectricGraphBuilder(loaded).Build();
            IReadOnlyList<ElectricGraphNode> neighbors = graph.GetNeighbors(bar.Id.ToString());

            AssertEqual(4, graph.Nodes.Count, "Nodes apos reload");
            AssertEqual(3, graph.Edges.Count, "Edges apos reload");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Edges invalidas apos reload");
            AssertContainsNode(neighbors, loaded.Elementos.OfType<Gerador>().Single(), "Vizinho gerador apos reload");

            foreach (Carga load in loaded.Elementos.OfType<Carga>())
                AssertContainsNode(neighbors, load, $"Vizinho {load.Nome} apos reload");
        }

        private static void DtoPermaneceEquivalenteAposReload()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            CircuitDto before = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();
            CircuitDto after = new CircuitBuilder(new ParameterReader(SaveAndLoad(circuit.Document))).Build();

            AssertEqual(before.Slack!.Id, after.Slack!.Id, "Slack.Id apos reload");
            AssertEqual(before.Slack.Nome, after.Slack.Nome, "Slack.Nome apos reload");
            AssertEqual(before.Slack.Barra, after.Slack.Barra, "Slack.Barra apos reload");
            AssertEqual(before.Loads.Count, after.Loads.Count, "Quantidade de cargas apos reload");
            AssertEqual(before.Lines.Count, after.Lines.Count, "Quantidade de linhas apos reload");
            AssertEqual(before.Loads[0].Id, after.Loads[0].Id, "Load.Id apos reload");
            AssertEqual(before.Loads[0].Nome, after.Loads[0].Nome, "Load.Nome apos reload");
            AssertEqual(before.Loads[0].Barra, after.Loads[0].Barra, "Load.Barra apos reload");
            AssertEqual(before.Lines[0].Id, after.Lines[0].Id, "Line.Id apos reload");
            AssertEqual(before.Lines[0].Nome, after.Lines[0].Nome, "Line.Nome apos reload");
            AssertEqual(before.Lines[0].Barra1, after.Lines[0].Barra1, "Line.Barra1 apos reload");
            AssertEqual(before.Lines[0].Barra2, after.Lines[0].Barra2, "Line.Barra2 apos reload");
            AssertEqual(before.Lines[0].Comprimento, after.Lines[0].Comprimento, "Line.Comprimento apos reload");
        }

        private static void IdsPermanecemEstaveisAposReload()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            AraciDocument loaded = SaveAndLoad(circuit.Document);
            Cabo cable = FindById<Cabo>(loaded, circuit.Cable.Id);

            AssertEqual(circuit.Cable.Id, cable.Id, "Id do cabo");
            AssertEqual(circuit.Generator.Id.ToString(), cable.OrigemId, "OrigemId apos reload");
            AssertEqual(circuit.Load.Id.ToString(), cable.DestinoId, "DestinoId apos reload");
            AssertEqual(circuit.Cable.OrigemTerminalId, cable.OrigemTerminalId, "OrigemTerminalId apos reload");
            AssertEqual(circuit.Cable.DestinoTerminalId, cable.DestinoTerminalId, "DestinoTerminalId apos reload");
            _ = FindById<Gerador>(loaded, circuit.Generator.Id);
            _ = FindById<Carga>(loaded, circuit.Load.Id);
        }

        private static void BuildsRepetidosAposReloadNaoAlteramDocument()
        {
            AraciDocument loaded = SaveAndLoad(CreateBranchDocument());
            int countBefore = loaded.Elementos.Count;

            ElectricGraph graph1 = new ElectricGraphBuilder(loaded).Build();
            ElectricGraph graph2 = new ElectricGraphBuilder(loaded).Build();
            ElectricGraph graph3 = new ElectricGraphBuilder(loaded).Build();

            AssertEqual(countBefore, loaded.Elementos.Count, "Contagem apos reload e builds");
            AssertEqual(graph1.Nodes.Count, graph2.Nodes.Count, "Nodes reload build 1/2");
            AssertEqual(graph2.Nodes.Count, graph3.Nodes.Count, "Nodes reload build 2/3");
            AssertEqual(graph1.Edges.Count, graph2.Edges.Count, "Edges reload build 1/2");
            AssertEqual(graph2.Edges.Count, graph3.Edges.Count, "Edges reload build 2/3");
        }

        private static void SinPodeSerCriadoEEntraNoDocument()
        {
            var context = new EditorContext();
            Sin sin = context.ElementoFactory.CriarSin();

            context.Document.AdicionarElemento(sin);

            AssertEqual(1, context.Document.Elementos.Count, "Quantidade no Document");
            Assert(context.Document.Elementos.Contains(sin), "SIN deve estar no Document.");
            AssertEqual("Sin", context.Elements.GetKind(sin), "Kind do SIN");
            AssertSinTerminals(sin, "SIN criado");
        }

        private static void SinApareceNoElectricGraph()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-GRAFO");

            document.AdicionarElemento(sin);

            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            ElectricGraphNode? node = graph.FindNode(sin.Id.ToString());

            Assert(node != null, "SIN deve aparecer como no do ElectricGraph.");
            AssertEqual(sin.Nome, node!.Name, "Nome do no SIN");
            AssertEqual(4, node.Terminals.Count, "Terminais do no SIN");
            AssertGraphTerminal(node, Sin.TERMINAL_NORTE, "Terminal NORTE no grafo");
            AssertGraphTerminal(node, Sin.TERMINAL_SUL, "Terminal SUL no grafo");
            AssertGraphTerminal(node, Sin.TERMINAL_LESTE, "Terminal LESTE no grafo");
            AssertGraphTerminal(node, Sin.TERMINAL_OESTE, "Terminal OESTE no grafo");
        }

        private static void SinPreservaIdAposReload()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RELOAD");

            document.AdicionarElemento(sin);

            AraciDocument loaded = SaveAndLoad(document);
            Sin loadedSin = FindById<Sin>(loaded, sin.Id);

            AssertEqual(sin.Id, loadedSin.Id, "Id do SIN");
            AssertEqual(sin.Nome, loadedSin.Nome, "Nome do SIN");
            AssertEqual(sin.Barra, loadedSin.Barra, "Barra do SIN");
            AssertSinTerminals(loadedSin, "SIN apos reload");
        }

        private static void CabosConectadosAosTerminaisDoSinPreservamConexoes()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-CABO");
            Carga loadNorte = CreateLoad("CARGA-SIN-N", 350, 120);
            Carga loadSul = CreateLoad("CARGA-SIN-S", 351, 121);
            Carga loadLeste = CreateLoad("CARGA-SIN-L", 352, 122);
            Carga loadOeste = CreateLoad("CARGA-SIN-O", 353, 123);
            Cabo cableNorte = CreateCable(sin, 0, loadNorte, 0, "L-SIN-N", 1.5);
            Cabo cableSul = CreateCable(sin, 1, loadSul, 0, "L-SIN-S", 1.6);
            Cabo cableLeste = CreateCable(sin, 2, loadLeste, 0, "L-SIN-L", 1.7);
            Cabo cableOeste = CreateCable(sin, 3, loadOeste, 0, "L-SIN-O", 1.8);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(loadNorte);
            document.AdicionarElemento(loadSul);
            document.AdicionarElemento(loadLeste);
            document.AdicionarElemento(loadOeste);
            document.AdicionarElemento(cableNorte);
            document.AdicionarElemento(cableSul);
            document.AdicionarElemento(cableLeste);
            document.AdicionarElemento(cableOeste);

            AraciDocument loaded = SaveAndLoad(document);
            ElectricGraph graph = new ElectricGraphBuilder(loaded).Build();
            Sin loadedSin = FindById<Sin>(loaded, sin.Id);

            AssertSinTerminals(loadedSin, "SIN conectado apos reload");
            AssertCableEndpoint(loaded, cableNorte, sin, Sin.TERMINAL_NORTE, loadNorte, "Cabo NORTE");
            AssertCableEndpoint(loaded, cableSul, sin, Sin.TERMINAL_SUL, loadSul, "Cabo SUL");
            AssertCableEndpoint(loaded, cableLeste, sin, Sin.TERMINAL_LESTE, loadLeste, "Cabo LESTE");
            AssertCableEndpoint(loaded, cableOeste, sin, Sin.TERMINAL_OESTE, loadOeste, "Cabo OESTE");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Grafo com SIN nao deve ter arestas invalidas");
        }

        private static void DtosSemSinMantemGeradorComoSlack()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();

            CircuitDto dto = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();

            Assert(dto.Slack != null, "SlackDto deve continuar existindo.");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Slack!.Id, "Slack deve continuar usando gerador");
            AssertEqual(0, dto.Generators.Count, "Primeiro gerador legado nao deve aparecer em Generators.");
            AssertEqual(1, dto.Loads.Count, "Quantidade de cargas sem SIN");
            AssertEqual(1, dto.Lines.Count, "Quantidade de linhas sem SIN");
        }

        private static void SinComGeradorViraSlackPreferencial()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            Sin sin = CreateSin("SIN-PREFERENCIAL");
            circuit.Document.Elementos.Insert(0, sin);

            CircuitDto dto = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack deve usar SIN");
            AssertEqual(sin.Nome, dto.Slack.Nome, "Slack.Nome SIN");
            AssertEqual(sin.Barra, dto.Slack.Barra, "Slack.Barra SIN");
            AssertEqual(1, dto.Generators.Count, "Gerador deve permanecer em Generators");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Generators[0].Id, "GeneratorDto.Id com SIN");
            AssertEqual(13.8, dto.Generators[0].Tensao, "GeneratorDto.Tensao com SIN");
            AssertEqual(circuit.Generator.PotenciaAtiva, dto.Generators[0].Potencia, "GeneratorDto.Potencia com SIN");
            AssertEqual(0.93, dto.Generators[0].FP, "GeneratorDto.FP com SIN");
            AssertEqual(1, dto.Loads.Count, "Cargas com SIN");
            AssertEqual(1, dto.Lines.Count, "Linhas com SIN");
        }

        private static void SinComGeradorPreservaGeneratorDtoReal()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-GER-REAL");
            Gerador generator = CreateGenerator("GERADOR-REAL", 2750, 0.96);
            Carga load = CreateLoad("CARGA-GER-REAL", 300, 100);

            generator.TensaoLinha = "0.69";
            generator.TipoGerador.TensaoKV = 34.5;

            document.AdicionarElemento(sin);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-SIN-LOAD-REAL", 1.0));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-GER-LOAD-REAL", 1.0));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();
            GeneratorDto generatorDto = dto.Generators.Single();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack deve usar SIN no circuito com gerador real");
            AssertEqual(generator.Id.ToString(), generatorDto.Id, "GeneratorDto real.Id");
            AssertEqual(generator.Nome, generatorDto.Nome, "GeneratorDto real.Nome");
            AssertEqual(generator.Nome, generatorDto.Barra, "GeneratorDto real.Barra");
            AssertEqual(3, generatorDto.Fases, "GeneratorDto real.Fases");
            AssertEqual(0.69, generatorDto.Tensao, "GeneratorDto real.Tensao");
            AssertEqual(2750, generatorDto.Potencia, "GeneratorDto real.Potencia");
            AssertEqual(0.96, generatorDto.FP, "GeneratorDto real.FP");
        }

        private static void SinComMultiplosGeradoresPreservaTodosGenerators()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-MULTI-GER");
            Gerador generatorA = CreateGenerator("GERADOR-SIN-A", 1100, 0.91);
            Gerador generatorB = CreateGenerator("GERADOR-SIN-B", 730, 0.87);
            Carga load = CreateLoad("CARGA-SIN-MULTI", 510, 170);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(generatorA);
            document.AdicionarElemento(generatorB);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-SIN-M", 1.1));
            document.AdicionarElemento(CreateCable(generatorA, 0, load, 0, "L-GA-SIN", 1.2));
            document.AdicionarElemento(CreateCable(generatorB, 0, load, 0, "L-GB-SIN", 1.3));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack deve usar SIN com multiplos geradores");
            AssertEqual(2, dto.Generators.Count, "Todos os geradores devem permanecer em Generators");
            Assert(dto.Generators.Any(g => g.Id == generatorA.Id.ToString()), "Gerador A deve estar em Generators.");
            Assert(dto.Generators.Any(g => g.Id == generatorB.Id.ToString()), "Gerador B deve estar em Generators.");
            AssertEqual(1100, dto.Generators.Single(g => g.Id == generatorA.Id.ToString()).Potencia, "GeneratorDto A.Potencia");
            AssertEqual(730, dto.Generators.Single(g => g.Id == generatorB.Id.ToString()).Potencia, "GeneratorDto B.Potencia");
        }

        private static void ReloadPreservaGeneratorDtoRealComSin()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RELOAD-GER-REAL");
            Gerador generator = CreateGenerator("GERADOR-RELOAD-REAL", 3150, 0.94);
            Carga load = CreateLoad("CARGA-RELOAD-GER-REAL", 300, 100);

            generator.TensaoLinha = "34.5";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-SIN-LOAD-RELOAD-GER", 1.0));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-GER-LOAD-RELOAD-GER", 1.0));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(SaveAndLoad(document))).Build();
            GeneratorDto generatorDto = dto.Generators.Single();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack SIN apos reload com gerador real");
            AssertEqual(generator.Id.ToString(), generatorDto.Id, "GeneratorDto reload.Id");
            AssertEqual(34.5, generatorDto.Tensao, "GeneratorDto reload.Tensao");
            AssertEqual(3150, generatorDto.Potencia, "GeneratorDto reload.Potencia");
            AssertEqual(0.94, generatorDto.FP, "GeneratorDto reload.FP");
        }

        private static void CircuitoEolicoSimplificadoPreservaGeneratorDto()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-EOLICO");
            Transformador trSe = CreateTransformador("TR-SE-EOLICO");
            Transformador trAerogerador = CreateTransformador("TR-AERO-EOLICO");
            Gerador generator = CreateGenerator("AEROGERADOR-001", 4200, 0.97);
            Carga load = CreateLoad("CARGA-AUX-EOLICA", 120, 40);

            sin.TensaoLinha = "138";

            trSe.TensaoPrimarioKV = 138.0;
            trSe.TensaoSecundarioKV = 34.5;
            trSe.PotenciaAparente = 65000.0;

            trAerogerador.TensaoPrimarioKV = 34.5;
            trAerogerador.TensaoSecundarioKV = 0.69;
            trAerogerador.PotenciaAparente = 5000.0;

            generator.TensaoLinha = "0.69";
            load.TensaoLinha = "0.69";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(trSe);
            document.AdicionarElemento(trAerogerador);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, trSe, 0, "L-EOLICO-138", 1.0));
            document.AdicionarElemento(CreateCable(trSe, 1, trAerogerador, 0, "L-EOLICO-34", 1.0));
            document.AdicionarElemento(CreateCable(trAerogerador, 1, generator, 0, "L-EOLICO-069", 1.0));
            document.AdicionarElemento(CreateCable(generator, 1, load, 0, "L-EOLICO-AUX", 0.1));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();
            GeneratorDto generatorDto = dto.Generators.Single();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack eolico deve usar SIN");
            AssertEqual(2, dto.Transformers.Count, "Circuito eolico deve preservar dois transformadores");
            AssertEqual(generator.Id.ToString(), generatorDto.Id, "GeneratorDto eolico.Id");
            AssertEqual(generator.Nome, generatorDto.Nome, "GeneratorDto eolico.Nome");
            AssertEqual(generator.Nome, generatorDto.Barra, "GeneratorDto eolico.Barra");
            AssertEqual(0.69, generatorDto.Tensao, "GeneratorDto eolico.Tensao");
            AssertEqual(4200, generatorDto.Potencia, "GeneratorDto eolico.Potencia");
            AssertEqual(0.97, generatorDto.FP, "GeneratorDto eolico.FP");
        }

        private static void MultiplosSinUsamPrimeiroDoDocumentComoSlack()
        {
            var document = new AraciDocument();
            Sin sinA = CreateSin("SIN-PRIMEIRO");
            Sin sinB = CreateSin("SIN-SEGUNDO");
            Gerador generator = CreateGenerator("GERADOR-COM-SIN", 900, 0.95);
            Carga load = CreateLoad("CARGA-MULTI-SIN", 250, 80);

            document.AdicionarElemento(sinA);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(sinB);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sinB, 2, load, 0, "L-MULTI-SIN", 1.4));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-G-MULTI-SIN", 1.5));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(sinA.Id.ToString(), dto.Slack.Id, "Primeiro SIN deve virar slack");
            AssertEqual(sinA.Nome, dto.Slack.Nome, "Nome do primeiro SIN slack");
            AssertEqual(1, dto.Generators.Count, "Gerador deve permanecer em Generators com multiplos SIN");
        }

        private static void ReloadComSinMantemSlackBaseadoNoSin()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            Sin sin = CreateSin("SIN-RELOAD-SLACK");
            circuit.Document.Elementos.Insert(0, sin);

            AraciDocument loaded = SaveAndLoad(circuit.Document);
            CircuitDto dto = new CircuitBuilder(new ParameterReader(loaded)).Build();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "Slack SIN apos reload");
            AssertEqual(sin.Nome, dto.Slack.Nome, "Slack.Nome SIN apos reload");
            AssertEqual(1, dto.Generators.Count, "Gerador preservado apos reload com SIN");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Generators[0].Id, "GeneratorDto.Id apos reload com SIN");
        }

        private static void OperationalGraphEnergizaSinCaboECarga()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-OP");
            Carga load = CreateLoad("CARGA-OP", 300, 100);
            Cabo cable = CreateCable(sin, 1, load, 0, "L-OP", 1.0);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN energizado");
            AssertEnergized(state, load, "Carga energizada");
            AssertEdgeEnergized(state, cable, "Cabo energizado");
            AssertEqual(1, state.SourceNodeIds.Count, "Quantidade de fontes operacionais");
            AssertEqual(sin.Id.ToString(), state.SourceNodeIds[0], "Fonte operacional SIN");
        }

        private static void OperationalGraphMantemCargaIsoladaDesenergizada()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-ISOLADO");
            Carga load = CreateLoad("CARGA-ISOLADA", 300, 100);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN isolado energizado");
            AssertDeenergized(state, load, "Carga isolada desenergizada");
        }

        private static void OperationalGraphEnergizaRamificacaoComBarra()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RAMO-OP");
            Barra bar = CreateBar("BARRA-RAMO-OP");
            Carga load1 = CreateLoad("CARGA-OP-R1", 320, 90);
            Carga load2 = CreateLoad("CARGA-OP-R2", 280, 85);
            Cabo cable1 = CreateCable(sin, 1, bar, 0, "L-OP-01", 1.0);
            Cabo cable2 = CreateCable(bar, 1, load1, 0, "L-OP-02", 1.1);
            Cabo cable3 = CreateCable(bar, 2, load2, 0, "L-OP-03", 1.2);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(load1);
            document.AdicionarElemento(load2);
            document.AdicionarElemento(cable1);
            document.AdicionarElemento(cable2);
            document.AdicionarElemento(cable3);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN ramificado");
            AssertEnergized(state, bar, "Barra ramificada");
            AssertEnergized(state, load1, "Carga ramo 1");
            AssertEnergized(state, load2, "Carga ramo 2");
            AssertEdgeEnergized(state, cable1, "Cabo ramo 1");
            AssertEdgeEnergized(state, cable2, "Cabo ramo 2");
            AssertEdgeEnergized(state, cable3, "Cabo ramo 3");
        }

        private static void OperationalGraphNaoPropagaPorCaboInvalido()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-INVALIDO");
            Carga load = CreateLoad("CARGA-BLOQUEADA", 300, 100);
            Cabo cable = CreateCable(sin, 1, load, 0, "L-INVALIDO", 1.0);
            cable.DestinoTerminalId = "NAO_EXISTE";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEnergized(state, sin, "SIN com cabo invalido");
            AssertDeenergized(state, load, "Carga atras de cabo invalido");
            AssertEdgeDeenergized(state, cable, "Cabo invalido desenergizado");
        }

        private static void OperationalGraphUsaGeradorComoFallbackSemSin()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            OperationalGraphState state = BuildOperationalState(circuit.Document);

            AssertEnergized(state, circuit.Generator, "Gerador fallback");
            AssertEnergized(state, circuit.Load, "Carga via gerador fallback");
            AssertEdgeEnergized(state, circuit.Cable, "Cabo via gerador fallback");
            AssertEqual(circuit.Generator.Id.ToString(), state.SourceNodeIds[0], "Fonte fallback gerador");
        }

        private static void OperationalGraphSemFonteNaoEnergizaNos()
        {
            var document = new AraciDocument();
            Carga load = CreateLoad("CARGA-SEM-FONTE", 300, 100);

            document.AdicionarElemento(load);

            OperationalGraphState state = BuildOperationalState(document);

            AssertEqual(0, state.SourceNodeIds.Count, "Sem fontes operacionais");
            AssertEqual(0, state.EnergizedNodeIds.Count, "Nos energizados sem fonte");
            AssertDeenergized(state, load, "Carga sem fonte");
        }

        private static void OperationalGraphRebuildRepetidoNaoAlteraDocument()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-REBUILD-OP");
            Carga load = CreateLoad("CARGA-REBUILD-OP", 300, 100);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-REBUILD-OP", 1.0));

            int countBefore = document.Elementos.Count;

            OperationalGraphState state1 = BuildOperationalState(document);
            OperationalGraphState state2 = BuildOperationalState(document);
            OperationalGraphState state3 = BuildOperationalState(document);

            AssertEqual(countBefore, document.Elementos.Count, "Contagem apos rebuild operacional");
            AssertEqual(state1.EnergizedNodeIds.Count, state2.EnergizedNodeIds.Count, "Operational nodes 1/2");
            AssertEqual(state2.EnergizedNodeIds.Count, state3.EnergizedNodeIds.Count, "Operational nodes 2/3");
            AssertEqual(state1.EnergizedEdgeIds.Count, state2.EnergizedEdgeIds.Count, "Operational edges 1/2");
            AssertEqual(state2.EnergizedEdgeIds.Count, state3.EnergizedEdgeIds.Count, "Operational edges 2/3");
        }

        private static void OperationalGraphAposReloadPreservaResultado()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RELOAD-OP");
            Carga load = CreateLoad("CARGA-RELOAD-OP", 300, 100);
            Cabo cable = CreateCable(sin, 1, load, 0, "L-RELOAD-OP", 1.0);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            AraciDocument loaded = SaveAndLoad(document);
            OperationalGraphState state = BuildOperationalState(loaded);

            Assert(state.IsNodeEnergized(sin.Id.ToString()), "SIN deve continuar energizado apos reload.");
            Assert(state.IsNodeEnergized(load.Id.ToString()), "Carga deve continuar energizada apos reload.");
            Assert(state.IsEdgeEnergized(cable.Id.ToString()), "Cabo deve continuar energizado apos reload.");
        }

        private static void TransformadorMinimoPossuiTerminais()
        {
            Transformador transformador = CreateTransformador("TR-TESTE");

            AssertTransformadorTerminals(transformador, "Transformador minimo");
            AssertEqual(120, transformador.Terminais[0].Posicao.X, "Primario.X");
            AssertEqual(80, transformador.Terminais[0].Posicao.Y, "Primario.Y");
            AssertEqual(120, transformador.Terminais[1].Posicao.X, "Secundario.X");
            AssertEqual(220, transformador.Terminais[1].Posicao.Y, "Secundario.Y");
        }

        private static void TransformadorApareceNoElectricGraph()
        {
            var document = new AraciDocument();
            Transformador transformador = CreateTransformador("TR-GRAFO");

            document.AdicionarElemento(transformador);

            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            ElectricGraphNode? node = graph.FindNode(transformador.Id.ToString());

            Assert(node != null, "Transformador deve aparecer como no do ElectricGraph.");
            AssertEqual(2, node!.Terminals.Count, "Terminais do transformador no grafo");
            AssertGraphTerminal(node, Transformador.TERMINAL_PRIMARIO, "Terminal PRIMARIO no grafo");
            AssertGraphTerminal(node, Transformador.TERMINAL_SECUNDARIO, "Terminal SECUNDARIO no grafo");
        }

        private static void TransformadorPreservaConexoesAposReload()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-TR");
            Transformador transformador = CreateTransformador("TR-RELOAD");
            Carga load = CreateLoad("CARGA-TR", 300, 100);
            Cabo primaryCable = CreateCable(sin, 1, transformador, 0, "L-TR-P", 1.0);
            Cabo secondaryCable = CreateCable(transformador, 1, load, 0, "L-TR-S", 1.1);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(load);
            document.AdicionarElemento(primaryCable);
            document.AdicionarElemento(secondaryCable);

            AraciDocument loaded = SaveAndLoad(document);
            Transformador loadedTransformador = FindById<Transformador>(loaded, transformador.Id);
            Cabo loadedPrimary = FindById<Cabo>(loaded, primaryCable.Id);
            Cabo loadedSecondary = FindById<Cabo>(loaded, secondaryCable.Id);
            ElectricGraph graph = new ElectricGraphBuilder(loaded).Build();

            AssertTransformadorTerminals(loadedTransformador, "Transformador apos reload");
            AssertEqual(Transformador.TERMINAL_PRIMARIO, loadedPrimary.DestinoTerminalId, "Primario apos reload");
            AssertEqual(Transformador.TERMINAL_SECUNDARIO, loadedSecondary.OrigemTerminalId, "Secundario apos reload");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Grafo com transformador nao deve ter arestas invalidas");
        }

        private static void TransformadorEntraNoDtoMinimo()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-DTO-TR");
            Transformador transformador = CreateTransformador("TR-DTO");
            Gerador generator = CreateGenerator("GERADOR-DTO-TR", 900, 0.95);
            Carga load = CreateLoad("CARGA-DTO-TR", 300, 100);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, transformador, 0, "L-DTO-TR-P", 1.0));
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-DTO-TR-S", 1.1));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-DTO-TR-G", 1.2));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "SlackDto.Id");
            AssertEqual(13.8, dto.Slack.Tensao, "SlackDto.Tensao");
            AssertEqual(1, dto.Transformers.Count, "Quantidade de transformadores no DTO");
            AssertEqual(transformador.Id.ToString(), dto.Transformers[0].Id, "TransformerDto.Id");
            AssertEqual(transformador.Nome, dto.Transformers[0].Nome, "TransformerDto.Nome");
            AssertEqual(3, dto.Transformers[0].Fases, "TransformerDto.Fases");
            AssertEqual(2, dto.Transformers[0].Enrolamentos, "TransformerDto.Enrolamentos");
            AssertEqual($"{transformador.Nome}_PRIMARIO", dto.Transformers[0].BarraPrimario, "TransformerDto.BarraPrimario");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", dto.Transformers[0].BarraSecundario, "TransformerDto.BarraSecundario");
            AssertEqual(13.8, dto.Transformers[0].TensaoPrimarioKV, "TransformerDto.TensaoPrimarioKV");
            AssertEqual(0.38, dto.Transformers[0].TensaoSecundarioKV, "TransformerDto.TensaoSecundarioKV");
            AssertEqual(500, dto.Transformers[0].PotenciaKVA, "TransformerDto.PotenciaKVA");
            AssertEqual(1, dto.Transformers[0].RPercentual, "TransformerDto.RPercentual");
            AssertEqual(5, dto.Transformers[0].XPercentual, "TransformerDto.XPercentual");
            AssertEqual("Wye", dto.Transformers[0].LigacaoPrimario, "TransformerDto.LigacaoPrimario");
            AssertEqual("Wye", dto.Transformers[0].LigacaoSecundario, "TransformerDto.LigacaoSecundario");
            AssertEqual(sin.Nome, dto.Lines[0].Barra1, "LineDto primario.Barra1");
            AssertEqual($"{transformador.Nome}_PRIMARIO", dto.Lines[0].Barra2, "LineDto primario.Barra2");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", dto.Lines[1].Barra1, "LineDto secundario.Barra1");
            AssertEqual(load.Nome, dto.Lines[1].Barra2, "LineDto secundario.Barra2");
        }

        private static void TransformadorUsaCentroComGeometriaPropria()
        {
            var context = new EditorContext();
            Transformador transformador = context.ElementoFactory.CriarTransformador();
            Point centro = new Point(500, 400);
            Point topoEsquerdo = context.Geometry.CalcularTopoEsquerdoPorCentro(transformador, centro);
            TransformadorViewModel vm = context.ElementoFactory.CriarTransformadorVM();

            AssertEqual(460, topoEsquerdo.X, "Transformador.PosicaoX por centro");
            AssertEqual(330, topoEsquerdo.Y, "Transformador.PosicaoY por centro");
            AssertEqual(ElementGeometryDefaults.TransformadorLargura, vm.Largura, "TransformadorViewModel.Largura");
            AssertEqual(ElementGeometryDefaults.TransformadorAltura, vm.Altura, "TransformadorViewModel.Altura");
        }

        private static void ReloadPreservaDtoDetalhadoTransformador()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-RELOAD-DTO-TR");
            Transformador transformador = CreateTransformador("TR-RELOAD-DTO");
            Gerador generator = CreateGenerator("GERADOR-RELOAD-DTO-TR", 900, 0.95);
            Carga load = CreateLoad("CARGA-RELOAD-DTO-TR", 300, 100);

            transformador.TensaoPrimarioKV = 34.5;
            transformador.TensaoSecundarioKV = 0.69;
            transformador.PotenciaAparente = 1500.0;
            transformador.RPercentual = 0.75;
            transformador.XPercentual = 6.5;
            transformador.LigacaoPrimario = "Delta";
            transformador.LigacaoSecundario = "Wye";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, transformador, 0, "L-RELOAD-TR-P", 1.0));
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-RELOAD-TR-S", 1.1));
            document.AdicionarElemento(CreateCable(generator, 0, load, 0, "L-RELOAD-TR-G", 1.2));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(SaveAndLoad(document))).Build();
            TransformerDto transformerDto = dto.Transformers.Single();

            AssertEqual($"{transformador.Nome}_PRIMARIO", transformerDto.BarraPrimario, "Reload TransformerDto.BarraPrimario");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", transformerDto.BarraSecundario, "Reload TransformerDto.BarraSecundario");
            AssertEqual(34.5, transformerDto.TensaoPrimarioKV, "Reload TransformerDto.TensaoPrimarioKV");
            AssertEqual(0.69, transformerDto.TensaoSecundarioKV, "Reload TransformerDto.TensaoSecundarioKV");
            AssertEqual(1500, transformerDto.PotenciaKVA, "Reload TransformerDto.PotenciaKVA");
            AssertEqual(0.75, transformerDto.RPercentual, "Reload TransformerDto.RPercentual");
            AssertEqual(6.5, transformerDto.XPercentual, "Reload TransformerDto.XPercentual");
            AssertEqual("Delta", transformerDto.LigacaoPrimario, "Reload TransformerDto.LigacaoPrimario");
            AssertEqual("Wye", transformerDto.LigacaoSecundario, "Reload TransformerDto.LigacaoSecundario");
            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "SIN deve continuar slack preferencial");
            AssertEqual(sin.Nome, dto.Lines[0].Barra1, "Reload LineDto primario.Barra1");
            AssertEqual($"{transformador.Nome}_PRIMARIO", dto.Lines[0].Barra2, "Reload LineDto primario.Barra2");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", dto.Lines[1].Barra1, "Reload LineDto secundario.Barra1");
            AssertEqual(load.Nome, dto.Lines[1].Barra2, "Reload LineDto secundario.Barra2");
        }

        private static void CircuitDtoPreservaParametrosReaisSinTransformadorCarga()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-138");
            Transformador transformador = CreateTransformador("TR-65MVA");
            Carga load = CreateLoad("CARGA-34KV", 5000, 1000);

            sin.TensaoLinha = "138";

            transformador.TensaoPrimarioKV = 138.0;
            transformador.TensaoSecundarioKV = 34.5;
            transformador.PotenciaAparente = 65000.0;
            transformador.RPercentual = 1.0;
            transformador.XPercentual = 8.0;
            transformador.LigacaoPrimario = "Wye";
            transformador.LigacaoSecundario = "Wye";

            load.TensaoLinha = "34.5";

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, transformador, 0, "L-REAL-P", 1.0));
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-REAL-S", 1.0));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();
            AssertCircuitDtoParametrosReais(dto, sin, transformador, load, "DTO real");

            CircuitDto reloadedDto = new CircuitBuilder(new ParameterReader(SaveAndLoad(document))).Build();
            AssertCircuitDtoParametrosReais(reloadedDto, sin, transformador, load, "DTO real apos reload");
        }

        private static void DtosAntigosDefaultPreservamSinECarga()
        {
            var document = new AraciDocument();
            Sin sin = new Sin
            {
                Nome = "SIN-DEFAULT",
                Barra = "SIN-DEFAULT",
                PosicaoX = 80,
                PosicaoY = 80,
                Tipo = new TipoSin()
            };
            Carga load = new Carga
            {
                Nome = "CARGA-DEFAULT",
                Barra = "CARGA-DEFAULT",
                PosicaoX = 300,
                PosicaoY = 100,
                Tipo = new TipoCarga()
            };

            sin.AtualizarTerminais(80, 80);
            load.AtualizarTerminais(80);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, load, 0, "L-DEFAULT", 1.0));

            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            AssertEqual(12.47, dto.Slack.Tensao, "Default Slack.Tensao");
            AssertEqual(12.47, dto.Loads.Single().Tensao, "Default Load.Tensao");
            AssertEqual(800, dto.Loads.Single().PotenciaAtiva, "Default Load.PotenciaAtiva");
            AssertEqual(300, dto.Loads.Single().PotenciaReativa, "Default Load.PotenciaReativa");
        }

        private static void TopologyValidatorAceitaSinTransformadorCargaSemGerador()
        {
            var document = new AraciDocument();
            Sin sin = CreateSin("SIN-TOPO-TR");
            Transformador transformador = CreateTransformador("TR-TOPO");
            Carga load = CreateLoad("CARGA-TOPO-TR", 300, 100);

            document.AdicionarElemento(sin);
            document.AdicionarElemento(transformador);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(sin, 1, transformador, 0, "L-TOPO-TR-P", 1.0));
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-TOPO-TR-S", 1.1));

            TopologyValidationResult result = new TopologyValidator(document).Validate();
            CircuitDto dto = new CircuitBuilder(new ParameterReader(document)).Build();

            Assert(result.IsValid, $"Topologia com SIN deve ser valida. Erros: {result.FormatErrors()}");
            AssertEqual(sin.Id.ToString(), dto.Slack.Id, "SIN deve virar SlackDto sem gerador");
            AssertEqual(0, dto.Generators.Count, "Sem geradores no DTO");
        }

        private static void TopologyValidatorAceitaGeradorLegadoSemSin()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            TopologyValidationResult result = new TopologyValidator(circuit.Document).Validate();
            CircuitDto dto = new CircuitBuilder(new ParameterReader(circuit.Document)).Build();

            Assert(result.IsValid, $"Topologia com gerador legado deve ser valida. Erros: {result.FormatErrors()}");
            AssertEqual(circuit.Generator.Id.ToString(), dto.Slack.Id, "Gerador legado deve virar SlackDto sem SIN");
        }

        private static void TopologyValidatorSemFonteSlackFalhaComMensagemClara()
        {
            var document = new AraciDocument();
            Transformador transformador = CreateTransformador("TR-SEM-FONTE");
            Carga load = CreateLoad("CARGA-SEM-FONTE", 300, 100);

            document.AdicionarElemento(transformador);
            document.AdicionarElemento(load);
            document.AdicionarElemento(CreateCable(transformador, 1, load, 0, "L-SEM-FONTE", 1.0));

            TopologyValidationResult result = new TopologyValidator(document).Validate();
            string errors = result.FormatErrors();

            Assert(!result.IsValid, "Topologia sem SIN e sem gerador deve falhar.");
            AssertContains(errors, "fonte slack", "Erro sem fonte slack");
            Assert(
                !errors.Contains("sem gerador", StringComparison.OrdinalIgnoreCase),
                $"Erro sem fonte nao deve mencionar apenas gerador. Texto: {errors}");
        }

        private static void TerminalEndpointIdentificaConexaoPorValor()
        {
            SimpleCircuit circuit = CreateSimpleCircuit();
            Terminal origem = GetTerminal(circuit.Generator, 0);
            Terminal destino = GetTerminal(circuit.Load, 0);
            TerminalEndpoint endpointOrigem = TerminalEndpoint.FromTerminal(origem);
            TerminalEndpoint endpointDestino = new(circuit.Load.Id.ToString(), destino.Id);
            ConnectivityService connectivity = new(circuit.Document);

            Assert(endpointOrigem.IsComplete, "Endpoint de origem deve estar completo.");
            AssertEqual(origem.Id, endpointOrigem.TerminalId, "Endpoint.TerminalId");
            AssertEqual(origem, connectivity.ObterTerminal(endpointOrigem), "Resolver endpoint origem");
            AssertEqual(circuit.Cable, connectivity.ObterCabosConectados(endpointOrigem).Single(), "Cabo conectado ao endpoint origem");
            AssertEqual(circuit.Cable, connectivity.ObterCabosConectados(endpointDestino).Single(), "Cabo conectado ao endpoint destino");
        }

        private static void RotacaoRecalculaTerminalPorPosicaoLocal()
        {
            var generator = new Gerador
            {
                PosicaoX = 100,
                PosicaoY = 100,
                Rotacao = 90
            };

            generator.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);
            Terminal topo = generator.Terminais.Single(t => t.Id == "TOPO");
            Terminal direita = generator.Terminais.Single(t => t.Id == "DIREITA");

            AssertEqual(170, topo.Posicao.X, "Topo rotacionado X");
            AssertEqual(135, topo.Posicao.Y, "Topo rotacionado Y");
            AssertEqual(135, direita.Posicao.X, "Direita rotacionada X");
            AssertEqual(170, direita.Posicao.Y, "Direita rotacionada Y");
        }

        private static void TerminalPlacementUsaPivoCentral()
        {
            var elemento = new Carga
            {
                PosicaoX = 100,
                PosicaoY = 100,
                Rotacao = 90
            };

            Point world = TerminalPlacement.ToWorld(elemento, new Point(35, 0), 70, 70);

            AssertEqual(170, world.X, "Terminal central 90 X");
            AssertEqual(135, world.Y, "Terminal central 90 Y");
        }

        private static void TerminalPlacementToLocalInverteToWorld()
        {
            var elemento = new Carga
            {
                PosicaoX = 123,
                PosicaoY = 77
            };

            var locals = new[]
            {
                new Point(35, 0),
                new Point(70, 35),
                new Point(35, 70),
                new Point(0, 35),
                new Point(10, 20)
            };

            foreach (double rotation in new double[] { 0, 90, 180, 270 })
            {
                elemento.Rotacao = rotation;

                foreach (Point local in locals)
                {
                    Point world = TerminalPlacement.ToWorld(elemento, local, 70, 70);
                    Point actual = TerminalPlacement.ToLocal(elemento, world, 70, 70);

                    AssertEqual(local.X, actual.X, $"ToLocal inverso {rotation}.X");
                    AssertEqual(local.Y, actual.Y, $"ToLocal inverso {rotation}.Y");
                }
            }
        }

        private static void CargaRotacionadaAlinhaTerminalComPivoCentral()
        {
            Carga load = CreateLoad("CARGA-CENTRAL", 300, 100);
            load.Rotacao = 90;
            load.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            AssertTerminalsUseCentralPivot(
                load,
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura,
                "Carga");
        }

        private static void GeradorRotacionadoAlinhaTerminaisComPivoCentral()
        {
            Gerador generator = CreateGenerator("GER-CENTRAL", 1000, 0.95);
            generator.Rotacao = 90;
            generator.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            AssertTerminalsUseCentralPivot(
                generator,
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura,
                "Gerador");
        }

        private static void SinRotacionadoAlinhaTerminaisComPivoCentral()
        {
            Sin sin = CreateSin("SIN-CENTRAL");
            sin.Rotacao = 90;
            sin.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            AssertTerminalsUseCentralPivot(
                sin,
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura,
                "SIN");
        }

        private static void TransformadorRotacionadoAlinhaTerminaisComPivoCentral()
        {
            Transformador transformador = CreateTransformador("TR-CENTRAL");
            transformador.Rotacao = 90;
            transformador.AtualizarTerminais(
                ElementGeometryDefaults.TransformadorLargura,
                ElementGeometryDefaults.TransformadorAltura);

            AssertTerminalsUseCentralPivot(
                transformador,
                ElementGeometryDefaults.TransformadorLargura,
                ElementGeometryDefaults.TransformadorAltura,
                "Transformador");
        }

        private static void BarraRotacionadaAlinhaTerminaisComPivoCentral()
        {
            Barra bar = CreateBar("BARRA-CENTRAL");
            bar.Rotacao = 90;
            bar.AtualizarTerminais(ElementGeometryDefaults.BarraLargura);

            AssertTerminalsUseCentralPivot(
                bar,
                ElementGeometryDefaults.BarraLargura,
                bar.Altura,
                "Barra");
        }

        private static void ElectricGraphBfsPercorreConexoesValidas()
        {
            AraciDocument document = CreateBranchDocument();
            Gerador generator = document.Elementos.OfType<Gerador>().Single();
            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            IReadOnlyList<ElectricGraphNode> visited = graph.BreadthFirst(generator.Id.ToString());

            AssertEqual(4, visited.Count, "Quantidade de nos no BFS");
            Assert(visited.All(n => graph.FindNode(n.ElementId) != null), "BFS deve retornar apenas nos do grafo.");
        }

        private static void RotacaoMaisNoventaAtualizaModelo()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-ROT", 300, 100);
            context.Document.AdicionarElemento(load);
            ElementoViewModel vm = GetVm(context, load);

            context.Selection.Selecionar(vm);
            bool rotated = context.Rotation.RotateSelectionClockwise();

            Assert(rotated, "Rotacao deve ser aplicada.");
            AssertEqual(90, load.Rotacao, "Rotacao da carga");
        }

        private static void RotacaoCiclaQuadrantes()
        {
            double value = 0;

            value = RotationService.RotateClockwise(value);
            AssertEqual(90, value, "Rotacao 0 -> 90");
            value = RotationService.RotateClockwise(value);
            AssertEqual(180, value, "Rotacao 90 -> 180");
            value = RotationService.RotateClockwise(value);
            AssertEqual(270, value, "Rotacao 180 -> 270");
            value = RotationService.RotateClockwise(value);
            AssertEqual(0, value, "Rotacao 270 -> 0");
        }

        private static void PreviewPreservaRotacaoEmModeloReal()
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<CargaViewModel, Carga>(
                context,
                context.ElementoFactory.CriarCargaVM,
                vm => (Carga)vm.Modelo);

            controller.Update(new Point(240, 180));
            controller.RotateClockwise();
            controller.RotateClockwise();

            Carga real = context.ElementoFactory.CriarCarga();
            real.Rotacao = controller.CurrentRotation;

            AssertEqual(180, real.Rotacao, "Rotacao copiada do preview");
        }

        private static void PreviewArmazenaRotacaoAntesDeExistir()
        {
            AssertPreviewArmazenaRotacaoAntesDeExistir<CargaViewModel, Carga>(
                "Carga",
                context => context.ElementoFactory.CriarCargaVM(),
                vm => (Carga)vm.Modelo);

            AssertPreviewArmazenaRotacaoAntesDeExistir<TransformadorViewModel, Transformador>(
                "Transformador",
                context => context.ElementoFactory.CriarTransformadorVM(),
                vm => (Transformador)vm.Modelo);

            AssertPreviewArmazenaRotacaoAntesDeExistir<BarraViewModel, Barra>(
                "Barra",
                context => context.ElementoFactory.CriarBarraVM(),
                vm => vm.Barra);
        }

        private static void PreviewExistenteRotacionaVisualmente()
        {
            AssertPreviewExistenteRotacionaVisualmente<CargaViewModel, Carga>(
                "Carga",
                context => context.ElementoFactory.CriarCargaVM(),
                vm => (Carga)vm.Modelo);

            AssertPreviewExistenteRotacionaVisualmente<TransformadorViewModel, Transformador>(
                "Transformador",
                context => context.ElementoFactory.CriarTransformadorVM(),
                vm => (Transformador)vm.Modelo);

            AssertPreviewExistenteRotacionaVisualmente<BarraViewModel, Barra>(
                "Barra",
                context => context.ElementoFactory.CriarBarraVM(),
                vm => vm.Barra);
        }

        private static void UpdateDoPreviewNaoResetaRotacao()
        {
            AssertUpdateDoPreviewNaoResetaRotacao<CargaViewModel, Carga>(
                "Carga",
                context => context.ElementoFactory.CriarCargaVM(),
                vm => (Carga)vm.Modelo);

            AssertUpdateDoPreviewNaoResetaRotacao<TransformadorViewModel, Transformador>(
                "Transformador",
                context => context.ElementoFactory.CriarTransformadorVM(),
                vm => (Transformador)vm.Modelo);

            AssertUpdateDoPreviewNaoResetaRotacao<BarraViewModel, Barra>(
                "Barra",
                context => context.ElementoFactory.CriarBarraVM(),
                vm => vm.Barra);
        }

        private static void ModeloRealRecebeRotacaoDoPreview()
        {
            AssertModeloRealRecebeRotacaoDoPreview<CargaViewModel, Carga>(
                "Carga",
                context => context.ElementoFactory.CriarCargaVM(),
                context => context.ElementoFactory.CriarCarga(),
                vm => (Carga)vm.Modelo);

            AssertModeloRealRecebeRotacaoDoPreview<TransformadorViewModel, Transformador>(
                "Transformador",
                context => context.ElementoFactory.CriarTransformadorVM(),
                context => context.ElementoFactory.CriarTransformador(),
                vm => (Transformador)vm.Modelo);

            AssertModeloRealRecebeRotacaoDoPreview<BarraViewModel, Barra>(
                "Barra",
                context => context.ElementoFactory.CriarBarraVM(),
                context => context.ElementoFactory.CriarBarra(),
                vm => vm.Barra);
        }

        private static void InputRouterEnviaSpaceParaInsercaoSemPreview()
        {
            RunSta(() =>
            {
                EditorContext context = CreateContextWithViewport();
                Assert(
                    context.Tools.AtivarInsercaoElemento(ElementRegistryService.KindCarga),
                    "Ferramenta de insercao de Carga deve ser ativada pelo ToolService.");

                Assert(context.Input.KeyDown(Key.Space), "InputRouter deve consumir Space na ferramenta de insercao sem preview.");
            });
        }

        private static void FerramentaLinhaAnotativaCriaPreviewSegmentosEUndoRedo()
        {
            RunSta(() =>
            {
                EditorContext context = CreateContextWithViewport();
                var semModificador = new ToolInputState(ModifierKeys.None, MouseButton.Left);
                var comShift = new ToolInputState(ModifierKeys.Shift, MouseButton.Left);
                var comCtrl = new ToolInputState(ModifierKeys.Control, MouseButton.Left);
                var comCtrlShift = new ToolInputState(ModifierKeys.Control | ModifierKeys.Shift, MouseButton.Left);

                context.Tools.AtivarInserirLinhaAnotativa();
                AssertEqual("Linha", context.Tools.FerramentaAtual.Nome, "Ferramenta Linha.Nome");
                Assert(context.Tools.FerramentaAtual.MantemBotaoAtivado, "Ferramenta Linha deve manter botao ativo.");

                context.Input.MouseDown(null, new Point(10, 20), semModificador);
                context.Input.MouseMove(new Point(110, 80), semModificador);

                LinhaAnotativaViewModel preview = context.Scene.Elementos
                    .OfType<LinhaAnotativaViewModel>()
                    .Single(vm => vm.IsPreview);

                AssertEqual(0, context.Document.Elementos.Count, "Preview nao deve entrar no Document.");
                AssertEqual(10, preview.Linha.PosicaoX, "Preview.PosicaoX");
                AssertEqual(20, preview.Linha.PosicaoY, "Preview.PosicaoY");
                AssertEqual(100, preview.Linha.X2, "Preview.X2");
                AssertEqual(60, preview.Linha.Y2, "Preview.Y2");

                context.Input.MouseDown(null, new Point(110, 80), semModificador);

                LinhaAnotativa linha = context.Document.Elementos.OfType<LinhaAnotativa>().Single();
                AssertEqual(10, linha.PosicaoX, "Linha.PosicaoX");
                AssertEqual(20, linha.PosicaoY, "Linha.PosicaoY");
                AssertEqual(100, linha.X2, "Linha.X2");
                AssertEqual(60, linha.Y2, "Linha.Y2");
                Assert(linha.Tipo is TipoLinhaAnotativa, "Linha criada sem modificador deve receber TipoLinhaAnotativa.");
                AssertEqual(ElementoDomainRole.Anotacao, linha.DomainRole, "Linha.DomainRole");
                Assert(!linha.ParticipaDoGrafoEletrico, "Linha nao deve participar do grafo eletrico.");
                Assert(!context.Scene.Elementos.OfType<LinhaAnotativaViewModel>().Any(vm => vm.IsPreview), "Preview deve ser removido sem Shift.");
                Assert(context.Scene.Elementos.OfType<LinhaAnotativaViewModel>().Any(vm => ReferenceEquals(vm.Modelo, linha)), "Linha definitiva deve entrar na Scene.");
                AssertEqual("Selecionar", context.Tools.FerramentaAtual.Nome, "Ferramenta apos finalizar sem Shift");

                context.Commands.Undo();
                AssertEqual(0, context.Document.Elementos.OfType<LinhaAnotativa>().Count(), "Undo deve remover LinhaAnotativa.");
                AssertEqual(0, context.Scene.Elementos.OfType<LinhaAnotativaViewModel>().Count(), "Undo deve remover ViewModel da linha.");

                context.Commands.Redo();
                AssertEqual(1, context.Document.Elementos.OfType<LinhaAnotativa>().Count(), "Redo deve recriar LinhaAnotativa.");
                AssertEqual(1, context.Scene.Elementos.OfType<LinhaAnotativaViewModel>().Count(), "Redo deve recriar ViewModel da linha.");

                EditorContext shiftUnico = CreateContextWithViewport();
                shiftUnico.Tools.AtivarInserirLinhaAnotativa();
                shiftUnico.Input.MouseDown(null, new Point(0, 0), semModificador);
                shiftUnico.Input.MouseMove(new Point(50, 20), comShift);

                LinhaAnotativaViewModel previewOrtogonal = shiftUnico.Scene.Elementos
                    .OfType<LinhaAnotativaViewModel>()
                    .Single(vm => vm.IsPreview);

                AssertEqual(50, previewOrtogonal.Linha.X2, "Preview ortogonal.X2");
                AssertEqual(0, previewOrtogonal.Linha.Y2, "Preview ortogonal.Y2");

                shiftUnico.Input.MouseDown(null, new Point(50, 20), comShift);

                LinhaAnotativa linhaShift = shiftUnico.Document.Elementos.OfType<LinhaAnotativa>().Single();

                AssertEqual("Selecionar", shiftUnico.Tools.FerramentaAtual.Nome, "Ferramenta deve voltar para selecao apos Shift sem Ctrl.");
                AssertEqual(50, linhaShift.X2, "Linha Shift.X2");
                AssertEqual(0, linhaShift.Y2, "Linha Shift.Y2");
                Assert(!shiftUnico.Scene.Elementos.OfType<LinhaAnotativaViewModel>().Any(vm => vm.IsPreview), "Preview deve ser removido apos Shift sem Ctrl.");

                EditorContext continuoLivre = CreateContextWithViewport();
                continuoLivre.Tools.AtivarInserirLinhaAnotativa();
                continuoLivre.Input.MouseDown(null, new Point(0, 0), semModificador);
                continuoLivre.Input.MouseDown(null, new Point(50, 20), comCtrl);
                continuoLivre.Input.MouseMove(new Point(70, 60), comCtrl);
                continuoLivre.Input.MouseDown(null, new Point(70, 60), comCtrl);

                LinhaAnotativa[] linhasLivres = continuoLivre.Document.Elementos.OfType<LinhaAnotativa>().ToArray();

                AssertEqual("Linha", continuoLivre.Tools.FerramentaAtual.Nome, "Ferramenta deve permanecer ativa com Ctrl.");
                AssertEqual(2, linhasLivres.Length, "Quantidade de segmentos com Ctrl");
                AssertEqual(0, linhasLivres[0].PosicaoX, "Primeiro segmento Ctrl.PosicaoX");
                AssertEqual(0, linhasLivres[0].PosicaoY, "Primeiro segmento Ctrl.PosicaoY");
                AssertEqual(50, linhasLivres[0].X2, "Primeiro segmento Ctrl.X2");
                AssertEqual(20, linhasLivres[0].Y2, "Primeiro segmento Ctrl.Y2");
                AssertEqual(50, linhasLivres[1].PosicaoX, "Segundo segmento Ctrl.PosicaoX");
                AssertEqual(20, linhasLivres[1].PosicaoY, "Segundo segmento Ctrl.PosicaoY");
                AssertEqual(20, linhasLivres[1].X2, "Segundo segmento Ctrl.X2");
                AssertEqual(40, linhasLivres[1].Y2, "Segundo segmento Ctrl.Y2");

                EditorContext continuoOrtogonal = CreateContextWithViewport();
                continuoOrtogonal.Tools.AtivarInserirLinhaAnotativa();
                continuoOrtogonal.Input.MouseDown(null, new Point(0, 0), semModificador);
                continuoOrtogonal.Input.MouseMove(new Point(50, 20), comCtrlShift);
                continuoOrtogonal.Input.MouseDown(null, new Point(50, 20), comCtrlShift);

                AssertEqual("Linha", continuoOrtogonal.Tools.FerramentaAtual.Nome, "Ferramenta deve permanecer ativa com Ctrl+Shift.");
                AssertEqual(1, continuoOrtogonal.Document.Elementos.OfType<LinhaAnotativa>().Count(), "Primeiro segmento com Ctrl+Shift");
                Assert(continuoOrtogonal.Scene.Elementos.OfType<LinhaAnotativaViewModel>().Any(vm => vm.IsPreview), "Preview deve continuar apos Ctrl+Shift.");

                continuoOrtogonal.Input.MouseMove(new Point(70, 60), comCtrlShift);
                continuoOrtogonal.Input.MouseDown(null, new Point(70, 60), comCtrlShift);

                LinhaAnotativa[] linhasOrtogonais = continuoOrtogonal.Document.Elementos.OfType<LinhaAnotativa>().ToArray();
                AssertEqual(2, linhasOrtogonais.Length, "Quantidade de segmentos com Ctrl+Shift");
                AssertEqual(0, linhasOrtogonais[0].PosicaoX, "Primeiro segmento Ctrl+Shift.PosicaoX");
                AssertEqual(0, linhasOrtogonais[0].PosicaoY, "Primeiro segmento Ctrl+Shift.PosicaoY");
                AssertEqual(50, linhasOrtogonais[0].X2, "Primeiro segmento Ctrl+Shift.X2");
                AssertEqual(0, linhasOrtogonais[0].Y2, "Primeiro segmento Ctrl+Shift.Y2");
                AssertEqual(50, linhasOrtogonais[1].PosicaoX, "Segundo segmento Ctrl+Shift.PosicaoX");
                AssertEqual(0, linhasOrtogonais[1].PosicaoY, "Segundo segmento Ctrl+Shift.PosicaoY");
                AssertEqual(0, linhasOrtogonais[1].X2, "Segundo segmento Ctrl+Shift.X2");
                AssertEqual(60, linhasOrtogonais[1].Y2, "Segundo segmento Ctrl+Shift.Y2");

                continuoOrtogonal.Input.KeyDown(Key.Escape);

                AssertEqual("Selecionar", continuoOrtogonal.Tools.FerramentaAtual.Nome, "Esc deve voltar para selecao.");
                Assert(!continuoOrtogonal.Scene.Elementos.OfType<LinhaAnotativaViewModel>().Any(vm => vm.IsPreview), "Esc deve remover preview.");
                AssertEqual(2, continuoOrtogonal.Document.Elementos.OfType<LinhaAnotativa>().Count(), "Esc nao deve remover linhas definitivas.");
            });
        }

        private static void LinhaAnotativaInclinadaMovePreservandoAncoras()
        {
            RunSta(() =>
            {
                EditorContext context = CreateContextWithViewport();
                var linha = new LinhaAnotativa
                {
                    Nome = "Linha SW NE",
                    PosicaoX = 100,
                    PosicaoY = 100,
                    X2 = -50,
                    Y2 = -50,
                    Rotacao = 0
                };

                context.Document.AdicionarElemento(linha);

                if (GetVm(context, linha) is not LinhaAnotativaViewModel vm)
                    throw new InvalidOperationException("ViewModel de LinhaAnotativa nao encontrada.");

                context.Move.BeginMove(new[] { vm });
                context.Move.MoverVisual(vm, new Vector(20, 30));
                context.Move.EndMove(new[] { vm });

                AssertEqual(120, linha.PosicaoX, "Linha inclinada.PosicaoX apos move");
                AssertEqual(130, linha.PosicaoY, "Linha inclinada.PosicaoY apos move");
                AssertEqual(-50, linha.X2, "Linha inclinada.X2 apos move");
                AssertEqual(-50, linha.Y2, "Linha inclinada.Y2 apos move");
                AssertEqual(70, vm.WorldX, "Linha inclinada.WorldX apos move");
                AssertEqual(80, vm.WorldY, "Linha inclinada.WorldY apos move");

                context.Commands.Undo();

                AssertEqual(100, linha.PosicaoX, "Linha inclinada.PosicaoX apos undo");
                AssertEqual(100, linha.PosicaoY, "Linha inclinada.PosicaoY apos undo");
                AssertEqual(-50, linha.X2, "Linha inclinada.X2 apos undo");
                AssertEqual(-50, linha.Y2, "Linha inclinada.Y2 apos undo");
                AssertEqual(50, vm.WorldX, "Linha inclinada.WorldX apos undo");
                AssertEqual(50, vm.WorldY, "Linha inclinada.WorldY apos undo");

                context.Commands.Redo();

                AssertEqual(120, linha.PosicaoX, "Linha inclinada.PosicaoX apos redo");
                AssertEqual(130, linha.PosicaoY, "Linha inclinada.PosicaoY apos redo");
                AssertEqual(-50, linha.X2, "Linha inclinada.X2 apos redo");
                AssertEqual(-50, linha.Y2, "Linha inclinada.Y2 apos redo");
                AssertEqual(70, vm.WorldX, "Linha inclinada.WorldX apos redo");
                AssertEqual(80, vm.WorldY, "Linha inclinada.WorldY apos redo");
            });
        }

        private static void BotoesDaRibbonNaoCapturamFoco()
        {
            AssertButtonsNotFocusable("Ribbon/Tabs/DiagramaTab.xaml", "DiagramaTab");
            AssertButtonsNotFocusable("Ribbon/Tabs/EditarTab.xaml", "EditarTab");
            AssertButtonsNotFocusable("Ribbon/RibbonView.xaml", "RibbonView");
        }

        private static void ViewportContinuaFocavel()
        {
            string xaml = File.ReadAllText(FindProjectFile("Views/ViewportView.xaml"));

            AssertContains(xaml, "Focusable=\"True\"", "ViewportView.Focusable");
        }

        private static void CatalogoPreservaRibbonOrdemEAtalhos()
        {
            EditorContext context = new();
            var definitions = context.Elements.RibbonDefinitions.ToList();
            var expected = new[]
            {
                (ElementKinds.Cabo, "CB"),
                (ElementKinds.Carga, "CG"),
                (ElementKinds.Gerador, "GE"),
                (ElementKinds.Sin, "SI"),
                (ElementKinds.Transformador, "TR"),
                (ElementKinds.Barra, "BA")
            };

            AssertEqual(expected.Length, definitions.Count, "Quantidade de definicoes da Ribbon");

            for (int i = 0; i < expected.Length; i++)
            {
                AssertEqual(expected[i].Item1, definitions[i].Kind, $"Ribbon[{i}].Kind");
                AssertEqual(expected[i].Item2, definitions[i].Atalho, $"Ribbon[{i}].Atalho");
            }
        }

        private static void CatalogoRegistraLinhaAnotativaComViewModelMinima()
        {
            EditorContext context = CreateContextWithViewport();

            ElementDefinition? definition = context.Elements.FindByKind(ElementKinds.LinhaAnotativa);

            if (definition == null)
                throw new InvalidOperationException("LinhaAnotativa deve estar registrada no catalogo.");

            AssertEqual(ElementKinds.LinhaAnotativa, definition.Kind, "LinhaAnotativa.Kind");
            AssertEqual("Linha", definition.NomeAmigavel, "LinhaAnotativa.NomeAmigavel");
            AssertEqual("LINHA", definition.PrefixoNome, "LinhaAnotativa.PrefixoNome");
            AssertEqual(typeof(LinhaAnotativa), definition.ModelType, "LinhaAnotativa.ModelType");
            AssertEqual(typeof(LinhaAnotativaViewModel), definition.ViewModelType, "LinhaAnotativa.ViewModelType");
            AssertEqual(typeof(TipoLinhaAnotativa), definition.TypeModelType, "LinhaAnotativa.TypeModelType");
            Assert(definition.ObterTipoPadrao() is TipoLinhaAnotativa, "LinhaAnotativa deve possuir TipoLinhaAnotativaPadrao.");
            AssertEqual(4, definition.ObterTipos().OfType<TipoLinhaAnotativa>().Count(), "LinhaAnotativa.TiposDisponiveis.Count");
            Assert(!definition.ExibirNoRibbon, "LinhaAnotativa nao deve aparecer na Ribbon.");
            Assert(!definition.UsaFerramentaEspecial, "LinhaAnotativa nao deve usar ferramenta especial.");
            Assert(!context.Elements.RibbonDefinitions.Any(d => d.Kind == ElementKinds.LinhaAnotativa), "RibbonDefinitions nao deve conter LinhaAnotativa.");
            AssertLinhaAnotativaTemplateRegistrado();

            Elemento modelo = context.ElementoFactory.CriarModelo(ElementKinds.LinhaAnotativa);

            if (modelo is not LinhaAnotativa linha)
                throw new InvalidOperationException("ElementoModelFactory deve criar LinhaAnotativa.");

            AssertEqual(ElementoDomainRole.Anotacao, linha.DomainRole, "LinhaAnotativa criada.DomainRole");
            Assert(!linha.ParticipaDoGrafoEletrico, "LinhaAnotativa criada nao deve participar do grafo eletrico.");
            Assert(linha.PossuiParametro(ElementoAnotativo.PARAM_COR_LINHA), "LinhaAnotativa criada deve possuir CorLinha.");
            Assert(linha.PossuiParametro(ElementoAnotativo.PARAM_ESPESSURA_LINHA), "LinhaAnotativa criada deve possuir EspessuraLinha.");
            Assert(linha.PossuiParametro(ElementoAnotativo.PARAM_VISIVEL), "LinhaAnotativa criada deve possuir Visivel.");
            Assert(linha.PossuiParametro(LinhaAnotativa.PARAM_X2), "LinhaAnotativa criada deve possuir X2.");
            Assert(linha.PossuiParametro(LinhaAnotativa.PARAM_Y2), "LinhaAnotativa criada deve possuir Y2.");
            Assert(!linha.PossuiParametro(TipoLinhaAnotativa.PARAM_ESTILO_LINHA), "LinhaAnotativa criada nao deve possuir EstiloLinha de instancia.");
            Assert(linha.Tipo is TipoLinhaAnotativa, "LinhaAnotativa criada deve receber TipoLinhaAnotativa.");
            Assert(!linha.ParticipaDoGrafoEletrico, "LinhaAnotativa criada deve continuar fora do grafo eletrico.");

            if (context.ElementoFactory.CriarViewModel(linha) is not LinhaAnotativaViewModel vm)
                throw new InvalidOperationException("LinhaAnotativa deve criar LinhaAnotativaViewModel.");

            Assert(ReferenceEquals(linha, vm.Modelo), "LinhaAnotativaViewModel deve referenciar a linha original.");
            AssertEqual(4, vm.TiposDisponiveis.Cast<object>().Count(), "LinhaAnotativaViewModel.TiposDisponiveis.Count");
            Assert(vm.TipoViewModel is TipoLinhaAnotativaViewModel, "LinhaAnotativaViewModel.TipoViewModel");
            Assert(vm.Node.GetType().Name == "LinhaAnotativaNode", "LinhaAnotativaViewModel deve usar LinhaAnotativaNode.");
            AssertEqual(vm.Bounds.X, vm.WorldX, "LinhaAnotativaViewModel.WorldX");
            AssertEqual(vm.Bounds.Y, vm.WorldY, "LinhaAnotativaViewModel.WorldY");
            AssertEqual(0.0, vm.RenderData.PontoLocalInicial.X, "Linha direita.RenderData.Inicial.X");
            AssertEqual(0.0, vm.RenderData.PontoLocalInicial.Y, "Linha direita.RenderData.Inicial.Y");
            AssertEqual(100.0, vm.RenderData.PontoLocalFinal.X, "Linha direita.RenderData.Final.X");
            AssertEqual(0.0, vm.RenderData.PontoLocalFinal.Y, "Linha direita.RenderData.Final.Y");
            AssertEqual(100.0, vm.Comprimento, "Linha direita.Comprimento");

            vm.X2 = -100.0;
            vm.Y2 = 50.0;

            AssertEqual(-100.0, linha.X2, "LinhaAnotativaViewModel.X2 atualiza modelo");
            AssertEqual(50.0, linha.Y2, "LinhaAnotativaViewModel.Y2 atualiza modelo");
            AssertEqual(100.0, vm.RenderData.PontoLocalInicial.X, "Linha negativa.RenderData.Inicial.X");
            AssertEqual(0.0, vm.RenderData.PontoLocalInicial.Y, "Linha negativa.RenderData.Inicial.Y");
            AssertEqual(0.0, vm.RenderData.PontoLocalFinal.X, "Linha negativa.RenderData.Final.X");
            AssertEqual(50.0, vm.RenderData.PontoLocalFinal.Y, "Linha negativa.RenderData.Final.Y");
            AssertEqual(Math.Sqrt(12500.0), vm.Comprimento, "Linha negativa.Comprimento");
            AssertEqual(vm.Bounds.X, vm.WorldX, "Linha negativa.WorldX");
            AssertEqual(vm.Bounds.Y, vm.WorldY, "Linha negativa.WorldY");

            vm.CorLinha = "#FF112233";
            vm.EspessuraLinha = 4.5;
            vm.Visivel = false;
            vm.Tipo = context.Types.TiposLinhasAnotativas.Single(t => t.EstiloLinha == "Tracejado");

            AssertEqual("#FF112233", linha.CorLinha, "LinhaAnotativaViewModel.CorLinha atualiza modelo");
            AssertEqual(4.5, linha.EspessuraLinha, "LinhaAnotativaViewModel.EspessuraLinha atualiza modelo");
            AssertEqual(false, linha.Visivel, "LinhaAnotativaViewModel.Visivel atualiza modelo");
            AssertEqual("Tracejado", vm.EstiloLinha, "LinhaAnotativaViewModel.EstiloLinha vem do tipo");
            AssertEqual(2, vm.RenderData.StrokeDashArray?.Count ?? 0, "LinhaAnotativaViewModel.StrokeDashArray tracejado");

            context.Document.AdicionarElemento(linha);

            Assert(context.Viewport?.ObterViewModel(linha) is LinhaAnotativaViewModel, "DocumentSceneSync deve criar ViewModel para LinhaAnotativa.");
            AssertEqual(1, context.Scene.Elementos.Count, "Scene deve receber ViewModel para LinhaAnotativa.");

            Assert(context.Elements.FindByKind(ElementKinds.Cabo) != null, "Cabo deve continuar registrado.");
            Assert(context.Elements.FindByKind(ElementKinds.Carga) != null, "Carga deve continuar registrada.");
            Assert(context.Elements.FindByKind(ElementKinds.Gerador) != null, "Gerador deve continuar registrado.");
            Assert(context.Elements.FindByKind(ElementKinds.Sin) != null, "SIN deve continuar registrado.");
            Assert(context.Elements.FindByKind(ElementKinds.Transformador) != null, "Transformador deve continuar registrado.");
            Assert(context.Elements.FindByKind(ElementKinds.Barra) != null, "Barra deve continuar registrada.");
        }

        private static void AssertLinhaAnotativaTemplateRegistrado()
        {
            string xaml = File.ReadAllText(FindProjectFile("Views/ViewportView.xaml"));

            AssertContains(xaml, "DataType=\"{x:Type viewModels:LinhaAnotativaViewModel}\"", "LinhaAnotativa.Template.DataType");
            AssertContains(xaml, "<Line", "LinhaAnotativa.Template.Line");
            AssertContains(xaml, "X1=\"{Binding RenderData.PontoLocalInicial.X}\"", "LinhaAnotativa.Template.X1");
            AssertContains(xaml, "Y1=\"{Binding RenderData.PontoLocalInicial.Y}\"", "LinhaAnotativa.Template.Y1");
            AssertContains(xaml, "X2=\"{Binding RenderData.PontoLocalFinal.X}\"", "LinhaAnotativa.Template.X2");
            AssertContains(xaml, "Y2=\"{Binding RenderData.PontoLocalFinal.Y}\"", "LinhaAnotativa.Template.Y2");
            AssertContains(xaml, "Stroke=\"{Binding RenderData.Stroke}\"", "LinhaAnotativa.Template.Stroke");
            AssertContains(xaml, "StrokeThickness=\"{Binding RenderData.StrokeThickness}\"", "LinhaAnotativa.Template.StrokeThickness");
            AssertContains(xaml, "StrokeDashArray=\"{Binding RenderData.StrokeDashArray}\"", "LinhaAnotativa.Template.StrokeDashArray");
        }

        private static void LinhaAnotativaExpoePropriedadesCorEHitTest()
        {
            EditorContext context = CreateContextWithViewport();
            var provider = new ElementInstancePropertyProvider();
            IReadOnlyList<InstancePropertyDescriptor> descriptors = provider.LinhaAnotativa();

            AssertEqual(4, descriptors.Count, "LinhaAnotativa descriptors.Count");
            AssertDescriptor(descriptors[0], "Nome", "Nome", 10, false);
            AssertDescriptor(descriptors[1], "Comprimento", "Comprimento", 20, false);
            AssertEqual(UnitKind.LengthMeter, descriptors[1].Unit, "Comprimento.Unit");
            AssertDescriptor(descriptors[2], "CorLinha", "Cor da linha", 30, true);
            Assert(descriptors[2].IsColor, "CorLinha deve ser marcada como propriedade de cor.");
            AssertDescriptor(descriptors[3], "EspessuraLinha", "Espessura da linha", 40, true);
            Assert(ColorPickerWindow.TryNormalizeHexColor("#000000", out string pretoNormalizado), "ColorPicker deve aceitar #RRGGBB.");
            AssertEqual("#FF000000", pretoNormalizado, "ColorPicker #000000 normalizado");
            Assert(ColorPickerWindow.TryNormalizeHexColor("000000", out string pretoSemHashNormalizado), "ColorPicker deve aceitar RRGGBB.");
            AssertEqual("#FF000000", pretoSemHashNormalizado, "ColorPicker 000000 normalizado");
            Assert(ColorPickerWindow.TryNormalizeHexColor("#FF112233", out string corNormalizada), "ColorPicker deve aceitar #AARRGGBB.");
            AssertEqual("#FF112233", corNormalizada, "ColorPicker #FF112233 normalizado");
            Assert(ColorPickerWindow.TryNormalizeHexColor("FF112233", out string corSemHashNormalizada), "ColorPicker deve aceitar AARRGGBB.");
            AssertEqual("#FF112233", corSemHashNormalizada, "ColorPicker FF112233 normalizado");
            Assert(!ColorPickerWindow.TryNormalizeHexColor("#GG112233", out _), "ColorPicker deve rejeitar hexadecimal invalido.");

            IReadOnlyList<string> paletteColors = ColorPickerWindow.GeneratePaletteHexColors();
            Assert(paletteColors.Count >= 100, "ColorPicker paleta deve conter pelo menos 100 cores.");
            Assert(paletteColors.Contains("#FF000000"), "ColorPicker paleta deve conter preto.");
            Assert(paletteColors.Contains("#FFFFFFFF"), "ColorPicker paleta deve conter branco.");
            Assert(paletteColors.Contains("#FFFF0000"), "ColorPicker paleta deve conter vermelho.");
            Assert(paletteColors.Contains("#FF00FF00"), "ColorPicker paleta deve conter verde.");
            Assert(paletteColors.Contains("#FF0000FF"), "ColorPicker paleta deve conter azul.");
            Assert(paletteColors.Contains("#FFFFFF00"), "ColorPicker paleta deve conter amarelo.");
            Assert(paletteColors.Contains("#FF00FFFF"), "ColorPicker paleta deve conter ciano.");
            Assert(paletteColors.Contains("#FFFF00FF"), "ColorPicker paleta deve conter magenta.");
            Assert(paletteColors.Any(c => !c.EndsWith("000000", StringComparison.OrdinalIgnoreCase) &&
                                          !c.EndsWith("FFFFFF", StringComparison.OrdinalIgnoreCase) &&
                                          !c.EndsWith("FF0000", StringComparison.OrdinalIgnoreCase) &&
                                          !c.EndsWith("00FF00", StringComparison.OrdinalIgnoreCase) &&
                                          !c.EndsWith("0000FF", StringComparison.OrdinalIgnoreCase)),
                "ColorPicker paleta deve conter tons intermediarios.");
            IReadOnlyList<ColorPickerWindow.ColorSwatch> swatches = ColorPickerWindow.GenerateHexPaletteSwatches();
            Assert(swatches.Count >= 100, "ColorPicker swatches hexagonais devem conter pelo menos 100 cores.");
            Assert(swatches.All(s => s.Points.Count == 6), "ColorPicker swatches devem usar seis pontos.");
            IReadOnlyList<int> rowCounts = ColorPickerWindow.GenerateHexPaletteRowCounts();
            AssertEqual(13, rowCounts.Count, "ColorPicker linhas da paleta hexagonal.Count");
            AssertEqual(7, rowCounts[0], "ColorPicker primeira linha hexagonal");
            AssertEqual(13, rowCounts[6], "ColorPicker linha central hexagonal");
            AssertEqual(7, rowCounts[^1], "ColorPicker ultima linha hexagonal");
            Assert(rowCounts.Distinct().Count() > 1, "ColorPicker paleta deve usar contagens variaveis por linha.");
            AssertEqual(rowCounts.Sum(), swatches.Count, "ColorPicker swatches nao devem incluir escala de cinza separada.");

            ElementDefinition? definition = context.Elements.FindByKind(ElementKinds.LinhaAnotativa);

            if (definition == null)
                throw new InvalidOperationException("LinhaAnotativa deve estar registrada no catalogo.");

            Assert(definition.PropriedadesInstancia.Count > 0, "LinhaAnotativa deve registrar propriedades de instancia.");

            IReadOnlyList<InstancePropertyDescriptor> catalogProperties = context.Elements.GetInstanceProperties(typeof(LinhaAnotativaViewModel));
            AssertEqual(4, catalogProperties.Count, "LinhaAnotativa catalog properties.Count");

            var linha = new LinhaAnotativa
            {
                PosicaoX = 10,
                PosicaoY = 20,
                X2 = 100,
                Y2 = 0
            };

            AssertEqual("#FF000000", linha.CorLinha, "LinhaAnotativa.CorLinha default preta");

            if (context.ElementoFactory.CriarViewModel(linha) is not LinhaAnotativaViewModel vm)
                throw new InvalidOperationException("LinhaAnotativa deve criar LinhaAnotativaViewModel.");

            AssertBrushColor("#FF000000", vm.RenderData.Stroke, "LinhaAnotativa.RenderData.Stroke default");
            Assert(vm.RenderData.StrokeDashArray == null, "LinhaAnotativa.RenderData.StrokeDashArray default continuo");
            AssertEqual(100, vm.Comprimento, "LinhaAnotativa.Comprimento inicial");

            PropertiesViewModel properties = new(new[] { vm }, context.EditarPropriedades, context.Settings);
            AssertPropertyRow(properties, "Nome", "Nome", true);
            AssertPropertyRow(properties, "Comprimento", "Comprimento", true);
            AssertPropertyRow(properties, "CorLinha", "Cor da linha", false);
            AssertPropertyRow(properties, "EspessuraLinha", "Espessura da linha", false);
            PropertyDescriptorViewModel corRow = GetPropertyRow(properties, "CorLinha");
            Assert(corRow.IsColor, "PropertyDescriptorViewModel.CorLinha.IsColor");
            Assert(corRow.EscolherCorCommand != null, "PropertyDescriptorViewModel.CorLinha.EscolherCorCommand");
            AssertBrushColor("#FF000000", corRow.ColorBrush, "PropertyDescriptorViewModel.CorLinha.ColorBrush");
            Assert(properties.ExibirSeletorTipo, "Painel da LinhaAnotativa deve exibir seletor de tipo.");
            Assert(properties.PodeAbrirPropriedadesTipo, "Painel da LinhaAnotativa deve permitir abrir propriedades de tipo.");
            Assert(properties.Tipo is TipoLinhaAnotativa, "Painel da LinhaAnotativa deve expor TipoLinhaAnotativa.");
            AssertEqual(4, properties.TiposDisponiveis.Cast<object>().Count(), "Painel LinhaAnotativa.TiposDisponiveis.Count");
            Assert(!properties.Propriedades.Any(p => p.PropertyName == "X2"), "Painel da LinhaAnotativa nao deve exibir X2.");
            Assert(!properties.Propriedades.Any(p => p.PropertyName == "Y2"), "Painel da LinhaAnotativa nao deve exibir Y2.");
            Assert(!properties.Propriedades.Any(p => p.PropertyName == "Visivel"), "Painel da LinhaAnotativa nao deve exibir Visivel.");
            Assert(!properties.Propriedades.Any(p => p.PropertyName == "EstiloLinha"), "Painel da LinhaAnotativa nao deve exibir EstiloLinha como instancia.");
            var invalidColorRow = new PropertyDescriptorViewModel(
                Array.Empty<ElementoViewModel>(),
                new InstancePropertyDescriptor(typeof(LinhaAnotativaViewModel), "CorLinha", "Cor da linha", 0, isColor: true),
                typeof(string),
                false,
                true);

            invalidColorRow.Value = "cor invalida";
            Assert(invalidColorRow.IsColor, "PropertyDescriptorViewModel.Cor invalida.IsColor");
            AssertBrushColor("#FF000000", invalidColorRow.ColorBrush, "PropertyDescriptorViewModel.Cor invalida.ColorBrush fallback");

            string templateXaml = File.ReadAllText(FindProjectFile("Resources/Templates/DataTemplates.xaml"));
            AssertContains(templateXaml, "IsColor", "PropertiesTemplate.IsColor");
            AssertContains(templateXaml, "ColorBrush", "PropertiesTemplate.ColorBrush");
            AssertContains(templateXaml, "EscolherCorCommand", "PropertiesTemplate.EscolherCorCommand");
            Assert(!templateXaml.Contains("TextBlock Text=\"{Binding Valor}\"", StringComparison.OrdinalIgnoreCase),
                "PropertiesTemplate cor nao deve exibir texto hexadecimal visivel.");
            Assert(!templateXaml.Contains("<Line", StringComparison.OrdinalIgnoreCase),
                "PropertiesTemplate amostra de cor nao deve conter Line.");

            string colorPickerXaml = File.ReadAllText(FindProjectFile("Properties/ColorPickerWindow.xaml"));
            string colorPickerCode = File.ReadAllText(FindProjectFile("Properties/ColorPickerWindow.xaml.cs"));
            AssertContains(colorPickerXaml, "ColorPaletteItemsControl", "ColorPickerWindow.Paleta.ItemsControl");
            AssertContains(colorPickerXaml, "Canvas", "ColorPickerWindow.Paleta.Canvas");
            AssertContains(colorPickerXaml, "Polygon", "ColorPickerWindow.Paleta.Polygon");
            AssertContains(colorPickerXaml, "PaletteColors", "ColorPickerWindow.Paleta.Binding");
            AssertContains(colorPickerXaml, "HorizontalAlignment=\"Center\"", "ColorPickerWindow.Paleta.Center");
            Assert(!colorPickerXaml.Contains("<Border BorderBrush=\"#4A4A4A\"", StringComparison.OrdinalIgnoreCase), "ColorPicker paleta nao deve estar em caixa com borda.");
            Assert(!colorPickerCode.Contains("GrayScaleTopGap", StringComparison.OrdinalIgnoreCase), "ColorPicker nao deve gerar faixa de cinza separada.");
            Assert(!colorPickerXaml.Contains("Content=\"Preto\"", StringComparison.OrdinalIgnoreCase), "ColorPicker rapido nao deve exibir Preto.");
            Assert(!colorPickerXaml.Contains("Content=\"Vermelho\"", StringComparison.OrdinalIgnoreCase), "ColorPicker rapido nao deve exibir Vermelho.");
            Assert(!colorPickerXaml.Contains("Content=\"Verde\"", StringComparison.OrdinalIgnoreCase), "ColorPicker rapido nao deve exibir Verde.");
            Assert(!colorPickerXaml.Contains("Content=\"Azul\"", StringComparison.OrdinalIgnoreCase), "ColorPicker rapido nao deve exibir Azul.");
            Assert(!colorPickerXaml.Contains("Content=\"Cinza\"", StringComparison.OrdinalIgnoreCase), "ColorPicker rapido nao deve exibir Cinza.");
            Assert(!colorPickerXaml.Contains("Content=\"Branco\"", StringComparison.OrdinalIgnoreCase), "ColorPicker rapido nao deve exibir Branco.");

            ElementoRenderData antes = vm.RenderData;
            vm.X2 = 160;
            vm.Y2 = 120;

            AssertEqual(160, vm.X2, "LinhaAnotativa.Properties.X2");
            AssertEqual(160, vm.RenderData.PontoLocalFinal.X, "LinhaAnotativa.RenderData.X2 editado");
            AssertEqual(200, vm.Comprimento, "LinhaAnotativa.Comprimento editado");
            Assert(!ReferenceEquals(antes, vm.RenderData), "RenderData deve ser recalculado apos X2.");

            corRow.Value = "#FFFF0000";
            AssertEqual("#FFFF0000", vm.CorLinha, "LinhaAnotativa.CorLinha via propriedade vermelho");
            AssertEqual("#FFFF0000", corRow.Valor, "PropertyDescriptorViewModel.CorLinha.Valor vermelho");
            AssertBrushColor("#FFFF0000", corRow.ColorBrush, "PropertyDescriptorViewModel.CorLinha.ColorBrush vermelho");

            corRow.Value = "#FF00AAFF";
            AssertEqual("#FF00AAFF", vm.CorLinha, "LinhaAnotativa.CorLinha via propriedade");
            AssertEqual("#FF00AAFF", corRow.Valor, "PropertyDescriptorViewModel.CorLinha.Valor editado");
            AssertBrushColor("#FF00AAFF", vm.RenderData.Stroke, "LinhaAnotativa.RenderData.Stroke editado");
            AssertBrushColor("#FF00AAFF", corRow.ColorBrush, "PropertyDescriptorViewModel.CorLinha.ColorBrush editado");

            context.Commands.Undo();

            AssertEqual("#FFFF0000", vm.CorLinha, "LinhaAnotativa.CorLinha apos undo");
            AssertBrushColor("#FFFF0000", vm.RenderData.Stroke, "LinhaAnotativa.RenderData.Stroke apos undo");

            context.Commands.Redo();

            AssertEqual("#FF00AAFF", vm.CorLinha, "LinhaAnotativa.CorLinha apos redo");
            AssertBrushColor("#FF00AAFF", vm.RenderData.Stroke, "LinhaAnotativa.RenderData.Stroke apos redo");

            var linha2 = new LinhaAnotativa
            {
                PosicaoX = 20,
                PosicaoY = 30,
                X2 = 80,
                Y2 = 10
            };

            if (context.ElementoFactory.CriarViewModel(linha2) is not LinhaAnotativaViewModel vm2)
                throw new InvalidOperationException("Segunda LinhaAnotativa deve criar LinhaAnotativaViewModel.");

            PropertiesViewModel multiplasLinhas = new(new[] { vm, vm2 }, context.EditarPropriedades, context.Settings);
            PropertyDescriptorViewModel corMultiplas = GetPropertyRow(multiplasLinhas, "CorLinha");

            corMultiplas.Value = "#FF336699";

            AssertEqual("#FF336699", vm.CorLinha, "LinhaAnotativa multipla primeira CorLinha");
            AssertEqual("#FF336699", vm2.CorLinha, "LinhaAnotativa multipla segunda CorLinha");
            AssertEqual("#FF336699", corMultiplas.Valor, "LinhaAnotativa multipla Valor");
            Assert(!corMultiplas.Varia, "LinhaAnotativa multipla Varia deve ser false apos aplicar cor.");
            AssertBrushColor("#FF336699", vm.RenderData.Stroke, "LinhaAnotativa multipla primeira Stroke");
            AssertBrushColor("#FF336699", vm2.RenderData.Stroke, "LinhaAnotativa multipla segunda Stroke");
            AssertBrushColor("#FF336699", corMultiplas.ColorBrush, "LinhaAnotativa multipla ColorBrush");

            context.Commands.Undo();

            AssertEqual("#FF00AAFF", vm.CorLinha, "LinhaAnotativa multipla primeira CorLinha undo");
            AssertEqual("#FF000000", vm2.CorLinha, "LinhaAnotativa multipla segunda CorLinha undo");

            context.Commands.Redo();

            AssertEqual("#FF336699", vm.CorLinha, "LinhaAnotativa multipla primeira CorLinha redo");
            AssertEqual("#FF336699", vm2.CorLinha, "LinhaAnotativa multipla segunda CorLinha redo");

            vm.EspessuraLinha = 3.25;
            AssertEqual(3.25, vm.RenderData.StrokeThickness, "LinhaAnotativa.RenderData.StrokeThickness editado");

            vm.Visivel = false;
            AssertEqual(false, vm.Visivel, "LinhaAnotativa.Visivel editado");

            properties.Tipo = context.Types.TiposLinhasAnotativas.Single(t => t.EstiloLinha == "Traço ponto");

            AssertEqual("Traço ponto", vm.EstiloLinha, "LinhaAnotativa.EstiloLinha editado");
            AssertEqual(4, vm.RenderData.StrokeDashArray?.Count ?? 0, "LinhaAnotativa.StrokeDashArray.Count");
            AssertEqual(8.0, vm.RenderData.StrokeDashArray![0], "LinhaAnotativa.StrokeDashArray[0]");
            AssertEqual(3.0, vm.RenderData.StrokeDashArray![1], "LinhaAnotativa.StrokeDashArray[1]");
            AssertEqual(2.0, vm.RenderData.StrokeDashArray![2], "LinhaAnotativa.StrokeDashArray[2]");
            AssertEqual(3.0, vm.RenderData.StrokeDashArray![3], "LinhaAnotativa.StrokeDashArray[3]");

            context.Document.AdicionarElemento(linha);
            SceneHitResult? hit = context.SceneQueries.HitTest(new Point(90, 80), 6);

            Assert(hit?.Elemento is LinhaAnotativaViewModel, "HitTest deve encontrar LinhaAnotativa proxima ao segmento.");

            EditorContext previewContext = CreateContextWithViewport();
            var previewLinha = new LinhaAnotativa
            {
                PosicaoX = 0,
                PosicaoY = 0,
                X2 = 100,
                Y2 = 0
            };

            if (previewContext.ElementoFactory.CriarViewModel(previewLinha) is not LinhaAnotativaViewModel preview)
                throw new InvalidOperationException("LinhaAnotativa preview deve criar ViewModel.");

            preview.IsPreview = true;
            previewContext.Scene.Elementos.Add(preview);

            Assert(previewContext.SceneQueries.HitTest(new Point(50, 0), 6) == null, "HitTest nao deve retornar preview de LinhaAnotativa.");
        }

        private static void AssertDescriptor(
            InstancePropertyDescriptor descriptor,
            string propertyName,
            string displayName,
            int order,
            bool isEditable)
        {
            AssertEqual(propertyName, descriptor.PropertyName, $"{propertyName}.PropertyName");
            AssertEqual(displayName, descriptor.DisplayName, $"{propertyName}.DisplayName");
            AssertEqual(order, descriptor.Order, $"{propertyName}.Order");
            AssertEqual(isEditable, descriptor.IsEditable, $"{propertyName}.IsEditable");
            AssertEqual(typeof(LinhaAnotativaViewModel), descriptor.OwnerType, $"{propertyName}.OwnerType");
        }

        private static void AssertPropertyRow(
            PropertiesViewModel properties,
            string propertyName,
            string displayName,
            bool isReadOnly)
        {
            PropertyDescriptorViewModel row = GetPropertyRow(properties, propertyName);

            AssertEqual(displayName, row.DisplayName, $"{propertyName}.DisplayName");
            AssertEqual(isReadOnly, row.IsReadOnly, $"{propertyName}.IsReadOnly");
            AssertEqual(!isReadOnly, row.IsEditable, $"{propertyName}.IsEditable");
        }

        private static void AssertBrushColor(string expected, Brush brush, string name)
        {
            if (brush is not SolidColorBrush solid)
                throw new InvalidOperationException($"{name}: brush nao e SolidColorBrush.");

            AssertEqual(expected, solid.Color.ToString(), name);
        }

        private static void CatalogoPreservaPropriedadesNaoEditaveis()
        {
            EditorContext context = new();
            Type[] viewModelTypes =
            {
                typeof(CaboViewModel),
                typeof(CargaViewModel),
                typeof(GeradorViewModel),
                typeof(SinViewModel),
                typeof(TransformadorViewModel),
                typeof(BarraViewModel)
            };

            foreach (Type viewModelType in viewModelTypes)
            {
                InstancePropertyDescriptor nome = GetInstanceProperty(context, viewModelType, "Nome");
                Assert(!nome.IsEditable, $"{viewModelType.Name}.Nome deve ser nao editavel.");
            }

            Assert(!GetInstanceProperty(context, typeof(CaboViewModel), "BarraOrigem").IsEditable, "Cabo.BarraOrigem deve ser nao editavel.");
            Assert(!GetInstanceProperty(context, typeof(CaboViewModel), "BarraDestino").IsEditable, "Cabo.BarraDestino deve ser nao editavel.");
        }

        private static void CatalogoPreservaEdicaoMista()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-MIXED", 120, 40);
            Gerador generator = CreateGenerator("GERADOR-MIXED", 500, 0.95);

            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(generator);

            var vms = new[] { GetVm(context, load), GetVm(context, generator) };
            Assert(context.Elements.CanEditAcrossMixedTypes(vms, "TensaoLinha"), "TensaoLinha deve permitir edicao mista.");
            Assert(!context.Elements.CanEditAcrossMixedTypes(vms, "Nome"), "Nome nao deve permitir edicao mista.");
        }

        private static void UnitFormatterConverteKvEVolt()
        {
            AssertEqual(13800.0, UnitFormatter.Convert(13.8, UnitKind.VoltageKV, UnitKind.VoltageVolt), "13.8 kV em V");
            AssertEqual(13.8, UnitFormatter.Convert(13800.0, UnitKind.VoltageVolt, UnitKind.VoltageKV), "13800 V em kV");
        }

        private static void UnitFormatterConverteMetroEKilometro()
        {
            AssertEqual(2.0, UnitFormatter.Convert(2000.0, UnitKind.LengthMeter, UnitKind.LengthKilometer), "2000 m em km");
            AssertEqual(2000.0, UnitFormatter.Convert(2.0, UnitKind.LengthKilometer, UnitKind.LengthMeter), "2 km em m");
        }

        private static void PropertiesViewModelDefaultMantemKv()
        {
            EditorContext context = CreateContextWithViewport();
            Transformador transformador = CreateTransformador("TR-UNITS-DEFAULT");
            context.Document.AdicionarElemento(transformador);

            PropertyDescriptorViewModel propriedade = GetPropertyRow(
                new PropertiesViewModel(new[] { GetVm(context, transformador) }, context.EditarPropriedades, context.Settings),
                "TensaoPrimarioKV");

            AssertEqual(UnitKind.VoltageKV, propriedade.BaseUnit, "BaseUnit default");
            AssertEqual(UnitKind.VoltageKV, propriedade.DisplayUnit, "DisplayUnit default");
            AssertEqual("kV", propriedade.UnitSymbol, "UnitSymbol default");
        }

        private static void PropertiesViewModelExibeTensaoEmVolt()
        {
            EditorContext context = CreateContextWithViewport();
            context.Settings.Units.Voltage = UnitKind.VoltageVolt;
            Transformador transformador = CreateTransformador("TR-UNITS-V");
            context.Document.AdicionarElemento(transformador);

            PropertyDescriptorViewModel propriedade = GetPropertyRow(
                new PropertiesViewModel(new[] { GetVm(context, transformador) }, context.EditarPropriedades, context.Settings),
                "TensaoPrimarioKV");

            AssertEqual(UnitKind.VoltageKV, propriedade.BaseUnit, "BaseUnit tensao em V");
            AssertEqual(UnitKind.VoltageVolt, propriedade.DisplayUnit, "DisplayUnit tensao em V");
            Assert(propriedade.Valor.Contains("13.800") || propriedade.Valor.Contains("13,800"), "Tensao deve ser exibida convertida para V.");
            Assert(propriedade.Valor.EndsWith(" V", StringComparison.Ordinal), "Tensao deve exibir simbolo V.");
        }

        private static void PropertiesViewModelEditaTensaoEmVoltESalvaKv()
        {
            EditorContext context = CreateContextWithViewport();
            context.Settings.Units.Voltage = UnitKind.VoltageVolt;
            Transformador transformador = CreateTransformador("TR-UNITS-EDIT-V");
            context.Document.AdicionarElemento(transformador);

            PropertyDescriptorViewModel propriedade = GetPropertyRow(
                new PropertiesViewModel(new[] { GetVm(context, transformador) }, context.EditarPropriedades, context.Settings),
                "TensaoPrimarioKV");

            propriedade.Valor = "13800 V";

            AssertEqual(13.8, transformador.TensaoPrimarioKV, "TensaoPrimarioKV apos edicao em V");
        }

        private static void PropertiesViewModelEditaComprimentoEmKmESalvaMetro()
        {
            EditorContext context = CreateContextWithViewport();
            context.Settings.Units.Length = UnitKind.LengthKilometer;
            Barra barra = CreateBar("BARRA-UNITS-KM");
            context.Document.AdicionarElemento(barra);

            PropertyDescriptorViewModel propriedade = GetPropertyRow(
                new PropertiesViewModel(new[] { GetVm(context, barra) }, context.EditarPropriedades, context.Settings),
                "Altura");

            propriedade.Valor = "2 km";

            AssertEqual(2000.0, barra.Altura, "Altura apos edicao em km");
        }

        private static void UnitsSettingsViewModelCopiaDefaults()
        {
            var settings = new UnitDisplaySettings();
            var viewModel = new UnitsSettingsViewModel(settings);

            AssertEqual(UnitKind.LengthMeter, viewModel.Length, "Length default");
            AssertEqual(UnitKind.VoltageKV, viewModel.Voltage, "Voltage default");
            AssertEqual(UnitKind.CurrentAmpere, viewModel.Current, "Current default");
            AssertEqual(UnitKind.ActivePowerKW, viewModel.ActivePower, "ActivePower default");
            AssertEqual(UnitKind.ReactivePowerKVAr, viewModel.ReactivePower, "ReactivePower default");
            AssertEqual(UnitKind.ApparentPowerKVA, viewModel.ApparentPower, "ApparentPower default");
            AssertEqual(UnitKind.Percent, viewModel.Percent, "Percent default");
        }

        private static void UnitsSettingsViewModelApplyToAlteraSettings()
        {
            var settings = new UnitDisplaySettings();
            var viewModel = new UnitsSettingsViewModel(settings)
            {
                Length = UnitKind.LengthKilometer,
                ActivePower = UnitKind.ActivePowerMW,
                ReactivePower = UnitKind.ReactivePowerMVAr,
                ApparentPower = UnitKind.ApparentPowerMVA
            };

            viewModel.ApplyTo(settings);

            AssertEqual(UnitKind.LengthKilometer, settings.Length, "Length aplicado");
            AssertEqual(UnitKind.ActivePowerMW, settings.ActivePower, "ActivePower aplicado");
            AssertEqual(UnitKind.ReactivePowerMVAr, settings.ReactivePower, "ReactivePower aplicado");
            AssertEqual(UnitKind.ApparentPowerMVA, settings.ApparentPower, "ApparentPower aplicado");
        }

        private static void UnitsSettingsViewModelAplicaTensaoVolt()
        {
            var settings = new UnitDisplaySettings();
            var viewModel = new UnitsSettingsViewModel(settings)
            {
                Voltage = UnitKind.VoltageVolt
            };

            viewModel.ApplyTo(settings);

            AssertEqual(UnitKind.VoltageVolt, settings.Voltage, "Voltage aplicado em V");
        }

        private static void UnitsSettingsViewModelToUnitDisplaySettingsCopiaSelecoes()
        {
            var viewModel = new UnitsSettingsViewModel(new UnitDisplaySettings())
            {
                Length = UnitKind.LengthKilometer,
                Voltage = UnitKind.VoltageVolt,
                ActivePower = UnitKind.ActivePowerMW,
                ReactivePower = UnitKind.ReactivePowerMVAr,
                ApparentPower = UnitKind.ApparentPowerMVA
            };

            UnitDisplaySettings settings = viewModel.ToUnitDisplaySettings();

            AssertEqual(UnitKind.LengthKilometer, settings.Length, "ToUnitDisplaySettings.Length");
            AssertEqual(UnitKind.VoltageVolt, settings.Voltage, "ToUnitDisplaySettings.Voltage");
            AssertEqual(UnitKind.ActivePowerMW, settings.ActivePower, "ToUnitDisplaySettings.ActivePower");
            AssertEqual(UnitKind.ReactivePowerMVAr, settings.ReactivePower, "ToUnitDisplaySettings.ReactivePower");
            AssertEqual(UnitKind.ApparentPowerMVA, settings.ApparentPower, "ToUnitDisplaySettings.ApparentPower");
        }

        private static void AlterarUnidadesProjetoUseCaseAplicaVoltageVolt()
        {
            var settings = new EditorSettings();
            var novasUnidades = new UnitDisplaySettings
            {
                Voltage = UnitKind.VoltageVolt
            };
            var useCase = new AlterarUnidadesProjetoUseCase(settings, () => { });

            useCase.Executar(novasUnidades);

            AssertEqual(UnitKind.VoltageVolt, settings.Units.Voltage, "UseCase Voltage");
        }

        private static void AlterarUnidadesProjetoUseCaseChamaRefresh()
        {
            var settings = new EditorSettings();
            bool refreshChamado = false;
            var useCase = new AlterarUnidadesProjetoUseCase(settings, () => refreshChamado = true);

            useCase.Executar(new UnitDisplaySettings());

            Assert(refreshChamado, "UseCase deve chamar refreshProperties.");
        }

        private static void AlterarUnidadesProjetoUseCasePreservaCopiaCompleta()
        {
            var settings = new EditorSettings();
            var unidades = new UnitDisplaySettings
            {
                Voltage = UnitKind.VoltageVolt
            };
            var useCase = new AlterarUnidadesProjetoUseCase(settings, () => { });

            useCase.Executar(unidades);

            AssertEqual(UnitKind.VoltageVolt, settings.Units.Voltage, "UseCase preserva Voltage");
            AssertEqual(UnitKind.LengthMeter, settings.Units.Length, "UseCase preserva Length default");
            AssertEqual(UnitKind.CurrentAmpere, settings.Units.Current, "UseCase preserva Current default");
            AssertEqual(UnitKind.ActivePowerKW, settings.Units.ActivePower, "UseCase preserva ActivePower default");
            AssertEqual(UnitKind.ReactivePowerKVAr, settings.Units.ReactivePower, "UseCase preserva ReactivePower default");
            AssertEqual(UnitKind.ApparentPowerKVA, settings.Units.ApparentPower, "UseCase preserva ApparentPower default");
            AssertEqual(UnitKind.Percent, settings.Units.Percent, "UseCase preserva Percent default");
        }

        private static void ExecutarSimulacaoUseCaseChamaPipeline()
        {
            var pipeline = new FakeSimulationPipeline();
            var dialogs = new FakeDialogService();
            var useCase = CriarExecutarSimulacaoUseCase(pipeline, dialogs);

            useCase.ExecutarFluxoDeCorrenteAsync().GetAwaiter().GetResult();

            AssertEqual(1, pipeline.ExecutarFluxoDeCorrenteChamadas, "Chamadas ao pipeline");
        }

        private static void ExecutarSimulacaoUseCaseAtualizaResultado()
        {
            var resultado = new SimulationResultDto
            {
                Sucesso = true,
                Mensagem = "ok",
                Script = "script"
            };
            var pipeline = new FakeSimulationPipeline(resultado);
            var useCase = CriarExecutarSimulacaoUseCase(pipeline, new FakeDialogService());

            useCase.ExecutarFluxoDeCorrenteAsync().GetAwaiter().GetResult();

            Assert(ReferenceEquals(resultado, useCase.Resultado), "Resultado deve ser o retornado pelo pipeline.");
        }

        private static void ExecutarSimulacaoUseCaseMostraMensagem()
        {
            var dialogs = new FakeDialogService();
            var useCase = CriarExecutarSimulacaoUseCase(new FakeSimulationPipeline(), dialogs);

            useCase.ExecutarFluxoDeCorrenteAsync().GetAwaiter().GetResult();

            AssertEqual(1, dialogs.ShowMessageChamadas, "Show message chamadas");
            AssertEqual("Fluxo de corrente", dialogs.LastSimulationMessage?.Title, "Titulo da mensagem");
        }

        private static void FluxoDeCorrenteApplicationDelegaParaUseCase()
        {
            var pipeline = new FakeSimulationPipeline();
            var useCase = CriarExecutarSimulacaoUseCase(pipeline, new FakeDialogService());
            var app = new FluxoDeCorrenteApplication(useCase);

            app.ExecutarAsync().GetAwaiter().GetResult();

            AssertEqual(1, pipeline.ExecutarFluxoDeCorrenteChamadas, "Wrapper deve chamar use case");
            Assert(ReferenceEquals(useCase.Resultado, app.Resultado), "Wrapper deve expor Resultado do use case.");
        }

        private static void ExecutarSimulacaoUseCaseMostraWarningEmExcecao()
        {
            var pipeline = new FakeSimulationPipeline
            {
                ExceptionToThrow = new InvalidOperationException("falha")
            };
            var dialogs = new FakeDialogService();
            var useCase = CriarExecutarSimulacaoUseCase(pipeline, dialogs);

            useCase.ExecutarFluxoDeCorrenteAsync().GetAwaiter().GetResult();

            AssertEqual(1, dialogs.WarningChamadas, "Warning chamadas");
            AssertEqual("Fluxo de corrente", dialogs.LastWarningTitle, "Warning titulo");
            AssertEqual("falha", dialogs.LastWarningMessage, "Warning mensagem");
        }

        private static void ExecutarSimulacaoUseCaseSemOptionsNaoConfirmaExportacao()
        {
            var dialogs = new FakeDialogService();
            var useCase = CriarExecutarSimulacaoUseCase(new FakeSimulationPipeline(), dialogs);

            useCase.ExecutarFluxoDeCorrenteAsync().GetAwaiter().GetResult();

            AssertEqual(0, dialogs.ConfirmChamadas, "Sem options nao deve confirmar exportacao");
        }

        private static void NovoProjetoUseCaseChamaNovo()
        {
            var projects = new FakeProjectPersistenceService();
            var useCase = new NovoProjetoUseCase(projects);

            useCase.Executar();

            AssertEqual(1, projects.NovoChamadas, "Novo chamadas");
        }

        private static void AbrirProjetoUseCaseChamaAbrirComDialogo()
        {
            var projects = new FakeProjectPersistenceService();
            var useCase = new AbrirProjetoUseCase(projects);

            useCase.ExecutarComDialogo();

            AssertEqual(1, projects.AbrirComDialogoChamadas, "AbrirComDialogo chamadas");
        }

        private static void AbrirProjetoUseCaseChamaAbrirPath()
        {
            var projects = new FakeProjectPersistenceService();
            var useCase = new AbrirProjetoUseCase(projects);

            useCase.Executar("teste.araci");

            AssertEqual(1, projects.AbrirChamadas, "Abrir chamadas");
            AssertEqual("teste.araci", projects.LastAbrirPath, "Abrir path");
        }

        private static void SalvarProjetoUseCaseChamaSalvarComDialogo()
        {
            var projects = new FakeProjectPersistenceService();
            var useCase = new SalvarProjetoUseCase(projects);

            useCase.ExecutarComDialogo();

            AssertEqual(1, projects.SalvarComDialogoChamadas, "SalvarComDialogo chamadas");
        }

        private static void SalvarProjetoUseCaseChamaSalvarPath()
        {
            var projects = new FakeProjectPersistenceService();
            var useCase = new SalvarProjetoUseCase(projects);

            useCase.Executar("teste.araci");

            AssertEqual(1, projects.SalvarChamadas, "Salvar chamadas");
            AssertEqual("teste.araci", projects.LastSalvarPath, "Salvar path");
        }

        private static void EditorContextExpoeUseCasesDeProjeto()
        {
            var context = new EditorContext();

            Assert(context.NovoProjeto != null, "EditorContext.NovoProjeto deve existir.");
            Assert(context.AbrirProjeto != null, "EditorContext.AbrirProjeto deve existir.");
            Assert(context.SalvarProjeto != null, "EditorContext.SalvarProjeto deve existir.");
        }

        private static void NovoProjetoUseCaseRealResetaUnitsDefault()
        {
            var context = new EditorContext();
            context.Settings.Units.Voltage = UnitKind.VoltageVolt;
            context.Settings.Units.Length = UnitKind.LengthKilometer;

            context.NovoProjeto.Executar();

            AssertEqual(UnitKind.VoltageKV, context.Settings.Units.Voltage, "NovoProjetoUseCase Voltage default");
            AssertEqual(UnitKind.LengthMeter, context.Settings.Units.Length, "NovoProjetoUseCase Length default");
        }

        private static void SalvarAbrirViaUseCasePreservaUnits()
        {
            string path = CreateTempProjectPath();

            try
            {
                var source = new EditorContext();
                source.Settings.Units.Voltage = UnitKind.VoltageVolt;
                source.SalvarProjeto.Executar(path);

                var target = new EditorContext();
                target.AbrirProjeto.Executar(path);

                AssertEqual(UnitKind.VoltageVolt, target.Settings.Units.Voltage, "Voltage via use cases");
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void AtualizarPropriedadesSelecionadasUseCasePreservaSelecao()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-REFRESH-SELECAO", 120, 40);
            context.Document.AdicionarElemento(load);
            ElementoViewModel vm = GetVm(context, load);

            context.Selection.Selecionar(vm);
            context.AtualizarPropriedadesSelecionadas.Executar();

            AssertEqual(1, context.Selection.Selecionados.Count, "Selecionados apos refresh");
            Assert(ReferenceEquals(vm, context.Selection.Selecionados[0]), "Refresh deve preservar item selecionado.");
            Assert(vm.IsSelecionado, "Refresh deve preservar estado visual selecionado.");
        }

        private static void SelecionarElementosUseCaseSelecionaElemento()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-USECASE-SEL", 120, 40);
            context.Document.AdicionarElemento(load);
            ElementoViewModel vm = GetVm(context, load);

            context.SelecionarElementos.Selecionar(vm);

            AssertEqual(1, context.Selection.Selecionados.Count, "Selecionados.Count");
            Assert(ReferenceEquals(vm, context.Editor.ElementoSelecionado), "Editor.ElementoSelecionado deve receber o VM.");
            Assert(vm.IsSelecionado, "VM deve ficar selecionado.");

            var properties = new PropertiesViewModel(new[] { vm }, context.EditarPropriedades, context.Settings);
            AssertContains(properties.Titulo, "Carga", "Carga PropertiesViewModel.Titulo");
            Assert(!properties.Titulo.Contains("Linha", StringComparison.OrdinalIgnoreCase), "Titulo da Carga nao deve ser afetado pela LinhaAnotativa.");
        }

        private static void SelecionarLinhaAnotativaUsaPainelGenerico()
        {
            EditorContext context = CreateContextWithViewport();
            var linha = new LinhaAnotativa
            {
                Nome = "Linha Painel",
                PosicaoX = 0,
                PosicaoY = 0,
                X2 = 100,
                Y2 = 25
            };

            context.Document.AdicionarElemento(linha);

            if (GetVm(context, linha) is not LinhaAnotativaViewModel vm)
                throw new InvalidOperationException("ViewModel de LinhaAnotativa nao encontrada.");

            context.SelecionarElementos.Selecionar(vm);

            AssertEqual(1, context.Selection.Selecionados.Count, "Linha selecionados.Count");
            Assert(vm.IsSelecionado, "Linha VM deve ficar selecionada.");
            Assert(context.Editor.ElementoSelecionado is PropertiesViewModel, "LinhaAnotativa deve usar PropertiesViewModel.");

            var properties = (PropertiesViewModel)context.Editor.ElementoSelecionado!;

            AssertContains(properties.Titulo, "1 Linha selecionada", "LinhaAnotativa PropertiesViewModel.Titulo singular");
            Assert(!properties.Titulo.Contains("LinhaAnotativa", StringComparison.OrdinalIgnoreCase), "Titulo da LinhaAnotativa nao deve exibir nome tecnico.");
            AssertPropertyRow(properties, "Nome", "Nome", true);
            AssertPropertyRow(properties, "Comprimento", "Comprimento", true);
            AssertPropertyRow(properties, "CorLinha", "Cor da linha", false);
            AssertPropertyRow(properties, "EspessuraLinha", "Espessura da linha", false);
            Assert(properties.ExibirSeletorTipo, "LinhaAnotativa painel generico deve exibir seletor de tipo.");
            Assert(properties.PodeAbrirPropriedadesTipo, "LinhaAnotativa painel generico deve permitir propriedades de tipo.");
            Assert(properties.Tipo is TipoLinhaAnotativa, "LinhaAnotativa painel generico deve expor TipoLinhaAnotativa.");
            Assert(!properties.Propriedades.Any(p => p.PropertyName == "X2"), "LinhaAnotativa painel generico nao deve exibir X2.");
            Assert(!properties.Propriedades.Any(p => p.PropertyName == "Y2"), "LinhaAnotativa painel generico nao deve exibir Y2.");
            Assert(!properties.Propriedades.Any(p => p.PropertyName == "Visivel"), "LinhaAnotativa painel generico nao deve exibir Visivel.");
            Assert(!properties.Propriedades.Any(p => p.PropertyName == "EstiloLinha"), "LinhaAnotativa painel generico nao deve exibir EstiloLinha.");

            var linha2 = new LinhaAnotativa
            {
                Nome = "Linha Painel 2",
                PosicaoX = 10,
                PosicaoY = 10,
                X2 = 40,
                Y2 = 0
            };

            context.Document.AdicionarElemento(linha2);

            if (GetVm(context, linha2) is not LinhaAnotativaViewModel vm2)
                throw new InvalidOperationException("Segunda ViewModel de LinhaAnotativa nao encontrada.");

            var multiplas = new PropertiesViewModel(new[] { vm, vm2 }, context.EditarPropriedades, context.Settings);

            AssertContains(multiplas.Titulo, "2 Linhas selecionadas", "LinhaAnotativa PropertiesViewModel.Titulo plural");
            Assert(!multiplas.Titulo.Contains("LinhaAnotativas", StringComparison.OrdinalIgnoreCase), "Titulo plural da LinhaAnotativa nao deve exibir plural tecnico.");
            Assert(!multiplas.Titulo.Contains("LinhaAnotativa", StringComparison.OrdinalIgnoreCase), "Titulo plural da LinhaAnotativa nao deve exibir nome tecnico.");
        }

        private static void SelecionarElementosUseCaseLimpaSelecao()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-USECASE-LIMPAR", 120, 40);
            context.Document.AdicionarElemento(load);
            ElementoViewModel vm = GetVm(context, load);

            context.SelecionarElementos.Selecionar(vm);
            context.SelecionarElementos.Limpar();

            AssertEqual(0, context.Selection.Selecionados.Count, "Selecionados.Count apos limpar");
            Assert(context.Editor.ElementoSelecionado == null, "Editor.ElementoSelecionado deve ficar null.");
            Assert(!vm.IsSelecionado, "VM deve sair da selecao.");
        }

        private static void SelecionarElementosUseCaseCriaPainelPropriedadesMultiplaSelecao()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load1 = CreateLoad("CARGA-USECASE-MULTI-1", 120, 40);
            Carga load2 = CreateLoad("CARGA-USECASE-MULTI-2", 130, 45);
            context.Document.AdicionarElemento(load1);
            context.Document.AdicionarElemento(load2);
            ElementoViewModel vm1 = GetVm(context, load1);
            ElementoViewModel vm2 = GetVm(context, load2);

            context.SelecionarElementos.Selecionar(new[] { vm1, vm2 });

            AssertEqual(2, context.Selection.Selecionados.Count, "Selecionados.Count multiplo");
            Assert(context.Editor.ElementoSelecionado is PropertiesViewModel, "Selecao multipla deve criar PropertiesViewModel.");
        }

        private static void EditorContextExpoeUseCasesDeSelecao()
        {
            var context = new EditorContext();

            Assert(context.SelecionarElementos != null, "EditorContext.SelecionarElementos deve existir.");
            Assert(context.AtualizarPropriedadesSelecionadas != null, "EditorContext.AtualizarPropriedadesSelecionadas deve existir.");
        }

        private static void EditorContextRefreshPropertiesPreservaSelecao()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-CONTEXT-REFRESH", 120, 40);
            context.Document.AdicionarElemento(load);
            ElementoViewModel vm = GetVm(context, load);

            context.SelecionarElementos.Selecionar(vm);
            context.RefreshProperties();

            AssertEqual(1, context.Selection.Selecionados.Count, "Selecionados.Count apos EditorContext.RefreshProperties");
            Assert(ReferenceEquals(vm, context.Selection.Selecionados[0]), "EditorContext.RefreshProperties deve preservar selecionado.");
        }

        private static void UnitValueConverterUsaSettingsEmRuntime()
        {
            UnitValueConverter.CurrentUnits = new UnitDisplaySettings
            {
                Voltage = UnitKind.VoltageVolt
            };

            var converter = new UnitValueConverter();
            string text = converter.Convert(13.8, typeof(string), "VoltageKV", System.Globalization.CultureInfo.CurrentCulture)?.ToString() ?? string.Empty;

            Assert(text.Contains("13.800") || text.Contains("13,800"), "Converter deve exibir tensao em V.");
            Assert(text.EndsWith(" V", StringComparison.Ordinal), "Converter deve usar simbolo V.");
            UnitValueConverter.CurrentUnits = new UnitDisplaySettings();
        }

        private static void UnitValueConverterConverteEdicaoParaUnidadeBase()
        {
            UnitValueConverter.CurrentUnits = new UnitDisplaySettings
            {
                Voltage = UnitKind.VoltageVolt
            };

            var converter = new UnitValueConverter();
            object? value = converter.ConvertBack("13800 V", typeof(double), "VoltageKV", System.Globalization.CultureInfo.CurrentCulture);

            Assert(value is double, "ConvertBack deve retornar double.");
            AssertEqual(13.8, (double)value!, "ConvertBack deve salvar kV.");
            UnitValueConverter.CurrentUnits = new UnitDisplaySettings();
        }

        private static void PersistenciaSalvaUnitsNoJson()
        {
            string path = CreateTempProjectPath();

            try
            {
                var context = new EditorContext();
                context.Settings.Units.Voltage = UnitKind.VoltageVolt;
                context.Settings.Units.Length = UnitKind.LengthKilometer;

                context.Projects.Salvar(path);
                string json = File.ReadAllText(path);

                AssertContains(json, "\"Units\"", "JSON Units");
                AssertContains(json, "\"Voltage\": \"VoltageVolt\"", "JSON Voltage");
                AssertContains(json, "\"Length\": \"LengthKilometer\"", "JSON Length");
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void PersistenciaReabreVoltageEmVolt()
        {
            string path = CreateTempProjectPath();

            try
            {
                var source = new EditorContext();
                source.Settings.Units.Voltage = UnitKind.VoltageVolt;
                source.Projects.Salvar(path);

                var target = new EditorContext();
                target.Projects.Abrir(path);

                AssertEqual(UnitKind.VoltageVolt, target.Settings.Units.Voltage, "Voltage apos abrir");
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void PersistenciaReabreLengthEmKm()
        {
            string path = CreateTempProjectPath();

            try
            {
                var source = new EditorContext();
                source.Settings.Units.Length = UnitKind.LengthKilometer;
                source.Projects.Salvar(path);

                var target = new EditorContext();
                target.Projects.Abrir(path);

                AssertEqual(UnitKind.LengthKilometer, target.Settings.Units.Length, "Length apos abrir");
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void NovoProjetoResetaUnitsDefault()
        {
            var context = new EditorContext();
            context.Settings.Units.Voltage = UnitKind.VoltageVolt;
            context.Settings.Units.Length = UnitKind.LengthKilometer;

            context.Projects.Novo();

            AssertEqual(UnitKind.VoltageKV, context.Settings.Units.Voltage, "Voltage apos Novo");
            AssertEqual(UnitKind.LengthMeter, context.Settings.Units.Length, "Length apos Novo");
        }

        private static void ArquivoAntigoSemUnitsAbreDefaults()
        {
            string path = CreateTempProjectPath();

            try
            {
                File.WriteAllText(path, "{\"Version\":1,\"ProjectName\":\"Antigo\",\"Elements\":[]}");

                var context = new EditorContext();
                context.Settings.Units.Voltage = UnitKind.VoltageVolt;
                context.Settings.Units.Length = UnitKind.LengthKilometer;
                context.Projects.Abrir(path);

                AssertEqual(UnitKind.VoltageKV, context.Settings.Units.Voltage, "Voltage arquivo antigo");
                AssertEqual(UnitKind.LengthMeter, context.Settings.Units.Length, "Length arquivo antigo");
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void ArquivoComUnitsInvalidoUsaFallback()
        {
            string path = CreateTempProjectPath();

            try
            {
                File.WriteAllText(
                    path,
                    "{\"Version\":1,\"ProjectName\":\"Invalido\",\"Units\":{\"Voltage\":\"LengthMeter\",\"Length\":\"NaoExiste\"},\"Elements\":[]}");

                var context = new EditorContext();
                context.Settings.Units.Voltage = UnitKind.VoltageVolt;
                context.Settings.Units.Length = UnitKind.LengthKilometer;
                context.Projects.Abrir(path);

                AssertEqual(UnitKind.VoltageKV, context.Settings.Units.Voltage, "Voltage invalido fallback");
                AssertEqual(UnitKind.LengthMeter, context.Settings.Units.Length, "Length invalido fallback");
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void UnitsPersistidasNaoAlteramDtoEletrico()
        {
            string path = CreateTempProjectPath();

            try
            {
                AraciDocument document = CreateBranchDocument();
                CircuitDto before = new CircuitBuilder(new ParameterReader(document)).Build();
                var source = new EditorContext();
                source.Settings.Units.Voltage = UnitKind.VoltageVolt;
                source.Settings.Units.Length = UnitKind.LengthKilometer;

                foreach (Elemento elemento in document.Elementos)
                    source.Document.AdicionarElemento(elemento);

                source.Projects.Salvar(path);

                var target = new EditorContext();
                target.Projects.Abrir(path);
                CircuitDto after = new CircuitBuilder(new ParameterReader(target.Document)).Build();

                AssertEqual(before.Loads.Count, after.Loads.Count, "Loads.Count apos units");
                AssertEqual(before.Lines.Count, after.Lines.Count, "Lines.Count apos units");

                if (before.Loads.Count > 0)
                {
                    AssertEqual(before.Loads[0].Tensao, after.Loads[0].Tensao, "Load.Tensao apos units");
                    AssertEqual(before.Loads[0].PotenciaAtiva, after.Loads[0].PotenciaAtiva, "Load.PotenciaAtiva apos units");
                }
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void PersistenciaPreservaLinhaAnotativa()
        {
            var document = new AraciDocument();
            EditorContext source = new();
            TipoLinhaAnotativa tipoLinha = source.Types.TiposLinhasAnotativas.Single(t => t.EstiloLinha == "Traço dois pontos");
            var linha = new LinhaAnotativa
            {
                Nome = "Linha Teste",
                PosicaoX = 12.5,
                PosicaoY = -8.25,
                Rotacao = 15,
                Escala = 1.5,
                CorLinha = "#FF00AAFF",
                EspessuraLinha = 2.75,
                Visivel = false,
                X2 = -120,
                Y2 = 45,
                Tipo = tipoLinha
            };

            document.AdicionarElemento(linha);

            var serializer = new ProjectSerializer(
                source.Elements,
                new ElementoModelFactory(source.Elements),
                source.TerminalLayout,
                source.Geometry);

            ProjectFileDto dto = serializer.CreateFileDto(
                document,
                ProjectMetadataDto.CreateNew("Linha Anotativa"),
                source.Settings.Units);

            ElementDto elementDto = dto.Elements.Single();
            string json = serializer.Serialize(dto);

            AssertEqual(ElementKinds.LinhaAnotativa, elementDto.Kind, "LinhaAnotativa DTO.Kind");
            AssertEqual(ElementoDomainRole.Anotacao.ToString(), elementDto.DomainRole, "LinhaAnotativa DTO.DomainRole");
            Assert(elementDto.Type != null, "LinhaAnotativa DTO.Type nao deve ser null.");
            AssertEqual("Linha traço dois pontos", elementDto.Type!.NomeTipo, "LinhaAnotativa DTO.Type.NomeTipo");
            AssertEqual("Anotações", elementDto.Type.Familia, "LinhaAnotativa DTO.Type.Familia");
            AssertEqual("Linhas", elementDto.Type.Categoria, "LinhaAnotativa DTO.Type.Categoria");
            AssertEqual(0, elementDto.Terminals.Count, "LinhaAnotativa DTO.Terminals.Count");
            AssertEqual(0, elementDto.Vertices.Count, "LinhaAnotativa DTO.Vertices.Count");
            AssertParametroSerializado(elementDto, Elemento.PARAM_NOME);
            AssertParametroSerializado(elementDto, ElementoAnotativo.PARAM_COR_LINHA);
            AssertParametroSerializado(elementDto, ElementoAnotativo.PARAM_ESPESSURA_LINHA);
            AssertParametroSerializado(elementDto, ElementoAnotativo.PARAM_VISIVEL);
            AssertParametroSerializado(elementDto, LinhaAnotativa.PARAM_X2);
            AssertParametroSerializado(elementDto, LinhaAnotativa.PARAM_Y2);
            Assert(!elementDto.Parameters.Any(p => p.Name == TipoLinhaAnotativa.PARAM_ESTILO_LINHA), "LinhaAnotativa DTO.Parameters nao deve conter EstiloLinha.");
            AssertContains(json, "\"Kind\": \"LinhaAnotativa\"", "LinhaAnotativa JSON.Kind");
            AssertContains(json, "\"DomainRole\": \"Anotacao\"", "LinhaAnotativa JSON.DomainRole");
            AssertContains(json, "\"NomeTipo\":", "LinhaAnotativa JSON.Type.NomeTipo");
            AssertContains(json, "\"Name\": \"X2\"", "LinhaAnotativa JSON.X2");
            AssertContains(json, "\"Name\": \"Y2\"", "LinhaAnotativa JSON.Y2");
            AssertContains(json, "\"Name\": \"CorLinha\"", "LinhaAnotativa JSON.CorLinha");
            AssertContains(json, "\"Name\": \"EspessuraLinha\"", "LinhaAnotativa JSON.EspessuraLinha");
            AssertContains(json, "\"Name\": \"Visivel\"", "LinhaAnotativa JSON.Visivel");
            Assert(!json.Contains("\"Name\": \"EstiloLinha\"", StringComparison.OrdinalIgnoreCase), "LinhaAnotativa JSON nao deve salvar EstiloLinha como parametro.");

            ProjectFileDto reloadedDto = serializer.Deserialize(json);
            LinhaAnotativa reloaded = serializer.CreateElements(reloadedDto).OfType<LinhaAnotativa>().Single();

            AssertEqual(linha.Id, reloaded.Id, "LinhaAnotativa.Id apos reload");
            AssertEqual(12.5, reloaded.PosicaoX, "LinhaAnotativa.PosicaoX apos reload");
            AssertEqual(-8.25, reloaded.PosicaoY, "LinhaAnotativa.PosicaoY apos reload");
            AssertEqual(15, reloaded.Rotacao, "LinhaAnotativa.Rotacao apos reload");
            AssertEqual(1.5, reloaded.Escala, "LinhaAnotativa.Escala apos reload");
            AssertEqual("Linha Teste", reloaded.Nome, "LinhaAnotativa.Nome apos reload");
            AssertEqual("#FF00AAFF", reloaded.CorLinha, "LinhaAnotativa.CorLinha apos reload");
            AssertEqual(2.75, reloaded.EspessuraLinha, "LinhaAnotativa.EspessuraLinha apos reload");
            AssertEqual(false, reloaded.Visivel, "LinhaAnotativa.Visivel apos reload");
            AssertEqual(-120, reloaded.X2, "LinhaAnotativa.X2 apos reload");
            AssertEqual(45, reloaded.Y2, "LinhaAnotativa.Y2 apos reload");
            Assert(reloaded.Tipo is TipoLinhaAnotativa, "LinhaAnotativa.Tipo apos reload deve ser TipoLinhaAnotativa.");
            AssertEqual("Traço dois pontos", ((TipoLinhaAnotativa)reloaded.Tipo!).EstiloLinha, "LinhaAnotativa.Tipo.EstiloLinha apos reload");
            AssertEqual(ElementoDomainRole.Anotacao, reloaded.DomainRole, "LinhaAnotativa.DomainRole apos reload");
            Assert(!reloaded.ParticipaDoGrafoEletrico, "LinhaAnotativa apos reload nao deve participar do grafo eletrico.");
            Assert(reloaded is not ITerminalOwner, "LinhaAnotativa nao deve possuir terminais.");

            EditorContext target = CreateContextWithViewport();

            Assert(target.ElementoFactory.CriarViewModel(reloaded) is LinhaAnotativaViewModel, "LinhaAnotativa apos reload deve criar ViewModel.");

            target.Document.AdicionarElemento(reloaded);

            Assert(target.Viewport?.ObterViewModel(reloaded) is LinhaAnotativaViewModel, "DocumentSceneSync deve criar ViewModel da LinhaAnotativa apos reload.");
            AssertEqual(1, target.Scene.Elementos.Count, "Scene deve receber LinhaAnotativa apos reload.");
        }

        private static void AssertParametroSerializado(ElementDto dto, string nome)
        {
            Assert(dto.Parameters.Any(p => p.Name == nome), $"Parametro '{nome}' deve ser serializado.");
        }

        private static void DocumentSceneSyncCriaViewModelAoAdicionarElemento()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-SYNC-ADD", 120, 40);

            context.Document.AdicionarElemento(load);

            ElementoViewModel? vm = context.Viewport?.ObterViewModel(load);
            Assert(vm is CargaViewModel, "ObterViewModel deve retornar CargaViewModel.");
            Assert(context.Scene.Elementos.Contains(vm!), "Scene deve conter a ViewModel criada.");
        }

        private static void DocumentSceneSyncRemoveViewModelAoRemoverElemento()
        {
            EditorContext context = CreateContextWithViewport();
            Carga load = CreateLoad("CARGA-SYNC-REMOVE", 120, 40);
            context.Document.AdicionarElemento(load);
            ElementoViewModel vm = GetVm(context, load);

            context.Document.RemoverElemento(load);

            Assert(!context.Scene.Elementos.Contains(vm), "Scene nao deve conter a ViewModel removida.");
            Assert(context.Viewport?.ObterViewModel(load) == null, "ObterViewModel deve retornar null apos remocao.");
        }

        private static void DocumentSceneSyncLimpaSceneAoLimparDocument()
        {
            EditorContext context = CreateContextWithViewport();
            Gerador generator = CreateGenerator("GERADOR-SYNC-CLEAR", 500, 0.95);
            Carga load = CreateLoad("CARGA-SYNC-CLEAR", 120, 40);
            Cabo cable = CreateCable(generator, load, "CABO-SYNC-CLEAR", 1);
            cable.Vertices.Insert(1, MidPoint(cable.Vertices[0], cable.Vertices[1]));

            context.Document.AdicionarElemento(generator);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(cable);
            SelectCable(context, cable);
            context.AlignmentGuides.MostrarReferenciaVertical(10, new Rect(0, 0, 20, 20));

            Assert(context.CableVertexEdit.Handles.Count > 0, "Pre-condicao: handles de cabo devem existir.");
            Assert(context.AlignmentGuides.Linhas.Count > 0, "Pre-condicao: guias devem existir.");

            context.Document.Limpar();

            AssertEqual(0, context.Scene.Elementos.Count, "Scene.Elementos.Count");
            AssertEqual(0, context.Selection.Selecionados.Count, "Selection.Selecionados.Count");
            AssertEqual(0, context.CableVertexEdit.Handles.Count, "CableVertexEdit.Handles.Count");
            AssertEqual(0, context.AlignmentGuides.Linhas.Count, "AlignmentGuides.Linhas.Count");
        }

        private static void DocumentSceneSyncPreservaCaboViewModel()
        {
            EditorContext context = CreateContextWithViewport();
            Gerador generator = CreateGenerator("GERADOR-SYNC-CABO", 500, 0.95);
            Carga load = CreateLoad("CARGA-SYNC-CABO", 120, 40);
            Cabo cable = CreateCable(generator, load, "CABO-SYNC-CABO", 1);

            context.Document.AdicionarElemento(generator);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(cable);

            Assert(context.Viewport?.ObterViewModel(cable) is CaboViewModel, "ObterViewModel deve preservar CaboViewModel.");
        }

        private static void AssertPreviewArmazenaRotacaoAntesDeExistir<TViewModel, TModel>(
            string name,
            Func<EditorContext, TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<TViewModel, TModel>(
                context,
                () => criarPreview(context),
                obterModelo);

            Assert(controller.RotateClockwise(), $"{name}: RotateClockwise antes do preview");
            AssertEqual(90, controller.CurrentRotation, $"{name}: CurrentRotation antes do preview");

            controller.Update(new Point(240, 180));

            Assert(controller.Preview != null, $"{name}: preview deve existir apos Update.");
            AssertEqual(90, obterModelo(controller.Preview!).Rotacao, $"{name}: Modelo.Rotacao do preview");
        }

        private static void AssertPreviewExistenteRotacionaVisualmente<TViewModel, TModel>(
            string name,
            Func<EditorContext, TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<TViewModel, TModel>(
                context,
                () => criarPreview(context),
                obterModelo);

            controller.Update(new Point(240, 180));
            Assert(controller.RotateClockwise(), $"{name}: RotateClockwise com preview");

            Assert(controller.Preview != null, $"{name}: preview deve existir.");
            AssertEqual(90, controller.CurrentRotation, $"{name}: CurrentRotation");
            AssertEqual(90, controller.Preview!.Rotacao, $"{name}: Preview.Rotacao");
            AssertEqual(90, obterModelo(controller.Preview).Rotacao, $"{name}: Preview.Modelo.Rotacao");
        }

        private static void AssertUpdateDoPreviewNaoResetaRotacao<TViewModel, TModel>(
            string name,
            Func<EditorContext, TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<TViewModel, TModel>(
                context,
                () => criarPreview(context),
                obterModelo);

            controller.RotateClockwise();
            controller.RotateClockwise();
            controller.Update(new Point(240, 180));
            controller.Update(new Point(260, 190));
            controller.Update(new Point(280, 200));

            Assert(controller.Preview != null, $"{name}: preview deve existir.");
            AssertEqual(180, controller.CurrentRotation, $"{name}: CurrentRotation apos Updates");
            AssertEqual(180, obterModelo(controller.Preview!).Rotacao, $"{name}: Modelo.Rotacao apos Updates");
        }

        private static void AssertModeloRealRecebeRotacaoDoPreview<TViewModel, TModel>(
            string name,
            Func<EditorContext, TViewModel> criarPreview,
            Func<EditorContext, TModel> criarModeloReal,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<TViewModel, TModel>(
                context,
                () => criarPreview(context),
                obterModelo);

            controller.RotateClockwise();
            controller.RotateClockwise();
            controller.RotateClockwise();

            TModel real = criarModeloReal(context);
            real.Rotacao = controller.CurrentRotation;

            AssertEqual(270, controller.CurrentRotation, $"{name}: CurrentRotation");
            AssertEqual(270, real.Rotacao, $"{name}: Rotacao do modelo real");
        }

        private static void ElementoRotacionadoPersisteAposReload()
        {
            var document = new AraciDocument();
            Carga load = CreateLoad("CARGA-PERSIST-ROT", 300, 100);
            load.Rotacao = 270;
            document.AdicionarElemento(load);

            AraciDocument loaded = SaveAndLoad(document);
            Carga loadedLoad = FindById<Carga>(loaded, load.Id);

            AssertEqual(270, loadedLoad.Rotacao, "Rotacao apos reload");
        }

        private static void TerminaisMudamPosicaoEPreservamIds()
        {
            Gerador generator = CreateGenerator("GER-TERM-ROT", 1000, 0.95);
            var before = generator.Terminais
                .Select(t => (t.Id, t.Posicao))
                .ToList();

            generator.Rotacao = 90;
            generator.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            for (int i = 0; i < before.Count; i++)
                AssertEqual(before[i].Id, generator.Terminais[i].Id, $"Terminal {i}.Id");

            Assert(
                before.Any(item => generator.Terminais.Single(t => t.Id == item.Id).Posicao != item.Posicao),
                "Ao menos um terminal deve mudar de posicao apos rotacao.");
        }

        private static void CaboPreservaTerminalIdAposRotacao()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            string origemTerminalId = circuit.Cable.OrigemTerminalId;
            string destinoTerminalId = circuit.Cable.DestinoTerminalId;

            RotateSelected(circuit.Context, circuit.Generator);

            AssertEqual(origemTerminalId, circuit.Cable.OrigemTerminalId, "OrigemTerminalId");
            AssertEqual(destinoTerminalId, circuit.Cable.DestinoTerminalId, "DestinoTerminalId");
        }

        private static void CaboReancoraVisualmenteAposRotacao()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            Point before = circuit.Cable.Vertices[0];

            RotateSelected(circuit.Context, circuit.Generator);

            Terminal terminal = GetTerminal(circuit.Generator, 0);
            Assert(before != circuit.Cable.Vertices[0], "Vertice do cabo deve mudar apos rotacao.");
            AssertEqual(terminal.Posicao.X, circuit.Cable.Vertices[0].X, "Cabo.Vertices[0].X");
            AssertEqual(terminal.Posicao.Y, circuit.Cable.Vertices[0].Y, "Cabo.Vertices[0].Y");
        }

        private static void UndoRedoRotacaoRestauraElementoECabos()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            Point beforeVertex = circuit.Cable.Vertices[0];

            RotateSelected(circuit.Context, circuit.Generator);
            Point afterVertex = circuit.Cable.Vertices[0];

            circuit.Context.Commands.Undo();
            AssertEqual(0, circuit.Generator.Rotacao, "Rotacao apos undo");
            AssertEqual(beforeVertex.X, circuit.Cable.Vertices[0].X, "Cabo X apos undo");
            AssertEqual(beforeVertex.Y, circuit.Cable.Vertices[0].Y, "Cabo Y apos undo");

            circuit.Context.Commands.Redo();
            AssertEqual(90, circuit.Generator.Rotacao, "Rotacao apos redo");
            AssertEqual(afterVertex.X, circuit.Cable.Vertices[0].X, "Cabo X apos redo");
            AssertEqual(afterVertex.Y, circuit.Cable.Vertices[0].Y, "Cabo Y apos redo");
        }

        private static void CableVertexEditCriaHandlesIntermediarios()
        {
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit(new Point(190, 120), new Point(230, 150));

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();

            AssertEqual(2, circuit.Context.CableVertexEdit.Handles.Count, "Quantidade de handles");
            AssertHandle(circuit.Context.CableVertexEdit.Handles[0], circuit.CableVm, 1, circuit.Cable.Vertices[1], "Handle 1");
            AssertHandle(circuit.Context.CableVertexEdit.Handles[1], circuit.CableVm, 2, circuit.Cable.Vertices[2], "Handle 2");
        }

        private static void CableVertexEditInsereVerticeNoSegmento()
        {
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit();
            Point novoVertice = MidPoint(circuit.Cable.Vertices[0], circuit.Cable.Vertices[1]);

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            bool inserted = circuit.Context.CableVertexEdit.TryInsertVertex(novoVertice);

            Assert(inserted, "TryInsertVertex deve retornar true.");
            AssertEqual(3, circuit.Cable.Vertices.Count, "Vertices.Count apos insert");
            AssertPointEqual(novoVertice, circuit.Cable.Vertices[1], "Vertice inserido");
            AssertEqual(1, circuit.Context.CableVertexEdit.Handles.Count, "Handles.Count apos insert");
            Assert(circuit.Context.CableVertexEdit.Handles[0].IsActive, "Handle inserido deve ficar ativo.");

            circuit.Context.Commands.Undo();
            AssertEqual(2, circuit.Cable.Vertices.Count, "Vertices.Count apos undo");
            AssertEqual(0, circuit.Context.CableVertexEdit.Handles.Count, "Handles.Count apos undo");

            circuit.Context.Commands.Redo();
            AssertEqual(3, circuit.Cable.Vertices.Count, "Vertices.Count apos redo");
            AssertPointEqual(novoVertice, circuit.Cable.Vertices[1], "Vertice reinserido");
        }

        private static void CableVertexEditRemoveHandleIntermediario()
        {
            Point intermediario = new(190, 120);
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit(intermediario);

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            bool removed = circuit.Context.CableVertexEdit.TryRemoveHandle(intermediario);

            Assert(removed, "TryRemoveHandle deve retornar true.");
            AssertEqual(2, circuit.Cable.Vertices.Count, "Vertices.Count apos remove");
            AssertPointEqual(circuit.Cable.Origem!.Posicao, circuit.Cable.Vertices[0], "Origem preservada");
            AssertPointEqual(circuit.Cable.Destino!.Posicao, circuit.Cable.Vertices[^1], "Destino preservado");
            AssertEqual(0, circuit.Context.CableVertexEdit.Handles.Count, "Handles.Count apos remove");

            circuit.Context.Commands.Undo();
            AssertEqual(3, circuit.Cable.Vertices.Count, "Vertices.Count apos undo");
            AssertPointEqual(intermediario, circuit.Cable.Vertices[1], "Intermediario restaurado");

            circuit.Context.Commands.Redo();
            AssertEqual(2, circuit.Cable.Vertices.Count, "Vertices.Count apos redo");
        }

        private static void CableVertexEditRemoveHandleAtivo()
        {
            Point intermediario = new(190, 120);
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit(intermediario);

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            Assert(circuit.Context.CableVertexEdit.TryBegin(intermediario), "TryBegin deve ativar handle.");
            circuit.Context.CableVertexEdit.End();

            bool removed = circuit.Context.CableVertexEdit.TryRemoveActive();

            Assert(removed, "TryRemoveActive deve retornar true.");
            AssertEqual(2, circuit.Cable.Vertices.Count, "Vertices.Count apos remove active");

            circuit.Context.Commands.Undo();
            AssertEqual(3, circuit.Cable.Vertices.Count, "Vertices.Count apos undo");
            circuit.Context.Commands.Redo();
            AssertEqual(2, circuit.Cable.Vertices.Count, "Vertices.Count apos redo");
        }

        private static void CableVertexEditArrastaVerticeIntermediario()
        {
            Point intermediario = new(190, 120);
            Point novo = new(210, 145);
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit(intermediario);
            Point origem = circuit.Cable.Vertices[0];
            Point destino = circuit.Cable.Vertices[^1];

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            Assert(circuit.Context.CableVertexEdit.TryBegin(intermediario), "TryBegin deve iniciar arraste.");
            circuit.Context.CableVertexEdit.Update(novo, CreateInputState(novo));
            circuit.Context.CableVertexEdit.End();

            AssertPointEqual(origem, circuit.Cable.Vertices[0], "Origem apos arraste");
            AssertPointEqual(destino, circuit.Cable.Vertices[^1], "Destino apos arraste");
            AssertPointEqual(novo, circuit.Cable.Vertices[1], "Intermediario apos arraste");
            AssertHandle(circuit.Context.CableVertexEdit.Handles[0], circuit.CableVm, 1, novo, "Handle apos arraste");

            circuit.Context.Commands.Undo();
            AssertPointEqual(intermediario, circuit.Cable.Vertices[1], "Intermediario apos undo");
            circuit.Context.Commands.Redo();
            AssertPointEqual(novo, circuit.Cable.Vertices[1], "Intermediario apos redo");
        }

        private static void CableVertexEditShiftRestringeArrasteOrtogonal()
        {
            Point intermediario = new(190, 120);
            Point tentativa = new(230, 135);
            Point esperado = new(tentativa.X, intermediario.Y);
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit(intermediario);

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            Assert(circuit.Context.CableVertexEdit.TryBegin(intermediario), "TryBegin deve iniciar arraste com Shift.");
            circuit.Context.CableVertexEdit.Update(tentativa, CreateInputState(tentativa, ModifierKeys.Shift));
            circuit.Context.CableVertexEdit.End();

            AssertPointEqual(esperado, circuit.Cable.Vertices[1], "Intermediario com restricao ortogonal");

            circuit.Context.Commands.Undo();
            AssertPointEqual(intermediario, circuit.Cable.Vertices[1], "Intermediario Shift apos undo");
            circuit.Context.Commands.Redo();
            AssertPointEqual(esperado, circuit.Cable.Vertices[1], "Intermediario Shift apos redo");
        }

        private static void CableVertexEditCancelRestauraEstadoInicial()
        {
            Point intermediario = new(190, 120);
            Point tentativa = new(210, 145);
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit(intermediario);

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            Assert(circuit.Context.CableVertexEdit.TryBegin(intermediario), "TryBegin deve iniciar arraste para cancel.");
            circuit.Context.CableVertexEdit.Update(tentativa, CreateInputState(tentativa));
            circuit.Context.CableVertexEdit.Cancel();

            AssertPointEqual(intermediario, circuit.Cable.Vertices[1], "Intermediario apos cancel");
            Assert(!circuit.Context.CableVertexEdit.IsEditing, "IsEditing deve ficar false apos cancel.");
            AssertEqual(1, circuit.Context.CableVertexEdit.Handles.Count, "Handles.Count apos cancel");

            circuit.Context.Commands.Undo();
            AssertPointEqual(intermediario, circuit.Cable.Vertices[1], "Cancel nao deve criar comando");
        }

        private static void CableVertexEditNaoInsereLongeDeSegmento()
        {
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit();
            int count = circuit.Cable.Vertices.Count;

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            int handles = circuit.Context.CableVertexEdit.Handles.Count;
            bool inserted = circuit.Context.CableVertexEdit.TryInsertVertex(new Point(-1000, -1000));

            Assert(!inserted, "TryInsertVertex longe deve retornar false.");
            AssertEqual(count, circuit.Cable.Vertices.Count, "Vertices.Count sem insert");
            AssertEqual(handles, circuit.Context.CableVertexEdit.Handles.Count, "Handles.Count sem insert");
        }

        private static void CableVertexEditNaoRemoveLongeDeHandle()
        {
            Point intermediario = new(190, 120);
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit(intermediario);
            var vertices = circuit.Cable.Vertices.ToList();

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            bool removed = circuit.Context.CableVertexEdit.TryRemoveHandle(new Point(-1000, -1000));

            Assert(!removed, "TryRemoveHandle longe deve retornar false.");
            AssertVertices(circuit.Cable, vertices, "Vertices sem remove");
        }

        private static void CableVertexEditClearLimpaHandles()
        {
            CableVertexEditCircuit circuit = CreateCableVertexEditCircuit(new Point(190, 120));

            SelectCable(circuit.Context, circuit.Cable);
            circuit.Context.CableVertexEdit.Refresh();
            AssertEqual(1, circuit.Context.CableVertexEdit.Handles.Count, "Handles antes do Clear");

            circuit.Context.CableVertexEdit.Clear();

            AssertEqual(0, circuit.Context.CableVertexEdit.Handles.Count, "Handles apos Clear");
            Assert(!circuit.Context.CableVertexEdit.IsEditing, "IsEditing apos Clear");
            Assert(!circuit.Context.CableVertexEdit.TryRemoveActive(), "TryRemoveActive apos Clear deve retornar false.");
        }

        private static void RotacaoReancoraCargaComCaboConectado()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            string origemTerminalId = circuit.Cable.OrigemTerminalId;
            string destinoTerminalId = circuit.Cable.DestinoTerminalId;
            Point before = circuit.Cable.Vertices[^1];
            Point middle = new Point(220, 140);
            circuit.Cable.Vertices.Insert(1, middle);

            RotateSelected(circuit.Context, circuit.Load);

            AssertEqual(origemTerminalId, circuit.Cable.OrigemTerminalId, "OrigemTerminalId");
            AssertEqual(destinoTerminalId, circuit.Cable.DestinoTerminalId, "DestinoTerminalId");
            Assert(before != circuit.Cable.Vertices[^1], "Destino do cabo deve mover com a Carga.");
            AssertCableEndpointAtTerminal(circuit.Cable, false, circuit.Load, 0, "Carga destino");
            AssertEqual(middle.X, circuit.Cable.Vertices[1].X, "Intermediario X");
            AssertEqual(middle.Y, circuit.Cable.Vertices[1].Y, "Intermediario Y");
        }

        private static void RotacaoReancoraGeradorComCaboConectado()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            string origemTerminalId = circuit.Cable.OrigemTerminalId;
            Point before = circuit.Cable.Vertices[0];

            RotateSelected(circuit.Context, circuit.Generator);

            AssertEqual(origemTerminalId, circuit.Cable.OrigemTerminalId, "OrigemTerminalId");
            Assert(before != circuit.Cable.Vertices[0], "Origem do cabo deve mover com o Gerador.");
            AssertCableEndpointAtTerminal(circuit.Cable, true, circuit.Generator, 0, "Gerador origem");
        }

        private static void RotacaoReancoraSinEmTodosTerminais()
        {
            EditorContext context = CreateContextWithViewport();
            Sin sin = CreateSin("SIN-ROT-ANCHORS");
            var loads = Enumerable.Range(1, 4)
                .Select(i => CreateLoad($"CARGA-SIN-ROT-{i}", 100 + i, 50 + i))
                .ToList();
            var cables = new List<Cabo>();

            context.Document.AdicionarElemento(sin);

            for (int i = 0; i < loads.Count; i++)
            {
                context.Document.AdicionarElemento(loads[i]);
                Cabo cable = CreateCable(sin, i, loads[i], 0, $"L-SIN-ROT-{i}", 1.0 + i);
                cables.Add(cable);
                context.Document.AdicionarElemento(cable);
            }

            var before = cables.Select(c => c.Vertices[0]).ToList();
            var terminalIds = cables.Select(c => c.OrigemTerminalId).ToList();

            RotateSelected(context, sin);

            for (int i = 0; i < cables.Count; i++)
            {
                AssertEqual(terminalIds[i], cables[i].OrigemTerminalId, $"SIN cabo {i}.OrigemTerminalId");
                Assert(before[i] != cables[i].Vertices[0], $"SIN cabo {i} deve mover.");
                AssertCableEndpointAtTerminal(cables[i], true, sin, i, $"SIN terminal {i}");
            }
        }

        private static void RotacaoReancoraTransformadorPrimarioSecundario()
        {
            EditorContext context = CreateContextWithViewport();
            Sin sin = CreateSin("SIN-TR-ROT");
            Transformador transformador = CreateTransformador("TR-ROT-ANCHORS");
            Carga load = CreateLoad("CARGA-TR-ROT", 300, 100);
            Cabo primary = CreateCable(sin, 1, transformador, 0, "L-TR-ROT-P", 1.0);
            Cabo secondary = CreateCable(transformador, 1, load, 0, "L-TR-ROT-S", 1.1);

            context.Document.AdicionarElemento(sin);
            context.Document.AdicionarElemento(transformador);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(primary);
            context.Document.AdicionarElemento(secondary);

            Point primaryBefore = primary.Vertices[^1];
            Point secondaryBefore = secondary.Vertices[0];

            RotateSelected(context, transformador);

            Assert(primaryBefore != primary.Vertices[^1], "Primario deve reancorar.");
            Assert(secondaryBefore != secondary.Vertices[0], "Secundario deve reancorar.");
            AssertCableEndpointAtTerminal(primary, false, transformador, 0, "Transformador primario");
            AssertCableEndpointAtTerminal(secondary, true, transformador, 1, "Transformador secundario");
        }

        private static void RotacaoReancoraBarraEmDoisTerminais()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            var terminalIds = circuit.Bar.Terminais.Select(t => t.Id).ToList();
            Point incomingBefore = circuit.Incoming.Vertices[^1];
            Point outgoingBefore = circuit.Outgoing.Vertices[0];
            Point middle = new Point(230, 150);
            circuit.Outgoing.Vertices.Insert(1, middle);

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(24, circuit.Bar.Terminais.Count, "Barra.Terminais.Count");

            for (int i = 0; i < terminalIds.Count; i++)
                AssertEqual(terminalIds[i], circuit.Bar.Terminais[i].Id, $"Barra.Terminal[{i}].Id");

            Assert(incomingBefore != circuit.Incoming.Vertices[^1], "Entrada da Barra deve mover.");
            Assert(outgoingBefore != circuit.Outgoing.Vertices[0], "Saida da Barra deve mover.");
            AssertCableEndpointAtTerminal(circuit.Incoming, false, circuit.Bar, 0, "Barra entrada");
            AssertCableEndpointAtTerminal(circuit.Outgoing, true, circuit.Bar, 1, "Barra saida");
            AssertEqual(middle.X, circuit.Outgoing.Vertices[1].X, "Barra intermediario X");
            AssertEqual(middle.Y, circuit.Outgoing.Vertices[1].Y, "Barra intermediario Y");
        }

        private static void UndoRedoRotacaoReancoraTerminaisECabos()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            Point terminalBefore = GetTerminal(circuit.Load, 0).Posicao;
            Point vertexBefore = circuit.Cable.Vertices[^1];

            RotateSelected(circuit.Context, circuit.Load);
            Point terminalAfter = GetTerminal(circuit.Load, 0).Posicao;
            Point vertexAfter = circuit.Cable.Vertices[^1];

            circuit.Context.Commands.Undo();
            AssertEqual(0, circuit.Load.Rotacao, "Carga.Rotacao undo");
            AssertEqual(terminalBefore.X, GetTerminal(circuit.Load, 0).Posicao.X, "Terminal X undo");
            AssertEqual(terminalBefore.Y, GetTerminal(circuit.Load, 0).Posicao.Y, "Terminal Y undo");
            AssertEqual(vertexBefore.X, circuit.Cable.Vertices[^1].X, "Cabo X undo");
            AssertEqual(vertexBefore.Y, circuit.Cable.Vertices[^1].Y, "Cabo Y undo");

            circuit.Context.Commands.Redo();
            AssertEqual(90, circuit.Load.Rotacao, "Carga.Rotacao redo");
            AssertEqual(terminalAfter.X, GetTerminal(circuit.Load, 0).Posicao.X, "Terminal X redo");
            AssertEqual(terminalAfter.Y, GetTerminal(circuit.Load, 0).Posicao.Y, "Terminal Y redo");
            AssertEqual(vertexAfter.X, circuit.Cable.Vertices[^1].X, "Cabo X redo");
            AssertEqual(vertexAfter.Y, circuit.Cable.Vertices[^1].Y, "Cabo Y redo");
        }

        private static void SnapEncontraTerminalAposRotacaoComCabo()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();

            RotateSelected(circuit.Context, circuit.Load);

            Terminal expected = GetTerminal(circuit.Load, 0);
            Terminal? snapped = circuit.Context.Snap.ObterTerminalMaisProximo(expected.Posicao);

            Assert(snapped != null, "Snap deve encontrar terminal rotacionado.");
            AssertEqual(expected.Id, snapped!.Id, "Snap.TerminalId");
            AssertEqual(expected.Dono.Id, snapped.Dono.Id, "Snap.Dono");
        }

        private static void ElectricGraphBuildAposRotacaoNaoAlteraDocument()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            RotateSelected(circuit.Context, circuit.Generator);
            int count = circuit.Context.Document.Elementos.Count;

            _ = new ElectricGraphBuilder(circuit.Context.Document).Build();
            _ = new ElectricGraphBuilder(circuit.Context.Document).Build();

            AssertEqual(count, circuit.Context.Document.Elementos.Count, "Quantidade de elementos apos builds");
        }

        private static void DtoNaoMudaPorCausaDaRotacao()
        {
            RotatedCircuit circuit = CreateRotatedCircuit();
            CircuitDto before = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            RotateSelected(circuit.Context, circuit.Generator);
            CircuitDto after = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            AssertEqual(before.Slack!.Id, after.Slack!.Id, "Slack.Id");
            AssertEqual(before.Lines.Single().Barra1, after.Lines.Single().Barra1, "Line.Barra1");
            AssertEqual(before.Lines.Single().Barra2, after.Lines.Single().Barra2, "Line.Barra2");
            AssertEqual(before.Loads.Single().Barra, after.Loads.Single().Barra, "Load.Barra");
        }

        private static void RotationServiceAceitaBarra()
        {
            EditorContext context = CreateContextWithViewport();
            Barra bar = CreateBar("BARRA-PODE-ROT");
            context.Document.AdicionarElemento(bar);

            Assert(RotationService.PodeRotacionar(GetVm(context, bar)), "Barra deve ser aceita para rotacao.");
        }

        private static void BarraNovaPossuiAlturaPadrao()
        {
            Barra bar = new();

            AssertEqual(Barra.ALTURA_PADRAO, bar.Altura, "Altura padrao da Barra");
            AssertEqual(24, bar.Terminais.Count, "Quantidade de terminais da Barra");
        }

        private static void AlterarAlturaDaBarraMudaBounds()
        {
            EditorContext context = CreateContextWithViewport();
            Barra bar = CreateBar("BARRA-ALT-BOUNDS");
            context.Document.AdicionarElemento(bar);
            BarraViewModel vm = GetBarVm(context, bar);

            double before = vm.Bounds.Height;
            vm.Altura = 220;

            AssertEqual(220, bar.Altura, "Barra.Altura");
            AssertEqual(220, vm.Bounds.Height, "Bounds.Height");
            Assert(before != vm.Bounds.Height, "Bounds deve mudar apos alterar altura.");
        }

        private static void BarraPadraoMantemVinteQuatroTerminaisComPitchFixo()
        {
            Barra bar = CreateBar("BARRA-PITCH-PADRAO");

            AssertEqual(24, bar.Terminais.Count, "Quantidade de terminais padrao");
            AssertNoDuplicateTerminalIds(bar, "Barra padrao");
            AssertEqual("BARRA-01", bar.Terminais[0].Id, "Primeiro terminal");
            AssertEqual("BARRA-24", bar.Terminais[^1].Id, "Ultimo terminal");
            AssertEqual(0, bar.Terminais[0].PosicaoLocal.Y, "Primeiro terminal local Y");
            AssertEqual(Barra.ALTURA_PADRAO, bar.Terminais[^1].PosicaoLocal.Y, "Ultimo terminal local Y");
            AssertTerminaisDaBarraSeguemPitchFixo(bar, "Barra padrao");
        }

        private static void CrescerBarraAumentaConectoresPreservandoIds()
        {
            Barra bar = CreateBar("BARRA-PITCH-CRESCE");
            var idsIniciais = bar.Terminais.Select(t => t.Id).ToList();

            bar.Altura = 240;
            bar.AtualizarTerminais();

            Assert(bar.Terminais.Count > idsIniciais.Count, "Quantidade deve aumentar.");
            AssertNoDuplicateTerminalIds(bar, "Barra aumentada");

            for (int i = 0; i < idsIniciais.Count; i++)
                AssertEqual(idsIniciais[i], bar.Terminais[i].Id, $"Terminal existente {i}.Id");

            AssertEqual("BARRA-25", bar.Terminais[24].Id, "Primeiro terminal novo");
            AssertTerminaisDaBarraSeguemPitchFixo(bar, "Barra aumentada");
        }

        private static void ReduzirBarraRemoveTerminaisLivresExcedentes()
        {
            Barra bar = CreateBar("BARRA-PITCH-REDUZ");
            bar.Altura = 240;
            bar.AtualizarTerminais();
            int quantidadeAumentada = bar.Terminais.Count;

            bar.Altura = Barra.ALTURA_MINIMA;
            bar.AtualizarTerminais();

            Assert(bar.Terminais.Count < quantidadeAumentada, "Quantidade deve reduzir.");
            AssertNoDuplicateTerminalIds(bar, "Barra reduzida");
            AssertTerminaisDaBarraSeguemPitchFixo(bar, "Barra reduzida");

            foreach (Terminal terminal in bar.Terminais)
                Assert(terminal.PosicaoLocal.Y <= bar.Altura + 0.000001, $"{terminal.Id}: Y deve ficar dentro da altura.");
        }

        private static void ReduzirBarraPreservaTerminalOcupado()
        {
            BarResizeCircuit circuit = CreateBarResizeCircuit();
            Terminal terminalAlto = GetTerminal(circuit.Bar, "BARRA-30");
            Cabo cable = CreateCable(circuit.Bar, terminalAlto, circuit.OtherBar, circuit.OtherBar.Terminais[0], "L-BARRA-ALTA", 1.0);
            circuit.Document.AdicionarElemento(cable);

            circuit.Context.GeometryUpdates.AplicarAlturaBarra(circuit.Bar, Barra.ALTURA_MINIMA);

            Terminal preservado = AssertTerminalExists(circuit.Bar, terminalAlto.Id);
            AssertNoDuplicateTerminalIds(circuit.Bar, "Barra reduzida com cabo");
            Assert(preservado.PosicaoLocal.Y <= circuit.Bar.Altura + 0.000001, "Terminal ocupado deve ficar em posicao local valida.");
            AssertEqual(terminalAlto.Id, cable.OrigemTerminalId, "OrigemTerminalId preservado");
            AssertEqual(preservado.Posicao.X, cable.Vertices[0].X, "Cabo origem X reancorado");
            AssertEqual(preservado.Posicao.Y, cable.Vertices[0].Y, "Cabo origem Y reancorado");
        }

        private static void ResizeDaBarraReancoraCaboConectado()
        {
            BarResizeCircuit circuit = CreateBarResizeCircuit();
            Terminal terminal = GetTerminal(circuit.Bar, "BARRA-24");
            Cabo cable = CreateCable(circuit.Bar, terminal, circuit.OtherBar, circuit.OtherBar.Terminais[0], "L-BARRA-REANCORA", 1.0);
            circuit.Document.AdicionarElemento(cable);
            string endpoint = cable.OrigemTerminalId;

            circuit.Context.GeometryUpdates.AplicarAlturaBarra(circuit.Bar, 240);

            Terminal atual = AssertTerminalExists(circuit.Bar, endpoint);
            AssertEqual(endpoint, cable.OrigemTerminalId, "Endpoint preservado");
            AssertEqual(atual.Posicao.X, cable.Vertices[0].X, "Cabo origem X reancorado");
            AssertEqual(atual.Posicao.Y, cable.Vertices[0].Y, "Cabo origem Y reancorado");
        }

        private static void UndoRedoResizeBarraPreservaCabo()
        {
            BarResizeCircuit circuit = CreateBarResizeCircuit();
            Terminal terminal = GetTerminal(circuit.Bar, "BARRA-30");
            Cabo cable = CreateCable(circuit.Bar, terminal, circuit.OtherBar, circuit.OtherBar.Terminais[0], "L-BARRA-UNDO", 1.0);
            circuit.Document.AdicionarElemento(cable);
            string terminalId = terminal.Id;
            var command = new ResizeBarraCommand(
                circuit.Bar,
                240,
                circuit.Bar.PosicaoX,
                circuit.Bar.PosicaoY,
                Barra.ALTURA_MINIMA,
                circuit.Bar.PosicaoX,
                circuit.Bar.PosicaoY,
                circuit.Context.GeometryUpdates);

            command.Execute();
            AssertResizePreservaCabo(circuit.Bar, cable, terminalId, Barra.ALTURA_MINIMA, "Execute");

            command.Undo();
            AssertResizePreservaCabo(circuit.Bar, cable, terminalId, 240, "Undo");

            command.Redo();
            AssertResizePreservaCabo(circuit.Bar, cable, terminalId, Barra.ALTURA_MINIMA, "Redo");
        }

        private static void ConnectivityRetornaTerminaisOcupadosDaBarra()
        {
            BarResizeCircuit circuit = CreateBarResizeCircuit();
            Terminal ocupado = GetTerminal(circuit.Bar, "BARRA-30");
            Terminal livre = GetTerminal(circuit.Bar, "BARRA-29");
            Cabo cable = CreateCable(circuit.Bar, ocupado, circuit.OtherBar, circuit.OtherBar.Terminais[0], "L-BARRA-OCUPADOS", 1.0);
            circuit.Document.AdicionarElemento(cable);

            IReadOnlySet<string> ocupados = circuit.Context.Connectivity.ObterTerminalIdsOcupados(circuit.Bar);

            Assert(ocupados.Contains(ocupado.Id), "Terminal ocupado deve aparecer no conjunto.");
            Assert(!ocupados.Contains(livre.Id), "Terminal livre nao deve aparecer no conjunto.");
        }

        private static void CaboConectadoABarraReancoraAposAlterarAltura()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            string origemId = circuit.Outgoing.OrigemId;
            string destinoId = circuit.Outgoing.DestinoId;
            string origemTerminalId = circuit.Outgoing.OrigemTerminalId;
            string destinoTerminalId = circuit.Outgoing.DestinoTerminalId;
            Point before = circuit.Outgoing.Vertices[0];
            Point middle = new Point(230, 150);
            circuit.Outgoing.Vertices.Insert(1, middle);

            SetBarHeight(circuit.Context, circuit.Bar, 240);

            AssertEqual(GetTerminal(circuit.Bar, 1).Posicao.X, circuit.Outgoing.Vertices[0].X, "Ponta conectada X");
            AssertEqual(GetTerminal(circuit.Bar, 1).Posicao.Y, circuit.Outgoing.Vertices[0].Y, "Ponta conectada Y");
            AssertCableEndpointAtTerminal(circuit.Outgoing, true, circuit.Bar, 1, "Barra saida apos altura");
            AssertEqual(middle.X, circuit.Outgoing.Vertices[1].X, "Intermediario X preservado");
            AssertEqual(middle.Y, circuit.Outgoing.Vertices[1].Y, "Intermediario Y preservado");
            AssertEqual(origemId, circuit.Outgoing.OrigemId, "OrigemId preservado");
            AssertEqual(destinoId, circuit.Outgoing.DestinoId, "DestinoId preservado");
            AssertEqual(origemTerminalId, circuit.Outgoing.OrigemTerminalId, "OrigemTerminalId preservado");
            AssertEqual(destinoTerminalId, circuit.Outgoing.DestinoTerminalId, "DestinoTerminalId preservado");
        }

        private static void BarraComAlturaAlteradaPersisteAposReload()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            SetBarHeight(circuit.Context, circuit.Bar, 260);

            AraciDocument loaded = SaveAndLoad(circuit.Context.Document);
            Barra loadedBar = FindById<Barra>(loaded, circuit.Bar.Id);

            AssertEqual(260, loadedBar.Altura, "Altura apos reload");
            Assert(loadedBar.Terminais[^1].PosicaoLocal.Y <= loadedBar.Altura + 0.000001, "Ultimo terminal apos reload deve ficar dentro da altura.");
            AssertTerminaisDaBarraSeguemPitchFixo(loadedBar, "Barra apos reload");
            AssertCableEndpointAtTerminal(
                FindById<Cabo>(loaded, circuit.Outgoing.Id),
                true,
                loadedBar,
                1,
                "Cabo saida apos reload");
        }

        private static void ElectricGraphContinuaValidoAposAlturaDaBarra()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            SetBarHeight(circuit.Context, circuit.Bar, 240);

            ElectricGraph graph = new ElectricGraphBuilder(circuit.Context.Document).Build();

            AssertEqual(2, graph.Edges.Count, "Quantidade de arestas");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Arestas invalidas");
            AssertEqual(2, graph.GetEdgesForElement(circuit.Bar.Id.ToString()).Count, "Arestas da Barra");
        }

        private static void DtoNaoMudaPorCausaDaAlturaDaBarra()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            CircuitDto before = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            SetBarHeight(circuit.Context, circuit.Bar, 240);
            CircuitDto after = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            AssertEqual(before.Slack!.Id, after.Slack!.Id, "Slack.Id");
            AssertEqual(before.Lines.Count, after.Lines.Count, "Lines.Count");
            AssertEqual(before.Loads.Count, after.Loads.Count, "Loads.Count");
            AssertEqual(before.Lines[0].Barra1, after.Lines[0].Barra1, "Line[0].Barra1");
            AssertEqual(before.Lines[0].Barra2, after.Lines[0].Barra2, "Line[0].Barra2");
            AssertEqual(before.Lines[1].Barra1, after.Lines[1].Barra1, "Line[1].Barra1");
            AssertEqual(before.Lines[1].Barra2, after.Lines[1].Barra2, "Line[1].Barra2");
            AssertEqual(before.Loads.Single().Barra, after.Loads.Single().Barra, "Load.Barra");
        }

        private static void RotacaoDaBarraFuncionaAposAlturaAlterada()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            SetBarHeight(circuit.Context, circuit.Bar, 240);
            Point before = circuit.Outgoing.Vertices[0];

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(90, circuit.Bar.Rotacao, "Rotacao da Barra");
            Assert(before != circuit.Outgoing.Vertices[0], "Cabo deve reancorar apos rotacao com altura alterada.");
            AssertCableEndpointAtTerminal(circuit.Outgoing, true, circuit.Bar, 1, "Barra saida apos altura e rotacao");
        }

        private static void CaboPermaneceAncoradoAposAlturaRotacaoMovimentoEReload()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            Point middle = new Point(230, 150);
            circuit.Outgoing.Vertices.Insert(1, middle);

            SetBarHeight(circuit.Context, circuit.Bar, 240);
            RotateSelected(circuit.Context, circuit.Bar);
            MoveElement(circuit.Context, circuit.Bar, new Vector(35, 20));

            AraciDocument loaded = SaveAndLoad(circuit.Context.Document);
            Barra loadedBar = FindById<Barra>(loaded, circuit.Bar.Id);
            Cabo loadedOutgoing = FindById<Cabo>(loaded, circuit.Outgoing.Id);

            AssertEqual(240, loadedBar.Altura, "Altura apos sequencia e reload");
            AssertEqual(90, loadedBar.Rotacao, "Rotacao apos sequencia e reload");
            AssertCableEndpointAtTerminal(loadedOutgoing, true, loadedBar, 1, "Cabo apos sequencia e reload");
            AssertEqual(middle.X, loadedOutgoing.Vertices[1].X, "Intermediario X apos sequencia e reload");
            AssertEqual(middle.Y, loadedOutgoing.Vertices[1].Y, "Intermediario Y apos sequencia e reload");
        }

        private static void AlturaInvalidaDaBarraNormalizaParaMinimo()
        {
            Barra bar = CreateBar("BARRA-ALT-MIN");

            bar.Altura = -10;
            bar.AtualizarTerminais();

            AssertEqual(Barra.ALTURA_MINIMA, bar.Altura, "Altura minima");
            Assert(bar.Terminais[^1].PosicaoLocal.Y <= bar.Altura + 0.000001, "Ultimo terminal com altura minima deve ficar dentro da altura.");
            AssertTerminaisDaBarraSeguemPitchFixo(bar, "Barra com altura minima");
        }

        private static void BarraSelecionadaRotacionaZeroParaNoventa()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(90, circuit.Bar.Rotacao, "Rotacao da Barra");
        }

        private static void BarraCiclaQuadrantes()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);
            AssertEqual(90, circuit.Bar.Rotacao, "Rotacao 0 -> 90");
            RotateSelected(circuit.Context, circuit.Bar);
            AssertEqual(180, circuit.Bar.Rotacao, "Rotacao 90 -> 180");
            RotateSelected(circuit.Context, circuit.Bar);
            AssertEqual(270, circuit.Bar.Rotacao, "Rotacao 180 -> 270");
            RotateSelected(circuit.Context, circuit.Bar);
            AssertEqual(0, circuit.Bar.Rotacao, "Rotacao 270 -> 0");
        }

        private static void PreviewDeBarraPreservaRotacao()
        {
            EditorContext context = CreateContextWithViewport();
            var controller = CriarPreviewController<BarraViewModel, Barra>(
                context,
                context.ElementoFactory.CriarBarraVM,
                vm => vm.Barra);

            controller.Update(new Point(240, 180));
            controller.RotateClockwise();

            Barra real = context.ElementoFactory.CriarBarra();
            real.Rotacao = controller.CurrentRotation;

            AssertEqual(90, real.Rotacao, "Rotacao copiada do preview da Barra");
        }

        private static void BarraPreservaVinteQuatroTerminalIdsAposRotacao()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            var before = circuit.Bar.Terminais.Select(t => t.Id).ToList();

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(24, circuit.Bar.Terminais.Count, "Quantidade de terminais");

            for (int i = 0; i < 24; i++)
            {
                string expected = $"BARRA-{i + 1:00}";
                AssertEqual(expected, circuit.Bar.Terminais[i].Id, $"Terminal {i}.Id padrao");
                AssertEqual(before[i], circuit.Bar.Terminais[i].Id, $"Terminal {i}.Id preservado");
            }
        }

        private static void TerminaisDaBarraMudamPosicaoVisualAposRotacao()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            var before = circuit.Bar.Terminais
                .Select(t => (t.Id, t.Posicao))
                .ToList();

            RotateSelected(circuit.Context, circuit.Bar);

            Assert(
                before.Any(item => circuit.Bar.Terminais.Single(t => t.Id == item.Id).Posicao != item.Posicao),
                "Ao menos um terminal da Barra deve mudar de posicao visual apos rotacao.");
        }

        private static void CaboConectadoABarraPreservaTerminalIdAposRotacao()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            string origemTerminalId = circuit.Outgoing.OrigemTerminalId;
            string destinoTerminalId = circuit.Incoming.DestinoTerminalId;

            RotateSelected(circuit.Context, circuit.Bar);

            AssertEqual(origemTerminalId, circuit.Outgoing.OrigemTerminalId, "Cabo saida OrigemTerminalId");
            AssertEqual(destinoTerminalId, circuit.Incoming.DestinoTerminalId, "Cabo entrada DestinoTerminalId");
        }

        private static void CaboConectadoABarraReancoraVisualmenteAposRotacao()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            Point before = circuit.Outgoing.Vertices[0];
            Point middle = new Point(230, 150);
            circuit.Outgoing.Vertices.Insert(1, middle);

            RotateSelected(circuit.Context, circuit.Bar);

            Terminal terminal = GetTerminal(circuit.Bar, 1);
            Assert(before != circuit.Outgoing.Vertices[0], "Vertice inicial deve mudar apos rotacao da Barra.");
            AssertEqual(terminal.Posicao.X, circuit.Outgoing.Vertices[0].X, "Cabo.Vertices[0].X");
            AssertEqual(terminal.Posicao.Y, circuit.Outgoing.Vertices[0].Y, "Cabo.Vertices[0].Y");
            AssertEqual(middle.X, circuit.Outgoing.Vertices[1].X, "Vertice intermediario X preservado");
            AssertEqual(middle.Y, circuit.Outgoing.Vertices[1].Y, "Vertice intermediario Y preservado");
        }

        private static void UndoRedoRotacaoDaBarraRestauraCabos()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            Point beforeVertex = circuit.Outgoing.Vertices[0];

            RotateSelected(circuit.Context, circuit.Bar);
            Point afterVertex = circuit.Outgoing.Vertices[0];

            circuit.Context.Commands.Undo();
            AssertEqual(0, circuit.Bar.Rotacao, "Rotacao da Barra apos undo");
            AssertEqual(beforeVertex.X, circuit.Outgoing.Vertices[0].X, "Cabo X apos undo");
            AssertEqual(beforeVertex.Y, circuit.Outgoing.Vertices[0].Y, "Cabo Y apos undo");

            circuit.Context.Commands.Redo();
            AssertEqual(90, circuit.Bar.Rotacao, "Rotacao da Barra apos redo");
            AssertEqual(afterVertex.X, circuit.Outgoing.Vertices[0].X, "Cabo X apos redo");
            AssertEqual(afterVertex.Y, circuit.Outgoing.Vertices[0].Y, "Cabo Y apos redo");
        }

        private static void BarraRotacionadaPersisteAposReload()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);
            AraciDocument loaded = SaveAndLoad(circuit.Context.Document);

            Barra loadedBar = FindById<Barra>(loaded, circuit.Bar.Id);
            Cabo loadedOutgoing = FindById<Cabo>(loaded, circuit.Outgoing.Id);

            AssertEqual(90, loadedBar.Rotacao, "Rotacao da Barra apos reload");
            AssertEqual(circuit.Outgoing.OrigemTerminalId, loadedOutgoing.OrigemTerminalId, "OrigemTerminalId apos reload");
            AssertEqual(circuit.Outgoing.Vertices[0].X, loadedOutgoing.Vertices[0].X, "Vertice X apos reload");
            AssertEqual(circuit.Outgoing.Vertices[0].Y, loadedOutgoing.Vertices[0].Y, "Vertice Y apos reload");
        }

        private static void ElectricGraphAposRotacaoDaBarraMantemArestasValidas()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);
            ElectricGraph graph = new ElectricGraphBuilder(circuit.Context.Document).Build();

            AssertEqual(2, graph.Edges.Count, "Quantidade de arestas");
            AssertEqual(0, graph.GetInvalidEdges().Count, "Arestas invalidas");
            AssertEqual(2, graph.GetEdgesForElement(circuit.Bar.Id.ToString()).Count, "Arestas da Barra");
        }

        private static void DtoNaoMudaPorCausaDaRotacaoDaBarra()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            CircuitDto before = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            RotateSelected(circuit.Context, circuit.Bar);
            CircuitDto after = new CircuitBuilder(new ParameterReader(circuit.Context.Document)).Build();

            AssertEqual(before.Slack!.Id, after.Slack!.Id, "Slack.Id");
            AssertEqual(before.Lines.Count, after.Lines.Count, "Lines.Count");
            AssertEqual(before.Loads.Count, after.Loads.Count, "Loads.Count");
            AssertEqual(before.Lines[0].Barra1, after.Lines[0].Barra1, "Line[0].Barra1");
            AssertEqual(before.Lines[0].Barra2, after.Lines[0].Barra2, "Line[0].Barra2");
            AssertEqual(before.Lines[1].Barra1, after.Lines[1].Barra1, "Line[1].Barra1");
            AssertEqual(before.Lines[1].Barra2, after.Lines[1].Barra2, "Line[1].Barra2");
            AssertEqual(before.Loads.Single().Barra, after.Loads.Single().Barra, "Load.Barra");
        }

        private static void HitTestEncontraBarraRotacionada()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();
            ElementoViewModel vm = GetVm(circuit.Context, circuit.Bar);

            RotateSelected(circuit.Context, circuit.Bar);

            Point visualPoint = RotateAround(
                new Point(circuit.Bar.PosicaoX + 5, circuit.Bar.PosicaoY + 8),
                vm.Centro,
                circuit.Bar.Rotacao);

            ElementoViewModel? hit = circuit.Context.SceneQueries.HitTest(visualPoint)?.Elemento;

            Assert(ReferenceEquals(vm, hit), "Hit-test deve retornar a Barra rotacionada.");
        }

        private static void SnapEncontraTerminalDeBarraRotacionada()
        {
            BarRotationCircuit circuit = CreateBarRotationCircuit();

            RotateSelected(circuit.Context, circuit.Bar);

            Terminal expected = GetTerminal(circuit.Bar, 23);
            Terminal? snapped = circuit.Context.Snap.ObterTerminalMaisProximo(expected.Posicao);

            Assert(snapped != null, "Snap deve encontrar terminal da Barra rotacionada.");
            AssertEqual(expected.Id, snapped!.Id, "TerminalId do snap");
            AssertEqual(circuit.Bar.Id, snapped.Dono.Id, "Dono do terminal do snap");
        }

        private static SimpleCircuit CreateSimpleCircuit()
        {
            var document = new AraciDocument();
            Gerador generator = CreateGenerator("GERADOR-TESTE", 1250, 0.93);
            Carga load = CreateLoad("CARGA-TESTE", 650, 210);

            Cabo cable = CreateCable(generator, load, "L-TESTE", 2.75);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(load);
            document.AdicionarElemento(cable);

            return new SimpleCircuit(document, generator, load, cable);
        }

        private static Elemento CreateAnnotation(string name)
        {
            return new FakeAnnotationElement
            {
                Nome = name,
                PosicaoX = 40,
                PosicaoY = 40
            };
        }

        private static string SerializeCircuitDto(CircuitDto dto)
        {
            return JsonSerializer.Serialize(dto);
        }

        private static RotatedCircuit CreateRotatedCircuit()
        {
            EditorContext context = CreateContextWithViewport();
            Gerador generator = CreateGenerator("GER-ROT-CABO", 1000, 0.95);
            Carga load = CreateLoad("CARGA-ROT-CABO", 300, 100);
            Cabo cable = CreateCable(generator, 0, load, 0, "L-ROT-CABO", 1.0);

            context.Document.AdicionarElemento(generator);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(cable);

            return new RotatedCircuit(context, generator, load, cable);
        }

        private static CableVertexEditCircuit CreateCableVertexEditCircuit(params Point[] intermediarios)
        {
            EditorContext context = CreateContextWithViewport();
            Gerador generator = CreateGenerator("GER-CABO-VERTEX", 1000, 0.95);
            Carga load = CreateLoad("CARGA-CABO-VERTEX", 300, 100);
            Cabo cable = CreateCable(generator, 0, load, 0, "L-CABO-VERTEX", 1.0);

            for (int i = 0; i < intermediarios.Length; i++)
                cable.Vertices.Insert(i + 1, intermediarios[i]);

            cable.AtualizarTerminaisPelasPontas();
            context.Document.AdicionarElemento(generator);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(cable);

            return new CableVertexEditCircuit(context, generator, load, cable, GetCableVm(context, cable));
        }

        private static BarRotationCircuit CreateBarRotationCircuit()
        {
            EditorContext context = CreateContextWithViewport();
            Gerador generator = CreateGenerator("GER-BARRA-ROT", 1000, 0.95);
            Barra bar = CreateBar("BARRA-ROT");
            Carga load = CreateLoad("CARGA-BARRA-ROT", 300, 100);
            Cabo incoming = CreateCable(generator, 0, bar, 0, "L-BARRA-IN", 1.0);
            Cabo outgoing = CreateCable(bar, 1, load, 0, "L-BARRA-OUT", 1.0);

            context.Document.AdicionarElemento(generator);
            context.Document.AdicionarElemento(bar);
            context.Document.AdicionarElemento(load);
            context.Document.AdicionarElemento(incoming);
            context.Document.AdicionarElemento(outgoing);

            return new BarRotationCircuit(context, generator, bar, load, incoming, outgoing);
        }

        private static BarResizeCircuit CreateBarResizeCircuit()
        {
            EditorContext context = CreateContextWithViewport();
            Barra bar = CreateBar("BARRA-RESIZE-A");
            Barra otherBar = CreateBar("BARRA-RESIZE-B");
            otherBar.PosicaoX = 300;
            context.Document.AdicionarElemento(bar);
            context.Document.AdicionarElemento(otherBar);
            context.GeometryUpdates.AplicarAlturaBarra(bar, 240);
            return new BarResizeCircuit(context, context.Document, bar, otherBar);
        }

        private static EditorContext CreateContextWithViewport()
        {
            var context = new EditorContext();
            var viewport = context.CriarViewportViewModel();

            context.InicializarViewport(viewport);
            return context;
        }

        private static InsertPreviewController<TViewModel, TModel> CriarPreviewController<TViewModel, TModel>(
            EditorContext context,
            Func<TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
            where TViewModel : ElementoViewModel
            where TModel : Elemento
        {
            return new InsertPreviewController<TViewModel, TModel>(
                criarPreview,
                obterModelo,
                context.Snap,
                context.Geometry,
                context.TerminalLayout,
                context.AlignmentGuides,
                context.Scene,
                context.SceneQueries);
        }

        private static void RotateSelected(EditorContext context, Elemento elemento)
        {
            ElementoViewModel vm = GetVm(context, elemento);

            context.Selection.Selecionar(vm);
            Assert(context.Rotation.RotateSelectionClockwise(), "Rotacao da selecao deve ser aplicada.");
        }

        private static void SetBarHeight(EditorContext context, Barra bar, double height)
        {
            GetBarVm(context, bar).Altura = height;
        }

        private static void MoveElement(EditorContext context, Elemento elemento, Vector delta)
        {
            ElementoViewModel vm = GetVm(context, elemento);

            context.Move.BeginMove(new[] { vm });
            context.Move.MoverVisual(vm, delta);
            context.Move.EndMove(new[] { vm });
        }

        private static BarraViewModel GetBarVm(EditorContext context, Barra bar)
        {
            if (GetVm(context, bar) is not BarraViewModel vm)
                throw new InvalidOperationException($"ViewModel da Barra '{bar.Nome}' nao encontrado.");

            return vm;
        }

        private static CaboViewModel GetCableVm(EditorContext context, Cabo cable)
        {
            if (GetVm(context, cable) is not CaboViewModel vm)
                throw new InvalidOperationException($"ViewModel do Cabo '{cable.Nome}' nao encontrado.");

            return vm;
        }

        private static void SelectCable(EditorContext context, Cabo cable)
        {
            context.Selection.Selecionar(GetCableVm(context, cable));
        }

        private static ToolInputState CreateInputState(Point world, ModifierKeys modifiers = ModifierKeys.None)
        {
            return new ToolInputState(modifiers, MouseButton.Left, 0, world, world);
        }

        private static ElementoViewModel GetVm(EditorContext context, Elemento elemento)
        {
            ElementoViewModel? vm = context.Viewport?.ObterViewModel(elemento);

            if (vm == null)
                throw new InvalidOperationException($"ViewModel de '{elemento.Nome}' nao encontrado.");

            return vm;
        }

        private static InstancePropertyDescriptor GetInstanceProperty(EditorContext context, Type viewModelType, string propertyName)
        {
            InstancePropertyDescriptor? descriptor = context.Elements
                .GetInstanceProperties(viewModelType)
                .FirstOrDefault(p => p.PropertyName == propertyName);

            if (descriptor == null)
                throw new InvalidOperationException($"Propriedade '{propertyName}' nao encontrada em {viewModelType.Name}.");

            return descriptor;
        }

        private static PropertyDescriptorViewModel GetPropertyRow(PropertiesViewModel properties, string propertyName)
        {
            PropertyDescriptorViewModel? row = properties.Propriedades.FirstOrDefault(p => p.PropertyName == propertyName);

            if (row == null)
                throw new InvalidOperationException($"Linha de propriedade '{propertyName}' nao encontrada.");

            return row;
        }

        private static AraciDocument CreateBranchDocument()
        {
            var document = new AraciDocument();
            Gerador generator = CreateGenerator("GERADOR-BRANCH", 1300, 0.94);
            Barra bar = CreateBar("BARRA-BRANCH");
            Carga load1 = CreateLoad("CARGA-B1", 320, 90);
            Carga load2 = CreateLoad("CARGA-B2", 280, 85);

            document.AdicionarElemento(generator);
            document.AdicionarElemento(bar);
            document.AdicionarElemento(load1);
            document.AdicionarElemento(load2);
            document.AdicionarElemento(CreateCable(generator, 0, bar, 0, "L-01", 1.0));
            document.AdicionarElemento(CreateCable(bar, 1, load1, 0, "L-02", 1.1));
            document.AdicionarElemento(CreateCable(bar, 2, load2, 0, "L-03", 1.2));

            return document;
        }

        private static Gerador CreateGenerator(string name, double power, double fp)
        {
            var generator = new Gerador
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoGerador
                {
                    TensaoKV = 13.8,
                    FatorPotencia = fp
                },
                PosicaoX = 100,
                PosicaoY = 100,
                PotenciaAtiva = power,
                FatorPotencia = fp,
                TensaoLinha = "13.8"
            };

            generator.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            return generator;
        }

        private static Carga CreateLoad(string name, double activePower, double reactivePower)
        {
            var load = new Carga
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoCarga
                {
                    Tensao = "13.8",
                    Conexao = "Wye",
                    ModeloCarga = 1
                },
                PosicaoX = 300,
                PosicaoY = 100,
                PotenciaAtiva = activePower,
                PotenciaReativa = reactivePower,
                TensaoLinha = "13.8"
            };

            load.AtualizarTerminais(ElementGeometryDefaults.EquipamentoLargura);

            return load;
        }

        private static Barra CreateBar(string name)
        {
            var bar = new Barra
            {
                Nome = name,
                PosicaoX = 200,
                PosicaoY = 100
            };

            bar.AtualizarTerminais();

            return bar;
        }

        private static Sin CreateSin(string name)
        {
            var sin = new Sin
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoSin
                {
                    Fases = 3,
                    PotenciaCurtoMVA = 500,
                    RelacaoXR = 10
                },
                PosicaoX = 80,
                PosicaoY = 80,
                TensaoLinha = "13.8"
            };

            sin.AtualizarTerminais(
                ElementGeometryDefaults.EquipamentoLargura,
                ElementGeometryDefaults.EquipamentoAltura);

            return sin;
        }

        private static Transformador CreateTransformador(string name)
        {
            var transformador = new Transformador
            {
                Nome = name,
                Barra = name,
                Tipo = new TipoTransformador
                {
                    Fases = 3,
                    Enrolamentos = 2
                },
                PosicaoX = 80,
                PosicaoY = 80,
                TensaoLinha = "13.8"
            };

            transformador.TensaoPrimarioKV = 13.8;
            transformador.TensaoSecundarioKV = 0.38;
            transformador.PotenciaAparente = 500;

            transformador.AtualizarTerminais(
                ElementGeometryDefaults.TransformadorLargura,
                ElementGeometryDefaults.TransformadorAltura);

            return transformador;
        }

        private static Cabo CreateCable(
            Gerador generator,
            Carga load,
            string name,
            double length)
        {
            return CreateCable(generator, 0, load, 0, name, length);
        }

        private static Cabo CreateCable(
            Elemento from,
            int fromTerminalIndex,
            Elemento to,
            int toTerminalIndex,
            string name,
            double length)
        {
            Terminal fromTerminal = GetTerminal(from, fromTerminalIndex);
            Terminal toTerminal = GetTerminal(to, toTerminalIndex);
            var cable = new Cabo
            {
                Nome = name,
                OrigemId = from.Id.ToString(),
                OrigemTerminalId = fromTerminal.Id,
                DestinoId = to.Id.ToString(),
                DestinoTerminalId = toTerminal.Id,
                Comprimento = length
            };

            cable.DefinirOrigem(fromTerminal.Posicao);
            cable.DefinirDestino(toTerminal.Posicao);
            cable.Vertices.Add(fromTerminal.Posicao);
            cable.Vertices.Add(toTerminal.Posicao);

            return cable;
        }

        private static Cabo CreateCable(
            Elemento from,
            Terminal fromTerminal,
            Elemento to,
            Terminal toTerminal,
            string name,
            double length)
        {
            var cable = new Cabo
            {
                Nome = name,
                OrigemId = from.Id.ToString(),
                OrigemTerminalId = fromTerminal.Id,
                DestinoId = to.Id.ToString(),
                DestinoTerminalId = toTerminal.Id,
                Comprimento = length
            };

            cable.DefinirOrigem(fromTerminal.Posicao);
            cable.DefinirDestino(toTerminal.Posicao);
            cable.Vertices.Add(fromTerminal.Posicao);
            cable.Vertices.Add(toTerminal.Posicao);

            return cable;
        }

        private static Terminal GetTerminal(Elemento elemento, int index)
        {
            if (elemento is not ITerminalOwner owner)
                throw new InvalidOperationException($"Elemento '{elemento.Nome}' nao possui terminais.");

            return owner.Terminais[index];
        }

        private static Terminal GetTerminal(Barra barra, string terminalId)
        {
            return barra.Terminais.First(t =>
                string.Equals(t.Id, terminalId, StringComparison.OrdinalIgnoreCase));
        }

        private static void AssertCableEndpointAtTerminal(
            Cabo cable,
            bool origin,
            Elemento elemento,
            int terminalIndex,
            string name)
        {
            Terminal terminal = GetTerminal(elemento, terminalIndex);
            Point vertex = origin
                ? cable.Vertices[0]
                : cable.Vertices[^1];
            string terminalId = origin
                ? cable.OrigemTerminalId
                : cable.DestinoTerminalId;

            AssertEqual(terminal.Id, terminalId, $"{name}.TerminalId");
            AssertEqual(terminal.Posicao.X, vertex.X, $"{name}.Vertice.X");
            AssertEqual(terminal.Posicao.Y, vertex.Y, $"{name}.Vertice.Y");
        }

        private static void AssertHandle(
            CableVertexHandleViewModel handle,
            CaboViewModel cabo,
            int indice,
            Point expected,
            string name)
        {
            Assert(ReferenceEquals(cabo, handle.Cabo), $"{name}.Cabo");
            AssertEqual(indice, handle.Indice, $"{name}.Indice");
            AssertEqual(expected.X, handle.X, $"{name}.X");
            AssertEqual(expected.Y, handle.Y, $"{name}.Y");
        }

        private static void AssertPointEqual(Point expected, Point actual, string name)
        {
            AssertEqual(expected.X, actual.X, $"{name}.X");
            AssertEqual(expected.Y, actual.Y, $"{name}.Y");
        }

        private static void AssertVertices(Cabo cabo, IReadOnlyList<Point> expected, string name)
        {
            AssertEqual(expected.Count, cabo.Vertices.Count, $"{name}.Count");

            for (int i = 0; i < expected.Count; i++)
                AssertPointEqual(expected[i], cabo.Vertices[i], $"{name}[{i}]");
        }

        private static Point MidPoint(Point a, Point b)
        {
            return new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }

        private static void AssertNoDuplicateTerminalIds(Barra barra, string name)
        {
            int idsUnicos = barra.Terminais
                .Select(t => t.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            AssertEqual(barra.Terminais.Count, idsUnicos, $"{name}.TerminalIds unicos");
        }

        private static Terminal AssertTerminalExists(Barra barra, string terminalId)
        {
            Terminal? terminal = barra.Terminais.FirstOrDefault(t =>
                string.Equals(t.Id, terminalId, StringComparison.OrdinalIgnoreCase));

            Assert(terminal != null, $"Terminal '{terminalId}' deve existir na barra '{barra.Nome}'.");
            return terminal!;
        }

        private static void AssertTerminaisDaBarraSeguemPitchFixo(Barra barra, string name)
        {
            double pitch = Barra.ALTURA_PADRAO / 23;

            foreach (Terminal terminal in barra.Terminais)
            {
                int slot = int.Parse(terminal.Id["BARRA-".Length..]) - 1;
                double expected = Math.Min(slot * pitch, barra.Altura);
                AssertEqual(expected, terminal.PosicaoLocal.Y, $"{name}.{terminal.Id}.Y");
            }
        }

        private static void AssertResizePreservaCabo(
            Barra barra,
            Cabo cable,
            string terminalId,
            double alturaEsperada,
            string name)
        {
            AssertEqual(alturaEsperada, barra.Altura, $"{name}.Altura");
            Terminal terminal = AssertTerminalExists(barra, terminalId);
            AssertNoDuplicateTerminalIds(barra, $"{name}.Barra");
            AssertEqual(terminalId, cable.OrigemTerminalId, $"{name}.OrigemTerminalId");
            AssertEqual(terminal.Posicao.X, cable.Vertices[0].X, $"{name}.Cabo.X");
            AssertEqual(terminal.Posicao.Y, cable.Vertices[0].Y, $"{name}.Cabo.Y");
        }

        private static void AssertTerminalsUseCentralPivot(
            Elemento elemento,
            double width,
            double height,
            string name)
        {
            if (elemento is not ITerminalOwner owner)
                throw new InvalidOperationException($"{name}: elemento sem terminais.");

            foreach (Terminal terminal in owner.Terminais)
            {
                Point expected = ExpectedCentralWorld(
                    elemento,
                    terminal.PosicaoLocal,
                    width,
                    height);

                AssertEqual(expected.X, terminal.Posicao.X, $"{name}.{terminal.Id}.X");
                AssertEqual(expected.Y, terminal.Posicao.Y, $"{name}.{terminal.Id}.Y");
            }
        }

        private static Point ExpectedCentralWorld(
            Elemento owner,
            Point local,
            double width,
            double height)
        {
            double scale = owner.Escala == 0 ? 1 : owner.Escala;
            double pivotX = width / 2;
            double pivotY = height / 2;
            double x = (local.X - pivotX) * scale;
            double y = (local.Y - pivotY) * scale;
            double radians = owner.Rotacao * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            return new Point(
                owner.PosicaoX + pivotX + x * cos - y * sin,
                owner.PosicaoY + pivotY + x * sin + y * cos);
        }

        private static void AssertLine(
            ParameterReader.LineData line,
            Cabo cable,
            string expectedFrom,
            string expectedTo,
            string name)
        {
            AssertEqual(cable.Id.ToString(), line.Id, $"{name}.Id");
            AssertEqual(cable.Nome, line.Nome, $"{name}.Nome");
            AssertEqual(expectedFrom, line.Barra1, $"{name}.Barra1");
            AssertEqual(expectedTo, line.Barra2, $"{name}.Barra2");
        }

        private static void AssertContainsNode(
            IEnumerable<ElectricGraphNode> nodes,
            Elemento expected,
            string name)
        {
            bool contains = nodes.Any(n =>
                string.Equals(n.ElementId, expected.Id.ToString(), StringComparison.OrdinalIgnoreCase));

            Assert(contains, $"{name}: no '{expected.Nome}' nao encontrado.");
        }

        private static void AssertSinTerminals(Sin sin, string name)
        {
            AssertEqual(4, sin.Terminais.Count, $"{name}.Terminais.Count");
            AssertEqual(Sin.TERMINAL_NORTE, sin.Terminais[0].Id, $"{name}.Terminal[0]");
            AssertEqual(Sin.TERMINAL_SUL, sin.Terminais[1].Id, $"{name}.Terminal[1]");
            AssertEqual(Sin.TERMINAL_LESTE, sin.Terminais[2].Id, $"{name}.Terminal[2]");
            AssertEqual(Sin.TERMINAL_OESTE, sin.Terminais[3].Id, $"{name}.Terminal[3]");

            foreach (Terminal terminal in sin.Terminais)
                AssertEqual(sin.Barra, terminal.Barra ?? string.Empty, $"{name}.{terminal.Id}.Barra");
        }

        private static void AssertTransformadorTerminals(Transformador transformador, string name)
        {
            AssertEqual(2, transformador.Terminais.Count, $"{name}.Terminais.Count");
            AssertEqual(Transformador.TERMINAL_PRIMARIO, transformador.Terminais[0].Id, $"{name}.Terminal[0]");
            AssertEqual(Transformador.TERMINAL_SECUNDARIO, transformador.Terminais[1].Id, $"{name}.Terminal[1]");

            foreach (Terminal terminal in transformador.Terminais)
                AssertEqual(transformador.Barra, terminal.Barra ?? string.Empty, $"{name}.{terminal.Id}.Barra");
        }

        private static void AssertGraphTerminal(ElectricGraphNode node, string terminalId, string name)
        {
            bool exists = node.Terminals.Any(t =>
                string.Equals(t.TerminalId, terminalId, StringComparison.OrdinalIgnoreCase));

            Assert(exists, $"{name}: terminal '{terminalId}' nao encontrado.");
        }

        private static void AssertCableEndpoint(
            AraciDocument loaded,
            Cabo expectedCable,
            Sin expectedSin,
            string expectedSinTerminalId,
            Carga expectedLoad,
            string name)
        {
            Cabo loadedCable = FindById<Cabo>(loaded, expectedCable.Id);

            AssertEqual(expectedSin.Id.ToString(), loadedCable.OrigemId, $"{name}.OrigemId");
            AssertEqual(expectedLoad.Id.ToString(), loadedCable.DestinoId, $"{name}.DestinoId");
            AssertEqual(expectedSinTerminalId, loadedCable.OrigemTerminalId, $"{name}.OrigemTerminalId");
            AssertEqual(expectedCable.DestinoTerminalId, loadedCable.DestinoTerminalId, $"{name}.DestinoTerminalId");
        }

        private static OperationalGraphState BuildOperationalState(AraciDocument document)
        {
            ElectricGraph graph = new ElectricGraphBuilder(document).Build();
            return new OperationalGraphStateBuilder().Build(graph);
        }

        private static void AssertEnergized(
            OperationalGraphState state,
            Elemento elemento,
            string name)
        {
            Assert(
                state.IsNodeEnergized(elemento.Id.ToString()),
                $"{name}: elemento deveria estar energizado.");
        }

        private static void AssertDeenergized(
            OperationalGraphState state,
            Elemento elemento,
            string name)
        {
            Assert(
                !state.IsNodeEnergized(elemento.Id.ToString()) &&
                state.DeenergizedNodeIds.Contains(elemento.Id.ToString()),
                $"{name}: elemento deveria estar desenergizado.");
        }

        private static void AssertEdgeEnergized(
            OperationalGraphState state,
            Cabo cabo,
            string name)
        {
            Assert(
                state.IsEdgeEnergized(cabo.Id.ToString()),
                $"{name}: cabo deveria estar energizado.");
        }

        private static void AssertEdgeDeenergized(
            OperationalGraphState state,
            Cabo cabo,
            string name)
        {
            Assert(
                !state.IsEdgeEnergized(cabo.Id.ToString()) &&
                state.DeenergizedEdgeIds.Contains(cabo.Id.ToString()),
                $"{name}: cabo deveria estar desenergizado.");
        }

        private static void AssertCircuitDtoParametrosReais(
            CircuitDto dto,
            Sin sin,
            Transformador transformador,
            Carga load,
            string name)
        {
            TransformerDto transformerDto = dto.Transformers.Single();
            LoadDto loadDto = dto.Loads.Single();

            AssertEqual(sin.Id.ToString(), dto.Slack.Id, $"{name}.Slack.Id");
            AssertEqual(138, dto.Slack.Tensao, $"{name}.Slack.Tensao");
            AssertEqual($"{transformador.Nome}_PRIMARIO", transformerDto.BarraPrimario, $"{name}.Transformer.BarraPrimario");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", transformerDto.BarraSecundario, $"{name}.Transformer.BarraSecundario");
            AssertEqual(138, transformerDto.TensaoPrimarioKV, $"{name}.Transformer.TensaoPrimarioKV");
            AssertEqual(34.5, transformerDto.TensaoSecundarioKV, $"{name}.Transformer.TensaoSecundarioKV");
            AssertEqual(65000, transformerDto.PotenciaKVA, $"{name}.Transformer.PotenciaKVA");
            AssertEqual(1, transformerDto.RPercentual, $"{name}.Transformer.RPercentual");
            AssertEqual(8, transformerDto.XPercentual, $"{name}.Transformer.XPercentual");
            AssertEqual("Wye", transformerDto.LigacaoPrimario, $"{name}.Transformer.LigacaoPrimario");
            AssertEqual("Wye", transformerDto.LigacaoSecundario, $"{name}.Transformer.LigacaoSecundario");
            AssertEqual(34.5, loadDto.Tensao, $"{name}.Load.Tensao");
            AssertEqual(5000, loadDto.PotenciaAtiva, $"{name}.Load.PotenciaAtiva");
            AssertEqual(1000, loadDto.PotenciaReativa, $"{name}.Load.PotenciaReativa");
            AssertEqual(sin.Nome, dto.Lines[0].Barra1, $"{name}.LinePrimario.Barra1");
            AssertEqual($"{transformador.Nome}_PRIMARIO", dto.Lines[0].Barra2, $"{name}.LinePrimario.Barra2");
            AssertEqual($"{transformador.Nome}_SECUNDARIO", dto.Lines[1].Barra1, $"{name}.LineSecundario.Barra1");
            AssertEqual(load.Nome, dto.Lines[1].Barra2, $"{name}.LineSecundario.Barra2");
        }

        private static void TabelaRemoveFiltrosCamposRemovidosComUndoRedo()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new EditarPropriedadesTabelaUseCase(document, commands);
            ProjectTable tabela = document.CriarNovaTabela();

            var campoNome = new ProjectTableFieldSelection { Categoria = ProjectTableElementCategory.Barras, CampoId = "Nome", NomeExibicao = "Nome", Ordem = 0 };
            var campoTensao = new ProjectTableFieldSelection { Categoria = ProjectTableElementCategory.Barras, CampoId = "Tensao", NomeExibicao = "Tensao", Ordem = 1 };
            tabela.CategoriasElementos = new List<ProjectTableElementCategory> { ProjectTableElementCategory.Barras };
            tabela.CamposSelecionados = new List<ProjectTableFieldSelection> { campoNome, campoTensao };
            tabela.Filtros = new List<ProjectTableFilterRule>
            {
                new() { Ordem = 0, Categoria = ProjectTableElementCategory.Barras, CampoId = "Nome", NomeExibicao = "Nome", Operador = ProjectTableFilterOperator.Contem, Valor = "A" },
                new() { Ordem = 1, Categoria = ProjectTableElementCategory.Barras, CampoId = "Tensao", NomeExibicao = "Tensao", Operador = ProjectTableFilterOperator.IgualA, Valor = "13" }
            };

            bool alterado = useCase.AlterarElementosTabela(
                tabela.Id,
                tabela.CategoriasElementos,
                new[] { campoNome });

            Assert(alterado, "AlterarElementosTabela deveria retornar true.");
            AssertEqual(1, tabela.Filtros.Count, "Tabela.Filtros.Count apos remover campo");
            AssertEqual("Nome", tabela.Filtros[0].CampoId, "Tabela.Filtros[0].CampoId");
            AssertEqual(ProjectTableFilterOperator.Contem, tabela.Filtros[0].Operador, "Tabela.Filtros[0].Operador");
            AssertEqual("A", tabela.Filtros[0].Valor, "Tabela.Filtros[0].Valor");
            AssertEqual(0, tabela.Filtros[0].Ordem, "Tabela.Filtros[0].Ordem");

            commands.Undo();

            AssertEqual(2, tabela.CamposSelecionados.Count, "Undo.CamposSelecionados.Count");
            AssertEqual(2, tabela.Filtros.Count, "Undo.Filtros.Count");
            AssertEqual("Tensao", tabela.Filtros[1].CampoId, "Undo.Filtros[1].CampoId");

            commands.Redo();

            AssertEqual(1, tabela.CamposSelecionados.Count, "Redo.CamposSelecionados.Count");
            AssertEqual(1, tabela.Filtros.Count, "Redo.Filtros.Count");
            AssertEqual("Nome", tabela.Filtros[0].CampoId, "Redo.Filtros[0].CampoId");
        }

        private static void ExcluirVistaLimpaFiltroTabelaComUndoRedo()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new ExcluirItemProjetoUseCase(document, commands);
            ProjectView vistaA = document.Vistas[0];
            ProjectView vistaB = document.CriarNovaVista();
            ProjectTable tabelaAfetada = document.CriarNovaTabela();
            ProjectTable tabelaOutraVista = document.CriarNovaTabela();
            ProjectTable tabelaTodas = document.CriarNovaTabela();

            tabelaAfetada.FiltroVistaId = vistaB.Id;
            tabelaOutraVista.FiltroVistaId = vistaA.Id;
            tabelaTodas.FiltroVistaId = null;

            bool excluiu = useCase.ExcluirVista(vistaB.Id);

            Assert(excluiu, "ExcluirVista deveria retornar true.");
            Assert(!document.Vistas.Any(v => v.Id == vistaB.Id), "Vista B deveria ser removida.");
            AssertEqual(null, tabelaAfetada.FiltroVistaId, "Tabela afetada deveria voltar para todas as vistas.");
            AssertEqual(vistaA.Id, tabelaOutraVista.FiltroVistaId, "Tabela de outra vista nao deveria mudar.");
            AssertEqual(null, tabelaTodas.FiltroVistaId, "Tabela sem filtro deveria continuar sem filtro.");

            commands.Undo();

            Assert(document.Vistas.Any(v => v.Id == vistaB.Id), "Undo deveria restaurar Vista B.");
            AssertEqual(vistaB.Id, tabelaAfetada.FiltroVistaId, "Undo deveria restaurar filtro da Vista B.");

            commands.Redo();

            Assert(!document.Vistas.Any(v => v.Id == vistaB.Id), "Redo deveria remover Vista B novamente.");
            AssertEqual(null, tabelaAfetada.FiltroVistaId, "Redo deveria limpar filtro da Vista B novamente.");
        }

        private static void TabelaAlteraMultiplasOrdenacoesComUndoRedo()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new EditarPropriedadesTabelaUseCase(document, commands);
            ProjectTable tabela = CriarTabelaComCampos(document);

            bool alterado = useCase.AlterarOrdenacaoTabela(
                tabela.Id,
                CriarOrdenacoes(tabela, "Tensao", "Nome"));

            Assert(alterado, "AlterarOrdenacaoTabela deveria retornar true.");
            AssertEqual(2, tabela.Ordenacoes.Count, "Ordenacoes.Count");
            AssertEqual("Tensao", tabela.Ordenacoes[0].CampoId, "Ordenacoes[0].CampoId");
            AssertEqual("Nome", tabela.Ordenacoes[1].CampoId, "Ordenacoes[1].CampoId");
            AssertEqual(0, tabela.Ordenacoes[0].Ordem, "Ordenacoes[0].Ordem");
            AssertEqual(1, tabela.Ordenacoes[1].Ordem, "Ordenacoes[1].Ordem");

            commands.Undo();
            AssertEqual(0, tabela.Ordenacoes.Count, "Undo deveria limpar ordenacao inicial.");

            commands.Redo();
            AssertEqual(2, tabela.Ordenacoes.Count, "Redo deveria restaurar ordenacoes.");
            AssertEqual("Nome", tabela.Ordenacoes[1].CampoId, "Redo.Ordenacoes[1].CampoId");
        }

        private static void TabelaLimitaOrdenacaoACincoRegras()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new EditarPropriedadesTabelaUseCase(document, commands);
            ProjectTable tabela = CriarTabelaComCampos(document);

            useCase.AlterarOrdenacaoTabela(
                tabela.Id,
                CriarOrdenacoes(tabela, "Nome", "Tensao", "Corrente", "PotenciaAtiva", "Tipo", "Comprimento"));

            AssertEqual(5, tabela.Ordenacoes.Count, "Ordenacoes deveria ser limitada a cinco regras.");
            AssertEqual("Tipo", tabela.Ordenacoes[4].CampoId, "Quinta regra preservada.");
            Assert(tabela.Ordenacoes.Select((o, index) => o.Ordem == index).All(ok => ok), "Ordenacoes deveriam ser reindexadas.");
        }

        private static void TabelaRemoveOrdenacoesDuplicadas()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new EditarPropriedadesTabelaUseCase(document, commands);
            ProjectTable tabela = CriarTabelaComCampos(document);

            useCase.AlterarOrdenacaoTabela(
                tabela.Id,
                CriarOrdenacoes(tabela, "Nome", "Tensao", "Nome", "Corrente"));

            AssertEqual(3, tabela.Ordenacoes.Count, "Duplicidade deveria ser removida.");
            AssertEqual("Nome", tabela.Ordenacoes[0].CampoId, "Primeira ocorrencia duplicada deveria ser preservada.");
            AssertEqual("Tensao", tabela.Ordenacoes[1].CampoId, "Ordem apos remover duplicidade.");
            AssertEqual("Corrente", tabela.Ordenacoes[2].CampoId, "Regra valida posterior deveria ser preservada.");
        }

        private static void TabelaLimpaApenasOrdenacaoDeCampoRemovido()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new EditarPropriedadesTabelaUseCase(document, commands);
            ProjectTable tabela = CriarTabelaComCampos(document);
            tabela.Ordenacoes = CriarOrdenacoes(tabela, "Nome", "Tensao", "Corrente").ToList();

            useCase.AlterarElementosTabela(
                tabela.Id,
                tabela.CategoriasElementos,
                tabela.CamposSelecionados.Where(c => c.CampoId != "Tensao").ToList());

            AssertEqual(2, tabela.Ordenacoes.Count, "Apenas uma ordenacao deveria ser removida.");
            AssertEqual("Nome", tabela.Ordenacoes[0].CampoId, "Ordenacao valida anterior preservada.");
            AssertEqual("Corrente", tabela.Ordenacoes[1].CampoId, "Ordenacao valida posterior preservada.");
            AssertEqual(1, tabela.Ordenacoes[1].Ordem, "Ordenacao remanescente deveria ser reindexada.");

            commands.Undo();
            AssertEqual(3, tabela.Ordenacoes.Count, "Undo deveria restaurar todas as ordenacoes.");
            AssertEqual("Tensao", tabela.Ordenacoes[1].CampoId, "Undo.Ordenacoes[1].CampoId");

            commands.Redo();
            AssertEqual(2, tabela.Ordenacoes.Count, "Redo deveria remover novamente apenas a regra invalida.");
            Assert(tabela.Ordenacoes.All(o => o.CampoId != "Tensao"), "Redo nao deveria manter regra do campo removido.");
        }

        private static void TabelaDuplicaEPersisteMultiplasOrdenacoes()
        {
            var document = new AraciDocument();
            ProjectTable tabela = CriarTabelaComCampos(document);
            tabela.Ordenacoes = CriarOrdenacoes(tabela, "Nome", "Tensao", "Corrente").ToList();

            ProjectTable duplicata = document.CriarDuplicataTabela(tabela);

            AssertEqual(3, duplicata.Ordenacoes.Count, "Duplicata deveria copiar ordenacoes.");
            AssertEqual("Tensao", duplicata.Ordenacoes[1].CampoId, "Duplicata.Ordenacoes[1].CampoId");
            duplicata.Ordenacoes[1].CampoId = "Alterado";
            AssertEqual("Tensao", tabela.Ordenacoes[1].CampoId, "Duplicata deveria usar copia profunda.");

            string path = CreateTempProjectPath();

            try
            {
                var context = new EditorContext();
                ProjectTable persistentTable = context.Document.CriarNovaTabela();
                persistentTable.CategoriasElementos = tabela.CategoriasElementos.ToList();
                persistentTable.CamposSelecionados = CopiarCamposTabela(tabela.CamposSelecionados).ToList();
                persistentTable.Ordenacoes = CriarOrdenacoes(tabela, "Nome", "Tensao", "Corrente").ToList();

                context.Projects.Salvar(path);

                var loadedContext = new EditorContext();
                loadedContext.Projects.Abrir(path);
                ProjectTable loadedTable = loadedContext.Document.Tabelas.Single(t => t.Id == persistentTable.Id);

                AssertEqual(3, loadedTable.Ordenacoes.Count, "Reload deveria preservar multiplas ordenacoes.");
                AssertEqual("Nome", loadedTable.Ordenacoes[0].CampoId, "Reload.Ordenacoes[0].CampoId");
                AssertEqual("Corrente", loadedTable.Ordenacoes[2].CampoId, "Reload.Ordenacoes[2].CampoId");
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void TabelaConverteOrdenacaoUnicaLegada()
        {
            var dto = new ProjectFileDto
            {
                Version = ProjectSerializer.CurrentVersion,
                AppName = ProjectSerializer.AppName,
                ProjectName = "Tabela legada",
                CreatedAt = DateTimeOffset.UtcNow,
                SavedAt = DateTimeOffset.UtcNow,
                Tables =
                {
                    new ProjectTableDto
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Tabela legada",
                        CategoriasElementos = { ProjectTableElementCategory.Barras.ToString() },
                        CamposSelecionados =
                        {
                            new ProjectTableFieldSelectionDto
                            {
                                Categoria = ProjectTableElementCategory.Barras.ToString(),
                                CampoId = "Nome",
                                NomeExibicao = "Nome",
                                Ordem = 0
                            }
                        },
                        Ordenacao = new ProjectTableSortingDto
                        {
                            Categoria = ProjectTableElementCategory.Barras.ToString(),
                            CampoId = "Nome",
                            NomeExibicao = "Nome",
                            Direcao = ProjectTableSortDirection.Decrescente.ToString()
                        }
                    }
                }
            };

            string path = CreateTempProjectPath();

            try
            {
                File.WriteAllText(path, JsonSerializer.Serialize(dto));

                var context = new EditorContext();
                context.Projects.Abrir(path);
                ProjectTable tabela = context.Document.Tabelas.Single();

                AssertEqual(1, tabela.Ordenacoes.Count, "Ordenacao unica legada deveria virar lista com uma regra.");
                AssertEqual("Nome", tabela.Ordenacoes[0].CampoId, "Ordenacao legada CampoId.");
                AssertEqual(ProjectTableSortDirection.Decrescente, tabela.Ordenacoes[0].Direcao, "Ordenacao legada Direcao.");
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static void TabelaDataBuilderGeraColunasPorCamposSelecionados()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            AssertEqual(3, result.Columns.Count, "Columns.Count");
            AssertEqual("Nome", result.Columns[0].CampoId, "Columns[0].CampoId");
            AssertEqual("PotenciaAtiva", result.Columns[1].CampoId, "Columns[1].CampoId");
            AssertEqual("Tensao", result.Columns[2].CampoId, "Columns[2].CampoId");
        }

        private static void TabelaDataBuilderGeraLinhasPorCategorias()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            AssertEqual(3, result.Rows.Count, "Rows.Count");
            Assert(result.Rows.All(r => r.Categoria == ProjectTableElementCategory.Cargas), "Todas as linhas deveriam ser Cargas.");
        }

        private static void TabelaDataBuilderRespeitaFiltroVista()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            ProjectView vistaB = document.Vistas.Single(v => v.Nome == "Vista filtro");
            tabela.FiltroVistaId = vistaB.Id;

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            AssertEqual(1, result.Rows.Count, "FiltroVista Rows.Count");
            AssertEqual("Carga B", result.Rows[0].ElementoNome, "FiltroVista ElementoNome");
        }

        private static void TabelaDataBuilderAplicaFiltroTodas()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.ModoFiltro = ProjectTableFilterLogicalMode.Todas;
            tabela.Filtros = new List<ProjectTableFilterRule>
            {
                new() { Ordem = 0, Categoria = ProjectTableElementCategory.Cargas, CampoId = "Nome", NomeExibicao = "Nome", Operador = ProjectTableFilterOperator.Contem, Valor = "Carga" },
                new() { Ordem = 1, Categoria = ProjectTableElementCategory.Cargas, CampoId = "PotenciaAtiva", NomeExibicao = "Potencia ativa", Operador = ProjectTableFilterOperator.IgualA, Valor = "800" }
            };

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            AssertEqual(1, result.Rows.Count, "Filtro Todas Rows.Count");
            AssertEqual("Carga B", result.Rows[0].ElementoNome, "Filtro Todas ElementoNome");
        }

        private static void TabelaDataBuilderAplicaFiltroQualquer()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.ModoFiltro = ProjectTableFilterLogicalMode.Qualquer;
            tabela.Filtros = new List<ProjectTableFilterRule>
            {
                new() { Ordem = 0, Categoria = ProjectTableElementCategory.Cargas, CampoId = "Nome", NomeExibicao = "Nome", Operador = ProjectTableFilterOperator.IgualA, Valor = "Carga C" },
                new() { Ordem = 1, Categoria = ProjectTableElementCategory.Cargas, CampoId = "PotenciaAtiva", NomeExibicao = "Potencia ativa", Operador = ProjectTableFilterOperator.IgualA, Valor = "800" }
            };

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            AssertEqual(2, result.Rows.Count, "Filtro Qualquer Rows.Count");
            Assert(result.Rows.Any(r => r.ElementoNome == "Carga B"), "Filtro Qualquer deveria incluir Carga B.");
            Assert(result.Rows.Any(r => r.ElementoNome == "Carga C"), "Filtro Qualquer deveria incluir Carga C.");
        }

        private static void TabelaDataBuilderAplicaOrdenacaoMultipla()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.Ordenacoes = new List<ProjectTableSorting>
            {
                new() { Ordem = 0, Categoria = ProjectTableElementCategory.Cargas, CampoId = "PotenciaAtiva", NomeExibicao = "Potencia ativa", Direcao = ProjectTableSortDirection.Crescente },
                new() { Ordem = 1, Categoria = ProjectTableElementCategory.Cargas, CampoId = "Nome", NomeExibicao = "Nome", Direcao = ProjectTableSortDirection.Decrescente }
            };

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            AssertEqual("Carga C", result.Rows[0].ElementoNome, "Ordenacao Rows[0]");
            AssertEqual("Carga A", result.Rows[1].ElementoNome, "Ordenacao Rows[1]");
            AssertEqual("Carga B", result.Rows[2].ElementoNome, "Ordenacao Rows[2]");
        }

        private static void TabelaDataBuilderSemOrdenacaoPreservaOrdem()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            AssertEqual("Carga A", result.Rows[0].ElementoNome, "Sem ordenacao Rows[0]");
            AssertEqual("Carga B", result.Rows[1].ElementoNome, "Sem ordenacao Rows[1]");
            AssertEqual("Carga C", result.Rows[2].ElementoNome, "Sem ordenacao Rows[2]");
        }

        private static void TabelaDataBuilderIgnoraOrdenacaoInvalidaSemAlterarTabela()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.Ordenacoes = new List<ProjectTableSorting>
            {
                new() { Ordem = 0, Categoria = ProjectTableElementCategory.Cargas, CampoId = "CampoInexistente", NomeExibicao = "Campo inexistente", Direcao = ProjectTableSortDirection.Decrescente }
            };

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            AssertEqual("Carga A", result.Rows[0].ElementoNome, "Ordenacao invalida deveria preservar ordem.");
            AssertEqual(1, tabela.Ordenacoes.Count, "Builder nao deveria alterar Ordenacoes.");
            AssertEqual("CampoInexistente", tabela.Ordenacoes[0].CampoId, "Builder nao deveria normalizar ProjectTable.");
        }

        private static void TabelaDataBuilderGeracaoRepetidaReadOnly()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            string assinaturaAntes = CriarAssinaturaTabelaDados(document, tabela);
            var builder = new ProjectTableDataBuilder();

            builder.Build(document, tabela);
            builder.Build(document, tabela);

            string assinaturaDepois = CriarAssinaturaTabelaDados(document, tabela);
            AssertEqual(assinaturaAntes, assinaturaDepois, "Geracao da tabela deveria ser read-only.");
        }

        private static void TabelaDataBuilderIgnoraCategoriaNaoSelecionada()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);

            ProjectTableDataResult result = new ProjectTableDataBuilder().Build(document, tabela);

            Assert(result.Rows.All(r => r.ElementoNome != "Gerador A"), "Categoria nao selecionada nao deveria aparecer.");
        }

        private static void TabelaCsvExportaCabecalhosLinhasEDisplayValue()
        {
            var result = new ProjectTableDataResult(
                new[]
                {
                    new ProjectTableDataColumn(ProjectTableElementCategory.Cargas, "Nome", "Nome", 0),
                    new ProjectTableDataColumn(ProjectTableElementCategory.Cargas, "Corrente", "Corrente", 1)
                },
                new[]
                {
                    new ProjectTableDataRow(
                        Guid.NewGuid(),
                        "Carga 1",
                        ProjectTableElementCategory.Cargas,
                        new[]
                        {
                            new ProjectTableDataCell(ProjectTableElementCategory.Cargas, "Nome", "Nome", "raw diferente", "Carga 1"),
                            new ProjectTableDataCell(ProjectTableElementCategory.Cargas, "Corrente", "Corrente", 611.04, "611.04∠0°")
                        })
                });

            string csv = new ProjectTableCsvExportService().GenerateCsv(result);

            AssertEqual("Nome;Corrente\r\nCarga 1;611.04∠0°", NormalizarQuebras(csv), "CSV cabecalho linhas display value");
            Assert(!csv.Contains("raw diferente"), "CSV deveria usar DisplayValue, nao RawValue.");
        }

        private static void TabelaCsvEscapaDelimitadorAspasEQuebra()
        {
            var result = new ProjectTableDataResult(
                new[]
                {
                    new ProjectTableDataColumn(ProjectTableElementCategory.Cargas, "Nome", "Nome", 0),
                    new ProjectTableDataColumn(ProjectTableElementCategory.Cargas, "Observacao", "Observação", 1)
                },
                new[]
                {
                    new ProjectTableDataRow(
                        Guid.NewGuid(),
                        "Carga 1",
                        ProjectTableElementCategory.Cargas,
                        new[]
                        {
                            new ProjectTableDataCell(ProjectTableElementCategory.Cargas, "Nome", "Nome", null, "Carga;1"),
                            new ProjectTableDataCell(ProjectTableElementCategory.Cargas, "Observacao", "Observação", null, "texto com \"aspas\"\ne quebra")
                        })
                });

            string csv = new ProjectTableCsvExportService().GenerateCsv(result);

            AssertEqual(
                NormalizarQuebras("Nome;Observação\n\"Carga;1\";\"texto com \"\"aspas\"\"\ne quebra\""),
                NormalizarQuebras(csv),
                "CSV escaping");
        }

        private static void TabelaCsvExportacaoRespeitaBuilderENaoAlteraEstado()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.Filtros = new List<ProjectTableFilterRule>
            {
                new()
                {
                    Ordem = 0,
                    Categoria = ProjectTableElementCategory.Cargas,
                    CampoId = "Nome",
                    NomeExibicao = "Nome",
                    Operador = ProjectTableFilterOperator.IgualA,
                    Valor = "Carga B"
                }
            };
            string assinaturaAntes = CriarAssinaturaTabelaDados(document, tabela);
            string path = Path.Combine(Path.GetTempPath(), $"araci-table-export-{Guid.NewGuid():N}.csv");
            var dialogs = new FakeDialogService { SaveCsvPath = path };
            var useCase = new ExportarTabelaUseCase(document, dialogs);

            try
            {
                bool exportou = useCase.Executar(tabela);
                string csv = File.ReadAllText(path, Encoding.UTF8);
                string assinaturaDepois = CriarAssinaturaTabelaDados(document, tabela);

                Assert(exportou, "Exportacao deveria retornar true.");
                AssertEqual("Nome;Potencia ativa;Tensao\r\nCarga B;800;13.8", NormalizarQuebras(csv), "CSV filtrado pelo builder");
                AssertEqual(assinaturaAntes, assinaturaDepois, "Exportacao nao deveria alterar Document ou ProjectTable.");
                AssertEqual(1, dialogs.SaveCsvChamadas, "ShowSaveCsvDialog chamadas");
                AssertEqual(1, dialogs.InfoChamadas, "ShowInfo chamadas apos sucesso");
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private static void TabelaCsvUseCaseCancelaSemEscrever()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            string path = Path.Combine(Path.GetTempPath(), $"araci-table-export-cancel-{Guid.NewGuid():N}.csv");
            var dialogs = new FakeDialogService { SaveCsvPath = null };
            var useCase = new ExportarTabelaUseCase(document, dialogs);

            bool exportou = useCase.Executar(tabela);

            Assert(!exportou, "Exportacao cancelada deveria retornar false.");
            Assert(!File.Exists(path), "Cancelamento nao deveria criar arquivo.");
            AssertEqual(1, dialogs.SaveCsvChamadas, "ShowSaveCsvDialog chamadas no cancelamento");
            AssertEqual(0, dialogs.InfoChamadas, "Cancelamento nao deveria mostrar sucesso.");
            AssertEqual(0, dialogs.ErrorChamadas, "Cancelamento nao deveria mostrar erro.");
        }

        private static void TabelaCsvUseCaseAvisaSemTabela()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            var dialogs = new FakeDialogService();
            var useCase = new ExportarTabelaUseCase(document, dialogs);

            bool exportou = useCase.Executar(null);

            Assert(!exportou, "Exportacao sem tabela deveria retornar false.");
            AssertEqual(1, dialogs.WarningChamadas, "Warning sem tabela");
            AssertEqual("Selecione uma tabela para exportar.", dialogs.LastWarningMessage, "Mensagem sem tabela");
            AssertEqual(0, dialogs.SaveCsvChamadas, "Sem tabela nao deveria abrir dialogo de salvar.");
        }

        private static void TabelaCsvUseCaseMostraErroDeEscrita()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            string assinaturaAntes = CriarAssinaturaTabelaDados(document, tabela);
            var dialogs = new FakeDialogService { SaveCsvPath = Path.Combine(Path.GetTempPath(), $"araci-table-export-error-{Guid.NewGuid():N}.csv") };
            var useCase = new ExportarTabelaUseCase(
                document,
                dialogs,
                new ProjectTableDataBuilder(),
                new ThrowingCsvExportService());

            bool exportou = useCase.Executar(tabela);

            Assert(!exportou, "Exportacao com erro deveria retornar false.");
            AssertEqual(1, dialogs.ErrorChamadas, "Erro de escrita deveria mostrar mensagem amigavel.");
            Assert(dialogs.LastErrorMessage?.Contains("Não foi possível salvar o arquivo CSV") == true, "Mensagem de erro deveria explicar falha ao salvar.");
            AssertEqual(assinaturaAntes, CriarAssinaturaTabelaDados(document, tabela), "Erro de exportacao nao deveria alterar estado.");
        }

        private static void PranchaNovaIniciaSemInstanciasTabela()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            List<ProjectSheetTableInstance> tabelas = prancha.Tabelas ?? throw new InvalidOperationException("ProjectSheet.Tabelas null.");

            AssertEqual(0, tabelas.Count, "ProjectSheet.Tabelas.Count inicial");
        }

        private static void ProjectSheetPossuiDefaultsValidosFolha()
        {
            var prancha = new ProjectSheet();

            AssertEqual(ProjectSheetFormat.A1, prancha.FormatoFolha, "ProjectSheet.FormatoFolha padrao");
            AssertEqual(ProjectSheetOrientation.Paisagem, prancha.OrientacaoFolha, "ProjectSheet.OrientacaoFolha padrao");
            AssertEqual(ProjectSheet.DefaultWidth, prancha.LarguraFolha, "ProjectSheet.LarguraFolha padrao");
            AssertEqual(ProjectSheet.DefaultHeight, prancha.AlturaFolha, "ProjectSheet.AlturaFolha padrao");

            prancha.LarguraFolha = double.NaN;
            prancha.AlturaFolha = double.PositiveInfinity;

            AssertEqual(ProjectSheet.DefaultWidth, prancha.LarguraFolha, "ProjectSheet.LarguraFolha invalida usa fallback");
            AssertEqual(ProjectSheet.DefaultHeight, prancha.AlturaFolha, "ProjectSheet.AlturaFolha invalida usa fallback");

            (double larguraA3Retrato, double alturaA3Retrato) = ProjectSheet.ObterDimensoesFormato(ProjectSheetFormat.A3, ProjectSheetOrientation.Retrato);

            AssertEqual(397, larguraA3Retrato, "A3 retrato largura");
            AssertEqual(561, alturaA3Retrato, "A3 retrato altura");
        }

        private static void ProjectSheetTypePossuiDefaultsValidos()
        {
            var tipo = new ProjectSheetType();

            Assert(tipo.Id != Guid.Empty, "ProjectSheetType.Id padrao");
            AssertEqual(ProjectSheetType.DefaultName, tipo.Nome, "ProjectSheetType.Nome padrao");
            AssertEqual(ProjectSheetFormat.A1, tipo.FormatoFolha, "ProjectSheetType.FormatoFolha padrao");
            AssertEqual(ProjectSheetOrientation.Paisagem, tipo.OrientacaoFolha, "ProjectSheetType.OrientacaoFolha padrao");
            AssertEqual(ProjectSheet.DefaultWidth, tipo.LarguraFolha, "ProjectSheetType.LarguraFolha padrao");
            AssertEqual(ProjectSheet.DefaultHeight, tipo.AlturaFolha, "ProjectSheetType.AlturaFolha padrao");

            tipo.LarguraFolha = double.NaN;
            tipo.AlturaFolha = double.NegativeInfinity;

            AssertEqual(ProjectSheet.DefaultWidth, tipo.LarguraFolha, "ProjectSheetType.LarguraFolha invalida usa fallback");
            AssertEqual(ProjectSheet.DefaultHeight, tipo.AlturaFolha, "ProjectSheetType.AlturaFolha invalida usa fallback");
        }

        private static void AraciDocumentNovoPossuiTipoPadraoPrancha()
        {
            var document = new AraciDocument();

            AssertEqual(1, document.TiposPrancha.Count, "TiposPrancha.Count inicial");
            Assert(document.TipoPranchaPadrao.Id != Guid.Empty, "TipoPranchaPadrao.Id");
            AssertEqual(ProjectSheetType.DefaultName, document.TipoPranchaPadrao.Nome, "TipoPranchaPadrao.Nome");
        }

        private static void CriarPranchaAssociaPranchaAoTipoPadrao()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new CriarItemProjetoUseCase(document, commands);
            Guid tipoPadraoId = document.TipoPranchaPadrao.Id;

            ProjectSheet prancha = useCase.CriarPrancha();

            AssertEqual(tipoPadraoId, prancha.SheetTypeId ?? Guid.Empty, "CriarPrancha SheetTypeId");
            AssertEqual(1, document.TiposPrancha.Count, "CriarPrancha nao cria novo tipo");
        }

        private static void PranchaPersisteInstanciaTabela()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            Guid instanciaId = Guid.NewGuid();
            prancha.Tabelas.Add(new ProjectSheetTableInstance
            {
                Id = instanciaId,
                TableId = tabela.Id,
                X = 12.5,
                Y = -8.25,
                Width = 210,
                Height = 95,
                RowStartIndex = 4,
                RowCount = 7
            });
            EditorContext context = new();
            var serializer = new ProjectSerializer(
                context.Elements,
                new ElementoModelFactory(context.Elements),
                context.TerminalLayout,
                context.Geometry);

            ProjectFileDto dto = serializer.CreateFileDto(
                document,
                ProjectMetadataDto.CreateNew("Prancha tabela"),
                context.Settings.Units);
            ProjectSheetTableInstanceDto instanceDto = dto.Sheets.Single(s => s.Id == prancha.Id).Tabelas.Single();
            string json = serializer.Serialize(dto);
            ProjectFileDto reloadedDto = serializer.Deserialize(json);
            IReadOnlyList<ProjectTable> tabelas = serializer.CreateProjectTables(reloadedDto);
            ProjectSheet reloaded = serializer.CreateProjectSheets(reloadedDto, tabelas.Select(t => t.Id)).Single(s => s.Id == prancha.Id);
            ProjectSheetTableInstance instance = reloaded.Tabelas.Single();

            AssertEqual(instanciaId, instanceDto.Id, "DTO instancia Id");
            AssertEqual(tabela.Id, instanceDto.TableId, "DTO instancia TableId");
            AssertEqual(12.5, instanceDto.X, "DTO instancia X");
            AssertEqual(-8.25, instanceDto.Y, "DTO instancia Y");
            AssertEqual(210, instanceDto.Width, "DTO instancia Width");
            AssertEqual(95, instanceDto.Height, "DTO instancia Height");
            AssertEqual(4, instanceDto.RowStartIndex, "DTO instancia RowStartIndex");
            AssertEqual(7, instanceDto.RowCount, "DTO instancia RowCount");
            AssertEqual(instanciaId, instance.Id, "Reload instancia Id");
            AssertEqual(tabela.Id, instance.TableId, "Reload instancia TableId");
            AssertEqual(12.5, instance.X, "Reload instancia X");
            AssertEqual(-8.25, instance.Y, "Reload instancia Y");
            AssertEqual(210, instance.Width, "Reload instancia Width");
            AssertEqual(95, instance.Height, "Reload instancia Height");
            AssertEqual(4, instance.RowStartIndex, "Reload instancia RowStartIndex");
            AssertEqual(7, instance.RowCount, "Reload instancia RowCount");
        }

        private static void PersistenciaPreservaPropriedadesPrancha()
        {
            var serializer = CriarProjectSerializerTeste();
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Numero = "EL-101";
            prancha.Nome = "Distribuicao geral";
            prancha.FormatoFolha = ProjectSheetFormat.Personalizado;
            prancha.OrientacaoFolha = ProjectSheetOrientation.Retrato;
            prancha.LarguraFolha = 640;
            prancha.AlturaFolha = 900;
            prancha.Tabelas.Add(new ProjectSheetTableInstance
            {
                TableId = tabela.Id,
                RowStartIndex = 2,
                RowCount = 3
            });

            EditorContext context = new();
            ProjectFileDto dto = serializer.CreateFileDto(document, ProjectMetadataDto.CreateNew("Prancha propriedades"), context.Settings.Units);
            ProjectSheetDto sheetDto = dto.Sheets.Single(s => s.Id == prancha.Id);
            string json = serializer.Serialize(dto);
            ProjectFileDto reloadedDto = serializer.Deserialize(json);
            IReadOnlyList<ProjectTable> tabelas = serializer.CreateProjectTables(reloadedDto);
            ProjectSheet reloaded = serializer.CreateProjectSheets(reloadedDto, tabelas.Select(t => t.Id)).Single(s => s.Id == prancha.Id);

            AssertEqual("EL-101", sheetDto.Numero, "DTO prancha Numero");
            AssertEqual("Distribuicao geral", sheetDto.Nome, "DTO prancha Nome");
            AssertEqual(ProjectSheetFormat.Personalizado.ToString(), sheetDto.FormatoFolha, "DTO prancha FormatoFolha");
            AssertEqual(ProjectSheetOrientation.Retrato.ToString(), sheetDto.OrientacaoFolha, "DTO prancha OrientacaoFolha");
            AssertEqual(640, sheetDto.LarguraFolha ?? 0, "DTO prancha LarguraFolha");
            AssertEqual(900, sheetDto.AlturaFolha ?? 0, "DTO prancha AlturaFolha");
            AssertEqual("EL-101", reloaded.Numero, "Reload prancha Numero");
            AssertEqual("Distribuicao geral", reloaded.Nome, "Reload prancha Nome");
            AssertEqual(ProjectSheetFormat.Personalizado, reloaded.FormatoFolha, "Reload prancha FormatoFolha");
            AssertEqual(ProjectSheetOrientation.Retrato, reloaded.OrientacaoFolha, "Reload prancha OrientacaoFolha");
            AssertEqual(640, reloaded.LarguraFolha, "Reload prancha LarguraFolha");
            AssertEqual(900, reloaded.AlturaFolha, "Reload prancha AlturaFolha");
            AssertEqual(2, reloaded.Tabelas.Single().RowStartIndex, "Reload preserva RowStartIndex");
            AssertEqual(3, reloaded.Tabelas.Single().RowCount, "Reload preserva RowCount");
        }

        private static void PersistenciaPreservaTiposPrancha()
        {
            var serializer = CriarProjectSerializerTeste();
            var document = new AraciDocument();
            var tipo = new ProjectSheetType
            {
                Nome = "A3 Retrato - Obra",
                FormatoFolha = ProjectSheetFormat.A3,
                OrientacaoFolha = ProjectSheetOrientation.Retrato,
                LarguraFolha = 397,
                AlturaFolha = 561
            };
            document.SubstituirTiposPrancha(new[] { tipo });
            EditorContext context = new();

            ProjectFileDto dto = serializer.CreateFileDto(document, ProjectMetadataDto.CreateNew("Tipos prancha"), context.Settings.Units);
            string json = serializer.Serialize(dto);
            ProjectFileDto reloadedDto = serializer.Deserialize(json);
            ProjectSheetType reloaded = serializer.CreateProjectSheetTypes(reloadedDto).Single(t => t.Id == tipo.Id);

            AssertEqual(1, dto.SheetTypes.Count, "DTO SheetTypes.Count");
            AssertEqual(tipo.Id, dto.SheetTypes[0].Id, "DTO SheetType.Id");
            AssertEqual("A3 Retrato - Obra", reloaded.Nome, "Reload SheetType.Nome");
            AssertEqual(ProjectSheetFormat.A3, reloaded.FormatoFolha, "Reload SheetType.FormatoFolha");
            AssertEqual(ProjectSheetOrientation.Retrato, reloaded.OrientacaoFolha, "Reload SheetType.OrientacaoFolha");
            AssertEqual(397, reloaded.LarguraFolha, "Reload SheetType.LarguraFolha");
            AssertEqual(561, reloaded.AlturaFolha, "Reload SheetType.AlturaFolha");
        }

        private static void PersistenciaPreservaAssociacaoPranchaTipo()
        {
            var serializer = CriarProjectSerializerTeste();
            var document = new AraciDocument();
            ProjectSheetType tipo = document.TipoPranchaPadrao;
            ProjectSheet prancha = document.CriarNovaPrancha();
            EditorContext context = new();

            ProjectFileDto dto = serializer.CreateFileDto(document, ProjectMetadataDto.CreateNew("Associacao prancha tipo"), context.Settings.Units);
            string json = serializer.Serialize(dto);
            ProjectFileDto reloadedDto = serializer.Deserialize(json);
            ProjectSheet reloaded = serializer.CreateProjectSheets(reloadedDto, Array.Empty<Guid>()).Single(s => s.Id == prancha.Id);

            AssertEqual(tipo.Id, dto.Sheets.Single(s => s.Id == prancha.Id).SheetTypeId ?? Guid.Empty, "DTO SheetTypeId");
            AssertEqual(tipo.Id, reloaded.SheetTypeId ?? Guid.Empty, "Reload SheetTypeId");
        }

        private static void CriarTipoPranchaAdicionaItemComDefaults()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new CriarItemProjetoUseCase(document, commands);

            ProjectSheetType tipo = useCase.CriarTipoPrancha();

            AssertEqual(2, document.TiposPrancha.Count, "TiposPrancha.Count apos criar");
            Assert(document.TiposPrancha.Contains(tipo), "Tipo criado deveria estar no documento.");
            Assert(tipo.Id != Guid.Empty, "Tipo criado Id");
            AssertEqual(ProjectSheetFormat.A1, tipo.FormatoFolha, "Tipo criado FormatoFolha");
            AssertEqual(ProjectSheetOrientation.Paisagem, tipo.OrientacaoFolha, "Tipo criado OrientacaoFolha");
            AssertEqual(ProjectSheet.DefaultWidth, tipo.LarguraFolha, "Tipo criado LarguraFolha");
            AssertEqual(ProjectSheet.DefaultHeight, tipo.AlturaFolha, "Tipo criado AlturaFolha");
        }

        private static void CriarTipoPranchaUndoRedo()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new CriarItemProjetoUseCase(document, commands);

            ProjectSheetType tipo = useCase.CriarTipoPrancha();
            commands.Undo();

            Assert(!document.TiposPrancha.Any(t => t.Id == tipo.Id), "Undo deveria remover tipo criado.");

            commands.Redo();

            Assert(document.TiposPrancha.Any(t => t.Id == tipo.Id), "Redo deveria restaurar tipo criado.");
        }

        private static void DuplicarTipoPranchaCopiaPropriedades()
        {
            var document = new AraciDocument();
            ProjectSheetType origem = document.TipoPranchaPadrao;
            origem.Nome = "A3 Obra";
            origem.FormatoFolha = ProjectSheetFormat.A3;
            origem.OrientacaoFolha = ProjectSheetOrientation.Retrato;
            origem.LarguraFolha = 397;
            origem.AlturaFolha = 561;
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new DuplicarItemProjetoUseCase(document, commands);

            bool duplicou = useCase.DuplicarTipoPrancha(origem.Id);
            ProjectSheetType duplicata = document.TiposPrancha.Single(t => t.Id != origem.Id);

            Assert(duplicou, "DuplicarTipoPrancha deveria retornar true.");
            Assert(duplicata.Id != origem.Id, "Duplicata deveria ter novo Id.");
            AssertEqual(ProjectSheetFormat.A3, duplicata.FormatoFolha, "Duplicata FormatoFolha");
            AssertEqual(ProjectSheetOrientation.Retrato, duplicata.OrientacaoFolha, "Duplicata OrientacaoFolha");
            AssertEqual(397, duplicata.LarguraFolha, "Duplicata LarguraFolha");
            AssertEqual(561, duplicata.AlturaFolha, "Duplicata AlturaFolha");
            Assert(!document.Pranchas.Any(p => p.SheetTypeId == duplicata.Id), "Duplicar tipo nao deveria reassociar pranchas.");
        }

        private static void DuplicarTipoPranchaUndoRedo()
        {
            var document = new AraciDocument();
            Guid origemId = document.TipoPranchaPadrao.Id;
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new DuplicarItemProjetoUseCase(document, commands);

            bool duplicou = useCase.DuplicarTipoPrancha(origemId);
            ProjectSheetType duplicata = document.TiposPrancha.Single(t => t.Id != origemId);
            commands.Undo();

            Assert(duplicou, "DuplicarTipoPrancha deveria retornar true.");
            Assert(!document.TiposPrancha.Any(t => t.Id == duplicata.Id), "Undo deveria remover duplicata.");

            commands.Redo();

            Assert(document.TiposPrancha.Any(t => t.Id == duplicata.Id), "Redo deveria restaurar duplicata.");
        }

        private static void RenomearTipoPranchaAlteraNome()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new RenomearItemProjetoUseCase(document, commands);
            ProjectSheetType tipo = document.TipoPranchaPadrao;

            bool renomeou = useCase.RenomearTipoPrancha(tipo.Id, "  Template obra  ");

            Assert(renomeou, "RenomearTipoPrancha deveria retornar true.");
            AssertEqual("Template obra", tipo.Nome, "Tipo renomeado");
            commands.Undo();
            AssertEqual(ProjectSheetType.DefaultName, tipo.Nome, "Undo renomear tipo");
        }

        private static void RenomearTipoPranchaBloqueiaNomeVazio()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new RenomearItemProjetoUseCase(document, commands);
            ProjectSheetType tipo = document.TipoPranchaPadrao;

            bool renomeou = useCase.RenomearTipoPrancha(tipo.Id, "   ");

            Assert(!renomeou, "Nome vazio deveria ser bloqueado.");
            AssertEqual(ProjectSheetType.DefaultName, tipo.Nome, "Nome deveria permanecer inalterado.");
            Assert(!commands.CanUndo, "Nome vazio nao deveria criar comando.");
        }

        private static void RenomearTipoPranchaBloqueiaDuplicadoCaseInsensitive()
        {
            var document = new AraciDocument();
            ProjectSheetType tipo = document.CriarNovoTipoPrancha();
            tipo.Nome = "Tipo Obra";
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new RenomearItemProjetoUseCase(document, commands);

            bool renomeou = useCase.RenomearTipoPrancha(tipo.Id, ProjectSheetType.DefaultName.ToUpperInvariant());

            Assert(!renomeou, "Nome duplicado case-insensitive deveria ser bloqueado.");
            AssertEqual("Tipo Obra", tipo.Nome, "Nome duplicado nao deveria alterar tipo.");
            Assert(!commands.CanUndo, "Nome duplicado nao deveria criar comando.");
        }

        private static void ExcluirTipoPranchaRemoveTipoNaoUsado()
        {
            var document = new AraciDocument();
            ProjectSheetType tipo = document.CriarNovoTipoPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new ExcluirItemProjetoUseCase(document, commands);

            bool excluiu = useCase.ExcluirTipoPrancha(tipo.Id);

            Assert(excluiu, "ExcluirTipoPrancha deveria retornar true.");
            Assert(!document.TiposPrancha.Any(t => t.Id == tipo.Id), "Tipo nao usado deveria ser removido.");
        }

        private static void ExcluirTipoPranchaUndoRedo()
        {
            var document = new AraciDocument();
            ProjectSheetType tipo = document.CriarNovoTipoPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new ExcluirItemProjetoUseCase(document, commands);

            bool excluiu = useCase.ExcluirTipoPrancha(tipo.Id);
            commands.Undo();

            Assert(excluiu, "ExcluirTipoPrancha deveria retornar true.");
            Assert(document.TiposPrancha.Any(t => t.Id == tipo.Id), "Undo deveria restaurar tipo excluido.");

            commands.Redo();

            Assert(!document.TiposPrancha.Any(t => t.Id == tipo.Id), "Redo deveria excluir tipo novamente.");
        }

        private static void ExcluirUltimoTipoPranchaBloqueado()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new ExcluirItemProjetoUseCase(document, commands);
            Guid tipoId = document.TipoPranchaPadrao.Id;

            bool excluiu = useCase.ExcluirTipoPrancha(tipoId);

            Assert(!excluiu, "Excluir ultimo tipo deveria ser bloqueado.");
            AssertEqual(1, document.TiposPrancha.Count, "Ultimo tipo deveria permanecer.");
            AssertEqual(tipoId, document.TipoPranchaPadrao.Id, "Tipo padrao deveria permanecer.");
        }

        private static void ExcluirTipoPranchaEmUsoBloqueado()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new ExcluirItemProjetoUseCase(document, commands);

            bool excluiu = useCase.ExcluirTipoPrancha(prancha.SheetTypeId ?? Guid.Empty);

            Assert(!excluiu, "Excluir tipo em uso deveria ser bloqueado.");
            AssertEqual(1, document.TiposPrancha.Count, "Tipo em uso deveria permanecer.");
        }

        private static void ExcluirTipoEmUsoNaoAlteraSheetTypeId()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            Guid? sheetTypeId = prancha.SheetTypeId;
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new ExcluirItemProjetoUseCase(document, commands);

            bool excluiu = useCase.ExcluirTipoPrancha(sheetTypeId ?? Guid.Empty);

            Assert(!excluiu, "Excluir tipo em uso deveria retornar false.");
            AssertEqual(sheetTypeId, prancha.SheetTypeId, "SheetTypeId nao deveria mudar.");
        }

        private static void ProjectBrowserListaTiposPrancha()
        {
            var document = new AraciDocument();
            ProjectSheetType tipo = document.CriarNovoTipoPrancha();
            var browser = new ProjectBrowserViewModel(document);

            ProjectBrowserSectionViewModel secao = browser.Secoes.Single(s => s.Titulo == "Tipos de Prancha");

            Assert(secao.Itens.Any(i => i.Id == document.TipoPranchaPadrao.Id), "Browser deveria listar tipo padrao.");
            Assert(secao.Itens.Any(i => i.Id == tipo.Id), "Browser deveria listar tipo criado.");
        }

        private static void ProjectSheetTypeViewModelUsaDimensoesTipo()
        {
            var document = new AraciDocument();
            ProjectSheetType tipo = document.TipoPranchaPadrao;
            tipo.Nome = "A2 Template";
            tipo.FormatoFolha = ProjectSheetFormat.A2;
            tipo.OrientacaoFolha = ProjectSheetOrientation.Retrato;
            tipo.LarguraFolha = 561;
            tipo.AlturaFolha = 794;

            var viewModel = new ProjectSheetTypeViewModel(document, tipo);

            AssertEqual(tipo.Id, viewModel.Id, "ProjectSheetTypeViewModel.Id");
            AssertEqual("A2 Template", viewModel.Nome, "ProjectSheetTypeViewModel.Nome");
            AssertEqual(ProjectSheetFormat.A2, viewModel.FormatoFolha, "ProjectSheetTypeViewModel.FormatoFolha");
            AssertEqual(ProjectSheetOrientation.Retrato, viewModel.OrientacaoFolha, "ProjectSheetTypeViewModel.OrientacaoFolha");
            AssertEqual(561, viewModel.SheetWidth, "ProjectSheetTypeViewModel.SheetWidth");
            AssertEqual(794, viewModel.SheetHeight, "ProjectSheetTypeViewModel.SheetHeight");
            Assert(viewModel.WorkspaceWidth > viewModel.SheetWidth, "WorkspaceWidth deveria incluir margem.");
            Assert(viewModel.WorkspaceHeight > viewModel.SheetHeight, "WorkspaceHeight deveria incluir margem.");
        }

        private static void ProjectSheetTypeViewModelAtualizaAposPropriedadesTipo()
        {
            var document = new AraciDocument();
            ProjectSheetType tipo = document.TipoPranchaPadrao;
            var viewModel = new ProjectSheetTypeViewModel(document, tipo);
            var commands = new Araci.Core.Commands.CommandManager();
            var editar = new EditarPropriedadesTipoPranchaUseCase(document, commands);

            bool alterou = editar.AlterarFormato(tipo.Id, ProjectSheetFormat.A3);

            Assert(alterou, "AlterarFormato deveria retornar true.");
            AssertEqual(ProjectSheetFormat.A3, viewModel.FormatoFolha, "ViewModel FormatoFolha apos alterar tipo");
            AssertEqual(561, viewModel.SheetWidth, "ViewModel SheetWidth apos alterar tipo");
            AssertEqual(397, viewModel.SheetHeight, "ViewModel SheetHeight apos alterar tipo");
        }

        private static void ProjectSheetTypeViewPossuiSuperficieTemplate()
        {
            RunSta(() =>
            {
                var view = new ProjectSheetTypeView();
                var surface = view.FindName("SheetTypeSurface") as Canvas;
                var page = view.FindName("TemplatePageBorder") as Border;

                Assert(surface != null, "ProjectSheetTypeView deveria possuir SheetTypeSurface.");
                Assert(page != null, "ProjectSheetTypeView deveria possuir TemplatePageBorder.");
            });
        }

        private static void ProjectSheetTypeViewMantemBindingsDimensoesTipo()
        {
            string xaml = File.ReadAllText(FindProjectFile("Views/ProjectSheetTypeView.xaml"));

            AssertContains(xaml, "Width=\"{Binding SheetWidth}\"", "ProjectSheetTypeView SheetWidth binding");
            AssertContains(xaml, "Height=\"{Binding SheetHeight}\"", "ProjectSheetTypeView SheetHeight binding");
            AssertContains(xaml, "Width=\"{Binding WorkspaceWidth}\"", "ProjectSheetTypeView WorkspaceWidth binding");
            AssertContains(xaml, "Height=\"{Binding WorkspaceHeight}\"", "ProjectSheetTypeView WorkspaceHeight binding");
            Assert(!xaml.Contains("TableInstances", StringComparison.OrdinalIgnoreCase), "ProjectSheetTypeView nao deveria renderizar tabelas.");
        }

        private static void ProjectBrowserSelecionaTipoAbreVisualizacaoTipo()
        {
            var document = new AraciDocument();
            bool abriuVista = false;
            bool abriuTabela = false;
            bool abriuPrancha = false;
            bool abriuPropriedadesTipo = false;
            bool abriuVisualizacaoTipo = false;
            var browser = new ProjectBrowserViewModel(
                document,
                _ => abriuVista = true,
                abrirTabela: _ => abriuTabela = true,
                abrirPrancha: _ => abriuPrancha = true,
                abrirPropriedadesTipoPrancha: _ => abriuPropriedadesTipo = true,
                abrirTipoPrancha: _ => abriuVisualizacaoTipo = true);
            ProjectBrowserItemViewModel item = browser.Secoes.Single(s => s.Titulo == "Tipos de Prancha").Itens.First();

            item.SelecionarCommand.Execute(null);

            Assert(!abriuVista, "Selecionar tipo nao deveria abrir vista.");
            Assert(!abriuTabela, "Selecionar tipo nao deveria abrir tabela.");
            Assert(!abriuPrancha, "Selecionar tipo nao deveria abrir prancha.");
            Assert(abriuVisualizacaoTipo, "Selecionar tipo deveria abrir visualizacao central do tipo.");
            Assert(abriuPropriedadesTipo, "Selecionar tipo deveria abrir propriedades do tipo.");
            Assert(item.IsSelected, "Tipo selecionado deveria ficar marcado.");
        }

        private static void ProjectBrowserSelecionaTipoNaoAlteraVistaAtiva()
        {
            var document = new AraciDocument();
            Guid? vistaAtivaAntes = document.VistaAtivaId;
            var browser = new ProjectBrowserViewModel(
                document,
                document.DefinirVistaAtiva,
                abrirTipoPrancha: _ => { });
            ProjectBrowserItemViewModel item = browser.Secoes.Single(s => s.Titulo == "Tipos de Prancha").Itens.First();

            item.SelecionarCommand.Execute(null);

            AssertEqual(vistaAtivaAntes, document.VistaAtivaId, "Selecionar tipo nao deveria alterar VistaAtivaId");
        }

        private static void ProjectBrowserSelecionaTipoNaoAlteraTabelasOuPranchas()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            Guid? sheetTypeId = prancha.SheetTypeId;
            var browser = new ProjectBrowserViewModel(
                document,
                abrirTipoPrancha: _ => { });
            ProjectBrowserItemViewModel item = browser.Secoes.Single(s => s.Titulo == "Tipos de Prancha").Itens.First();

            item.SelecionarCommand.Execute(null);

            AssertEqual(1, document.Tabelas.Count, "Selecionar tipo nao deveria alterar Tabelas.Count");
            AssertEqual(tabela.Id, document.Tabelas[0].Id, "Tabela preservada ao selecionar tipo");
            AssertEqual(1, document.Pranchas.Count, "Selecionar tipo nao deveria alterar Pranchas.Count");
            AssertEqual(prancha.Id, document.Pranchas[0].Id, "Prancha preservada ao selecionar tipo");
            AssertEqual(sheetTypeId, document.Pranchas[0].SheetTypeId, "SheetTypeId preservado ao selecionar tipo");
        }

        private static void SelecaoTipoExibeProjectSheetTypePropertiesViewModel()
        {
            var context = new EditorContext();
            ProjectSheetType tipo = context.Document.TipoPranchaPadrao;
            object? selecionado = null;
            var browser = new ProjectBrowserViewModel(
                context.Document,
                context.DefinirVistaAtiva,
                context.RenomearItemProjeto,
                context.ExcluirItemProjeto,
                context.DuplicarItemProjeto,
                abrirPropriedadesTipoPrancha: id =>
                {
                    ProjectSheetType? atual = context.Document.TiposPrancha.FirstOrDefault(t => t.Id == id);
                    selecionado = atual == null
                        ? null
                        : new ProjectSheetTypePropertiesViewModel(context.Document, atual, context.RenomearItemProjeto, context.EditarPropriedadesTipoPrancha);
                });

            ProjectBrowserItemViewModel item = browser.Secoes.Single(s => s.Titulo == "Tipos de Prancha").Itens.Single(i => i.Id == tipo.Id);
            item.SelecionarCommand.Execute(null);

            Assert(selecionado is ProjectSheetTypePropertiesViewModel, "Selecao de tipo deveria criar ProjectSheetTypePropertiesViewModel.");
        }

        private static void PropriedadesTipoPranchaEditaveisUndoRedo()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var renomear = new RenomearItemProjetoUseCase(document, commands);
            var editar = new EditarPropriedadesTipoPranchaUseCase(document, commands);
            ProjectSheetType tipo = document.TipoPranchaPadrao;
            var viewModel = new ProjectSheetTypePropertiesViewModel(document, tipo, renomear, editar);

            viewModel.Nome = "Tipo revisado";
            viewModel.FormatoFolha = ProjectSheetFormat.A3;
            viewModel.LarguraFolha = 640;

            AssertEqual("Tipo revisado", tipo.Nome, "Tipo Nome editado");
            AssertEqual(ProjectSheetFormat.Personalizado, tipo.FormatoFolha, "Tipo Formato apos largura manual");
            AssertEqual(640, tipo.LarguraFolha, "Tipo Largura editada");

            commands.Undo();
            AssertEqual(561, tipo.LarguraFolha, "Undo largura volta ao A3");

            commands.Undo();
            AssertEqual(ProjectSheetFormat.A1, tipo.FormatoFolha, "Undo formato volta ao A1");

            commands.Undo();
            AssertEqual(ProjectSheetType.DefaultName, tipo.Nome, "Undo nome tipo");

            commands.Redo();
            AssertEqual("Tipo revisado", tipo.Nome, "Redo nome tipo");
        }

        private static void PersistenciaSalvaReabreTiposCriadosEditados()
        {
            var serializer = CriarProjectSerializerTeste();
            var document = new AraciDocument();
            ProjectSheetType tipo = document.CriarNovoTipoPrancha();
            tipo.Nome = "Template executivo";
            tipo.FormatoFolha = ProjectSheetFormat.A2;
            tipo.OrientacaoFolha = ProjectSheetOrientation.Retrato;
            tipo.LarguraFolha = 561;
            tipo.AlturaFolha = 794;
            EditorContext context = new();

            ProjectFileDto dto = serializer.CreateFileDto(document, ProjectMetadataDto.CreateNew("G21 tipos"), context.Settings.Units);
            ProjectFileDto reloadedDto = serializer.Deserialize(serializer.Serialize(dto));
            ProjectSheetType reloaded = serializer.CreateProjectSheetTypes(reloadedDto).Single(t => t.Id == tipo.Id);

            AssertEqual("Template executivo", reloaded.Nome, "Reload tipo Nome");
            AssertEqual(ProjectSheetFormat.A2, reloaded.FormatoFolha, "Reload tipo FormatoFolha");
            AssertEqual(ProjectSheetOrientation.Retrato, reloaded.OrientacaoFolha, "Reload tipo OrientacaoFolha");
            AssertEqual(561, reloaded.LarguraFolha, "Reload tipo LarguraFolha");
            AssertEqual(794, reloaded.AlturaFolha, "Reload tipo AlturaFolha");
        }

        private static void OperacoesTipoPranchaNaoAlteramTabelasOuPranchas()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            Guid pranchaId = prancha.Id;
            Guid? sheetTypeId = prancha.SheetTypeId;
            var commands = new Araci.Core.Commands.CommandManager();
            var criar = new CriarItemProjetoUseCase(document, commands);
            var duplicar = new DuplicarItemProjetoUseCase(document, commands);
            var renomear = new RenomearItemProjetoUseCase(document, commands);
            var editar = new EditarPropriedadesTipoPranchaUseCase(document, commands);

            ProjectSheetType tipo = criar.CriarTipoPrancha();
            duplicar.DuplicarTipoPrancha(tipo.Id);
            renomear.RenomearTipoPrancha(tipo.Id, "Administrativo");
            editar.AlterarAltura(tipo.Id, 600);

            AssertEqual(1, document.Tabelas.Count, "Tabelas.Count apos operacoes de tipo");
            AssertEqual(tabela.Id, document.Tabelas[0].Id, "Tabela preservada");
            AssertEqual(1, document.Pranchas.Count, "Pranchas.Count apos operacoes de tipo");
            AssertEqual(pranchaId, document.Pranchas[0].Id, "Prancha preservada");
            AssertEqual(sheetTypeId, document.Pranchas[0].SheetTypeId, "SheetTypeId da prancha preservado");
        }

        private static void PranchaDuplicaInstanciasTabelaComCopiaProfunda()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet origem = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance
            {
                TableId = tabela.Id,
                X = 10,
                Y = 20,
                Width = 220,
                Height = 120,
                RowStartIndex = 3,
                RowCount = 5
            };
            origem.Tabelas.Add(instancia);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new DuplicarItemProjetoUseCase(document, commands);

            bool duplicou = useCase.DuplicarPrancha(origem.Id);
            ProjectSheet duplicata = document.Pranchas.Single(p => p.Id != origem.Id);
            ProjectSheetTableInstance copia = duplicata.Tabelas.Single();

            Assert(duplicou, "DuplicarPrancha deveria retornar true.");
            Assert(!ReferenceEquals(instancia, copia), "Instancia duplicada nao deveria compartilhar referencia.");
            Assert(instancia.Id != copia.Id, "Instancia duplicada deveria receber novo Id.");
            AssertEqual(instancia.TableId, copia.TableId, "Duplicata instancia TableId");
            AssertEqual(instancia.X, copia.X, "Duplicata instancia X");
            AssertEqual(instancia.Y, copia.Y, "Duplicata instancia Y");
            AssertEqual(instancia.Width, copia.Width, "Duplicata instancia Width");
            AssertEqual(instancia.Height, copia.Height, "Duplicata instancia Height");
            AssertEqual(instancia.RowStartIndex, copia.RowStartIndex, "Duplicata instancia RowStartIndex");
            AssertEqual(instancia.RowCount, copia.RowCount, "Duplicata instancia RowCount");
        }

        private static void DuplicarPranchaPreservaAssociacaoTipo()
        {
            var document = new AraciDocument();
            ProjectSheet origem = document.CriarNovaPrancha();
            Guid tipoId = origem.SheetTypeId ?? Guid.Empty;
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new DuplicarItemProjetoUseCase(document, commands);

            bool duplicou = useCase.DuplicarPrancha(origem.Id);
            ProjectSheet duplicata = document.Pranchas.Single(p => p.Id != origem.Id);

            Assert(duplicou, "DuplicarPrancha deveria retornar true.");
            AssertEqual(tipoId, duplicata.SheetTypeId ?? Guid.Empty, "Duplicata SheetTypeId");
            AssertEqual(1, document.TiposPrancha.Count, "DuplicarPrancha nao duplica tipo");
        }

        private static void OperacoesPranchaNaoAlteramTiposIndevidamente()
        {
            var document = new AraciDocument();
            var commands = new Araci.Core.Commands.CommandManager();
            var criar = new CriarItemProjetoUseCase(document, commands);
            var duplicar = new DuplicarItemProjetoUseCase(document, commands);
            var excluir = new ExcluirItemProjetoUseCase(document, commands);
            Guid tipoId = document.TipoPranchaPadrao.Id;

            ProjectSheet prancha = criar.CriarPrancha();
            AssertEqual(1, document.TiposPrancha.Count, "Tipos apos criar");

            bool duplicou = duplicar.DuplicarPrancha(prancha.Id);
            Assert(duplicou, "DuplicarPrancha deveria retornar true.");
            AssertEqual(1, document.TiposPrancha.Count, "Tipos apos duplicar");

            ProjectSheet duplicata = document.Pranchas.Single(p => p.Id != prancha.Id);
            bool excluiu = excluir.ExcluirPrancha(duplicata.Id);
            Assert(excluiu, "ExcluirPrancha deveria retornar true.");
            AssertEqual(1, document.TiposPrancha.Count, "Tipos apos excluir");
            AssertEqual(tipoId, document.TipoPranchaPadrao.Id, "Tipo padrao preservado");
        }

        private static void ExcluirTabelaLimpaInstanciasPranchaComUndoRedo()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectTable outraTabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instanciaAfetada = new ProjectSheetTableInstance { TableId = tabela.Id, X = 1, Y = 2, Width = 100, Height = 50 };
            var instanciaOutraTabela = new ProjectSheetTableInstance { TableId = outraTabela.Id, X = 3, Y = 4, Width = 120, Height = 60 };
            prancha.Tabelas.Add(instanciaAfetada);
            prancha.Tabelas.Add(instanciaOutraTabela);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new ExcluirItemProjetoUseCase(document, commands);

            bool excluiu = useCase.ExcluirTabela(tabela.Id);

            Assert(excluiu, "ExcluirTabela deveria retornar true.");
            Assert(!document.Tabelas.Any(t => t.Id == tabela.Id), "Tabela deveria ser removida.");
            AssertEqual(1, prancha.Tabelas.Count, "Instancias apos excluir tabela");
            AssertEqual(outraTabela.Id, prancha.Tabelas[0].TableId, "Instancia de outra tabela deveria ser preservada.");

            commands.Undo();

            Assert(document.Tabelas.Any(t => t.Id == tabela.Id), "Undo deveria restaurar tabela.");
            AssertEqual(2, prancha.Tabelas.Count, "Undo deveria restaurar instancia removida.");
            AssertEqual(instanciaAfetada.Id, prancha.Tabelas[0].Id, "Undo instancia restaurada Id");
            AssertEqual(tabela.Id, prancha.Tabelas[0].TableId, "Undo instancia restaurada TableId");
            AssertEqual(outraTabela.Id, prancha.Tabelas[1].TableId, "Undo deveria preservar ordem.");

            commands.Redo();

            Assert(!document.Tabelas.Any(t => t.Id == tabela.Id), "Redo deveria remover tabela novamente.");
            AssertEqual(1, prancha.Tabelas.Count, "Redo deveria remover instancia novamente.");
            AssertEqual(outraTabela.Id, prancha.Tabelas[0].TableId, "Redo instancia restante");
        }

        private static void PranchaCarregaArquivoAntigoSemInstancias()
        {
            var serializer = CriarProjectSerializerTeste();
            Guid sheetId = Guid.NewGuid();
            string json = $$"""
            {
              "Version": 1,
              "Sheets": [
                {
                  "Id": "{{sheetId}}",
                  "Nome": "Prancha antiga",
                  "Numero": "A001"
                }
              ]
            }
            """;

            ProjectFileDto dto = serializer.Deserialize(json);
            ProjectSheet prancha = serializer.CreateProjectSheets(dto, Array.Empty<Guid>()).Single();
            List<ProjectSheetTableInstance> tabelas = prancha.Tabelas ?? throw new InvalidOperationException("ProjectSheet.Tabelas null.");

            AssertEqual(sheetId, prancha.Id, "Prancha antiga Id");
            AssertEqual("Prancha antiga", prancha.Nome, "Prancha antiga Nome");
            AssertEqual(0, tabelas.Count, "Prancha antiga Tabelas.Count");
        }

        private static void ArquivoAntigoSemPropriedadesPranchaUsaDefaults()
        {
            var serializer = CriarProjectSerializerTeste();
            Guid sheetId = Guid.NewGuid();
            string json = $$"""
            {
              "Version": 1,
              "Sheets": [
                {
                  "Id": "{{sheetId}}",
                  "Nome": "Prancha antiga",
                  "Numero": "A001"
                }
              ]
            }
            """;

            ProjectFileDto dto = serializer.Deserialize(json);
            ProjectSheet prancha = serializer.CreateProjectSheets(dto, Array.Empty<Guid>()).Single();

            AssertEqual(ProjectSheetFormat.A1, prancha.FormatoFolha, "Prancha antiga FormatoFolha default");
            AssertEqual(ProjectSheetOrientation.Paisagem, prancha.OrientacaoFolha, "Prancha antiga OrientacaoFolha default");
            AssertEqual(ProjectSheet.DefaultWidth, prancha.LarguraFolha, "Prancha antiga LarguraFolha default");
            AssertEqual(ProjectSheet.DefaultHeight, prancha.AlturaFolha, "Prancha antiga AlturaFolha default");
        }

        private static void ArquivoAntigoSemTiposPranchaUsaTipoPadrao()
        {
            var serializer = CriarProjectSerializerTeste();
            Guid sheetId = Guid.NewGuid();
            string json = $$"""
            {
              "Version": 1,
              "Sheets": [
                {
                  "Id": "{{sheetId}}",
                  "Nome": "Prancha G19",
                  "Numero": "A001",
                  "FormatoFolha": "Personalizado",
                  "OrientacaoFolha": "Retrato",
                  "LarguraFolha": 640,
                  "AlturaFolha": 900
                }
              ]
            }
            """;

            ProjectFileDto dto = serializer.Deserialize(json);
            IReadOnlyList<ProjectSheetType> tipos = serializer.CreateProjectSheetTypes(dto);
            IReadOnlyList<ProjectSheet> pranchas = serializer.CreateProjectSheets(dto, Array.Empty<Guid>());
            var document = new AraciDocument();
            document.SubstituirTiposPrancha(tipos);
            document.SubstituirPranchas(pranchas);
            ProjectSheet prancha = document.Pranchas.Single();

            AssertEqual(1, document.TiposPrancha.Count, "Arquivo antigo TiposPrancha.Count");
            AssertEqual(document.TipoPranchaPadrao.Id, prancha.SheetTypeId ?? Guid.Empty, "Arquivo antigo SheetTypeId fallback");
            AssertEqual("A001", prancha.Numero, "Arquivo G19 preserva Numero");
            AssertEqual(ProjectSheetFormat.Personalizado, prancha.FormatoFolha, "Arquivo G19 preserva FormatoFolha");
            AssertEqual(ProjectSheetOrientation.Retrato, prancha.OrientacaoFolha, "Arquivo G19 preserva OrientacaoFolha");
            AssertEqual(640, prancha.LarguraFolha, "Arquivo G19 preserva LarguraFolha");
            AssertEqual(900, prancha.AlturaFolha, "Arquivo G19 preserva AlturaFolha");
        }

        private static void PranchaIgnoraInstanciaOrfaNoLoad()
        {
            var serializer = CriarProjectSerializerTeste();
            Guid tabelaValidaId = Guid.NewGuid();
            Guid tabelaOrfaId = Guid.NewGuid();
            var dto = new ProjectFileDto
            {
                Tables = new List<ProjectTableDto>
                {
                    new() { Id = tabelaValidaId, Nome = "Tabela 1" }
                },
                Sheets = new List<ProjectSheetDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Prancha 1",
                        Numero = "A001",
                        Tabelas = new List<ProjectSheetTableInstanceDto>
                        {
                            new() { Id = Guid.NewGuid(), TableId = tabelaValidaId, X = 1, Y = 2, Width = 100, Height = 50 },
                            new() { Id = Guid.NewGuid(), TableId = tabelaOrfaId, X = 3, Y = 4, Width = 120, Height = 60 },
                            new() { Id = Guid.NewGuid(), TableId = Guid.Empty, X = 5, Y = 6, Width = 140, Height = 70 }
                        }
                    }
                }
            };
            IReadOnlyList<ProjectTable> tabelas = serializer.CreateProjectTables(dto);

            ProjectSheet prancha = serializer.CreateProjectSheets(dto, tabelas.Select(t => t.Id)).Single();

            AssertEqual(1, prancha.Tabelas.Count, "Instancias validas apos ignorar orfas");
            AssertEqual(tabelaValidaId, prancha.Tabelas[0].TableId, "Instancia valida TableId");
        }

        private static void EditarPropriedadesPranchaUndoRedo()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new EditarPropriedadesPranchaUseCase(document, commands);

            bool alterouNumero = useCase.AlterarNumero(prancha.Id, " EL-101 ");

            Assert(alterouNumero, "AlterarNumero deveria retornar true.");
            AssertEqual("EL-101", prancha.Numero, "Numero alterado");

            commands.Undo();

            AssertEqual("A001", prancha.Numero, "Undo numero");

            commands.Redo();

            AssertEqual("EL-101", prancha.Numero, "Redo numero");

            bool alterouFormato = useCase.AlterarFormato(prancha.Id, ProjectSheetFormat.A3);

            Assert(alterouFormato, "AlterarFormato deveria retornar true.");
            AssertEqual(ProjectSheetFormat.A3, prancha.FormatoFolha, "Formato alterado");
            AssertEqual(ProjectSheetOrientation.Paisagem, prancha.OrientacaoFolha, "Orientacao mantida");
            AssertEqual(561, prancha.LarguraFolha, "A3 paisagem largura");
            AssertEqual(397, prancha.AlturaFolha, "A3 paisagem altura");

            commands.Undo();

            AssertEqual(ProjectSheetFormat.A1, prancha.FormatoFolha, "Undo formato");
            AssertEqual(ProjectSheet.DefaultWidth, prancha.LarguraFolha, "Undo formato largura");
            AssertEqual(ProjectSheet.DefaultHeight, prancha.AlturaFolha, "Undo formato altura");

            bool alterouOrientacao = useCase.AlterarOrientacao(prancha.Id, ProjectSheetOrientation.Retrato);

            Assert(alterouOrientacao, "AlterarOrientacao deveria retornar true.");
            AssertEqual(ProjectSheetOrientation.Retrato, prancha.OrientacaoFolha, "Orientacao alterada");
            AssertEqual(794, prancha.LarguraFolha, "A1 retrato largura");
            AssertEqual(1122, prancha.AlturaFolha, "A1 retrato altura");

            commands.Undo();

            AssertEqual(ProjectSheetOrientation.Paisagem, prancha.OrientacaoFolha, "Undo orientacao");
            AssertEqual(ProjectSheet.DefaultWidth, prancha.LarguraFolha, "Undo orientacao largura");
            AssertEqual(ProjectSheet.DefaultHeight, prancha.AlturaFolha, "Undo orientacao altura");

            bool alterouLargura = useCase.AlterarLargura(prancha.Id, 700);

            Assert(alterouLargura, "AlterarLargura deveria retornar true.");
            AssertEqual(ProjectSheetFormat.Personalizado, prancha.FormatoFolha, "Largura manual torna formato personalizado");
            AssertEqual(700, prancha.LarguraFolha, "Largura manual");

            commands.Undo();

            AssertEqual(ProjectSheetFormat.A1, prancha.FormatoFolha, "Undo largura formato");
            AssertEqual(ProjectSheet.DefaultWidth, prancha.LarguraFolha, "Undo largura");

            bool alterouAltura = useCase.AlterarAltura(prancha.Id, 500);

            Assert(alterouAltura, "AlterarAltura deveria retornar true.");
            AssertEqual(ProjectSheetFormat.Personalizado, prancha.FormatoFolha, "Altura manual torna formato personalizado");
            AssertEqual(500, prancha.AlturaFolha, "Altura manual");

            commands.Undo();

            AssertEqual(ProjectSheetFormat.A1, prancha.FormatoFolha, "Undo altura formato");
            AssertEqual(ProjectSheet.DefaultHeight, prancha.AlturaFolha, "Undo altura");
        }

        private static void InserirTabelaNaPranchaCriaInstanciaUndoRedo()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new InserirTabelaNaPranchaUseCase(document, commands);

            ProjectSheetTableInstance? instancia = useCase.Inserir(prancha.Id, tabela.Id);

            Assert(instancia != null, "Inserir deveria retornar instancia.");
            Assert(instancia!.Id != Guid.Empty, "Instancia.Id nao deveria ser vazio.");
            AssertEqual(tabela.Id, instancia.TableId, "Instancia.TableId");
            AssertEqual(InserirTabelaNaPranchaUseCase.DefaultX, instancia.X, "Instancia.X padrao");
            AssertEqual(InserirTabelaNaPranchaUseCase.DefaultY, instancia.Y, "Instancia.Y padrao");
            AssertEqual(InserirTabelaNaPranchaUseCase.DefaultWidth, instancia.Width, "Instancia.Width padrao");
            AssertEqual(InserirTabelaNaPranchaUseCase.DefaultHeight, instancia.Height, "Instancia.Height padrao");
            AssertEqual(1, prancha.Tabelas.Count, "Prancha.Tabelas.Count apos inserir");
            Assert(ReferenceEquals(instancia, prancha.Tabelas[0]), "Command deveria adicionar a mesma instancia criada pelo use case.");

            Guid instanciaId = instancia.Id;
            commands.Undo();

            AssertEqual(0, prancha.Tabelas.Count, "Undo deveria remover instancia.");

            commands.Redo();

            AssertEqual(1, prancha.Tabelas.Count, "Redo deveria restaurar instancia.");
            AssertEqual(instanciaId, prancha.Tabelas[0].Id, "Redo deveria preservar o mesmo Id da instancia.");
            Assert(ReferenceEquals(instancia, prancha.Tabelas[0]), "Redo deveria reinserir a mesma instancia.");
        }

        private static void InserirTabelaNaPranchaIgnoraIdsInvalidos()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new InserirTabelaNaPranchaUseCase(document, commands);

            ProjectSheetTableInstance? semPrancha = useCase.Inserir(Guid.NewGuid(), tabela.Id);
            ProjectSheetTableInstance? semTabela = useCase.Inserir(prancha.Id, Guid.NewGuid());

            Assert(semPrancha == null, "SheetId inexistente deveria retornar null.");
            Assert(semTabela == null, "TableId inexistente deveria retornar null.");
            AssertEqual(0, prancha.Tabelas.Count, "Ids invalidos nao deveriam alterar prancha.");
            Assert(!commands.CanUndo, "Ids invalidos nao deveriam entrar no historico.");
        }

        private static void InserirTabelaNaPranchaMultiplasInstanciasIndependentes()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new InserirTabelaNaPranchaUseCase(document, commands);

            ProjectSheetTableInstance? primeira = useCase.Inserir(prancha.Id, tabela.Id, x: 10, y: 20, width: 200, height: 100);
            ProjectSheetTableInstance? segunda = useCase.Inserir(prancha.Id, tabela.Id, x: 30, y: 40, width: 240, height: 120);

            Assert(primeira != null && segunda != null, "Multiplas insercoes deveriam retornar instancias.");
            AssertEqual(2, prancha.Tabelas.Count, "Multiplas insercoes Count");
            Assert(primeira!.Id != segunda!.Id, "Multiplas instancias deveriam ter IDs distintos.");
            Assert(!ReferenceEquals(primeira, segunda), "Multiplas instancias deveriam ser objetos distintos.");
            AssertEqual(10, prancha.Tabelas[0].X, "Primeira instancia X");
            AssertEqual(30, prancha.Tabelas[1].X, "Segunda instancia X");

            commands.Undo();

            AssertEqual(1, prancha.Tabelas.Count, "Undo deveria remover apenas ultima insercao.");
            AssertEqual(primeira.Id, prancha.Tabelas[0].Id, "Undo preserva primeira instancia.");
        }

        private static void InserirMultiplasTabelasNaPranchaEmUmaOperacao()
        {
            var document = new AraciDocument();
            ProjectTable tabelaA = document.CriarNovaTabela();
            ProjectTable tabelaB = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new InserirTabelaNaPranchaUseCase(document, commands);

            IReadOnlyList<ProjectSheetTableInstance> instancias = useCase.InserirMultiplas(prancha.Id, new[] { tabelaA.Id, tabelaB.Id });

            AssertEqual(2, instancias.Count, "InserirMultiplas retorno Count");
            AssertEqual(2, prancha.Tabelas.Count, "InserirMultiplas prancha Count");
            AssertEqual(tabelaA.Id, prancha.Tabelas[0].TableId, "InserirMultiplas primeira TableId");
            AssertEqual(tabelaB.Id, prancha.Tabelas[1].TableId, "InserirMultiplas segunda TableId");
            Assert(commands.CanUndo, "InserirMultiplas deveria entrar no historico.");
        }

        private static void InserirMultiplasTabelasUndoRedoAgrupado()
        {
            var document = new AraciDocument();
            ProjectTable tabelaA = document.CriarNovaTabela();
            ProjectTable tabelaB = document.CriarNovaTabela();
            ProjectTable tabelaC = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new InserirTabelaNaPranchaUseCase(document, commands);

            IReadOnlyList<ProjectSheetTableInstance> instancias = useCase.InserirMultiplas(prancha.Id, new[] { tabelaA.Id, tabelaB.Id, tabelaC.Id });
            List<Guid> ids = instancias.Select(i => i.Id).ToList();
            List<double> ys = instancias.Select(i => i.Y).ToList();

            commands.Undo();

            AssertEqual(0, prancha.Tabelas.Count, "Undo agrupado deveria remover todas as instancias.");

            commands.Redo();

            AssertEqual(3, prancha.Tabelas.Count, "Redo agrupado deveria restaurar todas as instancias.");
            AssertEqual(ids[0], prancha.Tabelas[0].Id, "Redo agrupado instancia 0 Id");
            AssertEqual(ids[1], prancha.Tabelas[1].Id, "Redo agrupado instancia 1 Id");
            AssertEqual(ids[2], prancha.Tabelas[2].Id, "Redo agrupado instancia 2 Id");
            AssertEqual(ys[0], prancha.Tabelas[0].Y, "Redo agrupado instancia 0 Y");
            AssertEqual(ys[1], prancha.Tabelas[1].Y, "Redo agrupado instancia 1 Y");
            AssertEqual(ys[2], prancha.Tabelas[2].Y, "Redo agrupado instancia 2 Y");
        }

        private static void InserirMultiplasTabelasIgnoraIdsInvalidos()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new InserirTabelaNaPranchaUseCase(document, commands);

            IReadOnlyList<ProjectSheetTableInstance> semPrancha = useCase.InserirMultiplas(Guid.NewGuid(), new[] { tabela.Id });
            IReadOnlyList<ProjectSheetTableInstance> comTabelaInvalida = useCase.InserirMultiplas(prancha.Id, new[] { Guid.Empty, Guid.NewGuid(), tabela.Id });

            AssertEqual(0, semPrancha.Count, "InserirMultiplas sem prancha retorno Count");
            AssertEqual(1, comTabelaInvalida.Count, "InserirMultiplas deveria ignorar tabelas invalidas");
            AssertEqual(tabela.Id, prancha.Tabelas.Single().TableId, "InserirMultiplas tabela valida restante");
        }

        private static void InserirMultiplasTabelasRemoveDuplicidades()
        {
            var document = new AraciDocument();
            ProjectTable tabelaA = document.CriarNovaTabela();
            ProjectTable tabelaB = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new InserirTabelaNaPranchaUseCase(document, commands);

            IReadOnlyList<ProjectSheetTableInstance> instancias = useCase.InserirMultiplas(prancha.Id, new[] { tabelaA.Id, tabelaB.Id, tabelaA.Id, tabelaB.Id });

            AssertEqual(2, instancias.Count, "InserirMultiplas sem duplicidade retorno Count");
            AssertEqual(2, prancha.Tabelas.Count, "InserirMultiplas sem duplicidade prancha Count");
            AssertEqual(tabelaA.Id, prancha.Tabelas[0].TableId, "InserirMultiplas preserva primeira ocorrencia A");
            AssertEqual(tabelaB.Id, prancha.Tabelas[1].TableId, "InserirMultiplas preserva primeira ocorrencia B");
        }

        private static void InserirMultiplasTabelasDistribuiPosicoes()
        {
            var document = new AraciDocument();
            ProjectTable tabelaA = document.CriarNovaTabela();
            ProjectTable tabelaB = document.CriarNovaTabela();
            ProjectTable tabelaC = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new InserirTabelaNaPranchaUseCase(document, commands);

            IReadOnlyList<ProjectSheetTableInstance> instancias = useCase.InserirMultiplas(prancha.Id, new[] { tabelaA.Id, tabelaB.Id, tabelaC.Id });

            AssertEqual(InserirTabelaNaPranchaUseCase.DefaultX, instancias[0].X, "InserirMultiplas primeira X");
            AssertEqual(InserirTabelaNaPranchaUseCase.DefaultY, instancias[0].Y, "InserirMultiplas primeira Y");
            AssertEqual(InserirTabelaNaPranchaUseCase.DefaultY + InserirTabelaNaPranchaUseCase.DefaultHeight + 20, instancias[1].Y, "InserirMultiplas segunda Y");
            AssertEqual(InserirTabelaNaPranchaUseCase.DefaultY + 2 * (InserirTabelaNaPranchaUseCase.DefaultHeight + 20), instancias[2].Y, "InserirMultiplas terceira Y");
            Assert(instancias.Select(i => (i.X, i.Y)).Distinct().Count() == 3, "Instancias multiplas nao deveriam ficar totalmente sobrepostas.");
        }

        private static void InserirTabelaPranchaDialogResultTransportaMultiplosIds()
        {
            Guid sheetId = Guid.NewGuid();
            Guid tabelaA = Guid.NewGuid();
            Guid tabelaB = Guid.NewGuid();

            var result = new InserirTabelaPranchaDialogResult(sheetId, new[] { tabelaA, Guid.Empty, tabelaB, tabelaA });

            AssertEqual(sheetId, result.SheetId, "DialogResult SheetId");
            AssertEqual(2, result.TableIds.Count, "DialogResult TableIds Count");
            AssertEqual(tabelaA, result.TableIds[0], "DialogResult TableIds 0");
            AssertEqual(tabelaB, result.TableIds[1], "DialogResult TableIds 1");
        }

        private static void InserirTabelaPranchaWindowSelecionaMultiplasSemDuplicidade()
        {
            RunSta(() =>
            {
                Guid pranchaId = Guid.NewGuid();
                var tabelaA = new ProjectItemDialogOption(Guid.NewGuid(), "Tabela A");
                var tabelaB = new ProjectItemDialogOption(Guid.NewGuid(), "Tabela B");
                var window = new InserirTabelaPranchaWindow(
                    new[] { new ProjectItemDialogOption(pranchaId, "Prancha A") },
                    new[] { tabelaA, tabelaB });

                window.MoverParaSelecionadas(new[] { tabelaA, tabelaB, tabelaA });

                AssertEqual(pranchaId, window.PranchaSelecionada?.Id, "Janela PranchaSelecionada");
                AssertEqual(2, window.TabelasSelecionadas.Count, "Janela TabelasSelecionadas Count");
                AssertEqual(tabelaA.Id, window.TableIdsSelecionados[0], "Janela TableIdsSelecionados 0");
                AssertEqual(tabelaB.Id, window.TableIdsSelecionados[1], "Janela TableIdsSelecionados 1");

                window.Close();
            });
        }

        private static void EditorContextExpoeInserirTabelaNaPrancha()
        {
            var context = new EditorContext();

            Assert(context.InserirTabelaNaPrancha != null, "EditorContext deveria expor InserirTabelaNaPranchaUseCase.");
        }

        private static void ProjectSheetViewModelExpoeInstanciasTabela()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance
            {
                TableId = tabela.Id,
                X = 11,
                Y = 22,
                Width = 333,
                Height = 144
            };
            prancha.Tabelas.Add(instancia);

            var viewModel = new ProjectSheetViewModel(document, prancha);
            ProjectSheetTableInstanceViewModel instanceViewModel = viewModel.TableInstances.Single();

            AssertEqual(prancha.Id, viewModel.SheetId, "ProjectSheetViewModel.SheetId");
            AssertEqual(instancia.Id, instanceViewModel.Id, "Instancia VM Id");
            AssertEqual(tabela.Id, instanceViewModel.TableId, "Instancia VM TableId");
            AssertEqual(11, instanceViewModel.X, "Instancia VM X");
            AssertEqual(22, instanceViewModel.Y, "Instancia VM Y");
            AssertEqual(333, instanceViewModel.Width, "Instancia VM Width");
            AssertEqual(144, instanceViewModel.Height, "Instancia VM Height");
            Assert(viewModel.HasInstances, "ProjectSheetViewModel deveria indicar instancias.");
            Assert(!viewModel.HasEmptyMessage, "ProjectSheetViewModel com instancias nao deveria exibir vazio.");
        }

        private static void ProjectSheetViewModelResolveNomeTabela()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            tabela.Nome = "Quadro de Cargas";
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Nome = "Prancha Geral";
            prancha.Numero = "A101";
            prancha.Tabelas.Add(new ProjectSheetTableInstance { TableId = tabela.Id });

            var viewModel = new ProjectSheetViewModel(document, prancha);

            AssertEqual("A101 - Prancha Geral", viewModel.Titulo, "ProjectSheetViewModel.Titulo");
            AssertEqual("Quadro de Cargas", viewModel.TableInstances.Single().TableName, "Nome resolvido da tabela");
        }

        private static void ProjectSheetViewModelTrataPranchaVazia()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();

            var viewModel = new ProjectSheetViewModel(document, prancha);

            AssertEqual(0, viewModel.TableInstances.Count, "Prancha vazia TableInstances.Count");
            Assert(!viewModel.HasInstances, "Prancha vazia nao deveria indicar instancias.");
            Assert(viewModel.HasEmptyMessage, "Prancha vazia deveria exibir mensagem.");
            AssertEqual("Nenhuma tabela inserida na prancha", viewModel.EmptyMessage, "Prancha vazia EmptyMessage");
        }

        private static void ProjectSheetViewModelTrataTabelaInexistente()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Tabelas.Add(new ProjectSheetTableInstance { TableId = Guid.NewGuid() });

            var viewModel = new ProjectSheetViewModel(document, prancha);

            AssertEqual(1, viewModel.TableInstances.Count, "Instancia com tabela inexistente Count");
            AssertEqual("Tabela nao encontrada", viewModel.TableInstances.Single().TableName, "Tabela inexistente TableName");
        }

        private static void ProjectSheetViewModelRefreshNaoAlteraModelo()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            tabela.Nome = "Tabela Original";
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance
            {
                TableId = tabela.Id,
                X = 40,
                Y = 50,
                Width = 400,
                Height = 240
            };
            prancha.Tabelas.Add(instancia);
            var viewModel = new ProjectSheetViewModel(document, prancha);
            Guid instanciaId = instancia.Id;
            Guid tabelaId = instancia.TableId;

            tabela.Nome = "Tabela Renomeada";
            viewModel.Refresh();

            AssertEqual(1, prancha.Tabelas.Count, "Refresh nao deveria alterar Tabelas.Count da prancha");
            AssertEqual(instanciaId, prancha.Tabelas[0].Id, "Refresh nao deveria alterar instancia Id");
            AssertEqual(tabelaId, prancha.Tabelas[0].TableId, "Refresh nao deveria alterar instancia TableId");
            AssertEqual(40, prancha.Tabelas[0].X, "Refresh nao deveria alterar instancia X");
            AssertEqual("Tabela Renomeada", viewModel.TableInstances.Single().TableName, "Refresh deveria atualizar nome exibido");
        }

        private static void MoverTabelaNaPranchaMoveInstanciaValida()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance { TableId = tabela.Id, X = 10, Y = 20, Width = 200, Height = 100 };
            prancha.Tabelas.Add(instancia);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new MoverTabelaNaPranchaUseCase(document, commands);

            bool moveu = useCase.Mover(prancha.Id, instancia.Id, 75, 95);

            Assert(moveu, "Mover deveria retornar true para instancia valida.");
            AssertEqual(75, instancia.X, "Mover instancia X");
            AssertEqual(95, instancia.Y, "Mover instancia Y");
            AssertEqual(200, instancia.Width, "Mover nao deveria alterar Width");
            AssertEqual(100, instancia.Height, "Mover nao deveria alterar Height");
            AssertEqual(tabela.Id, instancia.TableId, "Mover nao deveria alterar TableId");
        }

        private static void MoverTabelaNaPranchaUndoRedo()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance { TableId = tabela.Id, X = 10, Y = 20 };
            prancha.Tabelas.Add(instancia);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new MoverTabelaNaPranchaUseCase(document, commands);

            useCase.Mover(prancha.Id, instancia.Id, 120, 140);
            commands.Undo();

            AssertEqual(10, instancia.X, "Undo movimento X");
            AssertEqual(20, instancia.Y, "Undo movimento Y");

            commands.Redo();

            AssertEqual(120, instancia.X, "Redo movimento X");
            AssertEqual(140, instancia.Y, "Redo movimento Y");
        }

        private static void MoverTabelaNaPranchaIdsInvalidosNaoAlteramEstado()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance { TableId = tabela.Id, X = 10, Y = 20 };
            prancha.Tabelas.Add(instancia);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new MoverTabelaNaPranchaUseCase(document, commands);

            bool semPrancha = useCase.Mover(Guid.NewGuid(), instancia.Id, 30, 40);
            bool semInstancia = useCase.Mover(prancha.Id, Guid.NewGuid(), 30, 40);
            bool coordenadaInvalida = useCase.Mover(prancha.Id, instancia.Id, double.NaN, 40);

            Assert(!semPrancha, "Mover com prancha invalida deveria retornar false.");
            Assert(!semInstancia, "Mover com instancia invalida deveria retornar false.");
            Assert(!coordenadaInvalida, "Mover com coordenada invalida deveria retornar false.");
            AssertEqual(10, instancia.X, "Ids invalidos nao deveriam alterar X");
            AssertEqual(20, instancia.Y, "Ids invalidos nao deveriam alterar Y");
            Assert(!commands.CanUndo, "Ids invalidos nao deveriam criar historico.");
        }

        private static void MoverTabelaNaPranchaSemAlteracaoNaoCriaComando()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance { TableId = tabela.Id, X = 10, Y = 20 };
            prancha.Tabelas.Add(instancia);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new MoverTabelaNaPranchaUseCase(document, commands);

            bool moveu = useCase.Mover(prancha.Id, instancia.Id, 10, 20);

            Assert(!moveu, "Mover sem alteracao deveria retornar false.");
            AssertEqual(10, instancia.X, "Mover sem alteracao X");
            AssertEqual(20, instancia.Y, "Mover sem alteracao Y");
            Assert(!commands.CanUndo, "Mover sem alteracao nao deveria criar historico.");
        }

        private static void MoverTabelaNaPranchaMoveApenasInstanciaAlvo()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instanciaA = new ProjectSheetTableInstance { TableId = tabela.Id, X = 10, Y = 20 };
            var instanciaB = new ProjectSheetTableInstance { TableId = tabela.Id, X = 30, Y = 40 };
            prancha.Tabelas.Add(instanciaA);
            prancha.Tabelas.Add(instanciaB);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new MoverTabelaNaPranchaUseCase(document, commands);

            bool moveu = useCase.Mover(prancha.Id, instanciaB.Id, 80, 90);

            Assert(moveu, "Mover instancia alvo deveria retornar true.");
            AssertEqual(10, instanciaA.X, "Instancia nao alvo X");
            AssertEqual(20, instanciaA.Y, "Instancia nao alvo Y");
            AssertEqual(80, instanciaB.X, "Instancia alvo X");
            AssertEqual(90, instanciaB.Y, "Instancia alvo Y");
        }

        private static void ProjectSheetViewModelSelecionaInstancia()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance { TableId = tabela.Id };
            prancha.Tabelas.Add(instancia);
            var viewModel = new ProjectSheetViewModel(document, prancha);

            viewModel.SelecionarInstancia(instancia.Id);

            AssertEqual(instancia.Id, viewModel.SelectedInstanceId, "SelectedInstanceId apos selecionar");
            Assert(viewModel.TableInstances.Single().IsSelected, "Instancia deveria ficar selecionada.");
        }

        private static void ProjectSheetViewModelLimpaSelecao()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance { TableId = tabela.Id };
            prancha.Tabelas.Add(instancia);
            var viewModel = new ProjectSheetViewModel(document, prancha);
            viewModel.SelecionarInstancia(instancia.Id);

            viewModel.LimparSelecao();

            Assert(viewModel.SelectedInstanceId == null, "SelectedInstanceId deveria ser null apos limpar.");
            Assert(!viewModel.TableInstances.Single().IsSelected, "Instancia nao deveria ficar selecionada apos limpar.");
        }

        private static void ProjectSheetViewModelRefreshPreservaSelecaoExistente()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance { TableId = tabela.Id };
            prancha.Tabelas.Add(instancia);
            var viewModel = new ProjectSheetViewModel(document, prancha);
            viewModel.SelecionarInstancia(instancia.Id);

            viewModel.Refresh();

            AssertEqual(instancia.Id, viewModel.SelectedInstanceId, "Refresh deveria preservar SelectedInstanceId");
            Assert(viewModel.TableInstances.Single().IsSelected, "Refresh deveria preservar IsSelected.");
        }

        private static void ProjectSheetViewModelRefreshRemoveSelecaoInexistente()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance { TableId = tabela.Id };
            prancha.Tabelas.Add(instancia);
            var viewModel = new ProjectSheetViewModel(document, prancha);
            viewModel.SelecionarInstancia(instancia.Id);

            prancha.Tabelas.Clear();
            viewModel.Refresh();

            Assert(viewModel.SelectedInstanceId == null, "Refresh deveria remover selecao inexistente.");
            AssertEqual(0, viewModel.TableInstances.Count, "Refresh apos remover instancia Count");
        }

        private static void ProjectSheetViewModelUsaDimensoesPrancha()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.FormatoFolha = ProjectSheetFormat.Personalizado;
            prancha.LarguraFolha = 700;
            prancha.AlturaFolha = 500;
            var commands = new Araci.Core.Commands.CommandManager();
            var editar = new EditarPropriedadesPranchaUseCase(document, commands);
            var viewModel = new ProjectSheetViewModel(document, prancha);

            AssertEqual(700, viewModel.SheetWidth, "ProjectSheetViewModel.SheetWidth inicial");
            AssertEqual(500, viewModel.SheetHeight, "ProjectSheetViewModel.SheetHeight inicial");
            AssertEqual(700 + viewModel.SheetOriginOffsetX * 2, viewModel.MinimumWorkspaceWidth, "ProjectSheetViewModel MinimumWorkspaceWidth inicial");
            AssertEqual(500 + viewModel.SheetOriginOffsetY * 2, viewModel.MinimumWorkspaceHeight, "ProjectSheetViewModel MinimumWorkspaceHeight inicial");

            bool alterou = editar.AlterarFormato(prancha.Id, ProjectSheetFormat.A4);

            Assert(alterou, "AlterarFormato deveria atualizar prancha observada.");
            AssertEqual(397, viewModel.SheetWidth, "ProjectSheetViewModel.SheetWidth apos formato");
            AssertEqual(280, viewModel.SheetHeight, "ProjectSheetViewModel.SheetHeight apos formato");
        }

        private static void ProjectSheetViewPossuiSuperficiePrancha()
        {
            RunSta(() =>
            {
                var view = new ProjectSheetView();
                var sheetSurface = view.FindName("SheetSurface") as Canvas;

                Assert(sheetSurface != null, "ProjectSheetView deveria possuir SheetSurface.");
                Assert(!sheetSurface!.ClipToBounds, "SheetSurface nao deveria recortar a area externa da folha.");
            });
        }

        private static void ProjectSheetViewModelPermiteInstanciaForaDaFolha()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var instancia = new ProjectSheetTableInstance
            {
                TableId = tabela.Id,
                X = -180,
                Y = -90,
                Width = 420,
                Height = 250
            };
            prancha.Tabelas.Add(instancia);

            var viewModel = new ProjectSheetViewModel(document, prancha);
            ProjectSheetTableInstanceViewModel instanceViewModel = viewModel.TableInstances.Single();

            AssertEqual(-180, instanceViewModel.X, "Instancia fora da folha X dominio");
            AssertEqual(-90, instanceViewModel.Y, "Instancia fora da folha Y dominio");
            Assert(instanceViewModel.ViewX >= 0, "Instancia fora a esquerda deveria continuar visivel no workspace.");
            Assert(instanceViewModel.ViewY >= 0, "Instancia fora acima deveria continuar visivel no workspace.");
            Assert(instanceViewModel.ViewX < viewModel.SheetOriginOffsetX, "ViewX deveria posicionar tabela antes da folha branca.");
            Assert(instanceViewModel.ViewY < viewModel.SheetOriginOffsetY, "ViewY deveria posicionar tabela acima da folha branca.");
            AssertEqual(viewModel.MinimumWorkspaceWidth, viewModel.WorkspaceWidth, "Workspace negativo deveria manter largura minima estavel.");
            AssertEqual(viewModel.MinimumWorkspaceHeight, viewModel.WorkspaceHeight, "Workspace negativo deveria manter altura minima estavel.");

            viewModel.SetPreviewPosition(instanceViewModel, viewModel.SheetWidth + 120, viewModel.SheetHeight + 80);

            Assert(instanceViewModel.X > viewModel.SheetWidth, "Preview deveria permitir X alem da folha.");
            Assert(instanceViewModel.Y > viewModel.SheetHeight, "Preview deveria permitir Y alem da folha.");
            Assert(viewModel.WorkspaceWidth > instanceViewModel.ViewX + instanceViewModel.Width, "Workspace deveria conter tabela a direita.");
            Assert(viewModel.WorkspaceHeight > instanceViewModel.ViewY + instanceViewModel.Height, "Workspace deveria conter tabela abaixo.");
        }

        private static void ProjectSheetViewModelMantemWorkspaceEstavelDurantePreviewNegativo()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Tabelas.Add(new ProjectSheetTableInstance
            {
                TableId = tabela.Id,
                X = 40,
                Y = 40,
                Width = 400,
                Height = 240
            });
            var viewModel = new ProjectSheetViewModel(document, prancha);
            ProjectSheetTableInstanceViewModel instance = viewModel.TableInstances.Single();
            double workspaceWidth = viewModel.WorkspaceWidth;
            double workspaceHeight = viewModel.WorkspaceHeight;
            double sheetOffsetX = viewModel.SheetOriginOffsetX;
            double sheetOffsetY = viewModel.SheetOriginOffsetY;

            viewModel.SetPreviewPosition(instance, -500, -360);

            AssertEqual(workspaceWidth, viewModel.WorkspaceWidth, "Preview negativo nao deveria alterar WorkspaceWidth");
            AssertEqual(workspaceHeight, viewModel.WorkspaceHeight, "Preview negativo nao deveria alterar WorkspaceHeight");
            AssertEqual(sheetOffsetX, viewModel.SheetOriginOffsetX, "Preview negativo nao deveria alterar SheetOriginOffsetX");
            AssertEqual(sheetOffsetY, viewModel.SheetOriginOffsetY, "Preview negativo nao deveria alterar SheetOriginOffsetY");
            AssertEqual(-500, instance.X, "Preview negativo X");
            AssertEqual(-360, instance.Y, "Preview negativo Y");
            Assert(instance.ViewX >= 0, "Preview negativo deveria permanecer dentro da margem visual esquerda.");
            Assert(instance.ViewY >= 0, "Preview negativo deveria permanecer dentro da margem visual superior.");
        }

        private static void ProjectSheetViewModelMantemWorkspaceEstavelDurantePreviewResize()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Tabelas.Add(new ProjectSheetTableInstance
            {
                TableId = tabela.Id,
                X = 40,
                Y = 40,
                Width = 400,
                Height = 240
            });
            var viewModel = new ProjectSheetViewModel(document, prancha);
            ProjectSheetTableInstanceViewModel instance = viewModel.TableInstances.Single();
            double workspaceWidth = viewModel.WorkspaceWidth;
            double workspaceHeight = viewModel.WorkspaceHeight;

            viewModel.SetPreviewSize(instance, 900, 620);

            AssertEqual(workspaceWidth, viewModel.WorkspaceWidth, "Preview resize nao deveria alterar WorkspaceWidth");
            AssertEqual(workspaceHeight, viewModel.WorkspaceHeight, "Preview resize nao deveria alterar WorkspaceHeight");
            AssertEqual(900, instance.Width, "Preview resize Width");
            AssertEqual(620, instance.Height, "Preview resize Height");
        }

        private static void ProjectSheetViewModelZoomAlteraPercentual()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var viewModel = new ProjectSheetViewModel(document, prancha);

            viewModel.ZoomIn();

            AssertEqual(1.1, viewModel.ZoomScale, "ZoomIn ZoomScale");
            AssertEqual("110%", viewModel.ZoomPercentText, "ZoomIn ZoomPercentText");

            viewModel.ZoomOut();
            viewModel.ZoomOut();

            AssertEqual(0.9, viewModel.ZoomScale, "ZoomOut ZoomScale");
            AssertEqual("90%", viewModel.ZoomPercentText, "ZoomOut ZoomPercentText");

            viewModel.ResetZoom();

            AssertEqual(1.0, viewModel.ZoomScale, "ResetZoom ZoomScale");
            AssertEqual("100%", viewModel.ZoomPercentText, "ResetZoom ZoomPercentText");
        }

        private static void ProjectSheetViewAplicaZoomNoWorkspace()
        {
            RunSta(() =>
            {
                var view = new ProjectSheetView();
                var zoomHost = view.FindName("ZoomHost") as Grid;
                var sheetSurface = view.FindName("SheetSurface") as Canvas;

                Assert(zoomHost != null, "ProjectSheetView deveria possuir ZoomHost.");
                Assert(sheetSurface != null, "ProjectSheetView deveria possuir SheetSurface Canvas.");
                Assert(zoomHost!.LayoutTransform is System.Windows.Media.ScaleTransform, "ZoomHost deveria usar LayoutTransform com ScaleTransform.");
                Assert(zoomHost.HorizontalAlignment == HorizontalAlignment.Left, "ZoomHost deveria iniciar a esquerda para scroll previsivel.");
                Assert(zoomHost.VerticalAlignment == VerticalAlignment.Top, "ZoomHost deveria iniciar no topo para scroll previsivel.");
                Assert(!sheetSurface!.ClipToBounds, "SheetSurface nao deveria recortar tabelas fora da folha.");
            });
        }

        private static void ProjectSheetViewPossuiFolhaNomeadaParaCentralizacao()
        {
            RunSta(() =>
            {
                var view = new ProjectSheetView();
                var sheetPageBorder = view.FindName("SheetPageBorder") as Border;

                Assert(sheetPageBorder != null, "ProjectSheetView deveria possuir SheetPageBorder para centralizar a folha real.");
            });
        }

        private static void ProjectSheetViewPossuiEstilosVisuaisBasicosTabela()
        {
            RunSta(() =>
            {
                var view = new ProjectSheetView();

                Assert(view.Resources["SheetTableTitleTextStyle"] is Style, "ProjectSheetView deveria possuir estilo visual para titulo da tabela.");
                Assert(view.Resources["SheetTableHeaderCellStyle"] is Style, "ProjectSheetView deveria possuir estilo visual para celula de cabecalho.");
                Assert(view.Resources["SheetTableHeaderTextStyle"] is Style, "ProjectSheetView deveria possuir estilo visual para texto de cabecalho.");
                Assert(view.Resources["SheetTableBodyCellStyle"] is Style, "ProjectSheetView deveria possuir estilo visual para celula de dados.");
                Assert(view.Resources["SheetTableEmptyMessageTextStyle"] is Style, "ProjectSheetView deveria possuir estilo visual para mensagem vazia.");
            });
        }

        private static void ProjectSheetTableInstanceViewModelExpoeDimensoesTabularesBasicas()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Tabelas.Add(new ProjectSheetTableInstance { TableId = tabela.Id });

            var viewModel = new ProjectSheetViewModel(document, prancha);
            ProjectSheetTableInstanceViewModel instance = viewModel.TableInstances.Single();

            Assert(instance.ColumnWidth > 0, "ColumnWidth deveria ser positivo.");
            Assert(instance.HeaderRowHeight > 0, "HeaderRowHeight deveria ser positivo.");
            Assert(instance.BodyRowHeight > 0, "BodyRowHeight deveria ser positivo.");
            Assert(instance.TitleHeight > 0, "TitleHeight deveria ser positivo.");
            Assert(instance.HeaderRowHeight >= instance.BodyRowHeight, "HeaderRowHeight deveria manter cabecalho legivel.");
        }

        private static void ProjectSheetViewUsaBindingsDimensoesTabulares()
        {
            string xaml = File.ReadAllText(FindProjectFile("Views/ProjectSheetView.xaml"), Encoding.UTF8);

            AssertContains(xaml, "Width=\"{Binding DataContext.ColumnWidth", "ProjectSheetView deveria vincular largura das colunas ao ViewModel.");
            AssertContains(xaml, "Height=\"{Binding DataContext.HeaderRowHeight", "ProjectSheetView deveria vincular altura do cabecalho ao ViewModel.");
            AssertContains(xaml, "Height=\"{Binding DataContext.BodyRowHeight", "ProjectSheetView deveria vincular altura das linhas ao ViewModel.");
            AssertContains(xaml, "Height=\"{Binding TitleHeight}\"", "ProjectSheetView deveria vincular altura do titulo ao ViewModel.");
            Assert(!xaml.Contains("<Setter Property=\"Width\" Value=\"112\"/>", StringComparison.Ordinal), "SheetTableHeaderCellStyle/BodyCellStyle nao deveriam fixar largura principal em XAML.");
            Assert(!xaml.Contains("<Setter Property=\"Height\" Value=\"26\"/>", StringComparison.Ordinal), "SheetTableHeaderCellStyle nao deveria fixar altura principal em XAML.");
            Assert(!xaml.Contains("<Setter Property=\"Height\" Value=\"24\"/>", StringComparison.Ordinal), "SheetTableBodyCellStyle nao deveria fixar altura principal em XAML.");
        }

        private static void ProjectSheetViewMantemBindingsDimensoesFolha()
        {
            string xaml = File.ReadAllText(FindProjectFile("Views/ProjectSheetView.xaml"), Encoding.UTF8);
            string propertiesXaml = File.ReadAllText(FindProjectFile("Properties/PropertiesHostView.xaml"), Encoding.UTF8);

            AssertContains(xaml, "Width=\"{Binding SheetWidth}\"", "ProjectSheetView deveria vincular largura da folha ao ViewModel.");
            AssertContains(xaml, "Height=\"{Binding SheetHeight}\"", "ProjectSheetView deveria vincular altura da folha ao ViewModel.");
            AssertContains(propertiesXaml, "ProjectSheetPropertiesViewModel", "PropertiesHostView deveria possuir template para propriedades da prancha.");
            AssertContains(propertiesXaml, "SelectedItem=\"{Binding FormatoFolha", "Template da prancha deveria editar formato.");
            AssertContains(propertiesXaml, "SelectedItem=\"{Binding OrientacaoFolha", "Template da prancha deveria editar orientacao.");
            AssertContains(propertiesXaml, "Text=\"{Binding LarguraFolha", "Template da prancha deveria editar largura.");
            AssertContains(propertiesXaml, "Text=\"{Binding AlturaFolha", "Template da prancha deveria editar altura.");
        }

        private static void ProjectSheetPropertiesViewModelExpoePropriedadesEditaveis()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            var commands = new Araci.Core.Commands.CommandManager();
            var renomear = new RenomearItemProjetoUseCase(document, commands);
            var editar = new EditarPropriedadesPranchaUseCase(document, commands);
            var viewModel = new ProjectSheetPropertiesViewModel(document, prancha, renomear, editar);

            viewModel.Numero = " EL-200 ";
            viewModel.Nome = " Prancha editada ";
            viewModel.FormatoFolha = ProjectSheetFormat.A2;
            viewModel.OrientacaoFolha = ProjectSheetOrientation.Retrato;
            viewModel.LarguraFolha = 620;
            viewModel.AlturaFolha = 880;

            Assert(viewModel.Formatos.Contains(ProjectSheetFormat.A0), "ProjectSheetPropertiesViewModel deveria expor formatos padrao.");
            Assert(viewModel.Formatos.Contains(ProjectSheetFormat.Personalizado), "ProjectSheetPropertiesViewModel deveria expor formato personalizado.");
            Assert(viewModel.Orientacoes.Contains(ProjectSheetOrientation.Paisagem), "ProjectSheetPropertiesViewModel deveria expor paisagem.");
            Assert(viewModel.Orientacoes.Contains(ProjectSheetOrientation.Retrato), "ProjectSheetPropertiesViewModel deveria expor retrato.");
            AssertEqual("EL-200", prancha.Numero, "ProjectSheetPropertiesViewModel Numero");
            AssertEqual("Prancha editada", prancha.Nome, "ProjectSheetPropertiesViewModel Nome");
            AssertEqual(ProjectSheetFormat.Personalizado, prancha.FormatoFolha, "ProjectSheetPropertiesViewModel dimensao manual formato");
            AssertEqual(ProjectSheetOrientation.Retrato, prancha.OrientacaoFolha, "ProjectSheetPropertiesViewModel Orientacao");
            AssertEqual(620, prancha.LarguraFolha, "ProjectSheetPropertiesViewModel LarguraFolha");
            AssertEqual(880, prancha.AlturaFolha, "ProjectSheetPropertiesViewModel AlturaFolha");
        }

        private static void DividirTabelaNaPranchaCriaInstanciaIndependente()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            ProjectSheet prancha = document.CriarNovaPrancha();
            var original = new ProjectSheetTableInstance { TableId = tabela.Id, X = 10, Y = 20, Width = 200, Height = 120 };
            prancha.Tabelas.Add(original);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new DividirTabelaNaPranchaUseCase(document, commands);

            ProjectSheetTableInstance? nova = useCase.Dividir(prancha.Id, original.Id);

            Assert(nova != null, "Dividir deveria criar nova instancia.");
            AssertEqual(2, prancha.Tabelas.Count, "Split Tabelas.Count");
            AssertEqual(tabela.Id, original.TableId, "Original TableId");
            AssertEqual(tabela.Id, nova!.TableId, "Nova TableId");
            AssertEqual(0, original.RowStartIndex, "Original RowStartIndex");
            AssertEqual(2, original.RowCount, "Original RowCount");
            AssertEqual(2, nova.RowStartIndex, "Nova RowStartIndex");
            AssertEqual(1, nova.RowCount, "Nova RowCount");
            AssertEqual(original.X + original.Width + DividirTabelaNaPranchaUseCase.DefaultSplitSpacing, nova.X, "Nova X");
            AssertEqual(original.Y, nova.Y, "Nova Y");
            AssertEqual(original.Width, nova.Width, "Nova Width");
            AssertEqual(original.Height, nova.Height, "Nova Height");
            Assert(commands.CanUndo, "Split deveria criar historico undo.");
        }

        private static void DividirTabelaNaPranchaUndoRedo()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            ProjectSheet prancha = document.CriarNovaPrancha();
            var original = new ProjectSheetTableInstance { TableId = tabela.Id };
            prancha.Tabelas.Add(original);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new DividirTabelaNaPranchaUseCase(document, commands);

            ProjectSheetTableInstance? nova = useCase.Dividir(prancha.Id, original.Id);
            Guid novaId = nova!.Id;

            commands.Undo();

            AssertEqual(1, prancha.Tabelas.Count, "Undo split Tabelas.Count");
            AssertEqual(original.Id, prancha.Tabelas[0].Id, "Undo preserva original");
            AssertEqual(0, original.RowStartIndex, "Undo original RowStartIndex");
            AssertEqual(null, original.RowCount, "Undo original RowCount");

            commands.Redo();

            AssertEqual(2, prancha.Tabelas.Count, "Redo split Tabelas.Count");
            AssertEqual(original.Id, prancha.Tabelas[0].Id, "Redo original Id");
            AssertEqual(novaId, prancha.Tabelas[1].Id, "Redo nova Id");
            AssertEqual(2, original.RowCount, "Redo original RowCount");
            AssertEqual(2, prancha.Tabelas[1].RowStartIndex, "Redo nova RowStartIndex");
        }

        private static void ProjectSheetViewModelRecortaLinhasPorFaixaInstancia()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Tabelas.Add(new ProjectSheetTableInstance { TableId = tabela.Id, RowStartIndex = 0, RowCount = 2 });
            prancha.Tabelas.Add(new ProjectSheetTableInstance { TableId = tabela.Id, RowStartIndex = 2, RowCount = 1 });

            var viewModel = new ProjectSheetViewModel(document, prancha);

            AssertEqual(2, viewModel.TableInstances.Count, "VM split TableInstances.Count");
            AssertEqual(2, viewModel.TableInstances[0].Rows.Count, "VM original Rows.Count");
            AssertEqual("Carga A", viewModel.TableInstances[0].Rows[0].Cells[0].DisplayValue, "VM original primeira linha");
            AssertEqual("Carga B", viewModel.TableInstances[0].Rows[1].Cells[0].DisplayValue, "VM original segunda linha");
            AssertEqual(1, viewModel.TableInstances[1].Rows.Count, "VM nova Rows.Count");
            AssertEqual("Carga C", viewModel.TableInstances[1].Rows[0].Cells[0].DisplayValue, "VM nova primeira linha");
            Assert(!viewModel.TableInstances[0].CanSplit, "Faixa limitada nao deveria permitir novo split nesta fase.");
            Assert(!viewModel.TableInstances[1].CanSplit, "Continuacao nao deveria permitir novo split nesta fase.");
        }

        private static void ProjectSheetViewModelDivideTabelaSelecionandoNovaInstancia()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            ProjectSheet prancha = document.CriarNovaPrancha();
            var original = new ProjectSheetTableInstance { TableId = tabela.Id };
            prancha.Tabelas.Add(original);
            var commands = new Araci.Core.Commands.CommandManager();
            var split = new DividirTabelaNaPranchaUseCase(document, commands);
            var viewModel = new ProjectSheetViewModel(document, prancha, dividirTabelaNaPrancha: split);

            bool dividiu = viewModel.DividirInstanciaTabela(original.Id);

            Assert(dividiu, "ProjectSheetViewModel deveria dividir instancia.");
            AssertEqual(2, viewModel.TableInstances.Count, "ProjectSheetViewModel apos split TableInstances.Count");
            AssertEqual(prancha.Tabelas[1].Id, viewModel.SelectedInstanceId, "ProjectSheetViewModel deveria selecionar nova instancia.");
            AssertEqual(2, viewModel.TableInstances[0].Rows.Count, "ProjectSheetViewModel original Rows.Count");
            AssertEqual(1, viewModel.TableInstances[1].Rows.Count, "ProjectSheetViewModel nova Rows.Count");
        }

        private static void ProjectSheetViewUsaInstanciaRealParaDivisaoTabela()
        {
            string xaml = File.ReadAllText(FindProjectFile("Views/ProjectSheetView.xaml"), Encoding.UTF8);
            string codeBehind = File.ReadAllText(FindProjectFile("Views/ProjectSheetView.xaml.cs"), Encoding.UTF8);

            Assert(!xaml.Contains("ItemsSource=\"{Binding Segments}\"", StringComparison.Ordinal), "ProjectSheetView nao deveria simular split com Segments internos.");
            Assert(!xaml.Contains("ItemsSource=\"{Binding Blocks}\"", StringComparison.Ordinal), "ProjectSheetView nao deveria simular split com Blocks internos.");
            Assert(!xaml.Contains("SelectedTableSplitButtonText", StringComparison.Ordinal), "ProjectSheetView nao deveria manter botao global de dividir/unir tabela.");
            Assert(!xaml.Contains("CanToggleSelectedTableSplit", StringComparison.Ordinal), "ProjectSheetView nao deveria habilitar split por botao global.");
            Assert(!xaml.Contains("ToggleSplitSelectedButton_Click", StringComparison.Ordinal), "ProjectSheetView nao deveria manter handler global de split.");
            AssertContains(xaml, "MouseLeftButtonDown=\"ToggleSplitInlineControl_MouseLeftButtonDown\"", "ProjectSheetView deveria possuir controle contextual de split na tabela.");
            AssertContains(xaml, "<Condition Binding=\"{Binding IsSelected}\" Value=\"True\"/>", "Controle contextual deveria aparecer apenas na tabela selecionada.");
            AssertContains(xaml, "<Condition Binding=\"{Binding CanSplit}\" Value=\"True\"/>", "Controle contextual deveria aparecer apenas quando a tabela puder ser dividida.");
            AssertContains(codeBehind, "ToggleSplitInlineControl_MouseLeftButtonDown", "ProjectSheetView code-behind deveria tratar o controle contextual de split.");
            AssertContains(codeBehind, "e.Handled = true", "Handler contextual deveria marcar o evento como tratado para evitar drag.");
            AssertContains(codeBehind, "ViewModel?.DividirInstanciaTabela(instance.Id)", "Handler contextual deveria chamar o fluxo oficial de divisao.");
            Assert(!codeBehind.Contains("instance.ToggleSplit()", StringComparison.Ordinal), "Handler contextual nao deveria alternar estado visual temporario.");
        }

        private static void ProjectSheetViewModelInstanciaRenderizaDadosReaisTabela()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Tabelas.Add(new ProjectSheetTableInstance { TableId = tabela.Id });

            var viewModel = new ProjectSheetViewModel(document, prancha);
            ProjectSheetTableInstanceViewModel instance = viewModel.TableInstances.Single();

            AssertEqual(3, instance.Columns.Count, "Prancha tabela Columns.Count");
            AssertEqual("Nome", instance.Columns[0].CampoId, "Prancha tabela coluna 0 CampoId");
            AssertEqual(3, instance.Rows.Count, "Prancha tabela Rows.Count");
            AssertEqual("Carga A", instance.Rows[0].Cells[0].DisplayValue, "Prancha tabela primeira celula");
            Assert(instance.HasRenderableTable, "Prancha tabela deveria ter dados renderizaveis.");
            Assert(!instance.HasEmptyDataMessage, "Prancha tabela com dados nao deveria exibir mensagem vazia.");
        }

        private static void ProjectSheetViewModelInstanciaTrataTabelaSemCampos()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.CamposSelecionados.Clear();
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Tabelas.Add(new ProjectSheetTableInstance { TableId = tabela.Id });

            var viewModel = new ProjectSheetViewModel(document, prancha);
            ProjectSheetTableInstanceViewModel instance = viewModel.TableInstances.Single();

            AssertEqual(0, instance.Columns.Count, "Prancha tabela sem campos Columns.Count");
            AssertEqual(0, instance.Rows.Count, "Prancha tabela sem campos Rows.Count");
            Assert(!instance.HasRenderableTable, "Prancha tabela sem campos nao deveria renderizar grade.");
            AssertEqual("Nenhum campo selecionado", instance.EmptyDataMessage, "Prancha tabela sem campos EmptyDataMessage");
        }

        private static void ProjectSheetViewModelInstanciaTrataTabelaSemLinhas()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.Filtros = new List<ProjectTableFilterRule>
            {
                new() { Ordem = 0, Categoria = ProjectTableElementCategory.Cargas, CampoId = "Nome", NomeExibicao = "Nome", Operador = ProjectTableFilterOperator.IgualA, Valor = "Nao existe" }
            };
            ProjectSheet prancha = document.CriarNovaPrancha();
            prancha.Tabelas.Add(new ProjectSheetTableInstance { TableId = tabela.Id });

            var viewModel = new ProjectSheetViewModel(document, prancha);
            ProjectSheetTableInstanceViewModel instance = viewModel.TableInstances.Single();

            AssertEqual(3, instance.Columns.Count, "Prancha tabela sem linhas Columns.Count");
            AssertEqual(0, instance.Rows.Count, "Prancha tabela sem linhas Rows.Count");
            Assert(!instance.HasRenderableTable, "Prancha tabela sem linhas nao deveria renderizar grade.");
            AssertEqual("Nenhum item encontrado", instance.EmptyDataMessage, "Prancha tabela sem linhas EmptyDataMessage");
        }

        private static void TabelaDataViewModelExpoeColunasLinhasECelulas()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            var viewModel = new ProjectTableDataViewModel(document, tabela);

            AssertEqual(tabela.Nome, viewModel.Titulo, "ViewModel.Titulo");
            AssertEqual(3, viewModel.Columns.Count, "ViewModel.Columns.Count");
            AssertEqual(3, viewModel.Rows.Count, "ViewModel.Rows.Count");
            AssertEqual("Carga A", viewModel.Rows[0][0], "ViewModel.Rows[0][0]");
            AssertEqual("500", viewModel.Rows[0][1], "ViewModel.Rows[0][1]");
            Assert(!viewModel.HasEmptyMessage, "ViewModel nao deveria exibir estado vazio.");
        }

        private static void TabelaDataViewModelTrataTabelaSemCampos()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.CamposSelecionados.Clear();

            var viewModel = new ProjectTableDataViewModel(document, tabela);

            AssertEqual(0, viewModel.Columns.Count, "Tabela sem campos Columns.Count");
            AssertEqual(0, viewModel.Rows.Count, "Tabela sem campos Rows.Count");
            AssertEqual("Nenhum campo selecionado", viewModel.EmptyMessage, "Tabela sem campos EmptyMessage");
            Assert(viewModel.HasEmptyMessage, "Tabela sem campos deveria exibir estado vazio.");
        }

        private static void TabelaDataViewModelTrataTabelaSemLinhas()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            tabela.Filtros = new List<ProjectTableFilterRule>
            {
                new() { Ordem = 0, Categoria = ProjectTableElementCategory.Cargas, CampoId = "Nome", NomeExibicao = "Nome", Operador = ProjectTableFilterOperator.IgualA, Valor = "Nao existe" }
            };

            var viewModel = new ProjectTableDataViewModel(document, tabela);

            AssertEqual(3, viewModel.Columns.Count, "Tabela sem linhas Columns.Count");
            AssertEqual(0, viewModel.Rows.Count, "Tabela sem linhas Rows.Count");
            AssertEqual("Nenhum item encontrado", viewModel.EmptyMessage, "Tabela sem linhas EmptyMessage");
        }

        private static void TabelaDataViewModelRefreshAtualizaDados()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            var viewModel = new ProjectTableDataViewModel(document, tabela);

            tabela.CamposSelecionados = tabela.CamposSelecionados.Take(1).ToList();
            viewModel.Refresh();

            AssertEqual(1, viewModel.Columns.Count, "Refresh Columns.Count");
            AssertEqual("Nome", viewModel.Columns[0].CampoId, "Refresh Columns[0].CampoId");
            AssertEqual("Carga A", viewModel.Rows[0][0], "Refresh Rows[0][0]");
        }

        private static void TabelaDataViewModelRefreshReativoAposUseCases()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new EditarPropriedadesTabelaUseCase(document, commands);
            var viewModel = new ProjectTableDataViewModel(document, tabela);
            document.PropriedadesTabelaAlteradas += tabelaAlterada =>
            {
                if (tabelaAlterada.Id == viewModel.TableId)
                    viewModel.Refresh();
            };

            useCase.AlterarElementosTabela(
                tabela.Id,
                tabela.CategoriasElementos,
                tabela.CamposSelecionados.Take(1).ToList());

            AssertEqual(1, viewModel.Columns.Count, "Refresh reativo apos campos Columns.Count");
            AssertEqual("Nome", viewModel.Columns[0].CampoId, "Refresh reativo apos campos Columns[0].CampoId");

            useCase.AlterarElementosTabela(
                tabela.Id,
                tabela.CategoriasElementos,
                CriarCamposTabelaDadosCarga());

            useCase.AlterarFiltrosTabela(
                tabela.Id,
                null,
                ProjectTableFilterLogicalMode.Todas,
                new[]
                {
                    new ProjectTableFilterRule
                    {
                        Ordem = 0,
                        Categoria = ProjectTableElementCategory.Cargas,
                        CampoId = "Nome",
                        NomeExibicao = "Nome",
                        Operador = ProjectTableFilterOperator.IgualA,
                        Valor = "Carga B"
                    }
                });

            AssertEqual(1, viewModel.Rows.Count, "Refresh reativo apos filtros Rows.Count");
            AssertEqual("Carga B", viewModel.Rows[0][0], "Refresh reativo apos filtros Rows[0][0]");

            useCase.AlterarFiltrosTabela(
                tabela.Id,
                null,
                ProjectTableFilterLogicalMode.Todas,
                Array.Empty<ProjectTableFilterRule>());

            useCase.AlterarOrdenacaoTabela(
                tabela.Id,
                new[]
                {
                    new ProjectTableSorting
                    {
                        Ordem = 0,
                        Categoria = ProjectTableElementCategory.Cargas,
                        CampoId = "PotenciaAtiva",
                        NomeExibicao = "Potencia ativa",
                        Direcao = ProjectTableSortDirection.Decrescente
                    }
                });

            AssertEqual("Carga B", viewModel.Rows[0][0], "Refresh reativo apos ordenacao Rows[0][0]");
            AssertEqual("800", viewModel.Rows[0][1], "Refresh reativo apos ordenacao Rows[0][1]");

            ProjectView vistaAtiva = document.Vistas[0];
            useCase.AlterarFiltrosTabela(
                tabela.Id,
                vistaAtiva.Id,
                ProjectTableFilterLogicalMode.Todas,
                Array.Empty<ProjectTableFilterRule>());

            AssertEqual(2, viewModel.Rows.Count, "Refresh reativo apos filtro de vista Rows.Count");
            Assert(viewModel.Rows.All(row => row[0] != "Carga B"), "Filtro de vista deveria remover Carga B.");
        }

        private static void FiltrosTabelaWindowPermiteSemFiltro()
        {
            RunSta(() =>
            {
                ProjectTableFilterRule filtroExistente = new()
                {
                    Ordem = 0,
                    Categoria = ProjectTableElementCategory.Cargas,
                    CampoId = "Nome",
                    NomeExibicao = "Nome",
                    Operador = ProjectTableFilterOperator.Contem,
                    Valor = "Carga"
                };
                var window = new Araci.Properties.FiltrosTabelaWindow(
                    CriarCamposTabelaDadosCarga(),
                    Array.Empty<ProjectViewDialogOption>(),
                    null,
                    ProjectTableFilterLogicalMode.Todas,
                    new[] { filtroExistente });
                var campo1 = (ComboBox)window.FindName("Campo1ComboBox");
                var campo2 = (ComboBox)window.FindName("Campo2ComboBox");
                var campo3 = (ComboBox)window.FindName("Campo3ComboBox");
                var valor1 = (TextBox)window.FindName("Valor1TextBox");
                var valor2 = (TextBox)window.FindName("Valor2TextBox");
                var valor3 = (TextBox)window.FindName("Valor3TextBox");

                AssertEqual("Sem filtro", ObterTextoItem(campo1.Items[0]), "Primeira opcao do parametro");
                AssertEqual("Nome", ObterCampoIdItem(campo1.SelectedItem!), "Filtro existente CampoId");
                AssertEqual("Carga", valor1.Text, "Filtro existente Valor");

                campo1.SelectedIndex = 0;
                IReadOnlyList<ProjectTableFilterRule> filtros = ObterFiltrosTabelaWindow(window);
                AssertEqual(0, filtros.Count, "Linha com Sem filtro nao deveria gerar regra");

                campo1.SelectedIndex = 1;
                valor1.Text = "Carga A";
                campo2.SelectedIndex = 0;
                valor2.Text = "Ignorado";
                campo3.SelectedIndex = 2;
                valor3.Text = "500";

                filtros = ObterFiltrosTabelaWindow(window);

                AssertEqual(2, filtros.Count, "Linhas validas com intermediaria Sem filtro");
                AssertEqual(0, filtros[0].Ordem, "Filtro valido 1 Ordem");
                AssertEqual("Nome", filtros[0].CampoId, "Filtro valido 1 CampoId");
                AssertEqual(1, filtros[1].Ordem, "Filtro valido 2 Ordem");
                AssertEqual("PotenciaAtiva", filtros[1].CampoId, "Filtro valido 2 CampoId");
            });
        }

        private static void TabelaRemoveFiltroComUndoRedo()
        {
            AraciDocument document = CriarDocumentoTabelaDados();
            ProjectTable tabela = CriarTabelaDadosCarga(document);
            var commands = new Araci.Core.Commands.CommandManager();
            var useCase = new EditarPropriedadesTabelaUseCase(document, commands);
            tabela.Filtros = new List<ProjectTableFilterRule>
            {
                new()
                {
                    Ordem = 0,
                    Categoria = ProjectTableElementCategory.Cargas,
                    CampoId = "Nome",
                    NomeExibicao = "Nome",
                    Operador = ProjectTableFilterOperator.Contem,
                    Valor = "Carga"
                }
            };

            bool alterado = useCase.AlterarFiltrosTabela(
                tabela.Id,
                null,
                ProjectTableFilterLogicalMode.Todas,
                Array.Empty<ProjectTableFilterRule>());

            Assert(alterado, "AlterarFiltrosTabela deveria remover filtro.");
            AssertEqual(0, tabela.Filtros.Count, "Filtros apos remover");

            commands.Undo();
            AssertEqual(1, tabela.Filtros.Count, "Undo deveria restaurar filtro.");
            AssertEqual("Nome", tabela.Filtros[0].CampoId, "Undo CampoId");

            commands.Redo();
            AssertEqual(0, tabela.Filtros.Count, "Redo deveria remover filtro novamente.");
        }

        private static void ProjectBrowserSelecionaTabelaESolicitaVisualizacao()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            Guid? tabelaVisualizada = null;
            Guid? tabelaPropriedades = null;
            var viewModel = new ProjectBrowserViewModel(
                document,
                abrirTabela: id => tabelaVisualizada = id,
                abrirPropriedadesTabela: id => tabelaPropriedades = id);

            ProjectBrowserItemViewModel itemTabela = viewModel.Secoes
                .SelectMany(secao => secao.Itens)
                .Single(item => item.Tipo == "Tabela" && item.Id == tabela.Id);

            itemTabela.SelecionarCommand.Execute(null);

            AssertEqual(tabela.Id, tabelaVisualizada, "Tabela visualizada");
            AssertEqual(tabela.Id, tabelaPropriedades, "Tabela propriedades");
        }

        private static void ProjectBrowserSelecionaPranchaESolicitaVisualizacao()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            Guid? pranchaVisualizada = null;
            Guid? tabelaVisualizada = null;
            Guid? vistaAtiva = null;
            var viewModel = new ProjectBrowserViewModel(
                document,
                definirVistaAtiva: id => vistaAtiva = id,
                abrirTabela: id => tabelaVisualizada = id,
                abrirPrancha: id => pranchaVisualizada = id);

            ProjectBrowserItemViewModel itemPrancha = viewModel.Secoes
                .SelectMany(secao => secao.Itens)
                .Single(item => item.Tipo == "Prancha" && item.Id == prancha.Id);

            itemPrancha.SelecionarCommand.Execute(null);

            AssertEqual(prancha.Id, pranchaVisualizada, "Prancha visualizada");
            Assert(tabelaVisualizada == null, "Selecionar prancha nao deveria solicitar tabela.");
            Assert(vistaAtiva == null, "Selecionar prancha nao deveria ativar vista.");
            Assert(itemPrancha.IsSelected, "Prancha deveria ficar selecionada no Project Browser.");
        }

        private static void ProjectBrowserSelecionaVistaERestauraViewport()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectView vista = document.Vistas[0];
            Guid? tabelaVisualizada = null;
            Guid? vistaAtiva = null;
            Guid? vistaPropriedades = null;
            var viewModel = new ProjectBrowserViewModel(
                document,
                definirVistaAtiva: id =>
                {
                    document.DefinirVistaAtiva(id);
                    vistaAtiva = id;
                },
                abrirTabela: id => tabelaVisualizada = id,
                abrirPropriedadesVista: id => vistaPropriedades = id);

            ProjectBrowserItemViewModel itemTabela = viewModel.Secoes
                .SelectMany(secao => secao.Itens)
                .Single(item => item.Tipo == "Tabela" && item.Id == tabela.Id);
            ProjectBrowserItemViewModel itemVista = viewModel.Secoes
                .SelectMany(secao => secao.Itens)
                .Single(item => item.Tipo == "Vista" && item.Id == vista.Id);

            itemTabela.SelecionarCommand.Execute(null);
            itemVista.SelecionarCommand.Execute(null);

            AssertEqual(tabela.Id, tabelaVisualizada, "Tabela visualizada antes de voltar para vista");
            AssertEqual(vista.Id, vistaAtiva, "Vista ativa apos selecionar vista");
            AssertEqual(vista.Id, vistaPropriedades, "Vista propriedades apos selecionar vista");
            Assert(itemVista.IsSelected, "Vista deveria ficar selecionada no Project Browser.");
            Assert(!itemTabela.IsSelected, "Tabela deveria deixar de ficar selecionada no Project Browser.");
            Assert(itemVista.IsActiveView, "Vista selecionada deveria ficar marcada como ativa.");
        }

        private static void ProjectBrowserSelecionaVistaDepoisDePrancha()
        {
            var document = new AraciDocument();
            ProjectSheet prancha = document.CriarNovaPrancha();
            ProjectView vista = document.Vistas[0];
            Guid? pranchaVisualizada = null;
            Guid? vistaAtiva = null;
            Guid? vistaPropriedades = null;
            var viewModel = new ProjectBrowserViewModel(
                document,
                definirVistaAtiva: id =>
                {
                    document.DefinirVistaAtiva(id);
                    vistaAtiva = id;
                },
                abrirPrancha: id => pranchaVisualizada = id,
                abrirPropriedadesVista: id => vistaPropriedades = id);

            ProjectBrowserItemViewModel itemPrancha = viewModel.Secoes
                .SelectMany(secao => secao.Itens)
                .Single(item => item.Tipo == "Prancha" && item.Id == prancha.Id);
            ProjectBrowserItemViewModel itemVista = viewModel.Secoes
                .SelectMany(secao => secao.Itens)
                .Single(item => item.Tipo == "Vista" && item.Id == vista.Id);

            itemPrancha.SelecionarCommand.Execute(null);
            itemVista.SelecionarCommand.Execute(null);

            AssertEqual(prancha.Id, pranchaVisualizada, "Prancha visualizada antes de voltar para vista");
            AssertEqual(vista.Id, vistaAtiva, "Vista ativa apos prancha");
            AssertEqual(vista.Id, vistaPropriedades, "Vista propriedades apos prancha");
            Assert(itemVista.IsSelected, "Vista deveria ficar selecionada apos prancha.");
            Assert(!itemPrancha.IsSelected, "Prancha deveria deixar de ficar selecionada apos vista.");
        }

        private static void ProjectBrowserSelecionaTabelaDepoisDePrancha()
        {
            var document = new AraciDocument();
            ProjectTable tabela = document.CriarNovaTabela();
            ProjectSheet prancha = document.CriarNovaPrancha();
            Guid? pranchaVisualizada = null;
            Guid? tabelaVisualizada = null;
            Guid? tabelaPropriedades = null;
            var viewModel = new ProjectBrowserViewModel(
                document,
                abrirTabela: id => tabelaVisualizada = id,
                abrirPrancha: id => pranchaVisualizada = id,
                abrirPropriedadesTabela: id => tabelaPropriedades = id);

            ProjectBrowserItemViewModel itemPrancha = viewModel.Secoes
                .SelectMany(secao => secao.Itens)
                .Single(item => item.Tipo == "Prancha" && item.Id == prancha.Id);
            ProjectBrowserItemViewModel itemTabela = viewModel.Secoes
                .SelectMany(secao => secao.Itens)
                .Single(item => item.Tipo == "Tabela" && item.Id == tabela.Id);

            itemPrancha.SelecionarCommand.Execute(null);
            itemTabela.SelecionarCommand.Execute(null);

            AssertEqual(prancha.Id, pranchaVisualizada, "Prancha visualizada antes de abrir tabela");
            AssertEqual(tabela.Id, tabelaVisualizada, "Tabela visualizada apos prancha");
            AssertEqual(tabela.Id, tabelaPropriedades, "Tabela propriedades apos prancha");
            Assert(itemTabela.IsSelected, "Tabela deveria ficar selecionada apos prancha.");
            Assert(!itemPrancha.IsSelected, "Prancha deveria deixar de ficar selecionada apos tabela.");
        }

        private static void ProjectTableGridViewRecriaColunasDinamicas()
        {
            RunSta(() =>
            {
                AraciDocument document = CriarDocumentoTabelaDados();
                ProjectTable tabelaA = CriarTabelaDadosCarga(document);
                ProjectTable tabelaB = CriarTabelaDadosCarga(document);
                tabelaB.Nome = "Tabela B";
                tabelaB.CamposSelecionados = CriarCamposTabelaDadosCarga().Take(1).ToList();

                var view = new ProjectTableGridView();
                var vmA = new ProjectTableDataViewModel(document, tabelaA);
                var vmB = new ProjectTableDataViewModel(document, tabelaB);
                var grid = (DataGrid)view.FindName("TableDataGrid");
                var separator = (Border)view.FindName("TableTopSeparator");

                view.DataContext = vmA;
                AssertEqual(3, grid.Columns.Count, "Grid colunas tabela A");
                AssertEqual("Nome", grid.Columns[0].Header, "Grid tabela A coluna 0");
                Assert(grid.IsReadOnly, "DataGrid deveria permanecer read-only.");
                Assert(!grid.CanUserAddRows, "DataGrid nao deveria permitir adicionar linhas.");
                Assert(!grid.CanUserDeleteRows, "DataGrid nao deveria permitir deletar linhas.");
                Assert(!grid.CanUserSortColumns, "DataGrid nao deveria ordenar pelo cabecalho.");
                AssertEqual(HorizontalAlignment.Left, grid.HorizontalAlignment, "DataGrid alinhamento horizontal");
                AssertEqual(DataGridGridLinesVisibility.Vertical, grid.GridLinesVisibility, "DataGrid linhas horizontais globais");
                Assert(grid.CellStyle != null, "DataGrid deveria ter CellStyle para linhas por celula.");
                AssertEqual(new Thickness(0, 1, 0, 0), separator.BorderThickness, "Separador superior da tabela");

                view.DataContext = vmB;
                AssertEqual(1, grid.Columns.Count, "Grid colunas tabela B apos troca");
                AssertEqual("Nome", grid.Columns[0].Header, "Grid tabela B coluna 0");

                tabelaB.CamposSelecionados = CriarCamposTabelaDadosCarga().Take(2).ToList();
                vmB.Refresh();

                AssertEqual(2, grid.Columns.Count, "Grid colunas tabela B apos refresh");
                AssertEqual("Nome", grid.Columns[0].Header, "Grid tabela B refresh coluna 0");
                AssertEqual("Potencia ativa", grid.Columns[1].Header, "Grid tabela B refresh coluna 1");

                view.DataContext = null;
                AssertEqual(0, grid.Columns.Count, "Grid colunas apos limpar DataContext");
            });
        }

        private static ProjectTable CriarTabelaComCampos(AraciDocument document)
        {
            ProjectTable tabela = document.CriarNovaTabela();
            tabela.CategoriasElementos = new List<ProjectTableElementCategory> { ProjectTableElementCategory.Barras };
            tabela.CamposSelecionados = new List<ProjectTableFieldSelection>
            {
                new() { Categoria = ProjectTableElementCategory.Barras, CampoId = "Nome", NomeExibicao = "Nome", Ordem = 0 },
                new() { Categoria = ProjectTableElementCategory.Barras, CampoId = "Tensao", NomeExibicao = "Tensao", Ordem = 1 },
                new() { Categoria = ProjectTableElementCategory.Barras, CampoId = "Corrente", NomeExibicao = "Corrente", Ordem = 2 },
                new() { Categoria = ProjectTableElementCategory.Barras, CampoId = "PotenciaAtiva", NomeExibicao = "Potencia ativa", Ordem = 3 },
                new() { Categoria = ProjectTableElementCategory.Barras, CampoId = "Tipo", NomeExibicao = "Tipo", Ordem = 4 },
                new() { Categoria = ProjectTableElementCategory.Barras, CampoId = "Comprimento", NomeExibicao = "Comprimento", Ordem = 5 }
            };

            return tabela;
        }

        private static AraciDocument CriarDocumentoTabelaDados()
        {
            var document = new AraciDocument();
            ProjectView vistaA = document.Vistas[0];
            ProjectView vistaB = document.CriarNovaVista();
            vistaB.Nome = "Vista filtro";

            document.Elementos.Add(new Carga
            {
                Nome = "Carga A",
                PotenciaAtiva = 500,
                TensaoLinha = "13.8",
                ViewId = vistaA.Id
            });
            document.Elementos.Add(new Carga
            {
                Nome = "Carga B",
                PotenciaAtiva = 800,
                TensaoLinha = "13.8",
                ViewId = vistaB.Id
            });
            document.Elementos.Add(new Carga
            {
                Nome = "Carga C",
                PotenciaAtiva = 500,
                TensaoLinha = "0.38",
                ViewId = vistaA.Id
            });
            document.Elementos.Add(new Gerador
            {
                Nome = "Gerador A",
                PotenciaAtiva = 1000,
                TensaoLinha = "13.8",
                ViewId = vistaA.Id
            });

            return document;
        }

        private static ProjectTable CriarTabelaDadosCarga(AraciDocument document)
        {
            ProjectTable tabela = document.CriarNovaTabela();
            tabela.CategoriasElementos = new List<ProjectTableElementCategory>
            {
                ProjectTableElementCategory.Cargas
            };
            tabela.CamposSelecionados = CriarCamposTabelaDadosCarga();

            return tabela;
        }

        private static List<ProjectTableFieldSelection> CriarCamposTabelaDadosCarga()
        {
            return new List<ProjectTableFieldSelection>
            {
                new() { Categoria = ProjectTableElementCategory.Cargas, CampoId = "Nome", NomeExibicao = "Nome", Ordem = 0 },
                new() { Categoria = ProjectTableElementCategory.Cargas, CampoId = "PotenciaAtiva", NomeExibicao = "Potencia ativa", Ordem = 1 },
                new() { Categoria = ProjectTableElementCategory.Cargas, CampoId = "Tensao", NomeExibicao = "Tensao", Ordem = 2 }
            };
        }

        private static string CriarAssinaturaTabelaDados(AraciDocument document, ProjectTable tabela)
        {
            string elementos = string.Join(
                ";",
                document.Elementos.Select(e => $"{e.Id}:{e.Nome}:{e.ViewId}:{e.PosicaoX}:{e.PosicaoY}"));
            string campos = string.Join(
                ";",
                tabela.CamposSelecionados.Select(c => $"{c.Ordem}:{c.Categoria}:{c.CampoId}:{c.NomeExibicao}"));
            string filtros = string.Join(
                ";",
                tabela.Filtros.Select(f => $"{f.Ordem}:{f.Categoria}:{f.CampoId}:{f.Operador}:{f.Valor}"));
            string ordenacoes = string.Join(
                ";",
                tabela.Ordenacoes.Select(o => $"{o.Ordem}:{o.Categoria}:{o.CampoId}:{o.Direcao}"));

            return $"{document.Elementos.Count}|{elementos}|{tabela.FiltroVistaId}|{tabela.ModoFiltro}|{campos}|{filtros}|{ordenacoes}";
        }

        private static string NormalizarQuebras(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        private static IReadOnlyList<ProjectTableSorting> CriarOrdenacoes(ProjectTable tabela, params string[] campoIds)
        {
            return campoIds
                .Select((campoId, index) =>
                {
                    ProjectTableFieldSelection campo = tabela.CamposSelecionados.Single(c => c.CampoId == campoId);
                    return new ProjectTableSorting
                    {
                        Ordem = index,
                        Categoria = campo.Categoria,
                        CampoId = campo.CampoId,
                        NomeExibicao = campo.NomeExibicao,
                        Direcao = index % 2 == 0
                            ? ProjectTableSortDirection.Crescente
                            : ProjectTableSortDirection.Decrescente
                    };
                })
                .ToList();
        }

        private static IReadOnlyList<ProjectTableFieldSelection> CopiarCamposTabela(IReadOnlyList<ProjectTableFieldSelection> campos)
        {
            return campos
                .Select(c => new ProjectTableFieldSelection
                {
                    Categoria = c.Categoria,
                    CampoId = c.CampoId,
                    NomeExibicao = c.NomeExibicao,
                    Ordem = c.Ordem
                })
                .ToList();
        }

        private static ProjectSerializer CriarProjectSerializerTeste()
        {
            EditorContext context = new();

            return new ProjectSerializer(
                context.Elements,
                new ElementoModelFactory(context.Elements),
                context.TerminalLayout,
                context.Geometry);
        }

        private static ExecutarSimulacaoUseCase CriarExecutarSimulacaoUseCase(
            FakeSimulationPipeline pipeline,
            FakeDialogService dialogs)
        {
            return new ExecutarSimulacaoUseCase(
                pipeline,
                new SimulationExportService(),
                new SimulationMessageBuilder(),
                dialogs);
        }

        private sealed class FakeSimulationPipeline : ISimulationPipeline
        {
            private readonly SimulationResultDto _resultado;

            public FakeSimulationPipeline()
                : this(new SimulationResultDto
                {
                    Sucesso = true,
                    Mensagem = "ok",
                    Script = "new circuit"
                })
            {
            }

            public FakeSimulationPipeline(SimulationResultDto resultado)
            {
                _resultado = resultado;
            }

            public int ExecutarFluxoDeCorrenteChamadas { get; private set; }
            public Exception? ExceptionToThrow { get; set; }

            public Task<SimulationResultDto> ExecutarFluxoDeCorrenteAsync()
            {
                ExecutarFluxoDeCorrenteChamadas++;

                if (ExceptionToThrow != null)
                    throw ExceptionToThrow;

                return Task.FromResult(_resultado);
            }
        }

        private sealed class ThrowingCsvExportService : IProjectTableCsvExportService
        {
            public string GenerateCsv(ProjectTableDataResult result)
            {
                throw new IOException("falha simulada");
            }
        }

        private sealed class FakeDialogService : IUserDialogService
        {
            public int InfoChamadas { get; private set; }
            public int WarningChamadas { get; private set; }
            public int ErrorChamadas { get; private set; }
            public int ConfirmChamadas { get; private set; }
            public int ShowMessageChamadas { get; private set; }
            public int SaveCsvChamadas { get; private set; }
            public string? SaveCsvPath { get; set; }
            public string? LastWarningTitle { get; private set; }
            public string? LastWarningMessage { get; private set; }
            public string? LastErrorTitle { get; private set; }
            public string? LastErrorMessage { get; private set; }
            public SimulationMessage? LastSimulationMessage { get; private set; }

            public void ShowInfo(string title, string message)
            {
                InfoChamadas++;
            }

            public void ShowWarning(string title, string message)
            {
                WarningChamadas++;
                LastWarningTitle = title;
                LastWarningMessage = message;
            }

            public void ShowError(string title, string message)
            {
                ErrorChamadas++;
                LastErrorTitle = title;
                LastErrorMessage = message;
            }

            public string? ShowSaveCsvDialog(string suggestedFileName)
            {
                SaveCsvChamadas++;
                return SaveCsvPath;
            }

            public InserirTabelaPranchaDialogResult? ShowInserirTabelaPranchaDialog(
                IReadOnlyList<ProjectItemDialogOption> pranchas,
                IReadOnlyList<ProjectItemDialogOption> tabelas)
            {
                return pranchas.Count > 0 && tabelas.Count > 0
                    ? new InserirTabelaPranchaDialogResult(pranchas[0].Id, tabelas.Select(t => t.Id))
                    : null;
            }

            public ElementosTabelaDialogResult? ShowElementosTabelaDialog(
                IReadOnlyList<ProjectTableElementCategory> categorias,
                IReadOnlyList<ProjectTableFieldSelection> camposSelecionados)
            {
                return new ElementosTabelaDialogResult(categorias, camposSelecionados);
            }

            public FiltrosTabelaDialogResult? ShowFiltrosTabelaDialog(
                IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
                IReadOnlyList<ProjectViewDialogOption> vistasDisponiveis,
                Guid? filtroVistaId,
                ProjectTableFilterLogicalMode modo,
                IReadOnlyList<ProjectTableFilterRule> filtros)
            {
                return new FiltrosTabelaDialogResult(filtroVistaId, modo, filtros);
            }

            public OrdenacaoTabelaDialogResult? ShowOrdenacaoTabelaDialog(
                IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
                IReadOnlyList<ProjectTableSorting> ordenacoes)
            {
                return new OrdenacaoTabelaDialogResult(ordenacoes);
            }

            public bool Confirm(string title, string message)
            {
                ConfirmChamadas++;
                return true;
            }

            public void Show(SimulationMessage message)
            {
                ShowMessageChamadas++;
                LastSimulationMessage = message;
            }
        }

        private sealed class FakeProjectPersistenceService : IProjectPersistenceService
        {
            public int NovoChamadas { get; private set; }
            public int SalvarComDialogoChamadas { get; private set; }
            public int AbrirComDialogoChamadas { get; private set; }
            public int SalvarChamadas { get; private set; }
            public int AbrirChamadas { get; private set; }
            public string? LastSalvarPath { get; private set; }
            public string? LastAbrirPath { get; private set; }

            public void Novo()
            {
                NovoChamadas++;
            }

            public void SalvarComDialogo()
            {
                SalvarComDialogoChamadas++;
            }

            public void AbrirComDialogo()
            {
                AbrirComDialogoChamadas++;
            }

            public void Salvar(string path)
            {
                SalvarChamadas++;
                LastSalvarPath = path;
            }

            public void Abrir(string path)
            {
                AbrirChamadas++;
                LastAbrirPath = path;
            }
        }

        private static AraciDocument SaveAndLoad(AraciDocument document)
        {
            string path = CreateTempProjectPath();

            try
            {
                var source = new EditorContext();

                foreach (Elemento elemento in document.Elementos)
                    source.Document.AdicionarElemento(elemento);

                source.Projects.Salvar(path);

                var target = new EditorContext();
                target.Projects.Abrir(path);

                return target.Document;
            }
            finally
            {
                DeleteIfExists(path);
            }
        }

        private static string CreateTempProjectPath()
        {
            return Path.Combine(Path.GetTempPath(), $"araci-check-{Guid.NewGuid():N}.araci");
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static T FindById<T>(AraciDocument document, Guid id)
            where T : Elemento
        {
            T? elemento = document.Elementos.OfType<T>().FirstOrDefault(e => e.Id == id);

            if (elemento == null)
                throw new InvalidOperationException($"Elemento {typeof(T).Name} '{id}' nao encontrado.");

            return elemento;
        }

        private static void AssertCablePersisted(Cabo expected, Cabo actual, string name)
        {
            AssertEqual(expected.Nome, actual.Nome, $"{name}.Nome");
            AssertEqual(expected.OrigemId, actual.OrigemId, $"{name}.OrigemId");
            AssertEqual(expected.DestinoId, actual.DestinoId, $"{name}.DestinoId");
            AssertEqual(expected.OrigemTerminalId, actual.OrigemTerminalId, $"{name}.OrigemTerminalId");
            AssertEqual(expected.DestinoTerminalId, actual.DestinoTerminalId, $"{name}.DestinoTerminalId");
            AssertEqual(expected.Comprimento, actual.Comprimento, $"{name}.Comprimento");
            AssertEqual(expected.Vertices.Count, actual.Vertices.Count, $"{name}.Vertices.Count");

            for (int i = 0; i < expected.Vertices.Count; i++)
            {
                AssertEqual(expected.Vertices[i].X, actual.Vertices[i].X, $"{name}.Vertices[{i}].X");
                AssertEqual(expected.Vertices[i].Y, actual.Vertices[i].Y, $"{name}.Vertices[{i}].Y");
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void AssertEqual<T>(T expected, T actual, string name)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new InvalidOperationException($"{name}: esperado '{expected}', obtido '{actual}'.");
        }

        private static void AssertEqual(double expected, double actual, string name)
        {
            if (Math.Abs(expected - actual) > 0.000001)
                throw new InvalidOperationException($"{name}: esperado '{expected}', obtido '{actual}'.");
        }

        private static void AssertContains(string text, string expected, string name)
        {
            if (!text.Contains(expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"{name}: texto nao contem '{expected}'. Texto: {text}");
        }

        private static void AssertButtonsNotFocusable(string relativePath, string name)
        {
            string xaml = File.ReadAllText(FindProjectFile(relativePath));
            IReadOnlyList<string> buttons = ExtractButtonTags(xaml);

            Assert(buttons.Count > 0, $"{name}: nenhum Button encontrado.");

            for (int i = 0; i < buttons.Count; i++)
            {
                string button = buttons[i];

                if (button.Contains("Style=\"{StaticResource RibbonToolButton}\"", StringComparison.OrdinalIgnoreCase))
                    continue;

                AssertContains(button, "Focusable=\"False\"", $"{name}.Button[{i}].Focusable");
                AssertContains(button, "IsTabStop=\"False\"", $"{name}.Button[{i}].IsTabStop");
            }
        }

        private static IReadOnlyList<string> ExtractButtonTags(string xaml)
        {
            var buttons = new List<string>();
            int index = 0;

            while (index < xaml.Length)
            {
                int start = xaml.IndexOf("<Button", index, StringComparison.OrdinalIgnoreCase);

                if (start < 0)
                    break;

                int end = xaml.IndexOf('>', start);

                if (end < 0)
                    break;

                buttons.Add(xaml[start..(end + 1)]);
                index = end + 1;
            }

            return buttons;
        }

        private static string FindProjectFile(string relativePath)
        {
            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            DirectoryInfo? directory = new(Directory.GetCurrentDirectory());

            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, normalized);

                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            throw new FileNotFoundException($"Arquivo de projeto nao encontrado: {relativePath}");
        }

        private static void AssertThrows<TException>(Action action, string name)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }

            throw new InvalidOperationException($"{name}: excecao {typeof(TException).Name} nao foi lancada.");
        }

        private static IReadOnlyList<ProjectTableFilterRule> ObterFiltrosTabelaWindow(Araci.Properties.FiltrosTabelaWindow window)
        {
            MethodInfo? method = typeof(Araci.Properties.FiltrosTabelaWindow).GetMethod(
                "ObterFiltros",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (method == null)
                throw new InvalidOperationException("Metodo ObterFiltros nao encontrado.");

            return (IReadOnlyList<ProjectTableFilterRule>)method.Invoke(window, null)!;
        }

        private static string ObterTextoItem(object item)
        {
            return item.GetType().GetProperty("Texto", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(item)
                ?.ToString() ?? string.Empty;
        }

        private static string ObterCampoIdItem(object item)
        {
            return item.GetType().GetProperty("CampoId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(item)
                ?.ToString() ?? string.Empty;
        }

        private static void RunSta(Action action)
        {
            Exception? exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
                throw exception;
        }

        private static Point RotateAround(Point point, Point center, double angle)
        {
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double x = point.X - center.X;
            double y = point.Y - center.Y;

            return new Point(
                center.X + x * cos - y * sin,
                center.Y + x * sin + y * cos);
        }

        private sealed record SimpleCircuit(
            AraciDocument Document,
            Gerador Generator,
            Carga Load,
            Cabo Cable);

        private sealed record RotatedCircuit(
            EditorContext Context,
            Gerador Generator,
            Carga Load,
            Cabo Cable);

        private sealed record CableVertexEditCircuit(
            EditorContext Context,
            Gerador Generator,
            Carga Load,
            Cabo Cable,
            CaboViewModel CableVm);

        private sealed record BarRotationCircuit(
            EditorContext Context,
            Gerador Generator,
            Barra Bar,
            Carga Load,
            Cabo Incoming,
            Cabo Outgoing);

        private sealed record BarResizeCircuit(
            EditorContext Context,
            AraciDocument Document,
            Barra Bar,
            Barra OtherBar);

        private sealed class FakeAnnotationElement : ElementoAnotativo
        {
            public override Elemento Clonar()
            {
                var clone = new FakeAnnotationElement();
                CopiarBasePara(clone);
                return clone;
            }
        }

        private sealed class FakeRectangularAnnotationElement : ElementoAnotativoRetangular
        {
            public override Elemento Clonar()
            {
                var clone = new FakeRectangularAnnotationElement();
                CopiarBasePara(clone);
                return clone;
            }
        }
    }
}
