namespace Core.Services.TokenServiceWrapper;

public class TokenService : ITokenService {
    private TimeSpan ExpiryDuration = new TimeSpan(0, 30, 0);
    public string BuildToken(string key, string issuer, User user) {
        var claims = new[] {
            new Claim(ClaimTypes.PrimarySid, user.id),
            new Claim(ClaimTypes.Email, user.email),
            new Claim(ClaimTypes.AuthenticationMethod, (user.isAdmin == true ? "ADMIN" : "NOT_ADMIN")),
            new Claim(ClaimTypes.Hash, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
            expires: DateTime.Now.Add(ExpiryDuration), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
    public static void buildTokenOptions(JwtBearerOptions opt, WebApplicationBuilder builder) {
        opt.TokenValidationParameters = new () {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    }

    public Tuple<String, String, bool> destructureToken(HttpContext http) => Tuple.Create(
        http.User.FindFirstValue(ClaimTypes.PrimarySid),
        http.User.FindFirstValue(ClaimTypes.Email),
        http.User.FindFirstValue(ClaimTypes.AuthenticationMethod) == "ADMIN"
    );
}