using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VkGroupBot.Utils
{
    class TimersPool
    {
        private static Dictionary<long, Timer> timersPool = new Dictionary<long, Timer>();
        private const long interval60Minutes = 40 * 60 * 1000;
        private static readonly object sychronized = new object();
        public static void startNewTask(long uid)
        {
            if(!timersPool.ContainsKey(uid))
            {
            Timer checkForTime = new Timer(new TimeHandler(uid).doTask, null, 0, interval60Minutes);
            timersPool.Add(uid, checkForTime);
            }
        }

        public static void stopTimer(long uid)
        {
            if(timersPool.ContainsKey(uid))
            {
                timersPool[uid].Change(0, System.Threading.Timeout.Infinite);
                timersPool[uid] = null;
                timersPool.Remove(uid);
            }
        }

        class TimeHandler
        {
            private long _uid;
            public TimeHandler(long uid)
            {
                this._uid = uid;
            }
            public void doTask(object sender)
            {
                lock (sychronized)
                {
                    new PostmanHelper(_uid).postNew();    
                }
            }
        }
    }

}
