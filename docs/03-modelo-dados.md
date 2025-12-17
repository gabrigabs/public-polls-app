# 💾 Modelo de Dados

## 1. Diagrama Entidade-Relacionamento

```mermaid
erDiagram
    USER ||--o{ SURVEY : "cria"
    SURVEY ||--|{ QUESTION : "contém"
    QUESTION ||--|{ OPTION : "possui"
    SURVEY ||--o{ RESPONSE : "recebe"
    RESPONSE ||--|{ ANSWER : "contém"
    QUESTION ||--o{ ANSWER : "referencia"
    OPTION ||--o{ ANSWER : "selecionada em"

    USER {
        uuid Id PK "Identificador único"
        string Email UK "Email único"
        string PasswordHash "Senha criptografada"
        string Name "Nome do usuário"
        enum Role "Admin | Viewer"
        datetime CreatedAt "Data de criação"
        datetime UpdatedAt "Última atualização"
    }

    SURVEY {
        uuid Id PK "Identificador único"
        uuid UserId FK "Criador da pesquisa"
        string Title "Título da pesquisa"
        string Description "Descrição opcional"
        datetime StartDate "Início da coleta"
        datetime EndDate "Fim da coleta"
        boolean IsActive "Se está ativa"
        string PublicUrl UK "Código público único"
        datetime CreatedAt "Data de criação"
        datetime UpdatedAt "Última atualização"
    }

    QUESTION {
        uuid Id PK "Identificador único"
        uuid SurveyId FK "Pesquisa pai"
        string Text "Texto da pergunta"
        int Order "Ordem de exibição"
        boolean IsRequired "Se é obrigatória"
        datetime CreatedAt "Data de criação"
    }

    OPTION {
        uuid Id PK "Identificador único"
        uuid QuestionId FK "Pergunta pai"
        string Text "Texto da opção"
        int Order "Ordem de exibição"
        datetime CreatedAt "Data de criação"
    }

    RESPONSE {
        uuid Id PK "Identificador único"
        uuid SurveyId FK "Pesquisa respondida"
        string RespondentIp "IP do respondente"
        string UserAgent "Browser do respondente"
        datetime SubmittedAt "Data da resposta"
    }

    ANSWER {
        uuid Id PK "Identificador único"
        uuid ResponseId FK "Resposta pai"
        uuid QuestionId FK "Pergunta respondida"
        uuid OptionId FK "Opção selecionada"
        datetime CreatedAt "Data de criação"
    }
```

---

## 2. Descrição das Entidades

### 2.1 User (Usuário)

Representa um administrador do sistema que pode criar e gerenciar pesquisas.

```mermaid
classDiagram
    class User {
        +Guid Id
        +string Email
        +string PasswordHash
        +string Name
        +UserRole Role
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +ICollection~Survey~ Surveys
    }

    class UserRole {
        <<enumeration>>
        Admin = 1
        Viewer = 2
    }

    User --> UserRole
```

| Campo | Tipo | Descrição | Restrições |
|-------|------|-----------|------------|
| Id | UUID | Identificador único | PK, Auto-gerado |
| Email | string | Endereço de email | Unique, Required, Max 256 |
| PasswordHash | string | Senha criptografada (SHA256) | Required |
| Name | string | Nome do usuário | Required, Max 100 |
| Role | enum | Papel no sistema | Default: Admin |
| CreatedAt | DateTime | Data de criação | Auto |
| UpdatedAt | DateTime? | Última atualização | Nullable |

### 2.2 Survey (Pesquisa)

Representa uma pesquisa com suas configurações.

```mermaid
classDiagram
    class Survey {
        +Guid Id
        +Guid UserId
        +string Title
        +string Description
        +DateTime StartDate
        +DateTime EndDate
        +bool IsActive
        +string PublicUrl
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +User User
        +ICollection~Question~ Questions
        +ICollection~Response~ Responses
        +bool IsOpen
    }
```

| Campo | Tipo | Descrição | Restrições |
|-------|------|-----------|------------|
| Id | UUID | Identificador único | PK |
| UserId | UUID | Criador | FK → User |
| Title | string | Título da pesquisa | Required, Max 200 |
| Description | string | Descrição | Max 1000 |
| StartDate | DateTime | Início da coleta | Required |
| EndDate | DateTime | Fim da coleta | Required |
| IsActive | bool | Se está ativa | Default: true |
| PublicUrl | string | Código público | Unique, Max 50, Auto-gerado |
| IsOpen | bool | Propriedade calculada | StartDate <= now <= EndDate && IsActive |

### 2.3 Question (Pergunta)

Representa uma pergunta de múltipla escolha.

```mermaid
classDiagram
    class Question {
        +Guid Id
        +Guid SurveyId
        +string Text
        +int Order
        +bool IsRequired
        +DateTime CreatedAt
        +Survey Survey
        +ICollection~Option~ Options
        +ICollection~Answer~ Answers
    }
```

### 2.4 Option (Opção de Resposta)

Representa uma alternativa de resposta.

```mermaid
classDiagram
    class Option {
        +Guid Id
        +Guid QuestionId
        +string Text
        +int Order
        +DateTime CreatedAt
        +Question Question
        +ICollection~Answer~ Answers
    }
```

### 2.5 Response (Submissão de Resposta)

Representa uma submissão completa de um respondente.

```mermaid
classDiagram
    class Response {
        +Guid Id
        +Guid SurveyId
        +string RespondentIp
        +string UserAgent
        +DateTime SubmittedAt
        +Survey Survey
        +ICollection~Answer~ Answers
    }
```

### 2.6 Answer (Resposta Individual)

Representa a resposta a uma pergunta específica.

```mermaid
classDiagram
    class Answer {
        +Guid Id
        +Guid ResponseId
        +Guid QuestionId
        +Guid OptionId
        +DateTime CreatedAt
        +Response Response
        +Question Question
        +Option Option
    }
```

---

## 3. Relacionamentos

```mermaid
graph LR
    subgraph "1:N - Um para Muitos"
        U[User] -->|"1"| S1[Survey]
        S1 -->|"N"| S2[Survey]
        
        SV[Survey] -->|"1"| Q1[Question]
        Q1 -->|"N"| Q2[Question]
        
        Q[Question] -->|"1"| O1[Option]
        O1 -->|"N"| O2[Option]
        
        SVR[Survey] -->|"1"| R1[Response]
        R1 -->|"N"| R2[Response]
        
        RE[Response] -->|"1"| A1[Answer]
        A1 -->|"N"| A2[Answer]
    end
```

| Relacionamento | Cardinalidade | Descrição |
|----------------|---------------|-----------|
| User → Survey | 1:N | Um usuário pode criar várias pesquisas |
| Survey → Question | 1:N | Uma pesquisa tem várias perguntas |
| Question → Option | 1:N | Uma pergunta tem várias opções |
| Survey → Response | 1:N | Uma pesquisa recebe várias respostas |
| Response → Answer | 1:N | Uma submissão tem várias respostas individuais |
| Question → Answer | 1:N | Uma pergunta é referenciada em várias respostas |
| Option → Answer | 1:N | Uma opção pode ser selecionada várias vezes |

---

## 4. Índices e Constraints

### 4.1 Índices

```sql
-- User
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- Survey
CREATE UNIQUE INDEX IX_Surveys_PublicUrl ON Surveys(PublicUrl);
CREATE INDEX IX_Surveys_UserId ON Surveys(UserId);
CREATE INDEX IX_Surveys_IsActive_StartDate_EndDate ON Surveys(IsActive, StartDate, EndDate);

-- Question
CREATE INDEX IX_Questions_SurveyId ON Questions(SurveyId);

-- Option
CREATE INDEX IX_Options_QuestionId ON Options(QuestionId);

-- Response
CREATE INDEX IX_Responses_SurveyId ON Responses(SurveyId);
CREATE INDEX IX_Responses_SurveyId_RespondentIp ON Responses(SurveyId, RespondentIp);

-- Answer
CREATE INDEX IX_Answers_ResponseId ON Answers(ResponseId);
CREATE INDEX IX_Answers_QuestionId ON Answers(QuestionId);
CREATE INDEX IX_Answers_OptionId ON Answers(OptionId);
```

### 4.2 Constraints de Delete

```mermaid
graph TD
    U[User] -->|CASCADE| S[Survey]
    S -->|CASCADE| Q[Question]
    Q -->|CASCADE| O[Option]
    S -->|CASCADE| R[Response]
    R -->|CASCADE| A[Answer]
    Q -.->|RESTRICT| A
    O -.->|RESTRICT| A

    %% Styling
    classDef u fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;
    classDef s fill:#bbdefb,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef q fill:#e1bee7,stroke:#6a1b9a,color:#000,stroke-width:2px;
    classDef o fill:#fff9c4,stroke:#fbc02d,color:#000,stroke-width:2px;
    classDef r fill:#ffe0b2,stroke:#ef6c00,color:#000,stroke-width:2px;
    classDef a fill:#ffcdd2,stroke:#c62828,color:#000,stroke-width:2px;

    class U u;
    class S s;
    class Q q;
    class O o;
    class R r;
    class A a;
```

| FK | On Delete | Justificativa |
|----|-----------|---------------|
| Survey.UserId | CASCADE | Deletar usuário deleta suas pesquisas |
| Question.SurveyId | CASCADE | Deletar pesquisa deleta perguntas |
| Option.QuestionId | CASCADE | Deletar pergunta deleta opções |
| Response.SurveyId | CASCADE | Deletar pesquisa deleta respostas |
| Answer.ResponseId | CASCADE | Deletar resposta deleta answers |
| Answer.QuestionId | RESTRICT | Não deletar pergunta se houver respostas |
| Answer.OptionId | RESTRICT | Não deletar opção se houver respostas |

---

## 5. Queries Importantes

### 5.1 Buscar Pesquisa Pública (Cache-first)

```mermaid
sequenceDiagram
    autonumber
    participant A as API
    participant R as Redis
    participant D as PostgreSQL

    A->>R: GET survey:{publicUrl}
    alt Cache Hit
        R-->>A: JSON da pesquisa
    else Cache Miss
        A->>D: SELECT survey, questions, options
        D-->>A: Dados
        A->>R: SET survey:{publicUrl} TTL 5min
        R-->>A: OK
    end
```

### 5.2 Agregação de Resultados

```sql
SELECT 
    q.Id AS QuestionId,
    q.Text AS QuestionText,
    o.Id AS OptionId,
    o.Text AS OptionText,
    COUNT(a.Id) AS Count,
    ROUND(COUNT(a.Id) * 100.0 / NULLIF(SUM(COUNT(a.Id)) OVER (PARTITION BY q.Id), 0), 2) AS Percentage
FROM Questions q
JOIN Options o ON o.QuestionId = q.Id
LEFT JOIN Answers a ON a.OptionId = o.Id
WHERE q.SurveyId = @SurveyId
GROUP BY q.Id, q.Text, o.Id, o.Text
ORDER BY q.Order, o.Order;
```

---

## Próximo Documento

➡️ [Justificativas Técnicas](04-justificativas-tecnicas.md)
