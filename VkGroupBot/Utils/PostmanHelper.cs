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
        private Group _group;
        VkApi vk = VkApiFactory.getInstance().getDefaultVkApi();
        public PostmanHelper(VkGroup group)
        {
            _group = VkGroupManager.getInstance().getGroupById(group.groupId);
        }

        public void postNew()
        {
            DateTime date = DateTime.Now.AddHours(-1);
            SortedList<double, Post> allPosts = new SortedList<double, Post>();
            foreach(string groupNae in testUids) 
            {
                int ignor;
                int offset = 1;
                Post post;
                while ((post = vk.Wall.Get(0, groupNae, out ignor, 1, offset, WallFilter.All)[0]).Date.Value.CompareTo(date) > 0)
                {
                    allPosts.Add(getKef(post), post);
                    offset++;
                }
                Thread.Sleep(1000);
            }
            Post best = allPosts[allPosts.Keys.Max()];
            postThis(best);
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
                            //byte[] responseByte = client.UploadValues(info.UploadUrl, values);
                            //string s = client.Encoding.GetString(responseByte);
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
            vk.Wall.Post(-_group.Id, false, true, best.Text, mediaAttachments, null, null, false, null, null, null, null, null);
   
        }

        private double getKef(Post post)
        {
            return (post.Reposts.Count * 1.2 + post.Likes.Count);
        }
        private List<string> testUids = new List<string>() { "club13704425", "psdnevnik", "eternity",  "velikieslova",
        "founddreams", "comotivation"};

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
            string header = string.Format(headerTemplate, paramName, "ptoho.jpg", contentType);
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
