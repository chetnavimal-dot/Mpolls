using System.Collections.Generic;

namespace MPolls.API.Models.Auth;

public record AuthTokenVerificationResponse(string Uid, Dictionary<string, object?> Claims);
