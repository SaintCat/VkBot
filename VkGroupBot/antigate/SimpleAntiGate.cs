using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace SimpleAntiGate
{
    /// <summary>
    /// Класс SimpleAntiGate - простой класс, служащий для распознавания каптчи, используя сервис AntiGate.com<br /> 
    /// Для работы с HTTP-протоколом используется класс WebClient<br />
    /// Автор: Geograph<br />
    /// Дата: 14.05.2014<br />
    /// Сайт: http://geograph.us<br />
    /// E-mail: geograph@list.ru<br />
    /// </summary>
    public static class AntiGate
    {
        private const int TryCount = 20;
        private const int WaitMillisecBeforeRequest = 3 * 1000;

        /// <summary>
        /// Сервер для отправки капчи на распознавание. 
        /// </summary>
        public static string AntiGateServer = "antigate.com";
        /// <summary>
        /// Ваш API ключ с сайта AntiGate.com.<br />
        /// Можно указать его один раз здесь и больше не указывать в функциях.
        /// </summary>
        public static string AntiGateKey;
        /// <summary>
        /// Номер капчи из последнего запроса
        /// </summary>
        public static string LastCaptchaId;

        /// <summary>
        /// Получить ваш текущий баланс
        /// </summary>
        /// <param name="antiGateKey">Ваш API ключ с сайта AntiGate.com</param>
        /// <returns>Возвращает баланс в виде строки</returns>
        public static string GetBalance(string antiGateKey = null)
        {
            antiGateKey = antiGateKey ?? AntiGateKey;
            string result;
            using (var webClient = new WebClient())
            {
                result = webClient.DownloadString(string.Format("http://{0}/res.php?key={1}&action=getbalance", AntiGateServer, antiGateKey));
            }
            return result;
        }

        /// <summary>
        /// Пожаловаться на неправильно разгаданный текст
        /// </summary>
        /// <param name="captchaId">Номер капчи, которая была разгадана не верно</param>
        /// <param name="antiGateKey">Ваш API ключ с сайта AntiGate.com</param>
        /// <returns>Возвращает ответ от сервера</returns>
        public static string ReportBad(string captchaId, string antiGateKey = null)
        {
            antiGateKey = antiGateKey ?? AntiGateKey;
            string result;
            using (var webClient = new WebClient())
            {
                result = webClient.DownloadString(string.Format("http://{0}/res.php?key={1}&action=reportbad&id={2}", AntiGateServer, antiGateKey, captchaId));
            }
            return result;
        }

        /// <summary>
        /// Распознать картинку из потока Stream
        /// </summary>
        /// <param name="imageStream">Поток Stream, содержащий картинку</param>
        /// <param name="minLen">Минимальная длина текста</param>
        /// <param name="maxLen">Максимальная длина текста</param>
        /// <param name="isNumeric">Капча состоит только из цифр</param>
        /// <param name="isPhrase">Капча состоит из нескольких слов</param>
        /// <param name="isRegSense">Капча чувствительна к регистру букв</param>
        /// <param name="isCalc">Цифры на капче нужно сосчитать</param>
        /// <param name="isRussian">В капчи присутствует только русский текст</param>
        /// <param name="antiGateKey">Ваш API ключ с сайта AntiGate.com</param>
        /// <returns>Возвращает распознанный текст или текст ошибки</returns>
        public static string Recognize(Stream imageStream, int minLen = 0, int maxLen = 0,
            bool isNumeric = false, bool isPhrase = false, bool isRegSense = false, bool isCalc = false, bool isRussian = false,
            string antiGateKey = null)
        {
            antiGateKey = antiGateKey ?? AntiGateKey;
            byte[] imageData;
            var buffer = new byte[16 * 1024];
            using (var stream = new MemoryStream())
            {
                int read;
                while ((read = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, read);
                }
                imageData = stream.ToArray();
            }

            var result = Recognize(imageData, minLen, maxLen, isNumeric, isPhrase, isRegSense, isCalc, isRussian, antiGateKey);

            return result;
        }

        /// <summary>
        /// Распознать картинку из объекта Image
        /// </summary>
        /// <param name="image">Объект типа Image, содержащий картинку</param>
        /// <param name="minLen">Минимальная длина текста</param>
        /// <param name="maxLen">Максимальная длина текста</param>
        /// <param name="isNumeric">Капча состоит только из цифр</param>
        /// <param name="isPhrase">Капча состоит из нескольких слов</param>
        /// <param name="isRegSense">Капча чувствительна к регистру букв</param>
        /// <param name="isCalc">Цифры на капче нужно сосчитать</param>
        /// <param name="isRussian">В капчи присутствует только русский текст</param>
        /// <param name="antiGateKey">Ваш API ключ с сайта AntiGate.com</param>
        /// <returns>Возвращает распознанный текст или текст ошибки</returns>
        public static string Recognize(Image image, int minLen = 0, int maxLen = 0,
            bool isNumeric = false, bool isPhrase = false, bool isRegSense = false, bool isCalc = false, bool isRussian = false,
            string antiGateKey = null)
        {
            antiGateKey = antiGateKey ?? AntiGateKey;
            byte[] imageData;

            using (var stream = new MemoryStream())
            {
                image.Save(stream, image.RawFormat);
                imageData = stream.ToArray();
            }

            var result = Recognize(imageData, minLen, maxLen, isNumeric, isPhrase, isRegSense, isCalc, isRussian, antiGateKey);

            return result;
        }

        /// <summary>
        /// Распознать картинку по ссылке или из файла на диске
        /// </summary>
        /// <param name="imageUrlOrFile">Ссылка на картинку капчи или полный путь до картинки на диске</param>
        /// <param name="cookies">Можно передать в функцию куки, для открытия капчи по ссылке, когда это необходимо</param>
        /// <param name="minLen">Минимальная длина текста</param>
        /// <param name="maxLen">Максимальная длина текста</param>
        /// <param name="isNumeric">Капча состоит только из цифр</param>
        /// <param name="isPhrase">Капча состоит из нескольких слов</param>
        /// <param name="isRegSense">Капча чувствительна к регистру букв</param>
        /// <param name="isCalc">Цифры на капче нужно сосчитать</param>
        /// <param name="isRussian">В капчи присутствует только русский текст</param>
        /// <param name="antiGateKey">Ваш API ключ с сайта AntiGate.com</param>
        /// <returns>Возвращает распознанный текст или текст ошибки</returns>
        public static string Recognize(string imageUrlOrFile, string cookies = null, int minLen = 0, int maxLen = 0,
            bool isNumeric = false, bool isPhrase = false, bool isRegSense = false, bool isCalc = false, bool isRussian = false,
            string antiGateKey = null)
        {
            antiGateKey = antiGateKey ?? AntiGateKey;
            byte[] imageData;
            if (imageUrlOrFile.Contains("://"))
            {
                using (var webClient = new WebClient())
                {
                    if (cookies != null) webClient.Headers.Add("Cookie", cookies);
                    imageData = webClient.DownloadData(imageUrlOrFile);
                }
            }
            else
            {
                if (!File.Exists(imageUrlOrFile)) return "ERROR_FILE_NOT_FOUND";
                imageData = File.ReadAllBytes(imageUrlOrFile);
            }
            var result = Recognize(imageData, minLen, maxLen, isNumeric, isPhrase, isRegSense, isCalc, isRussian, antiGateKey);

            return result;
        }

        /// <summary>
        /// Распознать картинку из массива байт
        /// </summary>
        /// <param name="imageData">Массив байт содержащий картинку</param>
        /// <param name="minLen">Минимальная длина текста</param>
        /// <param name="maxLen">Максимальная длина текста</param>
        /// <param name="isNumeric">Капча состоит только из цифр</param>
        /// <param name="isPhrase">Капча состоит из нескольких слов</param>
        /// <param name="isRegSense">Капча чувствительна к регистру букв</param>
        /// <param name="isCalc">Цифры на капче нужно сосчитать</param>
        /// <param name="isRussian">В капчи присутствует только русский текст</param>
        /// <param name="antiGateKey">Ваш API ключ с сайта AntiGate.com</param>
        /// <returns>Возвращает распознанный текст или текст ошибки</returns>
        public static string Recognize(byte[] imageData, int minLen = 0, int maxLen = 0,
            bool isNumeric = false, bool isPhrase = false, bool isRegSense = false, bool isCalc = false, bool isRussian = false,
            string antiGateKey = null)
        {
            antiGateKey = antiGateKey ?? AntiGateKey;
            var postValues = new NameValueCollection
            {
                { "key", antiGateKey },
                { "method", "base64" },
                { "soft_id", "597" },
                { "body", Convert.ToBase64String(imageData) },
            };
            if (minLen > 0) postValues.Add("min_len", minLen.ToString());
            if (maxLen > 0) postValues.Add("max_len", maxLen.ToString());
            if (isNumeric) postValues.Add("numeric", "1");
            if (isPhrase) postValues.Add("phrase", "1");
            if (isRegSense) postValues.Add("regsense", "1");
            if (isCalc) postValues.Add("calc", "1");
            if (isRussian) postValues.Add("is_russian", "1");

            var result = "";
            using (var webClient = new WebClient())
            {
                for (var i = 0; i < TryCount; i++)
                {
                    result = Encoding.UTF8.GetString(webClient.UploadValues("http://" + AntiGateServer + "/in.php", postValues));
                    if (result.Contains("OK|")) break;
                    if (result.Contains("ERROR_NO_SLOT_AVAILABLE"))
                    {
                        Thread.Sleep(WaitMillisecBeforeRequest);
                        continue;
                    }
                    if (result.Contains("ERROR_")) return result;
                    if (!result.Contains("OK|")) return "UNKNOWN_ERROR: " + result;
                }
                if (result.Contains("ERROR_")) return result;
                var captchaId = result.Replace("OK|", "").Trim();
                LastCaptchaId = captchaId;

                for (var i = 0; i < TryCount; i++)
                {
                    Thread.Sleep(WaitMillisecBeforeRequest);
                    result = webClient.DownloadString(string.Format("http://{0}/res.php?key={1}&action=get&id={2}", AntiGateServer, antiGateKey, captchaId));
                    if (result.Contains("ERROR_NO_SLOT_AVAILABLE")) continue;
                    if (result.Contains("ERROR_")) return result;
                    if (result.Contains("OK|")) return result.Replace("OK|", "").Trim();
                }
            }

            return result;
        }
    }
}
