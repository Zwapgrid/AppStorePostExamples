using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using CreateAndValidateConnections.Models;
using Newtonsoft.Json;

namespace CreateAndValidateConnections
{
    public class ZwapgridConnector
    {
        private readonly ConnectorConfiguration _configuration;
        public ZwapgridConnector(ConnectorConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Create connection
        public Task<CreateConnectionOutput> CreateConnection(CreateConnectionInput input) =>
            MakeRequestAndGetResponse(input,
                constructRequest: (input) => GetCreateConnectionRequest(input),
                parseOutput: (response) => ParseCreateConnectionOutputAsync(response));

        private HttpRequestMessage GetCreateConnectionRequest(CreateConnectionInput input) =>
            ConstructGetRequest(input, "connections");

        private Task<CreateConnectionOutput> ParseCreateConnectionOutputAsync(HttpResponseMessage response) =>
            ParseOutputAsync<CreateConnectionOutput>(response);
        #endregion

        #region Validate connection
        public Task<ValidateConnectionOutput> ValidateConnection(ValidateConnectionInput input) =>
            MakeRequestAndGetResponse(input,
                constructRequest: (input) => GetValidateConnectionRequest(input),
                parseOutput: (response) => ParseValidateConnectionOutputAsync(response));

        private HttpRequestMessage GetValidateConnectionRequest(ValidateConnectionInput input) =>
            ConstructGetRequest(input, "connections/validate");

        private Task<ValidateConnectionOutput> ParseValidateConnectionOutputAsync(HttpResponseMessage response) =>
            ParseOutputAsync<ValidateConnectionOutput>(response);
        #endregion

        #region Helpers
        private async Task<TOutput> MakeRequestAndGetResponse<TInput, TOutput>(TInput input, 
            Func<TInput, HttpRequestMessage> constructRequest,
            Func<HttpResponseMessage, Task<TOutput>> parseOutput)
        {
            using var httpClient = new HttpClient();

            var request = constructRequest(input);

            var response = await httpClient.SendAsync(request);

            var outputParsed = await parseOutput(response);

            return outputParsed;
        }

        private HttpRequestMessage ConstructGetRequest(object input, string endpoint)
        {
            var postJson = JsonConvert.SerializeObject(input);
            var baseUrl = $"{_configuration.ApiUrl.EnsureEndsWith("/")}api/v1";
            var requestUrl = $"{baseUrl}/{endpoint}";

            var request = new HttpRequestMessage(
               HttpMethod.Post,
               requestUrl)
            {
                Content = new StringContent(postJson, Encoding.UTF8, MediaTypeNames.Application.Json),
            };

            request.Headers.Add("Authorization", $"Partner {_configuration.PartnerToken}");

            return request;
        }

        private async Task<TOutput> ParseOutputAsync<TOutput>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var exceptionString = $"StatusCode: {response.StatusCode}; Message: {response.ReasonPhrase}";
                var exception = new Exception(exceptionString);
                exception.Data.Add("StatusCode", response.StatusCode);
                exception.Data.Add("Message", response.ReasonPhrase);
                exception.Data.Add("Content", responseContent);
                throw exception;
            }

            var responseObject = JsonConvert.DeserializeObject<ZgApiResponse<TOutput>>(responseContent);

            return responseObject.Result;
        }
        #endregion
    }
}
