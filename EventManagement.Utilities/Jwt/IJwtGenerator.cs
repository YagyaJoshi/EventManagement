namespace EventManagement.Utilities.Jwt
{
    public interface IJwtGenerator
    {
        string GenerateJwtToken(long userId, string roles);
    }
}
