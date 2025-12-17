# 🏗️ Arquitetura C4 Model

## 1. Introdução ao C4 Model

O C4 Model é uma abordagem de documentação de arquitetura de software criada por Simon Brown. Consiste em 4 níveis de abstração:

```mermaid
graph TD
    L1["📍 Nível 1: Contexto<br/>Visão de alto nível do sistema"]
    L2["📦 Nível 2: Container<br/>Aplicações e armazenamentos"]
    L3["🧩 Nível 3: Componente<br/>Módulos e classes principais"]
    L4["📝 Nível 4: Código<br/>Detalhes de implementação"]

    L1 --> L2 --> L3 --> L4

    style L1 fill:#e3f2fd
    style L2 fill:#f3e5f5
    style L3 fill:#e8f5e9
    style L4 fill:#fff3e0
```

---

## 2. Nível 1: Diagrama de Contexto

### 2.1 Diagrama

```mermaid
graph TB
    subgraph "👥 Usuários"
        R[("🗳️ Respondente<br/>Cidadão")]
        A[("👔 Administrador<br/>Gestor")]
    end

    subgraph "📍 Sistema PublicPolls"
        PP["🏛️ PublicPolls<br/>Sistema de Questionários"]
    end

    subgraph "🌐 Externos"
        SM["📱 Redes Sociais<br/>Divulgação"]
        EM["📧 Serviço de Email<br/>Notificações"]
    end

    R -->|Responde via HTTPS| PP
    A -->|Gerencia via HTTPS| PP
    SM -.->|Divulga Links| R
    PP -.->|Envia Email| EM

    %% Styling
    classDef users fill:#e8f5e9,stroke:#1b5e20,color:#000,stroke-width:2px;
    classDef system fill:#e3f2fd,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef ext fill:#f5f5f5,stroke:#616161,color:#000,stroke-width:2px,stroke-dasharray: 5 5;

    class R,A users;
    class PP system;
    class SM,EM ext;
```

### 2.2 Descrição dos Elementos

| Elemento | Tipo | Descrição |
|----------|------|-----------|
| **Respondente** | Pessoa | Cidadão que acessa o link da pesquisa compartilhado nas redes sociais e responde às perguntas |
| **Administrador** | Pessoa | Usuário autenticado que cria pesquisas, gerencia perguntas e visualiza resultados |
| **PublicPolls** | Sistema | Sistema central que gerencia pesquisas de múltipla escolha |
| **Redes Sociais** | Sistema Externo | Canais onde os links das pesquisas são divulgados |
| **Serviço de Email** | Sistema Externo | Serviço SMTP para envio de notificações |

---

## 3. Nível 2: Diagrama de Container

### 3.1 Diagrama

```mermaid
graph TB
    subgraph "👥 Usuários"
        R[("🗳️ Respondente")]
        A[("👔 Administrador")]
    end

    subgraph "📦 PublicPolls System"
        WEB["🖥️ Frontend Web<br/>Blazor WebAssembly"]
        API["⚙️ API REST<br/>ASP.NET Core 8"]
        
        subgraph "💾 Dados"
            CACHE[("⚡ Cache<br/>Redis 7")]
            DB[("🐘 Banco de Dados<br/>PostgreSQL 15")]
        end
    end

    R -->|HTTPS| WEB
    A -->|HTTPS| WEB
    WEB -->|HTTPS/JSON| API
    API -->|Redis Protocol| CACHE
    API -->|TCP/SQL| DB

    %% Styling
    classDef users fill:#e8f5e9,stroke:#1b5e20,color:#000,stroke-width:2px;
    classDef front fill:#f3e5f5,stroke:#4a148c,color:#000,stroke-width:2px;
    classDef back fill:#e3f2fd,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef data fill:#fff9c4,stroke:#fbc02d,color:#000,stroke-width:2px;

    class R,A users;
    class WEB front;
    class API back;
    class CACHE,DB data;
```

### 3.2 Descrição dos Containers

| Container | Tecnologia | Responsabilidade |
|-----------|------------|------------------|
| **Frontend Web** | Blazor WebAssembly | Interface SPA para respondentes e administradores. Roda no browser usando WebAssembly. |
| **API REST** | ASP.NET Core 8 | Endpoints RESTful para todas as operações. Autenticação via JWT. Documentação via Swagger. |
| **Cache** | Redis 7 | Cache de pesquisas ativas para reduzir carga no banco. Rate limiting por IP. |
| **Banco de Dados** | PostgreSQL 15 | Armazenamento persistente de todos os dados: usuários, pesquisas, perguntas, opções e respostas. |

### 3.3 Fluxo de Dados

```mermaid
sequenceDiagram
    participant R as Respondente
    participant W as Blazor Web
    participant A as API REST
    participant C as Redis Cache
    participant D as PostgreSQL

    Note over R,D: Fluxo de Resposta de Pesquisa

    R->>W: Acessa /p/{publicUrl}
    W->>A: GET /api/surveys/{url}/public
    A->>C: Busca no cache
    
    alt Cache Hit
        C-->>A: Retorna pesquisa
    else Cache Miss
        A->>D: SELECT survey, questions, options
        D-->>A: Dados da pesquisa
        A->>C: Armazena no cache (TTL 5min)
    end
    
    A-->>W: JSON da pesquisa
    W-->>R: Renderiza questionário
    
    R->>W: Submete respostas
    W->>A: POST /api/surveys/{id}/responses
    A->>D: INSERT response, answers
    D-->>A: Confirmação
    A-->>W: { success: true }
    W-->>R: Tela de confirmação
```

---

## 4. Nível 3: Diagrama de Componentes (API)

### 4.1 Diagrama

```mermaid
graph TB
    subgraph "⚙️ API REST - ASP.NET Core 8"
        subgraph "🎯 Controllers"
            AC["🔐 AuthController"]
            SC["📋 SurveysController"]
        end

        subgraph "🧠 Services"
            AS["🔑 AuthService"]
            SS["📊 SurveyService"]
            RS["📝 ResponseService"]
            RES["📈 ResultsService"]
        end

        subgraph "💾 Repositories"
            UR["👤 UserRepository"]
            SR["📋 SurveyRepository"]
            RR["📝 ResponseRepository"]
        end

        subgraph "🏗️ Infrastructure"
            CTX["🗄️ AppDbContext<br/>EF Core"]
        end
    end

    AC --> AS
    SC --> SS
    SC --> RS
    SC --> RES
    
    AS --> UR
    SS --> SR
    RS --> RR
    RES --> RR
    
    UR --> CTX
    SR --> CTX
    RR --> CTX

    %% Styling
    classDef ctrl fill:#ffe0b2,stroke:#ef6c00,color:#000,stroke-width:2px;
    classDef svc fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;
    classDef repo fill:#bbdefb,stroke:#1565c0,color:#000,stroke-width:2px;
    classDef infra fill:#e1bee7,stroke:#6a1b9a,color:#000,stroke-width:2px;

    class AC,SC ctrl;
    class AS,SS,RS,RES svc;
    class UR,SR,RR repo;
    class CTX infra;
```

### 4.2 Descrição dos Componentes

| Camada | Componente | Responsabilidade |
|--------|------------|------------------|
| **Controller** | AuthController | Recebe requisições de autenticação e delega para AuthService |
| **Controller** | SurveysController | Recebe requisições de pesquisas, respostas e resultados |
| **Service** | AuthService | Registro de usuários, validação de credenciais, geração de JWT |
| **Service** | SurveyService | Regras de negócio de pesquisas: criação, atualização, listagem |
| **Service** | ResponseService | Validação e persistência de respostas, controle de duplicatas |
| **Service** | ResultsService | Agregação de respostas e cálculo de percentuais |
| **Repository** | UserRepository | CRUD de usuários via EF Core |
| **Repository** | SurveyRepository | CRUD de pesquisas, perguntas e opções |
| **Repository** | ResponseRepository | Persistência de respostas e consultas agregadas |
| **Infrastructure** | AppDbContext | Configuração do EF Core, mapeamento de entidades |

---

## 5. Nível 3: Diagrama de Componentes (Frontend)

### 5.1 Diagrama

```mermaid
graph TB
    subgraph "🖥️ Frontend Web - Blazor WebAssembly"
        subgraph "📄 Pages"
            IP["🏠 Index.razor"]
            LP["🔐 Login.razor"]
            RP["📝 Register.razor"]
            SLP["📋 Surveys.razor"]
            NSP["➕ NewSurvey.razor"]
            RSP["📊 Results.razor"]
            PSP["🗳️ PublicSurvey.razor"]
        end

        subgraph "🔌 Services"
            ASVC["🔑 AuthService"]
            SSVC["📊 SurveyService"]
        end

        subgraph "📐 Shared"
            ML["📐 MainLayout.razor"]
        end
    end

    LP --> ASVC
    RP --> ASVC
    SLP --> SSVC
    NSP --> SSVC
    RSP --> SSVC
    PSP --> SSVC

    IP --> ML
    LP --> ML
    RP --> ML
    SLP --> ML
    NSP --> ML
    RSP --> ML
    PSP --> ML

    %% Styling
    classDef page fill:#e1bee7,stroke:#6a1b9a,color:#000,stroke-width:2px;
    classDef svc fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;
    classDef shared fill:#f5f5f5,stroke:#616161,color:#000,stroke-width:2px;

    class IP,LP,RP,SLP,NSP,RSP,PSP page;
    class ASVC,SSVC svc;
    class ML shared;
```

---

## 6. Diagrama de Implantação

```mermaid
graph TB
    subgraph "🖥️ Cliente"
        B["🌐 Browser<br/>Chrome, Edge"]
    end

    subgraph "🐳 Docker Compose"
        subgraph "📦 Containers"
            C1["🔌 publicpolls-api<br/>:5001"]
            C2["🖥️ publicpolls-web<br/>:5002"]
            C3["🐘 publicpolls-db<br/>:5432"]
            C4["⚡ publicpolls-cache<br/>:6379"]
        end

        subgraph "💾 Volumes"
            V1["💾 postgres_data"]
            V2["💾 redis_data"]
        end
    end

    B -->|HTTPS:5002| C2
    C2 -->|HTTP:5001| C1
    C1 -->|TCP:5432| C3
    C1 -->|TCP:6379| C4
    C3 -.-> V1
    C4 -.-> V2

    %% Styling
    classDef client fill:#bbdefb,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef container fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;
    classDef volume fill:#fff9c4,stroke:#fbc02d,color:#000,stroke-width:2px;

    class B client;
    class C1,C2,C3,C4 container;
    class V1,V2 volume;
```

---

## Próximo Documento

➡️ [Modelo de Dados](03-modelo-dados.md)
