using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvGames.Application.Exceptions;

[ExcludeFromCodeCoverage]
public class UnauthorizedException : HttpException
{
    public UnauthorizedException(string message)
        : base(401, "Unauthorized", message) { }
}