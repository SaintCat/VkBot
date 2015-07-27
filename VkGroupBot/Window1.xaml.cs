using NLog;
using NLog.Config;
using NLog.Fluent;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VkGroupBot.Treeview;
using VkGroupBot.Utils;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VkGroupBot
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        VkGroup currentlySelectedGroup;
        VkUser currentlySelectedUser;
        private const string ConsoleTargetName = "WpfConsole";
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public Window1()
        {
            InitializeComponent();
            
            List<VkGroupCategory> headCategories = new List<VkGroupCategory>();
            VkGroupCategory person1 = new VkGroupCategory() { Name = "Фото", isGroup = false };
            person1.IconSource = person1.GetIconSource();
            VkGroupCategory person2 = new VkGroupCategory() { Name = "Юмор", isGroup = false };
            person2.IconSource = person2.GetIconSource();
            VkGroup child1 = new VkGroup() { Name = "Лучший юмор", isGroup = true, groupId = 98013659 };
            child1.IconSource = child1.GetIconSource();
            person2.Children.Add(child1);

            VkGroup test = new VkGroup() { Name = "Лучшие фото", isGroup = true, groupId = 98738124 };
            test.IconSource = test.GetIconSource();
            person1.Children.Add(test);
            test = new VkGroup() { Name = "VkBoTest", isGroup = true, groupId = 97462940 };
            test.IconSource = test.GetIconSource();
            
            person1.Children.Add(test);
           

            headCategories.Add(person1);
            headCategories.Add(person2);

            person1.IsExpanded = true;
            person2.IsExpanded = true;
           
            categoriesTreeView.ItemsSource = headCategories;
            List<VkUser> userCategories = new List<VkUser>();
            VkUser user3 = new VkUser() { Name = "User3", email = "dogs_heart18@mail.ru", password = "accfake18" };


            
            userCategories.Add(user3);
            usersTreeView.ItemsSource = userCategories;

         
                logger.Info("Application started");
            
          
        }

        private void MainWindowInitialized(object sender, EventArgs e)
        {
            // Set Log Target
            AsyncTargetWrapper _wrapper;
            // https://nlog.codeplex.com/workitem/6272
            var target = new WpfRichTextBoxTarget
            {
                Name = ConsoleTargetName,
                Layout = "${time:}|${threadid:padding=3}|${level:uppercase=true:padding=-5}|${logger:padding=-15}|${message}|${exception}",
                ControlName = TextLog.Name,
                FormName = Name,
                AutoScroll = true,
                MaxLines = 100000,
                UseDefaultRowColoringRules = true
            };
            _wrapper = new AsyncTargetWrapper
            {
                Name = ConsoleTargetName,
                WrappedTarget = target
            };
            //SimpleConfigurator.ConfigureForFileLogging("BotLog.txt", LogLevel.Trace);
            SimpleConfigurator.ConfigureForTargetLogging(_wrapper, LogLevel.Trace);
        }

        private void categoriesTreeView_Expanded(object sender, RoutedEventArgs e)
        {
            if (categoriesTreeView.SelectedItem is VkGroup)
            {
                VkGroup item = (VkGroup)categoriesTreeView.SelectedItem;
                currentlySelectedGroup = item;
                Group group = VkGroupManager.getInstance().getGroupById(item.groupId);
                nameLabel.Text = group.Name;
                statusLabel.Content = group.Status;
                Hyperlink link = new Hyperlink();
                link.NavigateUri = new Uri("http://vk.com/" + group.ScreenName);
                link.RequestNavigate += RequestNavigateEventHandler;

                link.Inlines.Add("vk.com/" + group.ScreenName);
                linkLabel.Inlines.Clear();
                linkLabel.Inlines.Add(link);
                image.Source = ByteToImage(group.photoBigSource);
                autoPostingCheckBox.IsChecked = item.autoPostingOn;
            }
            else
            {
                currentlySelectedGroup = null;
            }
          

        }

        void RequestNavigateEventHandler(object sender, RequestNavigateEventArgs e)
                {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)); e.Handled = true; 
            
                }

        private void Hyperlink_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            Process.Start(hyperlink.NavigateUri.ToString());
        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }

        private void autoPostingCheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (currentlySelectedGroup != null)
            {
                currentlySelectedGroup.autoPostingOn = (bool)autoPostingCheckBox.IsChecked;
                if (currentlySelectedGroup.autoPostingOn)
                {
                    TimersPool.startNewTask(currentlySelectedGroup.groupId);
                }
                else
                {
                    TimersPool.stopTimer(currentlySelectedGroup.groupId);
                }
            }
        }

        private void usersTreeView_Selected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

                VkUser item = (VkUser)usersTreeView.SelectedItem;
                currentlySelectedUser = item;
                VkApi vk = VkApiFactory.getInstance().getVkApi(item.email, item.password, Settings.All);
                User group = vk.Users.Get((long)vk.UserId, ProfileFields.All);
                nameLabel2.Text = group.FirstName + " " + group.LastName;
                statusLabel2.Content = group.Status;
                Hyperlink link = new Hyperlink();
                link.NavigateUri = new Uri("http://vk.com/" + group.Domain);
                link.RequestNavigate += RequestNavigateEventHandler;

                link.Inlines.Add("vk.com/" + group.Domain);
                linkLabel2.Inlines.Clear();
                linkLabel2.Inlines.Add(link);
                //image.Source = ByteToImage(group.PhotoPreviews.Photo400);
                autoPostingCheckBox2.IsChecked = item.workIsOn;
           
        }

        private void autoPostingCheckBox2_Checked_1(object sender, RoutedEventArgs e)
        {
            if (currentlySelectedUser != null)
            
            {
                logger.Error("Start new user taskk");
                currentlySelectedUser.workIsOn = (bool)autoPostingCheckBox2.IsChecked;
                if (currentlySelectedUser.workIsOn)
                {
                    UsersTaskPool.startNewTask(currentlySelectedUser);
                }
                else
                {
                    UsersTaskPool.stopTimer(currentlySelectedUser);
                }
            }
        }
    }
    
}
