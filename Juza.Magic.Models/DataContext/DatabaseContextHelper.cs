using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

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

        //public static string ShowErrorMessage(Exception ex)
        //{
        //    if (ex is ArgumentNullException)
        //    {
        //        return "This item seems to no longer be there... It has probably been deleted in the meanwhile.";
        //    }

        //    if (ex is DbEntityValidationException)
        //    {
        //        var errors = string.Empty;
        //        foreach (var validationErrors in ((DbEntityValidationException) ex).EntityValidationErrors)
        //        {
        //            foreach (var validationError in validationErrors.ValidationErrors)
        //            {
        //                errors += "Property: " + validationError.PropertyName +
        //                    " <span class=\"text-danger\">Error: " + validationError.ErrorMessage + "</span><br />";
        //            }
        //        }

        //        return errors + ex;
        //    }

        //    return "There was a problem with saving to the database..." + ex;
        //    //    return "There was a problem with saving to the database... This is probably a connection problem, maybe try again."
        //}
    }
}