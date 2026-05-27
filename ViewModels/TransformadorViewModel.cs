using System.Collections;
using System.Windows;
using Araci.Core.Rendering;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services;

namespace Araci.ViewModels
{
    public class TransformadorViewModel : ElementoViewModel
    {
        private readonly TerminalLayoutService _terminalLayout;

        public TransformadorViewModel(
            Transformador modelo,
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs,
            TerminalLayoutService terminalLayout)
            : base(
                modelo,
                new EquipamentoNode(
                    modelo,
                    ElementGeometryDefaults.TransformadorLargura,
                    ElementGeometryDefaults.TransformadorAltura),
                types,
                names,
                typePropertiesDialogs)
        {
            _terminalLayout = terminalLayout;
            SelecionarPrimeiroTipoDisponivel();
            AtualizarTerminais();
        }

        public Transformador Transformador => (Transformador)Modelo;

        public override IEnumerable TiposDisponiveis => Types.TiposTransformadores;

        public string Nome
        {
            get => Transformador.Nome;
            set
            {
                if (Transformador.Nome == value)
                    return;

                RenomearModelo(value);
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string Barra
        {
            get => Transformador.Barra;
            set
            {
                if (Transformador.Barra == value)
                    return;

                Transformador.Barra = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public string TensaoLinha
        {
            get => Transformador.TensaoLinha;
            set
            {
                if (Transformador.TensaoLinha == value)
                    return;

                Transformador.TensaoLinha = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public int Alimentador
        {
            get => Transformador.Alimentador;
            set
            {
                if (Transformador.Alimentador == value)
                    return;

                Transformador.Alimentador = value;
                OnPropertyChanged();
                NotificarParametros();
            }
        }

        public int Fases
        {
            get => Transformador.Obter<int>(TipoTransformador.PARAM_FASES);
            set => DefinirParametro(TipoTransformador.PARAM_FASES, value <= 0 ? 1 : value);
        }

        public int Enrolamentos
        {
            get => Transformador.Obter<int>(TipoTransformador.PARAM_ENROLAMENTOS);
            set => DefinirParametro(TipoTransformador.PARAM_ENROLAMENTOS, value <= 0 ? 2 : value);
        }

        public double TensaoPrimarioKV
        {
            get => Transformador.Obter<double>(TipoTransformador.PARAM_TENSAO_PRIMARIO_KV);
            set => DefinirParametro(TipoTransformador.PARAM_TENSAO_PRIMARIO_KV, value);
        }

        public double TensaoSecundarioKV
        {
            get => Transformador.Obter<double>(TipoTransformador.PARAM_TENSAO_SECUNDARIO_KV);
            set => DefinirParametro(TipoTransformador.PARAM_TENSAO_SECUNDARIO_KV, value);
        }

        public double PotenciaKVA
        {
            get => Transformador.Obter<double>(TipoTransformador.PARAM_POTENCIA_KVA);
            set => DefinirParametro(TipoTransformador.PARAM_POTENCIA_KVA, value);
        }

        public double PotenciaMVA
        {
            get => Transformador.Obter<double>(TipoTransformador.PARAM_POTENCIA_MVA);
            set => DefinirParametro(TipoTransformador.PARAM_POTENCIA_MVA, value);
        }

        public double RPercentual
        {
            get => Transformador.Obter<double>(TipoTransformador.PARAM_R_PERCENTUAL);
            set => DefinirParametro(TipoTransformador.PARAM_R_PERCENTUAL, value);
        }

        public double XPercentual
        {
            get => Transformador.Obter<double>(TipoTransformador.PARAM_X_PERCENTUAL);
            set => DefinirParametro(TipoTransformador.PARAM_X_PERCENTUAL, value);
        }

        public string LigacaoPrimario
        {
            get => Transformador.Obter<string>(TipoTransformador.PARAM_LIGACAO_PRIMARIO);
            set => DefinirParametro(TipoTransformador.PARAM_LIGACAO_PRIMARIO, value);
        }

        public string LigacaoSecundario
        {
            get => Transformador.Obter<string>(TipoTransformador.PARAM_LIGACAO_SECUNDARIO);
            set => DefinirParametro(TipoTransformador.PARAM_LIGACAO_SECUNDARIO, value);
        }

        protected override void AtualizarNode()
        {
            base.AtualizarNode();
            AtualizarTerminais();
        }

        public override void Mover(Vector delta)
        {
            base.Mover(delta);
            AtualizarTerminais();
        }

        private void AtualizarTerminais()
        {
            _terminalLayout.AtualizarTerminais(Transformador);
        }

        private void DefinirParametro<T>(string nome, T valor)
        {
            Transformador.Definir(nome, valor);
            OnPropertyChanged();
            NotificarParametros();
        }
    }
}
