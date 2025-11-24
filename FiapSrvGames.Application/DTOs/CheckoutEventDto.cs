namespace FiapSrvGames.Application.DTOs;

public class CheckoutEventDto
{
    public Guid UserId { get; set; }
    public List<Guid> GameIds { get; set; }
}