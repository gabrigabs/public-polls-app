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

    style E fill:#4caf50,color:#fff
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

```mermaid
mindmap
  root((Objetivos<br/>Técnicos))
    Performance
      Response time < 200ms
      Throughput > 10k req/s
      Cache hit ratio > 80%
    Disponibilidade
      Uptime 99.9%
      Zero downtime deploy
      Failover automático
    Manutenibilidade
      Clean Architecture
      Testes automatizados
      Documentação completa
    Segurança
      JWT Authentication
      Rate Limiting
      Input Validation
```

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
timeline
    title Cronograma do Projeto
    
    Semana 1 : Setup Projeto
             : Domain Layer
             : Infrastructure Layer
    
    Semana 2 : Application Services
             : API Controllers
             : Swagger Documentation
    
    Semana 3 : Frontend Blazor
             : Páginas de Resposta
             : Dashboard Admin
    
    Semana 4 : Testes
             : Documentação
             : Deploy
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
