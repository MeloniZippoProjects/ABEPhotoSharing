using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace KPTrustedParty
{
    public partial class KPDatabase
    {
        public class User
        {
            [Key]
            public string Name { get; set; }

            [Required]
            public byte[] Salt { get; set; }

            [Required]
            public byte[] SaltedPasswordHash { get; set; }

            public string Policy { get; set; } //todo: change to an object for better processing?

            /// <summary>
            /// Private Key of User. 
            /// The value of this property is the binary
            /// content of the private key file.
            /// </summary>
            public byte[] PrivateKey { get; set; }

            public virtual Token Token { get; set; }

            public override string ToString()
            {
                //todo: why should we print salt & stuff?    
                var retString = $"Username: {Name} ; Policy: {Policy};";
                if (this.Token != null)
                    retString += $" Session Ends: {Token.ExpirationDateTime.ToLocalTime()}; ";
                return retString;
            }
        }

        public static void RegisterUser(string username, string password)
        {
            //todo: should limit the char set for username and password?

            if (password.Length < 8)
            {
                Console.WriteLine("Password is too short: at least 8 characters");
                return;
            }

            User newUser = new User {Name = username};

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
            catch (Exception e)
            {
                //todo: better handling of errors?
                Console.WriteLine(e.Message);
            }
        }

        public static User AuthenticateUser(string username, string password)
        {
            using (var db = new KPDatabaseContext())
            {
                User targetUser = db.Users.Find(username);
                if (targetUser == null)
                    return null;

                byte[] salt = targetUser.Salt;
                byte[] hashedPassword = targetUser.SaltedPasswordHash;

                byte[] toHash = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(salt) + password);
                byte[] hashed = sha256.ComputeHash(toHash);
                return hashed.SequenceEqual(hashedPassword) ? targetUser : null;
            }
        }

        public static Token LoginUser(string username, string password)
        {
            using (var db = new KPDatabaseContext())
            {
                User authUser = AuthenticateUser(username, password);
                if (authUser != null)
                {
                    byte[] tokenBytes = new byte[128];
                    rngCsp.GetBytes(tokenBytes);
                    var tokenString = Convert.ToBase64String(tokenBytes);

                    var existingTokenQuery = from token in db.Tokens
                        where token.User.Name == authUser.Name
                        select token;

                    Token returnToken = null;
                    if (existingTokenQuery.Count() == 1)
                    {
                        var currentToken = existingTokenQuery.FirstOrDefault();
                        Debug.Assert(currentToken != null, nameof(currentToken) + " != null");
                        currentToken.TokenString = tokenString;
                        currentToken.ExpirationDateTime = DateTime.Now.AddMinutes(5);
                        returnToken = currentToken;
                    }
                    else
                    {
                        var newToken = new Token
                        {
                            TokenString = tokenString,
                            ExpirationDateTime = DateTime.Now.AddMinutes(5),
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
            using (var db = new KPDatabaseContext())
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
            using (var db = new KPDatabaseContext())
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
            using (var db = new KPDatabaseContext())
            {
                return db.Users.ToList();
            }
        }

        public static User UserLogged(String sessionToken)
        {
            if (sessionToken == null)
                return null;
            using (var db = new KPDatabaseContext())
            {
                var token = TokenExists(sessionToken);
                if (token != null && token.ExpirationDateTime.CompareTo(DateTime.Now) > 0)
                    return db.Users.First(user => user.Token.TokenString == token.TokenString);
                return null;
            }
        }

        public static void SetUserPolicy(string username, string policy)
        {
            using (var db = new KPDatabaseContext())
            {
                var user = db.Users.FirstOrDefault(dbUser => dbUser.Name == username);
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
            using (var db = new KPDatabaseContext())
            {
                var user = db.Users.FirstOrDefault(dbUser => dbUser.Name == username);
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