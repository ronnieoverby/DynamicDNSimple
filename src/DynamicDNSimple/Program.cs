using System.Threading;
using CoreTechs.Common;
using DNSimple;
using System;
using System.Linq;
using System.Net;

namespace DynamicDNSimple
{
    class Program
    {
        static void Main(string[] args)
        {
            var username = args[0];
            var password = args[1];
            var domain = args[2];
            var recordName = args[3];

            var dns = new DNSimpleRestClient(username, password);


            dynamic[] recs = GetWithRetry(() => dns.ListRecords(domain));

            var rec = recs.SingleOrDefault(x => x.record.name.Equals(recordName, StringComparison.OrdinalIgnoreCase));
            bool hasRecord = rec != null;

            var newIp = GetWithRetry(GetNewIp);

            if (hasRecord)
            {
                rec = rec.record;
                var oldIp = IPAddress.Parse(rec.content.Trim());
                bool ipChanged = !oldIp.Equals(newIp);

                // update it if the ip address has changed
                if (!ipChanged.Dump("IP Changed")) return;

                "Updating Record".Dump();
                DoWithRetry(() =>
                    dns.UpdateRecord(rec.domain_id,
                        rec.id,
                        recordName,
                        newIp.ToString(),
                        60));
            }
            else
            {
                // add it
                "Adding Record".Dump();

                DoWithRetry(() =>
                    dns.AddRecord(domain,
                        recordName,
                        "A",
                        newIp.ToString(),
                        60));
            }
        }
        
        static IPAddress GetNewIp()
        {
            var web = new WebClient();
            var s = web.DownloadString("http://icanhazip.com").Trim();
            var ip = IPAddress.Parse(s);
            return ip;
        }

        private static T GetWithRetry<T>(Func<T> factory)
        {
            IRetryStrategy retryStrategy = new RetryStrategy
            {
                MaxDuration = TimeSpan.FromMinutes(10),
                FailureDelay = TimeSpan.FromSeconds(10),
            };

            return Attempt.Repeatedly.Get(factory).UsingStrategy(retryStrategy, default(CancellationToken))
                .ThrowIfCantSucceed()
                .GetValueOrDefault();
        }

        private static void DoWithRetry(Action action)
        {
            GetWithRetry(() =>
            {
                action();
                return true;
            });
        }

 
    }
}

