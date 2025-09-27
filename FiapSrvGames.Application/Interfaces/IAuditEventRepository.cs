using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiapSrvGames.Domain.Entities;

namespace FiapSrvGames.Application.Interfaces;

public interface IAuditEventRepository
{
    Task CreateAsync(AuditEvent auditEvent);
}
