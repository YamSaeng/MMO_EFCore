using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MMO_EFCore
{
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
        public string Name { get; set; }

        public Item Item { get; set; }
        public Guild Guild { get; set; }
    }

    //플레이어와 1 : 다 구조
    [Table("Guild")]
    public class Guild
    {
        public int GuildId { get; set; }
        public string GuildName { get; set; }
        public ICollection<Player> Members { get; set; }
    }

    // DTO ( Data Transfer Object )
    // DB에 접근한 대상을 컨텐츠 단에 바로 넘기지 않고 재가공해서 넘겨줄때 사용하는 오브젝트
    // 앞서 설명한 로딩 관련한 세번째 구조인 특정 길드에 있는 길드원 수를 추출하는 SelectLoading를 예로 들면
    public class GuildDTO
    {
        public int GuildId { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
    }
}
