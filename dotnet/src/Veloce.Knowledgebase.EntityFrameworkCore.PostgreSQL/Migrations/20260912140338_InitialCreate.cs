using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloce.Knowledgebase.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    SpeedRecord = table.Column<decimal>(type: "numeric", nullable: true),
                    LeanRecordLeft = table.Column<decimal>(type: "numeric", nullable: true),
                    LeanRecordRight = table.Column<decimal>(type: "numeric", nullable: true),
                    GForceRecord = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    ImageData = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehicleId);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rides",
                columns: table => new
                {
                    RideId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<decimal>(type: "numeric", nullable: false),
                    Distance = table.Column<decimal>(type: "numeric", nullable: false),
                    HighestSpeed = table.Column<decimal>(type: "numeric", nullable: false),
                    CoordinateHighestSpeed = table.Column<string>(type: "text", nullable: false),
                    AverageSpeed = table.Column<decimal>(type: "numeric", nullable: false),
                    HighestLeanAngleLeft = table.Column<decimal>(type: "numeric", nullable: false),
                    CoordinateHighestLeanAngleLeft = table.Column<string>(type: "text", nullable: false),
                    HighestLeanAngleRight = table.Column<decimal>(type: "numeric", nullable: false),
                    CoordinateHighestLeanAngleRight = table.Column<string>(type: "text", nullable: false),
                    HighestGForce = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rides", x => x.RideId);
                    table.ForeignKey(
                        name: "FK_Rides_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rides_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RidePoints",
                columns: table => new
                {
                    RidePointId = table.Column<Guid>(type: "uuid", nullable: false),
                    RideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Coordinate = table.Column<string>(type: "text", nullable: false),
                    Speed = table.Column<decimal>(type: "numeric", nullable: false),
                    GForce = table.Column<decimal>(type: "numeric", nullable: false),
                    Lean = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RidePoints", x => x.RidePointId);
                    table.ForeignKey(
                        name: "FK_RidePoints_Rides_RideId",
                        column: x => x.RideId,
                        principalTable: "Rides",
                        principalColumn: "RideId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RidePoints_RideId",
                table: "RidePoints",
                column: "RideId");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_UserId",
                table: "Rides",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_VehicleId",
                table: "Rides",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RidePoints");

            migrationBuilder.DropTable(
                name: "Rides");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
