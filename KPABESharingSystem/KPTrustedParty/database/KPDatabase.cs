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

        private static readonly int PasswordHashSize = Settings.Default.PasswordHashSize;
        private static readonly int SaltedHashIterations = Settings.Default.SaltedHashIterations;
        private static readonly string DatabaseName = Settings.Default.DatabaseName;
        private static readonly string DatabaseFile = Settings.Default.DatabaseFile;

        public class KpDatabaseContext : DbContext
        {
            private static readonly string DbConnectionString = @"Data Source=(LocalDb)\mssqllocaldb;" +
                                                       "Initial Catalog=" + DatabaseName +";Integrated Security=SSPI;" +
                                                       "AttachDBFilename=" + Path.Combine(Directory.GetCurrentDirectory(),DatabaseFile);

            
            public KpDatabaseContext() : base(DbConnectionString)
            {
                System.Data.Entity.Database.SetInitializer<KpDatabaseContext>(new CreateDatabaseIfNotExists<KpDatabaseContext>());
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Token>()
                    .HasRequired(d => d.User)
                    .WithOptional(u => u.Token)
                    .WillCascadeOnDelete(true);
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