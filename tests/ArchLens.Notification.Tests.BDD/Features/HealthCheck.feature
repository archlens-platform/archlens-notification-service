# language: pt-BR
Funcionalidade: Health Check da API de Notificacoes
  Como um operador de infraestrutura
  Quero verificar o estado de saude da API de Notificacoes
  Para garantir que o servico esta funcionando corretamente

  Cenario: Endpoint de health check deve estar acessivel
    Dado que a API de Notificacoes esta em execucao
    Quando eu faco uma requisicao GET para "/health"
    Entao o status da resposta deve ser 200

  Cenario: Health check deve retornar status saudavel
    Dado que a API de Notificacoes esta em execucao
    Quando eu faco uma requisicao GET para "/health"
    Entao o status da resposta deve ser 200
    E o corpo da resposta deve conter "Healthy"

  Cenario: Health check deve responder com content-type correto
    Dado que a API de Notificacoes esta em execucao
    Quando eu faco uma requisicao GET para "/health"
    Entao o status da resposta deve ser 200
    E o content-type da resposta deve conter "text/plain"
