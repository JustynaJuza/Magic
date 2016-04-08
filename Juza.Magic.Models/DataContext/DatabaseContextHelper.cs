using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using Juza.Magic.Models.Extensions;

namespace Juza.Magic.Models.DataContext
{
    // TODO: Review this when its needed in controllers.
    public class DatabaseContextHelper
    {
        private readonly IDbContext _context;
        //private readonly IErrorDescriptionResolver _errorDescriptionResolver;

        public DatabaseContextHelper(IDbContext context)
            //IErrorDescriptionResolver errorDescriptionResolver)
        {
            _context = context;
            //_errorDescriptionResolver = errorDescriptionResolver;
        }

        private readonly Type[] _handledExceptions =
        {
            typeof (DbUpdateException), typeof (DbUpdateConcurrencyException),
            typeof (DbEntityValidationException), typeof (NotSupportedException),
            typeof (ObjectDisposedException), typeof (InvalidOperationException)
        };
        
        //public SuccessResult Save()
        //{
        //    var result = new SuccessResult();
        //    try
        //    {
        //        result.Success = _context.SaveChanges() > 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!ex.HandleException(_handledExceptions)) throw;
        //        result.Exception = ex;
        //        //result.Description = _errorDescriptionResolver.GetDatabaseErrorDescription(ex);
        //    }

        //    return result;
        //}
    }
}