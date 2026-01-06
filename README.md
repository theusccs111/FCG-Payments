# FCG-Payments

## 📋 Introdução

**FCG-Payments** é um microserviço responsável pelo processamento, gerenciamento e orquestração de pagamentos da plataforma FCG. Este serviço integra-se com provedores de pagamento, processa transações, gerencia cobranças e envia notificações de pagamento para toda a plataforma via mensageria assíncrona.

## 🎯 Objetivos

- Processar pagamentos de pedidos de jogos
- Gerenciar múltiplos provedores de pagamento
- Orquestrar fluxo de pagamento (criação, validação, confirmação)
- Notificar outros serviços sobre status de pagamentos
- Manter histórico de transações
- Lidar com reembolsos e chargebacks

## 🏗️ Arquitetura

### Padrão Clean Architecture

O projeto segue a arquitetura em camadas com separação clara de responsabilidades:

```
FCG-Payments/
├── FCG-Payments.Api/             # Camada de Apresentação (Controllers, Endpoints)
├── FCG-Payments.Application/     # Camada de Aplicação (UseCases, DTOs, Services)
├── FCG-Payments.Domain/          # Camada de Domínio (Entidades, Interfaces)
├── FCG-Payments.Infrastructure/  # Camada de Infraestrutura (BD, Externos)
└── FCG-Payments-WorkerService/   # Processador de Mensagens + Agendador
```

### Fluxo de Dados

```
Cliente HTTP
    ↓
Controllers (FCG-Payments.Api)
    ↓
Application Services (Orquestração de Pagamento)
    ↓
Providers de Pagamento (Stripe, PayPal, etc.)
    ↓
Domain/Repository Pattern (Dados)
    ↓
MongoDB + Azure Service Bus
```

## 🔧 Stack Tecnológico

- **Framework**: ASP.NET Core 8.0
- **Autenticação**: JWT Bearer
- **Banco de Dados**: MongoDB 5.0+
- **Persistência**: Entity Framework Core
- **Mensageria**: Azure Service Bus
- **Processamento**: Azure Durable Functions / Worker Service
- **API Documentation**: Swagger/OpenAPI
- **Provedores de Pagamento**: Stripe, PayPal (integrável)
- **Docker**: Containerização
- **CI/CD**: Azure Pipelines

## 📨 Microserviços e Mensageria

### Arquitetura de Mensageria

**FCG-Payments** é o hub central de processamento de pagamentos:

```
FCG-Games/Users/Libraries
    ↓
[Solicita Pagamento]
    ↓
FCG-Payments.Api
    ↓
[Valida e Processa]
    ↓
Azure Service Bus Topic: PaymentEvents
    ↓
┌─────────────────────────────┐
├─ FCG-Games.Consumer        │
├─ FCG-Libraries.Consumer    │
└─ FCG-Users.Consumer        │
```

### Comunicação com Provedores

```
FCG-Payments.Api
    ↓
PaymentProviderService
    ↓
Stripe API / PayPal API / Outros
    ↓
[Webhook] → FCG-Payments.Api
    ↓
Processa Confirmação
    ↓
Publica Evento no Service Bus
```

### Azure Service Bus - Eventos de Pagamento

#### Consumer Service (FCG-Payments-WorkerService)
- **Tipo**: Azure Durable Functions + Worker Service
- **Responsabilidade**: 
  - Monitora webhooks de provedores de pagamento
  - Processa confirmações e notificações
  - Reprocessa pagamentos falhados
  - Gerencia agendamentos de cobrança
- **Padrão**: Listen & Process + Scheduled Tasks
- **Eventos Consumidos**: 
  - `PaymentWebhookEvent`: Notificação de provedores
  - `PaymentRetryEvent`: Retry de pagamentos
  - `SubscriptionChargeEvent`: Cobranças recorrentes

#### Publisher Service
- **Localização**: `FCG.Shared.EventService.Publisher`
- **Função**: Publica eventos de status de pagamento
- **Eventos Publicados**:
  - `PaymentInitiatedEvent`: Pagamento iniciado
  - `PaymentProcessingEvent`: Pagamento em processamento
  - `PaymentCompletedEvent`: Pagamento confirmado ✅
  - `PaymentFailedEvent`: Pagamento falhou ❌
  - `PaymentRefundedEvent`: Pagamento reembolsado
  - `SubscriptionActivatedEvent`: Assinatura ativada

### Fluxo Completo de Pagamento

```
1. FCG-Games.Api
   └─ POST /api/orders/{orderId}/payment
      (Usuário clica em "Pagar")

2. FCG-Payments.Api
   └─ POST /api/payments
      (Cria transação de pagamento)

3. PaymentService
   ├─ Valida dados da transação
   ├─ Chama provider (ex: Stripe)
   └─ Retorna status inicial

4. FCG-Payments.Api
   ├─ Retorna URL de pagamento ou status
   └─ Armazena transação em MongoDB

5. Usuário completa pagamento no provider

6. Provider envia Webhook
   └─ FCG-Payments.Api recebe confirmação

7. FCG-Payments-WorkerService
   ├─ Processa webhook
   ├─ Atualiza status em MongoDB
   └─ Publica PaymentCompletedEvent

8. Todos os consumers recebem evento
   ├─ FCG-Games.Consumer: Desbloqueando acesso ao jogo
   ├─ FCG-Libraries.Consumer: Adicionando à biblioteca
   └─ FCG-Users.Consumer: Atualizando histórico
```

## 📁 Estrutura do Projeto

### FCG-Payments.Api
- **Program.cs**: Configuração do host e injeção de dependências
- **Controllers/**: Endpoints HTTP
  - `PaymentController.cs`: Criar e consultar pagamentos
  - `WebhookController.cs`: Receber webhooks de provedores
  - `RefundController.cs`: Processar reembolsos
- **ApimAuthenticationHandler.cs**: Middleware de autenticação JWT

### FCG-Payments.Application
- **Services/**: Lógica de negócios
  - `PaymentService.cs`: Orquestração de pagamentos
  - `PaymentProviderService.cs`: Abstração de provedores
  - `WebhookProcessorService.cs`: Processamento de webhooks
  - `RefundService.cs`: Lógica de reembolsos
- **DTOs/**: Data Transfer Objects
- **Validators/**: Validação de dados
- **Interfaces/**: Contratos de serviços
  - `IPaymentProvider.cs`: Interface para provedores

### FCG-Payments.Domain
- **Entities/**: Modelos de domínio
  - `Payment.cs`: Transação de pagamento
  - `Transaction.cs`: Histórico de transações
  - `Refund.cs`: Reembolso
- **Interfaces/**: Contratos de repositório
- **Enums/**: Enumerações
  - `PaymentStatus.cs`: Pending, Processing, Completed, Failed, Refunded
  - `PaymentMethod.cs`: CreditCard, Paypal, etc.

### FCG-Payments.Infrastructure
- **Context/**: DbContext do Entity Framework
- **Repositories/**: Implementação de acesso a dados
- **Services/**: Serviços de infraestrutura
- **Providers/**: Implementações de provedores
  - `StripePaymentProvider.cs`
  - `PayPalPaymentProvider.cs`
  - `AbstractPaymentProvider.cs`: Classe base

### FCG-Payments-WorkerService
- **Program.cs**: Configuração do Worker Service
- **Worker.cs**: Lógica principal
- **WebhookProcessor.cs**: Processa webhooks
- **ScheduledTasks.cs**: Tarefas agendadas
- **DependencyInjection.cs**: Setup de DI

## 🚀 Como Executar

### Pré-requisitos
- .NET 8.0 SDK
- MongoDB rodando (local ou cloud)
- Azure Service Bus configurado
- Contas em provedores de pagamento (Stripe, PayPal, etc.)
- Docker (opcional)

### Desenvolvimento Local

1. **Clonar o repositório**
   ```bash
   git clone https://github.com/theusccs111/FCG-Payments.git
   cd FCG-Payments
   ```

2. **Configurar appsettings.json**
   ```json
   {
     "ConnectionStrings": {
       "MongoDB": "mongodb://localhost:27017/fcg-payments"
     },
     "PaymentProviders": {
       "Stripe": {
         "SecretKey": "sk_test_xxx",
         "PublicKey": "pk_test_xxx",
         "WebhookSecret": "whsec_xxx"
       },
       "PayPal": {
         "ClientId": "xxx",
         "ClientSecret": "xxx"
       }
     },
     "AzureServiceBus": {
       "ConnectionString": "your-service-bus-connection-string"
     }
   }
   ```

3. **Restaurar dependências e executar API**
   ```bash
   dotnet restore
   dotnet run --project FCG-Payments.Api
   ```

4. **Executar Worker Service**
   ```bash
   dotnet run --project FCG-Payments-WorkerService
   ```

### Docker

```bash
docker-compose up --build
```

## 🔐 Autenticação e Segurança

- **Tipo**: JWT Bearer Token
- **Issuer**: Serviço FCG-Users
- **Validação**: ApimAuthenticationHandler
- **HTTPS**: Obrigatório em produção
- **PCI Compliance**: Dados sensíveis não são armazenados

### Boas Práticas

- Nunca armazenar números de cartão completos
- Usar tokenização de provedores
- Validar IPs de webhooks
- Verificar assinatura de webhooks
- Usar rate limiting para endpoints de pagamento

## 📚 Documentação de API

Acesse o Swagger em: `http://localhost/swagger/index.html`

### Principais Endpoints

**Pagamentos**
- `POST /api/payments` - Criar novo pagamento
- `GET /api/payments/{paymentId}` - Obter status
- `GET /api/payments` - Listar pagamentos do usuário

**Webhooks**
- `POST /api/webhooks/stripe` - Receber eventos Stripe
- `POST /api/webhooks/paypal` - Receber eventos PayPal

**Reembolsos**
- `POST /api/payments/{paymentId}/refund` - Processar reembolso
- `GET /api/refunds/{refundId}` - Obter status reembolso
