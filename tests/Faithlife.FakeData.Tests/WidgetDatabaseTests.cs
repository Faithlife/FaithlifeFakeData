using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Faithlife.FakeData.Tests
{
	[TestFixture]
	public class WidgetDatabaseTests
	{
		[Test]
		public void CrudTests()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			using (var context = database.Lock())
			{
				var records = context.Widgets.AddRange(new[]
				{
					new WidgetRecord { Name = "Alice" },
					new WidgetRecord { Name = "Bob" },
				});

				var aliceId = records[0].WidgetId;
				var bobId = records[1].WidgetId;

				context.Widgets.Single(x => x.WidgetId == aliceId).Name.Should().Be("Alice");
				context.Widgets.Single(x => x.WidgetId == bobId).Name.Should().Be("Bob");

				context.Widgets.UpdateWhere(x => x.WidgetId == bobId, x => x.Name = "Robert").Should().Be(1);
				context.Widgets.RemoveWhere(x => x.WidgetId == aliceId).Should().Be(1);

				context.Widgets.Any(x => x.WidgetId == aliceId).Should().BeFalse();
				context.Widgets.Single(x => x.WidgetId == bobId).Name.Should().Be("Robert");
			}
		}

		[Test]
		public async Task LockAsync()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			using (var context = await database.LockAsync(CancellationToken.None).ConfigureAwait(false))
				context.Widgets.Add(new WidgetRecord { Name = "Alice" });
		}

		[Test]
		public void NullRecord()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			using (var context = database.Lock())
			{
				Invoking(() => context.Widgets.Add(null!)).Should().Throw<ArgumentNullException>();
				Invoking(() => context.Widgets.AddRange(null!)).Should().Throw<ArgumentNullException>();
				Invoking(() => context.Widgets.AddRange(new WidgetRecord[] { null! })).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		public void NotLocked()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			FakeDatabaseTable<WidgetRecord> widgets;
			using (var context = database.Lock())
				widgets = context.Widgets;
			Invoking(() => widgets.Any().Should().BeFalse()).Should().Throw<InvalidOperationException>();
			Invoking(() => widgets.Add(new WidgetRecord { Name = "Alice" })).Should().Throw<InvalidOperationException>();
			Invoking(() => widgets.UpdateWhere(x => true, x => { })).Should().Throw<InvalidOperationException>();
			Invoking(() => widgets.RemoveWhere(x => true)).Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void SetNextAutoId()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			using (var context = database.Lock())
			{
				context.Widgets.SetNextAutoId(123);
				context.Widgets.GetNextAutoId().Should().Be(123);
				context.Widgets.Add(new WidgetRecord { Name = "Alice" }).WidgetId.Should().Be(123);
				context.Widgets.GetNextAutoId().Should().Be(124);
			}
		}

		[Test]
		public void NameRequired()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			using (var context = database.Lock())
				Invoking(() => context.Widgets.Add(new WidgetRecord())).Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void NameTooLong()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			using (var context = database.Lock())
				Invoking(() => context.Widgets.Add(new WidgetRecord { Name = new string('X', 101) })).Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ValidCreatedDate()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			using (var context = database.Lock())
				context.Widgets.Add(new WidgetRecord { Name = "Alice", Created = "2000-01-02T03:04:05Z" });
		}

		[Test]
		public void InvalidCreatedDate()
		{
			var database = FakeDatabase.Create<WidgetDatabaseContext>();
			using (var context = database.Lock())
				Invoking(() => context.Widgets.Add(new WidgetRecord { Name = "Alice", Created = "2000-01-02" })).Should().Throw<InvalidOperationException>();
		}

		private static Action Invoking(Action action) => action;
	}
}
