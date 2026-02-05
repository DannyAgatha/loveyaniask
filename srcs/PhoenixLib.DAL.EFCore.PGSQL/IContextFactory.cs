// NosEmu
// 


using Microsoft.EntityFrameworkCore;

namespace PhoenixLib.DAL.EFCore.PGSQL
{
    public interface IContextFactory<out T> where T : DbContext
    {
        /// <summary>
        ///     Instantiates a new <see cref="T" />
        /// </summary>
        /// <returns></returns>
        T CreateContext();
    }
}