using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VanityUrls.Data.Migrations
{
    public partial class VanityUrlUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex("IX_AspNetUsers_VanityUrl", "AspNetUsers", "VanityUrl", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_AspNetUsers_VanityUrl");
        }
    }
}
