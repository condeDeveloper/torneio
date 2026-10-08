using Conde.Torneio;
using Xunit;

namespace Conde.Torneio.Testes;

/// <summary>
/// Os testes das TABELAS que o medidor imprime.
///
/// Eles existem porque numero em README que ninguem confere envelhece errado em
/// silencio: alguem troca uma constante, a tabela muda e o texto continua dizendo
/// a frase antiga. Aqui o teste afirma sobre o mesmo objeto que vira linha
/// impressa.
/// </summary>
public class MedidasTestes
{
    /// <summary>
    /// A linha principal: os tres teoremas valem em TODOS os torneios de cada
    /// tamanho, sem uma excecao.
    /// </summary>
    [Fact]
    public void OsTresTeoremasValemEmTodaLinha()
    {
        var linhas = Medidas.EmTodosOsTorneios();

        Assert.Equal(5, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(0, l.SemCaminho));
        Assert.All(linhas, l => Assert.Equal(0, l.SemRei));
        Assert.All(linhas, l => Assert.Equal(0, l.OMaiorNaoEhRei));

        // e a contagem de torneios e dois elevado ao numero de pares
        foreach (var l in linhas)
            Assert.Equal(1L << (l.Jogadores * (l.Jogadores - 1) / 2), l.Torneios);

        var seis = linhas.Single(l => l.Jogadores == 6);

        Assert.Equal(32_768, seis.Torneios);
        Assert.Equal(6_144, seis.ComCondorcet);
    }

    /// <summary>
    /// A frase do texto: a frequencia do vencedor de Condorcet CAI pela metade a
    /// cada jogador a mais.
    /// </summary>
    [Fact]
    public void AFrequenciaDoVencedorCaiPelaMetadeACadaJogador()
    {
        var linhas = Medidas.EmTodosOsTorneios();

        for (var i = 1; i < linhas.Count; i++)
            Assert.InRange(linhas[i].PorCentoComCondorcet / linhas[i - 1].PorCentoComCondorcet, 0.55, 0.80);

        Assert.Equal(100.0, linhas.Single(l => l.Jogadores == 2).PorCentoComCondorcet, 9);
        Assert.Equal(75.0, linhas.Single(l => l.Jogadores == 3).PorCentoComCondorcet, 9);
        Assert.Equal(18.75, linhas.Single(l => l.Jogadores == 6).PorCentoComCondorcet, 9);
    }

    /// <summary>
    /// E a medida em torneios SORTEADOS bate com a conta, ate duas casas.
    /// </summary>
    [Fact]
    public void AMedidaSorteadaBateComAConta()
    {
        var linhas = Medidas.Condorcet(20_000);

        Assert.Equal(8, linhas.Count);

        foreach (var l in linhas.Where(l => l.Previsto > 1))
            Assert.InRange(l.PorCento - l.Previsto, -1.0, 1.0);

        var tres = linhas.Single(l => l.Jogadores == 3);

        Assert.Equal(75.0, tres.Previsto, 9);
        Assert.InRange(tres.PorCento, 74.0, 76.0);

        // e com vinte jogadores ela praticamente some
        Assert.True(linhas.Single(l => l.Jogadores == 20).PorCento < 0.1);
    }

    /// <summary>
    /// A frase do texto: Copeland acerta 5.152 dos 32.768 e a insercao acerta
    /// 22.046.
    /// </summary>
    [Fact]
    public void CopelandAcertaMuitoMenosQueAInsercao()
    {
        var linhas = Medidas.QuemAcerta();

        Assert.Equal(4, linhas.Count);

        var seis = linhas.Single(l => l.Jogadores == 6);

        Assert.Equal(32_768, seis.Torneios);
        Assert.Equal(5_152, seis.CopelandAcertou);
        Assert.Equal(22_046, seis.InsercaoAcertou);
        Assert.Equal(12_226, seis.CaminhoAcertou);
        Assert.Equal(3.0, seis.PiorRazaoCopeland, 9);

        // e a vantagem da insercao sobre Copeland cresce com o tamanho
        for (var i = 1; i < linhas.Count; i++)
        {
            var antes = (double)linhas[i - 1].CopelandAcertou / linhas[i - 1].InsercaoAcertou;
            var agora = (double)linhas[i].CopelandAcertou / linhas[i].InsercaoAcertou;

            Assert.True(agora <= antes, $"{linhas[i].Jogadores} jogadores");
        }
    }

    /// <summary>
    /// A frase do texto: nos torneios conhecidos, nenhum metodo fica abaixo da
    /// melhor ordem, e Copeland fica acima em varios.
    /// </summary>
    [Fact]
    public void NosTorneiosConhecidosCopelandFicaAcimaEmVarios()
    {
        var linhas = Medidas.ContraAMelhorOrdem();

        Assert.Equal(12, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.Copeland >= l.Melhor, l.Torneio));
        Assert.All(linhas, l => Assert.True(l.PorInsercao >= l.Melhor, l.Torneio));
        Assert.All(linhas, l => Assert.True(l.PorCaminho >= l.Melhor, l.Torneio));

        Assert.True(linhas.Count(l => l.Copeland > l.Melhor) >= 4);

        // no transitivo todos acertam e so uma ordem e otima
        var transitivo = linhas.Single(l => l.Torneio == "transitivo de 6");

        Assert.Equal(0, transitivo.Melhor);
        Assert.Equal(0, transitivo.Copeland);
        Assert.Equal(1, transitivo.QuantasEmpatam);

        // e no ciclico de 7 todas as sete rotacoes empatam
        var ciclico = linhas.Single(l => l.Torneio == "ciclico de 7");

        Assert.Equal(7, ciclico.QuantasEmpatam);
    }

    /// <summary>
    /// A frase do texto: a forca bruta olha n fatorial ordens e os metodos fazem
    /// uma passada pelos jogos.
    /// </summary>
    [Fact]
    public void AForcaBrutaOlhaFatorialEOsMetodosFazemUmaPassada()
    {
        var linhas = Medidas.Custos();

        Assert.Equal(7, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(ForcaBruta.Fatorial(l.Jogadores), l.Ordens));
        Assert.All(linhas, l => Assert.Equal(l.Jogadores * (l.Jogadores - 1) / 2, l.PassosDaInsercao));

        var nove = linhas.Single(l => l.Jogadores == 9);

        Assert.Equal(362_880, nove.Ordens);
        Assert.Equal(36, nove.PassosDaInsercao);
    }
}
