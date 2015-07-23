using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkGroupBot.Treeview
{
    class VkGroup : VkGroupCategory
    {

        public long groupId { get; set; }
        public Boolean autoPostingOn { get; set; }
    }
}
