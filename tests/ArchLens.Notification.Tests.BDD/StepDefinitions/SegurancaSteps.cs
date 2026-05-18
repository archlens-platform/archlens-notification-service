using FluentAssertions;
using ArchLens.Notification.Tests.BDD.Hooks;
using Reqnroll;

namespace ArchLens.Notification.Tests.BDD.StepDefinitions;

[Binding]
public sealed class SegurancaSteps(ScenarioContext scenarioContext)
{
    private HttpResponseMessage Response => (HttpResponseMessage)scenarioContext["Response"];

    [Then(@"a resposta deve conter o header ""(.*)"" com valor ""(.*)""")]
    public void EntaoARespostaDeveConterOHeaderComValor(string headerName, string headerValue)
    {
        var found = Response.Headers.TryGetValues(headerName, out var values);
        if (!found)
        {
            // Also check content headers
            found = Response.Content.Headers.TryGetValues(headerName, out values);
        }
        found.Should().BeTrue($"o header '{headerName}' deve estar presente na resposta");
        values!.Should().Contain(v => v.Contains(headerValue));
    }

    [When(@"eu faco uma requisicao GET para ""(.*)"" sem correlation id")]
    public async Task QuandoEuFacoUmaRequisicaoGetParaSemCorrelationId(string endpoint)
    {
        var client = (HttpClient)scenarioContext["HttpClient"];
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        scenarioContext["Response"] = await client.SendAsync(request);
    }

    [Then(@"a resposta deve possuir o header ""(.*)""")]
    public void EntaoARespostaDevePossuirOHeader(string headerName)
    {
        Response.Headers.Contains(headerName).Should().BeTrue(
            $"o header '{headerName}' deve estar presente na resposta");
    }

    [Then(@"o valor do header ""(.*)"" deve ser um GUID valido")]
    public void EntaoOValorDoHeaderDeveSerUmGuidValido(string headerName)
    {
        var value = Response.Headers.GetValues(headerName).First();
        Guid.TryParse(value, out _).Should().BeTrue(
            $"o valor do header '{headerName}' deve ser um GUID valido, mas era '{value}'");
    }

    [Given(@"que eu envio o header ""(.*)"" com valor ""(.*)""")]
    public void DadoQueEuEnvioOHeaderComValor(string headerName, string headerValue)
    {
        if (!scenarioContext.ContainsKey("CustomHeaders"))
        {
            scenarioContext["CustomHeaders"] = new Dictionary<string, string>();
        }

        var headers = (Dictionary<string, string>)scenarioContext["CustomHeaders"];
        headers[headerName] = headerValue;
    }

    [Given(@"que o usuario nao esta autenticado")]
    public void DadoQueOUsuarioNaoEstaAutenticado()
    {
        BddTestAuthHandler.IsAuthenticated = false;
    }
}
