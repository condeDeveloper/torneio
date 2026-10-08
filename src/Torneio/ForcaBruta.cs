namespace Conde.Torneio;

/// <summary>
/// O JUIZ, e ele e a definicao escrita do jeito mais burro possivel.
///
/// A melhor classificacao e a que contraria menos jogos, e saber que ela e a
/// melhor exige olhar todas as outras. Sao n fatorial ordens, e com oito
/// jogadores isso e quarenta mil.
///
/// Custa isso e precisa custar. Achar essa ordem e o problema de Slater, de 1961,
/// e ele e dos dificeis: um juiz rapido para ele seria uma noticia bem maior que
/// este repositorio.
///
/// E ha um segundo juiz aqui que nao fala de ordem nenhuma: o VENCEDOR DE
/// CONDORCET, que e quem ganhou de todo mundo. Quando ele existe, toda ordem boa
/// precisa po-lo em primeiro, e isso e um fato sobre o torneio e nao sobre o
/// metodo.
/// </summary>
public static class ForcaBruta
{
    /// <param name="Inversoes">quantos jogos a melhor ordem contraria</param>
    /// <param name="Ordem">uma ordem que chega nesse numero</param>
    /// <param name="Quantas">quantas ordens empatam no minimo</param>
    public record Resultado(int Inversoes, int[] Ordem, int Quantas);

    /// <summary>
    /// A MELHOR ORDEM, por forca bruta sobre todas as permutacoes.
    ///
    /// Devolver tambem QUANTAS ordens empatam no minimo nao e enfeite: quando esse
    /// numero passa de um, o torneio nao tem uma classificacao, tem varias
    /// igualmente defensaveis, e isso e informacao que um "a resposta e esta"
    /// esconderia.
    /// </summary>
    public static Resultado Melhor(Torneio torneio)
    {
        if (torneio.Jogadores > 9) throw new ArgumentException("forca bruta so ate 9 jogadores", nameof(torneio));
        if (torneio.Jogadores == 0) return new Resultado(0, [], 1);

        var melhor = int.MaxValue;
        var melhorOrdem = Array.Empty<int>();
        var quantas = 0;

        foreach (var ordem in Permutacoes(torneio.Jogadores))
        {
            var inversoes = torneio.Inversoes(ordem);

            if (inversoes > melhor) continue;

            if (inversoes < melhor)
            {
                melhor = inversoes;
                melhorOrdem = [.. ordem];
                quantas = 0;
            }

            quantas++;
        }

        return new Resultado(melhor, melhorOrdem, quantas);
    }

    /// <summary>
    /// O VENCEDOR DE CONDORCET: quem ganhou de TODO MUNDO.
    ///
    /// Ele pode nao existir, e e justamente disso que o assunto trata. Quando
    /// existe, ele e unico, e nenhuma classificacao defensavel pode po-lo em
    /// segundo.
    /// </summary>
    public static int? Condorcet(Torneio torneio)
    {
        for (var j = 0; j < torneio.Jogadores; j++)
        {
            var ganhouDeTodos = true;

            for (var outro = 0; outro < torneio.Jogadores && ganhouDeTodos; outro++)
                if (outro != j && !torneio.GanhouDe(j, outro)) ganhouDeTodos = false;

            if (ganhouDeTodos) return j;
        }

        return null;
    }

    /// <summary>
    /// Todas as ordens de n jogadores, pelo algoritmo de Heap, SEM RECURSAO.
    ///
    /// Sem recursao porque a profundidade seria o numero de jogadores e porque o
    /// algoritmo de Heap iterativo gera cada permutacao com UMA troca a partir da
    /// anterior, o que e o que torna possivel percorrer centenas de milhares delas.
    /// </summary>
    public static IEnumerable<int[]> Permutacoes(int n)
    {
        var atual = Enumerable.Range(0, n).ToArray();
        var contador = new int[n];

        yield return atual;

        var i = 0;

        while (i < n)
        {
            if (contador[i] >= i) { contador[i] = 0; i++; continue; }

            var j = i % 2 == 0 ? 0 : contador[i];

            (atual[i], atual[j]) = (atual[j], atual[i]);

            yield return atual;

            contador[i]++;
            i = 0;
        }
    }

    public static long Fatorial(int n)
    {
        var f = 1L;

        for (var i = 2; i <= n; i++) f *= i;

        return f;
    }
}
