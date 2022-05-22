using System;
using Google.Apis.Auth;
using Coflnet.Sky.Core;
using System.Threading.Tasks;
using Coflnet.Payments.Client.Api;

namespace Coflnet.Sky.Commands.Shared
{
    public class PremiumService
    {
        static string premiumPlanName = SimplerConfig.SConfig.Instance["PRODUCTS:PREMIUM"];
        static string testpremiumPlanName = SimplerConfig.SConfig.Instance["PRODUCTS:TEST_PREMIUM"];

        private UserApi userApi;

        public PremiumService(UserApi userApi)
        {
            this.userApi = userApi;
        }

        public GoogleUser GetUserWithToken(string token)
        {

            return UserService.Instance.GetOrCreateUser(ValidateToken(token).Subject);
        }
        
        public GoogleJsonWebSignature.Payload ValidateToken(string token)
        {
            try
            {
                var client = GoogleJsonWebSignature.ValidateAsync(token);
                client.Wait();
                var tokenData = client.Result;
                Console.WriteLine("google user: " + tokenData.Name);
                return tokenData;
            }
            catch (Exception e)
            {
                throw new CoflnetException("invalid_token", $"{e.InnerException.Message}");
            }
        }

        public async Task<DateTime> ExpiresWhen(int userId)
        {
            return await ExpiresWhen(userId.ToString());
        }

        public async Task<DateTime> ExpiresWhen(string userId)
        {
            if(GoogleUser.EveryoneIsPremium)
                return DateTime.Now + TimeSpan.FromDays(30);
            var until = await userApi.UserUserIdOwnsLongestPostAsync(userId, new (){ premiumPlanName, testpremiumPlanName});
            return until;
        } 
        public async Task<bool> HasPremium(int userId)
        {
            return (await ExpiresWhen(userId)) > DateTime.Now;
        } 
    }
}