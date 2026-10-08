namespace Conde.Torneio;

/// <summary>
/// OS METODOS DE CLASSIFICACAO: como transformar um torneio numa ordem.
///
/// Todos eles resolvem o mesmo problema e nenhum deles e "o certo", porque o
/// problema nao tem resposta certa: com A ganhando de B, B de C e C de A, nao
/// existe ordem que respeite os tres jogos, e qualquer classificacao vai
/// contrariar pelo menos um.
///
/// O que da para fazer e escolher o que minimizar, e cada metodo minimiza outra
/// coisa:
///
/// - COPELAND conta vitorias. E o que todo campeonato usa, e e barato;
/// - SLATER minimiza o numero de jogos contrariados. E o que mais parece "a ordem
///   certa", e e um problema dificil;
/// - a ORDEM POR INSERCAO monta a classificacao um jogador de cada vez, como
///   quem organiza cartas na mao.
///
/// A tabela do medidor diz onde eles concordam e onde nao.
/// </summary>
public static class Metodos
{
    /// <summary>
    /// COPELAND: ordene por numero de vitorias, do maior para o menor.
    ///
    /// E o metodo de todo campeonato de pontos corridos, e ele e barato: uma
    /// contagem e uma ordenacao. O empate e desfeito pelo numero do jogador, que e
    /// uma escolha arbitraria e precisa ser declarada: num torneio ciclico TODO
    /// MUNDO empata, e ai o desempate e a classificacao inteira.
    /// </summary>
    public static int[] Copeland(Torneio torneio) =>
        [.. Enumerable.Range(0, torneio.Jogadores)
            .OrderByDescending(torneio.Vitorias)
            .ThenBy(j => j)];

    /// <summary>
    /// A ORDEM POR INSERCAO: pegue os jogadores um de cada vez e encaixe cada um
    /// onde ele contraria menos jogos.
    ///
    /// E o jeito de quem organiza cartas na mao, e ele produz uma ordem razoavel
    /// rapido. Ele nao tem garantia nenhuma, e a tabela diz quantas vezes ele
    /// acerta a melhor ordem possivel.
    /// </summary>
    public static int[] PorInsercao(Torneio torneio)
    {
        var ordem = new List<int>();

        for (var j = 0; j < torneio.Jogadores; j++)
        {
            var melhor = 0;
            var menos = int.MaxValue;

            for (var onde = 0; onde <= ordem.Count; onde++)
            {
                var contraria = 0;

                for (var i = 0; i < onde; i++)
                    if (torneio.GanhouDe(j, ordem[i])) contraria++;

                for (var i = onde; i < ordem.Count; i++)
                    if (torneio.GanhouDe(ordem[i], j)) contraria++;

                if (contraria >= menos) continue;

                menos = contraria;
                melhor = onde;
            }

            ordem.Insert(melhor, j);
        }

        return [.. ordem];
    }

    /// <summary>
    /// A ORDEM POR CAMINHO, que sai do teorema de Redei: todo torneio tem um
    /// caminho que passa por todo mundo uma vez.
    ///
    /// Ela e construida por insercao tambem, mas com uma regra diferente: cada
    /// jogador novo entra no primeiro lugar em que o caminho continua valendo. O
    /// resultado e uma ordem em que CADA UM ganhou do seguinte, sem nenhuma
    /// excecao.
    ///
    /// Isso e menos do que parece: a ordem respeita n-1 jogos e pode contrariar
    /// todos os outros. E tambem e mais do que parece: ela SEMPRE existe, em todo
    /// torneio, e isso e de 1934.
    /// </summary>
    public static int[] PorCaminho(Torneio torneio)
    {
        var caminho = new List<int>();

        for (var j = 0; j < torneio.Jogadores; j++)
        {
            var onde = caminho.Count;

            for (var i = 0; i < caminho.Count; i++)
            {
                if (!torneio.GanhouDe(j, caminho[i])) continue;

                onde = i;

                break;
            }

            caminho.Insert(onde, j);
        }

        return [.. caminho];
    }

    /// <summary>
    /// Uma ordem e um CAMINHO quando cada um ganhou do seguinte.
    ///
    /// E a conferencia do teorema de Redei, e ela e barata: n-1 olhadas. O teorema
    /// diz que esse caminho sempre existe; esta funcao diz se uma ordem dada e um.
    /// </summary>
    public static bool EhCaminho(Torneio torneio, IReadOnlyList<int> ordem)
    {
        for (var i = 0; i + 1 < ordem.Count; i++)
            if (!torneio.GanhouDe(ordem[i], ordem[i + 1])) return false;

        return true;
    }

    /// <summary>Uma ordem e valida quando ela e uma permutacao de todos os jogadores.</summary>
    public static bool EhOrdemValida(Torneio torneio, IReadOnlyList<int> ordem) =>
        ordem.Count == torneio.Jogadores && ordem.Distinct().Count() == torneio.Jogadores
        && ordem.All(j => j >= 0 && j < torneio.Jogadores);

    public static IEnumerable<(string Nome, Func<Torneio, int[]> Classificar)> Todos()
    {
        yield return ("Copeland", Copeland);
        yield return ("por insercao", PorInsercao);
        yield return ("por caminho", PorCaminho);
    }
}
