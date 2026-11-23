namespace ALotOfDamage.Core
{
    public enum Vantagem { Normal, Preventivo, Emboscada }

    // Regras puras de quem começa com vantagem e de quando o monstro persegue
    public static class RegrasDoEncontro
    {
        public static Vantagem AoAtacar(bool inimigoPerseguindo) =>
            inimigoPerseguindo ? Vantagem.Normal : Vantagem.Preventivo;

        public static Vantagem AoSerAlcancado() => Vantagem.Emboscada;

        public static bool DevePerseguir(bool perseguindo, float distancia, float raioDeVisao, float raioDeDesistencia, bool emProtecao)
        {
            if (perseguindo) return distancia <= raioDeDesistencia;
            return distancia < raioDeVisao && !emProtecao;
        }

        // Monstro derrotado volta ao mapa depois de um tempo, para o bestiário poder ser completado
        public static bool Renasceu(float momentoDaDerrota, float agora, float segundosParaRenascer) =>
            agora - momentoDaDerrota >= segundosParaRenascer;
    }
}
