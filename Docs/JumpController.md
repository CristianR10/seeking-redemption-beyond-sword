# Sistema de Pulo

O sistema de pulo foi desenvolvido para ser previsível, fácil de ajustar e proporcionar uma boa sensação de controle ao jogador. Em vez de definir valores arbitrários para gravidade e força do pulo, eles são calculados automaticamente com base na altura desejada e no tempo até o ápice do salto.

---

## Como funciona

O pulo é definido por duas variáveis principais:

- **JumpHeight**: altura máxima do pulo em pixels.
- **TimeToPeak**: tempo que o personagem leva para chegar ao ponto mais alto.

A partir desses valores, são calculados automaticamente:

- Gravidade durante a subida.
- Gravidade durante a queda.
- Velocidade inicial do pulo.

Isso significa que, para alterar a altura do salto, basta modificar `JumpHeight`, sem precisar recalibrar gravidade e velocidade manualmente.

Exemplo:

```csharp
JumpHeight = 64f;
TimeToPeak = 0.35f;
```

---

# Fórmulas

## Gravidade

```
Gravity = (2 × Altura) / (Tempo²)
```

## Velocidade Inicial

```
JumpVelocity = -(2 × Altura) / Tempo
```

O sinal negativo é utilizado porque, no Godot, o eixo Y cresce para baixo.

---

# Better Jump

Durante a subida e a descida são utilizadas gravidades diferentes.

- Subindo → gravidade menor.
- Caindo → gravidade maior.

Isso deixa o pulo mais agradável e responsivo, evitando uma queda lenta.

---

# Variable Jump

O jogador pode controlar a altura do pulo.

- Segurando o botão → pulo máximo.
- Soltando antes → pulo menor.

Ao soltar o botão durante a subida, parte da velocidade vertical é reduzida.

---

# Coyote Time

Permite pular mesmo alguns milissegundos depois de sair da plataforma.

Exemplo:

```
Jogador sai da borda

     ________
_____|       |

        O

Ainda é possível apertar pulo por aproximadamente 0.12 segundos.
```

Isso evita a sensação de que o jogo "ignorou" o comando.

---

# Jump Buffer

Armazena o comando de pulo por alguns milissegundos.

Exemplo:

```
Jogador aperta pulo antes de tocar no chão.

     O

___________

Assim que tocar no chão, o personagem pula automaticamente.
```

Isso melhora bastante a responsividade do jogo.

---

# Fast Fall (Opcional)

Caso implementado, ao segurar **↓**, a gravidade aumenta durante a queda.

Resultado:

- Queda normal.
- Queda acelerada ao segurar para baixo.

É uma técnica utilizada em diversos jogos de plataforma para dar mais controle ao jogador.

---

# Vantagens

- Fácil de configurar.
- Altura do pulo previsível.
- Sensação mais natural.
- Melhor experiência para o jogador.
- Código desacoplado e reutilizável.

---

# Parâmetros mais importantes

| Variável | Descrição |
|----------|-----------|
| JumpHeight | Altura máxima do pulo em pixels. |
| TimeToPeak | Tempo até chegar ao ponto mais alto. |
| TimeToFall | Tempo para retornar ao chão. |
| CoyoteTime | Tempo extra para permitir o pulo após sair da plataforma. |
| JumpBuffer | Tempo que o comando de pulo fica armazenado. |
| FastFallMultiplier | Multiplicador da gravidade ao segurar ↓ (opcional). |

---

# Resumo

O sistema calcula automaticamente toda a física do pulo com base na altura desejada, tornando os ajustes muito mais simples. Além disso, técnicas como **Better Jump**, **Variable Jump**, **Coyote Time** e **Jump Buffer** tornam o controle mais preciso, responsivo e agradável para o jogador.