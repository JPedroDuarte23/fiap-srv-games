using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvGames.Application.Exceptions;

[ExcludeFromCodeCoverage]
public class NotFoundException : HttpException
{
    public NotFoundException(string message)
        : base(404, "Not Found", message) { }
}