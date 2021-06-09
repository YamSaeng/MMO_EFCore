using System;

namespace MMO_EFCore
{
    class Program
    {
        static void Main(string[] args)
        {
            DBCommands.InitializeDB(ForceReset: false);

            Console.WriteLine("명령어 입력 : ");
            Console.WriteLine("[0] Force Reset");
            Console.WriteLine("[1] ReadAll");
            Console.WriteLine("[2] ShowItems");

            while (true)
            {
                Console.Write("> ");
                string Command = Console.ReadLine();
                switch(Command)
                {
                    case "0":
                        DBCommands.InitializeDB(ForceReset: true);
                        break;
                    case "1":
                        DBCommands.ReadAll();
                        break;
                    case "2":
                        DBCommands.ShowItems();
                        //DBCommands.UpdateDate();
                        break;
                    case "3":
                        //DBCommands.DeleteItem();
                        break;
                }
            }
        }
    }
}
