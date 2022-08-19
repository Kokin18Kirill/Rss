using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rss_фидер
{
    [Serializable]
    public class Setting
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AddressProxy { get; set; }
        public List<string> AddressRss { get; set; }
        public int TimeUpdata { get; set; }

    }
}
