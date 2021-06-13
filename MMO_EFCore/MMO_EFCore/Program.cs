using Microsoft.EntityFrameworkCore;
using System;

namespace MMO_EFCore
{
    class Program
    {
        // UDF Test
        // Annotation (데이터 주석 방식으로 DB함수로 만들어줌)
        [DbFunction()]
        public static double? GetAverageReviewScore(int ItemID)
        {
            throw new NotImplementedException("사용 금지!"); // c# 에서 호출하면 예외 발생 시키도록
        }

        static void Main(string[] args)
        {
            DBCommands.InitializeDB(ForceReset: false);

            Console.WriteLine("명령어 입력 : ");
            Console.WriteLine("[0] Force Reset");
            Console.WriteLine("[1] Show Item");
            Console.WriteLine("[2] TestUpdateAttack");

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
                        DBCommands.ShowItemsT();
                        //DBCommands.Update_1VN();
                        //DBCommands.TestNullable();
                        //DBCommands.UpdateByReload();
                        break;
                    case "2":
                        DBCommands.TestUpdateAttack();
                        //DBCommands.CalcAverage();
                        //DBCommands.UpdateByFull();
                        //DBCommands.EagarLoading();
                        //DBCommands.UpdateDate();
                        break;
                    case "3":
                        //DBCommands.ExplicitLoading();
                        //DBCommands.DeleteItem();
                        break;
                    case "4":
                        //DBCommands.SelectLoading();
                        break;
                }
            }
        }
    }
}
