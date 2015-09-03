﻿using SimpleAntiGate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkGroupBot.Utils;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace LiveUserTest
{
    class Program
    {

        private static List<Message> getUnreadedMessages(ReadOnlyCollection<Message> messages)
        {
            List<Message> res = new List<Message>();
            foreach (Message m in messages)
            {
                if (m.ReadState == MessageReadState.Unreaded)
                {
                    if (m.Date.Value.DayOfYear == System.DateTime.Now.DayOfYear)
                    {
                        res.Add(m);
                    }
                }
            }
            return res;
        }
        static void Main(string[] args)
        {
            VkApi vk = VkApiFactory.getInstance().getDefaultVkApi();
            vk.isDefaultVkApi = true;
            while (true)
            {
                
                int total = 0;
                ReadOnlyCollection<Message> messages = vk.Messages.Get(MessageType.Received, out total);
                Console.WriteLine("New messages in count = " + total);
                List<Message> unreaded = getUnreadedMessages(messages);
                foreach(Message m in unreaded) 
                {
                    try
                    {
                        string answer = getAnswer((long)vk.UserId, (long)m.UserId, m.Body);
                        vk.Messages.MarkAsRead((long)m.Id);
                        vk.Messages.SetActivity((long)m.UserId);

                        Thread.Sleep(4000);
                        vk.Messages.Send((long)m.UserId, false, answer);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }
                }
                Thread.Sleep(1500);
            }

        }

        private static string getAnswer(long userId, long senderId, String reply)
        {
            string answer = null;
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["id"] = Convert.ToString(userId);
                values["botid"] = "970c8b3d-2e25-471d-8aab-efc87bcb7155";
            
                var response = client.UploadValues("http://bot.mew.su/service/getsession.php", values);

                var responseString = Encoding.Default.GetString(response);
                System.Console.WriteLine(responseString);

                var values2 = new NameValueCollection();
                values2["session"] = responseString;
                values2["botid"] = Convert.ToString(userId);
                values2["sender"] = Convert.ToString(senderId);
                values2["ischat"] = "0";
                values2["text"] = reply;
                var response2 = client.UploadValues("http://bot.mew.su/service/speak.php", values2);
                answer = Encoding.UTF8.GetString(response2);
                System.Console.WriteLine(answer);
            }
            return answer;
        }

    }

    public class UserTaskManager
    {
        private const int interval = 60000*5;
        private const int shift = 60000;
        private List<UserTask> _tasks;
        private Random r = new Random();

        public UserTaskManager(List<UserTask> task)
        {
            this._tasks = task;
            Shuffle(this._tasks);
            
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            Random rnd = new Random();
            while (n > 1)
            {
                int k = (rnd.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public void start()
        {
            foreach(UserTask task in _tasks)
            {
                try
                {
                    task.execute();
                    int shiftVal = -shift + r.Next(shift * 2);
                    Thread.Sleep(interval + shiftVal);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        public class RandomMessageToRandomFriend : UserTask
        {
            VkApi vk;
            User user;

            public RandomMessageToRandomFriend(VkApi vk, User user)
            {
                this.vk = vk;
                this.user = user;
            }

            public void execute()
            {
            }
        }

        public class JoinInSomeGroup : UserTask
        {
            VkApi vk;
            User user;

            public JoinInSomeGroup(VkApi vk, User user)
            {
                this.vk = vk;
                this.user = user;
            }

            public void execute()
            {
                Boolean joined = false;
                int counter = 0;
                while (!joined)
                {
                    Console.WriteLine("Join in some group");
                    int total;
                    ReadOnlyCollection<Group> grous = vk.Groups.Search("Лучшее", out total, counter, 5);
                    foreach(Group g in grous){
                        if (!(bool)g.IsMember)
                        {
                            vk.Groups.Join(g.Id);
                            joined = true;
                            break;
                        }
                    }
                    counter += total;
                }
            }
        }

        public class RepostFromSomeGroup : UserTask
        {
            VkApi vk;
            User user;

            public RepostFromSomeGroup(VkApi vk, User user)
            {
                this.vk = vk;
                this.user = user;
            }

            public void execute()

            {
                Console.WriteLine("Reposting something from some group");
                ReadOnlyCollection<Group> groups = vk.Groups.Get(user.Id, true, GroupsFilters.All, GroupsFields.All, 0, 30);
                Random r = new Random();
                int rPos = r.Next(groups.Count);
                Group groupForRepost = groups[rPos];
                int total;
                ReadOnlyCollection<Post> posts = vk.Wall.Get(0, groupForRepost.ScreenName, out total, 20, 0, WallFilter.All);
                int rPost = r.Next(posts.Count);
                Post p = posts[rPost];
                vk.Wall.Repost("wall-" + groupForRepost.Id+ "_" + p.Id, ")", null);
            }
        }

        public class LikeToFriend : UserTask
        {
            VkApi vk;
            User user;

            public LikeToFriend(VkApi vk, User user)
            {
                this.vk = vk;
                this.user = user;
            }

            public void execute()
            {
                Console.WriteLine("Likes photo of friend");
                ReadOnlyCollection<User> friend = vk.Friends.Get(user.Id);
                Random r = new Random();
                int rrr = r.Next(friend.Count);
                User us = friend[rrr];
                int conter = 0;
                bool liked = false;
                while (!liked)
                {
                    ReadOnlyCollection<Photo> photoes = vk.Photo.GetProfile(us.Id, null, false, true, null, null, null, 20, conter);
                    foreach (Photo p in photoes)
                    {
                        bool asd;

                        if (!vk.Likes.IsLiked(LikeObjectType.Photo, p.Id, out asd, null, null))
                        {
                            vk.Likes.Add(LikeObjectType.Photo, p.Id, p.OwnerId, null);
                            liked = true;
                            break;
                        }
                    }
                    conter += photoes.Count;
                }
                
            }
        }

        public class InviteFriendInGroup : UserTask
        {
            VkApi vk;
            User user;
            long groupName;

            public InviteFriendInGroup(VkApi vk, User user, long groupName)
            {
                this.vk = vk;
                this.user = user;
                this.groupName = groupName;
            }

            public void execute()
            {
                string fileName = user.Id + "_" + groupName + ".txt";
                string[] alreadyUsedIds;
                if (!(System.IO.File.Exists(fileName)))
                {
                    System.IO.File.Create(fileName).Close();
                    alreadyUsedIds = new string[]{};
                } else {
                    alreadyUsedIds = System.IO.File.ReadAllLines(fileName);
                }
                Console.WriteLine("Invited friend to group");
                bool invited = false;
                int countForRepeat = 5;
                int repearcount = 0;
                while (!invited)
                {
                    if (repearcount == countForRepeat)
                    {
                        break;
                    }
                    ReadOnlyCollection<User> friend = vk.Friends.Get(user.Id, ProfileFields.All);
                    var friend2 = sort(friend);
                    foreach (User us in friend2)
                    {

                        try
                        {
                            if (!vk.Groups.IsMember(groupName, us.Id) && !alreadyUsedIds.Contains(user.Id.ToString()))
                            {
                                try
                                {
                                    vk.Groups.Invite(groupName, us.Id);
                                    invited = true;
                                    System.IO.File.AppendAllLines(fileName, new string[] { us.Id.ToString() });

                                    return;
                                }
                                catch (CaptchaNeededException ex)
                                {
                                    try
                                    {
                                        string captchaAnswer = AntiGate.Recognize(ex.Img.AbsoluteUri);

                                        vk.Groups.Invite(groupName, us.Id, ex.Sid, captchaAnswer);
                                        invited = true;
                                        System.IO.File.AppendAllLines(fileName, new string[] { us.Id.ToString() });
                                        return;
                                    }
                                    catch (CaptchaNeededException ex2)
                                    {
                                        AntiGate.ReportBad(AntiGate.LastCaptchaId);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //System.IO.File.AppendAllLines(fileName, new string[] { us.Id.ToString() });   
                        }
                    }
                    repearcount++;
                    Thread.Sleep(60000);
                }
            }
        }

        private static List<User> sort(ReadOnlyCollection<User> friend)
        {
            List<User> res = new List<User>();
            List<User> online = new List<User>();
            List<User> notOnline = new List<User>();
            foreach(User us in friend)
            {
                if ((bool)us.Online)
                {
                    online.Add(us);
                }
                else
                {
                    notOnline.Add(us);
                }
            }
            res.AddRange(online);
            res.AddRange(notOnline);

            return res;
        }

        public class AddNewFriend : UserTask
        {
            VkApi vk;
            private User user;

            public AddNewFriend(VkApi vk, User user)
            {
                this.vk = vk;
                this.user = user;
            }
            
            public void execute()
            {
                try
                {
                    vk.Friends.Add(user.Id, user.FirstName + ", привет. Можно с тобой познакомиться?)");

                    System.Console.WriteLine("Request is sended to user with id = " + user.Id);
                   
                }
                catch (CaptchaNeededException ex)
                {
                    try
                    {
                        System.Console.WriteLine("Oopps, catchaaaa =(((((");
                        string captchaAnswer = AntiGate.Recognize(ex.Img.AbsoluteUri);
                        System.Console.WriteLine("catcha is " + captchaAnswer);
                        vk.Friends.Add(user.Id, user.FirstName + ", привет. Можно с тобой познакомиться?)", ex.Sid, captchaAnswer);

                        System.Console.WriteLine("Request is sended to user with id = " + user.Id);
                    }
                    catch (CaptchaNeededException ex2)
                    {
                        AntiGate.ReportBad(AntiGate.LastCaptchaId);
                    }
                }
            }
        }
    }

    public interface UserTask
    {
        void execute();
    }
}