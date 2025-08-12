using Microsoft.EntityFrameworkCore;
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
    public interface IReceiptService
    {
        #region Get All, Get By Id
         Task<List<ReceiptDto>> Search(SearchInReceiptsDto dto);
         Task<ReceiptDto?> GetByIdAsync(int InboundDocument_Id);

        #endregion

        #region Add, Update, Change status, Delete
        Task<int> CreateAsync(ReceiptDto dto);

         Task<bool> UpdateAsync(ReceiptDto dto);

         Task<DeleteResourceResult> DeleteAsnc(int id);

        #endregion

        #region functions for validations

        bool IsIdExist(int id);

        bool IsResourcesExistAndActive(int ResourceId);

        bool IsUnitssExistAndActive(int UnitId);
        bool IsInboundDocNumberExist(int id,string number);
        Task<FilterReceiptRequiresDto> GetAllFitters();

        #endregion
    }

}
