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
        static string premiumPlusSlug = SimplerConfig.SConfig.Instance["PRODUCTS:PREMIUM_PLUS"];
        static string starterPremiumSlug = SimplerConfig.SConfig.Instance["PRODUCTS:STARTER_PREMIUM"];

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

        public async Task<(AccountTier, DateTime)> GetCurrentTier(int userId)
        {
            try
            {
                if (GoogleUser.EveryoneIsPremium)
                    return (AccountTier.PREMIUM, DateTime.Now + TimeSpan.FromDays(30));
                var owns = await userApi.UserUserIdOwnsUntilPostAsync(userId.ToString(), new() { premiumPlanName, premiumPlusSlug, starterPremiumSlug });
                if (premiumPlusSlug != null && owns.TryGetValue(premiumPlusSlug, out DateTime end))
                    return (AccountTier.PREMIUM_PLUS, end);
                if (premiumPlanName != null && owns.TryGetValue(premiumPlanName, out end))
                    return (AccountTier.PREMIUM, end);
                if (starterPremiumSlug != null && owns.TryGetValue(starterPremiumSlug, out end))
                    return (AccountTier.STARTER_PREMIUM, end);
            }
            catch (Exception e)
            {
                dev.Logger.Instance.Error(e, "retrieving premium status for " + userId);
                return (AccountTier.PREMIUM, DateTime.Now + TimeSpan.FromMinutes(3));
            }
            return (AccountTier.NONE, DateTime.Now + TimeSpan.FromMinutes(3));
        }

        public async Task<DateTime> ExpiresWhen(string userId)
        {
            if (GoogleUser.EveryoneIsPremium)
                return DateTime.Now + TimeSpan.FromDays(30);
            var until = await userApi.UserUserIdOwnsLongestPostAsync(userId, new() { premiumPlanName, testpremiumPlanName });
            return until;
        }
        public async Task<bool> HasPremium(int userId)
        {
            return (await ExpiresWhen(userId)) > DateTime.Now;
        }
    }
}