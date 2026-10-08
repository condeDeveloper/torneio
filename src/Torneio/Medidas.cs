namespace Conde.Torneio;

/// <summary>
/// As TABELAS que o medidor imprime e que os testes conferem.
///
/// Elas ficam aqui, e nao dentro do programa do medidor, por um motivo so: numero
/// em README que ninguem confere envelhece errado em silencio. Alguem troca uma
/// constante, a tabela muda, e o texto continua afirmando a frase antiga. Aqui o
/// teste afirma sobre o MESMO objeto que vira linha impressa.
/// </summary>
public static class Medidas
{
    public record LinhaTodos(
        int Jogadores, long Torneios, int SemCaminho, int SemRei, int OMaiorNaoEhRei,
        int ComCondorcet, double PorCentoComCondorcet);

    /// <summary>
    /// Os teoremas conferidos em TODOS os torneios de cada tamanho.
    ///
    /// Nao e amostra. Com seis jogadores sao 32.768 torneios, e os tres teoremas
    /// precisam valer nos 32.768.
    /// </summary>
    public static List<LinhaTodos> EmTodosOsTorneios(int ate = 6)
    {
        var linhas = new List<LinhaTodos>();

        for (var jogadores = 2; jogadores <= ate; jogadores++)
        {
            long quantos = 0;
            var semCaminho = 0;
            var semRei = 0;
            var oMaiorNaoEhRei = 0;
            var comCondorcet = 0;

            foreach (var torneio in Torneios.Todos(jogadores))
            {
                quantos++;

                if (Metodos.PorCaminho(torneio) is var caminho && !Metodos.EhCaminho(torneio, caminho)) semCaminho++;
                if (Teoremas.Reis(torneio).Count == 0) semRei++;
                if (!Teoremas.EhRei(torneio, Teoremas.ODeMaisVitorias(torneio))) oMaiorNaoEhRei++;
                if (ForcaBruta.Condorcet(torneio) is not null) comCondorcet++;
            }

            linhas.Add(new LinhaTodos(
                jogadores, quantos, semCaminho, semRei, oMaiorNaoEhRei,
                comCondorcet, 100.0 * comCondorcet / quantos));
        }

        return linhas;
    }

    public record LinhaMetodo(
        string Torneio, int Jogadores, int Melhor, int Copeland, int PorInsercao, int PorCaminho, int QuantasEmpatam);

    /// <summary>
    /// Os metodos contra a MELHOR ORDEM, nos torneios em que a forca bruta cabe.
    ///
    /// A coluna de quantas ordens empatam no minimo e a mais honesta da tabela:
    /// quando ela passa de um, o torneio nao tem uma classificacao, tem varias
    /// igualmente defensaveis.
    /// </summary>
    public static List<LinhaMetodo> ContraAMelhorOrdem()
    {
        var linhas = new List<LinhaMetodo>();

        foreach (var (nome, torneio) in Torneios.Conhecidos())
        {
            var melhor = ForcaBruta.Melhor(torneio);

            linhas.Add(new LinhaMetodo(
                nome, torneio.Jogadores, melhor.Inversoes,
                torneio.Inversoes(Metodos.Copeland(torneio)),
                torneio.Inversoes(Metodos.PorInsercao(torneio)),
                torneio.Inversoes(Metodos.PorCaminho(torneio)),
                melhor.Quantas));
        }

        return linhas;
    }

    public record LinhaAcerto(
        int Jogadores, int Torneios, int CopelandAcertou, int InsercaoAcertou, int CaminhoAcertou,
        double PiorRazaoCopeland, double PiorRazaoInsercao);

    /// <summary>
    /// Quantas vezes cada metodo acerta a melhor ordem, em TODOS os torneios de
    /// cada tamanho.
    ///
    /// "Acertar" aqui e chegar no mesmo NUMERO de inversoes, e nao na mesma ordem:
    /// com varias ordens empatando no minimo, exigir a mesma seria exigir que o
    /// metodo adivinhasse um desempate arbitrario.
    /// </summary>
    public static List<LinhaAcerto> QuemAcerta(int ate = 6)
    {
        var linhas = new List<LinhaAcerto>();

        for (var jogadores = 3; jogadores <= ate; jogadores++)
        {
            var quantos = 0;
            var copeland = 0;
            var insercao = 0;
            var caminho = 0;
            var piorCopeland = 1.0;
            var piorInsercao = 1.0;

            foreach (var torneio in Torneios.Todos(jogadores))
            {
                quantos++;

                var melhor = ForcaBruta.Melhor(torneio).Inversoes;
                var deCopeland = torneio.Inversoes(Metodos.Copeland(torneio));
                var deInsercao = torneio.Inversoes(Metodos.PorInsercao(torneio));

                if (deCopeland == melhor) copeland++;
                if (deInsercao == melhor) insercao++;
                if (torneio.Inversoes(Metodos.PorCaminho(torneio)) == melhor) caminho++;

                if (melhor > 0)
                {
                    piorCopeland = Math.Max(piorCopeland, (double)deCopeland / melhor);
                    piorInsercao = Math.Max(piorInsercao, (double)deInsercao / melhor);
                }
            }

            linhas.Add(new LinhaAcerto(jogadores, quantos, copeland, insercao, caminho, piorCopeland, piorInsercao));
        }

        return linhas;
    }

    public record LinhaCondorcet(int Jogadores, int Torneios, int ComCondorcet, double PorCento, double Previsto);

    /// <summary>
    /// Com que frequencia existe um vencedor que ganha de TODO MUNDO, em torneios
    /// sorteados.
    ///
    /// A conta preve n sobre dois elevado a n menos um: sao n candidatos, e cada um
    /// precisa ganhar os n-1 jogos dele. A medida precisa bater com ela, e quando
    /// bate os dois lados ficam mais confiaveis.
    /// </summary>
    public static List<LinhaCondorcet> Condorcet(int torneios = 20_000)
    {
        var linhas = new List<LinhaCondorcet>();

        foreach (var jogadores in new[] { 3, 4, 5, 6, 8, 10, 15, 20 })
        {
            var quantos = 0;

            for (var k = 1UL; k <= (ulong)torneios; k++)
                if (ForcaBruta.Condorcet(Torneios.Sorteado(jogadores, k)) is not null) quantos++;

            var previsto = 100.0 * jogadores / Math.Pow(2, jogadores - 1);

            linhas.Add(new LinhaCondorcet(jogadores, torneios, quantos, 100.0 * quantos / torneios, previsto));
        }

        return linhas;
    }

    public record LinhaCusto(int Jogadores, long Ordens, long OrdensDaForcaBruta, int PassosDeCopeland, int PassosDaInsercao);

    /// <summary>
    /// O CUSTO da forca bruta contra o dos metodos, em ordens e em passos.
    ///
    /// Nenhum segundo em lugar nenhum: segundo depende de maquina, de compilador e
    /// de quem mais esta rodando. Ordem considerada e o mesmo numero nos tres
    /// sistemas da integracao continua.
    /// </summary>
    public static List<LinhaCusto> Custos()
    {
        var linhas = new List<LinhaCusto>();

        foreach (var jogadores in new[] { 3, 4, 5, 6, 7, 8, 9 })
        {
            var pares = jogadores * (jogadores - 1) / 2;

            linhas.Add(new LinhaCusto(
                jogadores, ForcaBruta.Fatorial(jogadores), ForcaBruta.Fatorial(jogadores),
                pares + jogadores, pares));
        }

        return linhas;
    }
}
