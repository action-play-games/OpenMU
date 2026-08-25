// <copyright file="FixSeasonSixNpcWindowsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Configures the missing Season 6 interaction windows for Mirage, Lugard and David.
/// </summary>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("E0BD2190-E1B1-4201-9A12-77CC5C48E183")]
public class FixSeasonSixNpcWindowsUpdatePlugIn : UpdatePlugInBase
{
    /// <summary>
    /// The plug-in name.
    /// </summary>
    internal const string PlugInName = "Fix Season 6 NPC windows";

    /// <summary>
    /// The plug-in description.
    /// </summary>
    internal const string PlugInDescription = "Configures the interaction windows for Mirage, Lugard and David.";

    private const short MirageNumber = 385;
    private const short LugardNumber = 540;
    private const short DavidNumber = 579;

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override UpdateVersion Version => UpdateVersion.FixSeasonSixNpcWindows;

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 08, 24, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        SetNpcWindow(gameConfiguration, MirageNumber, NpcWindow.IllusionTemple);
        SetNpcWindow(gameConfiguration, LugardNumber, NpcWindow.LugardDoppelgangerEntry);
        SetNpcWindow(gameConfiguration, DavidNumber, NpcWindow.CombineLuckyItem);
        return ValueTask.CompletedTask;
    }

    private static void SetNpcWindow(GameConfiguration gameConfiguration, short npcNumber, NpcWindow window)
    {
        var definition = gameConfiguration.Monsters.FirstOrDefault(monster => monster.Number == npcNumber)
            ?? throw new InvalidOperationException($"NPC definition {npcNumber} does not exist.");
        definition.NpcWindow = window;
    }
}
