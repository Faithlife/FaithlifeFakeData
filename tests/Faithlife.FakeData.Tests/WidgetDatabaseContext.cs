namespace Faithlife.FakeData.Tests;

internal sealed class WidgetDatabaseContext : FakeDatabaseContext
{
	public WidgetDatabaseContext()
	{
		Widgets = CreateTable<WidgetRecord>();
	}

	public FakeDatabaseTable<WidgetRecord> Widgets { get; }
}
