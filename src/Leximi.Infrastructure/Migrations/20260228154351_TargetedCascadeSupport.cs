using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leximi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TargetedCascadeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_LearningItems_LearningItemId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningItems_LearningSets_LearningSetId",
                table: "LearningItems");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_LearningItems_LearningItemId",
                table: "Answers",
                column: "LearningItemId",
                principalTable: "LearningItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningItems_LearningSets_LearningSetId",
                table: "LearningItems",
                column: "LearningSetId",
                principalTable: "LearningSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_LearningItems_LearningItemId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningItems_LearningSets_LearningSetId",
                table: "LearningItems");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_LearningItems_LearningItemId",
                table: "Answers",
                column: "LearningItemId",
                principalTable: "LearningItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningItems_LearningSets_LearningSetId",
                table: "LearningItems",
                column: "LearningSetId",
                principalTable: "LearningSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
