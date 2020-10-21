using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;

namespace AoDAuth
{
    class Program
    {
        // Duh who would have guessed...
        public static string username;
        public static string password;

        // strings in the HTML the CSRF-Token is surrounded by
        public static string tokenStart = $"<meta name=\"csrf-token\" content=\"";
        public static string tokenEnd = "\" /> ";

        static void Main(string[] args)
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://anime-on-demand.de");

            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();


            request.Method = HttpMethod.Get;
            response = client.SendAsync(request).Result;

            // parsing HTML to get the CSRF-Token
            string source = response.Content.ReadAsStringAsync().Result;
            int start = source.IndexOf(tokenStart) + tokenStart.Length;
            int end = source.IndexOf(tokenEnd);

            string result = source.Substring(start, end - start);
            string csrfToken = result.Substring(0, result.IndexOf("\""));

            Console.WriteLine(csrfToken + Environment.NewLine);

            HttpRequestMessage postRequest = new HttpRequestMessage(HttpMethod.Post, "/users/sign_in");

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("utf8", "✓"),
                new KeyValuePair<string, string>("authenticity_token", csrfToken),
                new KeyValuePair<string, string>("user[login]", username),
                new KeyValuePair<string, string>("user[password]", password),
                new KeyValuePair<string, string>("user[remember_me]", "0"),
                new KeyValuePair<string, string>("commit", "Einloggen")

            });

            postRequest.Content = formContent;

            response = client.SendAsync(postRequest, HttpCompletionOption.ResponseHeadersRead).Result;
            // returns _aod_session
            Console.WriteLine(response.Headers.GetValues("Set-Cookie").ElementAt(2));
        }
    }
}
