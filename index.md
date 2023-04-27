# Faithlife.FakeData

**Faithlife.FakeData** is a fake in-memory relational database for prototypes and unit tests.

[![NuGet](https://img.shields.io/nuget/v/Faithlife.FakeData.svg)](https://www.nuget.org/packages/Faithlife.FakeData)

## Overview

Why use a fake in-memory database? In-memory databases can shorten the development cycle, especially during early development: they don't require disk resources, the database schema is easier to change, unit tests against an in-memory database run quickly, etc.

Why use a fake *relational* in-memory database? When it comes time to create an actual relational database implementation, the more the fake database looks like the actual database, the easier it will be.

While creating the actual database implementation, it's nice to already have passing unit tests. Once the actual database implementation is created, you can abandon the fake database, or you can keep both, and run unit tests against both the fake and the real databases. The justification of continuing to maintain the fake database is debatable, but you may find it useful for ongoing development, e.g. by writing new features against the fake database at first, and then following up with the actual implementation. Having two database implementations also encourages good abstractions, helping prevent database code from leaking into higher layers.

**Faithlife.FakeData** provides a trivial implementation of an in-memory relational database. There are no indexes, so every query is basically a "full table scan," which can be slow for large databases, especially for joins. This library is therefore great for prototypes and unit tests, but not appropriate for production.

The fake database does basic validation of column data, but there is room for improvement. For example, it does not currently support defining and enforcing foreign keys or unique columns.

Consult the [reference documentation](Faithlife.FakeData.md) for additional details.

## Usage

Each table of your fake database should have a corresponding "record" class. [(Try it!)](https://dotnetfiddle.net/f7PWM0)

Each column of the table should have a corresponding read-write property on the record class. Use property types that correspond to data types supported by actual relational databases (integers, strings, etc.). Use a nullable type if the column should allow null.

Some attributes from [`System.ComponentModel.DataAnnotations`](https://docs.microsoft.com/dotnet/api/system.componentmodel.dataannotations) are also supported:

* [`KeyAttribute`](https://docs.microsoft.com/dotnet/api/system.componentmodel.dataannotations.keyattribute): Indicates the primary key. If the column type is `int` or `long`, it defaults to an "auto-increment" value when a row is added.
* [`RequiredAttribute`](https://docs.microsoft.com/dotnet/api/system.componentmodel.dataannotations.requiredattribute): Prevents the column from being set to `null`.
* [`StringLengthAttribute`](https://docs.microsoft.com/dotnet/api/system.componentmodel.dataannotations.stringlengthattribute): Prevents the column from being set to a string longer than the specified length.
* [`RegularExpressionAttribute`](https://docs.microsoft.com/dotnet/api/system.componentmodel.dataannotations.regularexpressionattribute): Prevents the column from being set to a string that doesn't match the specified regular expression.

```csharp
public sealed class UserRecord
{
    [Key]
    public long UserId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [RegularExpression(@"^[a-zA-Z0-9_]+$")]
    public string? Alias { get; set; }
}

public sealed class UserRoleRecord
{
    [Key]
    public long UserRoleId { get; set; }

    public long UserId { get; set; }

    public int Role { get; set; }
}
```

The database schema is represented by a class that derives from [`FakeDatabaseContext`](Faithlife.FakeData/FakeDatabaseContext.md). It should have one read-only property of type [`FakeDatabaseTable<T>`](Faithlife.FakeData/FakeDatabaseTable-1.md) for each table in the database. Initialize each property in the constructor with a call to [`CreateTable<T>()`](Faithlife.FakeData/FakeDatabaseContext/CreateTable.md).

```csharp
public sealed class UserDatabaseContext : FakeDatabaseContext
{
    public UserDatabaseContext()
    {
        Users = CreateTable<UserRecord>();
        UserRoles = CreateTable<UserRoleRecord>();
    }

    public FakeDatabaseTable<UserRecord> Users { get; }

    public FakeDatabaseTable<UserRoleRecord> UserRoles { get; }
}
```

To create an empty database, call [`FakeDatabase.Create<T>()`](Faithlife.FakeData/FakeDatabase/Create.md).

```csharp
var database = FakeDatabase.Create<UserDatabaseContext>();
```

To add rows to a table, lock the database and call [`Add`](Faithlife.FakeData/FakeDatabaseTable-1/Add.md) or [`AddRange`](Faithlife.FakeData/FakeDatabaseTable-1/AddRange.md) on the table from the returned context.

```csharp
using var context = database.Lock();
var alice = context.Users.Add(new UserRecord { Name = "Alice", Alias = "4l1c3" });
var bob = context.Users.Add(new UserRecord { Name = "Bob", Alias = "b0b" });
context.UserRoles.AddRange(new[]
{
  new UserRoleRecord { UserId = alice.UserId, Role = 1 },
  new UserRoleRecord { UserId = bob.UserId, Role = 2 },
  new UserRoleRecord { UserId = bob.UserId, Role = 1 },
});
```

To run a query, use LINQ to Objects on the tables, which implement `IEnumerable<T>`.

```csharp
var aliases = context.Users.Select(x => x.Alias).Distinct();
```

LINQ query syntax is nice for inner joins.

```csharp
var namesWithRoleTwo =
    from user in context.Users
    join role in context.UserRoles on user.UserId equals role.UserId
    where role.Role == 2
    select user.Name;
```

To update rows in a table, call [`UpdateWhere`](Faithlife.FakeData/FakeDatabaseTable-1/UpdateWhere.md), which returns the number of rows affected.

```csharp
context.Users.UpdateWhere(x => x.UserId == bob.UserId, x => x.Name = "Robert");
```

To remove frows from a table, call [`RemoveWhere`](Faithlife.FakeData/FakeDatabaseTable-1/RemoveWhere.md), which also returns the number of rows affected.

```csharp
context.Users.RemoveWhere(x => x.UserId == bob.UserId);
```

If you maintain both the fake database and the actual database, consider using the record classes with the actual database implementation. They can be useful when working with micro-ORMs like Dapper, e.g. by mapping a `SELECT` query into one or more record classes, or by using a record instance to supply parameters to an `INSERT` query.
