using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using System.Security.Cryptography;

namespace KPTrustedParty
{
    public class KPDatabase
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        private static SHA256 sha256 = SHA256CryptoServiceProvider.Create();

        public class User
        {
            [Key]
            public string Name { get; set; }

            [Required]
            public byte[] Salt { get; set; }
            [Required]
            public byte[] SaltedPasswordHash { get; set; }

            public string Policy { get; set; }  //todo: change to an object for better processing?

            public override string ToString()
            {
                //todo: why should we print salt & stuff?
                return $"Username: {Name} ; Policy: {Policy}";  
            }
        }

        public class KPDatabaseContext : DbContext
        {
            private static string DBConnectionString = @"Data Source=(LocalDb)\mssqllocaldb;Initial Catalog=database;Integrated Security=SSPI;AttachDBFilename=" + Directory.GetCurrentDirectory() + @"\KPDatabase.mdf";

            public KPDatabaseContext() : base(DBConnectionString) { }

            public DbSet<User> Users { get; set; }
        }
        
        public static void RegisterUser(string username, string password)
        {
            //todo: should limit the char set for username and password?

            if (password.Length < 8)
            {
                Console.WriteLine("Password is too short: at least 8 characters");
                return;
            }

            User newUser = new User { Name = username };

            byte[] salt = new byte[256];
            rngCsp.GetBytes(salt);
            newUser.Salt = salt;

            byte[] toHash = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(salt) + password);
            newUser.SaltedPasswordHash = sha256.ComputeHash(toHash);

            try
            {
                using (var db = new KPDatabaseContext())
                {
                    db.Users.Add(newUser);
                    db.SaveChanges();
                }
            }
            catch(Exception e)
            {
                //todo: better handling of errors?
                Console.WriteLine(e.Message);
            }
        }

        //todo: implement authentication.
        public static bool AuthenticateUser(string username, string password)
        {
            return false;
        }

        public static void DetailUser(string username)
        {
            using (var db = new KPDatabaseContext())
            {
                User targetUser = db.Users.Find(username);
                if(targetUser == null)
                    Console.WriteLine($"User {username} was not found.");
                else
                    Console.WriteLine(targetUser);
            }
        }

        public static void ListUsers()
        {
            using (var db = new KPDatabaseContext())
            {
                Console.WriteLine($"Total users count: {db.Users.Count()}");
                foreach (User user in db.Users)
                {
                    Console.WriteLine(user);
                }
            }
        }
    }

    
}
