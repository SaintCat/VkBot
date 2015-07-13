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
        public Window1()
        {
            InitializeComponent();
            
            List<VkGroupCategory> headCategories = new List<VkGroupCategory>();
            VkGroupCategory person1 = new VkGroupCategory() { Name = "Цитаты", isGroup = false };
            person1.IconSource = person1.GetIconSource();
            VkGroupCategory person2 = new VkGroupCategory() { Name = "Юмор", isGroup = false };
            person2.IconSource = person2.GetIconSource();
            VkGroup child1 = new VkGroup() { Name = "Юморная группа", isGroup = true, groupId = 10639516 };
            child1.IconSource = child1.GetIconSource();
            person2.Children.Add(child1);

            VkGroup test = new VkGroup() { Name = "Собачье сердце", isGroup = true, groupId = 95032731 };
            test.IconSource = test.GetIconSource();
            person1.Children.Add(test);
            test = new VkGroup() { Name = "VkBoTest", isGroup = true, groupId = 97462940 };
            test.IconSource = test.GetIconSource();
            
            person1.Children.Add(test);
           

            VkGroupCategory person3 = new VkGroupCategory() { Name = "Фильмы", isGroup = false };
            person3.IconSource = person1.GetIconSource();

            headCategories.Add(person1);
            headCategories.Add(person2);
            headCategories.Add(person3);

            person2.IsExpanded = true;
            person2.IsSelected = true;

            categoriesTreeView.ItemsSource = headCategories;
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
    }
    
}
