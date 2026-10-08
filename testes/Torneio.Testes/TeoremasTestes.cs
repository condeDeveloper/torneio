using Conde.Torneio;
using Xunit;

namespace Conde.Torneio.Testes;

/// <summary>
/// Os teoremas, conferidos em TODOS os torneios de cada tamanho.
///
/// Isso e raro e vale dizer por que: o numero de torneios de n jogadores e dois
/// elevado ao numero de pares, e com seis jogadores isso e 32.768. Da para gerar
/// todos. Entao "todo torneio tem um caminho que passa por todo mundo" nao e
/// testado em cem exemplos: e testado em todos os 32.768.
/// </summary>
public class TeoremasTestes
{
    /// <summary>
    /// REDEI, 1934: todo torneio tem um caminho que passa por todo mundo uma vez.
    ///
    /// O metodo por insercao CONSTROI um desses caminhos, e a forca bruta confirma
    /// que ele existe olhando todas as ordens. Os dois precisam concordar: sem o
    /// segundo, "o metodo achou" e "o teorema vale" seriam a mesma afirmacao.
    /// </summary>
    [Fact]
    public void TodoTorneioTemUmCaminhoQuePassaPorTodoMundo()
    {
        for (var n = 2; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
            {
                var caminho = Metodos.PorCaminho(torneio);

                Assert.True(Metodos.EhOrdemValida(torneio, caminho), $"{n} jogadores");
                Assert.True(Metodos.EhCaminho(torneio, caminho), $"{n} jogadores");
            }
    }

    /// <summary>
    /// E a forca bruta concorda: ela acha um caminho em todo torneio de ate seis.
    /// </summary>
    [Fact]
    public void AForcaBrutaTambemAchaOCaminhoEmTodoTorneio()
    {
        for (var n = 2; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
                Assert.NotNull(Teoremas.CaminhoPorForcaBruta(torneio));
    }

    /// <summary>
    /// MOON, 1968: todo torneio tem um REI, que alcanca todo mundo em no maximo
    /// dois jogos.
    ///
    /// E a prova diz ONDE procurar: quem tem mais vitorias e sempre um rei. Se ele
    /// nao alcancasse alguem em dois jogos, esse alguem teria ganhado dele e de
    /// todos os que ele venceu, e teria mais vitorias que ele.
    /// </summary>
    [Fact]
    public void TodoTorneioTemReiEOMaiorEhSempreUm()
    {
        for (var n = 2; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
            {
                Assert.NotEmpty(Teoremas.Reis(torneio));
                Assert.True(Teoremas.EhRei(torneio, Teoremas.ODeMaisVitorias(torneio)), $"{n} jogadores");
            }
    }

    /// <summary>
    /// E o rei NAO precisa ser unico nem ser quem tem mais vitorias: num ciclo de
    /// tres, os TRES sao reis.
    ///
    /// Esse teste existe para o de cima nao passar por acidente. Se o rei fosse
    /// sempre unico e sempre o de mais vitorias, o teorema de Moon seria uma
    /// reformulacao de "existe um primeiro colocado".
    /// </summary>
    [Fact]
    public void ORaiNaoEhUnicoNemEhSempreOMaior()
    {
        foreach (var n in new[] { 3, 5, 7 })
            Assert.Equal(n, Teoremas.Reis(Torneios.Ciclico(n)).Count);

        // e existe torneio em que o rei nao e o de mais vitorias
        var varios = 0;

        foreach (var torneio in Torneios.Todos(5))
        {
            var reis = Teoremas.Reis(torneio);
            var maior = Teoremas.ODeMaisVitorias(torneio);

            if (reis.Count > 1 && reis.Any(r => r != maior)) varios++;
        }

        Assert.True(varios > 0, "se o rei fosse sempre o de mais vitorias, Moon nao diria nada novo");
    }

    /// <summary>
    /// CONDORCET: o vencedor que ganha de todos pode NAO existir, e quando existe
    /// ele e o UNICO rei.
    /// </summary>
    [Fact]
    public void QuandoHaVencedorDeCondorcetEleEhOUnicoRei()
    {
        for (var n = 2; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
            {
                var c = Teoremas.Conferir(torneio);

                Assert.True(c.Moon, $"{n} jogadores");
                Assert.True(c.CondorcetEhOUnicoRei, $"{n} jogadores");
            }
    }

    [Fact]
    public void OVencedorDeCondorcetEhUnicoQuandoExiste()
    {
        for (var n = 2; n <= 6; n++)
            foreach (var torneio in Torneios.Todos(n))
            {
                var vencedor = ForcaBruta.Condorcet(torneio);

                if (vencedor is null) continue;

                // ninguem mais ganha de todos
                for (var j = 0; j < n; j++)
                {
                    if (j == vencedor) continue;

                    Assert.False(Enumerable.Range(0, n).Where(o => o != j).All(o => torneio.GanhouDe(j, o)));
                }
            }
    }

    /// <summary>
    /// O ACHADO: a frequencia do vencedor de Condorcet DESABA com o tamanho, e ela
    /// bate com a conta.
    ///
    /// Sao n candidatos, e cada um precisa ganhar os n-1 jogos dele: n sobre dois
    /// elevado a n menos um. Com tres jogadores sao tres quartos; com vinte, um em
    /// dez mil.
    ///
    /// Esse e o motivo pratico de o problema existir: para quase todo torneio de
    /// tamanho real, nao ha um primeiro colocado obvio, e a classificacao e sempre
    /// uma escolha de metodo.
    /// </summary>
    [Fact]
    public void AFrequenciaDoVencedorBateComAConta()
    {
        for (var n = 2; n <= 6; n++)
        {
            var quantos = 0;
            var total = 0;

            foreach (var torneio in Torneios.Todos(n))
            {
                total++;

                if (ForcaBruta.Condorcet(torneio) is not null) quantos++;
            }

            // a conta exata, e nao uma estimativa: sao n torneios com cada vencedor
            // possivel, vezes os 2^((n-1)(n-2)/2) jeitos de o resto acontecer
            var previsto = n * (1L << ((n - 1) * (n - 2) / 2));

            Assert.Equal(previsto, quantos);
            Assert.Equal(1L << (n * (n - 1) / 2), total);
        }
    }

    [Fact]
    public void AForcaBrutaRecusaTorneioGrandeDemaisEmVezDeDemorarPraSempre()
    {
        Assert.Throws<ArgumentException>(() => ForcaBruta.Melhor(new Torneio(12)));
        Assert.Throws<ArgumentException>(() => Teoremas.CaminhoPorForcaBruta(new Torneio(12)));
    }

    [Fact]
    public void AsPermutacoesSaoTodasEDiferentes()
    {
        for (var n = 1; n <= 7; n++)
        {
            var todas = ForcaBruta.Permutacoes(n).Select(p => string.Join(",", p)).ToList();

            Assert.Equal(ForcaBruta.Fatorial(n), todas.Count);
            Assert.Equal(ForcaBruta.Fatorial(n), todas.Distinct().Count());
        }
    }
}
