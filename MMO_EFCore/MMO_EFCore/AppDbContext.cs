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
        public DbSet<CItem> _Items { get; set; }
        public DbSet<CPlayer> _Players { get; set; }

        // DB를 연결할때 필요하면 문자열로 각종 설정을 붙인다.
        public const string ConnectionString = @"Data Source=(localdb)\ProjectsV13;Initial Catalog=EFCoreDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            Options.UseSqlServer(ConnectionString);
        }
    }
}
