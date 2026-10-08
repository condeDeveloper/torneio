using Conde.Torneio;
using Xunit;

namespace Conde.Torneio.Testes;

public class TorneioTestes
{
    [Fact]
    public void OSorteioEhDeterminista()
    {
        var a = new Sorteio(7);
        var b = new Sorteio(7);

        for (var i = 0; i < 1000; i++) Assert.Equal(a.Proximo(), b.Proximo());
    }

    /// <summary>
    /// A fracao precisa cobrir os DOIS LADOS do meio: um deslocamento de 33 em vez
    /// de 32 deixaria o gerador com 31 bits, e "cada jogo decidido por cara ou
    /// coroa" viraria "o de numero menor sempre ganha", com os torneios continuando
    /// plausiveis.
    /// </summary>
    [Fact]
    public void AFracaoCobreOsDoisLadosDoMeio()
    {
        var sorteio = new Sorteio(7);
        var acima = 0;
        var maior = 0.0;

        for (var i = 0; i < 10_000; i++)
        {
            var f = sorteio.Fracao();

            Assert.InRange(f, 0.0, 1.0);
            if (f > 0.5) acima++;
            if (f > maior) maior = f;
        }

        Assert.InRange(acima, 4_700, 5_300);
        Assert.True(maior > 0.999, $"o maior sorteio foi {maior}");
    }

    /// <summary>
    /// TODO torneio gerado aqui esta COMPLETO: todo par jogou exatamente uma vez.
    ///
    /// Sem isso nada neste repositorio vale. Os teoremas de Redei e de Moon sao
    /// sobre torneios completos, e um torneio com um jogo faltando e so um grafo
    /// dirigido qualquer. Um gerador poderia esquecer um par sem que nada parecesse
    /// errado.
    /// </summary>
    [Fact]
    public void TodoTorneioGeradoEstaCompleto()
    {
        foreach (var (nome, torneio) in Torneios.Conhecidos())
            Assert.True(torneio.EstaCompleto(), nome);

        for (var n = 2; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
                Assert.True(torneio.EstaCompleto(), $"{n} jogadores");
    }

    [Fact]
    public void GanharInverteOJogoAnterior()
    {
        var torneio = new Torneio(3);

        torneio.Ganhou(0, 1);

        Assert.True(torneio.GanhouDe(0, 1));
        Assert.False(torneio.GanhouDe(1, 0));

        torneio.Ganhou(1, 0);

        Assert.False(torneio.GanhouDe(0, 1));
        Assert.True(torneio.GanhouDe(1, 0));
    }

    [Fact]
    public void NinguemJogaContraSiMesmo() =>
        Assert.Throws<ArgumentException>(() => new Torneio(3).Ganhou(1, 1));

    /// <summary>
    /// TODOS os torneios de n jogadores sao dois elevado ao numero de pares, e eles
    /// sao todos DIFERENTES.
    ///
    /// Esse numero e a base do repositorio inteiro: e ele que permite conferir os
    /// teoremas em todos e nao numa amostra. Se o gerador repetisse um torneio, a
    /// conferencia estaria cobrindo menos do que diz.
    /// </summary>
    [Fact]
    public void TodosOsTorneiosSaoTodosEDiferentes()
    {
        for (var n = 2; n <= 5; n++)
        {
            var pares = n * (n - 1) / 2;
            var todos = Torneios.Todos(n).ToList();
            var assinaturas = new HashSet<string>();

            foreach (var torneio in todos)
            {
                var s = "";

                for (var a = 0; a < n; a++)
                    for (var b = a + 1; b < n; b++) s += torneio.GanhouDe(a, b) ? '1' : '0';

                assinaturas.Add(s);
            }

            Assert.Equal(1 << pares, todos.Count);
            Assert.Equal(todos.Count, assinaturas.Count);
        }
    }

    /// <summary>
    /// O torneio TRANSITIVO tem ordem perfeita: zero jogos contrariados pela ordem
    /// natural, e um vencedor que ganha de todos.
    /// </summary>
    [Fact]
    public void OTransitivoNaoContrariaNada()
    {
        for (var n = 2; n <= 8; n++)
        {
            var torneio = Torneios.Transitivo(n);

            Assert.Equal(0, torneio.Inversoes([.. Enumerable.Range(0, n)]));
            Assert.Equal(0, ForcaBruta.Condorcet(torneio));
            Assert.Equal(n - 1, torneio.Vitorias(0));
            Assert.Equal(0, torneio.Vitorias(n - 1));
        }
    }

    /// <summary>
    /// E o CICLICO IMPAR da o mesmo numero de vitorias a todo mundo, e nao tem
    /// vencedor nenhum.
    ///
    /// E o caso em que o metodo de Copeland empata TODOS, e a classificacao que ele
    /// produz e inteiramente o desempate.
    /// </summary>
    [Fact]
    public void OCiclicoImparEmpataTodoMundo()
    {
        foreach (var n in new[] { 3, 5, 7, 9 })
        {
            var torneio = Torneios.Ciclico(n);

            Assert.Single(Enumerable.Range(0, n).Select(torneio.Vitorias).Distinct());
            Assert.Equal((n - 1) / 2, torneio.Vitorias(0));
            Assert.Null(ForcaBruta.Condorcet(torneio));
        }
    }

    [Fact]
    public void AsInversoesContamOsJogosContrariados()
    {
        // 0 ganha de 1, 1 ganha de 2, 2 ganha de 0
        var torneio = Torneios.Ciclico(3);

        Assert.Equal(1, torneio.Inversoes([0, 1, 2]));
        Assert.Equal(1, torneio.Inversoes([1, 2, 0]));
        Assert.Equal(2, torneio.Inversoes([2, 1, 0]));
    }

    [Fact]
    public void UmaOrdemQueNaoCobreOTorneioEhRecusada() =>
        Assert.Throws<ArgumentException>(() => Torneios.Ciclico(3).Inversoes([0, 1]));
}
