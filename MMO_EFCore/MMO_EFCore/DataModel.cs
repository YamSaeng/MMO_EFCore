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
    //      public class Item
    //      {
    //          public int OwnerId { get; set; }
    //          [InverseProperty("OwnedItem")] Owner에 대응되는 Item을 OwnedItem으로 설정
    //          public Player Owner { get; set; }
    //          public int CreatorId { get; set; }
    //          [InverseProperty("CreatedItems")] Creator에 대응되는 Item을 CreatedItems으로 설정
    //          public Player Creator { get; set; }
    //      }
    //      반대의 경우도 성립된다. 위에서는 Player에 연동되는 Item을 설정한것이라면
    //      아래처럼 Item에 연동되는 Player를 설정할 수도 있는것
    //      Mssql 버그중 하나로 FK를 둘다 Nullable로 설정하지 않고 같은 테이블을 참조하면 나중에 Cascade할때 에러를 내뱉는 버그가 잇어서
    //      둘 중 하나는 Nullable로 설정해줘야한다.

    //      public class Player
    //      {
    //          [InverseProperty("Owner")] OwnedItem에 대응되는 Player를 Owner로
    //          public Item OwnedItem { get; set; }
    //          [InverseProperty("Creator")] CreatedItems에 대응되는 Player를 Creator로
    //          public ICollection<Item> CreatedItems { get; set; }
    //      }

    // Fluent API로 Relationship 설정
    // .HasOne() - 1 : 1 관계   .HasMany() - 1 : N  나
    // .WithOne() - 1 : 1 관계  .WithMany() - 1 : N 상대방쪽
    // .HasForeignKey()  .IsRequired()  .OnDelete()
    // .HasConstraintName()  .HasPrincipalKey()

    // DB 관계 모델링할때
    // 1  : 1
    // 1  : 다
    // 다 : 다

    //DbSet<클래스이름> 변수이름 변수이름이 DB에 테이블 이름으로 저장된다.
    //변수명으로 저장하고 싶지 않을때 [Table("저장하고 싶은 테이블 이름")] 이런식으로 지정하면 해당 테이블 명으로 DB에 저장된다.
    //이와 같이 테이블에 해당하는 
    [Table("Item")]
    public class Item
    {
        //해당 아이템이 삭제 대기 중인지 아닌지 
        public bool SoftDeleted { get; set; }
        //PrimaryKey
        public int ItemId { get; set; }
        //해당 아이템의 종류를 구분할 ID
        public int TemplateId { get; set; }
        public DateTime CreateDate { get; set; }

        //다른 클래스 참조 -> 외부키 ( EF에서는 외부키를 Navigational Property라고 부름 )
        //외부키로 존재하면 외부키를 들고 잇는 클래스를 DB에 저장할떄 알아서 외부키를 저장해준다.
        //대신 외부키로 설정해서 테이블에 저장하면 외부키에 대해서 질의 하고 싶을때 한번 거쳐서 질의를 해야하는 단점이 존재한다.
        //그래서 보통은 그냥 외부키로 설정해놓은것도 변수에 저장해서 관리한다.
        //외부키를 굳이 설정하지 않더라도 다른 테이블의 이름을 변수로 적어두면 EF가 알아서 외부키로 잡아준다.
        public int OwnerId { get; set; }
        public Player Owner { get; set; }

        public int? CreatorId { get; set; }
        public Player Creator { get; set; }
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
    public class Player
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
