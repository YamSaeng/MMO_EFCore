using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMO_EFCore
{
    class DBCommands
    {
        //초기화할때 시간 소모 좀 잇음
        public static void InitializeDB(bool ForceReset = false)
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

                //테스트 데이터 생성
                CreateTestData(DB);
                Console.WriteLine("DB Initialized");
            }
        }

        public static void CreateTestData(AppDbContext DB)
        {
            CPlayer Player = new CPlayer()
            {
                Name = "SungWon"
            };

            List<CItem> Items = new List<CItem>()
            {
                new CItem()
                {
                    TemplateId = 101,
                    CreateDate = DateTime.Now,
                    Owner = Player
                },
                new CItem()
                {
                    TemplateId = 102,
                    CreateDate = DateTime.Now,
                    Owner = Player
                },
                new CItem()
                {
                    TemplateId = 103,
                    CreateDate = DateTime.Now,
                    Owner = new CPlayer() { Name = "YamSaeng"}
                }
            };

            DB._Items.AddRange(Items);
            DB.SaveChanges(); //DB에 저장
        }

        public static void ReadAll()
        {
            using (AppDbContext DB = new AppDbContext())
            {
                // AsNoTracking 데이터를 읽기전용으로 읽어들인다.
                // Entity Framework 에서 DB._Items에 변화가 있는지 감시(= 데이터 변경 탐지)하는 Tracking Snapshot이라는 작업을 수행하는데
                // AsNoTracking를 붙여주면 아래 작업을 할때에는 감시할 필요 없다고 알려주는 기능
                // 기본적으로 외부키로 선언한 테이블에 대해서 직접적으로 접근하면 null로 체크 되어 있어서
                // 접근 할 시 에러가 난다. 
                // 따라서 아래처럼 forecah문에서 접근하고 싶으면 Include를 이용해서 
                // Include : Eager Loading (즉시 로딩)                 
                foreach (CItem Item in DB._Items.AsNoTracking().Include(A => A.Owner))
                {
                    Console.WriteLine($"TemplateId ({Item.TemplateId}) Owner({Item.Owner.Name}) CreateTime({Item.CreateDate})");
                }
            }
        }

        //특정 플레이어가 소지한 아이템들의 CreateDate를 수정
        public static void UpdateDate()
        {
            Console.WriteLine("Input Player Name");
            Console.WriteLine("> ");
            string Name = Console.ReadLine();

            using (AppDbContext DB = new AppDbContext())
            {
                // 입력한 이름과 같은 사람의 아이템들을 가져오고
                var Items = DB._Items.Include(P => P.Owner).Where(P => P.Owner.Name == Name);                                

                // 아이템 목록 시간을 갱신해주고
                foreach(CItem Item in Items)
                {
                    Item.CreateDate = DateTime.Now;
                }

                //반영한다.
                DB.SaveChanges();
            }

            //데이터 출력해봄
            ReadAll();
        }

        public static void DeleteItem()
        {
            Console.WriteLine("Input Player Name");
            Console.Write("> ");
            string Name = Console.ReadLine();

            using (AppDbContext DB = new AppDbContext())
            {
                // 입력한 이름과 같은 사람의 아이템들을 가져오고
                var Items = DB._Items.Include(P => P.Owner).Where(P => P.Owner.Name == Name);
                //삭제할 아이템 목록 담고
                DB._Items.RemoveRange(Items);
                //반영한다.
                DB.SaveChanges();
            }

            //데이터 출력해봄
            ReadAll();
        }
    }
}
