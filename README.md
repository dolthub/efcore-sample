# efcore-sample
Sample code demonstrating how to use .NET Entity Framework Core (EF Core) to access a [Dolt](https://doltdb.com/) database.

For a walkthrough of this sample code, [check out the associated post on the DoltHub blog](https://dolthub.com/blog/2023-12-04-works-with-dolt-efcore/).

## Project Overview
This sample project shows how to configure a .NET Entity Framework Core project to connect to a Dolt database and then query it using the Entity Framework Core APIs. The major components of this project are:
- `dolt` dir – This directory holds a pre-made Dolt database. This database has the application's schema already created, but doesn't have any data in it yet.
- `DoltDbContext.cs` – This C# file contains the `DbContext` implementation for our EF Core application, which configures the connection to our Dolt database.
- `Models.cs` – This C# file contains the two model classes that define the entities this application works with.
- `Program.cs` – This C# file contains the `main` method that runs this application. It connects to the Dolt database, creates new entities, and runs various types of queries. 

## Running This Sample

### Install the Dolt binary
You can find [Dolt's installation instructions online](https://github.com/dolthub/dolt#installation). I use `brew install dolt` on my Mac, but the Dolt install instructions provide options for using a Windows MSI, Chocolatey, or even building from source. 

### Start up a Dolt `sql-server`
Move into the `dolt` subdirectory of this project and run: `dolt sql-server -uroot --port 11229`. Note that if you want to run on a different port, you'll need to update `DoltDbContext.cs` to use that same port, too. 

### Run the .NET project
Run `dotnet restore` to ensure all the project's dependencies have been restored, then you can run `dotnet run` to executed the code in `Program.cs`. 

When you run the project using `dotnet run`, you should see output similar to:
```
❯ dotnet run 
Current branch: main 

Inserting new products and customers..
Querying for customers using product ID 08dbf2c4-9e42-425a-840a-0cc01a0c431e:
  - Especially Basket (ID: 08dbf2c4-9e45-487a-88da-2a0f725c19d8)

Creating new branch 'branch-1d07fbfb-e63c-4515-b64c-81fe22382972'

Switched to branch: branch-1d07fbfb-e63c-4515-b64c-81fe22382972
Changed tables between main and branch-1d07fbfb-e63c-4515-b64c-81fe22382972:
  - Customers (modified - data change)
```


## Problems? Questions?
If you've found an issue with this sample project, feel free to [create an issue in this project's GitHub repo](https://github.com/dolthub/efcore-sample/issues/new). 

If you've found a problem with Dolt working correctly with .NET Entity Framework core, feel free to [create an issue in the Dolt GitHub repo](https://github.com/dolthub/dolt/issues/new).

If you just wanna ask questions or discuss how to use Dolt, then please [swing by our Discord server](https://discord.gg/gqr7K4VNKe) and come find us! Our dev team spends our days on Discord and we love it when customers come by to chat about databases, versioning, or programming frameworks! 🤓
