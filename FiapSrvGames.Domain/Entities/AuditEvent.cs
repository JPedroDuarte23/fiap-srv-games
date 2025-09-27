using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvGames.Domain.Entities;

public class AuditEvent
{
    public Guid Id { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Guid EntityId { get; set; }
    public string EventType { get; set; }
    public object EventData { get; set; }
    public DateTime Timestamp { get; set; }
}
