using ConsoleApp1;

string teamplate = "<p>Имя: ${user.name}<p/> <p>Имя: ${user.name}<p/> <p>Имя: ${user.name}<p/> <p>Имя: ${user.name}<p/>";

User user = new User ( "Ваня", "11-541", 22, true );
string result = teamplate.RenderForm;


Pen pen = new Pen("","", 1, 100);
Console.WriteLine(result);
