using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace MMO_EFCore
{
    class Program
    {
        //초기화할때 시간 소모 좀 잇음
        static void InitializeDB(bool ForceReset = false)
        {
            //using문을 사용해서 자동으로 리소스 해제 하는 부분은 Dispose가 호출되도록 유도한다.
            using (AppDbContext DB = new AppDbContext())
            {
                //DB가 이미 만들어져 있는지 확인한다.
                //ForceReset가 false면 진입 true면 탈출 
                //ForceReset가 true면 강제로 밀어버리고 DB 새로 생성
                if (!ForceReset && (DB.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
                {
                    return;
                }

                //이전 데이터를 밀어버리고
                DB.Database.EnsureDeleted();
                //DB 새로 생성
                DB.Database.EnsureCreated();

                Console.WriteLine("DB Initialized");
            }
        }

        static void Main(string[] args)
        {
            InitializeDB(ForceReset: true);
        }
    }
}
