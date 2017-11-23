using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KPTrustedParty
{
    public partial class KPDatabase
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
            using (var db = new KPDatabaseContext())
            {
                var findToken = db.Tokens.FirstOrDefault(token => token.TokenString.Equals(sessionToken));

                return findToken;
            }
        }
    }
}