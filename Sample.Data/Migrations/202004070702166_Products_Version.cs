﻿namespace Sample.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Products_Version : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Timestamp", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "Timestamp");
        }
    }
}
