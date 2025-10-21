namespace GamiPort.Infrastructure.Login
{
	public sealed class LoginIdentityResult
	{
		public bool IsAuthenticated { get; init; }
		public int? UserId { get; init; }
		public string Source { get; init; } = "";
	}

	public interface ILoginIdentity
	{
		Task<LoginIdentityResult> GetAsync(CancellationToken ct = default);
	}
}
