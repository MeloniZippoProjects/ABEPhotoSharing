using System.Data.Entity;
using System.IO;
using System.Security.Cryptography;

namespace KPTrustedParty.Database
{
    public partial class KpDatabase
    {
        private static readonly RNGCryptoServiceProvider RngCsp = new RNGCryptoServiceProvider();
        private static readonly SHA256 Sha256 = SHA256.Create();

        public class KpDatabaseContext : DbContext
        {
            private static readonly string DbConnectionString = @"Data Source=(LocalDb)\mssqllocaldb;" +
                                                       "Initial Catalog=database;Integrated Security=SSPI;" +
                                                       "AttachDBFilename=" + Directory.GetCurrentDirectory() +
                                                       @"\KPDatabase.mdf";

            public KpDatabaseContext() : base(DbConnectionString)
            {
            }

            public DbSet<User> Users { get; set; }
            public DbSet<Token> Tokens { get; set; }
            public DbSet<Universe> Universes { get; set; }
        }
    }
}