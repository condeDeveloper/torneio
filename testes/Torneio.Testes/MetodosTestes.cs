using Conde.Torneio;
using Xunit;

namespace Conde.Torneio.Testes;

public class MetodosTestes
{
    /// <summary>
    /// Todo metodo devolve uma ORDEM VALIDA: uma permutacao de todos os jogadores.
    ///
    /// E a unica coisa que todos eles prometem, e e preciso conferir porque dois
    /// deles montam a ordem por insercao, e uma insercao com indice errado duplica
    /// um jogador e perde outro sem que o resultado deixe de parecer plausivel.
    /// </summary>
    [Fact]
    public void TodoMetodoDevolveUmaOrdemValida()
    {
        for (var n = 2; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
                foreach (var (nome, classificar) in Metodos.Todos())
                    Assert.True(Metodos.EhOrdemValida(torneio, classificar(torneio)), $"{nome}, {n} jogadores");
    }

    /// <summary>
    /// E nenhum deles fica ABAIXO da melhor ordem, que seria a unica maneira de o
    /// juiz estar errado.
    /// </summary>
    [Fact]
    public void NenhumMetodoFicaAbaixoDaMelhorOrdem()
    {
        for (var n = 3; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
            {
                var melhor = ForcaBruta.Melhor(torneio).Inversoes;

                foreach (var (nome, classificar) in Metodos.Todos())
                    Assert.True(torneio.Inversoes(classificar(torneio)) >= melhor, $"{nome}, {n} jogadores");
            }
    }

    /// <summary>
    /// No torneio TRANSITIVO todos acertam: com a ordem perfeita existindo, nao ha
    /// o que escolher.
    ///
    /// E esse e o caso em que a maioria testa, e e por isso que a diferenca entre
    /// os metodos passa despercebida.
    /// </summary>
    [Fact]
    public void NoTransitivoTodosAcertam()
    {
        for (var n = 2; n <= 9; n++)
        {
            var torneio = Torneios.Transitivo(n);

            foreach (var (nome, classificar) in Metodos.Todos())
                Assert.Equal(0, torneio.Inversoes(classificar(torneio)));
        }
    }

    /// <summary>
    /// O ACHADO PRINCIPAL: COPELAND, que e o metodo de todo campeonato de pontos
    /// corridos, e o PIOR dos tres.
    ///
    /// Em todos os 32.768 torneios de seis jogadores, ele acerta a melhor ordem em
    /// 5.152. O metodo por insercao, que e mais simples de explicar, acerta em
    /// 22.046.
    ///
    /// O motivo: contar vitorias joga fora QUEM foi vencido. Dois jogadores com
    /// tres vitorias cada nao sao a mesma coisa se um venceu os tres melhores e o
    /// outro venceu os tres piores, e a contagem nao enxerga isso.
    /// </summary>
    [Fact]
    public void CopelandEhOPiorDosTres()
    {
        var copeland = 0;
        var insercao = 0;
        var caminho = 0;
        var total = 0;

        foreach (var torneio in Torneios.Todos(6))
        {
            total++;

            var melhor = ForcaBruta.Melhor(torneio).Inversoes;

            if (torneio.Inversoes(Metodos.Copeland(torneio)) == melhor) copeland++;
            if (torneio.Inversoes(Metodos.PorInsercao(torneio)) == melhor) insercao++;
            if (torneio.Inversoes(Metodos.PorCaminho(torneio)) == melhor) caminho++;
        }

        Assert.Equal(32_768, total);
        Assert.Equal(5_152, copeland);
        Assert.Equal(22_046, insercao);
        Assert.Equal(12_226, caminho);
    }

    /// <summary>
    /// E ele contraria ate TRES VEZES mais jogos que o necessario.
    ///
    /// Tres vezes nao e pouco quando o numero e pequeno: num torneio em que a
    /// melhor ordem contraria dois jogos, Copeland contraria seis.
    /// </summary>
    [Fact]
    public void CopelandContrariaAteTresVezesMaisQueONecessario()
    {
        var pior = 1.0;

        foreach (var torneio in Torneios.Todos(6))
        {
            var melhor = ForcaBruta.Melhor(torneio).Inversoes;

            if (melhor == 0) continue;

            pior = Math.Max(pior, (double)torneio.Inversoes(Metodos.Copeland(torneio)) / melhor);
        }

        Assert.Equal(3.0, pior, 9);
    }

    /// <summary>
    /// O CAMINHO que o metodo por caminho devolve e um caminho de verdade: cada um
    /// ganhou do seguinte.
    ///
    /// E isso nao o torna bom: respeitar n-1 jogos e pouco quando ha n sobre dois
    /// jogos no total. Ele acerta a melhor ordem em 12.226 dos 32.768, e perde para
    /// a insercao.
    /// </summary>
    [Fact]
    public void OMetodoPorCaminhoDevolveUmCaminhoDeVerdade()
    {
        for (var n = 2; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
                Assert.True(Metodos.EhCaminho(torneio, Metodos.PorCaminho(torneio)), $"{n} jogadores");
    }

    /// <summary>
    /// E a conferencia de caminho RECUSA uma ordem que nao e: ela nao e uma
    /// formalidade.
    /// </summary>
    [Fact]
    public void AConferenciaDeCaminhoRecusaOQueNaoEh()
    {
        var torneio = Torneios.Transitivo(4);

        Assert.True(Metodos.EhCaminho(torneio, [0, 1, 2, 3]));
        Assert.False(Metodos.EhCaminho(torneio, [3, 2, 1, 0]));
        Assert.False(Metodos.EhCaminho(torneio, [0, 2, 1, 3]));
    }

    /// <summary>
    /// A MELHOR ORDEM nem sempre e unica, e o numero de empates e informacao.
    ///
    /// Num ciclo de n jogadores, TODAS as n rotacoes empatam no minimo: o torneio
    /// nao tem uma classificacao, tem n igualmente defensaveis.
    /// </summary>
    [Fact]
    public void NoCiclicoTodasAsRotacoesEmpatamNoMinimo()
    {
        foreach (var n in new[] { 3, 5, 7 })
        {
            var r = ForcaBruta.Melhor(Torneios.Ciclico(n));

            Assert.Equal(n, r.Quantas);
        }
    }

    [Fact]
    public void AOrdemValidaRecusaOQueNaoEhPermutacao()
    {
        var torneio = Torneios.Transitivo(3);

        Assert.True(Metodos.EhOrdemValida(torneio, [2, 0, 1]));
        Assert.False(Metodos.EhOrdemValida(torneio, [0, 1]));
        Assert.False(Metodos.EhOrdemValida(torneio, [0, 1, 1]));
        Assert.False(Metodos.EhOrdemValida(torneio, [0, 1, 5]));
    }
}
