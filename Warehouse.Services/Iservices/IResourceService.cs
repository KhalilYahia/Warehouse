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
    public interface IResourceService
    {
        #region Get All, Get By Id
        Task<List<ResourceDto>> GetAllAsync();

        Task<ResourceDto?> GetByIdAsync(int id);

        Task<List<ResourceDto>> GetByStatusAsync(STATUS status);

        #endregion

        #region Add, Update, Change status, Delete
        Task<int> CreateAsync(ResourceDto dto);

        Task<bool> UpdateAsync(ResourceDto dto);

        Task<DeleteResourceResult> ChangeStatusAsync(int id);

        Task<DeleteResourceResult> DeleteAsnc(int id);

        #endregion

        #region functions for validations

        bool IsIdExist(int id);

        bool IsNameExist(int id, string name);

        #endregion
    }

}
