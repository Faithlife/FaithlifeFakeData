# FakeDatabase&lt;TContext&gt;.LockAsync method

Locks the database.

```csharp
public Task<TContext> LockAsync(CancellationToken cancellationToken)
```

## Remarks

Database tables and records should only be accessed while the database is locked. Dispose the returned context to unlock the database.

## See Also

* class [FakeDatabase&lt;TContext&gt;](../FakeDatabase-1.md)
* namespace [Faithlife.FakeData](../../Faithlife.FakeData.md)

<!-- DO NOT EDIT: generated by xmldocmd for Faithlife.FakeData.dll -->
