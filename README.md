# torneio

Classificação de torneios do zero em C# e .NET 8: todo mundo joga contra todo
mundo, não há empate, e a pergunta é quem ficou em primeiro.

A resposta é que **não existe resposta**. Com A ganhando de B, B de C e C de A,
nenhuma ordem respeita os três jogos, e qualquer classificação vai contrariar
pelo menos um. Isso é o paradoxo de Condorcet, de 1785, e não é curiosidade: é o
motivo de "quem é o melhor?" não ter resposta óbvia em campeonato nenhum, em
eleição nenhuma e em nenhuma comparação de preferências.

O juiz é a **definição**: a melhor classificação é a que contraria menos jogos, e
saber que ela é a melhor exige olhar todas as n! ordens.

## Os teoremas, conferidos em todos os torneios

```
$ dotnet medidor.dll teoremas

  jogadores   torneios   sem caminho   sem rei   o de mais vitorias nao e rei   com vencedor de Condorcet   por cento
          2          2             0         0                              0                           2       100.0
          3          8             0         0                              0                           6        75.0
          4         64             0         0                              0                          32        50.0
          5      1,024             0         0                              0                         320        31.2
          6     32,768             0         0                              0                       6,144        18.8
```

Isso não é amostra. O número de torneios de n jogadores é dois elevado ao número
de pares, e com seis jogadores isso é **32.768** — dá para gerar todos. Então
"todo torneio tem um caminho que passa por todo mundo" não é testado em cem
exemplos: é testado nos 32.768.

**Rédei, 1934**: todo torneio tem um caminho hamiltoniano, uma ordem em que cada
um ganhou do seguinte. Sempre, em qualquer torneio.

**Moon, 1968**: todo torneio tem um **rei**, alguém que alcança todo mundo em no
máximo dois jogos. E a prova diz onde procurar: quem tem mais vitórias é sempre
um. Se ele não alcançasse alguém em dois jogos, esse alguém teria ganhado dele e
de todos os que ele venceu, e teria mais vitórias que ele.

O rei não precisa ser único. Num ciclo de três, os **três** são reis.

## O primeiro colocado some

```
$ dotnet medidor.dll condorcet

  jogadores   torneios sorteados   com vencedor   por cento   o que a conta preve
          3              20,000         14,993       74.97                 75.00
          4              20,000          9,986       49.93                 50.00
          5              20,000          6,267       31.34                 31.25
          6              20,000          3,716       18.58                 18.75
          8              20,000          1,261        6.30                  6.25
         10              20,000            400        2.00                  1.95
         15              20,000             23        0.12                  0.09
         20              20,000              1        0.01                  0.00
```

A conta prevê n/2^(n−1): são n candidatos, e cada um precisa ganhar os n−1 jogos
dele. A medida bate com ela em duas casas.

Com vinte jogadores, **um torneio em vinte mil** tem alguém que ganhou de todos.
Para qualquer campeonato de tamanho real, não há um primeiro colocado óbvio, e a
classificação é sempre uma escolha de método.

## O método que todo campeonato usa é o pior dos três

```
$ dotnet medidor.dll acerto

  jogadores   torneios   Copeland acertou   por insercao acertou   por caminho acertou   pior razao Copeland   pior razao insercao
          3          8                  7                      8                     8                  2.00                  1.00
          4         64                 38                     56                    50                  3.00                  2.00
          5      1,024                341                    814                   604                  3.00                  3.00
          6     32,768              5,152                 22,046                12,226                  3.00                  4.00
```

**Copeland** ordena por número de vitórias. É o que todo campeonato de pontos
corridos faz, e é barato: uma contagem e uma ordenação. Em todos os 32.768
torneios de seis jogadores, ele acerta a melhor ordem em **5.152**.

O método **por inserção** — pegue os jogadores um de cada vez e encaixe cada um
onde ele contraria menos jogos, como quem organiza cartas na mão — acerta em
**22.046**.

O motivo é que contar vitórias joga fora **quem** foi vencido. Dois jogadores com
três vitórias cada não são a mesma coisa se um venceu os três melhores e o outro
venceu os três piores, e a contagem não enxerga isso.

E Copeland chega a contrariar **três vezes** mais jogos que o necessário. Três
vezes não é pouco quando o número é pequeno: num torneio em que a melhor ordem
contraria dois jogos, ele contraria seis.

## Quando não há uma classificação, há várias

```
$ dotnet medidor.dll metodos

  torneio                       jogadores   a melhor   Copeland   por insercao   por caminho   quantas ordens empatam
  transitivo de 6                       6          0          0              0             0                        1
  ciclico de 3                          3          1          1              1             1                        3
  ciclico de 7                          7          6          6              6             6                        7
  quase transitivo (5 zebras)           7          2          2              2             4                        5
  sorteado de 7 (1)                     7          4          6              5             7                        1
  sorteado de 7 (4)                     7          4          7              5            10                        3
```

A última coluna é a mais honesta da tabela. Quando ela passa de um, o torneio não
tem **uma** classificação: tem várias igualmente defensáveis, e escolher entre
elas é um desempate arbitrário que o método não tem como justificar.

Num ciclo de sete, todas as **sete** rotações empatam no mínimo.

## Como rodar

```
dotnet test testes/Torneio.Testes/Torneio.Testes.csproj
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

Os comandos do medidor são `teoremas`, `metodos`, `acerto`, `condorcet`, `custo`
e `tudo`.

A suíte de testes leva uns setenta segundos de propósito: ela percorre todos os
32.768 torneios de seis jogadores e resolve cada um por força bruta.

## O que as medidas me corrigiram

**Copeland perdeu para o método mais simples que eu escrevi.** Eu pus a ordenação
por inserção no repositório como o "jeito ingênuo", o que serve de chão para a
tabela ter contra o que comparar. Ele acerta quatro vezes mais que o método que
todo campeonato do mundo usa.

Isso não quer dizer que os campeonatos estão errados: Copeland é barato,
explicável e à prova de discussão, e isso vale muito num contexto em que alguém
vai contestar o resultado. Mas "a classificação certa" ele não produz, e a tabela
diz em quantos casos.

**"Acertar" não podia ser "dar a mesma ordem".** A primeira versão do teste
comparava a ordem devolvida com a ordem ótima, elemento por elemento, e falhava
em quase tudo. Com várias ordens empatando no mínimo, exigir a mesma seria exigir
que o método adivinhasse um desempate arbitrário.

Acertar passou a ser chegar no mesmo **número** de jogos contrariados, e a
quantidade de ordens que empatam virou uma coluna da tabela — que é onde ela
precisava estar desde o começo.

**O teorema de Rédei precisava de dois juízes, não de um.** O método por caminho
constrói um caminho hamiltoniano e eu ia conferir que ele é um caminho, e pronto.
Mas isso não testa o teorema: testa o método. Se o método tivesse um erro e
devolvesse um caminho só em alguns torneios, o teste passaria nesses e nada
diria sobre os outros.

A força bruta sobre todas as ordens entrou como terceiro: o teorema diz que a
ordem existe, o método constrói uma, e a força bruta confirma que existe. Sem ela,
"o método achou" e "o teorema vale" seriam a mesma afirmação.

**A previsão de Condorcet é exata, e não uma estimativa.** O teste não compara a
medida com n/2^(n−1) aproximadamente: ele conta exatamente n · 2^((n−1)(n−2)/2)
torneios com vencedor, porque são n escolhas de quem vence e o resto dos jogos
acontece de qualquer jeito. Bate nos 32.768 sem folga nenhuma.

## Licença

MIT.
