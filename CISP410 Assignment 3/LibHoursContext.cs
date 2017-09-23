namespace CISP410_Assignment_3
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    // Class built my Entity framework
    public class LibHoursContext : DbContext
    {
        // Your context has been configured to use a 'LibHoursContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'CISP410_Assignment_2.LibHoursContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'LibHoursContext' 
        // connection string in the application configuration file.
        public LibHoursContext()
            : base("name=LibHoursContext")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<LibHour> LibHours { get; set; }
    }
    // Context used to model database
    public class LibHour
    {
        public int Id { get; set; }
        public string LibName { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Saturday { get; set; }
        public string Sunday { get; set; }
    }
}