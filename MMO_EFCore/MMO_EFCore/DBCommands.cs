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
            Player SungWon = new Player() { Name = "SungWon" };
            Player WonJi = new Player() { Name = "WonJi" };
            Player YamSaeng = new Player() { Name = "YamSaeng" };

            Player Player = new Player()
            {
                Name = "SungWon"
            };

            List<Item> Items = new List<Item>()
            {
                new Item()
                {
                    TemplateId = 101,
                    CreateDate = DateTime.Now,
                    Owner = SungWon
                },
                new Item()
                {
                    TemplateId = 102,
                    CreateDate = DateTime.Now,
                    Owner = WonJi
                },
                new Item()
                {
                    TemplateId = 103,
                    CreateDate = DateTime.Now,
                    Owner = YamSaeng
                }
            };

            Guild Guild = new Guild()
            {
                GuildName = "A1",
                Members = new List<Player> { SungWon, WonJi, YamSaeng }
            };

            DB._Items.AddRange(Items);
            DB._Guilds.Add(Guild);
            DB.SaveChanges(); //DB에 저장한다.            
        }
                
        public static void EagarLoading()
        {
            // 길드 테이블 추가 후 테스트
            // 특정 길드에 있는 길드원들이 소지한 모든 아이템들을 보고 싶을 때

            Console.WriteLine("길드 이름 입력");
            Console.Write(" > ");
            string Name = Console.ReadLine();

            using (AppDbContext DB = new AppDbContext())
            {
                // 장점 : DB 접근 한 번으로 다 로딩(JOIN)
                // 단점 : 다 필요한지 모르겟는데 로딩시켜주는 부분 ex) 모든 멤버들의 아이템을 다 로딩하는 부분을 의미                
                Guild Guild = DB._Guilds.AsNoTracking()
                    .Where(g => g.GuildName == Name)
                    .Include(g => g.Members) // sql의 select from where에 해당
                    .ThenInclude(p => p.Item) // Include에 대해서 추가적으로 로딩시켜 주고 싶을 대상이 필요할때 사용
                    .First(); // sql로 따지면 top 1과 동치

                foreach(Player player in Guild.Members)
                {
                    Console.WriteLine($"TemplateId({player.Item.ItemId}) Owner ({player.Name})");
                }
                // AsNoTracking 데이터를 읽기전용으로 읽어들인다.
                // Entity Framework 에서 DB._Items에 변화가 있는지 감시(= 데이터 변경 탐지)하는 Tracking Snapshot이라는 작업을 수행하는데
                // AsNoTracking를 붙여주면 아래 작업을 할때에는 감시할 필요 없다고 알려주는 기능
                // 기본적으로 외부키로 선언한 테이블에 대해서 직접적으로 접근하면 null로 체크 되어 있어서
                // 접근 할 시 에러가 난다. 
                // 따라서 아래처럼 forecah문에서 접근하고 싶으면 Include를 이용해서 
                // Include : Eager Loading (즉시 로딩)                 
                foreach (Item Item in DB._Items.AsNoTracking().Include(A => A.Owner))
                {
                    Console.WriteLine($"TemplateId ({Item.TemplateId}) Owner({Item.Owner.Name}) CreateTime({Item.CreateDate})");
                }
            }
        }

        public static void ExplicitLoading()
        {
            // 길드 테이블 추가 후 테스트
            // 특정 길드에 있는 길드원들이 소지한 모든 아이템들을 보고 싶을 때

            Console.WriteLine("길드 이름 입력");
            Console.Write(" > ");
            string Name = Console.ReadLine();

            using (AppDbContext DB = new AppDbContext())
            {
                // 장점 : 필요한 시점에 필요한 데이터만 로딩할 수 있음
                // 단점 : DB 접근 비용
                // 이렇게만 하면 Members가 null 멤버가 가지고 있는 item들도 null로 들어오게된다.
                Guild guild = DB._Guilds
                    .Where(g => g.GuildName == Name)
                    .First();

                // 명시적으로 로딩
                // Guild에 있는 Members를 로딩 시켜주라는 말
                DB.Entry(guild).Collection(g => g.Members).Load();
                
                foreach(Player player in guild.Members)
                {
                    //player에 있는 item를 로딩 시켜주라는 말
                    DB.Entry(player).Reference(p => p.Item).Load();
                }

                foreach (Player player in guild.Members)
                {
                    Console.WriteLine($"TemplateId({player.Item.ItemId}) Owner ({player.Name})");
                }
            }
        }

        // 특정 길드에 있는 길드원 수를 추출
        // 장점 : 필요한 정보만 빼서 로딩
        // 단점 : 일일히 Select 안에 만들어줘야 하는 부분
        public static void SelectLoading()
        {
            Console.WriteLine("길드 이름 입력");
            Console.Write(" > ");
            string Name = Console.ReadLine();

            using (AppDbContext DB = new AppDbContext())
            {
                //SELECT COUNT(*) 처럼 특정 값을 설정해서 추출해줄수 있는 기능
                var Info = DB._Guilds.Where(g => g.GuildName == Name)
                    .Select(g=> new
                    {
                        Name = g.GuildName,
                        MemberCount = g.Members.Count
                    })
                    .First();

                Console.WriteLine($"GuildName : ({Info.Name}), MemberCount({Info.MemberCount})");
            }
        }

        public static void ShowItems()
        {
            Console.WriteLine("플레이어 이름 입력");
            Console.Write(" > ");
            string Name = Console.ReadLine();

            using (AppDbContext DB = new AppDbContext())
            {
                foreach (Player Player in DB._Players.AsNoTracking().Where(p => p.Name == Name).Include(P => P.Item))
                {
                    Console.WriteLine($"{Player.Item.TemplateId}");
                }
            }
        }
        ////특정 플레이어가 소지한 아이템들의 CreateDate를 수정
        //public static void UpdateDate()
        //{
        //    Console.WriteLine("Input Player Name");
        //    Console.WriteLine("> ");
        //    string Name = Console.ReadLine();

        //    using (AppDbContext DB = new AppDbContext())
        //    {
        //        // 입력한 이름과 같은 사람의 아이템들을 가져오고
        //        var Items = DB._Items.Include(P => P.Owner).Where(P => P.Owner.Name == Name);                                

        //        // 아이템 목록 시간을 갱신해주고
        //        foreach(CItem Item in Items)
        //        {
        //            Item.CreateDate = DateTime.Now;
        //        }

        //        //반영한다.
        //        DB.SaveChanges();
        //    }

        //    //데이터 출력해봄
        //    ReadAll();
        //}

        //public static void DeleteItem()
        //{
        //    Console.WriteLine("Input Player Name");
        //    Console.Write("> ");
        //    string Name = Console.ReadLine();

        //    using (AppDbContext DB = new AppDbContext())
        //    {
        //        // 입력한 이름과 같은 사람의 아이템들을 가져오고
        //        var Items = DB._Items.Include(P => P.Owner).Where(P => P.Owner.Name == Name);
        //        //삭제할 아이템 목록 담고
        //        DB._Items.RemoveRange(Items);
        //        //반영한다.
        //        DB.SaveChanges();
        //    }

        //    //데이터 출력해봄
        //    ReadAll();
        //}
    }
}
