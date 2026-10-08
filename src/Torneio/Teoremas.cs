namespace Conde.Torneio;

/// <summary>
/// OS TEOREMAS, e aqui eles sao conferidos em TODOS os torneios de um tamanho, e
/// nao numa amostra.
///
/// Isso e raro e vale dizer por que: o numero de torneios de n jogadores e dois
/// elevado ao numero de pares, que com seis jogadores da 32.768. Da para gerar
/// todos. Entao "todo torneio tem um caminho que passa por todo mundo" nao e
/// testado em cem exemplos: e testado em todos os 32.768.
///
/// Os tres teoremas daqui sao:
///
/// - REDEI, 1934: todo torneio tem um caminho hamiltoniano;
/// - MOON, 1968: todo torneio tem um REI, que alcanca todo mundo em no maximo
///   dois jogos;
/// - CONDORCET, 1785: o vencedor que ganha de todos pode NAO existir, e a
///   frequencia com que ele existe desaba com o tamanho.
/// </summary>
public static class Teoremas
{
    /// <summary>
    /// Um REI e um jogador que alcanca todo mundo em no maximo DOIS jogos: ou ele
    /// ganhou, ou ele ganhou de alguem que ganhou.
    ///
    /// Moon provou em 1968 que todo torneio tem pelo menos um, e o detalhe bonito e
    /// que o rei NAO precisa ser quem tem mais vitorias, e tambem nao precisa ser
    /// unico. Num ciclo de tres, os tres sao reis.
    /// </summary>
    public static bool EhRei(Torneio torneio, int jogador)
    {
        for (var outro = 0; outro < torneio.Jogadores; outro++)
        {
            if (outro == jogador || torneio.GanhouDe(jogador, outro)) continue;

            var alcanca = false;

            for (var meio = 0; meio < torneio.Jogadores && !alcanca; meio++)
                if (torneio.GanhouDe(jogador, meio) && torneio.GanhouDe(meio, outro)) alcanca = true;

            if (!alcanca) return false;
        }

        return true;
    }

    public static List<int> Reis(Torneio torneio) =>
        [.. Enumerable.Range(0, torneio.Jogadores).Where(j => EhRei(torneio, j))];

    /// <summary>
    /// QUEM TEM MAIS VITORIAS e sempre um rei, e isso e a prova do teorema de Moon
    /// em uma frase.
    ///
    /// Se o de mais vitorias nao alcancasse alguem em dois jogos, esse alguem teria
    /// ganhado dele e de todos os que ele venceu, e teria mais vitorias que ele.
    ///
    /// Esta funcao existe para a conferencia poder ser feita dos dois lados: o
    /// teorema diz que EXISTE rei, e a prova diz ONDE procurar um.
    /// </summary>
    public static int ODeMaisVitorias(Torneio torneio)
    {
        var melhor = 0;

        for (var j = 1; j < torneio.Jogadores; j++)
            if (torneio.Vitorias(j) > torneio.Vitorias(melhor)) melhor = j;

        return melhor;
    }

    /// <summary>
    /// Um CAMINHO HAMILTONIANO por forca bruta: uma ordem em que cada um ganhou do
    /// seguinte.
    ///
    /// Ele existe aqui como juiz do metodo por insercao: o teorema de Redei diz que
    /// a ordem existe, o metodo constroi uma, e esta funcao confirma que ela existe
    /// mesmo olhando todas as ordens. Sem esse terceiro, "o metodo achou" e "o
    /// teorema vale" seriam a mesma afirmacao.
    /// </summary>
    public static int[]? CaminhoPorForcaBruta(Torneio torneio)
    {
        if (torneio.Jogadores > 9) throw new ArgumentException("forca bruta so ate 9 jogadores", nameof(torneio));

        foreach (var ordem in ForcaBruta.Permutacoes(torneio.Jogadores))
            if (Metodos.EhCaminho(torneio, ordem)) return [.. ordem];

        return null;
    }

    /// <param name="TemCaminho">o teorema de Redei vale aqui</param>
    /// <param name="Reis">quantos jogadores sao reis</param>
    /// <param name="OMaiorEhRei">quem tem mais vitorias e rei</param>
    /// <param name="Condorcet">o vencedor que ganha de todos, se existir</param>
    public record Conferencia(bool TemCaminho, int Reis, bool OMaiorEhRei, int? Condorcet)
    {
        /// <summary>Moon: existe pelo menos um rei, sempre.</summary>
        public bool Moon => Reis >= 1;

        /// <summary>E quando ha vencedor de Condorcet, ele e o UNICO rei.</summary>
        public bool CondorcetEhOUnicoRei => Condorcet is null || Reis == 1;
    }

    public static Conferencia Conferir(Torneio torneio)
    {
        var reis = Reis(torneio);

        return new Conferencia(
            CaminhoPorForcaBruta(torneio) is not null,
            reis.Count,
            EhRei(torneio, ODeMaisVitorias(torneio)),
            ForcaBruta.Condorcet(torneio));
    }
}
