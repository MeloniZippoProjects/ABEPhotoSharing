using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KPTrustedParty.Database
{
    public partial class KpDatabase
    {
        public class Token
        {
            [Key, ForeignKey("User")]
            public string UserName { get; set; }

            public virtual User User { get; set; }

            public string TokenString { get; set; }

            public DateTime ExpirationDateTime { get; set; }
        }

        public static Token TokenExists(string sessionToken)
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                Token findToken = db.Tokens.FirstOrDefault(token => token.TokenString.Equals(sessionToken));

                return findToken;
            }
        }
    }
}