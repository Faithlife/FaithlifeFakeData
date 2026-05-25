#if !NET
namespace Faithlife.FakeData;

internal static class ArgumentNullExceptionExtensions
{
	extension(ArgumentNullException)
	{
		public static void ThrowIfNull(object? argument, string? paramName)
		{
			if (argument is null)
				throw new ArgumentNullException(paramName);
		}
	}
}
#endif
