using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanoriaCapstone.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizShowResultsAndTimePerQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowResults",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "TimePerQuestion",
                table: "Quizzes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowResults",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "TimePerQuestion",
                table: "Quizzes");
        }
    }
}
