using System.Collections.Generic;
using Araci.Services.Settings;
using Araci.ViewModels;

namespace Araci.Applications.Factories
{
    public class ElementInstancePropertyProvider
    {
        public IReadOnlyList<InstancePropertyDescriptor> Cabo()
        {
            return new[]
            {
                Prop<CaboViewModel>("Nome", "Nome", 10, false),
                Prop<CaboViewModel>("BarraOrigem", "Barra origem", 20, false),
                Prop<CaboViewModel>("BarraDestino", "Barra destino", 30, false),
                Prop<CaboViewModel>("Comprimento", "Comprimento", 40, unit: UnitKind.LengthMeter),
                Prop<CaboViewModel>("Ampacidade", "Ampacidade", 50, unit: UnitKind.CurrentAmpere),
                Prop<CaboViewModel>("TensaoLinha", "Tensão linha", 60, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<CaboViewModel>("TensaoFaseA", "Tensão fase A", 70, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<CaboViewModel>("TensaoFaseB", "Tensão fase B", 80, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<CaboViewModel>("TensaoFaseC", "Tensão fase C", 90, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<CaboViewModel>("CorrenteLinha", "Corrente linha", 100, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<CaboViewModel>("CorrenteFaseA", "Corrente fase A", 110, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<CaboViewModel>("CorrenteFaseB", "Corrente fase B", 120, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<CaboViewModel>("CorrenteFaseC", "Corrente fase C", 130, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> Carga()
        {
            return new[]
            {
                Prop<CargaViewModel>("Nome", "Nome", 10, false),
                Prop<CargaViewModel>("PotenciaAtiva", "Potência ativa", 20, allowMixedTypeEdit: true, unit: UnitKind.ActivePowerKW),
                Prop<CargaViewModel>("PotenciaReativa", "Potência reativa", 30, allowMixedTypeEdit: true, unit: UnitKind.ReactivePowerKVAr),
                Prop<CargaViewModel>("Alimentador", "Alimentador", 40, allowMixedTypeEdit: true),
                Prop<CargaViewModel>("CorrenteLinha", "Corrente linha", 50, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<CargaViewModel>("CorrenteFaseA", "Corrente fase A", 60, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<CargaViewModel>("CorrenteFaseB", "Corrente fase B", 70, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<CargaViewModel>("CorrenteFaseC", "Corrente fase C", 80, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<CargaViewModel>("TensaoLinha", "Tensão linha", 90, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<CargaViewModel>("TensaoFaseA", "Tensão fase A", 100, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<CargaViewModel>("TensaoFaseB", "Tensão fase B", 110, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<CargaViewModel>("TensaoFaseC", "Tensão fase C", 120, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> Gerador()
        {
            return new[]
            {
                Prop<GeradorViewModel>("Nome", "Nome", 10, false),
                Prop<GeradorViewModel>("PotenciaAparente", "Potência aparente", 20, unit: UnitKind.ApparentPowerKVA),
                Prop<GeradorViewModel>("PotenciaAtiva", "Potência ativa", 30, allowMixedTypeEdit: true, unit: UnitKind.ActivePowerKW),
                Prop<GeradorViewModel>("PotenciaReativa", "Potência reativa", 40, allowMixedTypeEdit: true, unit: UnitKind.ReactivePowerKVAr),
                Prop<GeradorViewModel>("Alimentador", "Alimentador", 45, allowMixedTypeEdit: true),
                Prop<GeradorViewModel>("TensaoLinha", "Tensão linha", 50, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<GeradorViewModel>("TensaoFaseA", "Tensão fase A", 60, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<GeradorViewModel>("TensaoFaseB", "Tensão fase B", 70, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<GeradorViewModel>("TensaoFaseC", "Tensão fase C", 80, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV),
                Prop<GeradorViewModel>("CorrenteLinha", "Corrente linha", 90, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<GeradorViewModel>("CorrenteFaseA", "Corrente fase A", 100, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<GeradorViewModel>("CorrenteFaseB", "Corrente fase B", 110, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere),
                Prop<GeradorViewModel>("CorrenteFaseC", "Corrente fase C", 120, allowMixedTypeEdit: true, unit: UnitKind.CurrentAmpere)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> Sin()
        {
            return new[]
            {
                Prop<SinViewModel>("Nome", "Nome", 10, false),
                Prop<SinViewModel>("TensaoLinha", "Tensão linha", 20, allowMixedTypeEdit: true, unit: UnitKind.VoltageKV)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> Transformador()
        {
            return new[]
            {
                Prop<TransformadorViewModel>("Nome", "Nome", 10, false),
                Prop<TransformadorViewModel>("Alimentador", "Alimentador", 30, allowMixedTypeEdit: true),
                Prop<TransformadorViewModel>("Fases", "Fases", 40),
                Prop<TransformadorViewModel>("Enrolamentos", "Enrolamentos", 50),
                Prop<TransformadorViewModel>("TensaoPrimarioKV", "Tensão primário", 60, unit: UnitKind.VoltageKV),
                Prop<TransformadorViewModel>("TensaoSecundarioKV", "Tensão secundário", 70, unit: UnitKind.VoltageKV),
                Prop<TransformadorViewModel>("PotenciaAparente", "Potência aparente", 80, unit: UnitKind.ApparentPowerKVA),
                Prop<TransformadorViewModel>("RPercentual", "R", 90, unit: UnitKind.Percent),
                Prop<TransformadorViewModel>("XPercentual", "X", 100, unit: UnitKind.Percent),
                Prop<TransformadorViewModel>("LigacaoPrimario", "Ligação primário", 110),
                Prop<TransformadorViewModel>("LigacaoSecundario", "Ligação secundário", 120)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> Barra()
        {
            return new[]
            {
                Prop<BarraViewModel>("Nome", "Nome", 10, false),
                Prop<BarraViewModel>("Tensao", "Tensão", 20, unit: UnitKind.VoltageKV),
                Prop<BarraViewModel>("Altura", "Altura", 30, unit: UnitKind.LengthMeter)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> LinhaAnotativa()
        {
            return new[]
            {
                Prop<LinhaAnotativaViewModel>("Nome", "Nome", 10, false),
                Prop<LinhaAnotativaViewModel>("Comprimento", "Comprimento", 20, false, unit: UnitKind.LengthMeter),
                Prop<LinhaAnotativaViewModel>("CorLinha", "Cor da linha", 30, isColor: true),
                Prop<LinhaAnotativaViewModel>("EspessuraLinha", "Espessura da linha", 40)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> RetanguloAnotativo()
        {
            return new[]
            {
                Prop<RetanguloAnotativoViewModel>("Nome", "Nome", 10, false),
                Prop<RetanguloAnotativoViewModel>("LarguraRetangulo", "Largura", 20, unit: UnitKind.LengthMeter),
                Prop<RetanguloAnotativoViewModel>("AlturaRetangulo", "Altura", 30, unit: UnitKind.LengthMeter),
                Prop<RetanguloAnotativoViewModel>("CorLinha", "Cor da borda", 40, isColor: true),
                Prop<RetanguloAnotativoViewModel>("EspessuraLinha", "Espessura da borda", 50)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> CirculoAnotativo()
        {
            return new[]
            {
                Prop<CirculoAnotativoViewModel>("Nome", "Nome", 10, false),
                Prop<CirculoAnotativoViewModel>("Raio", "Raio", 20, unit: UnitKind.LengthMeter),
                Prop<CirculoAnotativoViewModel>("Diametro", "Diâmetro", 30, false, unit: UnitKind.LengthMeter),
                Prop<CirculoAnotativoViewModel>("CorLinha", "Cor da borda", 40, isColor: true),
                Prop<CirculoAnotativoViewModel>("EspessuraLinha", "Espessura da borda", 50)
            };
        }

        public IReadOnlyList<InstancePropertyDescriptor> TextoAnotativo()
        {
            return new[]
            {
                Prop<TextoAnotativoViewModel>("Nome", "Nome", 10, false),
                Prop<TextoAnotativoViewModel>("Conteudo", "Texto", 20),
                Prop<TextoAnotativoViewModel>("CorTexto", "Cor do texto", 30, isColor: true),
                Prop<TextoAnotativoViewModel>("AlturaTexto", "Altura do texto", 40, unit: UnitKind.LengthMeter),
                Prop<TextoAnotativoViewModel>("Fonte", "Fonte", 50),
                Prop<TextoAnotativoViewModel>("AlinhamentoHorizontal", "Alinhamento", 60)
            };
        }

        private static InstancePropertyDescriptor Prop<T>(
            string propertyName,
            string displayName,
            int order,
            bool isEditable = true,
            bool allowMixedTypeEdit = false,
            UnitKind unit = UnitKind.None,
            bool isColor = false)
            where T : ElementoViewModel
        {
            return new InstancePropertyDescriptor(typeof(T), propertyName, displayName, order, isEditable, allowMixedTypeEdit, unit, isColor);
        }
    }
}