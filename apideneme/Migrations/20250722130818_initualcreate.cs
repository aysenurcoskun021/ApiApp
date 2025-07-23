using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace apideneme.Migrations
{
    /// <inheritdoc />
    public partial class initualcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Info",
                columns: table => new
                {
                    InfoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    seed = table.Column<string>(type: "text", nullable: false),
                    results = table.Column<int>(type: "integer", nullable: false),
                    page = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info", x => x.InfoId);
                });

            migrationBuilder.CreateTable(
                name: "location",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Street_number = table.Column<int>(type: "integer", nullable: false),
                    Street_name = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: false),
                    postcode = table.Column<string>(type: "text", nullable: false),
                    Coordinates_latitude = table.Column<string>(type: "text", nullable: false),
                    Coordinates_longitude = table.Column<string>(type: "text", nullable: false),
                    Timezone_offset = table.Column<string>(type: "text", nullable: false),
                    Timezone_description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location", x => x.LocationId);
                });

            migrationBuilder.CreateTable(
                name: "login",
                columns: table => new
                {
                    LoginId = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    salt = table.Column<string>(type: "text", nullable: false),
                    md5 = table.Column<string>(type: "text", nullable: false),
                    sha1 = table.Column<string>(type: "text", nullable: false),
                    sha256 = table.Column<string>(type: "text", nullable: false),
                    uuid = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login", x => x.LoginId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    nat = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    cell = table.Column<string>(type: "text", nullable: false),
                    Registered_date = table.Column<string>(type: "text", nullable: false),
                    Registered_age = table.Column<string>(type: "text", nullable: false),
                    Dob_date = table.Column<string>(type: "text", nullable: false),
                    Dob_age = table.Column<string>(type: "text", nullable: false),
                    Id_name = table.Column<string>(type: "text", nullable: false),
                    Id_value = table.Column<string>(type: "text", nullable: true),
                    Name_title = table.Column<string>(type: "text", nullable: false),
                    Name_first = table.Column<string>(type: "text", nullable: false),
                    Name_last = table.Column<string>(type: "text", nullable: false),
                    Picture_large = table.Column<string>(type: "text", nullable: false),
                    Picture_medium = table.Column<string>(type: "text", nullable: false),
                    Picture_thumbnail = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_User_location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "location",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_login_LoginId",
                        column: x => x.LoginId,
                        principalTable: "login",
                        principalColumn: "LoginId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_LocationId",
                table: "User",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_User_LoginId",
                table: "User",
                column: "LoginId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Info");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "location");

            migrationBuilder.DropTable(
                name: "login");
        }
    }
}
