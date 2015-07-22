using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private VkApiFactory()
        {            
            string email = "dogs_heart14@mail.ru";        	// email или телефон
            string pass = "accfake14";              	// пароль для авторизации
            Settings scope = Settings.All;  	// Права доступа приложения

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
            vk = new VkApi();
            vk.Authorize(appID, email, pass, settings);
            return vk;
        }
    }
}
