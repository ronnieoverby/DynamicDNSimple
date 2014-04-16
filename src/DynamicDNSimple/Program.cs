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

            var cts = new CancellationTokenSource();
            IRetryStrategy retryStrategy = new RetryStrategy
            {
                MaxDuration = TimeSpan.FromMinutes(10),
                FailureDelay = TimeSpan.FromSeconds(10),
            };
            dynamic[] recs =
                Attempt.Repeatedly.Get(() => dns.ListRecords(domain)).UsingStrategy(retryStrategy, cts.Token)
                    .ThrowIfCantSucceed()
                    .GetValueOrDefault();

            var rec = recs.SingleOrDefault(x => x.record.name.Equals(recordName, StringComparison.OrdinalIgnoreCase));
            bool hasRecord = rec != null;

            var newIp = GetNewIp();

            if (hasRecord)
            {
                rec = rec.record;
                var oldIp = IPAddress.Parse(rec.content.Trim());
                bool ipChanged = !oldIp.Equals(newIp);

                // update it if the ip address has changed
                if (ipChanged.Dump("IP Changed"))
                {
                    "Updating Record".Dump();
                    dns.UpdateRecord(rec.domain_id,
                        rec.id,
                        recordName,
                        newIp.ToString(),
                        60);
                }
            }
            else
            {
                // add it
                "Adding Record".Dump();

                dns.AddRecord(domain,
                    recordName,
                    "A",
                    newIp.ToString(),
                    60);
            }
        }

        static IPAddress GetNewIp()
        {
            var web = new WebClient();
            var s = web.DownloadString("http://icanhazip.com").Trim();
            var ip = IPAddress.Parse(s);
            return ip;
        }
    }
}

