namespace VkNet.Utils
{
    using System.Net;
    using System.Text;

    using Exception;
    using System;

    internal sealed class WebCall
    {
        private HttpWebRequest Request { get; set; }

        private WebCallResult Result { get; set; }

        private WebCall(string url, Cookies cookies, VkApi vk)
        {
            Request = (HttpWebRequest)WebRequest.Create(url);
            Request.Accept = "text/html";
            Request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            Request.CookieContainer = cookies.Container;
            if (!vk.isDefaultVkApi)
            {
                createProxy(ProxyManager.getInstance().reserveOrGet(vk.email), Request);
            }
            Result = new WebCallResult(url, cookies);
        }

        public static WebCallResult MakeCall(string url, VkApi vk)
        {
            var call = new WebCall(url, new Cookies(), vk);

            return call.MakeRequest(vk);
        }

#if false // async version for PostCall
        public static async Task<string> PostCallAsync(string url, string parameters)
        {
            var content = new StringContent(parameters);
            string output = string.Empty;
            using (var client = new HttpClient())
            {   
                HttpResponseMessage response = await client.PostAsync(url, content);
                output = await response.Content.ReadAsStringAsync();
            }

            return output;
        }
#endif

        public static WebCallResult PostCall(string url, string parameters,VkApi vk)
        {
            var call = new WebCall(url, new Cookies(), vk);
            call.Request.Method = "POST";
            call.Request.ContentType = "application/x-www-form-urlencoded";
            
            var data = Encoding.UTF8.GetBytes(parameters);
            call.Request.ContentLength = data.Length;

            using (var requestStream = call.Request.GetRequestStream())
                requestStream.Write(data, 0, data.Length);                

            return call.MakeRequest(vk);
        }

        private static void createProxy(string proxy, HttpWebRequest request)
        {
            string[] splitted = proxy.Split(':');
            if (splitted.Length >= 2)
            {
                string uri = splitted[0];
                int port = Convert.ToInt32(splitted[1]);
                request.Proxy = new WebProxy(uri, port);
            }
            if (splitted.Length == 4)
            {
                request.Proxy.Credentials = new NetworkCredential(splitted[2], splitted[3]);
            }
        }

        public static WebCallResult Post(WebForm form, VkApi vk)
        {
            var call = new WebCall(form.ActionUrl, form.Cookies, vk);

            var request = call.Request;
            
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var formRequest = form.GetRequest();
            request.ContentLength = formRequest.Length;
            request.Referer = form.OriginalUrl;
            request.GetRequestStream().Write(formRequest, 0, formRequest.Length);
            request.AllowAutoRedirect = false;

            return call.MakeRequest(vk);
        }

        private WebCallResult RedirectTo(string url, VkApi vk)
        {
            var call = new WebCall(url, Result.Cookies, vk);

            var request = call.Request;
            request.Method = "GET";
            request.ContentType = "text/html";
            request.Referer = Request.Referer;

            return call.MakeRequest(vk);
        }

        private WebCallResult MakeRequest(VkApi vk)
        {
            using (var response = GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                    throw new VkApiException("Response is null.");

                var encoding = response.CharacterSet != null ? Encoding.GetEncoding(response.CharacterSet) : Encoding.UTF8;
                Result.SaveResponse(response.ResponseUri, stream, encoding);

                Result.SaveCookies(response.Cookies);

                if (response.StatusCode == HttpStatusCode.Redirect)
                    return RedirectTo(response.Headers["Location"], vk);

                return Result;
            }
        }

        private HttpWebResponse GetResponse()
        {
            try
            {
                return (HttpWebResponse)Request.GetResponse();
            }
            catch (WebException ex)
            {
                throw new VkApiException(ex.Message, ex);
            }
        }
    }
}
