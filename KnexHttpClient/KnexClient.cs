using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace KnexHttpClient
{
    public class KnexClient
    {
        private CookieContainer _CookieJar { get; set; }
        private WebProxy _Proxy { get; set; }

        public KnexClient(CookieContainer CookieJar, WebProxy Proxy = null)
        {
            this._CookieJar = CookieJar;
            this._Proxy = Proxy;
        }

        //we gotta use curl, since httpclietn doesnt like us to fuck with ciphers
        public HttpResponseMessage SendWithCipher(HttpRequestMessage Message, string[] Ciphers, int Timeout = 0)
        {
            //convert our httpreqruest into a string
            var x = ConvertRequestToCurl(Message, this._CookieJar, Ciphers);

            //execute with curl
            var y = Curl.ExecuteCurl(x, Timeout);

            //return a formatted version of the response
            return ConvertStringToResponse(y);
        }

        /// <summary>
        /// Converts the curl response into an httprequestmessage
        /// </summary>
        /// <param name="CurlResponse"></param>
        /// <returns></returns>
        private HttpResponseMessage ConvertStringToResponse(string CurlResponse)
        {
            var response = new HttpResponseMessage();

            if (!CurlResponse.Contains("<!DOCTYPE html>"))
            {
                var firstList = CurlResponse.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                Console.WriteLine(firstList.Length.ToString());
                if (firstList.Length > 1) //check for response body
                {
                    response.Content = new StringContent(firstList[0].Trim(), System.Text.Encoding.UTF8, "applicaton/json");
                }
                else
                {
                    response.Content = new StringContent("");
                }

                var datalist = firstList.Length > 1 ? firstList[1].Split(',') : CurlResponse.Split(','); //curl does body \n the rest of the data so we get the 1st index
                response.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), datalist[0].Replace("StatusCode:", "").Trim()); //get status code
                response.ReasonPhrase = datalist[1].Replace("ReasonPhrase: ", "").Replace("'", "").Trim(); //get reason phrase
                response.Version = new Version(datalist[2].Replace("Version:", "").Trim()); //get version
                //skip content: bc that was done above

                //put header response into jobject
                var headers = datalist[4].Replace("Headers: ", "").Replace("{", "").Replace("}", "").Trim();
                foreach(var header in headers.Split(';'))
                {
                    var splitheader = header.Split(':');
                    response.Headers.Add(splitheader[0].Trim(), splitheader[1].Trim());
                }
            }
            else
            {
                //web pages dont give back statuses and stuff so i just load it into the body and call it a day.
                response.Content = new StringContent(CurlResponse);
                response.ReasonPhrase = "HTML";
            }

            return response;
        }

        /// <summary>
        /// Converts the elemnts of an httprequestmessage to a curl string
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="CookieJar"></param>
        /// <param name="Ciphers"></param>
        /// <returns></returns>
        private string ConvertRequestToCurl(HttpRequestMessage Message, CookieContainer CookieJar, string[] Ciphers)
        {
            string x = "curl ";
            foreach(Cookie cookie in CookieJar.GetCookies(Message.RequestUri))
            {
                x += $"-b '{cookie.Name}={cookie.Value}' ";
            }
            foreach(var header in Message.Headers)
            {
                x += $"-H '{header.Key}: {(header.Value as string[])[0]}' ";
            }
            x += $"--ciphers {string.Join(",", Ciphers)} ";

            if (Message.Content != null)
                x += $"-d '{Message.Content.ReadAsStringAsync().Result}' ";

            if (_Proxy != null)
            {
                x += $"-x {_Proxy.Address} ";
                if(_Proxy.Credentials != null)
                {
                    var creds = _Proxy.Credentials.GetCredential(Message.RequestUri, "");
                    x += $"-U {creds.UserName}:{creds.Password} ";
                }
            }

            //end it off
            x += $"-X {Message.Method.ToString().ToUpper()} {Message.RequestUri.AbsoluteUri}";

            return x;
        }
    }
}


