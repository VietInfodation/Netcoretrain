using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedService.Interfaces
{
    public interface ICsvService
    {
        string WriteCSV<dynamic>(IQueryable<dynamic> records);
        string WriteXlsx<dynamic>(IQueryable<dynamic> records);
    }
}
