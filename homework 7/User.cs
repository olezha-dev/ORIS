using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class User
    {
        string Name { get; set; }
        string Group { get; set; }
        bool Sex { get; set; }
        int Arg { get; set; }
        public User(string name, string group, int arg, bool sex)
        {
            Name = name;
            Group = group;
            Arg = arg;
            Sex = sex;
        }
    }
}
