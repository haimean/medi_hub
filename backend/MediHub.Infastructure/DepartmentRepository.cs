using Microsoft.Extensions.Configuration;
using MISA.ApplicationCore.Entities;
using MISA.ApplicationCore.Interfaces.Repository;
using MISA.Infrastructure;

namespace MISA.Infastructure
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        #region DECLARE

        #endregion

        #region Contructor
        public DepartmentRepository(IConfiguration configuration) : base(configuration)
        {

        }
        #endregion

        #region Method


        #endregion
    }
}
