namespace Faithlife.FakeData.Tests
{
	public sealed class WidgetDatabaseContext : FakeDatabaseContext
	{
		public FakeDatabaseTable<WidgetRecord> Widgets { get; } = null;
	}
}
