// <copyright file="EntityDataContext.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.EntityFramework;

using Microsoft.EntityFrameworkCore;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.Persistence.EntityFramework.Extensions;
using MUnique.OpenMU.Persistence.EntityFramework.Extensions.ModelBuilder;
using MUnique.OpenMU.Persistence.EntityFramework.Model;

/// <summary>
/// Context for all types of the data model.
/// </summary>
public class EntityDataContext : ExtendedTypeContext
{
    /// <summary>
    /// Gets or sets the current game configuration.
    /// This is used by the <see cref="ConfigurationTypeRepository{T}"/> which gets its data from the current game configuration.
    /// </summary>
    internal GameConfiguration? CurrentGameConfiguration { get; set; }

    /// <summary>
    /// Gets the persistent Castle Siege state.
    /// </summary>
    internal DbSet<CastleSiegeData> CastleSiegeData => this.Set<CastleSiegeData>();

    /// <summary>
    /// Gets the Castle Siege guild registrations.
    /// </summary>
    internal DbSet<CastleSiegeGuildRegistration> CastleSiegeGuildRegistrations => this.Set<CastleSiegeGuildRegistration>();

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!ConnectionConfigurator.IsInitialized)
        {
            ConnectionConfigurator.Initialize(new ConfigFileDatabaseConnectionStringProvider());
        }

        base.OnConfiguring(optionsBuilder);
        this.Configure(optionsBuilder);
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppearanceData>(o => o.Ignore(p => p.CharacterStatus)); // todo
        modelBuilder.Ignore<ConstantElement>();
        modelBuilder.Ignore<SimpleElement>();
        modelBuilder.Entity<Model.AttributeDefinition>();
        modelBuilder.Entity<ConnectServerDefinition>();
        modelBuilder.Entity<ChatServerDefinition>();
        modelBuilder.Entity<MiniGameRankingEntry>().Apply();
        modelBuilder.Entity<GameServerDefinition>(entity =>
        {
            entity.Property(e => e.PvpEnabled).HasDefaultValue(true);
        });
        modelBuilder.Entity<ConfigurationUpdate>().Apply();
        modelBuilder.Entity<ConfigurationUpdateState>();
        modelBuilder.Entity<SystemConfiguration>();

        modelBuilder.Entity<PowerUpDefinitionValue>().Apply();
        modelBuilder.Entity<Model.ConstValueAttribute>().Apply();
        modelBuilder.Entity<Account>().Apply();
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.CashShopWCoinC).IsConcurrencyToken();
            entity.Property(e => e.CashShopWCoinP).IsConcurrencyToken();
            entity.Property(e => e.CashShopGoblinPoints).IsConcurrencyToken();
            entity.Property(e => e.CashShopRevision).IsConcurrencyToken();
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_Account_CashShopBalances",
                "\"CashShopWCoinC\" >= 0 AND \"CashShopWCoinP\" >= 0 AND \"CashShopGoblinPoints\" >= 0 AND \"CashShopRevision\" >= 0"));
        });
        modelBuilder.Entity<CashShopStorageItem>(entity =>
        {
            entity.Property(e => e.StorageIndex).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.StorageIndex).IsUnique();
            entity.HasIndex(e => new { e.AccountId, e.Kind, e.State, e.CreatedAt });
            entity.HasOne(e => e.RawAccount).WithMany().HasForeignKey(e => e.AccountId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_CashShopStorageItem_StorageIndex", "\"StorageIndex\" > 0 AND \"StorageIndex\" <= 4294967295");
                table.HasCheckConstraint("CK_CashShopStorageItem_Price", "\"Price\" >= 0");
                table.HasCheckConstraint("CK_CashShopStorageItem_Enums", "\"Kind\" BETWEEN 0 AND 1 AND \"State\" BETWEEN 0 AND 2 AND \"Currency\" BETWEEN 0 AND 2");
            });
        });
        modelBuilder.Entity<CashShopLedgerEntry>(entity =>
        {
            entity.HasIndex(e => e.OperationId).IsUnique();
            entity.HasIndex(e => new { e.AccountId, e.CreatedAt });
            entity.HasOne(e => e.RawAccount).WithMany().HasForeignKey(e => e.AccountId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_CashShopLedgerEntry_BalanceAfter", "\"BalanceAfter\" >= 0");
                table.HasCheckConstraint("CK_CashShopLedgerEntry_Enums", "\"Operation\" BETWEEN 0 AND 4 AND \"Currency\" BETWEEN 0 AND 2");
            });
        });
        modelBuilder.Entity<Character>().Apply();
        modelBuilder.Entity<CharacterClass>().Apply();
        modelBuilder.Entity<CastleSiegeConfiguration>().Apply();
        modelBuilder.Entity<CastleSiegeData>().Apply();
        modelBuilder.Entity<CastleSiegeGuildRegistration>().Apply();
        modelBuilder.Entity<CastleSiegeNpcDefinition>().Apply();
        modelBuilder.Entity<CastleSiegeNpcState>().Apply();
        modelBuilder.Entity<DropItemGroup>().Apply();
        modelBuilder.Entity<ExitGate>().Apply();
        modelBuilder.Entity<GameConfiguration>().Apply();
        modelBuilder.Entity<GameMapDefinition>().Apply();
        modelBuilder.Entity<ItemCrafting>().Apply();
        modelBuilder.Entity<ItemDefinition>().Apply();
        modelBuilder.Entity<ItemLevelBonusTable>().Apply();
        modelBuilder.Entity<ItemDropItemGroup>().Apply();
        modelBuilder.Entity<ItemOptionCombinationBonus>().Apply();
        modelBuilder.Entity<ItemOptionDefinition>().Apply();
        modelBuilder.Entity<ItemOptionType>().Apply();
        modelBuilder.Entity<ItemSetGroup>().Apply();
        modelBuilder.Entity<ItemSlotType>().Apply();
        modelBuilder.Entity<ItemStorage>().Apply();
        modelBuilder.Entity<ItemBasePowerUpDefinition>().Apply();
        modelBuilder.Entity<LevelBonus>().Apply();
        modelBuilder.Entity<MagicEffectDefinition>().Apply();
        modelBuilder.Entity<MasterSkillRoot>().Apply();
        modelBuilder.Entity<MiniGameChangeEvent>().Apply();
        modelBuilder.Entity<MiniGameDefinition>().Apply();
        modelBuilder.Entity<MiniGameSpawnWave>().Apply();
        modelBuilder.Entity<MonsterDefinition>().Apply();
        modelBuilder.Entity<MonsterSpawnArea>().Apply();
        modelBuilder.Entity<Skill>().Apply();
        modelBuilder.Entity<SkillComboDefinition>().Apply();
        modelBuilder.Entity<SkillEntry>().Apply();
        modelBuilder.Entity<MasterSkillDefinition>().Apply();
        modelBuilder.Entity<LetterBody>().Apply();
        modelBuilder.Entity<LetterHeader>().Apply();
        modelBuilder.Entity<QuestDefinition>().Apply();
        modelBuilder.Entity<WarpInfo>().Apply();

        // join entity keys:
        this.AddJoinDefinitions(modelBuilder);

        modelBuilder.UseGuidV7Ids();

        GuildContext.ConfigureModel(modelBuilder);
        FriendContext.ConfigureModel(modelBuilder);
    }
}
