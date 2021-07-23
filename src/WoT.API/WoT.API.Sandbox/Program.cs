using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WoT.API.Sandbox.Models;


namespace WoT.API.Sandbox
{
    public static class JsonReaderExtensions
    {
        public static IEnumerable<T> SelectTokensWithRegex<T>(
            this JsonReader jsonReader, Regex regex)
        {
            JsonSerializer serializer = new JsonSerializer();
            while (jsonReader.Read())
            {
                if (regex.IsMatch(jsonReader.Path)
                    && jsonReader.TokenType != JsonToken.PropertyName)
                {
                    yield return serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }

    internal class Program
    {
        private static readonly string[] Regions = new string[]
        {
            "ru",
            "eu",
            "com",
            "asia",
        };
        private static readonly string ApplicationId = Environment
            .GetEnvironmentVariable(
                "wot_api_applicationId",
                EnvironmentVariableTarget.Machine);
        //private static readonly int ClanId = 1000057667; // gatt
        private static readonly Regions Region = Sandbox.Regions.na;
        private static readonly string PublicApiHost = $"api.worldoftanks.{Regions[(int)Region]}/wot";
        private static readonly string InGameApiHost = "wgsh-wotus.wargaming.net/game_api";
        private static readonly string ClanRatingsPath = "clanratings/clans/";
        private static readonly string ClanJournalPath = "clan_journal/battles";
        private static readonly string ApplicationIdParamName = "application_id";
        private static readonly string ClanIdParamName = "clan_id";
        private static readonly string LimitParamName = "limit";
        //private static readonly string UntilParamName = "until";
        private static readonly string BattleTypeParamName = "battle_type";
        //private static readonly string TierParamName = "max_vehicle_level";
        //private static readonly string BattleType = "FORT_BATTLE";
        private static readonly string GACookie = "_ga=GA1.2.851061819.1625792015";
        private static readonly HttpClient Client = new();

        private static async Task Main(string[] args)
        {
            if (args.Length == 3)
            {
                var accessToken = args[0];
                var limit = args[1];
                var runauth = args[2];
                //var until = args[2];
                //await GetClanJournalAsync(accessToken, limit, until);
                //await DeserializeFromStreamCallAsync(accessToken, limit);
                await AuthenticationRoutineAsync(runauth);
            }

            //await CreateSessionsAsync();

            //await GetClanRatingsAsync(ClanId);
            //Console.WriteLine();
        }
        

        /*private static async Task CreateSessionsAsync()
        {
            var json = await File.ReadAllTextAsync("GattClanJournal.json");
            var battles = JsonConvert.DeserializeObject<List<BattleEvent>>(json);
            var battleSessions = battles
                .Select(b => b.StartedUtc.Date)
                .Distinct()
                .Select(d => new BattleSession(
                    d, 
                    battles.Where(b=> b.StartedUtc.Date == d).ToArray()))
                .ToArray();

            json = JsonConvert.SerializeObject(battleSessions);
            await File.WriteAllTextAsync("GattBattleSessions.json", json);
        }*/


        /*private static async Task GetClanRatingsAsync(int clanId)
        {
            //https://api.worldoftanks.com/wot/clanratings/clans/?application_id=XXX&clan_id=XXX

            var query = HttpUtility.ParseQueryString(String.Empty);
            query[ApplicationIdParamName] = ApplicationId;
            query[ClanIdParamName] = clanId.ToString();
            var queryString = query.ToString();

            var builder = new UriBuilder("https", PublicApiHost)
            {
                Query = queryString,
                Path = ClanRatingsPath,
            };
            Console.WriteLine(builder.Uri);

            var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
            var response = await Client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            else
            {
                Console.WriteLine($"fail: {response.StatusCode}, {response.ReasonPhrase}");
            }
        }*/

        private static async Task GetClanJournalAsync(string accessToken, string limit/*, string until*/)
        {
            // https://wgsh-wotus.wargaming.net/game_api/clan_journal/battles?limit=5000&until=1626658065  

            /*THE BELOW HANDLES SORTIES, WHICH ARE TIERS 6 AND 8*/

            //var limit = 5000;
            var ssidCooke = $"wgsh_ssid={accessToken}";
            var queryS = HttpUtility.ParseQueryString(String.Empty);
            queryS[LimitParamName] = limit.ToString();
            //queryS[BattleTypeParamName] = "SORTIE";
            //query[UntilParamName] = until.ToString();
            //query[TierParamName] = 6.ToString();

            var uriBuilderS = new UriBuilder("https", InGameApiHost)
            {
                Path = ClanJournalPath,
                Query = queryS.ToString(),
            };

            Console.WriteLine(uriBuilderS.Uri);

            var requestS = new HttpRequestMessage(HttpMethod.Get, uriBuilderS.Uri);
            requestS.Headers.Connection.Add("keep-alive");
            requestS.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            requestS.Headers.Add("Cookie", $"{GACookie}; {ssidCooke}");

            Console.WriteLine(requestS);

            var responseS = await Client.SendAsync(requestS);

            if (responseS.IsSuccessStatusCode)
            {
                var content = await responseS.Content.ReadAsStringAsync();
                Console.WriteLine($"bytes received: {content.Length}");
                var page = JsonConvert.DeserializeObject<JournalPage>(content);
                Console.WriteLine($"battles recieved: {page.meta.total_count}");

                var battles = new List<Battle>();
                battles.AddRange(page.data);
                var json = JsonConvert.SerializeObject(battles);
                await File.WriteAllTextAsync("GattClanJournal.json", json);
            }
            else
            {
                Console.WriteLine($"fail: {responseS.StatusCode}, {responseS.ReasonPhrase}");
                var content = await responseS.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }

            /*THE BELOW HANDLES FORT_BATTLES, WHICH ARE ADVANCES AT TIER 10*/

            /*var queryF = HttpUtility.ParseQueryString(String.Empty);
            queryF[LimitParamName] = limit.ToString();
            queryF[BattleTypeParamName] = "FORT_BATTLE";

            var uriBuilderF = new UriBuilder("https", InGameApiHost)
            {
                Path = ClanJournalPath,
                Query = queryF.ToString(),
            };

            Console.WriteLine(uriBuilderF.Uri);

            var requestF = new HttpRequestMessage(HttpMethod.Get, uriBuilderF.Uri);
            requestF.Headers.Connection.Add("keep-alive");
            requestF.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            requestF.Headers.Add("Cookie", $"{GACookie}; {ssidCooke}");

            Console.WriteLine(requestF);

            var responseF = await Client.SendAsync(requestF);

            if (responseF.IsSuccessStatusCode)
            {*/
            //THE BELOW IS THE WORKING CODE AS OF 7_21_2021 1343 MT
            /*
            var content = await responseF.Content.ReadAsStringAsync();
            Console.WriteLine($"bytes received: {content.Length}");
            AdvancesPage advancesPage = (AdvancesPage)JsonConvert.DeserializeObject(content, typeof(AdvancesPage));
            Console.WriteLine($"battles received: {advancesPage.meta.total_count}");

            var apconv = JsonConvert.SerializeObject(advancesPage.data);
            var apconvDS = JsonConvert.DeserializeObject(apconv);
            var apconvDS_S = apconvDS.ToString();
            var apconvSS = apconvDS_S.Substring(1, apconvDS_S.Length - 2);
            Data dataPage = JsonConvert.DeserializeObject<Data>(apconvSS);
            var dpconv = JsonConvert.SerializeObject(dataPage.rounds);
            await File.WriteAllTextAsync("rounds.json", dpconv);
            */

            //THE BELOW IS CODE IN PROGRESS, TRYING TO WRITE A STREAMER TO PROCESS ITERATIVE JSON BLOCKS
            /*
            var content = await responseF.Content.ReadAsStringAsync();
            Console.WriteLine($"bytes received: {content.Length}");
            //AdvancesPage advancesPage = (AdvancesPage)JsonConvert.DeserializeObject(content, typeof(AdvancesPage));
            //Console.WriteLine($"battles received: {advancesPage.meta.total_count}");

            //var apconv = JsonConvert.SerializeObject(advancesPage.data);
            //var apconvDS = JsonConvert.DeserializeObject(apconv);
            //var apconvDS_S = apconvDS.ToString();
            //var apconvSS = apconvDS_S.Substring(1, apconvDS_S.Length - 2);
            //Data dataPage = JsonConvert.DeserializeObject<Data>(apconvSS);
            //var dpconv = JsonConvert.SerializeObject(dataPage.rounds);

            var regex = new Regex(@"^\[\d+\]\.type");
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            using (MemoryStream stream = new MemoryStream(bytes))
            using (StreamReader sr = new StreamReader(stream))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                reader.SupportMultipleContent = true;

                var serializer = new JsonSerializer();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        AdvancesPage advancesPage = (AdvancesPage)JsonConvert.DeserializeObject(content, typeof(AdvancesPage));
                        var apconv = JsonConvert.SerializeObject(advancesPage.data);
                        Data dataPage = (Data)JsonConvert.DeserializeObject(apconv, typeof(Data));
                        var dpconv = JsonConvert.SerializeObject(dataPage.rounds);
                        Rounds output = serializer.Deserialize<Rounds>(dpconv);
                        Console.WriteLine(output.type);
                        Console.WriteLine(output.series_id);
                        Console.WriteLine(output.id);
                        Console.WriteLine(output.result);
                    }
                }
            }
            
            }

            else
            {
                Console.WriteLine($"fail: {responseF.StatusCode}, {responseF.ReasonPhrase}");
                var content = await responseF.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
        }
        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                var searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }
        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();

            return content;
        }
        public class ApiException : Exception
        {
            public int StatusCode { get; set; }

            public string Content { get; set; }
        }

        
        private static async Task<List<AdvancesPage>> DeserializeFromStreamCallAsync(string accessToken, string limit)
        {
            var queryF = HttpUtility.ParseQueryString(String.Empty);
            queryF[LimitParamName] = limit.ToString();
            queryF[BattleTypeParamName] = "FORT_BATTLE";
            var uriBuilderF = new UriBuilder("https", InGameApiHost)
            {
                Path = ClanJournalPath,
                Query = queryF.ToString(),
            };
            var ssidCooke = $"wgsh_ssid={accessToken}";
            
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilderF.Uri);
            request.Headers.Connection.Add("keep-alive");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Cookie", $"{GACookie}; {ssidCooke}");
            using (var response = await Client.SendAsync(request))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                    return DeserializeJsonFromStream<List<AdvancesPage>>(stream);

                var content = await StreamToStringAsync(stream);
                Console.WriteLine($"Data: {content}");
                throw new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content
                };
            }
            
        }*/
        }

        private static async Task AuthenticationRoutineAsync(string runauth)
        {
            var dorun = runauth;
            string authority = "wgcg-na.wargaming.net";
            string UAS = "Chrome/73.0.3683.75 WorldOfTanks/0.0.0.0 (en)";
            string origin = "https://wgsh-wotus-static.wgcdn.co";
            string referer = "https://wgsh-wotus-static.wgcdn.co";


            var uriBuilder = new UriBuilder()
            {
                Scheme = "https",
                Host = "wgcg-na.wargaming.net",
                Path = "/login/access_token/"
            };
            var auth = new HttpRequestMessage(HttpMethod.Options, uriBuilder.Uri);
            auth.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            auth.Headers.Add("user-agent", $"{UAS}");
            auth.Headers.Add("origin", $"{origin}");
            auth.Headers.Add("referer", $"{referer}");
            auth.Headers.Add("Access-Control-Request-Headers", "x-access-token");
            auth.Headers.Add("Access-Control-Request-Method", "GET");
            Console.WriteLine($"auth: {auth}");

            var uriBuilder1 = new UriBuilder("https", authority)
            {
                Path = "/game_api/account_info"
            };
            var auth1 = new HttpRequestMessage(HttpMethod.Get, uriBuilder1.Uri);
            auth1.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            auth1.Headers.Add("user-agent", $"{UAS}");
            auth1.Headers.Add("origin", $"{origin}");
            auth1.Headers.Add("referer", $"{referer}");

            var uriBuilder2 = new UriBuilder("https", authority)
            {
                Path = "/oauth/verify/token?access_token=WQF8JXJSJeEZxH2ifXCbAedeFBTDVS&periphery_id=303&spa_id=1024097263"
            };
            var auth2 = new HttpRequestMessage(HttpMethod.Get, uriBuilder2.Uri);
            auth2.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            auth2.Headers.Add("user-agent", $"{UAS}");
            auth2.Headers.Add("origin", $"{origin}");
            auth2.Headers.Add("referer", $"{referer}");

            if (dorun == "y")
            {
                var response = Client.Send(auth);
                Console.WriteLine($"Response: " + response.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var response1 = Client.Send(auth1);
                    Console.WriteLine($"Response1: " + response1.ToString());
                    if (response.IsSuccessStatusCode)
                    {
                        var response2 = Client.Send(auth2);
                        Console.WriteLine($"Response2: " + response2.ToString());
                    }
                }
            }
            else
            {
                Console.WriteLine("Not running auth routine. If you meant to auth, use y instead of something else.");
            }
        }
    }
}
