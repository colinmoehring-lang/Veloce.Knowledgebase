using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloce.Knowledgebase.Migrations
{
    /// <inheritdoc />
    public partial class ProductionHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SecurityStamp",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Rides",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "PasswordResetCodes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "EmailVerificationCodes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

            migrationBuilder.CreateTable(
                name: "AuthRateLimitBuckets",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    WindowEndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthRateLimitBuckets", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthRateLimitBuckets_WindowEndsAtUtc",
                table: "AuthRateLimitBuckets",
                column: "WindowEndsAtUtc");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerificationCodes_Users_UserId",
                table: "EmailVerificationCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetCodes_Users_UserId",
                table: "PasswordResetCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("""
                ALTER TABLE "Vehicles" ADD CONSTRAINT "AK_Vehicles_VehicleId_UserId" UNIQUE ("VehicleId", "UserId");
                ALTER TABLE "Rides" ADD CONSTRAINT "FK_Rides_VehicleOwnership"
                    FOREIGN KEY ("VehicleId", "UserId") REFERENCES "Vehicles" ("VehicleId", "UserId");
                CREATE FUNCTION "RotateUserSecurityStampOnStatusChange"() RETURNS trigger AS $$
                BEGIN
                    NEW."SecurityStamp" := gen_random_uuid();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER "TR_Users_RotateSecurityStampOnStatusChange"
                    BEFORE UPDATE OF "Status" ON "Users"
                    FOR EACH ROW WHEN (OLD."Status" IS DISTINCT FROM NEW."Status")
                    EXECUTE FUNCTION "RotateUserSecurityStampOnStatusChange"();
                ALTER TABLE "Users" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "Vehicles" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "Rides" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "RidePoints" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "EmailVerificationCodes" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "PasswordResetCodes" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "AuthRateLimitBuckets" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "__EFMigrationsHistory" ENABLE ROW LEVEL SECURITY;
                REVOKE ALL PRIVILEGES ON TABLE "Users", "Vehicles", "Rides", "RidePoints", "EmailVerificationCodes", "PasswordResetCodes", "AuthRateLimitBuckets", "__EFMigrationsHistory" FROM PUBLIC;
                DO $$
                DECLARE client_role text;
                BEGIN
                    FOR client_role IN SELECT rolname FROM pg_roles WHERE rolname IN ('anon', 'authenticated') LOOP
                        EXECUTE format(
                            'REVOKE ALL PRIVILEGES ON TABLE "Users", "Vehicles", "Rides", "RidePoints", "EmailVerificationCodes", "PasswordResetCodes", "AuthRateLimitBuckets", "__EFMigrationsHistory" FROM %I',
                            client_role);
                    END LOOP;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Rides" DROP CONSTRAINT "FK_Rides_VehicleOwnership";
                ALTER TABLE "Vehicles" DROP CONSTRAINT "AK_Vehicles_VehicleId_UserId";
                DROP TRIGGER "TR_Users_RotateSecurityStampOnStatusChange" ON "Users";
                DROP FUNCTION "RotateUserSecurityStampOnStatusChange"();
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_EmailVerificationCodes_Users_UserId",
                table: "EmailVerificationCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PasswordResetCodes_Users_UserId",
                table: "PasswordResetCodes");

            migrationBuilder.DropTable(
                name: "AuthRateLimitBuckets");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rides");

        }
    }
}
