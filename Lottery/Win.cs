using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery
{
   public class Win
    {
        public static int num = 0;
        public static bool flag = true;
        public Win(string awards, string userName)
        {
            if (flag)
            {
                num = 1;
                flag = false;
            }
            else
            {
                num += 1;
            }
            Id = num;
            Awards = awards;
            UserName = userName;
        }

        public int Id { get; set; }
        public string Awards { get; set; }
        public string UserName { get; set; }
    }
}
