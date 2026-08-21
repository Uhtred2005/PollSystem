using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoteService.Migrations
{
    /// <inheritdoc />
    public partial class InitialVoteDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PollCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OptionIndex = table.Column<int>(type: "integer", nullable: false),
                    VoterToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PollCode",
                table: "Votes",
                column: "PollCode");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PollCode_VoterToken",
                table: "Votes",
                columns: new[] { "PollCode", "VoterToken" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votes");
        }
    }
}
