using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coflnet.Sky.PlayerName.Client.Api;
using Coflnet.Sky.Commands.Shared;

namespace Coflnet.Sky.PlayerName
{
    public class PlayerNameService
    {
        PlayerName.Client.Api.PlayerNameApi client;

        public PlayerNameService(PlayerNameApi client)
        {
            this.client = client;
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
                await IndexerClient.TriggerNameUpdate(playerUuid);
                await Task.Delay(5000);
                playerUuid = (await client.PlayerNameUuidNameGetAsync(name))?.Trim('"');
            }
            return playerUuid;
        }

        public async Task<Dictionary<string, string>> GetNames(IEnumerable<string> uuids)
        {
            return await client.PlayerNameNamesBatchPostAsync(uuids.ToList());
        }
    }
}