namespace Faithlife.FakeData.Tests;

public sealed class WidgetDatabaseContext : FakeDatabaseContext
{
	public WidgetDatabaseContext()
	{
		Widgets = CreateTable<WidgetRecord>();
	}

	public FakeDatabaseTable<WidgetRecord> Widgets { get; }
}
