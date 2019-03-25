using System;
using System.Linq;
using System.Reflection;
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
		public void Dispose() => m_semaphore.Release();

		/// <summary>
		/// Creates an instance.
		/// </summary>
		protected FakeDatabaseContext()
		{
			m_semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

			// automatically create tables for backing fields
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (var fieldInfo in GetType().GetFields(bindingFlags)
				.Where(x => x.FieldType.IsGenericType && x.FieldType.GetGenericTypeDefinition() == typeof(FakeDatabaseTable<>)))
			{
				if (fieldInfo.GetValue(this) == null)
					fieldInfo.SetValue(this, s_createTableMethod.MakeGenericMethod(fieldInfo.FieldType.GenericTypeArguments[0]).Invoke(this, new object[0]));
			}
		}

		/// <summary>
		/// Creates a table.
		/// </summary>
		protected FakeDatabaseTable<T> CreateTable<T>() => new FakeDatabaseTable<T>(this);

		internal bool IsLocked => m_semaphore.CurrentCount == 0;

		internal void Lock() => m_semaphore.Wait();

		internal Task LockAsync(CancellationToken cancellationToken) => m_semaphore.WaitAsync(cancellationToken);

		internal FakeDatabaseContext ShallowClone() => (FakeDatabaseContext) MemberwiseClone();

		private static readonly MethodInfo s_createTableMethod = typeof(FakeDatabaseContext).GetMethod("CreateTable", BindingFlags.NonPublic | BindingFlags.Instance);

		private readonly SemaphoreSlim m_semaphore;
	}
}
