using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Warehouse.Data;
using Warehouse.Domain;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;

namespace Warehouse.Services.services
{
    public class BoxesFieldService : IBoxesFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BoxesFieldService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Buy section for add to repository
        public async Task<int> AddAsync(InputBoxesDto inDto)
        {
            if (inDto == null) throw new ArgumentNullException(nameof(inDto));
            if ((inDto.BoFOpDetails.Count() == 0) || (inDto.BoFOpDetails == null)) throw new ArgumentNullException(nameof(inDto));
            if ((inDto.BoFOpDetails[0].Direction != "خارج") && (inDto.BoFOpDetails[0].Direction != "داخل فارغ") && (inDto.BoFOpDetails[0].Direction != "داخل ممتلئ")) throw new ArgumentNullException(nameof(inDto));

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var Boxes_operation = _mapper.Map<BoFOperations>(inDto);
                    Boxes_operation.BoFOpDetails = _mapper.Map<List<BoFOpDetails>>(inDto.BoFOpDetails);

                    // Check if the repository exists
                    Boxes_operation.Count = inDto.BoFOpDetails.Sum(m => m.Count);


                    // Check if the buyer exists or add a new buyer if not
                    if (await GetOrCreateUserAsync(inDto, Boxes_operation))
                    {
                       
                        _unitOfWork.repository<BoFOperations>().Add(Boxes_operation);
                        await _unitOfWork.Complete();

                        if(Boxes_operation.BoFOpDetails.First().Direction=="خارج")
                        {
                            await UpdateBoFTotalAsync(Boxes_operation.Count, false);
                        }
                        else
                        {
                            await UpdateBoFTotalAsync(Boxes_operation.Count, true);
                        }

                        transactionScope.Complete(); // Commit transaction

                        return Boxes_operation.Id;
                        
                    }
                    return 0; // error
                }
                catch (Exception)
                {
                    // If anything fails, the transaction will be rolled back
                    throw;
                }
            }
        }

        private async Task<bool> GetOrCreateUserAsync(InputBoxesDto inDto, BoFOperations inOut)
        {
            if (string.IsNullOrWhiteSpace(inDto.BoFUserName))
                throw new ArgumentException("user name cannot be null or empty");

            var supplierNameWithoutSpaces = Regex.Replace(inDto.BoFUserName, @"\s+", "");
            var supplier = (await _unitOfWork.repository<BoFUser>().Get(m => m.UserNameWithoutSpaces == supplierNameWithoutSpaces)).FirstOrDefault();
            
            if (supplier != null)
            {
                inOut.BoFUserId = supplier.Id;
                inOut.BoFUserName = supplier.UserName;

                return true;
            }
            else
            {
                supplier = new BoFUser
                {
                    UserName = inOut.BoFUserName,
                    UserNameWithoutSpaces = supplierNameWithoutSpaces
                };
                
                _unitOfWork.repository<BoFUser>().Add(supplier);
                await _unitOfWork.Complete();

                inOut.BoFUserId = supplier.Id;
                inOut.BoFUserName = supplier.UserName;
            }

            return true;
        }

       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TotalOut">The amount to be added to the TotalOut property.</param>
        /// <returns>
        /// Returns true if the update is successful and the TotalOut property is updated.
        /// Returns false if the TotalFunds entity is not found in the database.
        /// </returns>
        private async Task<bool> UpdateBoFTotalAsync(int count, bool IsIn_Operation)
        {
            // Find the existing entity by id
            var existingTotalBoxes = (await _unitOfWork.repository<BoFTotal>().GetAllAsync()).FirstOrDefault();
            if (existingTotalBoxes == null)
            {
                return false; // Entity not found
            }

            // Update the properties
            if (IsIn_Operation)
            {
                existingTotalBoxes.TotalIn += count;
               
            }
            else
            {
                existingTotalBoxes.TotalOut += count;
                
            }

            existingTotalBoxes.Current = existingTotalBoxes.TotalIn - existingTotalBoxes.TotalOut;

            _unitOfWork.repository<BoFTotal>().UpdateAsync(existingTotalBoxes);
            // Save changes to the database
            await _unitOfWork.Complete();

            return true; // Update successful
        }

        #endregion



        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // Retrieve the Repository_InOut entity by ID
                            var inOut = await _unitOfWork.repository<BoFOperations>().GetByIdAsync(id);
                    if (inOut == null)
                    {
                        return false; // Entity not found
                    }

                            // Retrieve the associated supplier
                            // var supplier = await _unitOfWork.repository<BoFUser>().GetByIdAsync(inOut.BoFUserId);

                    await UpdateBoFTotalAsync(-inOut.Count, inOut.BoFOpDetails.First().Direction == "خارج" ? false : true);

                    // Remove the Repository_InOut entity
                    _unitOfWork.repository<BoFOperations>().Delete(inOut);
                    await _unitOfWork.Complete();

                    transactionScope.Complete(); // Commit transaction
                    return true; // Successfully deleted

                }

                catch (Exception)
                {
                    // If an exception is thrown, it will rollback automatically
                    throw;
                }
             }
        }



        public async Task<GetAllBoF_MainDto> GetBoFMainPage()
        {
            var BoFTotal_model = await _unitOfWork.repository<BoFTotal>().GetAllAsync();

            var res_ = new GetAllBoF_MainDto();
            res_.TopOfThePage= _mapper.Map<GetAllBoF_TotalDto>(BoFTotal_model.FirstOrDefault());

            #region Middle of the main page
            res_.MiddleOfThePage = new List<GetBoF_ColorGroupDto>();

            var BoFOpDetails_models = await _unitOfWork.repository<BoFOpDetails>().GetAllAsync(includeProperties: "BoFOperation");
            if (BoFOpDetails_models.Any())
            {
                // Group by ColorType and convert to Dictionary<string, List<BoFOpDetails>>
                Dictionary<string, List<BoFOpDetails>> BoFOpDetails_ColorGrouped = BoFOpDetails_models
                    .GroupBy(d => d.ColorType)
                    .ToDictionary(g => g.Key, g => g.ToList());

               

                int totalin = 0;
                int totalOut = 0;
                foreach (var single_Color in BoFOpDetails_ColorGrouped)
                {
                    var Helper_ColorGrouped = new GetBoF_ColorGroupDto
                    {
                        Color = single_Color.Key,
                        TotalIn = 0,
                        TotalOut = 0,
                        Remainder = 0
                    };

                    foreach (var detail in single_Color.Value)
                    {
                        if (detail.Direction == "خارج")
                        {
                            Helper_ColorGrouped.TotalOut += detail.Count;
                        }
                        else
                        {
                            Helper_ColorGrouped.TotalIn += detail.Count;
                        }
                    }
                    Helper_ColorGrouped.Remainder = Helper_ColorGrouped.TotalIn - Helper_ColorGrouped.TotalOut;
                    res_.MiddleOfThePage.Add(Helper_ColorGrouped);

                }
            }
           
            #endregion

            #region Bottom of the main page

            var BoFUsers_models = await _unitOfWork.repository<BoFUser>().GetAllAsync(includeProperties: "BoFOperations");
            
            res_.BottomOfThePage = new List<GetBoF_UserGroupDto>();

            
            foreach (var single_user in BoFUsers_models)
            {
                var Helper_UserGrouped = new GetBoF_UserGroupDto
                {
                    UserId = single_user.Id,
                    UserName=single_user.UserName,
                    TotalOut = 0,
                    TotalIn = 0,                    
                    Remainder = 0
                };

                foreach (var detail in single_user.BoFOperations)
                {
                    if(detail.BoFOpDetails.Any())
                    {
                        if (detail.BoFOpDetails.First().Direction == "خارج")
                        {
                            Helper_UserGrouped.TotalOut += detail.Count;
                        }
                        else
                        {
                            Helper_UserGrouped.TotalIn += detail.Count;
                        }
                    }                  
                }
                Helper_UserGrouped.Remainder = Helper_UserGrouped.TotalOut - Helper_UserGrouped.TotalIn;
                res_.BottomOfThePage.Add(Helper_UserGrouped);

            }
            res_.BottomOfThePage = res_.BottomOfThePage.OrderByDescending(m => m.Remainder).ToList();

            #endregion
            return res_;
        }

        public async Task<GetAllBoF_OperationsDto> GetBoFOperationsPage_ByUserId(int UserId)
        {
            var res_ = new GetAllBoF_OperationsDto();

            #region Top of the operations page

            res_.TopOfThePage = new List<GetBoF_ColorGroupDto>();

            var BoFOperations_models = await _unitOfWork.repository<BoFOperations>().Get(m=>m.BoFUserId==UserId, includeProperties: "BoFOpDetails");
            if (BoFOperations_models.Any())
            {
                // Group by ColorType and convert to Dictionary<string, List<BoFOpDetails>>
                Dictionary<string, List<BoFOpDetails>> BoFOpDetails_ColorGrouped = BoFOperations_models.SelectMany(m=>m.BoFOpDetails)
                    .GroupBy(d => d.ColorType)
                    .ToDictionary(g => g.Key, g => g.ToList());


                int totalin = 0;
                int totalOut = 0;
                foreach (var single_Color in BoFOpDetails_ColorGrouped)
                {
                    var Helper_ColorGrouped = new GetBoF_ColorGroupDto
                    {
                        Color = single_Color.Key,
                        TotalIn = 0,
                        TotalOut = 0,
                        Remainder = 0
                    };

                    foreach (var detail in single_Color.Value)
                    {
                        if (detail.Direction == "خارج")
                        {
                            Helper_ColorGrouped.TotalOut += detail.Count;
                        }
                        else
                        {
                            Helper_ColorGrouped.TotalIn += detail.Count;
                        }
                    }
                    Helper_ColorGrouped.Remainder = Helper_ColorGrouped.TotalOut - Helper_ColorGrouped.TotalIn;
                    res_.TopOfThePage.Add(Helper_ColorGrouped);

                }
            }

            #endregion

            #region Bottom of the main page

            //var BoFUsers_models = await _unitOfWork.repository<BoFUser>().GetAllAsync(includeProperties: "BoFOperations");

            res_.BottomOfThePage = _mapper.Map<List<GetBoF_AllOperationsDto>>(BoFOperations_models.OrderByDescending(m=>m.Date).ToList());


            #endregion
            return res_;
        }

        public async Task<BoF_DetailedDto> GetByIdAsync(int Operation_id)
        {
            var models_ = (await _unitOfWork.repository<BoFOperations>().Get(m=>m.Id== Operation_id));
            if(models_.Any())
            {
                var res_= _mapper.Map<BoF_DetailedDto>(models_.FirstOrDefault());
                res_.BoFOpDetails = _mapper.Map<List<BoF_OpDetailsDto>>(models_.FirstOrDefault().BoFOpDetails);
                
                return res_;
            }
            return null;
        }

        public async Task<BoF_InfoDTO> GetBoF_Info()
        {
            var models_ = (await _unitOfWork.repository<BoFUser>().GetAllAsync());
            var res_ = new BoF_InfoDTO();
            res_.Users = _mapper.Map<List<BoF_UserDTO>>(models_);
            
            return res_;
        }

        //BoF_InfoDTO

        //public async Task<List<Cars_InDetailsDto>> GetAllAsync_ForDesktop()
        //{
        //    var inOuts = await _unitOfWork.repository<Cars>().GetAllAsync();
        //    return _mapper.Map<List<Cars_InDetailsDto>>(inOuts.ToList());
        //}


    }
}
