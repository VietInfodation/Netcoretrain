namespace SharedService.Interfaces
{
    public interface IUnitofWorks
    {
        IGenericRepository<T> GetRepository<T>() where T : class;
        int Commit();
        void Rollback();
        void BeginTransaction();
    }
}
