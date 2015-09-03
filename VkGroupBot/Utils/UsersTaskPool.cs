using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkGroupBot.Treeview;
using VkNet.Enums;

namespace VkGroupBot.Utils
{
    class UsersTaskPool
    {
        private static Dictionary<string, Thread> timersPool = new Dictionary<string, Thread>();
        
        public static void startNewTask(VkUser user)
        {
            if (!timersPool.ContainsKey(user.email))
            {
                Thread checkForTime = new Thread(new TimeHandler(user.email, user.password, Sex.Male).doTask);
                checkForTime.IsBackground = true;
                timersPool.Add(user.email, checkForTime);
                checkForTime.Start();
            }
        }

        public static void stopTimer(VkUser user)
        {
            if (timersPool.ContainsKey(user.email))
            {
                timersPool[user.email].Abort();
                timersPool[user.email] = null;
                timersPool.Remove(user.email);
            }
        }

        class TimeHandler
        {
            private string _uid;
            private string _pass;
            private Sex sex;
            public TimeHandler(string uid, string pass, Sex sex)
            {
                this._uid = uid;
                this._pass = pass;
                this.sex = sex;
            }
            public void doTask(object sender)
            {
                new UserTaskHelper(_uid, _pass, sex).start();
                
            }
        }
    }
}
