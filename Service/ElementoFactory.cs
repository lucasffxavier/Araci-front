using System;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class ElementoFactory
    {
        private readonly TypeLibraryService _types;
        private readonly NameService _names;
        private readonly TypePropertiesDialogService _typePropertiesDialogs;
        private readonly TerminalLayoutService _terminalLayout;

        public ElementoFactory(
            TypeLibraryService types,
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs,
            TerminalLayoutService terminalLayout)
        {
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _typePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
        }

        public ElementoViewModel? CriarViewModel(Elemento modelo)
        {
            return modelo switch
            {
                Cabo cabo => new CaboViewModel(cabo, _types, _names, _typePropertiesDialogs),
                Carga carga => new CargaViewModel(carga, _types, _names, _typePropertiesDialogs, _terminalLayout),
                Gerador gerador => new GeradorViewModel(gerador, _types, _names, _typePropertiesDialogs, _terminalLayout),
                Barra barra => new BarraViewModel(barra, _types, _names, _typePropertiesDialogs, _terminalLayout),
                _ => null
            };
        }

        public Cabo CriarCabo()
        {
            return new Cabo
            {
                Tipo = _types.TipoCaboPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de cabo cadastrado.")
            };
        }

        public CaboViewModel CriarCaboVM()
        {
            return new CaboViewModel(CriarCabo(), _types, _names, _typePropertiesDialogs);
        }

        public Carga CriarCarga()
        {
            var carga = new Carga
            {
                Tipo = _types.TipoCargaPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de carga cadastrado.")
            };

            _terminalLayout.AtualizarTerminais(carga);
            return carga;
        }

        public CargaViewModel CriarCargaVM()
        {
            return new CargaViewModel(CriarCarga(), _types, _names, _typePropertiesDialogs, _terminalLayout);
        }

        public Gerador CriarGerador()
        {
            var gerador = new Gerador
            {
                Tipo = _types.TipoGeradorPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de gerador cadastrado.")
            };

            _terminalLayout.AtualizarTerminais(gerador);
            return gerador;
        }

        public GeradorViewModel CriarGeradorVM()
        {
            return new GeradorViewModel(CriarGerador(), _types, _names, _typePropertiesDialogs, _terminalLayout);
        }

        public Barra CriarBarra()
        {
            var barra = new Barra
            {
                Tipo = _types.TipoBarraPadrao
                    ?? throw new InvalidOperationException("Nenhum tipo de barra cadastrado.")
            };

            _terminalLayout.AtualizarTerminais(barra);
            return barra;
        }

        public BarraViewModel CriarBarraVM()
        {
            return new BarraViewModel(CriarBarra(), _types, _names, _typePropertiesDialogs, _terminalLayout);
        }
    }
}
