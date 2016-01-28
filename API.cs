using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace HitboxDashboard_WPF
{
    public static class API
    {
        public const String BaseApi = "https://www.hitbox.tv/api";

        public static object Post(string url, string json)
        {
            try
            {
                WebClient c = new WebClient();
                var responseFromServer = c.UploadString(url, json);

                c.Dispose();
                return responseFromServer;
            }
            catch (WebException exception)
            {
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    var responseText = reader.ReadToEnd();
                    Debug.WriteLine(exception);
                    return responseText;
                }
            }
        }
        
        public static void PostAync(string url, string json)
        {
            try
            {
                WebClient c = new WebClient();
                Uri url2 = new Uri(url);
                c.UploadStringAsync(url2, json);

                c.Dispose();
            }
            catch (WebException exception)
            {
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    Debug.WriteLine(exception);
                }
            }
        }

        public static object Put(string url, string json)
        {
            try
            {
                WebClient c = new WebClient();
                var responseFromServer = c.UploadString(url, "PUT", json);

                c.Dispose();
                return responseFromServer;
            }
            catch (WebException exception)
            {
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    var responseText = reader.ReadToEnd();
                    Debug.WriteLine(exception);
                    return responseText;
                }
            }
        }

        public static object Get(string url)
        {
            try
            {
                WebClient c = new WebClient();

                var responseFromServer = c.DownloadString(url);

                c.Dispose();
                //Trace.WriteLine(String.Format("GetApi Data: {0}", responseFromServer));
                return responseFromServer;
            }
            catch (WebException exception)
            {
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    var responseText = reader.ReadToEnd();
                    return responseText;
                }
            }
        }

        //private void UpdateChatBlackList(string blacklist)
        //{
        //    var json = new JavaScriptSerializer().Serialize(new
        //    {
        //        blacklist = new[]
        //        {
        //            blacklist
        //        }
        //    });
        //    WebClient c = new WebClient();
        //    //Trace.WriteLine(String.Format("{0}/chat/blacklist/hitakashi?authToken={1}", baseAPI, authToken));
        //    //c.UploadString(string.Format("{0}/chat/blacklist/hitakashi?authToken={1}", baseAPI, authToken), json);
        //}


        //public static Boolean Token()
        //{
        //    var json = new JavaScriptSerializer().Serialize(new
        //    {
        //        login = HitboxForm.Instance.usernameTextBox.Text,
        //        pass = HitboxForm.Instance.passwordTextBox.Text
        //    });

        //    var post = Post(BaseApi + "/auth/token", json);

        //    if (post.Equals("auth_failed"))
        //    {
        //        HitboxForm.Instance.InvalidUsernamePasswordLogin();
        //        return false;
        //    }
        //    HitboxForm.Instance.authToken = JObjectParse(post, "authToken");
        //    return true;
        //}

        public static string JObjectParse(object o, string value)
        {
            JObject obj = JObject.Parse((string)o);
            return (string)obj[value];
        }

        public static Boolean Contains(JArray array, string value)
        {
            foreach (string z in array)
            {
                if (z.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }
      
    }
}
