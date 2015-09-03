using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VkNet
{
    public class ProxyManager
    {
        private static ProxyManager instance;
        private List<string> proxies;
        private Dictionary<string, string> proxiesReservedMap = new Dictionary<string, string>();
        private List<string> free = new List<string>();
        private ProxyManager()
        {
            proxies = new List<string>();
            //proxies.Add("81.176.228.20:8972:test:i3zYXBSU");
            proxies.Add("91.198.127.241:8085");
            proxies.Add("91.216.3.2:8085");
            proxies.Add("37.230.212.6:8085");
            proxies.Add("93.179.91.244:8085");
            proxies.Add("37.18.42.4:8085");
            
            free.AddRange(proxies);
        }
        public static ProxyManager getInstance()
        {
            if (instance == null)
            {
                instance = new ProxyManager();
            }
            return instance;
        }

        public string reserveOrGet(string uid)
        {
            if (proxiesReservedMap.ContainsKey(uid))
            {
                return proxiesReservedMap[uid];
            }
            while (free.Count == 0)
            {
                Thread.Sleep(2000);
            }
            string freeProxi = free.Last();
            free.Remove(freeProxi);
            proxiesReservedMap[uid] = freeProxi;
            return freeProxi;
        }

        public void unReserve(string uid)
        {
            if (proxiesReservedMap.ContainsKey(uid))
            {
                string nowFreeProxy = proxiesReservedMap[uid];
                proxiesReservedMap.Remove(uid);
                free.Add(nowFreeProxy);
            }
        }
    }
}
