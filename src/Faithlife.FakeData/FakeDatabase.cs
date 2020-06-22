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
}
