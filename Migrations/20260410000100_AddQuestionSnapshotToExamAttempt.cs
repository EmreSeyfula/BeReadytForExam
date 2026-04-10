using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeReadyForExam.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionSnapshotToExamAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestionSnapshot",
                table: "ExamAttempts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionSnapshot",
                table: "ExamAttempts");
        }
    }
}
