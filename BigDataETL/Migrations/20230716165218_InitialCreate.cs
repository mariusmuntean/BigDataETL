using System;
using BigDataETL.Data.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigDataETL.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:line_item_status", "li_s1,li_s2,li_s3")
                .Annotation("Npgsql:Enum:order_status", "os1,os2,os3");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtcCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<OrderStatus>(type: "order_status", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtcCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<LineItemStatus>(type: "line_item_status", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtcCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    PreviousStatus = table.Column<OrderStatus>(type: "order_status", nullable: true),
                    NewOrderStatus = table.Column<OrderStatus>(type: "order_status", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderEvents_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineItemEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtcCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    PreviousStatus = table.Column<LineItemStatus>(type: "line_item_status", nullable: true),
                    NewStatus = table.Column<LineItemStatus>(type: "line_item_status", nullable: true),
                    LineItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineItemEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineItemEvents_LineItems_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "LineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineItemEvents_LineItemId",
                table: "LineItemEvents",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LineItems_OrderId",
                table: "LineItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEvents_OrderId",
                table: "OrderEvents",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineItemEvents");

            migrationBuilder.DropTable(
                name: "OrderEvents");

            migrationBuilder.DropTable(
                name: "LineItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
