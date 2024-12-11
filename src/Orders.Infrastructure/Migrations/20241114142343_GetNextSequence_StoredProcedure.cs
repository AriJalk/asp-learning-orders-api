using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class GetNextSequence_StoredProcedure : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			string sql =
				@" CREATE PROCEDURE [dbo].[GetNextSequenceNumber]
            AS
            BEGIN
                SET NOCOUNT ON;

                BEGIN TRY
                    -- Lock the row and update the sequence number atomically
                    UPDATE SequenceNumber WITH (UPDLOCK)
                    SET NextSequenceNumber = NextSequenceNumber + 1
                    OUTPUT INSERTED.NextSequenceNumber
                    WHERE Id = 1;
                END TRY
                BEGIN CATCH
                    -- Error handling: Rethrow the error to propagate it
                    THROW;  
                END CATCH
            END";
			migrationBuilder.Sql(sql);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			string sql = @"DROP PROCEDURE [dbo].[GetNextSequenceNumber]";

			migrationBuilder.Sql(sql);
		}
	}
}
