// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ConditionIsAlwaysTrueOrFalse

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using iSAMS_RestApi.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iSAMS_RestApi
{
    public static class Program
    {
        private const string Authority = Domain + "/auth";

        private const string Domain = "https://developerdemo.isams.cloud"; // <= your_host_here (without a trailing /)

        public const string RestApiClientId = "{your_client_id_here}"; // <= your_client_id_here

        public const string RestApiClientSecret = "{your_client_secret_here}"; // <= your_client_secret_here

        // Add required scopes, see https://developer.isams.com/docs/getting-started-api-authentication
        public const string RestApiScope = "restapi"; // restapi for the Public REST API, and apiv1 for the Batch API

        private const string TokenPath = "auth/connect/token";

        private const bool VerboseLogging = true;

        private static int _apiClientCounter;

        public static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                GetAllEmployees().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to complete: {ex.Message}");
            }
            finally
            {
                Console.WriteLine(
                    $"{_apiClientCounter} requests made to the REST API in {stopwatch.ElapsedMilliseconds / 1000} seconds.");
                stopwatch.Stop();
                Console.ReadKey();
            }
        }

        private static async Task GetAllEmployees()
        {
            TokenResponse tokenResponse;

            using (var httpClient = new HttpClient())
            {
                Console.WriteLine("Retrieving the discovery document...");
                var discoveryDocumentResponse = await httpClient.GetDiscoveryDocumentAsync(Authority);
                if (discoveryDocumentResponse.IsError)
                {
                    throw new AccessTokenException(
                        $"[{discoveryDocumentResponse.HttpStatusCode}] Error retrieving the discovery document [{discoveryDocumentResponse.Error}].");
                }

                Console.WriteLine("Retrieved the discovery document.");

                var authTokenUrl = $"{Domain.TrimEnd('/')}/{TokenPath.TrimStart('/')}";
                var apiClientCredentials = new ClientCredentialsTokenRequest();
                apiClientCredentials.Address = authTokenUrl;
                apiClientCredentials.ClientId = RestApiClientId;
                apiClientCredentials.ClientSecret = RestApiClientSecret;
                apiClientCredentials.Scope = RestApiScope;

                Console.WriteLine($"Authenticating {RestApiClientId}...");
                tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(apiClientCredentials);
                if (tokenResponse.IsError)
                {
                    throw new AccessTokenException(
                        $"[{tokenResponse.HttpStatusCode}] Error authenticating [{tokenResponse.Error}].");
                }

                Console.WriteLine("Authenticated successfully.");
            }

            using (var apiClient = new HttpClient())
            {
                apiClient.BaseAddress = new Uri($"{Domain.TrimEnd('/')}/api");
                apiClient.DefaultRequestHeaders.Clear();

                apiClient.SetBearerToken(tokenResponse.AccessToken);
                Console.WriteLine("Set the bearer token for all API requests.");

                Console.WriteLine("Retrieving the HR data...");
                SetRequestHeaders(apiClient, "application/hal+json");

                var apiPath = $"{Domain.TrimEnd('/')}/api/humanresources/absencecategories";
                if (VerboseLogging) Console.WriteLine("Retrieving absence categories...");
                var response = await InternalGetAsync(apiClient, apiPath);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RestApiException(apiPath,
                        $"[{response.StatusCode}] Error retrieving the absence categories [{response.ReasonPhrase}].");
                }

                if (VerboseLogging) Console.WriteLine("Retrieved absence categories.");

                apiPath = $"{Domain.TrimEnd('/')}/api/humanresources/contracttypes";
                if (VerboseLogging) Console.WriteLine("Retrieving contract types...");
                response = await InternalGetAsync(apiClient, apiPath);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RestApiException(apiPath,
                        $"[{response.StatusCode}] Error retrieving the contract types [{response.ReasonPhrase}].");
                }

                if (VerboseLogging) Console.WriteLine("Retrieved contract types.");

                apiPath = $"{Domain.TrimEnd('/')}/api/humanresources/qualificationtypes";
                if (VerboseLogging) Console.WriteLine("Retrieving qualification types...");
                response = await InternalGetAsync(apiClient, apiPath);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RestApiException(apiPath,
                        $"[{response.StatusCode}] Error retrieving the qualification types [{response.ReasonPhrase}].");
                }

                if (VerboseLogging) Console.WriteLine("Retrieved qualification types.");

                apiPath = $"{Domain.TrimEnd('/')}/api/humanresources/qualificationsubjects";
                if (VerboseLogging) Console.WriteLine("Retrieving qualification subjects...");
                response = await InternalGetAsync(apiClient, apiPath);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RestApiException(apiPath,
                        $"[{response.StatusCode}] Error retrieving the qualification subjects [{response.ReasonPhrase}].");
                }

                if (VerboseLogging) Console.WriteLine("Retrieved qualification subjects.");

                apiPath = $"{Domain.TrimEnd('/')}/api/humanresources/qualificationlevels";
                if (VerboseLogging) Console.WriteLine("Retrieving qualification levels...");
                response = await InternalGetAsync(apiClient, apiPath);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RestApiException(apiPath,
                        $"[{response.StatusCode}] Error retrieving the qualification levels [{response.ReasonPhrase}].");
                }

                if (VerboseLogging) Console.WriteLine("Retrieved qualification levels.");

                apiPath = $"{Domain.TrimEnd('/')}/api/humanresources/disclosureandprobation/disclosuretypes";
                if (VerboseLogging) Console.WriteLine("Retrieving disclosure types...");
                response = await InternalGetAsync(apiClient, apiPath);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RestApiException(apiPath,
                        $"[{response.StatusCode}] Error retrieving the disclosure types [{response.ReasonPhrase}].");
                }

                if (VerboseLogging) Console.WriteLine("Retrieved disclosure types.");

                apiPath = $"{Domain.TrimEnd('/')}/api/humanresources/disclosureandprobation/righttoworkchecktypes";
                if (VerboseLogging) Console.WriteLine("Retrieving right to work check types...");
                response = await InternalGetAsync(apiClient, apiPath);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RestApiException(apiPath,
                        $"[{response.StatusCode}] Error retrieving the right to work check types [{response.ReasonPhrase}].");
                }

                if (VerboseLogging) Console.WriteLine("Retrieved right to work check types.");

                apiPath = $"{Domain.TrimEnd('/')}/api/humanresources/employeelevels";
                if (VerboseLogging) Console.WriteLine("Retrieving employee levels...");
                response = await InternalGetAsync(apiClient, apiPath);
                if (!response.IsSuccessStatusCode)
                {
                    throw new RestApiException(apiPath,
                        $"[{response.StatusCode}] Error retrieving the employee levels [{response.ReasonPhrase}].");
                }

                if (VerboseLogging) Console.WriteLine("Retrieved employee levels.");

                Console.WriteLine("Retrieved the HR data.");

                var pageNumber = 0;
                var pageSize = 100;

                Console.WriteLine("Retrieving the employees...");
                SetRequestHeaders(apiClient, "application/hal+json");

                var employeeIds = new List<long>();
                JObject halLinkNext = null;

                do
                {
                    var halLinkNextHref = (JValue) halLinkNext?["href"];

                    pageNumber++;
                    apiPath = halLinkNextHref != null
                        ? halLinkNextHref.ToString(CultureInfo.InvariantCulture)
                        : $"{Domain.TrimEnd('/')}/api/humanresources/employees?page={pageNumber}&pageSize={pageSize}";

                    if (VerboseLogging) Console.WriteLine($"Retrieving page #{pageNumber} of employees...");
                    var employeesResponse = await InternalGetAsync(apiClient, apiPath);
                    if (!employeesResponse.IsSuccessStatusCode)
                    {
                        throw new RestApiException(apiPath,
                            $"[{employeesResponse.StatusCode}] Error retrieving page #{pageNumber} of employees [{employeesResponse.ReasonPhrase}].");
                    }

                    var stringContent = await employeesResponse.Content.ReadAsStringAsync();
                    var jsonContent = (JObject) JsonConvert.DeserializeObject(stringContent);
                    var employees = (JArray) jsonContent["employees"];
                    employeeIds.AddRange(employees.Select(x => Convert.ToInt64(x["id"])));

                    if (VerboseLogging) Console.WriteLine($"Retrieved page #{pageNumber} employees out of {jsonContent.GetValue("totalPages")} pages.");

                    var halLinks = (JObject) jsonContent["_links"];
                    halLinkNext = (JObject) halLinks["next"];
                } while (halLinkNext != null);

                Console.WriteLine($"Retrieved {employeeIds.Count} employees.");

                Console.WriteLine($"Retrieving additional information for {employeeIds.Count} employees...");
                SetRequestHeaders(apiClient, "application/hal+json");

                var employeeCounter = 0;
                foreach (var employeeId in employeeIds)
                {
                    employeeCounter++;

                    apiPath =
                        $"{Domain.TrimEnd('/')}/api/humanresources/employees/{employeeId}/financialinformation";
                    if (VerboseLogging)
                        Console.WriteLine($"Retrieving financial information for employee #{employeeId}... {employeeCounter} of {employeeIds.Count} employees");
                    var financialInformationResponse = await InternalGetAsync(apiClient, apiPath);

                    if (!financialInformationResponse.IsSuccessStatusCode)
                    {
                        throw new RestApiException(apiPath,
                            $"[{financialInformationResponse.StatusCode}] Error retrieving financial information for employee #{employeeId} [{financialInformationResponse.ReasonPhrase}].");
                    }

                    if (VerboseLogging)
                        Console.WriteLine($"Retrieved financial information for employee #{employeeId}.");

                    apiPath =
                        $"{Domain.TrimEnd('/')}/api/humanresources/employees/{employeeId}/contracts";
                    if (VerboseLogging) Console.WriteLine($"Retrieving contracts for employee #{employeeId}... {employeeCounter} of {employeeIds.Count} employees");
                    var employeesResponse = await InternalGetAsync(apiClient, apiPath);

                    if (!employeesResponse.IsSuccessStatusCode)
                    {
                        throw new RestApiException(apiPath,
                            $"[{employeesResponse.StatusCode}] Error retrieving contracts for employee #{employeeId} [{employeesResponse.ReasonPhrase}].");
                    }

                    if (VerboseLogging) Console.WriteLine($"Retrieved contracts for employee #{employeeId}.");
                }

                Console.WriteLine($"Retrieved additional information for {employeeIds.Count} employees.");
            }
        }

        private static void SetRequestHeaders(HttpClient apiClient, string accept)
        {
            apiClient.DefaultRequestHeaders.Accept.Clear();
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
        }

        private static Task<HttpResponseMessage> InternalGetAsync(HttpClient apiClient, string apiPath)
        {
            _apiClientCounter++;
            return apiClient.GetAsync(apiPath);
        }
    }
}