// <copyright file="20260814080500_AddSecureCashShop.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

#nullable disable

namespace MUnique.OpenMU.Persistence.EntityFramework.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

    /// <inheritdoc />
    public partial class AddSecureCashShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CashShopGoblinPoints",
                schema: "data",
                table: "Account",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "CashShopLastOperationId",
                schema: "data",
                table: "Account",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CashShopLastRequestAt",
                schema: "data",
                table: "Account",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CashShopLastRequestFingerprint",
                schema: "data",
                table: "Account",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<long>(
                name: "CashShopRevision",
                schema: "data",
                table: "Account",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CashShopWCoinC",
                schema: "data",
                table: "Account",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CashShopWCoinP",
                schema: "data",
                table: "Account",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "CashShopLedgerEntry",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    Delta = table.Column<long>(type: "bigint", nullable: false),
                    BalanceAfter = table.Column<long>(type: "bigint", nullable: false),
                    StorageIndex = table.Column<long>(type: "bigint", nullable: true),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Actor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashShopLedgerEntry", x => x.Id);
                    table.CheckConstraint("CK_CashShopLedgerEntry_BalanceAfter", "\"BalanceAfter\" >= 0");
                    table.CheckConstraint("CK_CashShopLedgerEntry_Enums", "\"Operation\" BETWEEN 0 AND 4 AND \"Currency\" BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_CashShopLedgerEntry_Account_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "data",
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashShopStorageItem",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageIndex = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    ProductSequence = table.Column<long>(type: "bigint", nullable: false),
                    PriceSequence = table.Column<long>(type: "bigint", nullable: false),
                    ItemCode = table.Column<int>(type: "integer", nullable: false),
                    StorageGroupCode = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    GiftSender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    GiftMessage = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashShopStorageItem", x => x.Id);
                    table.CheckConstraint("CK_CashShopStorageItem_Enums", "\"Kind\" BETWEEN 0 AND 1 AND \"State\" BETWEEN 0 AND 2 AND \"Currency\" BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_CashShopStorageItem_Price", "\"Price\" >= 0");
                    table.CheckConstraint("CK_CashShopStorageItem_StorageIndex", "\"StorageIndex\" > 0 AND \"StorageIndex\" <= 4294967295");
                    table.ForeignKey(
                        name: "FK_CashShopStorageItem_Account_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "data",
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Account_CashShopBalances",
                schema: "data",
                table: "Account",
                sql: "\"CashShopWCoinC\" >= 0 AND \"CashShopWCoinP\" >= 0 AND \"CashShopGoblinPoints\" >= 0 AND \"CashShopRevision\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashShopLedgerEntry_AccountId_CreatedAt",
                schema: "data",
                table: "CashShopLedgerEntry",
                columns: new[] { "AccountId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CashShopLedgerEntry_OperationId",
                schema: "data",
                table: "CashShopLedgerEntry",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashShopStorageItem_AccountId_Kind_State_CreatedAt",
                schema: "data",
                table: "CashShopStorageItem",
                columns: new[] { "AccountId", "Kind", "State", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CashShopStorageItem_StorageIndex",
                schema: "data",
                table: "CashShopStorageItem",
                column: "StorageIndex",
                unique: true);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION data."RejectCashShopLedgerMutation"()
                RETURNS trigger
                LANGUAGE plpgsql
                AS 'BEGIN RAISE EXCEPTION ''Cash shop ledger is append-only''; END;';

                CREATE TRIGGER "TR_CashShopLedgerEntry_AppendOnly"
                BEFORE UPDATE OR DELETE ON data."CashShopLedgerEntry"
                FOR EACH ROW EXECUTE FUNCTION data."RejectCashShopLedgerMutation"();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashShopLedgerEntry",
                schema: "data");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS data.\"RejectCashShopLedgerMutation\"();");

            migrationBuilder.DropTable(
                name: "CashShopStorageItem",
                schema: "data");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Account_CashShopBalances",
                schema: "data",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "CashShopGoblinPoints",
                schema: "data",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "CashShopLastOperationId",
                schema: "data",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "CashShopLastRequestAt",
                schema: "data",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "CashShopLastRequestFingerprint",
                schema: "data",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "CashShopRevision",
                schema: "data",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "CashShopWCoinC",
                schema: "data",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "CashShopWCoinP",
                schema: "data",
                table: "Account");
        }
    }
}
