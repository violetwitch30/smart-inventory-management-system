using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Assignment1.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    CustomerEmail = table.Column<string>(type: "text", nullable: false),
                    ProductIds = table.Column<int[]>(type: "integer[]", nullable: false),
                    Quantities = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<float>(type: "real", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "Name" },
                values: new object[,]
                {
                    { 1, "Electronics" },
                    { 2, "Home" },
                    { 3, "Clothing" },
                    { 4, "Food" },
                    { 5, "Books" }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderId", "CustomerEmail", "CustomerName", "OrderDate", "ProductIds", "Quantities" },
                values: new object[,]
                {
                    { 1, "john@gmail.com", "John Doe", new DateTime(2025, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new[] { 1, 4, 5 }, new[] { 2, 1, 1 } },
                    { 2, "jane@gmail.com", "Jane Smith", new DateTime(2025, 3, 30, 0, 0, 0, 0, DateTimeKind.Utc), new[] { 2, 10 }, new[] { 1, 1 } }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "CategoryId", "Description", "LowStockThreshold", "Name", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, 5, "Harry Potter books for fantasy lovers", 10, "Harry Potter", 50f, 100 },
                    { 2, 5, "Hunger Games for dystopia lovers", 10, "Hunger Games", 40f, 50 },
                    { 3, 5, "Classic fantasy novel", 10, "The Lord of the Rings", 60f, 30 },
                    { 4, 1, "New fast laptop for work and gaming", 10, "Laptop", 1500f, 30 },
                    { 5, 1, "Latest smartphone with great camera", 10, "Smartphone", 999f, 20 },
                    { 6, 1, "Noise-canceling wireless earbuds", 10, "Wireless Earbuds", 150f, 50 },
                    { 7, 1, "Feature-packed smartwatch", 10, "Smartwatch", 250f, 3 },
                    { 8, 2, "Powerful vacuum cleaner for home use", 10, "Vacuum Cleaner", 200f, 15 },
                    { 9, 2, "Automatic coffee maker", 10, "Coffee Maker", 80f, 40 },
                    { 10, 3, "Beautiful summer dress", 10, "Summer Dress", 25f, 5 },
                    { 11, 3, "Stylish leather jacket", 10, "Leather Jacket", 150f, 10 },
                    { 12, 3, "Comfortable running shoes", 10, "Running Shoes", 120f, 20 },
                    { 13, 3, "High-quality denim jeans", 10, "Jeans", 60f, 30 },
                    { 14, 4, "Delicious milk chocolate", 10, "Chocolate Bar", 2.5f, 100 },
                    { 15, 4, "Pure organic honey", 10, "Organic Honey", 15f, 9 },
                    { 16, 4, "Italian pasta", 10, "Pasta Pack", 5f, 75 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
