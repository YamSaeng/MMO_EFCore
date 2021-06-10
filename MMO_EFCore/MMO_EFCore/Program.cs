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
            Console.WriteLine("[1] ShowItems");
            Console.WriteLine("[2] Eagar Loading");
            Console.WriteLine("[3] Explicit Loading");
            Console.WriteLine("[4] Select Loading");

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
                        DBCommands.ShowItems();
                        break;
                    case "2":
                        DBCommands.EagarLoading();
                        //DBCommands.UpdateDate();
                        break;
                    case "3":
                        DBCommands.ExplicitLoading();
                        //DBCommands.DeleteItem();
                        break;
                    case "4":
                        DBCommands.SelectLoading();
                        break;
                }
            }
        }
    }
}
