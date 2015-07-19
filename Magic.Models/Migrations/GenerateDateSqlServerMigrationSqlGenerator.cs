using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;

namespace Magic.Models.Migrations
{
    internal class GenerateDateSqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        protected override void Generate(AddColumnOperation addColumnOperation)
        {
            SetDateCreatedColumn(addColumnOperation.Column);

            base.Generate(addColumnOperation);
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            SetDateCreatedColumn(createTableOperation.Columns);

            base.Generate(createTableOperation);
        }

        private static void SetDateCreatedColumn(IEnumerable<ColumnModel> columns)
        {
            foreach (var columnModel in columns)
            {
                SetDateCreatedColumn(columnModel);
            }
        }

        private static void SetDateCreatedColumn(PropertyModel column)
        {
            if (column.Name == "DateCreated" || column.Name == "TimeSend")
            {
                column.DefaultValueSql = "GETDATE()";
            }
        }
    }
}
