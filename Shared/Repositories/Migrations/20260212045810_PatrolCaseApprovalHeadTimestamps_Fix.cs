using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PatrolCaseApprovalHeadTimestamps_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('patrol_case', 'approved_by_head_1_at') IS NULL
BEGIN
    ALTER TABLE [patrol_case] ADD [approved_by_head_1_at] datetime2 NULL;
END
IF COL_LENGTH('patrol_case', 'approved_by_head_2_at') IS NULL
BEGIN
    ALTER TABLE [patrol_case] ADD [approved_by_head_2_at] datetime2 NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('patrol_case', 'approved_by_head_1_at') IS NOT NULL
BEGIN
    ALTER TABLE [patrol_case] DROP COLUMN [approved_by_head_1_at];
END
IF COL_LENGTH('patrol_case', 'approved_by_head_2_at') IS NOT NULL
BEGIN
    ALTER TABLE [patrol_case] DROP COLUMN [approved_by_head_2_at];
END
");
        }
    }
}
