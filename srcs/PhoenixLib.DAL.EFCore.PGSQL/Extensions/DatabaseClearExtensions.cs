// NosEmu
// 


using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace PhoenixLib.DAL.EFCore.PGSQL.Extensions
{
    public static class DatabaseClearExtensions
    {
        public static void ResetTable<T>(this DbContext dbContext, DbSet<T> dbSet) where T : class
        {
            dbSet.DeleteFromQuery();
            dbContext.Database.ExecuteSqlRaw(CleanTable<T>());
        }


        private static string CleanTable<T>() =>
            @$"ALTER SEQUENCE ""{((TableAttribute)typeof(T).GetCustomAttribute(typeof(TableAttribute))).Schema}"".""{((TableAttribute)typeof(T).GetCustomAttribute(typeof(TableAttribute))).Name}_Id_seq"" RESTART;";
    }
}