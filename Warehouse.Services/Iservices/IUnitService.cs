using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;

namespace Warehouse.Services.Iservices
{
    public interface IUnitService
    {
        #region Get All, Get By Id
         Task<List<UnitDto>> GetAllAsync();

         Task<UnitDto?> GetByIdAsync(int id);

         Task<List<UnitDto>> GetByStatusAsync(STATUS status);

        #endregion

        #region Add, Update, Change status, Delete
         Task<int> CreateAsync(UnitDto dto);

         Task<bool> UpdateAsync(UnitDto dto);

         Task<DeleteResourceResult> ChangeStatusAsync(int id);

         Task<DeleteResourceResult> DeleteAsnc(int id);

        #endregion

        #region functions for validations

        bool IsIdExist(int id);

        bool IsNameExist(int id,string name);

        #endregion
    }

}
