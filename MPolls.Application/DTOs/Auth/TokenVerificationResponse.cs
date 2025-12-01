using System.Collections.Generic;

namespace MPolls.Application.DTOs.Auth;

public record TokenVerificationResponse(string Uid, Dictionary<string, object?> Claims);
