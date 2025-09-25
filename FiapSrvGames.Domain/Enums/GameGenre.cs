using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FiapSrvGames.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameGenre
{
    Action,
    Shooter,
    Fighting,
    Platformer,
    Adventure,
    RPG,
    MMORPG,
    Strategy,
    RTS,
    TurnBasedStrategy,
    Simulation,
    Sports,
    Racing,
    Puzzle,
    Horror,
    Survival,
    Sandbox,
    CardGame
}