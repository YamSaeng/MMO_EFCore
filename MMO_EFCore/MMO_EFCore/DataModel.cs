using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MMO_EFCore
{
    //   --- Configuration ---
    // DB를 상세하게 설정하는 방법

    // A) Convention (관례)
    // - 각종 형식과 이름 등을 정해진 규칙에 맞게 만들면, EF Core에서 알아서 맞게 처리해준다.
    // - 쉽고 빠르지만, 모든 경우를 처리할 수는 없다.
    // 예를들면 테이블명에 ID를 붙이면 EF Core에서 자동으로 Primary Key로 인식하는 경우
    // B) Data Annotation (데이터 주석)
    // - class/property 등에 Attribute를 붙여 추가 정보를 기입
    // 예를 들어 [Table("Item")] 같은 경우
    // C) Fluent Api (직접 정의)
    // - OnModelCreating에서 직접 설정을 정의해서 만드는 방식
    // - 활용 범위가 가장 넓다
    // ex) 필터링 추가하는 HasQueryFilter 추가

    // 설정관련해서 우선순위 C) Fluent API > B) Data Annotation > A) Convention
    // 즉 A를 이용해서 설정한 값을 B가 덮어 씌울 수 있고, 최종적으로 C가 덮어 씌운다.

    // ----------- Convention을 주요 사용 --------------
    // 1) Entity Class 관련 
    // - public 접근 한정 + non-static
    // - 프로퍼티 중에서 public getter를 찾으면서 분석 get 잇는 프로퍼티가 public 아니면 에러
    // - 프로퍼티 이름 = table column 이름
    // 2) 이름, 형식, 크기 관련
    // - .NET 형식 <-> SQL 형식 알아서 변환 (ex) int, bool 등
    // - .NET 형식의 Nullable 여부를 따라간다
    //   - C#에서 string은 기본적으로 nullable을 취하는데, 그대로 적용한다.
    //   - int 같은경우는 기본적으로 non-nullable를 취하므로, 그대로 적용한다.
    //   - nullable 적용시켜주고 싶으면 자료형옆에 ? 붙이면 된다.
    // 3) Primary Key 관련
    // - Id 혹은 클래스이름 + ID 로 정의된 프로퍼티를 PK로 인식한다.
    // - 복합키(Composite Key) Convention으로 처리가 불가능
    //   - 복합키는 여러가지의 변수를 하나의 PK로 설정하는 것을 의미한다.
    // --------------------------------------------------

    // Q1) DB Column type, Size, Nullable 등
    // Nullable설정  ?   [Required] //Not Null 적용 .IsReuired()
    // 문자열 길이       [MaxLength(20)]      .HasMaxLength(20)
    // 문자 형식                              .IsUnicode(true)

    // Q2) Primary Key
    // [Key] -- PK 설정
    // [key][Column(Order=0)] [key][Column(Order=1)] -- 복합키 설정
    // .HasKey( x => x.Prop1 ) -- PK 설정
    // .HasKey( x => new {x.Prop1, x.Prop2} ) -- 복합키 설정

    // Q3) Index
    // 인덱스 추가                 .HasIndex(p=> p.Prop1)
    // 복합 인덱스 추가            .HasIndex(p=> new {p.Prop1, p.Prop2})
    // 인덱스 이름을 정해서 추가   .HasIndex(p=> p.Prop1).HasName("Index_MyProp")
    // 유니크 인덱스 추가          .HasIndex(p=> p.Prop1).IsUnique()

    // Q4) 테이블 이름 설정
    // DBSet<T> Property 이름 or class 이름
    // [Table("테이블이름"]      .ToTable("테이블이름")

    // Q5) 칼럼 이름
    // Property 이름
    // [Column("컬럼이름")]       .HasColumnName("컬럼이름")

    // Q6) 코드 모델링에서는 사용하지만, DB 모델링에서는 제외하고 싶을 경우 ( 프로퍼티 / 클래스 모두 가능 )
    // 예를 들어 Item과 Player관계에서 Item이 Player를 가지고 있지만 DB에 외부키로 등록하고 싶지는 않을경우 ( 메모리에서만 관리 해주는 용도 )
    // 또는, 특정 클래스를 DB에 등록하고 싶지는 않을경우를 의미한다.
    // [NotMapped]                .Ignore()

    // Q7) Soft Delete
    // .HasQueryFilter()    

    // --- Relationship Configuration ---
    // 기본 용어 복습
    // 1) Principal Entity
    // 2) Dependent Entity - Principal에 의존적인 Entity Principal이 없으면 의미가 없어지는 Entity
    //                      - 보통 외부키를 사용하는쪽이 Dependent Entity가 되는 듯
    // 3) Navigational Property - 참조형으로 들고 잇는 변수 ex) Item의 Player
    // 4) Primary Key (PK) - 기본키 (=주키)
    // 5) Foreign Key (FK) - 외부키 
    // 6) Principal Key = PK or Unique Alternate Key
    //   - Primary Key 와 Primary Key를 대체하는 Key(Unique하고 Index를 적용한)를 포함해서 이르는 말
    // 7) Required Relationship (Not-Null) 관계되어 잇는 대상이 꼭 필요한 경우
    // 8) Optional Relationship (Nullable) 관계되어 잇는 대상이 꼭 필요하지 않는 경우

    // Convention을 이용한 FK 설정
    // ex) Item에 Player를 참조할때 외부키 이름 명명법
    // 1) <PrincipalKeyName>                           PlayerId
    // 2) <Class><PrincipalKeyName>                    PlayerPlayerId
    // 3) <NavigationalPropertyName><PrincipalKeyName> OwnerPlayerId / OwnerId 가장나은듯

    // FK와 Nullable
    // 1) Required Relationship (Not-NUll)
    //   -- 반드시 상대방이 필요 
    //   -- 삭제할 때 OnDelete 인자를 Cascade 모드로 호출 -> Principal삭제하면 Dependent를 삭제한다.
    // 2) Optional Relationshup (Nullable)
    //   -- 삭제할 때 OnDelete 인자를 ClientSetNull 모드로 호출
    //   -> Principal 삭제할 때 Dependent Tracking하고 있으면, FK를 null로 셋팅
    //   -> Principal 삭제할 때 Dependent Tracking하고 있지 않으면, Exception 발생

    // Convention 방식으로 못하는 것
    // 1) 복합 FK
    // 2) 다수의 Navigational Property가 같은 클래스를 참조할 때
    //  ex) Item에 주인과 해당 아이템을 최초로 만든 Player를 동시에 가지고 싶을때의 경우
    //      public class Item { Player Owner, Player CreatePlayer }
    //      public class Player { Item OwnedItem, ICollection<item> CreatedItems }
    //      위와 같은 경우 OwnedItem을 Owner에 연결해야 하는지 CreatePlayer에 연결해야 하는지 EF Core에서 판단하지 못하기 때문
    //      따라서 Convention방식으로는 해결하지 못한다.
    // 3) DB나 삭제 관련 커스터마이징이 필요할 경우
    //   - 삭제 할때 내 입맛에 맞게 삭제 과정을 커스터마이징 하고 싶은 경우가 이에 해당

    // Data Annotation으로 Relationshup 설정
    // [ForeignKey("프로퍼티이름")]
    //  - ex) 
    //        [ForeignKey("OwnerId")] - OnwerId를 외부키로 설정
    //        [ForeignKey("Owner")] - OwnerId가 참조하는 개체가 Owner이라고 알려줌 -- 이와같은 경우 OwnerId가 외부키가 되는건지?
    //        public int OwnerId 
    //        [ForeignKey("OwnerId")] - Owner의 외부키가 OwnerId라고 알려줌 -- 이것도 OwnerId가 외부키가 되는건지?
    //        [ForeignKey("ItemId, TemplateId")] 복합키 기능 Owner의 외부키를 ItemId, TemplateId로 설정
    //        public Player Owner
    // [InverseProperty] - 다수의 Navigational Property를 참조할떄의 해결방법
    // ex) [InverseProperty("짝꿍이름")]
    //  public class Item
    //  {
    //      public int OwnerId { get; set; }
    //      [InverseProperty("OwnedItem")] Owner에 대응되는 Item을 OwnedItem으로 설정
    //      public Player Owner { get; set; }
    //      public int CreatorId { get; set; }
    //      [InverseProperty("CreatedItems")] Creator에 대응되는 Item을 CreatedItems으로 설정
    //      public Player Creator { get; set; }
    //  }
    //  반대의 경우도 성립된다. 위에서는 Player에 연동되는 Item을 설정한것이라면
    //  아래처럼 Item에 연동되는 Player를 설정할 수도 있는것
    //  Mssql 버그중 하나로 FK를 둘다 Nullable로 설정하지 않고 같은 테이블을 참조하면 나중에 Cascade할때 에러를 내뱉는 버그가 잇어서
    //  둘 중 하나는 Nullable로 설정해줘야한다.

    //  public class Player
    //  {
    //      [InverseProperty("Owner")] OwnedItem에 대응되는 Player를 Owner로
    //      public Item OwnedItem { get; set; }
    //      [InverseProperty("Creator")] CreatedItems에 대응되는 Player를 Creator로
    //      public ICollection<Item> CreatedItems { get; set; }
    //  }

    // Fluent API로 Relationship 설정
    // .HasOne() - 1 : 1 관계   .HasMany() - 1 : N  나
    // .WithOne() - 1 : 1 관계  .WithMany() - 1 : N 상대방쪽
    // .HasForeignKey()  .IsRequired()  .OnDelete()
    // .HasConstraintName()  .HasPrincipalKey()

    // Showdow Property + Backing Field
    // Class에는 있지만 DB에는 적용하고 싶지 않을때 -> [NotMapped] .Ignore()
    // DB에는 있지만 Class에는 없을때 -> Shadow Property
    // 예를 들어 RecoveredDate(아이템을 잃어버렸다가 복구한날짜)와 같은 column이 있을때
    // DB에 있어서 기록 할 수 있지만 굳이 class에는 안가지고 잇어도 될거같은 변수들 ( 데이터를 숨기는 것 )
    // 생성 -> .Property<DateTime>("UpdateOn"
    // 접근하고 할때 Read / Write -> .Property("RecoveredDate").CurrentValue를 이용해 읽거나 수정

    // Backing Field (EF Core)
    // private field를 DB에 매핑하고, public getter로 가공해서 사용
    // ex) DB에는 json 형태로 string을 저장하고, getter은 json을 가공해서 사용하고자 할때 
    // 일반적으로 Fluent API 방식만 사용    

    #region Entity <-> DB Table 연동하는 다양한 방법들
    // Entity <-> DB Table 연동하는 다양한 방법들
    // 지금까지 사용하던 방식 특정 데이터를 Read / Write 하기 위해 Entity Class를 통으로 읽어들이는 부분에 있어서 부담이 생김
    // 물론 Select Loading, DTO 방식도 있지만 좀 더 기본적인 방법이 있는데 다음과 같다.

    // 1) Owned Type
    // - 일반 Class를 Entity Class에 추가하는 개념
    // public class ItemOption
    // {
    //     public int Str { get; set; }
    //     public int Dex { get; set; }
    //     public int HP { get; set; }
    // }
    // ItemOption 클래스를 Item에 추가할때 해당 클래스가 Navigational 프로퍼티로써 별도의 테이블이 아닌 값으로 연동시켜주고 싶을 경우
    // 즉,  public class Item { public ItemOption Option {get;set;} }
    //   -> public class Item { public int Str{get;set;} public int Dex{get;set;} public int HP {get;set;} } 이런식으로 인식되어서 사용하고 싶을 경우가 생길 수 있다.
    // ItemOption option 변수로 들어가 있지만 테이블로는 따로 관리가 안되고 값 그자체로 인식 되고 싶을 경우를 의미
    // a) 동일한 테이블에 추가하고 싶을 경우
    // - .OwnsOne()
    // 기존처럼 Relationship 방식으로 했을 경우에는 기본적으로 FK로 빠져있기때문에 정보가 자동으로 추가되지 않아 .Include를 통해 데이터를 긁어왓어야 했는데,
    // 위처럼 Relationshuip이 아닌 Ownership의 개념은 .include를 통해 데이터를 긁어올 필요 없이 자동으로 데이터가 로딩된다.
    // 하나의 테이블안에서 관련된 변수들을 하나의 클래스로 묶어서 관리하고자 할때 사용하면 편함    
    // b) 다른 테이블에 추가
    // - .OwnsOne().Totable()

    // 2) Table Per Hierarchy (TPH) 
    // - 상속 관계의 여러 class -> 하나의 테이블에 매핑하고 싶을 경우
    // a) Convention
    // - class를 상속받아서 만들고, DBSet에 추가하면 테이블에 저장된다.
    // - 저장될때 테이블이 따로 만들어져야 할거 같지만 합쳐서 상속관계라면 부모 테이블에 합쳐서 저장이된다.
    // - 기본적으로 합쳐서 저장되기때문에 구분을 해야하는데, EF에서 알아서 Discriminator이라는 Column을 생성하고 해당 Column은 string으로 클래스를 구분해준다.
    // - 이 경우에는 Item을 저장할때는 Item에 접근해야하고 EventItem을 저장할떄는 EventItem을 저장해야하는 부분에서 있어서 불편할 수 있는데,
    // - Fluent API를 통해 보다 쉽게 할 수 있다.
    // b) Fluent API
    // - 기본적으로 부모와 자식을 구분 할 수 있는 enum값이 필요하다.
    // - .HasDiscriminator().HasValue()

    // 3) Table Splitting
    // - 다수의 Entity Class -> 하나의 테이블에 매핑하고 싶을 경우
    #endregion

    #region User Defined Function (UDF)
    // User Defined Function (UDF)
    // 우리가 직접 만든 SQL을 호출하게 하는 기능   
    // - 연산을 DB쪽에 하도록 넘기고 싶은 경우
    // - EF Core 쿼리가 사람이 직접 작성하는 SQL 쿼리보단 효율이 떨어지기 때문에
    // -> SQL 쿼리를 직접만들고 함수를 만들어서 호출하게 만들 수 있다.

    // 1) Configuration
    // - static 함수를 만들고 EF Core에 등록하는 과정
    // 2) Database Setup ( .ExecuteSqlRaw(함수이름) )
    // 3) 사용

    /*
      
        Annotation (데이터 주석 방식으로 DB함수로 만들어줌)
        static 함수 선언
        [DbFunction()]
        public static double? GetAverageReviewScore(int ItemID)
        {
            throw new NotImplementedException("사용 금지!"); // c# 에서 호출하면 예외 발생 시키도록
        }

        함수 DB에 등록시키기
        string Command =
        함수를 생성한다 GetAverageReviewScore sql로 내용을 채워준후
        .ExecuteSqlRaw 함수로 등록한다.
        @" CREATE FUNCTION GetAverageReviewScore (@itemId INT) RETURNS FLOAT
           AS
           BEGIN
           DECLARE @Result AS FLOAT

           SELECT @Result = AVG(CAST([Score] AS FLOAT))
           FROM ItemReview AS r
           WHERE @itemId = r.ItemId

           RETURN @result
           END";

           DB.Database.ExecuteSqlRaw(Command);
        
        사용
        foreach(double? Average in DB._Items.Select(i => Program.GetAverageReviewScore(i.ItemId))) // 이런식으로 사용
        만약 Program.GetAverageReviewScore를 직접 호출하면 위에서 설정한 throw로 인해 예외를 던진다.
    */
    public class ItemReview
    {
        public int ItemReviewId { get; set; }
        public int Score { get; set; }
    }
    #endregion

    #region 초기값 (Default Value)
    // 특정 프로퍼티의 값을 말 그대로 초기값을 주고 싶을 경우를 의미
    // 특히나 DateTime 경우 매우 유용하다고 할 수 있다.    
    // 유의 해야할 점
    // 1) Entity Class 자체의 초기값으로 붙는건지
    // -> = 메모리상에서의 초기값
    // 2) DB Table 차원에서 초기값으로 적용 되는건지
    // -> = DB상에서의 초기값
    // - 결과는 같다고 생각할 수 있지만,
    // 만약, EF <-> DB 외에 다른 경로로 DB를 사용하는 경우 차이가 날 수 있다.

    // 기본값 설정하는 방법
    // 1) Auto - Property Initializer (C# 6.0) - 프로퍼티 옆에 초기값 쓰고 ; 붙이면 됨
    // - Entity 차원에서의 초기값으로서 SaveChanges를 호출하지 않으면 DB에 적용되지 않는다.
    // - 따라서 외부에서 SQL문법으로 쿼리를 날리면 초기값이 적용되지 않고 SQL 쿼리 그대로 DB에 적용되는 점이 있다.
    // 2) Fluent API
    // - DB Table에 초기값을 적용 시킨다. ( DEFAULT )
    // Builder.Entity<Item>()
    //        .Property(초기값 적용할 프로퍼티 이름)
    //        .HasDefaultValue(초기값);
    // - 이와 같은 경우에 기본 타입들은 초기값을 적용시킬 수 있지만 시간과 같은 경우에 애매한데, 이유는 다음과 같다.
    // 만약 .HasDefaultValue(DateTime.Now)를 이용해 시간을 적용해도 구문 그대로 해당 함수를 실행했을때의 값이
    // 초기값으로 설정되기때문에 새로운 데이터가 들어오더라도 들어온 시점의 시간이 추가 되지 않는 문제점이 생긴다.
    // 3) SQL Fragment ( 새로운 값이 추가되는 시점에 DB쪽에서 실행 )
    // - .HasDefaultValueSql
    // Builder.Entity<Item>()
    //        .Property("CreateDate")
    //        .HasDefaultValueSql("GETDATE()"); -- 이처럼 Sql 명령어 조각을 넣어주는 방식
    // 단 이렇게 설정하면 외부에서 해당 변수를 수정 할 수 없게 private으로 막아줘야 의미가 있다고 할 수 있다.
    // 4) Value Generator (EF Core에서 실행된다)
    // - 일종의 Generator 규칙 (초기값을 생성하는 규칙을 정할 수 있음) 

    public class PlayerNameGenerator : ValueGenerator<string>
    {
        //임시값을 생성 할 것인지
        //false로 해서 내가 정해준 값으로 할것이라고 해줌
        public override bool GeneratesTemporaryValues => false;
                
        public override string Next(EntityEntry entry)
        {
            string name = $"Player_+{DateTime.Now.ToString("yyyymmdd")}";
            return name;
        }
    }

    #endregion

    #region Migration
    // DB 버전관리에 대한 내용
    // 기준 값을 정해야 한다.
    // DB 테이블에 있는 값들을 원본으로 할 것인지,
    // 실질적으로 코드로 구성하고 있는 데이터인 public class AppDbContext를 원본으로 할 것인지
    // 거의 닭이 먼저냐 알이 먼저냐

    // 1) Code - First
    // - AppDBContext가 원본이며, AppDBConect안에 있는 Entity Class를 설계한 데이터들이 원본 설계이며
    // - 만약 틀어짐이 발생했다면, DB가 잘못된 것으로 여기는 방법
    // - 지금까지 우리가 사용하던 방식 ( Entity Class / DbContext가 기준 )
    // - 항상 최신 상태로 DB를 업데이트 하고 싶다는 말이 아님

    // --- Migration Step ---
    // A) Migration 만들고 -> Add-Migration [Migration 이름] ( 패키지 관리자 콘솔에 입력해야 함 )
    // B) Migration을 적용한다.

    // Add-Migration [Name]
    // (1) DbContext를 찾아서 분석해서 DB를 모델링한다. (최신)
    // (2) ~ModelSnapshot.cs을 이용해서 가장 마지막 Migration 상태의 DB 모델링 (가장 마지막 상태)
    // (3) 1 과 2를 비교해서 결과를 도출 ( 최신과 가장 마지막 상태로 저장되어 있던 Migration을 비교 ) 맨 처음에는 1번 파일 없음
    //   - a) ModelSnapShot -> 최신 DB 모델링
    //   - b) Migrate.Designer.cs와 Migrate.cs -> Migration 관련된 세부 정보
    //      Migrate.cs 에 Up 함수와 Down 함수가 있는데 
    //      Up은 이전 상태에서 해당 Migration으로 오려면 해야하는 작업을 서술
    //      Down은 Migration 상태에서 이전 상태로 가려면 해야하는 작업을 서술
    // 즉, A(현재) B(과거) Up = B(과거) -> A(현재) Down = A(현재) -> B(과거)
    // Up 과 Down에 내용을 추가 할 수 있음

    // Migration 적용
    //  (1) SQL change script - Script-Migration [From] [To] [Options]  From, To 에는 Migration 이름 넣으면 됨
    //                          From 하고 To 비워져 있으면 최초에서 최후 버전으로 인식해서 적용시켜준다. 
    //      이 방법을 권장 쿼리문을 DB에 넘겨주면 되니까 EF와는 독립적으로 사용하기도 좋고 
    //  (2) Database.Migrate 호출 -- 잘 사용 안함
    //  (3) Command Line 방식 - Update-Database [Options]
    //     - 아무런 옵션 없이 Update-Database 입력하면 데이터베이스의 상태를 최신 상태로 업데이트 해준다.
    //     - 특정 마이그레이션으로 이동 : Update-Database 마이그레이션이름 이렇게 입력하면 해당 마이그레이션 상태로 돌려준다.
    //     - 마지막 Migration 삭제 (Remove-Migration)

    // 2) Database - First 
    // - Code-First 와는 반대로 DB쪽이 원본이고 거기에 맞춰서 DbContext를 작성하는 방식
    // - DB 를 보면서 DBContext 작성

    // 3) SQL - First
    // - SQL 쿼리문을 직접 작성해서 Migration 만드는 방법
    // - Code - First (1) SQL chagne script를 이용하는 방법
    // - DB 끼리의 비교를 이용해서 SQL를 추출하는 방법 

    #endregion

    #region DB Context 심화 과정 (최적화 등 때문)
    // DB Context의 핵심 기능 3가지
    // 1) ChangeTracker 
    // - Tracking State 관리하는 객체
    // 2) Database
    // - Transaction
    // - DB Creation / Migration
    // - Raw SQL ( SQL 실행 )
    // 3) Model
    // - DB 모델링 관련

    // State (상태)
    // 1) Detached (No Tracking | 추적되지 않는 상태, SaveChanges를 해도 추적하지 않고 있어서 DB에 반영이 안되어 있음) 
    // 2) Unchanged (DB에 있고, 메모리에 들고 있는 해당 변수가 변동사항이 없어서 수정할 필요가 없는 상태, SaveChanges를 해도 값이 그대로니까 아무것도 하지 않음)
    // 3) Deleted (DB에는 아직 있지만, 삭제되어야하는 상태, SaveChanges로 DB에 적용)
    // 4) Modified (DB에 있고, 클라에서 수정(메모리상에서)된 상태, SaveChanges로 DB에 적용)
    // 5) Added (DB에는 아직 없고, 클라에서 생성(메모리상), SaveChanges로 DB에 적용)

    // State 체크 방법
    // Entry().State
    // Entry().Property().IsModified  - 변화가 있어서 적용을 해야하는지의 여부
    // Entry().Navigation().IsModified - 위와 마찬가지  

    // State가 대부분 '직관적'이지만 Relationship이 개입하면 좀 복잡해진다.
    // State에 대해 좀 더 자세히 알아보는 이유는 우리가 sql문을 작성해서 쿼리를 날려 수정하는 방법을 사용하지 않고
    // 상태 변화를 감지하여 SaveChange하고 DB에 저장하는 방식을 사용하고 있기 때문이다

    // 1) Add / AddRange 사용할 때의 상태 변화
    // Add를 적용하고 나서 FK로 참조하고 있는 Navigational Property의 상태도 Added로 변한다.  
    // - NotTracking 상태라면 Added
    // - Tracking 상태라면, FK 설정이 필요한지에 따라 Modified / 기존 상태 유지 ( = Unchanged )

    // 2) Remove / RemoveRange 사용할 때의 상태 변화
    // - ( DB에 의해 생성된 Key가 있는지 없는지 ) && ( C# 기본값이 인지 아닌지 ) -> 필요에 따라 Unchanged / Modified / Deleted
    // - ( DB에 의해 생성된 Key가 없고 ) || ( C# 기본값 이라면 ) -> Added
    // - 삭제하는데 왜 굳이 Added인것인지 동작의 일관성 때문
    // - DB에서도 일단 존재는 알고 있어야 하기에 특히나 Cascade Delete 처리할때 등

    // 3) Update / UpdateRange
    // - EF에서 Entity를 Update하는 기본적인 방법은 Update가 아님
    // - Tracked Entity를 얻어와서 Property를 수정하고 최종적으로 SaveChange
    // - Update는 UnTracked Entity를 통으로 업데이트 할 때 사용한다

    // EF Core에서 Update하면 일어나는 과정
    // 1) Update 호출
    // 2) Entity State = Modified 로 변경
    // 3) 해당 객체의 모든 Non - Relational Property의 IsModified = true로 변경 -> 이로써 객체가 통으로 DB에 갱신이 됨
    // - 만약 Relationship이 있는 경우 
    // - ( DB에 의해 생성된 Key가 있는지 없는지 ) && ( C# 기본값이 인지 아닌지(!=0) ) -> 필요에 따라 Unchanged / Modified / Deleted
    // - ( DB에 의해 생성된 Key가 없고 ) || ( C# 기본값 이라면 (==0) ) -> Added

    // 4) Attach
    // - Untracked Entity를 Tracked Entity로 변경해주는 기능
    // - ( DB에 의해 생성된 Key가 있는지 없는지 ) && ( C# 기본값이 인지 아닌지(!=0) ) -> Unchanged
    // - ( DB에 의해 생성된 Key가 없고 ) || ( C# 기본값 이라면 (==0) ) -> Added
    // - Attach는 주로 DB에 있는 내용을 확인할때 사용하는데, 다음과 같은 예를 들 수 있다.
    // 특정 Entity의 정보를 모두 알고 잇을 경우 해당 정보를 DB로 부터 긁어온다고 했을때의 과정을 살펴보면
    // 우선 해당 정보를 새로 선언하고 똑같이 설정한 후 Load를 통하여 가져오는것 보다
    // Attach를 이용해서 새로 선언한 정보를 Tracked Entity로 바꿔서 해당 정보를 가져오는 방법으로 사용 할 수 있다.

    #endregion

    #region State 조작
    // 직접 State를 조작할 수 있다. 주로 최적화 이유 때문
    // Entry().State = EntityState.원하는상태
    // Entry().Property("프로퍼티이름").IsModified = true

    // 상태조작할때 참조하는 값(= 네비게이셔널 프로퍼티)을 가지고 있으면 복잡해질 경우가 생길 수 있는데 다음과 같다
    // 릴레이션 쉽이 있는 트래킹 되지 않는 상태에서 조작을 할 경우에 데이터를 통으로 받아서 
    // Update를 호출 할 경우 네비게이셔널 프로퍼티도 같이 수정되는 경우가 생길 경우
    // 만약 특정 프로퍼티만 수정을 하고 싶을 때 해당 프로퍼티의 상태 값을 바꿔주는 것을 사용하면 굉장히 유용하게 사용할 수 있다.
    // 특정 프로퍼티의 상태는 Unchanged상태로 둬서 변화를 주게 하지 않는다는 등
    // 이때 등장하는 것이 TrackGraph

    // - TrackGraph
    // Relationship이 있는 Untracked Entity의 State 조작을 보다 쉽게 할 수 잇다.
    // ex) 전체 데이터 중에서 일부만 갱신하고 싶을 경우

    // - ChangeTracker
    // 상태 정보의 변화를 감지하고 싶을 때 사용
    // ex) 특정 플레이어의 프로퍼티 값이 바뀔 경우 로그를 남기고 싶을 때
    // ex) Validation 코드를 넣고 싶을 경우
    // ex) 플레이어가 생성된 시점을 CreateTime으로 정보를 추가하고 싶을 경우
    // 과정
    // 1) SaveChanges를 override
    // 2) ChangeTracker Enrites를 이용해서 바뀔 정보를 추출 / 사용 ( 로그 등 )
    
    // DB.SaveChanges의 위치에 따른 해석                
    // 1) 모든 구문이 끝나고 나서 SaveChange 호출
    // 2) 각 구문이 끝날 때 SaveChange 호출
    // 우선 SaveChange를 호출하는 것은 Transaction을 하는것과 동일하다고 생각 할 수 있다.
    // 일련의 과정을 최종적으로 DB에 적용하라는것이라는 것인데, 모든 구문이 끝나고 나서 SaveChange를 할경우
    // 각 구문에서 하나라도 에러가 난다면 DB에 반영이 되지 않는 것을 의미한다.
    // 반면 각 구문이 끝나고 나서 SaveChange를 호출할때는 서로 연관되지 않고 Transaction을 하는 것과 같다.

    // 만들어진 시간 추적하기 위한 인터페이스
    // 상속받는 대상이 SetCrateTime을 구현하고 SaveChange할때 해당 시점에서의 시간을 저장한다.
    public interface ILogentity
    {
        DateTime CreateTime { get; }
        void SetCreateTime();
    }
    #endregion

    #region SQL 직접 호출
    // 경우에 따라 직접 만든 SQL을 호출할 수 있다.
    // ex) LINQ로 처리할 수 없는 것 -> Stored Procedure 호출 등
    // ex) 성능 최적화 등

    // 1) FromSql -> FromSqlRaw / FromSqlInterpolated 로 쪼개짐
    // - SELECT 구문한 질의
    
    // 2) ExecuteSqlCommand -> ExecuteSqlRaw / ExecuteSqlInterpolated 로 쪼개짐
    // - SELECT를 제외한 쿼리

    // 3) Reload    
    // - Trackect Entity를 다루고 있을 때 2)번에 의해 DB 정보가 변경 될 경우의 상황
    // 이럴 경우 당연히 다루고 있는 Entity의 정보는 갱신되지 않는다.
    // 이럴 때 사용하는 것이 Reload 이다. ( Entity와 DB를 맞춰 주는 것 )
    // DB.Entry(맞춰줄 대상).Reload();

    #endregion
    // DB 관계 모델링할때
    // 1  : 1
    // 1  : 다
    // 다 : 다

    public enum ItemType
    {
        NormalItem,
        EventItem
    }

    public class ItemOption
    {
        public int Str { get; set; }
        public int Dex { get; set; }
        public int HP { get; set; }
    }

    // Table Splitting Test
    public class ItemDetail
    {
        public int ItemDetailId { get; set; }
        public string Description { get; set; }
    }
    //DbSet<클래스이름> 변수이름 변수이름이 DB에 테이블 이름으로 저장된다.
    //변수명으로 저장하고 싶지 않을때 [Table("저장하고 싶은 테이블 이름")] 이런식으로 지정하면 해당 테이블 명으로 DB에 저장된다.
    //이와 같이 테이블에 해당하는 
    [Table("Item")]
    public class Item
    {
        public ItemType Type { get; set; }

        private string _JsonData;
        public string JsonData
        {
            get { return _JsonData; }
            //set { _JsonData = value; } 
            //만약 특정 상황이 생겨서 해당 변수에 대해 set이 없을 경우 
            //EF Core의 규칙에 의해서 set이 없을 경우에는 DB에 저장이 되지 않는 문제가 생긴다.
        }

        // Owend Type - .OwnsOne() 연습
        public ItemOption Option { get; set; }

        // Table Splitting Test
        public ItemDetail Detail { get; set; }

        //함수를 이용해서 JsonData를 간접적으로 수정할때 해당 함수를 이용해서 _JsonData를 수정하고 있고
        //대신 JsonData에 set가 없기때문에 정작 DB에는 저장이 안되는 상황
        //이럴 경우에 쓰는 방식이 Backing Field 방식이다.
        //Fluent API에 다음과 같이 작성한다.
        //Builder.Entity<Item>() // ItemEntity에서
        //    .Property(i => i.JsonData) //JsonData라고 하는 변수에
        //    .HasField("_JsonData"); //_JsonData를 연동시켜줘 라고 하는 방식

        public void SetOption(ItemOption Option)
        {
            _JsonData = JsonConvert.SerializeObject(Option);
        }

        public ItemOption GetOption()
        {
            return JsonConvert.DeserializeObject<ItemOption>(_JsonData);
        }

        //해당 아이템이 삭제 대기 중인지 아닌지 
        public bool SoftDeleted { get; set; }
        //PrimaryKey
        public int ItemId { get; set; } = 0;
        //해당 아이템의 종류를 구분할 ID
        public int TemplateId { get; set; }
        public DateTime CreateDate { get; private set; }

        //다른 클래스 참조 -> 외부키 ( EF에서는 외부키를 Navigational Property라고 부름 )
        //외부키로 존재하면 외부키를 들고 잇는 클래스를 DB에 저장할떄 알아서 외부키를 저장해준다.
        //대신 외부키로 설정해서 테이블에 저장하면 외부키에 대해서 질의 하고 싶을때 한번 거쳐서 질의를 해야하는 단점이 존재한다.
        //그래서 보통은 그냥 외부키로 설정해놓은것도 변수에 저장해서 관리한다.
        //외부키를 굳이 설정하지 않더라도 다른 테이블의 이름을 변수로 적어두면 EF가 알아서 외부키로 잡아준다.
        public int OwnerId { get; set; }
        public Player Owner { get; set; }

        public int? CreatorId { get; set; }
        public Player Creator { get; set; }

        // UDF Test
        public  ICollection<ItemReview> Reviews { get; set; }
    }

    public class EventItem : Item
    {
        public DateTime DestroyDate { get; set; }
    }

    //1 : 다 구조
    //[Table("Player")]
    //public class CPlayer
    //{
    //    //클래스이름에 Id를 붙이면 해당 변수가 PrimaryKey로 설정된다.
    //    public int CPlayerId { get; set; }
    //    public string Name { get; set; }

    //    public ICollection<CItem> Items { get; set; }
    //}

    //1 : 1 구조
    //1 : 1 구조에서 외부키로 사용할때 한쪽에 외부키를 명시해주지 않으면
    //EF에서 판단을 못하여 에러를 뱉는다.
    //외부키를 명시해주는 방법은 외부키 위에 외부키 + Id를 선언해서 만들어주거나
    //[ForeignKey("외부키 구분자 이름")]를 이용해 선언해준다.
    //또한 1 : 1 구조에서 똑같은 외부키 값을 넣게 되면 전에 입력한 외부키 값이 null로 밀려 있게된다.
    [Table("Player")]
    public class Player : ILogentity
    {
        //클래스이름에 Id를 붙이면 해당 변수가 PrimaryKey로 설정된다.
        public int PlayerId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [InverseProperty("Owner")]
        public Item OwnedItem { get; set; }
        [InverseProperty("Creator")]
        public ICollection<Item> CreatedItems { get; set; }

        public Guild Guild { get; set; }

        //Change Track Test
        public DateTime CreateTime { get; private set; }

        public void SetCreateTime()
        {
            CreateTime = DateTime.Now;
        }
    }

    //플레이어와 1 : 다 구조
    [Table("Guild")]
    public class Guild
    {
        public int? GuildId { get; set; }
        public string GuildName { get; set; }
        public ICollection<Player> Members { get; set; }
    }

    // DTO ( Data Transfer Object )
    // DB에 접근한 대상을 컨텐츠 단에 바로 넘기지 않고 재가공해서 넘겨줄때 사용하는 오브젝트
    // 앞서 설명한 로딩 관련한 세번째 구조인 특정 길드에 있는 길드원 수를 추출하는 SelectLoading를 예로 들면
    public class GuildDTO
    {
        public int? GuildId { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
    }
}
