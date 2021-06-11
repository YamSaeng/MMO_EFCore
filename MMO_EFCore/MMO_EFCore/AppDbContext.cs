using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_EFCore
{
    // EF.Core 작동 순서
    // 1) DbContext를 new를 이용해서 만들면
    // 2) DbSet<T>을 찾는다.
    // 3) 모델링 Class를 분석해서, 칼럼을 찾는다.
    // 4) 모델링 Class에서 참조하는(외부키) 다른 Class가 있으면, 해당 클래스에 대해서 3)과정 수행
    // 5) OnModelCreating 함수 호출 ( 세부적인 설정을 더 하고 싶으면 OnModelCreating를 override 하면 됨)
    // 6) 데이터베이스의 전체 모델링 구조를 내부 메모리에 들고 있음
    public class AppDbContext : DbContext
    {
        // CItem을 DB에 등록시켜준다.
        // DbSet<Item> -> EF Core한테 알려준다.
        // _Items이라는 DB 테이블이 있는데, 세부적인 칼럼/키 정보를 Item클래스에서 참고하라고 알려준다는 의미
        public DbSet<Item> _Items { get; set; }
        public DbSet<Player> _Players { get; set; }
        public DbSet<Guild> _Guilds { get; set; }

        // DB를 연결할때 필요하면 문자열로 각종 설정을 붙인다.
        public const string ConnectionString = @"Data Source=(localdb)\ProjectsV13;Initial Catalog=EFCoreDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            Options.UseSqlServer(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder Builder)
        {
            // 앞으로 Item Entity에 접근할 때 항상 사용되는 모델 레벨(데이터모델에 기록해둔 테이블을 의미)의 필터링을 하고 싶을 경우
            // Item 테이블에서 데이터를 가지고 올때 SoftDeleted가 false인 대상만 가져올 수 있도록 조건을 걸어줌            
            Builder.Entity<Item>().HasQueryFilter(i => i.SoftDeleted == false);
            // 필터를 무시하고 싶으면 IgnoreQueryFilter 옵션 추가
            #region 설정한 필터 무시하고 싶을 경우
            // item 테이블 순회할때 필터 무시해주고 아래에서 검사해주는 방식의 예
            //foreach (var item in DB._Items.Include(i => i.Owner).IgnoreQueryFilters().ToList())
            //{
            //    if(Item.SoftDeleted)
            //    {
            //        Console.WriteLine($"DELETE - ItemId({item.ItemId}) TemplateId({item.TemplateId})");
            //    }
            //    else
            //    {
            //        if (item.Owner == null)
            //        {
            //            Console.WriteLine($"ItemId({item.ItemId}) TemplateId({item.TemplateId})Owner (0)");
            //        }
            //        else
            //        {
            //            Console.WriteLine($"ItemId({item.ItemId}) TemplateId({item.TemplateId}) OwnerId({item.Owner.PlayerId}) Owner({item.Owner.Name})");
            //        }
            //    }                
            //}
            // 이런식으로 추가해주면 됨
            #endregion

            #region 프로퍼티에 Index추가하고 Unique옵션 추가 하기
            Builder.Entity<Player>()
                .HasIndex(p => p.Name) //이름에 Index를 달아줌
                .HasName("Index_Person_Name") //인덱스 이름 정하기
                .IsUnique(); //이름이 겹치면 안되므로 Unique로 설정
                             //Builder.Entity<Player>().Property(p => p.Name).IsRequired(); // Name을 Not Null로 설정 isRequired(false) null로 설정

            //Builder.Entity<Player>()
            //    .HasMany(p => p.CreatedItems)
            //    .WithOne(i => i.Creator)
            //    .HasForeignKey(i => i.CreatorId); // 1 : N 관계에서는 N인쪽에서 FK가 잇어서 상대방이 명확하지만

            //Builder.Entity<Player>()
            //    .HasOne(p => p.OwnedItem)
            //    .WithOne(i => i.Owner)
            //    .HasForeignKey<Item>(i => i.Owner); // 1 : 1 관계에서는 어느쪽이 FK인지 확인이 어려워서 명시적으로 타입을 지정해야함
            #endregion

            // shadow Property
            Builder.Entity<Item>().Property<DateTime>("RecoveredDate");

            //Owned Type
            //Builder.Entity<Item>()
            //    .OwnsOne(i => i.Option); //클래스 타입이지만 테이블로 따로 관리하지 않고 클래스 안에 있는 데이터 그 자체를 값으로써 추가해준다.
            Builder.Entity<Item>()
                .OwnsOne(i => i.Option) // Item Entity에 Option이라는 값이 있는데 그것들 토대로
                .ToTable("ItemOption"); // ItemOption 테이블을 만들어라 라는 뜻
                                        // 이처럼 테이블을 만들어주면 Option을 가지고 있는 기본키가 해당 테이블에 기본키가 되면서 동시에 외부키가 된다.
        }
    }
}
