using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leximi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifiedDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add new columns to LearningItems
            migrationBuilder.AddColumn<string>(
                name: "FlashcardBack",
                table: "LearningItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlashcardFront",
                table: "LearningItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionContent",
                table: "LearningItems",
                type: "nvarchar(max)",
                nullable: true);

            // 2. Drop old foreign keys explicitly using SQL to be safe
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Answers_Questions_QuestionId') ALTER TABLE Answers DROP CONSTRAINT FK_Answers_Questions_QuestionId");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserAnswers_Questions_QuestionId') ALTER TABLE UserAnswers DROP CONSTRAINT FK_UserAnswers_Questions_QuestionId");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Questions_LearningItems_LearningItemId') ALTER TABLE Questions DROP CONSTRAINT FK_Questions_LearningItems_LearningItemId");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Flashcards_LearningItems_LearningItemId') ALTER TABLE Flashcards DROP CONSTRAINT FK_Flashcards_LearningItems_LearningItemId");

            // 3. Migrate data from Questions and Flashcards to LearningItems
            migrationBuilder.Sql("UPDATE li SET li.QuestionContent = q.Content FROM LearningItems li INNER JOIN Questions q ON q.LearningItemId = li.Id");
            migrationBuilder.Sql("UPDATE li SET li.FlashcardFront = f.Front, li.FlashcardBack = f.Back FROM LearningItems li INNER JOIN Flashcards f ON f.LearningItemId = li.Id");

            // 4. Update Answers and UserAnswers values to point to LearningItems instead of Questions
            migrationBuilder.Sql("UPDATE a SET a.QuestionId = q.LearningItemId FROM Answers a INNER JOIN Questions q ON q.Id = a.QuestionId");
            migrationBuilder.Sql("UPDATE ua SET ua.QuestionId = q.LearningItemId FROM UserAnswers ua INNER JOIN Questions q ON q.Id = ua.QuestionId");

            // 5. Drop Tables
            migrationBuilder.DropTable(
                name: "Flashcards");

            migrationBuilder.DropTable(
                name: "Questions");

            // 6. Rename columns and add new foreign keys
            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "UserAnswers",
                newName: "LearningItemId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers",
                newName: "IX_UserAnswers_LearningItemId");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "Answers",
                newName: "LearningItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                newName: "IX_Answers_LearningItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_LearningItems_LearningItemId",
                table: "Answers",
                column: "LearningItemId",
                principalTable: "LearningItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_LearningItems_LearningItemId",
                table: "UserAnswers",
                column: "LearningItemId",
                principalTable: "LearningItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_LearningItems_LearningItemId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_LearningItems_LearningItemId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "FlashcardBack",
                table: "LearningItems");

            migrationBuilder.DropColumn(
                name: "FlashcardFront",
                table: "LearningItems");

            migrationBuilder.DropColumn(
                name: "QuestionContent",
                table: "LearningItems");

            migrationBuilder.RenameColumn(
                name: "LearningItemId",
                table: "UserAnswers",
                newName: "QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswers_LearningItemId",
                table: "UserAnswers",
                newName: "IX_UserAnswers_QuestionId");

            migrationBuilder.RenameColumn(
                name: "LearningItemId",
                table: "Answers",
                newName: "QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_Answers_LearningItemId",
                table: "Answers",
                newName: "IX_Answers_QuestionId");

            migrationBuilder.CreateTable(
                name: "Flashcards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Back = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Front = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flashcards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flashcards_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_LearningItemId",
                table: "Flashcards",
                column: "LearningItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_LearningItemId",
                table: "Questions",
                column: "LearningItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
