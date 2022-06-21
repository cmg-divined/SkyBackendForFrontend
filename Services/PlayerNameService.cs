using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coflnet.Sky.PlayerName.Client.Api;

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
            return await client.PlayerNameNameUuidGetAsync(uuid);
        }
        public async Task<Dictionary<string,string>> GetNames(IEnumerable<string> uuids)
        {
            return await client.PlayerNameNamesBatchPostAsync(uuids.ToList());
        }
    }
}