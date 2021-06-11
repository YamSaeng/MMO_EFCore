using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
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

        // 이번 주제 : State (상태)
        // DBContext를 상속받아 관리되는 변수들은 각각 특정 상태값을 가진다.
        // 상태값은 총 5가지가 있는데, 다음과 같다
        // 1) Detached (No Tracking | 추적되지 않는 상태, SaveChanges를 해도 추적하지 않고 있어서 DB에 반영이 안되어 있음) 
        // 2) Unchanged (DB에 있고, 메모리에 들고 있는 해당 변수가 변동사항이 없어서 수정할 필요가 없는 상태, SaveChanges를 해도 값이 그대로니까 아무것도 하지 않음)
        // 3) Deleted (DB에는 아직 있지만, 삭제되어야하는 상태, SaveChanges로 DB에 적용)
        // 4) Modified (DB에 있고, 클라에서 수정(메모리상에서)된 상태, SaveChanges로 DB에 적용)
        // 5) Added (DB에는 아직 없고, 클라에서 생성(메모리상), SaveChanges로 DB에 적용)
        // 1 -> 기본상태

        // SaveChanges 호출하면 실행되면 발생하는 일
        // 1) 추가된 객체들의 상태가 Unchanged로 바뀐다
        // 2) 데이터를 DB에 추가후 ID를 받아와서 해당 객체의 .ID Property를 채워준다.
        // 3) Relationship 참고해서, FK 세팅 및 객체 참조 연결

        // 이미 존재하는 사용자를 연동하려면?
        // 1) Tracked Instance (추적되고 있는 객체)를 얻어온다.
        // 2) 데이터를 연결한다.
        public static void CreateTestData(AppDbContext DB)
        {
            Player SungWon = new Player() { Name = "SungWon" };
            Player WonJi = new Player() { Name = "WonJi" };
            Player YamSaeng = new Player() { Name = "YamSaeng" };

            // 1) SungWon Detached 상태 ( 메모리 상에는 있지만 아직 아무런 연동 작업도 하지 않았기 때문에 Detached 상태 )
            Console.WriteLine(DB.Entry(SungWon).State);

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

            // Test Shadow Property Value Write
            // 숨겨진 프로퍼티 값 가져와서 수정하기
            DB.Entry(Items[0]).Property("RecoveredDate").CurrentValue = DateTime.Now;

            Guild Guild = new Guild()
            {
                GuildName = "A1",
                Members = new List<Player> { SungWon, WonJi, YamSaeng }
            };

            DB._Items.AddRange(Items);
            DB._Guilds.Add(Guild);

            // 2) SungWon Added 상태 ( 메모리 상에 있는 데이터가 위 문장에 있는 명령어로 인해 Add 상태로 바뀌게됨 )
            // 플레이어는 Item에 간접적(외부키)로 선언되어 있지만 EF가 알아서 DB에 넣어주는 작업을 해준다.
            Console.WriteLine(DB.Entry(SungWon).State);

            DB.SaveChanges(); //DB에 저장한다.            

            // 3) SungWon Unchanged 상태 ( DB에도 반영 완료 되서 평온한 상태 )
            Console.WriteLine(DB.Entry(SungWon).State);
        }

        #region Update
        // Update 3단계
        // 1) Tracked Entity를 얻어 온다.
        // 2) Entity 클래스의 Property를 변경한다. ( Set )
        // 3) SaveChanges를 호출한다.

        // Update를 할 때 전체 수정을 하는 것인지, 수정사항이 있는 애들만 골라서 하는것인지
        // 1) SaveChanges를 호출 하면 내부적으로 DetectChanges라는 함수를 호출한다.
        // 2) DetectChanges함수에서는 최초 Snapshot / 현재 Snapshot 을 비교해서
        // -> 수정사항이 있는 애들만 골라서 처리해줌

        //Update할때 2가지의 상황 
        //Connected : 일반적인 상황
        //Disconnected : Update 3단계가 연속적으로 일어나지 않고, 끊기는 경우를 말함
        //웹에서 아이디를 수정하고자 할때 기존 아이디를 불러와서 표시해주는 부분이 1) 에 해당하고
        //2) 3)은 나중에 연결이 끊긴 상태로 전송하게 되는 경우를 예로 들 수 잇다.

        //처리하는 2가지 방법
        //1) Reload 방식, 서로 필요한 정보만 보내서, 1 - 2 - 3 과정을 다시 밟는다
        //2) Full update 방식, 모든 정보를 다 보내고 받아서, Entity를 다시 만들고 통으로 Upate해주는 방식
        public static void UpdateTest()
        {
            using (AppDbContext DB = new AppDbContext())
            {
                //where과 비슷한 느낌의 Single 구문 
                //Single은 조건에 맞는 하나의 값을 반환한다. 2개 이상일 경우 Exception을 뱉는다.
                //최초 ----------------------------------------------------
                Guild guild = DB._Guilds.Single(g => g.GuildName == "A1"); // 1) 에 해당

                guild.GuildName = "B1"; // 2) 에 해당
                //현재 ----------------------------------------------------

                DB.SaveChanges(); // 3) 에 해당
            }
        }
        
        public static void ShowGuilds()
        {
            using (AppDbContext DB = new AppDbContext())
            {
                foreach(var guild in DB._Guilds.MapGuildToDto())
                {
                    Console.WriteLine($"GuildId ({guild.GuildId}) GuildName ({guild.Name}) MemberCount ({guild.MemberCount})");
                }
            }
        }
        
        // 장점 : 최소한의 정보로 Update 가능
        // 단점 : Read 두번하는것이 단점
        public static void UpdateByReload()
        {
            //웹으로 따지면 
            ShowGuilds(); //클라에게 정보 보여줌

            //클라에서 수정을 원하는 데이터의 ID / 정보가 왔다고 가정
            Console.WriteLine("Input GuildId");
            Console.Write(" > ");
            int Id = int.Parse(Console.ReadLine());
            Console.WriteLine("Input GuilName");
            Console.Write(" > ");
            string Name = Console.ReadLine();

            using (AppDbContext DB = new AppDbContext())
            {
                //다시 DB에 접근해서 UpDate 3단계 수행해줌
                Guild guild = DB.Find<Guild>(Id); //프라이머리 키를 이용해 Guild 찾아옴 1)
                guild.GuildName = Name;           // 2)
                DB.SaveChanges();                 // 3)
            }

            Console.WriteLine("----------Update Complete----------");
            ShowGuilds();
        }

        
        public static string MakeUpdateJsonStr()
        {
            var JsonStr = "{\"GuildId\":1, \"GuildName\":\"Hello\", \"Members\":null}";
            return JsonStr;
        }

        // 장점 : DB에 다시 Read할 필요 없이 바로 Update 
        // 단점 : 모든 정보 필요 ( 데이터를 새로 만드는 작업 필요 ), 보안 문제 (상대를 신용할때 사용해야함)
        public static void UpdateByFull()
        {
            ShowGuilds();

            string JsonStr = MakeUpdateJsonStr();
            Guild guild = JsonConvert.DeserializeObject<Guild>(JsonStr);

            using (AppDbContext DB = new AppDbContext())
            {
                //위에서 만들어준 guild를 이용해 통으로 업데이트 시켜준다.
                //guild안에 프라이머리 키가 잇기때문에 업데이트 된다.
                DB._Guilds.Update(guild);
                DB.SaveChanges();
            }

            Console.WriteLine("----------Update Complete----------");
            ShowGuilds();
        }

        #endregion

        #region DataLoading 종류
        //관련 데이터 로딩할때 특히 외부키
        #region EagarLoading
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
                    .ThenInclude(p => p.OwnedItem) // Include에 대해서 추가적으로 로딩시켜 주고 싶을 대상이 필요할때 사용
                    .First(); // sql로 따지면 top 1과 동치

                foreach(Player player in Guild.Members)
                {
                    Console.WriteLine($"TemplateId({player.OwnedItem.ItemId}) Owner ({player.Name})");
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
        #endregion

        #region ExplicitLoading
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
                    DB.Entry(player).Reference(p => p.OwnedItem).Load();
                }

                foreach (Player player in guild.Members)
                {
                    Console.WriteLine($"TemplateId({player.OwnedItem.ItemId}) Owner ({player.Name})");
                }
            }
        }
        #endregion

        #region SelectLoading
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
                    .Select(g=> new GuildDTO() //new 를 이용해 익명의 값이 아닌 GuildDTO를 이용해 명시적으로 값을 알게 함    
                    {
                        Name = g.GuildName,
                        MemberCount = g.Members.Count
                    })
                    .First();

                Console.WriteLine($"GuildName : ({Info.Name}), MemberCount({Info.MemberCount})");
            }
        }
        #endregion

        #endregion

        public static void ShowItems()
        {
            Console.WriteLine("플레이어 이름 입력");
            Console.Write(" > ");
            string Name = Console.ReadLine();

            using (AppDbContext DB = new AppDbContext())
            {
                foreach (Player Player in DB._Players.AsNoTracking().Where(p => p.Name == Name).Include(P => P.OwnedItem))
                {
                    Console.WriteLine($"{Player.OwnedItem.TemplateId}");
                }
            }
        }

        #region 외부키와 nullable
        // Relationship 복습
        // - Principal Entity (주요 -> Player)
        // - Dependent Entity (의존적 -> FK 포함하는 쪽 -> Item)

        // Dependent 데이터가 Principal 데이터 없이 존재할 수 있을까?
        // 내가 정한 정책에 따라 위에 질문에 대한 답이 달라진다.
        // 1) 주인이 없는 아이템은 불가능하다
        // 2) 주인이 없는 아이템은 가능하다.

        // 2번과 같은 정책일때 어떻게 구분해서 설정할까?
        // Nullable로 구분해준다.
        // 즉, 1번정책이라면 외부키를 값으로 설정하고,
        //     2번정책이라면 외부키를 Nullable로 설정하면 됨

        public static void ShowItemsT()
        {
            using (AppDbContext DB = new AppDbContext())
            {
                foreach(var item in DB._Items.Include(i=>i.Owner).ToList())
                {
                    if(item.Owner == null)
                    {
                        Console.WriteLine($"ItemId({item.ItemId}) TemplateId({item.TemplateId})Owner (0)");
                    }
                    else
                    {
                        Console.WriteLine($"ItemId({item.ItemId}) TemplateId({item.TemplateId}) OwnerId({item.Owner.PlayerId}) Owner({item.Owner.Name})");
                    }
                }
            }
        }

        //Nullable을 표시하려면 변수 옆에 ?를 붙이면 된다.
        // 1) FK가 Nullable이 아니라면
        // - Player가 지워지면, FK로 해당 Player를 참조하는 Item도 같이 삭제된다.
        // 2) FK가 Nullable이라면
        // - Player가 지워지더라도, FK로 해당 Player를 참조하는 Item은 그대로 있는다.

        public static void TestNullable()
        {
            ShowItemsT();

            Console.WriteLine("Input Delete PlayerId");
            Console.Write(" > ");
            int Id = int.Parse(Console.ReadLine());

            using (AppDbContext DB = new AppDbContext())
            {
                Player player = DB._Players
                    .Include(p => p.OwnedItem) //외부키를 nullable가 가능하게 하고 이부분에서 Item을 include해서 로딩하는 부분을 제거하면 아래에서 player를 제거할때 에러가 난다. 
                                          //include를 이용해 로딩, 즉 추적하지 않으면 외부키를null로 밀수 없기 때문 왠만해선 외부키 로딩은 무조건 해줘야함
                    .Single(p => p.PlayerId == Id);

                DB._Players.Remove(player);
                DB.SaveChanges();
            }

            Console.WriteLine("-------------Test Complete--------------");
            ShowItemsT();
        }
        #endregion

        #region RelationShip Update
        // Update Relationship 1 : 1 
        // 아이템의 Owner를 바꾸고 싶거나 Player의 Item을 바꾸고 싶을 때
        // 외부키 수정에 관한 방법 1 : 1 경우
        public static void Update_1v1()
        {
            ShowItemsT();

            Console.WriteLine("Input ItemSwitch PlayerId");
            Console.Write(" > ");
            int Id = int.Parse(Console.ReadLine());

            using (AppDbContext DB = new AppDbContext())
            {
                Player player = DB._Players
                    .Include(p => p.OwnedItem)
                    .Single(p => p.PlayerId == Id);

                //메모리에 들고 있는것처럼 
                if(player.OwnedItem != null)
                {
                    //외부키에 접근해서 데이터 수정
                    player.OwnedItem.TemplateId = 777;
                    player.OwnedItem.CreateDate = DateTime.Now;
                }

                //이처럼 아이템 새로 생성해서 player에 넣어주면
                //기존에 있던 아이템 Owner에서 player가 빠지고
                //새로운 아이템에 player가 Owner로 할당된다.
                player.OwnedItem = new Item()
                {
                    TemplateId = 777,
                    CreateDate = DateTime.Now
                };

                DB.SaveChanges();
            }

            ShowItemsT();
        }

        public static void ShowGuild()
        {
            using (AppDbContext DB = new AppDbContext())
            {
                foreach (var guild in DB._Guilds.Include(g => g.Members).ToList())
                {
                    Console.WriteLine($"GuildId ({guild.GuildId}) GuildName ({guild.GuildName}) MemberCount ({guild.Members.Count})");
                }
            }
        }

        // Update Relationship 1 : N
        public static void Update_1VN()
        {
            ShowGuild();

            Console.WriteLine("Input GuildId");
            Console.Write(" > ");
            int Id = int.Parse(Console.ReadLine());

            using (AppDbContext DB = new AppDbContext())
            {
                Guild guild = DB._Guilds
                    .Include(g => g.Members)
                    .Single(g => g.GuildId == Id);

                // Include를 주석 처리하고 Add를 이용하여 추가할 경우
                // Members가 null이므로 에러가난다.
                // Include를 포함 시키고 Add 시키면 말 그대로 추가된다.
                //guild.Members.Add(new Player()
                //{
                //    Name = "BiDDak"
                //});

                //Add방법 말고 직접 메모리에서 생성한후 넣는 방식을 사용할떄
                guild.Members = new List<Player>()
                {
                    new Player() {Name = "BiDDak"}
                };

                // Include를 주석 처리하고 플레이어를 추가할 경우
                // EF에서 Guild에 있는 다른 멤버들의 정보를 모르기 때문에
                // Guild의 Member에 플레이어를 말 그대로 추가해준다.

                // 반면, Include를 주석처리하지 않고 플레이어를 추가할 경우
                // EF에서 Member들의 정보를 알고 있는 상태로 
                // Guild의 Members에 새로 생성한 플레이어를 넣는것이므로 기존에 있던 멤버들과 연결을 끊고
                // 새로 만들어준 플레이어를 넣는다.

                DB.SaveChanges();
            }

            Console.WriteLine("----- Test Complete -------");
            ShowGuild();
        }
        #endregion

        #region Delete
        // 1) Tracking Entity를 얻어오고
        // 2) Remove 호출
        // 3) SaveChanges 호출
        public static void TestDelete()
        {
            ShowItemsT();

            Console.WriteLine("Seelct Delete ItemId");
            Console.Write(" > ");
            int Id = int.Parse(Console.ReadLine());

            using (AppDbContext DB = new AppDbContext())
            {
                Item item = DB._Items.Find(Id);
                //DB._Items.Remove(item); //보통 이렇게 삭제하지 않고 따로 변수를 둬서 삭제 대기 중이라는 것을 기록해둔다.
                item.SoftDeleted = true; 
                DB.SaveChanges();
            }

            Console.WriteLine("---- TestDelete Complete ----");
            ShowItemsT();
        }
        #endregion

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
