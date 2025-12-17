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
    subgraph ext["🌐 Sistemas Externos"]
        SM["📱 Redes Sociais<br/>(Facebook, Instagram, Twitter, WhatsApp)"]
        EM["📧 Serviço de Email<br/>(SMTP para notificações)"]
    end

    subgraph users["👥 Usuários"]
        R["🗳️ Respondente<br/>Cidadão que acessa<br/>link da pesquisa"]
        A["👔 Administrador<br/>Gestor que cria<br/>e analisa pesquisas"]
    end

    PP[("🏛️ PublicPolls<br/>Sistema de Questionários Online<br/><br/>Permite criar pesquisas de<br/>múltipla escolha e coletar<br/>respostas em larga escala")]

    R -->|"Responde pesquisas<br/>[HTTPS]"| PP
    A -->|"Gerencia pesquisas<br/>Visualiza resultados<br/>[HTTPS]"| PP
    SM -.->|"Divulga links<br/>[Anúncios/Posts]"| R
    PP -.->|"Envia notificações<br/>[SMTP]"| EM

    style PP fill:#1976d2,color:#fff,stroke:#1565c0
    style R fill:#e3f2fd,stroke:#1976d2
    style A fill:#fff3e0,stroke:#ff9800
    style SM fill:#f5f5f5,stroke:#9e9e9e
    style EM fill:#f5f5f5,stroke:#9e9e9e
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
    subgraph users["👥 Usuários"]
        R["🗳️ Respondente"]
        A["👔 Administrador"]
    end

    subgraph boundary["🏛️ PublicPolls System"]
        subgraph frontend["Frontend"]
            WEB["🖥️ Frontend Web<br/><br/>[Blazor WebAssembly]<br/><br/>Interface SPA para<br/>responder pesquisas<br/>e dashboard admin"]
        end

        subgraph backend["Backend"]
            API["⚙️ API REST<br/><br/>[ASP.NET Core 8]<br/><br/>Endpoints RESTful<br/>Autenticação JWT<br/>Swagger/OpenAPI"]
        end

        subgraph data["Camada de Dados"]
            CACHE["⚡ Cache<br/><br/>[Redis 7]<br/><br/>Cache de pesquisas<br/>Rate Limiting<br/>Sessões"]

            DB[("🐘 Banco de Dados<br/><br/>[PostgreSQL 15]<br/><br/>Usuários, Pesquisas<br/>Perguntas, Opções<br/>Respostas")]
        end
    end

    R -->|"Responde pesquisa<br/>[HTTPS]"| WEB
    A -->|"Gerencia sistema<br/>[HTTPS]"| WEB
    WEB -->|"Chamadas API<br/>[HTTPS/JSON]"| API
    API -->|"Lê/Escreve<br/>[Redis Protocol]"| CACHE
    API -->|"CRUD<br/>[TCP/SQL]"| DB

    style WEB fill:#7c4dff,color:#fff
    style API fill:#4caf50,color:#fff
    style CACHE fill:#f44336,color:#fff
    style DB fill:#2196f3,color:#fff
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
    subgraph api["⚙️ API REST - ASP.NET Core 8"]
        subgraph controllers["Controllers (Entrada)"]
            AC["🔐 AuthController<br/><br/>POST /register<br/>POST /login"]
            SC["📋 SurveysController<br/><br/>GET /surveys<br/>POST /surveys<br/>GET /surveys/{id}<br/>PUT /surveys/{id}<br/>DELETE /surveys/{id}<br/>GET /surveys/{url}/public<br/>POST /surveys/{id}/responses<br/>GET /surveys/{id}/results"]
        end

        subgraph services["Services (Lógica de Negócio)"]
            AS["🔑 AuthService<br/><br/>• Registro<br/>• Login<br/>• Geração JWT"]
            SS["📊 SurveyService<br/><br/>• CRUD Pesquisas<br/>• Validações"]
            RS["📝 ResponseService<br/><br/>• Submit respostas<br/>• Validação IP"]
            RES["📈 ResultsService<br/><br/>• Agregação<br/>• Cálculo %"]
        end

        subgraph repos["Repositories (Acesso a Dados)"]
            UR["👤 UserRepository"]
            SR["📋 SurveyRepository"]
            RR["📝 ResponseRepository"]
        end

        subgraph infra["Infrastructure"]
            CTX["🗄️ AppDbContext<br/>[Entity Framework Core]"]
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

    style AC fill:#ff9800,color:#fff
    style SC fill:#ff9800,color:#fff
    style AS fill:#4caf50,color:#fff
    style SS fill:#4caf50,color:#fff
    style RS fill:#4caf50,color:#fff
    style RES fill:#4caf50,color:#fff
    style UR fill:#2196f3,color:#fff
    style SR fill:#2196f3,color:#fff
    style RR fill:#2196f3,color:#fff
    style CTX fill:#9c27b0,color:#fff
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
    subgraph web["🖥️ Frontend Web - Blazor WebAssembly"]
        subgraph pages["Pages (Razor)"]
            IP["🏠 Index.razor<br/>Landing Page"]
            LP["🔐 Login.razor<br/>Autenticação"]
            RP["📝 Register.razor<br/>Cadastro"]
            SLP["📋 Surveys.razor<br/>Lista de Pesquisas"]
            NSP["➕ NewSurvey.razor<br/>Criar Pesquisa"]
            RSP["📊 Results.razor<br/>Resultados"]
            PSP["🗳️ PublicSurvey.razor<br/>Responder Pesquisa"]
        end

        subgraph services["Services (HTTP)"]
            ASVC["🔑 AuthService<br/>Login/Registro"]
            SSVC["📊 SurveyService<br/>CRUD Pesquisas"]
        end

        subgraph shared["Shared"]
            ML["📐 MainLayout.razor<br/>Layout Principal"]
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

    style IP fill:#7c4dff,color:#fff
    style LP fill:#7c4dff,color:#fff
    style RP fill:#7c4dff,color:#fff
    style SLP fill:#7c4dff,color:#fff
    style NSP fill:#7c4dff,color:#fff
    style RSP fill:#7c4dff,color:#fff
    style PSP fill:#7c4dff,color:#fff
```

---

## 6. Diagrama de Implantação

```mermaid
graph TB
    subgraph client["🖥️ Cliente"]
        B["🌐 Browser<br/>Chrome, Firefox, Safari, Edge"]
    end

    subgraph docker["🐳 Docker Compose"]
        subgraph containers["Containers"]
            C1["📦 publicpolls-api<br/>ASP.NET Core 8<br/>Porta: 5001"]
            C2["📦 publicpolls-web<br/>Nginx + Blazor<br/>Porta: 5002"]
            C3["📦 publicpolls-db<br/>PostgreSQL 15<br/>Porta: 5432"]
            C4["📦 publicpolls-cache<br/>Redis 7<br/>Porta: 6379"]
        end

        subgraph volumes["Volumes"]
            V1["💾 postgres_data"]
            V2["💾 redis_data"]
        end
    end

    B -->|HTTPS:5002| C2
    C2 -->|HTTP:5001| C1
    C1 -->|TCP:5432| C3
    C1 -->|TCP:6379| C4
    C3 --> V1
    C4 --> V2

    style C1 fill:#4caf50,color:#fff
    style C2 fill:#7c4dff,color:#fff
    style C3 fill:#2196f3,color:#fff
    style C4 fill:#f44336,color:#fff
```

---

## Próximo Documento

➡️ [Modelo de Dados](03-modelo-dados.md)
