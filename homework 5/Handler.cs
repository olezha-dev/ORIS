using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    public class Handler
    {
        public string Handle(string? login, string? password)
        {
            if (login == "olezha" && password == "12354")
            {
                Console.WriteLine("вход совершился");
                return "Вход разрешён";
            }

            Console.WriteLine($"Логин: {login}");
            Console.WriteLine($"Пароль: {password}");
            Console.WriteLine("Данные получены");
            return "Данные получены";
        }
    }
}
