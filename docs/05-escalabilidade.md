# 📈 Estratégias de Escalabilidade

## 1. Cenário de Carga

### 1.1 Requisitos de Volume

```mermaid
graph LR
    subgraph "Cenário Esperado"
        A["📱 Anúncios em<br/>Redes Sociais"] --> B["👥 1M+ pessoas<br/>veem o anúncio"]
        B --> C["🗳️ 100K+<br/>respondem"]
        C --> D["📊 Pico de<br/>10K req/s"]
    end

    classDef critical fill:#ffcdd2,stroke:#c62828,color:#000,stroke-width:2px;
    class D critical;
```

| Métrica | Valor Esperado | Crítico |
|---------|----------------|---------|
| Usuários diários | 100.000 | Suportar pico |
| Requisições/segundo (pico) | 10.000 | Performance crítica |
| Tamanho médio resposta | 5 KB | Otimizar payload |
| Tempo de resposta (P95) | < 200ms | UX aceitável |

---

## 2. Arquitetura Escalável

### 2.1 Diagrama de Escala Horizontal

```mermaid
graph TB
    subgraph clients["👥 Milhões de Respondentes"]
        C1["📱"]
        C2["📱"]
        C3["📱"]
        CN["📱..."]
    end

    LB["⚖️ Load Balancer<br/>(Nginx/HAProxy)"]

    subgraph api_cluster["🔄 Cluster de APIs"]
        API1["⚙️ API 1"]
        API2["⚙️ API 2"]
        API3["⚙️ API 3"]
        APIN["⚙️ API N..."]
    end

    REDIS["⚡ Redis Cluster<br/>Cache Distribuído"]

    subgraph db_cluster["💾 PostgreSQL"]
        PRIMARY["🐘 Primary<br/>(Write)"]
        REPLICA1["📖 Replica 1<br/>(Read)"]
        REPLICA2["📖 Replica 2<br/>(Read)"]
    end

    C1 --> LB
    C2 --> LB
    C3 --> LB
    CN --> LB

    LB --> API1
    LB --> API2
    LB --> API3
    LB --> APIN

    API1 --> REDIS
    API2 --> REDIS
    API3 --> REDIS
    APIN --> REDIS

    API1 --> PRIMARY
    API2 --> PRIMARY
    API3 --> PRIMARY

    API1 -.-> REPLICA1
    API2 -.-> REPLICA2
    API3 -.-> REPLICA1

    PRIMARY --> REPLICA1
    PRIMARY --> REPLICA2

    %% Styling
    classDef lb fill:#e0f7fa,stroke:#006064,color:#000,stroke-width:2px;
    classDef api fill:#bbdefb,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef redis fill:#ffcdd2,stroke:#c62828,color:#000,stroke-width:2px;
    classDef db fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;

    class LB lb;
    class API1,API2,API3,APIN api;
    class REDIS redis;
    class PRIMARY,REPLICA1,REPLICA2 db;
```

---

## 3. Estratégias Implementadas

### 3.1 Cache Redis (Cache-First Pattern)

```mermaid
sequenceDiagram
    autonumber
    participant C as Cliente
    participant A as API
    participant R as Redis
    participant D as PostgreSQL

    C->>A: GET /surveys/{url}/public
    A->>R: GET survey:{url}
    
    alt Cache Hit (90%+ dos casos)
        R-->>A: JSON da pesquisa
        A-->>C: ✅ Response < 10ms
    else Cache Miss
        A->>D: SELECT survey + questions + options
        D-->>A: Dados
        A->>R: SET survey:{url} TTL 5min
        A-->>C: ✅ Response < 100ms
    end
```

**Configuração:**
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "PublicPolls_";
});
```

**Benefícios:**
| Métrica | Sem Cache | Com Cache |
|---------|-----------|-----------|
| Latência média | 50ms | 5ms |
| Carga no DB | 100% | 10% |
| Throughput | 1K req/s | 10K req/s |

---

### 3.2 API Stateless

```mermaid
graph LR
    subgraph "Stateless Design"
        JWT["🎫 JWT Token<br/>Contém todas as claims"]
        API["⚙️ Qualquer instância<br/>pode processar"]
        NO["❌ Sem session server"]
    end

    JWT --> API --> NO

    %% Styling
    classDef jwt fill:#e1bee7,stroke:#6a1b9a,color:#000,stroke-width:2px;
    classDef api fill:#bbdefb,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef no fill:#ffcdd2,stroke:#c62828,color:#000,stroke-width:2px;

    class JWT jwt;
    class API api;
    class NO no;
```

**Por que stateless?**
- Qualquer instância pode processar qualquer requisição
- Fácil escalar horizontalmente
- Não precisa de sticky sessions
- Load balancer pode usar round-robin

---

### 3.3 Índices Otimizados

```mermaid
graph TB
    subgraph "Queries Frequentes"
        Q1["Buscar pesquisa por URL pública"]
        Q2["Listar pesquisas do usuário"]
        Q3["Verificar se IP já respondeu"]
        Q4["Agregar resultados"]
    end

    subgraph "Índices Criados"
        I1["IX_Surveys_PublicUrl (UNIQUE)"]
        I2["IX_Surveys_UserId"]
        I3["IX_Responses_SurveyId_RespondentIp"]
        I4["IX_Answers_OptionId + QuestionId"]
    end

    Q1 --> I1
    Q2 --> I2
    Q3 --> I3
    Q4 --> I4

    %% Styling
    classDef query fill:#bbdefb,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef idx fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;

    class Q1,Q2,Q3,Q4 query;
    class I1,I2,I3,I4 idx;
```

---

### 3.4 Rate Limiting

```mermaid
graph LR
    subgraph "Proteção contra Abuso"
        IP["📍 IP do Cliente"]
        RL["🚦 Rate Limiter<br/>(Redis)"]
        API["⚙️ API"]
    end

    IP -->|"Requisição"| RL
    RL -->|"✅ Dentro do limite"| API
    RL -->|"❌ 429 Too Many Requests"| IP

    classDef rl fill:#e0f7fa,stroke:#006064,color:#000,stroke-width:2px;
    class RL rl;
```

**Regras:**
| Endpoint | Limite | Janela |
|----------|--------|--------|
| POST /responses | 1 por survey | Por IP |
| GET /surveys/public | 100 | Por minuto |
| POST /auth/login | 5 | Por minuto |

---

## 4. Estratégias Futuras (Não Implementadas)

### 4.1 Escalabilidade do Banco de Dados

```mermaid
graph TB
    subgraph "Fase 1 (Atual)"
        P1["🐘 PostgreSQL Single"]
    end

    subgraph "Fase 2 (Se necessário)"
        P2["🐘 Primary + Read Replicas"]
    end

    subgraph "Fase 3 (Se necessário)"
        P3["🐘 Sharding por Survey"]
    end

    P1 -->|"100K respostas/dia"| P2
    P2 -->|"1M respostas/dia"| P3

    %% Styling
    classDef phase fill:#bbdefb,stroke:#0d47a1,color:#000,stroke-width:2px;
    class P1,P2,P3 phase;
```

### 4.2 Mensageria Assíncrona

```mermaid
graph LR
    subgraph "Futuro: RabbitMQ"
        API["⚙️ API"]
        Q["📬 Queue<br/>Respostas"]
        W["🔄 Worker<br/>Processador"]
        DB["🐘 PostgreSQL"]
    end

    API -->|"Publica"| Q
    Q -->|"Consome"| W
    W -->|"Persiste"| DB

    classDef queue fill:#e0f7fa,stroke:#006064,color:#000,stroke-width:2px;
    class Q queue;
```

**Quando implementar:**
- Se latência de escrita > 100ms
- Se perda de respostas for inaceitável
- Se precisar de retry automático

---

## 5. Monitoramento (Recomendado)

### 5.1 Métricas Importantes

graph TB
    root("📊 Métricas")

    subgraph "🚀 Performance"
        P1["Response Time P50"]
        P2["Response Time P95"]
        P3["Response Time P99"]
        P4["Throughput req/s"]
    end

    subgraph "🆙 Disponibilidade"
        D1["Uptime %"]
        D2["Error Rate %"]
        D3["Circuit Breaker status"]
    end

    subgraph "💾 Recursos"
        R1["CPU %"]
        R2["Memory %"]
        R3["DB connections"]
        R4["Redis memory"]
    end

    subgraph "💼 Negócio"
        N1["Respostas/hora"]
        N2["Pesquisas ativas"]
        N3["Usuários únicos"]
    end

    root --> P1
    root --> D1
    root --> R1
    root --> N1

    P1 ~~~ P2 ~~~ P3 ~~~ P4
    D1 ~~~ D2 ~~~ D3
    R1 ~~~ R2 ~~~ R3 ~~~ R4
    N1 ~~~ N2 ~~~ N3

    %% Styling
    classDef root fill:#212121,stroke:#000,color:#fff,stroke-width:2px;
    classDef perf fill:#bbdefb,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef avail fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;
    classDef res fill:#ffe0b2,stroke:#ef6c00,color:#000,stroke-width:2px;
    classDef biz fill:#e1bee7,stroke:#6a1b9a,color:#000,stroke-width:2px;

    class root root;
    class P1,P2,P3,P4 perf;
    class D1,D2,D3 avail;
    class R1,R2,R3,R4 res;
    class N1,N2,N3 biz;

### 5.2 Stack Recomendada

| Componente | Ferramenta | Propósito |
|------------|------------|-----------|
| APM | Application Insights | Traces, métricas |
| Logs | Serilog + Seq | Log agregado |
| Dashboard | Grafana | Visualização |
| Alertas | PagerDuty | Notificações |

---

## 6. Estimativas de Capacidade

### 6.1 Cenário: Dia da Eleição

```mermaid
gantt
    title Carga Esperada - Dia da Eleição
    dateFormat HH:mm
    axisFormat %H:%M

    section Manhã
    Pico 1 (8h)    :active, 08:00, 2h
    Normal          :09:00, 3h

    section Tarde
    Pico 2 (12h)   :crit, 12:00, 2h
    Normal          :14:00, 4h

    section Noite
    Pico 3 (19h)   :crit, 19:00, 3h
    Queda           :22:00, 2h
```

### 6.2 Dimensionamento

| Carga | Instâncias API | Redis | PostgreSQL |
|-------|----------------|-------|------------|
| 1K req/s | 2 | 1 (256MB) | 1 (2 vCPU) |
| 5K req/s | 4 | 1 (512MB) | 1 (4 vCPU) |
| 10K req/s | 8 | 3 (Cluster) | 1 + 2 Replicas |
| 50K req/s | 20 | 6 (Cluster) | Sharding |

---

## Próximo Documento

➡️ [Referência da API](06-api-reference.md)
