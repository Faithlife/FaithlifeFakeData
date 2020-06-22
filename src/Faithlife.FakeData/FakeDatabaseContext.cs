using System;
using System.Threading;
using System.Threading.Tasks;

namespace Faithlife.FakeData
{
	/// <summary>
	/// Base class for a fake database context.
	/// </summary>
	/// <remarks>The derived class should have one or more read-only properties of type
	/// <see cref="FakeDatabaseTable{T}"/>.</remarks>
	public abstract class FakeDatabaseContext : IDisposable
	{
		/// <summary>
		/// Unlocks the database.
		/// </summary>
		public void Dispose()
		{
			m_semaphore.Release();
			m_semaphore.Dispose();
		}

		/// <summary>
		/// Creates an instance.
		/// </summary>
		protected FakeDatabaseContext()
		{
			m_semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
		}

		/// <summary>
		/// Creates a table.
		/// </summary>
		protected FakeDatabaseTable<T> CreateTable<T>() => new FakeDatabaseTable<T>(this);

		internal bool IsLocked => m_semaphore.CurrentCount == 0;

		internal void Lock() => m_semaphore.Wait();

		internal Task LockAsync(CancellationToken cancellationToken) => m_semaphore.WaitAsync(cancellationToken);

		internal FakeDatabaseContext ShallowClone() => (FakeDatabaseContext) MemberwiseClone();

		private readonly SemaphoreSlim m_semaphore;
	}
}
