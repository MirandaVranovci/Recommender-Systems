using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Recommender_system.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
    public class SRDB : SREntities
    {
        public override int SaveChanges()
        {
            var userid = 0;
            try
            {
                var user = (GetUser)HttpContext.Current.Session["User"];
                userid = user.ID;

                foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified))
                {
                    // For each changed record, get the audit record entries and add them
                    foreach (LOG x in GetAuditRecordsForChange(ent, userid))
                    {
                        Log.Add(x);
                    }
                }


            }
            catch
            {
                // Get all Added/Deleted/Modified entities (not Unmodified or Detached)

            }

            // Call the original SaveChanges(), which will save both the changes made and the audit records
            return SaveChanges();
        }

        public override async Task<int> SaveChangesAsync()
        {

            var userid = 0;
            try
            {
                var user = (GetUser)HttpContext.Current.Session["User"];
                if (user != null)
                {
                    userid = user.ID;

                    foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified))
                    {
                        // For each changed record, get the audit record entries and add them
                        foreach (LOG x in GetAuditRecordsForChange(ent, userid))
                        {
                           // Log.Add(x);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }

            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)


            // Call the original SaveChanges(), which will save both the changes made and the audit records
            try
            {
                return await base.SaveChangesAsync();
            }

            catch (Exception ex)
            {
                //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }

            return 0;
        }

        private List<LOG> GetAuditRecordsForChange(DbEntityEntry dbEntry, int userId)
        {
            List<LOG> result = new List<LOG>();

            DateTime changeTime = DateTime.Now;

            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;

            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;

            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            string keyName = "ID";//dbEntry.Entity.GetType().GetProperties().Single(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;

            if (dbEntry.State == EntityState.Added)
            {
                // For Inserts, just add the whole record
                // If the entity implements IDescribableEntity, use the description from Describe(), otherwise use ToString()
                result.Add(new LOG()
                {
                    //LogID = Guid.NewGuid(),
                    //EventType = "New", // Added
                    //TableName = tableName,
                    //RecordID = dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),  // Again, adjust this if you have a multi-column key
                    //ColumName = "*ALL",    // Or make it nullable, whatever you want
                    //NewValue = (dbEntry.CurrentValues.ToObject() is IDescribableEntity) ? (dbEntry.CurrentValues.ToObject() as IDescribableEntity).Describe() : dbEntry.CurrentValues.ToObject().ToString(),
                    //CreatedBy = userId,
                    //Created_date = changeTime
                }
                    );
            }
            else if (dbEntry.State == EntityState.Deleted)
            {

                // Same with deletes, do the whole record, and use either the description from Describe() or ToString()
                result.Add(new LOG()
                {
                    //LogID = Guid.NewGuid(),
                    //EventType = "Delete", // Deleted
                    //TableName = tableName,
                    //RecordID = dbEntry.OriginalValues.GetValue<object>(keyName).ToString(),
                    //ColumName = "*ALL",
                    //NewValue = (dbEntry.OriginalValues.ToObject() is IDescribableEntity) ? (dbEntry.OriginalValues.ToObject() as IDescribableEntity).Describe() : dbEntry.OriginalValues.ToObject().ToString(),
                    //CreatedBy = userId,
                    //Created_date = changeTime
                }
                    );
            }
            else if (dbEntry.State == EntityState.Modified)
            {
                foreach (string propertyName in dbEntry.OriginalValues.PropertyNames)
                {
                    var originalValue = dbEntry.GetDatabaseValues().GetValue<object>(propertyName);

                    // For updates, we only want to capture the columns that actually changed
                    if (!object.Equals(originalValue, dbEntry.CurrentValues.GetValue<object>(propertyName)))
                    {
                        result.Add(new LOG()
                        {
                            //LogID = Guid.NewGuid(),
                            //EventType = "Update",    // Modified
                            //TableName = tableName,
                            //RecordID = dbEntry.OriginalValues.GetValue<object>(keyName).ToString(),
                            //ColumName = propertyName,
                            //OldValue = originalValue == null ? null : originalValue.ToString(),
                            //NewValue = dbEntry.CurrentValues.GetValue<object>(propertyName) == null ? "empty" : dbEntry.CurrentValues.GetValue<object>(propertyName).ToString(),
                            //CreatedBy = userId,
                            //Created_date = changeTime
                        }
                            );
                    }
                }
            }
            // Otherwise, don't do anything, we don't care about Unchanged or Detached entities

            return result;
        }

        public DbSet<LOG> Log { get; set; }

    }

 

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

}