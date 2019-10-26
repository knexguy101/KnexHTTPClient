using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KnexHttpClient;
using Org.BouncyCastle.Crypto.Tls;

namespace UnitTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            var cj = new CookieContainer();
            KnexClient client = new KnexClient(cj);

            //make new request like normal
            HttpRequestMessage rq = new HttpRequestMessage(HttpMethod.Get, "https://stockx.com/api/browse?productCategory=sneakers&page=0" ); 

            //add whatever headers you need
            rq.Headers.TryAddWithoutValidation("Content-Type", "applicaton/json");           
            rq.Headers.TryAddWithoutValidation("Accept", "*/*");
            rq.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");

            //list your ciphers
            List<string> cipherlist = new List<string>() { "TLS_AES_256_GCM_SHA384" };

            //get a response using the function in the class
            var response = client.SendWithCipher(rq, cipherlist.ToArray());

            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            Console.WriteLine(response); //content-length and type will show up automatically in headers section if it is html
            Console.WriteLine("");
            Console.WriteLine("");
            Console.ReadKey();
        }
    }
}
