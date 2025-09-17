using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvGames.Application.Exceptions;

[ExcludeFromCodeCoverage]
public class ModifyDatabaseException : HttpException
{
    public ModifyDatabaseException(string message)
    : base(500, "Internal Server Error", message) { }
}
