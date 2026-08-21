// <copyright file="CashShopGroupHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.CashShop;

using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Packet handler for the Season 6 cash shop group (0xD2).
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Group Handler", Description = "Routes Season 6 C1 D2 cash shop requests.")]
[Guid("3761573D-679B-4A77-937F-80303EA7D860")]
internal sealed class CashShopGroupHandlerPlugIn : GroupPacketHandlerPlugIn
{
    /// <summary>
    /// The cash shop packet group key.
    /// </summary>
    internal const byte GroupKey = 0xD2;

    /// <summary>
    /// Initializes a new instance of the <see cref="CashShopGroupHandlerPlugIn"/> class.
    /// </summary>
    /// <param name="clientVersionProvider">The client version provider.</param>
    /// <param name="manager">The plug-in manager.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public CashShopGroupHandlerPlugIn(IClientVersionProvider clientVersionProvider, PlugInManager manager, ILoggerFactory loggerFactory)
        : base(clientVersionProvider, manager, loggerFactory)
    {
    }

    /// <inheritdoc/>
    public override byte Key => GroupKey;

    /// <inheritdoc/>
    public override bool IsEncryptionExpected => false;
}
