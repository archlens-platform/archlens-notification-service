using System.Net;
using FluentAssertions;
using Reqnroll;

namespace ArchLens.Notification.Tests.BDD.StepDefinitions;

[Binding]
public sealed class CommonSteps(ScenarioContext scenarioContext)
{
    private HttpClient Client => (HttpClient)scenarioContext["HttpClient"];
    private HttpResponseMessage Response
    {
        get => (HttpResponseMessage)scenarioContext["Response"];
        set => scenarioContext["Response"] = value;
    }

    [Given(@"que a API de Notificacoes esta em execucao")]
    public void DadoQueAApiDeNotificacoesEstaEmExecucao()
    {
        Client.Should().NotBeNull("a API deve estar em execucao");
    }

    [When(@"eu faco uma requisicao GET para ""(.*)""")]
    public async Task QuandoEuFacoUmaRequisicaoGetPara(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

        if (scenarioContext.ContainsKey("CustomHeaders"))
        {
            var headers = (Dictionary<string, string>)scenarioContext["CustomHeaders"];
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        Response = await Client.SendAsync(request);
    }

    [Then(@"o status da resposta deve ser (.*)")]
    public void EntaoOStatusDaRespostaDeveSer(int statusCode)
    {
        ((int)Response.StatusCode).Should().Be(statusCode);
    }

    [Then(@"o corpo da resposta deve conter ""(.*)""")]
    public async Task EntaoOCorpoDaRespostaDeveConter(string texto)
    {
        var body = await Response.Content.ReadAsStringAsync();
        body.Should().Contain(texto);
    }

    [Then(@"o content-type da resposta deve conter ""(.*)""")]
    public void EntaoOContentTypeDaRespostaDeveConter(string contentType)
    {
        Response.Content.Headers.ContentType?.ToString().Should().Contain(contentType);
    }
}
