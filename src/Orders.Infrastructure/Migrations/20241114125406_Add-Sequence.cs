using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "OrderNumberSequence");

            migrationBuilder.CreateTable(
                name: "SequenceNumber",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NextSequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SequenceNumber", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SequenceNumber",
                columns: new[] { "Id", "NextSequenceNumber" },
                values: new object[] { (short)1, 0L });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SequenceNumber");

            migrationBuilder.CreateSequence(
                name: "OrderNumberSequence",
                startValue: 0L);
        }
    }
}
