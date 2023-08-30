using Coursesvc.Models;
using SharedService.Interfaces;
using SharedService.Repository;

namespace SharedService
{
    public class UnitofWorks : IUnitofWorks
    {
        private readonly CourseContext _context;

        //private IDbContextTransaction _transaction;

        public UnitofWorks(CourseContext context)//, //IDbContextTransaction transaction)
        {

            //_transaction = transaction;
            _context = context;
        }
        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public int Commit()
        {
            return _context.SaveChanges();
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            return new GenericRepository<T>(_context);
        }

        public void Rollback()
        {
            _context.Database.CurrentTransaction.Rollback();
        }
    }
}