using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace KPTrustedParty
{
    public partial class KPDatabase
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        private static SHA256 sha256 = SHA256CryptoServiceProvider.Create();

        public class KPDatabaseContext : DbContext
        {
            private static string DBConnectionString = @"Data Source=(LocalDb)\mssqllocaldb;" + 
                "Initial Catalog=database;Integrated Security=SSPI;" + 
                "AttachDBFilename=" + Directory.GetCurrentDirectory() + @"\KPDatabase.mdf";

            public KPDatabaseContext() : base(DBConnectionString) { }

            public DbSet<User> Users { get; set; }
            public DbSet<Token> Tokens { get; set; }
            public DbSet<Universe> Universes { get; set; }
        }

        //todo: implement authentication.
    }

    
}
