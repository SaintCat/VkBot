using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VkGroupBot.Utils
{
    class VkGroupManager
    {
        private static VkGroupManager instance;
        private static int MAX_SIZE_FOR_GROUP_CACHE = 150;
        private LimitedSizeDictionary<long, Group> groupsCache;
        private VkGroupManager()
        {
            groupsCache = new LimitedSizeDictionary<long, Group>(MAX_SIZE_FOR_GROUP_CACHE);
        }

        public static VkGroupManager getInstance()
        {
            if (instance == null)
            {
                instance = new VkGroupManager();
            }
            return instance;
        }

        public Group getGroupById(long uid)
        {
            if (!groupsCache.ContainsKey(uid))
            {
                Group group = VkApiFactory.getInstance().getDefaultVkApi().Groups.GetById(uid, GroupsFields.All);
                groupsCache[uid] = group;
                return group;
            }
            else
            {
                return groupsCache[uid];
            }
        }

    }

    class LimitedSizeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        Dictionary<TKey, TValue> dict;
        Queue<TKey> queue;
        int size;

        public LimitedSizeDictionary(int size)
        {
           
            this.size = size;
            dict = new Dictionary<TKey, TValue>(size + 1);
            queue = new Queue<TKey>(size);
        }

        public void Add(TKey key, TValue value)
        {
            dict.Add(key, value);
            
            if (queue.Count == size)
                dict.Remove(queue.Dequeue());
            queue.Enqueue(key);
        }

        public bool Remove(TKey key)
        {
            if (dict.Remove(key))
            {
                Queue<TKey> newQueue = new Queue<TKey>(size);
                foreach (TKey item in queue)
                    if (!dict.Comparer.Equals(item, key))
                        newQueue.Enqueue(item);
                queue = newQueue;
                return true;
            }
            else
                return false;
        }
    }
}
