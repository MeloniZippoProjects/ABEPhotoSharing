using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KPTrustedParty.Database
{
    public partial class KpDatabase
    {
        public class Universe
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Version { get; set; }

            /// <summary>
            /// The value of the universe in form of a string. 
            /// Can be used with KPService.Universe
            /// </summary>
            public string UniverseString { get; set; }

            public byte[] MasterKey { get; set; }

            public byte[] PublicKey { get; set; }
        }

        public static Universe GetLatestUniverse()
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                return db.Universes.OrderByDescending(universe => universe.Version).FirstOrDefault();
            }
        }

        public static Universe GetUniverse(int version)
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                return db.Universes.FirstOrDefault(universe => universe.Version == version);
            }
        }

        public static void InsertUniverse(string universeString, byte[] masterKey, byte[] publicKey)
        {
            using (KpDatabaseContext db = new KpDatabaseContext())
            {
                db.Universes.Add(new Universe
                {
                    UniverseString = universeString,
                    MasterKey = masterKey,
                    PublicKey = publicKey
                });
                try
                {
                    db.SaveChanges();
                }
                catch (ValidationException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}