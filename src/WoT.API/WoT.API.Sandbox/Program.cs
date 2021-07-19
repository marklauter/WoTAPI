using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using WoT.API.Sandbox.Models;

namespace WoT.API.Sandbox
{
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
        private static readonly int ClanId = 1000057667; // gatt
        private static readonly Regions Region = Sandbox.Regions.na;
        private static readonly string PublicApiHost = $"api.worldoftanks.{Regions[(int)Region]}/wot";
        private static readonly string InGameApiHost = "wgsh-wotus.wargaming.net/game_api";
        private static readonly string ClanRatingsPath = "clanratings/clans/";
        private static readonly string ClanJournalPath = "clan_journal/battles";
        private static readonly string ApplicationIdParamName = "application_id";
        private static readonly string ClanIdParamName = "clan_id";
        private static readonly string LimitParamName = "limit";
        private static readonly string UntilParamName = "until";
        private static readonly string BattleTypeParamName = "battle_type";
        private static readonly string TierParamName = "max_vehicle_level";
        private static readonly string BattleType = "SORTIE";
        private static readonly string GACookie = "_ga=GA1.2.177974393.1624070683";
        private static readonly HttpClient Client = new();

        private static async Task Main(string[] args)
        {
            if (args.Length == 1)
            {
                var accessToken = args[0];
                await GetClanJournalAsync(ClanId, accessToken);
            }
            else
            {
                await CreateSessionsAsync();
            }

            //await GetClanRatingsAsync(ClanId);
            //Console.WriteLine();
        }

        private static async Task CreateSessionsAsync()
        {
            var json = await File.ReadAllTextAsync("GattClanJournal.json");
            var battles = JsonConvert.DeserializeObject<List<BattleEvent>>(json);
            var battleSessions = battles
                .Select(b => b.StartedUtc.Date)
                .Distinct()
                .Select(d => new BattleSession(
                    d,
                    battles.Where(b => b.StartedUtc.Date == d).ToArray()))
                .ToArray();

            json = JsonConvert.SerializeObject(battleSessions);
            await File.WriteAllTextAsync("GattBattleSessions.json", json);
        }

        private static async Task AuthAsync()
        {
        }

        private static async Task GetClanRatingsAsync(int clanId)
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
        }

        private static async Task GetClanJournalAsync(int clanId, string accessToken)
        {
            // https://wgsh-wotus.wargaming.net/game_api/clan_journal/battles?limit=32&until=1626321599&battle_type=SORTIE&max_vehicle_level=8            

            var limit = 5000;
            var ssidCooke = $"wgsh_ssid={accessToken}";
            var query = HttpUtility.ParseQueryString(String.Empty);
            query[LimitParamName] = limit.ToString();
            query[UntilParamName] = 1626321599.ToString();
            query[BattleTypeParamName] = BattleType;
            //query[TierParamName] = 6.ToString();

            var uriBuilder = new UriBuilder("https", InGameApiHost)
            {
                Path = ClanJournalPath,
                Query = query.ToString(),
            };

            Console.WriteLine(uriBuilder.Uri);

            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
            request.Headers.Connection.Add("keep-alive");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Cookie", $"{GACookie}; {ssidCooke}");

            var response = await Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var battles = new List<Battle>();
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"bytes received: {content.Length}");
                var page = JsonConvert.DeserializeObject<JournalPage>(content);
                Console.WriteLine($"battles recieved: {page.meta.total_count}");

                battles.AddRange(page.data);
                var json = JsonConvert.SerializeObject(battles);
                await File.WriteAllTextAsync("GattClanJournal.json", json);
            }
            else
            {
                Console.WriteLine($"fail: {response.StatusCode}, {response.ReasonPhrase}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
        }
    }
}
