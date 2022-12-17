using System;
using System.Threading.Tasks;
using RestSharp;

namespace Coflnet.Sky.Commands.MC
{
    public class NextUpdateRetriever
    {
        static RestClient client = new RestClient(SimplerConfig.SConfig.Instance["UPDATER_BASE_URL"]);
        public async Task<DateTime> Get()
        {
            try
            {
                DateTime last = default;
                while (last < new DateTime(2020, 1, 1))
                {
                    last = (await client.ExecuteAsync<DateTime>(new RestRequest("/api/time"))).Data;
                }
                var next = last + TimeSpan.FromSeconds(60);
                while (next < DateTime.Now)
                    next += TimeSpan.FromMinutes(1);
                return next;
            }
            catch (Exception e)
            {
                dev.Logger.Instance.Error(e, "getting next update time");
                throw e;
            }
        }
    }
}