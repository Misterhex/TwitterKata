using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Misterhex
{
    public class TwitterApi
    {
        private readonly IEnumerable<KeyValuePair<string, string>> _paramsWithoutSignature;

        public TwitterApi()
        {
            _paramsWithoutSignature = GetParametersWithoutSignature();
        }

        public virtual string Request(string url)
        {
            var authorizationHeader = GetAuthorizationHeader(url);

            WebRequest webRequest = HttpWebRequest.Create(url);
            webRequest.Headers.Add(HttpRequestHeader.Authorization, authorizationHeader);

            var webResponse = webRequest.GetResponse();
            StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
            var responseString = streamReader.ReadToEnd();
            return responseString;
        }

        private long SecondsSinceEpoch()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }

        private string GetAuthorizationHeader(string url)
        {
            List<KeyValuePair<string, string>> paramsWithoutSignatureList = new List<KeyValuePair<string, string>>(this._paramsWithoutSignature);

            string signature = CreateSignature(HttpMethod.GET, url);
            paramsWithoutSignatureList.Add(new KeyValuePair<string, string>("oauth_signature", signature));

            var paramsWithSignatureList = paramsWithoutSignatureList.OrderBy(i => i.Key).ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append("OAuth ");

            foreach (var keyValue in paramsWithSignatureList)
            {
                sb.Append(keyValue.Key.UrlEncodeToUpper());
                sb.Append("=");
                sb.Append('"');
                sb.Append(keyValue.Value.UrlEncodeToUpper());
                sb.Append('"');
                sb.Append(", ");
            }

            string result = sb.ToString().TrimEnd(',', ' ');
            return result;
        }

        private List<KeyValuePair<string, string>> GetParametersWithoutSignature()
        {
            string nounce = GenerateNounce();
            string timestamp = SecondsSinceEpoch().ToString();

            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>() 
            {
                new KeyValuePair<string,string>("oauth_consumer_key","YasidTt6NvhVRkzeQoTXw"),
                new KeyValuePair<string,string>("oauth_nonce",nounce),
                new KeyValuePair<string,string>("oauth_signature_method","HMAC-SHA1"),
                new KeyValuePair<string,string>("oauth_timestamp",timestamp),
                new KeyValuePair<string,string>("oauth_token","140354657-EKsNXJrBwRcv73W4k3fn3Uf32lRDs6o99h9vo91h"),
                new KeyValuePair<string,string>("oauth_version","1.0")
            };
            return keyValues;
        }

        private string GenerateNounce()
        {
            string s = Guid.NewGuid().ToString();
            s = s.Replace("-", string.Empty);
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(s);
            string result = Convert.ToBase64String(bytes);
            result = new string(result.Take(32).ToArray());
            return result;
        }

        private List<KeyValuePair<string, string>> ParseUrlParams(string url)
        {
            int index = url.IndexOf("?") + 1;
            string toParse = new string(url.Skip(index).ToArray());

            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            foreach (var keyValueString in toParse.Split('&'))
            {
                var keyAndValue = keyValueString.Split('=').ToList();
                if (keyAndValue.Count == 2)
                {
                    result.Add(new KeyValuePair<string, string>(keyAndValue.ElementAt(0), keyAndValue.ElementAt(1)));
                }
            }

            return result;
        }

        private string CreateSignatureParameterString(string url)
        {
            var urlParams = ParseUrlParams(url);

            var sortedByKey = this._paramsWithoutSignature.Concat(urlParams).OrderBy(i => i.Key);

            StringBuilder sb = new StringBuilder();
            foreach (var param in sortedByKey)
            {
                sb.Append(param.Key.UrlEncodeToUpper() + "=" + param.Value.UrlEncodeToUpper());
                sb.Append("&");
            }
            var result = sb.ToString().TrimEnd('&');
            return result;
        }

        private string CreateSignatureBaseString(HttpMethod method, string url)
        {
            string urlMain = url.IndexOf('?') != -1 ? new string(url.Take(url.IndexOf('?')).ToArray()) : url;

            StringBuilder sb = new StringBuilder();
            sb.Append(method.ToString().ToUpper());
            sb.Append("&");
            sb.Append(urlMain.UrlEncodeToUpper());
            sb.Append("&");
            sb.Append(CreateSignatureParameterString(url).UrlEncodeToUpper());
            var result = sb.ToString();

            return result;
        }

        private string CreateSigningKeyString()
        {
            string consumerSecret = "lc3nWO73RN5MOQ1FEn2Pgbh0rMSlXOtFneZH3c69g";
            string tokenSecret = "MUFMkNhpPH8e7PbaSbJ7Ys7AlfK1fDHv76TvT4Ojk";
            string signingKey = consumerSecret.UrlEncodeToUpper() + "&" + tokenSecret.UrlEncodeToUpper();
            return signingKey;
        }

        private string CreateSignature(HttpMethod method, string url)
        {
            string signingKey = CreateSigningKeyString();
            string baseString = CreateSignatureBaseString(method, url);

            byte[] key = Encoding.ASCII.GetBytes((signingKey));
            byte[] signatureBaseString = Encoding.ASCII.GetBytes((baseString));
            HMACSHA1 hmacsha1 = new HMACSHA1(key);
            byte[] hash = hmacsha1.ComputeHash(signatureBaseString);
            string signature = Convert.ToBase64String(hash);

            return signature;
        }
    }

    public static class StringExtensions
    {
        public static string UrlEncodeToUpper(this string s)
        {
            string encoded = HttpUtility.UrlEncode(s);

            List<char> newForm = new List<char>();

            int counter = 0;
            foreach (var c in encoded)
            {
                if (c == '%')
                {
                    newForm.Add(c);
                    counter = 2;
                    continue;
                }

                if (counter > 0)
                {
                    newForm.Add(c.ToString().ToUpper().ToArray().First());

                }
                else
                {
                    newForm.Add(c);
                }
                counter--;
            }

            string result = new string(newForm.ToArray());
            return result;
        }

    }

    public enum HttpMethod
    {
        POST,
        GET
    }
}
