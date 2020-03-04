using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConstructUrl
{
    public class Program
    {
        public static readonly string Version = typeof(Program).Assembly.GetName().Version.ToString();

        public const string ZwapgridConnectConfigurationFile = "connectConfig.json";
        public const string UrlParametersFile = "urlParameters.json";

        public static async Task Main(string[] args)
        {
            WriteHeader($"Zwapgrid url constructor. Version:{Version}");

            try
            {
                var configuration = await GetConfigurationAsync();
                var urlParametersData = await GetUrlParametersData();

                WriteHeader("Url construction ...");

                var zwapstoreUrl = ConstructZwapstoreUrl(configuration, urlParametersData);

                WriteHeader(
                    "Url constructed SUCCESSFULLY",
                    "Zwapstore url:",
                    zwapstoreUrl
                    );
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }

            WriteMessage("Press any key ...");

            _ = Console.ReadKey();
        }

        private static async Task<ConnectorConfiguration> GetConfigurationAsync()
        {
            WriteMessage("Reading connect configuration");

            using var file = File.OpenText(GetFilePath(ZwapgridConnectConfigurationFile));
            var contentStr = await file.ReadToEndAsync();
            var connectorConfiguration = JsonConvert.DeserializeObject<ConnectorConfiguration>(contentStr);

            if (string.IsNullOrEmpty(connectorConfiguration.AppUrl))
                throw new Exception("AppUrl is missing in connector configuration");
            if (string.IsNullOrEmpty(connectorConfiguration.PartnerToken))
                throw new Exception("PartnerToken is missing in connector configuration");

            WriteMessage($"Connect configuration read. Url: {connectorConfiguration.AppUrl}");

            return connectorConfiguration;
        }

        private static async Task<Dictionary<string, string>> GetUrlParametersData()
        {
            WriteMessage("Reading url parameters");

            using var file = File.OpenText(GetFilePath(UrlParametersFile));
            var contentStr = await file.ReadToEndAsync();
            var urlParametersObj = JObject.Parse(contentStr);

            // TODO json schema validation

            var paramsDict = new Dictionary<string, string>();

            ParseJObject(urlParametersObj, paramsDict);

            if (paramsDict.Count == 0)
                throw new Exception("Url parameters config contains no parameters");

            WriteMessage("Url parameters read");

            return paramsDict;
        }

        private static void ParseJObject(JObject jObject, Dictionary<string, string> dict, string prefix = "")
        {
            foreach (var jItem in jObject.Children())
            {
                if (!(jItem is JProperty jProp))
                    throw new Exception($"Json file has invalid format. Token type {jItem.Type} is not supported");

                if (jProp.Value == null)
                    throw new Exception($"Json file has invalid format. Token with empty value is not supported");

                switch (jProp.Value.Type)
                {
                    case JTokenType.String:
                        {
                            var propName = $"{prefix}{jProp.Name}";
                            dict.Add(propName, jProp.Value.ToString());
                            break;
                        }
                    case JTokenType.Object:
                        {
                            var newPrefix = prefix + jProp.Name + ".";
                            ParseJObject(jProp.Value as JObject, dict, newPrefix);
                            break;
                        }
                    default:
                        throw new Exception($"Json file has invalid format. Token type {jProp.Value.Type} is not supported");
                }
            }
        }

        private static string GetFilePath(string fileName) => Path.Combine(Directory.GetCurrentDirectory(), fileName);

        private static string ConstructZwapstoreUrl(ConnectorConfiguration configuration, Dictionary<string, string> keyValuePairs)
        {
            var baseUrl = $"{configuration.AppUrl.EnsureEndsWith("/")}zwapstore" +
                $"?token={UrlEncode(configuration.PartnerToken)}";

            foreach (var parameterItem in keyValuePairs)
            {
                WriteMessage($"Applying parameter '{parameterItem.Key}' ...");
                baseUrl += $"&{parameterItem.Key}={UrlEncode(parameterItem.Value)}";
            }

            return baseUrl;
        }

        private static string UrlEncode(string input) => WebUtility.UrlEncode(input);

        #region Console helpers
        private static void WriteHeader(params string[] messages)
        {
            Console.WriteLine(new string('-', 20));
            foreach (var item in messages)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine(new string('-', 20));
        }

        private static void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }

        private static void WriteError(Exception ex)
        {
            WriteHeader(GetErrorLines(ex).ToArray());
        }

        private static IEnumerable<string> GetErrorLines(Exception ex)
        {
            yield return "Unexpected ERROR occurred";

            yield return $"Message: {ex.Message}";

            if (ex.Data == null || ex.Data.Count == 0)
                yield break;

            foreach (var exceptionDataItem in ex.Data.Cast<DictionaryEntry>())
            {
                yield return $"{exceptionDataItem.Key}:{exceptionDataItem.Value}";
            }
        }
        #endregion
    }
}
