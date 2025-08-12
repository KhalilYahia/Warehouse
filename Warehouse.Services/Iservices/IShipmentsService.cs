
using Microsoft.EntityFrameworkCore;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;

namespace Warehouse.Services.Iservices
{
    public interface IShipmentsService
    {
        #region Get All, Get By Id
        Task<List<ShipmentDto>> Search(SearchInShipmentsDto dto);

        Task<ShipmentDto?> GetByIdAsync(int InboundDocument_Id);

        #endregion

        #region Add, Update, Delete
        Task<int> CreateAsync(ShipmentDto dto);

        Task<bool> UpdateAsync(ShipmentDto dto);

        Task<DeleteResourceResult> UnSign(int OutboundDocumentId);
        Task<DeleteResourceResult> DeleteAsnc(int id);

        #endregion

        #region functions for validations

        bool IsIdExist(int id);

        bool IsResourceExistAndActive(int ResourceId);

        bool IsUnitExistAndActive(int UnitId);

        bool IsClientExistAndActive(int ClientId);
        bool IsSigned(int id);

        #endregion
    }

}
