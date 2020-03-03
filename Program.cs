using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppStorePostExamples.Models;
using Newtonsoft.Json;

namespace AppStorePostExamples
{
    public class Program
    {
        public static readonly string Version = typeof(Program).Assembly.GetName().Version.ToString();

        public const string ZwapgridConnectConfigurationFile = "connectConfig.json";
        public const string ConnectionDataFile = "connectionData.json";

        public static async Task Main(string[] args)
        {
            WriteHeader($"Zwapgrid connector. Version:{Version}");

            try
            {
                var configuration = await GetConfigurationAsync();
                var createConnectionData = await GetCreateConnectionData();

                var createConnectionInput = new CreateConnectionInput
                {
                    PartnerToken = configuration.PartnerToken,
                    Connection = createConnectionData,
                };

                var connector = new ZwapgridConnector(configuration: configuration);

                WriteMessage("Creating connection ...");
                var createConnectionOutput = await connector.CreateConnection(createConnectionInput);
                WriteMessage($"Connection created. ConnectionId: {createConnectionOutput.Connection.Id}");

                var connectionIdEncrypted = GetConnectionIdEncrypted(createConnectionOutput.Connection.Id.Value,
                    publicKeyString: createConnectionOutput.PublicKey,
                    partnerToken: configuration.PartnerToken);
                WriteMessage("Encryption completed. Encrypted value:");
                WriteMessage(connectionIdEncrypted);

                var validateConnectionInput = new ValidateConnectionInput
                {
                    PartnerToken = configuration.PartnerToken,
                    Id = connectionIdEncrypted,
                };

                WriteMessage("Validating connection ...");
                var validateConnectionOutput = await connector.ValidateConnection(validateConnectionInput);

                if (validateConnectionOutput.Success)
                {
                    WriteHeader("Validation SUCCESS");
                }
                else
                {
                    WriteHeader(
                        "Validation FAILED",
                        validateConnectionOutput.Message);
                }

            }
            catch(Exception ex)
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

            if (string.IsNullOrEmpty(connectorConfiguration.ApiUrl))
                throw new Exception("ApiUrl is missing in connector configuration");
            if (string.IsNullOrEmpty(connectorConfiguration.PartnerToken))
                throw new Exception("PartnerToken is missing in connector configuration");

            WriteMessage($"Connect configuration read. Url: {connectorConfiguration.ApiUrl}");

            return connectorConfiguration;
        }

        private static async Task<Connection> GetCreateConnectionData()
        {
            WriteMessage("Reading connection data");

            using var file = File.OpenText(GetFilePath(ConnectionDataFile));
            var contentStr = await file.ReadToEndAsync();
            var connection = JsonConvert.DeserializeObject<Connection>(contentStr);

            if (string.IsNullOrEmpty(connection.Type))
                throw new Exception("ConnectionType is missing in connection data");

            WriteMessage($"Connection data read. Connection type: {connection.Type}");

            return connection;
        }

        private static string GetFilePath(string fileName) => Path.Combine(Directory.GetCurrentDirectory(), fileName);

        private static string GetConnectionIdEncrypted(int connectionId, string publicKeyString, string partnerToken)
        {
            var cryptor = new RSACryptor();

            var publicKey = cryptor.PEMStringToRSAKey(publicKeyString);

            var stringToEncrypt = $"{connectionId}||{partnerToken}";

            var stringEncrypted = cryptor.Encrypt(stringToEncrypt, publicKey);

            return stringEncrypted;
        }

        #region Console helpers
        private static void WriteHeader(params string[] messages)
        {
            Console.WriteLine(new string('-', 20));
            foreach(var item in messages)
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

            foreach(var exceptionDataItem in ex.Data.Cast<DictionaryEntry>())
            {
                yield return $"{exceptionDataItem.Key}:{exceptionDataItem.Value}";
            }
        }
        #endregion
    }
}
