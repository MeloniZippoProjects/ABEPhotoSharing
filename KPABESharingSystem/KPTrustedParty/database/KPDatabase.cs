using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KPTrustedParty.Properties;

namespace KPTrustedParty.Database
{
    public partial class KpDatabase
    {
        private static readonly RNGCryptoServiceProvider RngCsp = new RNGCryptoServiceProvider();

        private static int PasswordHashSize = Settings.Default.PasswordHashSize;
        private static int SaltedHashIterations = Settings.Default.SaltedHashIterations;

        public class KpDatabaseContext : DbContext
        {
            private static readonly string DbConnectionString = @"Data Source=(LocalDb)\mssqllocaldb;" +
                                                       "Initial Catalog=kpdatabase2;Integrated Security=SSPI;" +
                                                       "AttachDBFilename=" + Directory.GetCurrentDirectory() +
                                                       @"\KPDatabase.mdf";

            
            public KpDatabaseContext() : base(DbConnectionString)
            {
                System.Data.Entity.Database.SetInitializer<KpDatabaseContext>(new CreateDatabaseIfNotExists<KpDatabaseContext>());
            }

            public DbSet<User> Users { get; set; }
            public DbSet<Token> Tokens { get; set; }
            public DbSet<Universe> Universes { get; set; }
        }

        public static void ResetUniverse()
        {
            using (var db = new KpDatabaseContext())
            {
                db.Universes.RemoveRange(db.Universes.ToList());

                foreach (var user in db.Users.ToList())
                {
                    user.Policy = "";
                    user.PrivateKey = new byte[0];
                }

                db.SaveChanges();
            }
        }
    }

    
}