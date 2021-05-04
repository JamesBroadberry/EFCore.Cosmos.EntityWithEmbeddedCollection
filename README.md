# Issue with EF Core Cosmos using Embedded Entities

This repository contains a project to replicate an issue found when using Embedded Entities in Cosmos with Entity Framework Core.

As far as I'm aware, the set-up for these embedded entities is correct and similar to the example provided in the [documentation](https://docs.microsoft.com/en-us/ef/core/providers/cosmos/?tabs=dotnet-core-cli#embedded-entities).

## Issue found

Running the solution as-is, provides the following error stack and seems to be thrown when adding the second (Jane's) order:

```log

Unhandled exception. System.InvalidOperationException: The property 'OrderItem.Id' is defined as read-only after it has been saved, but its value has been modified or marked as modified.
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.InternalEntityEntry.PrepareToSave()
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.GetEntriesToSave(Boolean cascadeChanges)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(DbContext _, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at EFCore.Cosmos.EntityWithEmbeddedCollection.Program.AddOrder(CosmosStoreDbContext context, String customerName) in C:\Users\XXXX\source\repos\EFCore.Cosmos.EntityWithEmbeddedCollection\EFCore.Cosmos.EntityWithEmbeddedCollection\Program.cs:line 56
   at EFCore.Cosmos.EntityWithEmbeddedCollection.Program.Main() in C:\Users\XXXX\source\repos\EFCore.Cosmos.EntityWithEmbeddedCollection\EFCore.Cosmos.EntityWithEmbeddedCollection\Program.cs:line 39
   at EFCore.Cosmos.EntityWithEmbeddedCollection.Program.<Main>()

```

It's worth noting that while debugging, inspecting the change tracker shows some interesting results and may help diagnose the issue.

This only seems to occur after multiple `OrderItem`s have been added and then another call to SaveChangesAsync is made (e.g. it can still be replicated using SaveChangesAsync before the line where janeOrder is created)

## Potential workaround - not ideal

One thing we've found which solves this issue is to put an ID on `OrderItem`. This seems to solve the issue but then the extra data stored in the database is superfluous, making the data look like this for one `Order`.

```json
{
    "Id": "31e1dd42-e98d-4b93-8e74-abb132a4d060",
    "CustomerName": "Bob",
    "Discriminator": "Order",
    "id": "Order|31e1dd42-e98d-4b93-8e74-abb132a4d060",
    "Items": [
        {
            "Id": "a0528614-0385-4abf-98cb-0f54047b876c",
            "ItemName": "Foo",
            "ItemPrice": 10,
            "OrderId": "31e1dd42-e98d-4b93-8e74-abb132a4d060"
        },
        {
            "Id": "84b33799-1eed-44e1-940d-e2ae46aeb208",
            "ItemName": "Bar",
            "ItemPrice": 20,
            "OrderId": "31e1dd42-e98d-4b93-8e74-abb132a4d060"
        }
    ],
    "_rid": "VO9GAMfnPeEXAAAAAAAAAA==",
    "_self": "dbs/VO9GAA==/colls/VO9GAMfnPeE=/docs/VO9GAMfnPeEXAAAAAAAAAA==/",
    "_etag": "\"00000000-0000-0000-40d3-b2cbeb6001d7\"",
    "_attachments": "attachments/",
    "_ts": 1620125601
}
```

## Ideal scenario

The ideal scenario here would for no exceptions to be thrown and the database to look like the following (ignoring the system generated properties):

```json
[
    {
        "Id": "217e5ac0-6a79-4dba-812d-7a1097824790",
        "CustomerName": "Bob",
        "Discriminator": "Order",
        "id": "Order|217e5ac0-6a79-4dba-812d-7a1097824790",
        "Items": [
            {
                "ItemName": "Foo",
                "ItemPrice": 10
            },
            {
                "ItemName": "Bar",
                "ItemPrice": 20
            }
        ],
        "_rid": "VO9GAMfnPeEGAAAAAAAAAA==",
        "_self": "dbs/VO9GAA==/colls/VO9GAMfnPeE=/docs/VO9GAMfnPeEGAAAAAAAAAA==/",
        "_etag": "\"00000000-0000-0000-40c4-96a0408201d7\"",
        "_attachments": "attachments/",
        "_ts": 1620119111
    },
    {
        "Id": "9f246d17-6510-4ccd-90d4-fab38a89701c",
        "CustomerName": "Jane",
        "Discriminator": "Order",
        "id": "Order|9f246d17-6510-4ccd-90d4-fab38a89701c",
        "Items": [
            {
                "ItemName": "Foobar",
                "ItemPrice": 30
            }
        ],
        "_rid": "VO9GAMfnPeEGAAAAAAAAAA==",
        "_self": "dbs/VO9GAA==/colls/VO9GAMfnPeE=/docs/VO9GAMfnPeEGAAAAAAAAAA==/",
        "_etag": "\"00000000-0000-0000-40c4-96a0408201d7\"",
        "_attachments": "attachments/",
        "_ts": 1620119111
    },
]
```
