using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;

namespace VkGroupBot.Utils
{
    public class VkApiFactory
    {
        private static VkApiFactory instance;
        VkApi vk;
        int appID = 4983267;    	// ID приложения  
        private const long interval1day = 60 * 60 * 1000 * 24;
        string email = "dogs_heart14@mail.ru";        	// email или телефон
        string pass = "accfake14";              	// пароль для авторизации
        Settings scope = Settings.All;  	// Права доступа приложения
        Timer timer;

        private static Dictionary<string, VkApiWrapper> timersPool = new Dictionary<string, VkApiWrapper>();
        

        private VkApiFactory()
        {            
            
            vk = new VkApi();
            vk.isDefaultVkApi = true;
            vk.email = email;
            vk.Authorize(appID, email, pass, scope);
            timer = new Timer(relogin, null, interval1day, interval1day);
        }


        private void relogin(object sender)
        {
            vk = new VkApi();
            vk.Authorize(appID, email, pass, scope);
        }
        public static VkApiFactory getInstance()
        {
            if (instance == null)
            {
                instance = new VkApiFactory();
            }
            return instance;
        }

        public VkApi getDefaultVkApi()
        {
            return vk;
        }


        public VkApi getVkApi(string email, string pass, Settings settings)
        {
            if(timersPool.ContainsKey(email))
            {
                return timersPool[email].vk;
            } else 
            {
                vk = new VkApi();
                vk.isDefaultVkApi = false;
                vk.email = email;
                vk.Authorize(appID, email, pass, settings);
                VkApiWrapper wrap = new VkApiWrapper();
                wrap.vk = vk;
                wrap.password = pass;
                wrap.settings = settings;
                timersPool[email] = wrap;
                return vk;
            }
          
        }

        class VkApiWrapper
        {
            public VkApi vk{get;set;}
            public string password{get;set;}
            public Settings settings { get; set; }
        }
    }
}
