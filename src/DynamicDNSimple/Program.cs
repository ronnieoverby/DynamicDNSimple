using DNSimple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DynamicDNSimple
{
    static class Extensions {
        public static T Dump<T>(this T obj, string title = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                Console.WriteLine(obj);

            else Console.WriteLine("{0}: {1}", title, obj);
            return obj;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var username = args[0];
            var password = args[1];
            var domain = args[2];
            var recordName = args[3];

            var dns = new DNSimpleRestClient(username, password);

            dynamic[] recs = dns.ListRecords(domain);
            var rec = recs.SingleOrDefault(x => x.record.name.Equals(recordName, StringComparison.OrdinalIgnoreCase));
            bool hasRecord = rec != null;

            var newIp = GetNewIp();

            if (hasRecord)
            {
                rec = rec.record;
                string oldIp = rec.content.Trim();
                var ipChanged = oldIp != newIp;

                if (!ipChanged.Dump("IP Changed"))
                    return;

                // update it if the ip address has changed
                "Updating Record".Dump();
                rec = dns.UpdateRecord(rec.domain_id,
                    rec.id,
                    recordName,
                    newIp,
                    60);
            }
            else
            {
                // add it
                "Adding Record".Dump();

                rec = dns.AddRecord(domain,
                    recordName,
                    "A",
                    newIp,
                    60);
            }


            Console.ReadKey();
        }



     static   string GetNewIp()
        {
            return new WebClient().DownloadString("http://icanhazip.com").Trim();
        }


    }
}

