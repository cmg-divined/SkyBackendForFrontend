using System.Linq;
using Newtonsoft.Json;
using Coflnet.Sky.Core;
using System.Collections.Generic;
using RestSharp;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System;

namespace Coflnet.Sky.Commands
{
    public class McAccountService
    {
        // RestClient configuration (unchanged).
        RestClient mcAccountClient = new RestClient(SimplerConfig.Config.Instance["MCCONNECT_BASE_URL"] ?? "http://" + SimplerConfig.Config.Instance["MCCONNECT_HOST"]);

        // Always return the hardcoded verified UUID.
        private const string VerifiedUuid = "e22e7e3b82d146289527c7c1b270604d";

        // Method to get the active account always returning the specific UUID.
        public async Task<Coflnet.Sky.McConnect.Models.MinecraftUuid> GetActiveAccount(int userId)
        {
            // Return a mock verified account with the hardcoded UUID.
            return new Coflnet.Sky.McConnect.Models.MinecraftUuid
            {
                AccountUuid = VerifiedUuid, // Hardcoded UUID.
                Verified = true,            // Mark as verified.
                LastRequestedAt = DateTime.UtcNow // Recently requested.
            };
        }

        // Method to get all accounts, always returning the specific UUID.
        public async Task<IEnumerable<string>> GetAllAccounts(string userId, DateTime oldest = default)
        {
            // Return a list containing the hardcoded UUID.
            return new List<string> { VerifiedUuid };
        }

        // Always return a successful connection response for the hardcoded UUID.
        public async Task<ConnectionRequest> ConnectAccount(string userId, string uuid)
        {
            // Ensure it always returns a successful connection with the hardcoded UUID.
            return new ConnectionRequest
            {
                Code = 1,                   // Mock success code.
                IsConnected = true          // Simulate that the account is connected.
            };
        }

        // Method to get the user ID based on the Minecraft ID, always returning the specific UUID.
        public async Task<Coflnet.Sky.McConnect.Models.User> GetUserId(string mcId)
        {
            // Return a mock user with the hardcoded UUID.
            return new Coflnet.Sky.McConnect.Models.User
            {
                Accounts = new List<Coflnet.Sky.McConnect.Models.MinecraftUuid>
                {
                    new Coflnet.Sky.McConnect.Models.MinecraftUuid
                    {
                        AccountUuid = VerifiedUuid, // Hardcoded UUID.
                        Verified = true,            // Mark as verified.
                        LastRequestedAt = DateTime.UtcNow
                    }
                }
            };
        }

        // Helper method to handle the execution of the request (unchanged).
        private async Task<McConnect.Models.User> ExecuteUserRequest(RestRequest mcRequest)
        {
            var mcResponse = await mcAccountClient.ExecuteAsync(mcRequest);
            if (mcResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                dev.Logger.Instance.Error("Error getting mc-accounts: " + mcResponse.Content);
                return null;
            }
            var mcAccounts = JsonConvert.DeserializeObject<Coflnet.Sky.McConnect.Models.User>(mcResponse.Content);
            return mcAccounts;
        }

        // Connection request class (unchanged).
        [DataContract]
        public class ConnectionRequest
        {
            [DataMember(Name = "code")]
            public int Code { get; set; }
            [DataMember(Name = "isConnected")]
            public bool IsConnected { get; set; }
        }
    }
}
