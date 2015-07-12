using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using VkGroupBot.treeview;

namespace VkGroupBot.Treeview
{
  
    public class VkGroupCategory : TreeViewItemBase
    {
        private static Bitmap _vkGroupIcon = Properties.Resources.open_folder;
        private static Bitmap _vkGroupCategoryIcon = Properties.Resources.personal;

        public VkGroupCategory()
        {
            this.Children = new ObservableCollection<VkGroupCategory>();
         
        }

        public string Name { get; set; }

        public ObservableCollection<VkGroupCategory> Children { get; set; }
        public Boolean isGroup { get; set; }
        public BitmapSource IconSource { get; set; }
        public BitmapSource GetIconSource()
        {
            return loadBitmap(!isGroup ? _vkGroupIcon : _vkGroupCategoryIcon);
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }
    }
}
