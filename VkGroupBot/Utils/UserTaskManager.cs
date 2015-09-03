using NLog;
using SimpleAntiGate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace VkGroupBot.Utils
{
    
    public class UserTaskHelper
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        List<UserTask> tasks;

        public UserTaskHelper(string email, string password, Sex sex)
        {
            AntiGate.AntiGateKey = "d14f05f2dbfc0bcb02e7ddfc72785e51";
            VkApi vk = VkApiFactory.getInstance().getDefaultVkApi();
           
            int ignor;
            tasks = new List<UserTask>();

            for (int z = 0; z < 50; z++)
            {
                  tasks.Add(new UserTaskManager.AddNewFriend(vk, sex));
            }
            User us = vk.Users.Get((long)vk.UserId, ProfileFields.All, null);
            for (int z = 0; z < 20; z++)
            {
                //tasks.Add(new UserTaskManager.InviteFriendInGroup(vk, us, 98738124));
            }
            for (int z = 0; z < 20; z++)
            {
                //tasks.Add(new UserTaskManager.InviteFriendInGroup(vk, us, 98013659));
            }
            for (int z = 0; z < 12; z++)
            {
                tasks.Add(new UserTaskManager.LikeToFriend(vk, us));
            }
            for (int z = 0; z < 8; z++)
            {
                //tasks.Add(new UserTaskManager.JoinInSomeGroup(vk, us));
            }
            for (int z = 0; z < 3; z++)
            {
            tasks.Add(new UserTaskManager.RepostFromSomeGroup(vk, us));
           }
           for (int z = 0; z < 8; z++)
           {
                //tasks.Add(new UserTaskManager.RandomMessageToRandomFriend(vk, us));
           }

           Thread myThread = new System.Threading.Thread(delegate()
           {
               startBot(vk);
           });
           myThread.IsBackground = true;
           myThread.Start();

        }


        private static List<Message> getUnreadedMessages(ReadOnlyCollection<Message> messages)
        {
            List<Message> res = new List<Message>();
            foreach (Message m in messages)
            {
                if (m.ReadState == MessageReadState.Unreaded && m.ChatId == null)
                {
                    if (m.Date.Value.DayOfYear == System.DateTime.Now.DayOfYear)
                    {
                        res.Add(m);
                    }
                }
            }
            return res;
        }

        private static Dictionary<long, int> messageCount = new Dictionary<long, int>();
        static List<string> primes = new List<string>(new string[] {"Кстати, может быть вступишь в группы, которые на моей стене? ты мне очень поможешь",
            "Что насчет того, чтобы вступить в группы на моей стене?))",
        "Мб вступишь в группу на моей стене? Я буду тебе благодарен",
        "Если вступишь в группу на моей стене, я пришлю тебе свое фото :P"});
        static Random rnd = new Random();
        static void startBot(VkApi vk)
        {
             while (true)
            {
                try
                {
                    int total = 0;
                    ReadOnlyCollection<Message> messages = vk.Messages.Get(MessageType.Received, out total);
                    logger.Info("New messages in count = " + total);
                    List<Message> unreaded = getUnreadedMessages(messages);
                    foreach (Message m in unreaded)
                    {
                        try
                        {
                            if (messageCount.ContainsKey((long)m.UserId))
                            {
                                messageCount[(long)m.UserId] = messageCount[(long)m.UserId] + 1;
                            }
                            else
                            {
                                messageCount.Add((long)m.UserId, 1);
                            }
                            string answer = getAnswer((long)vk.UserId, (long)m.UserId, m.Body);
                            answer = answer.Replace("Виу-пиу", "Александр");
                            answer = answer.Replace("<userlink>", "");
                            answer = answer.Replace("</userlink>", "");
                            answer = answer.Replace("</br>", "");
                            answer = answer.Replace("<br>", "");
                            answer = answer.Replace("Инф", "человек");
                            answer = answer.Replace("инф", "человек");
                            answer = answer.Replace("малявка", "совсем не взрослая");
                            answer = answer.Replace("старикашка", "совсем взрослая");
                            vk.Messages.MarkAsRead((long)m.Id);
                            vk.Messages.SetActivity((long)m.UserId);

                            Thread.Sleep(4500);
                            vk.Messages.Send((long)m.UserId, false, answer);
                            if (messageCount[(long)m.UserId] % 30 == 0)
                            {
                                Thread.Sleep(2000);
                                int r = rnd.Next(primes.Count);
                                vk.Messages.Send((long)m.UserId, false, primes[r]);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                        }
                    }
                    Thread.Sleep(2500);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

        }

        private static string getAnswer(long userId, long senderId, String reply)
        {
            string answer = null;
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["id"] = Convert.ToString(senderId);
                values["botid"] = "970c8b3d-2e25-471d-8aab-efc87bcb7155";

                var response = client.UploadValues("http://bot.mew.su/service/getsession.php", values);

                var responseString = Encoding.Default.GetString(response);
                logger.Debug(responseString);

                var values2 = new NameValueCollection();
                values2["session"] = responseString;
                values2["botid"] = Convert.ToString(userId);
                values2["sender"] = Convert.ToString(senderId);
                values2["ischat"] = "0";
                values2["text"] = reply;
                var response2 = client.UploadValues("http://bot.mew.su/service/speak.php", values2);
                answer = Encoding.UTF8.GetString(response2);
                logger.Debug(answer);
            }
            return answer;
        }

        public void start()
        {
            new UserTaskManager(tasks).start();
        }

        private bool validateUser(User user)
        {


            if (user.BirthDate == null || user.Online == false)
            {
                logger.Debug( "User is not valid online=" + user.Online + " friendCount=" + user.Counters);
                return false;
            }
            logger.Debug( "User is valid");
            return true;

        }

    }

  

    public class UserTaskManager
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        
        private const int interval = 60000 * 3;
        private const int shift = 30000;
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
            foreach (UserTask task in _tasks)
            {
                try
                {
                    task.execute();
                    int shiftVal = -shift + r.Next(shift * 2);
                    Thread.Sleep(interval + shiftVal);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
            logger.Error("THIS IS FINALLY ENDED. EHHHHYYYY!!!!!!!!!!!!!!!");
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
                    logger.Debug(vk.email + " " + "Join in some group");
                    int total;
                    ReadOnlyCollection<Group> grous = vk.Groups.Search("Лучшее", out total, counter, 5);
                    foreach (Group g in grous)
                    {
                        if (!(bool)g.IsMember)
                        {
                            vk.Groups.Join(g.Id);
                            joined = true;
                            break;
                        }
                    }
                    counter += 5;
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
                logger.Debug(vk.email + " " + "Reposting something from some group");
                ReadOnlyCollection<Group> groups = vk.Groups.Get(user.Id, true, GroupsFilters.All, GroupsFields.All, 0, 30);
                Random r = new Random();
                int rPos = r.Next(groups.Count);
                Group groupForRepost = groups[rPos];
                int total;
                ReadOnlyCollection<Post> posts = vk.Wall.Get(0, groupForRepost.ScreenName, out total, 20, 0, WallFilter.All);
                int rPost = r.Next(posts.Count);
                Post p = posts[rPost];
                vk.Wall.Repost("wall-" + groupForRepost.Id + "_" + p.Id, ")", null);
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
                logger.Debug(vk.email + " " + "Likes photo of friend");
                ReadOnlyCollection<User> friend = vk.Friends.Get(user.Id);
                if (friend.Count == 0)
                {
                    return;
                }
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
                    alreadyUsedIds = new string[] { };
                }
                else
                {
                    alreadyUsedIds = System.IO.File.ReadAllLines(fileName);
                }
                logger.Debug(vk.email + " " + "Invited friend to group");
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
                    if (friend.Count == 0)
                    {
                        return;
                    }
                    var friend2 = sort(friend);
                    foreach (User us in friend2)
                    {

                        try
                        {
                            if (!alreadyUsedIds.Contains(us.Id.ToString()) && !vk.Groups.IsMember(groupName, us.Id))
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
                                        System.IO.File.AppendAllLines(fileName, new string[] { us.Id.ToString() });
                                        
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            System.IO.File.AppendAllLines(fileName, new string[] { us.Id.ToString() });   
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
            foreach (User us in friend)
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
            private Sex sex;

            public AddNewFriend(VkApi vk, Sex sex)
            {
                this.vk = vk;
                this.sex = sex;
            }

            public void execute()
            {
                int ignor;
                Random rnd = new Random();
                int offset = rnd.Next(100);
                IReadOnlyCollection<User> users2 = vk.Users.SearchAdvanced(out ignor, sex.Equals(Sex.Female) ? Sex.Male : Sex.Female, 16, ProfileFields.All, 15, offset);
                
                User user = users2.ToList<User>()[rnd.Next(users2.Count)];
                
                try
                {
                    
                    vk.Friends.Add(user.Id, user.FirstName + ", привет. Можно с тобой познакомиться?)");

                    logger.Debug(vk.email + " " + "Request is sended to user with id = " + user.Id);

                }
                catch (CaptchaNeededException ex)
                {
                    try
                    {
                        logger.Error(vk.email + " " + "Oopps, catchaaaa =(((((");
                        string captchaAnswer = AntiGate.Recognize(ex.Img.AbsoluteUri);
                        logger.Error(vk.email + " " + "catcha is " + captchaAnswer);
                        vk.Friends.Add(user.Id, user.FirstName + ", привет. Можно с тобой познакомиться?)", ex.Sid, captchaAnswer);

                        logger.Error(vk.email + " " + "Request is sended to user with id = " + user.Id);
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
