using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Migrations
{
    /// <inheritdoc />
    public partial class CreateArticoliAndCarrelloTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articoli",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descrizione = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Prezzo = table.Column<double>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articoli", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carrelli",
                columns: table => new
                {
                    IdCarrello = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticoloId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantita = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carrelli", x => x.IdCarrello);
                    table.ForeignKey(
                        name: "FK_Carrelli_Articoli_ArticoloId",
                        column: x => x.ArticoloId,
                        principalTable: "Articoli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carrelli_ArticoloId",
                table: "Carrelli",
                column: "ArticoloId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Carrelli");

            migrationBuilder.DropTable(
                name: "Articoli");
        }
    }
}
