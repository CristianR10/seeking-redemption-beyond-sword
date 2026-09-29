# Sistema de ProgressBar das AreaSoul

## 1. Objetivo

A `ProgressBar` representa a quantidade de `AreaSoul` restantes na fase.

A barra começa em **100%**, independentemente da quantidade de almas.

Conforme o jogador coleta as almas, a porcentagem máxima disponível diminui proporcionalmente à quantidade de almas restantes.

Além disso, a barra possui um **dreno automático por tempo**, fazendo com que seu valor diminua continuamente.

---

## 2. Exemplo inicial

Considere uma fase com **5 AreaSoul**:

```text
Total de almas = 5
Almas restantes = 5
```

A barra começa em:

```text
100%
```

Cada alma representa:

```text
100 / 5 = 20%
```

Portanto:

| Almas restantes |     Cálculo | Valor da barra |
| --------------: | ----------: | -------------: |
|               5 | 5 / 5 × 100 |           100% |
|               4 | 4 / 5 × 100 |            80% |
|               3 | 3 / 5 × 100 |            60% |
|               2 | 2 / 5 × 100 |            40% |
|               1 | 1 / 5 × 100 |            20% |
|               0 | 0 / 5 × 100 |             0% |

---

# 3. Cálculo da porcentagem

A fórmula utilizada é:

```text
Porcentagem = (Almas restantes / Total de almas) × 100
```

Por exemplo, inicialmente temos:

```text
Almas restantes = 5
Total de almas = 5
```

Então:

```text
(5 / 5) × 100 = 100%
```

Depois que o jogador coleta uma alma:

```text
Almas restantes = 4
Total de almas = 5
```

O cálculo passa a ser:

```text
(4 / 5) × 100 = 80%
```

Depois de coletar outra:

```text
(3 / 5) × 100 = 60%
```

E assim por diante.

---

# 4. Por que guardar `_totalSouls`?

O `_totalSouls` representa a quantidade original de almas existentes na fase.

Exemplo:

```csharp
_totalSouls = 5;
```

Esse valor funciona como a referência para todos os cálculos posteriores.

Ele **não deve diminuir** quando o jogador coleta uma alma.

Já `_remainingSouls` representa a quantidade atual:

```text
_totalSouls     = 5
_remainingSouls = 4
```

Isso permite calcular:

```text
4 / 5 × 100 = 80%
```

Se alterássemos `_totalSouls` para 4, perderíamos a referência original da fase.

---

# 5. Como as AreaSoul são encontradas

Cada `AreaSoul` é adicionada ao grupo:

```csharp
AddToGroup("area_soul");
```

Isso permite que o `GlobalBars` encontre todas as almas através de:

```csharp
GetTree().GetNodesInGroup("area_soul")
```

Depois filtramos somente os objetos que são realmente `AreaSoul`:

```csharp
.OfType<AreaSoul>()
```

E transformamos o resultado em um array:

```csharp
.ToArray();
```

O resultado é utilizado para descobrir a quantidade atual:

```csharp
_remainingSouls = souls.Length;
```

---

# 6. Quando uma alma é coletada

Quando o jogador (`Amos`) entra em contato com uma `AreaSoul`:

```csharp
private void OnBodyEntered(Node2D body)
{
    if (body is Amos)
    {
        GlobalBars globalBars = GetTree()
            .GetFirstNodeInGroup("global_bars") as GlobalBars;

        globalBars?.SoulCollected();

        QueueFree();
    }
}
```

A sequência é:

```text
Amos toca na AreaSoul
        ↓
OnBodyEntered()
        ↓
SoulCollected()
        ↓
GlobalBars atualiza a quantidade de almas
        ↓
ProgressBar é recalculada
        ↓
AreaSoul é removida com QueueFree()
```

---

# 7. Recalculando a barra

Quando `SoulCollected()` é chamado:

```csharp
public void SoulCollected()
{
    UpdateSoulsCount();
    UpdateProgressBar();
}
```

Primeiro descobrimos quantas almas ainda existem:

```text
_remainingSouls
```

Depois calculamos a porcentagem:

```csharp
double percentage =
    (double)_remainingSouls / _totalSouls * 100.0;
```

Por exemplo:

```text
_remainingSouls = 3
_totalSouls = 5
```

Resultado:

```text
3 / 5 × 100 = 60%
```

Então:

```csharp
_progressBar.Value = 60;
```

---

# 8. Dreno automático

A quantidade de almas não é o único fator que altera a barra.

A barra também diminui automaticamente com o tempo.

Isso acontece no `_Process()`:

```csharp
public override void _Process(double delta)
{
    if (_progressBar.Value > 0)
    {
        _progressBar.Value -= DrainSpeed * (float)delta;

        if (_progressBar.Value < 0)
        {
            _progressBar.Value = 0;
        }
    }
}
```

O `DrainSpeed` determina a velocidade da redução.

Por exemplo:

```csharp
[Export]
private float DrainSpeed = 5.0f;
```

Significa aproximadamente:

```text
5 pontos de ProgressBar por segundo
```

Portanto:

```text
100
 ↓
95
 ↓
90
 ↓
85
 ↓
...
```

O `delta` é utilizado para que a velocidade seja baseada no tempo e não na quantidade de frames.

---

# 9. Os dois mecanismos trabalham juntos

Existem dois eventos diferentes que podem alterar a barra.

### Dreno automático

Acontece continuamente:

```text
Tempo passa
   ↓
ProgressBar diminui
```

### Coleta de alma

Acontece quando o jogador coleta uma `AreaSoul`:

```text
AreaSoul coletada
   ↓
quantidade restante é atualizada
   ↓
barra é recalculada
```

Exemplo:

```text
5 almas
barra = 100%

tempo passa
barra = 80%

jogador coleta uma alma

4 almas restantes
barra = 80%

tempo passa
barra = 75%

tempo passa
barra = 70%

jogador coleta outra alma

3 almas restantes
barra = 60%
```

O ponto importante é que **a coleta recalcula a barra com base na quantidade de almas restantes**, enquanto o tempo continua drenando o valor.

---

# 10. Resumo da lógica

```text
                    INÍCIO
                       │
                       ▼
             Encontrar AreaSoul
                       │
                       ▼
              Contar quantidade
                       │
                       ▼
              _totalSouls = 5
                       │
                       ▼
              ProgressBar = 100%
                       │
                       ▼
              ┌────────────────┐
              │ Tempo passando │
              └───────┬────────┘
                      │
                      ▼
             Barra diminui
                      │
                      ▼
          Jogador coletou uma alma?
                 │           │
                NÃO         SIM
                 │           │
                 │           ▼
                 │     Atualizar almas
                 │           │
                 │           ▼
                 │     Calcular:
                 │
                 │   restantes / total
                 │          × 100
                 │           │
                 │           ▼
                 │      Atualizar barra
                 │           │
                 └───────┬───┘
                         │
                         ▼
                  Continuar jogo
```

## Fórmula principal

A regra central do sistema é:

```text
BARra = (ALMAS_RESTANTES / ALMAS_TOTAIS) × 100
```

Onde:

```text
ALMAS_TOTAIS
= quantidade de AreaSoul existentes no início da fase

ALMAS_RESTANTES
= quantidade de AreaSoul que ainda não foi coletada
```

O `DrainSpeed` atua separadamente sobre o valor atual da barra.

Assim, **a quantidade de almas define o valor de referência da barra, enquanto o tempo faz esse valor diminuir automaticamente**.
