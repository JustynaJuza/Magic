using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Juza.Magic.Models.DataContext
{
    public partial class MagicDbContext
    {
        public class UseDateTime2Convention : Convention
        {
            public UseDateTime2Convention()
            {
                Properties<DateTime>()
                    .Configure(c => c.HasColumnType("datetime2").HasPrecision(0));
            }
        }

        public class DateCreatedIsGeneratedConvention : Convention
        {
            public DateCreatedIsGeneratedConvention()
            {
                Properties<DateTime>()
                    .Where(x => x.Name == "DateCreated")
                    .Configure(x => x.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed));
            }
        }
    }
}