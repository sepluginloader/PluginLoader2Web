using FluentMigrator;

namespace PluginLoader2Web.Data.Migrations
{
    [Migration(20250508)]
    public class CreateTableSchema : Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            //Web Users. Only store email, username etc.
            Create.Table("UserAccounts")
                .WithColumn("GithubID").AsInt64().PrimaryKey() //This is a unique userid
                .WithColumn("DiscordID").AsInt64().Nullable() //Possible Link DiscordID
                .WithColumn("Username").AsString(256).NotNullable() //Store username
                .WithColumn("Email").AsString(256).NotNullable() //Possible email updates?
                .WithColumn("Role").AsByte().NotNullable() //Role based authorization for certain pages
                .WithColumn("JoinDate").AsDateTime().NotNullable() //Website JoinDate? May be useful
                .WithColumn("LastUpdate").AsDateTime().NotNullable(); //Last Plugin Update to see if they have been online

            // PluginProjects table
            Create.Table("PluginProjects")
                .WithColumn("PluginId").AsGuid().PrimaryKey()   // Private unique identifier for the plugin
                .WithColumn("Name").AsString(256).NotNullable() // Name of the plugin
                .WithColumn("AuthorID").AsInt64().NotNullable() // Username of the plugin author
                .WithColumn("ToolTip").AsString().NotNullable()
                .WithColumn("LongDescription").AsString().NotNullable()
                .WithColumn("RepoURL").AsString().NotNullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable() // Date when the plugin was created
                .WithColumn("CommunitySpotlight").AsBoolean().NotNullable(); // Flag for community spotlight

            // PluginProjectVersions table
            Create.Table("PluginProjectVersions")
                .WithColumn("VersionId").AsGuid().PrimaryKey()  // Unique internal identifier for the version
                .WithColumn("PluginID").AsGuid().NotNullable()  // PluginID to tie to plugin project table
                .WithColumn("PluginVersion").AsString(50).NotNullable()
                .WithColumn("CommitID").AsString(50).NotNullable() // CommitID of release
                .WithColumn("Beta").AsBoolean().NotNullable()
                .WithColumn("ReleaseDate").AsDateTime().NotNullable() // Release date of the version
                .WithColumn("ChangeLog").AsString().Nullable();

            // Add foreign key constraint
            Create.ForeignKey("FK_PluginProjectVersions_PluginProjects")
                .FromTable("PluginProjectVersions").ForeignColumn("PluginID")
                .ToTable("PluginProjects").PrimaryColumn("PluginId");
        }

        /// <inheritdoc />
        public override void Down()
        {
            // Drop foreign key first
            Delete.ForeignKey("FK_PluginProjectVersions_PluginProjects").OnTable("PluginProjectVersions");

            // Drop tables
            Delete.Table("PluginProjectVersions");
            Delete.Table("PluginProjects");
        }


    }
}
