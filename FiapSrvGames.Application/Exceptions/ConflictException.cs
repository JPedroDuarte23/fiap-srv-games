using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvGames.Application.Exceptions;

[ExcludeFromCodeCoverage]
public class ConflictException : HttpException
{
    public ConflictException(string message)
     : base(409, "Conflict", message) { }
}
