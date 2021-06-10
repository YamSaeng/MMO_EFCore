using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMO_EFCore
{
    public static class Extensions
    {
        public static IQueryable<GuildDTO> MapGuildToDto(this IQueryable<Guild> guild)
        {
            return guild.Select(g => new GuildDTO()
            {
                GuildId = g.GuildId,
                Name = g.GuildName,
                MemberCount = g.Members.Count
            });
        }
    }
}
