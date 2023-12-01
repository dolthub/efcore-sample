using Microsoft.EntityFrameworkCore;
using System;

namespace DoltEFCoreDemo;

public class DoltDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }

    private string branchName;

    public DoltDbContext() {
        this.branchName = "main";
    }

    public DoltDbContext(string branchName) {
        // See the OnConfiguring function below to see how branchName is used
        this.branchName = branchName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // NOTE: To control which branch of the database we connect to, we can specify an existing branch 
        //       after the database name, separated by a forward slash.
        //       More info: https://docs.dolthub.com/sql-reference/version-control/branches#specify-a-database-revision-in-the-connection-string
        var dbAndBranch = "dolt/" + this.branchName;
        var connectionString = "server=localhost;user=root;password=;database=" + dbAndBranch + ";port=11229";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 34));
        options.UseMySql(connectionString, serverVersion);
    }
}