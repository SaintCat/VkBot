using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using VkGroupBot.Treeview;
using VkNet;
using VkNet.Enums;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace VkGroupBot.Utils
{
    class PostmanHelper
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        private Group _group;
        VkApi vk = VkApiFactory.getInstance().getDefaultVkApi();
        public PostmanHelper(long groupUid)
        {
            _group = VkGroupManager.getInstance().getGroupById(groupUid);
        }

        public void postNew()
        {
            DateTime date = DateTime.Now.AddMinutes(-40);
            SortedList<double, Post> allPosts = new SortedList<double, Post>();
            List<string> listForUser = getListForUser(_group.Id);
            foreach (string groupNae in listForUser) 
            {
                int ignor;
                int offset = 1;
                Post post;
                while ((post = vk.Wall.Get(0, groupNae, out ignor, 1, offset, WallFilter.All)[0]).Date.Value.CompareTo(date) > 0)
                {
                    allPosts.Add(getKef(post), post);
                    offset++;
                    //Thread.Sleep(1500);
                }
                Thread.Sleep(2000);
            }
            logger.Info("Find new posts in count " + allPosts.Count + " trying to get the best.");
            if (allPosts.Count > 0)
            {
                Post best = allPosts[allPosts.Keys.Max()];
                postThis(best);
                logger.Info("Best is " +best.Id + " Text : " + best.Text);
            }
        }

        private void postThis(Post best)
        {
            IList<MediaAttachment> mediaAttachments = new List<MediaAttachment>();
            foreach (Attachment att in best.Attachments)
            {
                if (att.Instance is MediaAttachment)
                {
                    MediaAttachment medAtt = (MediaAttachment)att.Instance;
                    if (att.Type == typeof(Photo))
                    {
                        UploadServerInfo info = vk.Photo.GetWallUploadServer(_group.Id);
                        NameValueCollection nvc = new NameValueCollection();
                        Dictionary<string, string> response = new Dictionary<string, string>();
                        using (WebClient client = new WebClient())
                        {
                            byte[] downloaded = client.DownloadData(((Photo)medAtt).Photo604);
                            var values = new NameValueCollection();
                            values["photo"] = Convert.ToBase64String(downloaded);
                           
                            string s = HttpUploadFile(info.UploadUrl.ToString(), downloaded, "photo", "image/jpeg", nvc);
                            response = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(s);
                        }
                        Photo ph = null;

                        ph = vk.Photo.SaveWallPhoto(response["photo"], null, _group.Id, int.Parse(response["server"]), response["hash"])[0];

                        mediaAttachments.Add(ph);
                    }
                    else
                    {
                        mediaAttachments.Add(medAtt);
                    }
                }

            }
         
            long id = vk.Wall.Post(-_group.Id, false, true, best.Text, mediaAttachments, null, null, false, null, null, null, null, null);
               logger.Info("new post published with id == " + id); 
   
        }

        private double getKef(Post post)
        {
            int members = (int)_group.MembersCount;

            return (float)(post.Reposts.Count * 2 + post.Likes.Count) /( (float)members * (DateTime.Now - (DateTime)post.Date).Minutes);
        }


        private List<string> getListForUser(long groupId)
        {
            switch (groupId)
            {
                case 98013659:
                    return uidsForHumor;
                case 98738124:
                    return uidsForPhotoes;
                default:
                    return new List<string>();
            }
        }

        private List<string> testUids = new List<string>() { "club13704425", "eternity",  "velikieslova",
        "founddreams", "comotivation", "1woman", "public41108497", "public86218441"};
        private List<string> uidsForHumor = new List<string>() {"public12382740", "public10639516", "public31836774","public29246653", "public30179569", "public23064236", "public26419239", "public36164349", "public33159467", "public22741624","public34491673"};
        private List<string> uidsForPhotoes = new List<string>() {"public27725748","public29411855", "public42564857", "public56905360", "club16979732", "public5880263", "public64628087", "public64173570", "club18099999", "public23308460" };




        public static string HttpUploadFile(string url, byte[] file, string paramName, string contentType, NameValueCollection nvc)
        {
            Console.WriteLine(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            //wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;





            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, "photo.jpg", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            // FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            //byte[] buffer = new byte[4096];
            //int bytesRead = 0;
            //while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            //{
            rs.Write(file, 0, file.Count());
            //}
            // fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);




                string s3 = reader2.ReadToEnd();

                return s3;



            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }


                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);



                string s3 = reader2.ReadToEnd();

                return s3;


            }
            finally
            {
                wr = null;
            }

        }
    }
}
