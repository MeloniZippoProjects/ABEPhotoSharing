using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KPTrustedParty.Database
{
    public partial class KpDatabase
    {
        public class User
        {
            [Key, RegularExpression(@"[\w\d]{3,21}")]
            public string Name { get; set; }

            [Required]
            public byte[] Salt { get; set; }

            [Required]
            public byte[] SaltedPasswordHash { get; set; }

            public string Policy { get; set; }

            /// <summary>
            /// Private Key of User. 
            /// The value of this property is the binary
            /// content of the private key file.
            /// </summary>
            public byte[] PrivateKey { get; set; }

            public virtual Token Token { get; set; }

            public override string ToString()
            {
                string retString = $"Username: {Name} ; Policy: {Policy};";
                return retString;
            }
        }

        public static void RegisterUser(string username, string password)
        {
            if (password.Length < 8)
            {
                Console.WriteLine("Password is too short: at least 8 characters");
                return;
            }

            User newUser = new User {Name = username};

            byte[] salt = new byte[256];
            RngCsp.GetBytes(salt);
            newUser.Salt = salt;

            var passwordHashAlgorithm = new Rfc2898DeriveBytes(password: password, salt: salt,
                iterations: SaltedHashIterations);
            newUser.SaltedPasswordHash = passwordHashAlgorithm.GetBytes(PasswordHashSize);

            try
            {
                using (KpDatabaseContext db = new KpDatabaseContext())
                {
                    db.Users.Add(newUser);
                    db.SaveChanges();
                }
            }
            catch (DbEntityValidationException validationException)
            {
                foreach (DbEntityValidationResult entityError in validationException.EntityValidationErrors)
                {
                    User invalidUser = (User) entityError.Entry.Entity;
                    Console.WriteLine($"There are validation errors in entity {invalidUser.Name}");
                    foreach (DbValidationError validationError in entityError.ValidationErrors)
                    {
                        Console.WriteLine($"{validationError.PropertyName}: {validationError.ErrorMessage}");
                    }
                }
            }
        }

        public static bool RemoveUser(string username)
        {
            using (var db = new KpDatabaseContext())
            {
                var targetUser = db.Users.Find(username);
                if (targetUser != null)
                {
                    db.Users.Remove(targetUser);
                    db.SaveChanges();
                }

                
                return targetUser != null;
            }
        }

        public static User AuthenticateUser(string username, string password)
        {
            using (var db = new KpDatabaseContext())
            {
                User targetUser = db.Users.Find(username);
                if (targetUser == null)
                    return null;

                byte[] salt = targetUser.Salt;
                byte[] hashedPassword = targetUser.SaltedPasswordHash;

                var passwordHashAlgorithm = new Rfc2898DeriveBytes(password: password, salt: salt,
                    iterations: SaltedHashIterations);

                byte[] hashedInput = passwordHashAlgorithm.GetBytes(PasswordHashSize);
                return hashedInput.SequenceEqual(hashedPassword) ? targetUser : null;
            }
        }

        public static Token LoginUser(string username, string password)
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                User authUser = AuthenticateUser(username, password);
                if (authUser != null)
                {
                    byte[] tokenBytes = new byte[256 / 8];
                    RngCsp.GetBytes(tokenBytes);
                    string tokenString = Convert.ToBase64String(tokenBytes);

                    IQueryable<Token> existingTokenQuery = from token in db.Tokens
                        where token.User.Name == authUser.Name
                        select token;

                    Token returnToken;
                    if (existingTokenQuery.Count() == 1)
                    {
                        Token currentToken = existingTokenQuery.FirstOrDefault();
                        Debug.Assert(currentToken != null, nameof(currentToken) + " != null");
                        currentToken.TokenString = tokenString;
                        currentToken.ExpirationDateTime = DateTime.Now.AddMinutes(30);
                        returnToken = currentToken;
                    }
                    else
                    {
                        Token newToken = new Token
                        {
                            TokenString = tokenString,
                            ExpirationDateTime = DateTime.Now.AddMinutes(30),
                            UserName = authUser.Name
                        };
                        db.Tokens.Add(newToken);

                        returnToken = newToken;
                    }

                    try
                    {
                        db.SaveChanges();
                        return returnToken;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return null;
                    }
                }

                return null;
            }
        }

        public static void DetailUser(string username)
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                User targetUser = db.Users.Find(username);
                if (targetUser == null)
                    Console.WriteLine($"User {username} was not found.");
                else
                    Console.WriteLine(targetUser);
            }
        }

        public static void ListUsers()
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                Console.WriteLine($"Total users count: {db.Users.Count()}");
                foreach (User user in db.Users.ToList())
                {
                    Console.WriteLine(user);
                }
            }
        }

        public static IEnumerable<User> GetUsersList()
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                return db.Users.ToList();
            }
        }

        public static User UserLogged(String sessionToken)
        {
            if (sessionToken == null)
                return null;
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                Token token = TokenExists(sessionToken);
                if (token != null && token.ExpirationDateTime.CompareTo(DateTime.Now) > 0)
                    return db.Users.First(user => user.Token.TokenString == token.TokenString);
                return null;
            }
        }

        public static void SetUserPolicy(string username, string policy)
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                User user = db.Users.FirstOrDefault(dbUser => dbUser.Name == username);
                if (user != null)
                {
                    user.Policy = policy;
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void SetUserPrivateKey(string username, byte[] privateKey)
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                User user = db.Users.FirstOrDefault(dbUser => dbUser.Name == username);
                if (user != null)
                {
                    user.PrivateKey = privateKey;
                }
                try
                {
                    db.SaveChanges();
                }
                catch (ValidationException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }
    }
}