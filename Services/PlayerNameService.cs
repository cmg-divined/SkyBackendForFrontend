using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coflnet.Sky.PlayerName.Client.Api;
using Coflnet.Sky.Commands.Shared;
using Microsoft.Extensions.Logging;

namespace Coflnet.Sky.PlayerName
{
    public class PlayerNameService
    {
        PlayerName.Client.Api.PlayerNameApi client;
        ILogger<PlayerNameService> logger;

        public PlayerNameService(PlayerNameApi client, ILogger<PlayerNameService> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<string> GetName(string uuid)
        {
            return (await client.PlayerNameNameUuidGetAsync(uuid))?.Trim('"');
        }
        public async Task<string> GetUuid(string name)
        {
            var playerUuid = (await client.PlayerNameUuidNameGetAsync(name))?.Trim('"');
            if (playerUuid == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        playerUuid = (await Coflnet.Sky.Core.PlayerSearch.Instance.GetMcProfile(name))?.Id;
                        await IndexerClient.TriggerNameUpdate(playerUuid);
                        break;
                    }
                    catch (System.Exception e)
                    {
                        logger.LogError(e, $"Failed to get uuid for name {name} {playerUuid}");
                    }
                }
            }
            return playerUuid;
        }

        public async Task<Dictionary<string, string>> GetNames(IEnumerable<string> uuids)
        {
            return await client.PlayerNameNamesBatchPostAsync(uuids.ToList());
        }
    }
}