namespace GamiPort.Models.ViewModels
{
	public class TopbarVM
	{
		public int UserId { get; set; }
		public string? NickName { get; set; }
		public bool IsAuthenticated => UserId > 0;
	}
}
