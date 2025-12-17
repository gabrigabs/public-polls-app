# 📋 Visão Geral do Sistema

## 1. Introdução

### 1.1 Contexto do Projeto

A startup identificou uma oportunidade de mercado no segmento de pesquisas eleitorais digitais. Com o crescimento das redes sociais como canal de comunicação, existe demanda por uma plataforma que permita:

- Criar pesquisas de intenção de voto rapidamente
- Distribuir via anúncios em redes sociais
- Coletar milhões de respostas
- Apresentar resultados sumarizados

### 1.2 Problema a Resolver

```mermaid
graph LR
    subgraph "Situação Atual"
        A[Pesquisas Tradicionais] --> B[Alto Custo]
        A --> C[Amostra Limitada]
        A --> D[Demora na Coleta]
    end

    subgraph "Solução Proposta"
        E[PublicPolls] --> F[Baixo Custo]
        E --> G[Milhões de Respostas]
        E --> H[Resultados em Tempo Real]
    end

    classDef solution fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;
    class E,F,G,H solution;
```

---

## 2. Objetivos do Sistema

### 2.1 Objetivos de Negócio

| Objetivo | Descrição | Métrica de Sucesso |
|----------|-----------|-------------------|
| **Escala** | Suportar milhões de respondentes | > 1M respostas/dia |
| **Velocidade** | Resultados em tempo real | Agregação < 5 segundos |
| **Simplicidade** | Interface intuitiva | < 2 minutos para responder |
| **Prazo** | Entrega antes das eleições | Deploy em 4 semanas |

### 2.2 Objetivos Técnicos

```graph TB
    root("🎯 Objetivos Técnicos")
    
    subgraph "🚀 Performance"
        P1["Response time < 200ms"]
        P2["Throughput > 10k req/s"]
        P3["Cache hit ratio > 80%"]
    end
    
    subgraph "🆙 Disponibilidade"
        D1["Uptime 99.9%"]
        D2["Zero downtime deploy"]
        D3["Failover automático"]
    end
    
    subgraph "🛠️ Manutenibilidade"
        M1["Clean Architecture"]
        M2["Testes automatizados"]
        M3["Documentação completa"]
    end
    
    subgraph "🔐 Segurança"
        S1["JWT Authentication"]
        S2["Rate Limiting"]
        S3["Input Validation"]
    end

    root --> P1
    root --> D1
    root --> M1
    root --> S1
    
    P1 ~~~ P2 ~~~ P3
    D1 ~~~ D2 ~~~ D3
    M1 ~~~ M2 ~~~ M3
    S1 ~~~ S2 ~~~ S3

    %% Styling
    classDef root fill:#212121,stroke:#000,color:#fff,stroke-width:2px;
    classDef perf fill:#e3f2fd,stroke:#1565c0,color:#000,stroke-width:2px;
    classDef avail fill:#e8f5e9,stroke:#2e7d32,color:#000,stroke-width:2px;
    classDef maint fill:#fff9c4,stroke:#fbc02d,color:#000,stroke-width:2px;
    classDef sec fill:#ffcdd2,stroke:#c62828,color:#000,stroke-width:2px;

    class root root;
    class P1,P2,P3 perf;
    class D1,D2,D3 avail;
    class M1,M2,M3 maint;
    class S1,S2,S3 sec;

---

## 3. Requisitos

### 3.1 Requisitos Funcionais

```mermaid
graph TB
    subgraph "RF - Autenticação"
        RF01[RF01: Registrar administrador]
        RF02[RF02: Autenticar usuário]
        RF03[RF03: Gerenciar sessão JWT]
    end

    subgraph "RF - Pesquisas"
        RF04[RF04: Criar pesquisa]
        RF05[RF05: Editar pesquisa]
        RF06[RF06: Excluir pesquisa]
        RF07[RF07: Listar pesquisas do usuário]
    end

    subgraph "RF - Perguntas"
        RF08[RF08: Adicionar perguntas]
        RF09[RF09: Definir opções de resposta]
        RF10[RF10: Ordenar perguntas]
    end

    subgraph "RF - Respostas"
        RF11[RF11: Exibir pesquisa pública]
        RF12[RF12: Submeter resposta]
        RF13[RF13: Validar respostas obrigatórias]
        RF14[RF14: Prevenir duplicatas por IP]
    end

    subgraph "RF - Resultados"
        RF15[RF15: Calcular percentuais]
        RF16[RF16: Exibir contagem por opção]
        RF17[RF17: Dashboard sumarizado]
    end

    %% Styling
    classDef auth fill:#e1bee7,stroke:#6a1b9a,color:#000,stroke-width:2px;
    classDef surv fill:#bbdefb,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef quest fill:#c8e6c9,stroke:#2e7d32,color:#000,stroke-width:2px;
    classDef resp fill:#ffe0b2,stroke:#ef6c00,color:#000,stroke-width:2px;
    classDef resu fill:#fff9c4,stroke:#fbc02d,color:#000,stroke-width:2px;

    class RF01,RF02,RF03 auth;
    class RF04,RF05,RF06,RF07 surv;
    class RF08,RF09,RF10 quest;
    class RF11,RF12,RF13,RF14 resp;
    class RF15,RF16,RF17 resu;
```

### 3.2 Requisitos Não-Funcionais

| Categoria | Requisito | Especificação |
|-----------|-----------|---------------|
| **Performance** | RNF01 | Tempo de resposta < 200ms (P95) |
| **Performance** | RNF02 | Suportar 10.000 req/s |
| **Escalabilidade** | RNF03 | Escalar horizontalmente |
| **Disponibilidade** | RNF04 | 99.9% uptime |
| **Segurança** | RNF05 | Autenticação JWT |
| **Segurança** | RNF06 | Rate limiting por IP |
| **Usabilidade** | RNF07 | Responsivo mobile-first |
| **Manutenibilidade** | RNF08 | Cobertura de testes > 70% |

---

## 4. Stakeholders

```mermaid
graph TB
    subgraph "Stakeholders Primários"
        R["🗳️ Respondentes<br/>Cidadãos que respondem pesquisas"]
        A["👔 Administradores<br/>Gestores de pesquisas"]
    end

    subgraph "Stakeholders Secundários"
        D["💻 Desenvolvedores<br/>Time de 5 programadores .NET"]
        S["📊 Startup<br/>Donos do negócio"]
    end

    subgraph "Stakeholders Externos"
        SM["📱 Redes Sociais<br/>Canais de divulgação"]
        E["📧 Serviço de Email<br/>Notificações"]
    end

    R -->|Usa| Sistema((PublicPolls))
    A -->|Gerencia| Sistema
    D -->|Desenvolve| Sistema
    S -->|Financia| Sistema
    SM -.->|Divulga| R
    Sistema -.->|Notifica| E

    classDef users fill:#e8f5e9,stroke:#1b5e20,color:#000,stroke-width:2px;
    classDef system fill:#e3f2fd,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef ext fill:#f5f5f5,stroke:#616161,color:#000,stroke-width:2px,stroke-dasharray: 5 5;

    class R,A,D,S users;
    class Sistema system;
    class SM,E ext;
```

---

## 5. Restrições do Projeto

### 5.1 Restrições Tecnológicas

> ⚠️ **IMPORTANTE**: Por questões contratuais, a solução DEVE ser desenvolvida utilizando componentes do .NET Framework.

| Restrição | Descrição |
|-----------|-----------|
| Linguagem | C# (obrigatório) |
| Framework Backend | ASP.NET Core (obrigatório) |
| Expertise do Time | 5 desenvolvedores .NET |
| Prazo | 4 semanas até as eleições |

### 5.2 Restrições de Negócio

```mermaid
graph LR
    subgraph "📅 Cronograma do Projeto"
        W1["Semana 1<br/>Setup + Domain + Infra"]
        W2["Semana 2<br/>App Services + API + Swagger"]
        W3["Semana 3<br/>Frontend Blazor + Pages"]
        W4["Semana 4<br/>Testes + Docs + Deploy"]
    end

    W1 --> W2 --> W3 --> W4

    %% Styling
    classDef time fill:#e3f2fd,stroke:#1565c0,color:#000,stroke-width:2px;
    class W1,W2,W3,W4 time;
```

---

## 6. Escopo

### 6.1 Incluído no Escopo (MVP)

✅ Autenticação de administradores (JWT)
✅ CRUD completo de pesquisas
✅ Perguntas de múltipla escolha
✅ Página pública de resposta
✅ Resultados sumarizados
✅ Cache Redis
✅ Documentação Swagger

### 6.2 Fora do Escopo (Versões Futuras)

❌ Perguntas abertas (texto livre)
❌ Upload de imagens nas perguntas
❌ Exportação de relatórios (Excel/PDF)
❌ Multi-tenancy
❌ Internacionalização (i18n)
❌ Integração com redes sociais (OAuth)

---

## 7. Premissas e Dependências

### 7.1 Premissas

| # | Premissa |
|---|----------|
| P1 | O time possui expertise em .NET/C# |
| P2 | Docker está disponível para desenvolvimento |
| P3 | PostgreSQL e Redis serão usados localmente |
| P4 | Não há requisitos de compliance específicos |

### 7.2 Dependências

```mermaid
graph LR
    subgraph "Dependências Internas"
        D1[.NET 8 SDK]
        D2[Visual Studio/VS Code]
        D3[Docker Desktop]
    end

    subgraph "Dependências Externas"
        D4[PostgreSQL 15]
        D5[Redis 7]
        D6[NuGet Packages]
    end

    D1 --> Sistema((PublicPolls))
    D2 --> Sistema
    D3 --> Sistema
    D4 --> Sistema
    D5 --> Sistema
    D6 --> Sistema

    classDef system fill:#e3f2fd,stroke:#0d47a1,color:#000,stroke-width:2px;
    classDef dep fill:#fff9c4,stroke:#fbc02d,color:#000,stroke-width:2px;

    class Sistema system;
    class D1,D2,D3,D4,D5,D6 dep;
```

---

## 8. Riscos Identificados

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Prazo apertado | Alta | Crítico | MVP mínimo, sem over-engineering |
| Alta carga simultânea | Média | Alto | Cache Redis, otimização de queries |
| Vulnerabilidades de segurança | Média | Alto | Rate limiting, validações rigorosas |
| Indisponibilidade em pico | Baixa | Crítico | Arquitetura escalável horizontalmente |

---

## Próximo Documento

➡️ [Arquitetura C4](02-arquitetura-c4.md)
