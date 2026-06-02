using System.Collections;
using System.Windows;
using Araci.Core.Rendering;
using Araci.Core.SceneNodes;
using Araci.Models;
using Araci.Services;
using Araci.Services.Geometry;
using Araci.Services.UI;
using Araci.Services.Catalog;

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
            get => Transformador.Fases;
            set => DefinirParametro(Transformador.PARAM_FASES, value <= 0 ? 3 : value);
        }

        public int Enrolamentos
        {
            get => Transformador.Enrolamentos;
            set => DefinirParametro(Transformador.PARAM_ENROLAMENTOS, value <= 0 ? 2 : value);
        }

        public double TensaoPrimarioKV
        {
            get => Transformador.TensaoPrimarioKV;
            set => DefinirParametro(Transformador.PARAM_TENSAO_PRIMARIO_KV, value > 0 ? value : 13.8);
        }

        public double TensaoSecundarioKV
        {
            get => Transformador.TensaoSecundarioKV;
            set => DefinirParametro(Transformador.PARAM_TENSAO_SECUNDARIO_KV, value > 0 ? value : 0.38);
        }

        public double PotenciaAparente
        {
            get => Transformador.PotenciaAparente;
            set => DefinirParametro(Transformador.PARAM_POTENCIA_APARENTE, value > 0 ? value : 500);
        }

        public double RPercentual
        {
            get => Transformador.RPercentual;
            set => DefinirParametro(Transformador.PARAM_R_PERCENTUAL, value < 0 ? 0 : value);
        }

        public double XPercentual
        {
            get => Transformador.XPercentual;
            set => DefinirParametro(Transformador.PARAM_X_PERCENTUAL, value < 0 ? 0 : value);
        }

        public string LigacaoPrimario
        {
            get => Transformador.LigacaoPrimario;
            set => DefinirParametro(Transformador.PARAM_LIGACAO_PRIMARIO, value);
        }

        public string LigacaoSecundario
        {
            get => Transformador.LigacaoSecundario;
            set => DefinirParametro(Transformador.PARAM_LIGACAO_SECUNDARIO, value);
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