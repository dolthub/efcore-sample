using System;
using System.Linq;
using System.Data;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CrypticWizard.RandomWordGenerator;
using static CrypticWizard.RandomWordGenerator.WordGenerator;

namespace DoltEFCoreDemo;

internal class Program
{
    private static void Main(string[] args)
    {
        // Dolt functions and stored procedures can be accessed by executing SQL queries directly.
        // Here, we're using active_branch() to see what branch we're working with.
        // https://docs.dolthub.com/sql-reference/version-control/dolt-sql-functions#active_branch
        using (var db = new DoltDbContext()) {
            var activeBranch = db.Database.SqlQuery<string>($"select active_branch();").ToList();
            Console.WriteLine($"Current branch: {activeBranch.First()} \n");
        }

        // Create and Query some of our modeled data. Note that we're using the default DoltDbContext, which 
        // uses the main branch, like we saw above. 
        using (var db = new DoltDbContext()) {
            Console.WriteLine("Inserting new products and customers..");
            WordGenerator nameGenerator = new WordGenerator();
            List<PartOfSpeech> pattern = [PartOfSpeech.adv, PartOfSpeech.noun];
            string productName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(nameGenerator.GetPattern(pattern, ' '));
            string customerName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(nameGenerator.GetPattern(pattern, ' '));
            var product1 = new Product {Name = productName};
            var customer1 = new Customer { Name = customerName, Products = new List<Product>{product1}};
            db.Add(product1);
            db.Add(customer1);
            db.SaveChanges();

            // Query some data
            Console.WriteLine($"Querying for customers using product ID {product1.ProductId}:");
            var customer = db.Customers
                .Where(c => c.Products.Contains(product1))
                .OrderBy(c => c.Name)
                .First();
            Console.WriteLine($"  - {customer.Name} (ID: {customer.CustomerId})");

            // Create a Dolt commit
            db.Database.ExecuteSql($"call dolt_commit('-Am', 'inserting test data');");
        }

        // Now let's create a new branch and then write some data to it. We do this by calling the
        // dolt_branch() stored procedure. (We use a Guid to ensure the branch name is unique so 
        // that we can run this program multiple times without having to clean up data.)
        // https://docs.dolthub.com/sql-reference/version-control/dolt-sql-procedures#dolt_branch
        string branchName;
        using (var db = new DoltDbContext()) {
            var id = Guid.NewGuid();
            branchName = "branch-" + id;
            Console.WriteLine($"\nCreating new branch '{branchName}'\n");
            db.Database.ExecuteSql($"call dolt_branch({branchName});");
        }

        // Now that we've created a new branch, we use a new DoltDbContext and specify the branch name.
        // This causes the DbContext to use a different connection string, and forces our connection to
        // use a specific branch of the database. 
        // 
        // Note that because our connections are being pooled and managed by the ORM, we don't want to
        // use dolt_checkout() to change our branch, because that will change the branch for the shared
        // connection, and when that connection is reused, the code that is using it, probably won't 
        // expect that it has been pointed to a different branch. 
        using (var db = new DoltDbContext(branchName)) {
            var activeBranch = db.Database.SqlQuery<string>($"select active_branch();").ToList();
            Console.WriteLine($"Switched to branch: {activeBranch.First()}");

            // Add a new customer on this branch
            db.Add(new Customer { Name = $"Customer from {branchName}" });
            db.SaveChanges();

            // And create a Dolt commit on this branch
            db.Database.ExecuteSql($"call dolt_commit('-Am', 'inserting test data');");
        }

        // Using the main branch of our database again, we can query a summary of the differences between
        // main and our new branch using the dolt_diff_summary() table function. 
        // https://docs.dolthub.com/sql-reference/version-control/dolt-sql-functions#dolt_diff_summary
        using (var db = new DoltDbContext()) {
            var conn = db.Database.GetDbConnection();

            Console.WriteLine($"Changed tables between main and {branchName}:");
            using (var cmd = conn.CreateCommand()) {
                cmd.CommandText = $"select * from dolt_diff_summary('main', '{branchName}');";
                conn.Open();
                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        string tableName  = reader.GetString(reader.GetOrdinal("from_table_name"));
                        string diffType   = reader.GetString(reader.GetOrdinal("diff_type"));
                        bool dataChange   = reader.GetBoolean(reader.GetOrdinal("data_change"));
                        bool schemaChange = reader.GetBoolean(reader.GetOrdinal("schema_change"));

                        string changeType = "";
                        if (dataChange && schemaChange) {
                            changeType = "data and schema change";
                        } else if (dataChange) {
                            changeType = "data change";
                        } else if (schemaChange) {
                            changeType = "schema change";
                        }
                        Console.WriteLine($"  - {tableName} ({diffType} - {changeType})");
                    }
                }
            }
        }
    }
}
