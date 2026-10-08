namespace Conde.Torneio;

/// <summary>
/// O SORTEIO, escrito aqui em vez de vir da biblioteca.
///
/// A documentacao do sorteio de biblioteca nao promete a mesma sequencia entre
/// versoes nem entre sistemas, e a integracao continua roda em tres. "Existe
/// vencedor de Condorcet em 42% dos torneios de oito" com torneios diferentes em
/// cada sistema nao e uma medida: sao tres medidas que ninguem pode comparar.
/// </summary>
public sealed class Sorteio
{
    private ulong estado;

    public Sorteio(ulong semente) => estado = semente == 0 ? 1 : semente;

    public uint Proximo()
    {
        estado = estado * 6364136223846793005UL + 1442695040888963407UL;
        return (uint)(estado >> 32);
    }

    public int Ate(int limite) => (int)(Proximo() % (uint)limite);

    public double Fracao() => Proximo() / 4294967296.0;
}

/// <summary>
/// Um TORNEIO: todo mundo joga contra todo mundo, e nao ha empate.
///
/// E a estrutura mais simples que existe depois de um grafo, e a mais
/// contraintuitiva de todas: ela parece que deveria produzir uma ordem, e nao
/// produz. Com tres jogadores, A ganha de B, B ganha de C e C ganha de A e o
/// torneio inteiro cabe nisso.
///
/// Esse e o paradoxo de Condorcet, de 1785, e ele nao e uma curiosidade: ele e o
/// motivo de "quem ficou em primeiro?" nao ter resposta obvia em campeonato
/// nenhum, em eleicao nenhuma e em nenhuma comparacao de preferencias.
/// </summary>
public sealed class Torneio
{
    private readonly bool[,] ganhou;

    public Torneio(int jogadores)
    {
        Jogadores = jogadores;
        ganhou = new bool[jogadores, jogadores];
    }

    public int Jogadores { get; }

    /// <summary>Registra que um ganhou do outro. O jogo contrario some.</summary>
    public void Ganhou(int vencedor, int perdedor)
    {
        if (vencedor == perdedor) throw new ArgumentException("ninguem joga contra si mesmo", nameof(vencedor));

        ganhou[vencedor, perdedor] = true;
        ganhou[perdedor, vencedor] = false;
    }

    public bool GanhouDe(int um, int outro) => ganhou[um, outro];

    /// <summary>Quantas vitorias cada jogador teve.</summary>
    public int Vitorias(int jogador)
    {
        var quantas = 0;

        for (var outro = 0; outro < Jogadores; outro++)
            if (outro != jogador && ganhou[jogador, outro]) quantas++;

        return quantas;
    }

    /// <summary>
    /// O torneio esta COMPLETO quando todo par jogou exatamente uma vez.
    ///
    /// Sem isso nada aqui vale: os teoremas de Redei e do rei sao sobre torneios
    /// completos, e um torneio com um jogo faltando e so um grafo dirigido
    /// qualquer. O gerador poderia esquecer um par sem que nada parecesse errado.
    /// </summary>
    public bool EstaCompleto()
    {
        for (var a = 0; a < Jogadores; a++)
            for (var b = a + 1; b < Jogadores; b++)
                if (ganhou[a, b] == ganhou[b, a]) return false;

        return true;
    }

    /// <summary>
    /// Quantas INVERSOES uma ordem tem: quantos jogos ela contraria.
    ///
    /// Se a ordem poe A antes de B e B ganhou de A, isso e uma inversao. A melhor
    /// ordem possivel e a que tem menos, e achar essa ordem e o problema de
    /// Slater, de 1961, que e um dos dificeis.
    /// </summary>
    public int Inversoes(IReadOnlyList<int> ordem)
    {
        if (ordem.Count != Jogadores) throw new ArgumentException("a ordem nao cobre o torneio", nameof(ordem));

        var quantas = 0;

        for (var i = 0; i < ordem.Count; i++)
            for (var j = i + 1; j < ordem.Count; j++)
                if (ganhou[ordem[j], ordem[i]]) quantas++;

        return quantas;
    }

    public override string ToString() => $"torneio de {Jogadores}";
}

/// <summary>
/// Os TORNEIOS que separam os metodos, e por que eles sao assim.
///
/// O que torna este repositorio diferente dos outros e que aqui da para gerar
/// TODOS os torneios de um tamanho: sao dois elevado ao numero de pares, e com
/// seis jogadores isso e 32.768. Os teoremas nao sao conferidos em amostra: sao
/// conferidos em todos.
/// </summary>
public static class Torneios
{
    /// <summary>Um torneio TRANSITIVO: a ordem e perfeita e nao ha surpresa nenhuma.</summary>
    public static Torneio Transitivo(int jogadores)
    {
        var torneio = new Torneio(jogadores);

        for (var a = 0; a < jogadores; a++)
            for (var b = a + 1; b < jogadores; b++) torneio.Ganhou(a, b);

        return torneio;
    }

    /// <summary>
    /// O CICLO: cada um ganha dos proximos, dando a volta.
    ///
    /// Com tres jogadores e pedra, papel e tesoura. Com qualquer numero impar, todo
    /// mundo tem o mesmo numero de vitorias e nenhuma ordem e melhor que outra por
    /// contagem de vitorias: e o caso em que o metodo de Copeland empata tudo.
    /// </summary>
    public static Torneio Ciclico(int jogadores)
    {
        var torneio = new Torneio(jogadores);
        var metade = (jogadores - 1) / 2;

        for (var a = 0; a < jogadores; a++)
            for (var d = 1; d <= metade; d++) torneio.Ganhou(a, (a + d) % jogadores);

        // com numero par sobra o par oposto, que precisa de um desempate qualquer
        if (jogadores % 2 == 0)
            for (var a = 0; a < jogadores / 2; a++) torneio.Ganhou(a, a + jogadores / 2);

        return torneio;
    }

    public static Torneio Sorteado(int jogadores, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var torneio = new Torneio(jogadores);

        for (var a = 0; a < jogadores; a++)
            for (var b = a + 1; b < jogadores; b++)
                if (sorteio.Fracao() < 0.5) torneio.Ganhou(a, b);
                else torneio.Ganhou(b, a);

        return torneio;
    }

    /// <summary>
    /// Um torneio QUASE TRANSITIVO: a ordem certa com algumas zebras.
    ///
    /// E o caso realista, e tambem o caso que separa os metodos: com a ordem quase
    /// perfeita, a diferenca entre contar vitorias e minimizar inversoes aparece em
    /// poucas posicoes, e e justamente ai que ela importa.
    /// </summary>
    public static Torneio QuaseTransitivo(int jogadores, int zebras, ulong semente)
    {
        var torneio = Transitivo(jogadores);
        var sorteio = new Sorteio(semente);

        for (var k = 0; k < zebras; k++)
        {
            var a = sorteio.Ate(jogadores);
            var b = sorteio.Ate(jogadores);

            if (a == b) continue;

            // inverte o jogo: quem perdia passa a ganhar
            if (torneio.GanhouDe(a, b)) torneio.Ganhou(b, a);
            else torneio.Ganhou(a, b);
        }

        return torneio;
    }

    /// <summary>
    /// TODOS os torneios de um tamanho, um por combinacao de resultados.
    ///
    /// Sao dois elevado ao numero de pares: com cinco jogadores sao 1.024, com seis
    /// sao 32.768, com sete sao dois milhoes. E e aqui que os teoremas deste
    /// repositorio sao conferidos, em TODOS e nao numa amostra.
    /// </summary>
    public static IEnumerable<Torneio> Todos(int jogadores)
    {
        var pares = new List<(int, int)>();

        for (var a = 0; a < jogadores; a++)
            for (var b = a + 1; b < jogadores; b++) pares.Add((a, b));

        for (var mascara = 0L; mascara < 1L << pares.Count; mascara++)
        {
            var torneio = new Torneio(jogadores);

            for (var k = 0; k < pares.Count; k++)
            {
                var (a, b) = pares[k];

                if ((mascara & (1L << k)) != 0) torneio.Ganhou(a, b);
                else torneio.Ganhou(b, a);
            }

            yield return torneio;
        }
    }

    public static IEnumerable<(string Nome, Torneio Torneio)> Conhecidos()
    {
        yield return ("transitivo de 6", Transitivo(6));
        yield return ("ciclico de 3", Ciclico(3));
        yield return ("ciclico de 5", Ciclico(5));
        yield return ("ciclico de 7", Ciclico(7));
        yield return ("quase transitivo (2 zebras)", QuaseTransitivo(7, 2, 3));
        yield return ("quase transitivo (5 zebras)", QuaseTransitivo(7, 5, 5));

        for (ulong semente = 1; semente <= 6; semente++)
            yield return ($"sorteado de 7 ({semente})", Sorteado(7, semente));
    }
}
