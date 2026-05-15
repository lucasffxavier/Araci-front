// =============================================================
// FASE 6 (PREPARAÇÃO) — CONTRATO DE FEATURE MODULE
// =============================================================
// Cada funcionalidade do editor implementa IFeatureModule.
// O bootstrap registra as features no contexto.
// O core NUNCA conhece as features diretamente.
//
// Fluxo:
//   1. Feature implementa IFeatureModule
//   2. Bootstrap chama feature.Registrar(context)
//   3. Feature se conecta a eventos/commands/tools
//   4. Feature se desconecta em Desregistrar()
// =============================================================

namespace Araci.Core.Features
{
    public interface IFeatureModule
    {
        // Nome descritivo (para logs e diagnóstico)
        string Nome { get; }

        // Conecta a feature ao contexto
        void Registrar(
            Araci.Services.EditorContext context);

        // Remove assinaturas e libera recursos
        void Desregistrar();
    }
}