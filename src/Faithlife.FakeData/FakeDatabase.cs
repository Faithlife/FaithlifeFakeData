using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Faithlife.FakeData
{
	/// <summary>
	/// Creates fake databases.
	/// </summary>
	public static class FakeDatabase
	{
		/// <summary>
		/// Creates a fake database using the specified context.
		/// </summary>
		public static FakeDatabase<T> Create<T>()
			where T : FakeDatabaseContext, new()
		{
			return new FakeDatabase<T>(new T());
		}
	}

	/// <summary>
	/// A fake database.
	/// </summary>
	[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Generic type.")]
	public sealed class FakeDatabase<TContext>
		where TContext : FakeDatabaseContext
	{
		/// <summary>
		/// Locks the database.
		/// </summary>
		/// <remarks>Database tables and records should only be accessed while the database is locked.
		/// Dispose the returned context to unlock the database.</remarks>
		public TContext Lock()
		{
			var context = (TContext) m_context.ShallowClone();
			context.Lock();
			return context;
		}

		/// <summary>
		/// Locks the database.
		/// </summary>
		/// <remarks>Database tables and records should only be accessed while the database is locked.
		/// Dispose the returned context to unlock the database.</remarks>
		public async Task<TContext> LockAsync(CancellationToken cancellationToken)
		{
			var context = (TContext) m_context.ShallowClone();
			await context.LockAsync(cancellationToken).ConfigureAwait(false);
			return context;
		}

		internal FakeDatabase(TContext context)
		{
			m_context = context;
		}

		private readonly TContext m_context;
	}
}
