using NLog;
using SimpleAntiGate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            VkApi vk = VkApiFactory.getInstance().getVkApi(email, password, Settings.All);
            int ignor;
            IReadOnlyCollection<User> users = vk.Users.SearchAdvanced(out ignor, sex, 16, ProfileFields.All, 150, 50);
            tasks = new List<UserTask>();

            int count = 0;
            foreach (User user in users)
            {
                if (count == 50)
                {
                    logger.Debug("End.......");
                    break;
                }
                System.Console.WriteLine();
                logger.Debug("trying to add new user with id = " + user.Id);
                if (validateUser(user))
                {
                    tasks.Add(new UserTaskManager.AddNewFriend(vk, user));
                    count++;
                }
            }
            User us = vk.Users.Get((long)vk.UserId, ProfileFields.All, null);
            for (int z = 0; z < 40; z++)
            {
                tasks.Add(new UserTaskManager.InviteFriendInGroup(vk, us, 95032731));
            }
            for (int z = 0; z < 12; z++)
            {
                tasks.Add(new UserTaskManager.LikeToFriend(vk, us));
            }
            for (int z = 0; z < 5; z++)
            {
                tasks.Add(new UserTaskManager.JoinInSomeGroup(vk, us));
            }
            for (int z = 0; z < 6; z++)
            {
                tasks.Add(new UserTaskManager.RepostFromSomeGroup(vk, us));
            }
            for (int z = 0; z < 8; z++)
            {
                tasks.Add(new UserTaskManager.RandomMessageToRandomFriend(vk, us));
            }

           

        }

        public void start()
        {
            new UserTaskManager(tasks).start();
        }

        private bool validateUser(User user)
        {


            if (user.BirthDate == null || user.Online == false)
            {
                logger.Debug("User is not valid online=" + user.Online + " friendCount=" + user.Counters);
                return false;
            }
            logger.Debug("User is valid");
            return true;

        }

    }

  

    public class UserTaskManager
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        
        private const int interval = 60000 * 5;
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
                    logger.Debug("Join in some group");
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
                logger.Debug("Reposting something from some group");
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
                logger.Debug("Likes photo of friend");
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
                    alreadyUsedIds = new string[] { };
                }
                else
                {
                    alreadyUsedIds = System.IO.File.ReadAllLines(fileName);
                }
                logger.Debug("Invited friend to group");
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

                    logger.Debug("Request is sended to user with id = " + user.Id);

                }
                catch (CaptchaNeededException ex)
                {
                    try
                    {
                        logger.Error("Oopps, catchaaaa =(((((");
                        string captchaAnswer = AntiGate.Recognize(ex.Img.AbsoluteUri);
                        logger.Error("catcha is " + captchaAnswer);
                        vk.Friends.Add(user.Id, user.FirstName + ", привет. Можно с тобой познакомиться?)", ex.Sid, captchaAnswer);

                        logger.Error("Request is sended to user with id = " + user.Id);
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
