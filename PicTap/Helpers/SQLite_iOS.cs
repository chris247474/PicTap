using System;
using SQLite;
using System.IO;
using Foundation;

namespace PicTap
{
public class SQLite_iOS
    {
        public SQLite_iOS() { }
        public SQLite.SQLiteConnection GetConnection()
        {
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
            string libraryPath = System.IO.Path.Combine(documentsPath, "..", "Library"); // Library folder
            var path = System.IO.Path.Combine(documentsPath, "CAPPDB26.db3");

            if (!File.Exists(path))
            {
                Console.WriteLine("Database doesn't exist yet, copying one-----------------------------------------------------------------------");
                var existingDb = NSBundle.MainBundle.PathForResource("people", "db3");
                File.Copy(existingDb, path);
            }

            var conn = new SQLite.SQLiteConnection(path);

            // Return the database connection 
            return conn;
        }

        public SQLiteConnection GetConnectionCAPP()
        {
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
            
            string libFolder = Path.Combine(documentsPath, "..", "Library", "Databases");

            if (!Directory.Exists(libFolder))
            {
                Directory.CreateDirectory(libFolder);
            }

            var path = Path.Combine(libFolder, "CAPPDB26.db3");

            if (!File.Exists(path))
            {
                Console.WriteLine("CAPP Database doesn't exist yet, copying one-----------------------------------------------------------------------");
                if (!File.Exists(path))
                {
                    Console.WriteLine("Database doesn't exist yet, copying one-----------------------------------------------------------------------");
                    var existingDb = NSBundle.MainBundle.PathForResource("CAPPDB26", "db3");
                    File.Copy(existingDb, path);
                }
            }

            var conn = new SQLite.SQLiteConnection(path);

            // Return the database connection 
            return conn;
        }

    }
}
