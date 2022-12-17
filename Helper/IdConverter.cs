using System;
using System.Text;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    public class IdConverter
    {
        /// <summary>
        /// Converts a playerUuid and a session id into a connection id
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public (long, string) ComputeConnectionId(string playerId, string sessionId)
        {
            var bytes = Encoding.UTF8.GetBytes(playerId.ToLower() + sessionId + DateTime.Now.RoundDown(TimeSpan.FromDays(120)).ToString());
            var hash = System.Security.Cryptography.SHA512.Create();
            var hashed = hash.ComputeHash(bytes);
            return (BitConverter.ToInt64(hashed), Convert.ToBase64String(hashed, 0, 16).Replace('+', '-').Replace('/', '_'));
        }
    }
}