﻿namespace SharedService.Interfaces
{
    public interface ICsvService
    {
        string WriteCSV<dynamic>(IQueryable<dynamic> records);
        string WriteXlsx<dynamic>(IQueryable<dynamic> records);
    }
}
