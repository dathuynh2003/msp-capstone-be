using MSP.Domain.Entities;

namespace MSP.Application.Abstracts
{
    public interface IAuthTokenProcessor
    {
        (string jwtToken, DateTime expriesAtUtc) GenerateJwtToken(User user, string[] roles);

        string GenerateRefreshToken();
        void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration);
        void ClearAuthTokenCookies();

    }
}
