using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Coflnet.Sky.Commands.Shared;

public interface IProfileClient
{
    ProfileClient.ForgeData GetForgeData(string playerId, string profile);
}

public class ProfileClient : IProfileClient
{
    private RestClient profileClient = null;
    public ProfileClient(IConfiguration config) =>
                profileClient = new RestClient(config["PROFILE_BASE_URL"]);
    
    
    public ForgeData GetForgeData(string playerId, string profile)
    {
        var request = new RestRequest($"api/profile/{playerId}/{profile}/data/forge", Method.Get);
        var response = profileClient.Execute<ForgeData>(request);
        return response.Data;
    }

    public class ForgeData
    {
        public int HotMLevel { get; set; }
        public float QuickForgeSpeed { get; set; }
        public Dictionary<string, int> CollectionLevels { get; set; }
    }
}

