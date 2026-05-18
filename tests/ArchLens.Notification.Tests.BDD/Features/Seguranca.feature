# language: pt-BR
Funcionalidade: Seguranca da API de Notificacoes
  Como um desenvolvedor
  Quero garantir que os headers de seguranca e correlacao estejam presentes
  Para proteger a aplicacao contra ataques comuns e rastrear requisicoes

  Cenario: Headers de seguranca devem estar presentes nas respostas
    Dado que a API de Notificacoes esta em execucao
    Quando eu faco uma requisicao GET para "/health"
    Entao a resposta deve conter o header "X-Content-Type-Options" com valor "nosniff"
    E a resposta deve conter o header "X-Frame-Options" com valor "DENY"
    E a resposta deve conter o header "X-XSS-Protection" com valor "0"
    E a resposta deve conter o header "Referrer-Policy" com valor "strict-origin-when-cross-origin"
    E a resposta deve conter o header "Cache-Control" com valor "no-store"

  Cenario: Header Permissions-Policy deve estar presente
    Dado que a API de Notificacoes esta em execucao
    Quando eu faco uma requisicao GET para "/health"
    Entao a resposta deve conter o header "Permissions-Policy" com valor "camera=(), microphone=(), geolocation=()"

  Cenario: Correlation ID deve ser gerado automaticamente quando nao enviado
    Dado que a API de Notificacoes esta em execucao
    Quando eu faco uma requisicao GET para "/health" sem correlation id
    Entao a resposta deve possuir o header "X-Correlation-Id"
    E o valor do header "X-Correlation-Id" deve ser um GUID valido

  Cenario: Correlation ID enviado pelo cliente deve ser preservado na resposta
    Dado que a API de Notificacoes esta em execucao
    E que eu envio o header "X-Correlation-Id" com valor "test-correlation-123"
    Quando eu faco uma requisicao GET para "/health"
    Entao a resposta deve conter o header "X-Correlation-Id" com valor "test-correlation-123"

  Cenario: Endpoint protegido deve exigir autenticacao
    Dado que a API de Notificacoes esta em execucao
    E que o usuario nao esta autenticado
    Quando eu faco uma requisicao GET para "/hubs/analysis/negotiate?negotiateVersion=1"
    Entao o status da resposta deve ser 401
