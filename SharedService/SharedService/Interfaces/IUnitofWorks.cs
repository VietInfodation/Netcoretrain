using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
