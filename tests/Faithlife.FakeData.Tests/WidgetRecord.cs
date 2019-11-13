using System.ComponentModel.DataAnnotations;

namespace Faithlife.FakeData.Tests
{
	public sealed class WidgetRecord
	{
		[Key]
		public long WidgetId { get; set; }

		[Required, StringLength(100)]
		public string Name { get; set; } = "";

		[RegularExpression(@"^[1-2][0-9]{3}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z$")]
		public string? Created { get; set; }
	}
}
