using Conde.Torneio;

var pedido = args.Length > 0 ? args[0] : "tudo";
var tudo = pedido == "tudo";

if (tudo || pedido == "teoremas") Teoremas();
if (tudo || pedido == "metodos") Metodos();
if (tudo || pedido == "acerto") Acertos();
if (tudo || pedido == "condorcet") Condorcets();
if (tudo || pedido == "custo") Custos();

static void Teoremas()
{
    Console.WriteLine();
    Console.WriteLine("  OS TEOREMAS, CONFERIDOS EM TODOS OS TORNEIOS DE CADA TAMANHO");
    Console.WriteLine();
    Console.WriteLine("  jogadores   torneios   sem caminho   sem rei   o de mais vitorias nao e rei   com vencedor de Condorcet   por cento");

    foreach (var l in Medidas.EmTodosOsTorneios())
        Console.WriteLine($"  {l.Jogadores,9} {l.Torneios,10:N0} {l.SemCaminho,13:N0} {l.SemRei,9:N0} {l.OMaiorNaoEhRei,30:N0} {l.ComCondorcet,27:N0} {l.PorCentoComCondorcet,11:F1}");

    Console.WriteLine();
    Console.WriteLine("  as tres colunas do meio sao ZERO em toda linha, e isso nao e amostra: com");
    Console.WriteLine("  seis jogadores sao 32.768 torneios e os teoremas valem nos 32.768");
    Console.WriteLine();
}

static void Metodos()
{
    Console.WriteLine();
    Console.WriteLine("  OS METODOS CONTRA A MELHOR ORDEM (jogos contrariados)");
    Console.WriteLine();
    Console.WriteLine("  torneio                       jogadores   a melhor   Copeland   por insercao   por caminho   quantas ordens empatam");

    foreach (var l in Medidas.ContraAMelhorOrdem())
        Console.WriteLine($"  {l.Torneio,-29} {l.Jogadores,9} {l.Melhor,10} {l.Copeland,10} {l.PorInsercao,14} {l.PorCaminho,13} {l.QuantasEmpatam,24:N0}");

    Console.WriteLine();
    Console.WriteLine("  quando a ultima coluna passa de um, o torneio nao tem UMA classificacao:");
    Console.WriteLine("  tem varias igualmente defensaveis");
    Console.WriteLine();
}

static void Acertos()
{
    Console.WriteLine();
    Console.WriteLine("  QUANTAS VEZES CADA METODO ACERTA A MELHOR ORDEM, EM TODOS OS TORNEIOS");
    Console.WriteLine();
    Console.WriteLine("  jogadores   torneios   Copeland acertou   por insercao acertou   por caminho acertou   pior razao Copeland   pior razao insercao");

    foreach (var l in Medidas.QuemAcerta())
        Console.WriteLine($"  {l.Jogadores,9} {l.Torneios,10:N0} {l.CopelandAcertou,18:N0} {l.InsercaoAcertou,22:N0} {l.CaminhoAcertou,21:N0} {l.PiorRazaoCopeland,21:F2} {l.PiorRazaoInsercao,21:F2}");

    Console.WriteLine();
    Console.WriteLine("  acertar aqui e chegar no mesmo NUMERO de jogos contrariados, e nao na mesma");
    Console.WriteLine("  ordem: exigir a mesma seria exigir que o metodo adivinhasse um desempate");
    Console.WriteLine();
}

static void Condorcets()
{
    Console.WriteLine();
    Console.WriteLine("  COM QUE FREQUENCIA EXISTE UM VENCEDOR QUE GANHA DE TODO MUNDO");
    Console.WriteLine();
    Console.WriteLine("  jogadores   torneios sorteados   com vencedor   por cento   o que a conta preve");

    foreach (var l in Medidas.Condorcet())
        Console.WriteLine($"  {l.Jogadores,9} {l.Torneios,19:N0} {l.ComCondorcet,14:N0} {l.PorCento,11:F2} {l.Previsto,21:F2}");

    Console.WriteLine();
    Console.WriteLine("  a conta preve n sobre dois elevado a n menos um: sao n candidatos, e cada um");
    Console.WriteLine("  precisa ganhar os n-1 jogos dele");
    Console.WriteLine();
}

static void Custos()
{
    Console.WriteLine();
    Console.WriteLine("  O CUSTO DA FORCA BRUTA CONTRA O DOS METODOS");
    Console.WriteLine();
    Console.WriteLine("  jogadores   ordens que existem   ordens que a forca bruta olha   passos de Copeland   passos da insercao");

    foreach (var l in Medidas.Custos())
        Console.WriteLine($"  {l.Jogadores,9} {l.Ordens,19:N0} {l.OrdensDaForcaBruta,30:N0} {l.PassosDeCopeland,20:N0} {l.PassosDaInsercao,20:N0}");

    Console.WriteLine();
    Console.WriteLine("  os metodos fazem uma passada pelos jogos; a forca bruta olha todas as ordens");
    Console.WriteLine();
}
