namespace Lottery
{
    public class User
    {
        public static int num = 0;
        public static bool flag = true;
        public User(string name, int star, int end)
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
            ID = num;
            Name = name;
            Star = star;
            End = end;
        }

        public int ID { get; set; }
        public string Name{get;set;}
        public int Star { get; set; }
        public int End { get; set; }
    }
}