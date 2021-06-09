using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MMO_EFCore
{
    //DbSet<클래스이름> 변수이름 변수이름이 DB에 테이블 이름으로 저장된다.
    //변수명으로 저장하고 싶지 않을때 [Table("저장하고 싶은 테이블 이름")] 이런식으로 지정하면 해당 테이블 명으로 DB에 저장된다.
    [Table("Item")]
    public class CItem
    {
        //PrimaryKey
        public int CItemId { get; set; }
        //해당 아이템의 종류를 구분할 ID
        public int TemplateId { get; set; }
        public DateTime CreateDate { get; set; }

        //다른 클래스 참조 -> 외부키 ( EF에서는 외부키를 Navigational Property라고 부름 )
        public int OwnerId { get; set; }
        public CPlayer Owner { get; set; }
    }
    
    [Table("Player")]
    public class CPlayer
    {
        //클래스이름에 Id를 붙이면 해당 변수가 PrimaryKey로 설정된다.
        public int CPlayerId { get; set; }
        public string Name { get; set; }

    }
}
