using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Add new Employee, 
        /// إضافة ورشة جديدة
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<int> AddNewEmployee(InputEmployeeDto dto)
        {
            var model = _mapper.Map<Employees>(dto);
            model.TotalWage = 0;
            model.TotalRewards = 0;
            model.Payments = 0;
            model.TotalDiscount = 0;
            model.TotalWageAfterDiscount = 0;
            model.Remainder = 0;

            _unitOfWork.repository<Employees>().Add(model);
            await _unitOfWork.Complete();
            
            return model.Id;
        }

        /// <summary>
        /// update exist Employee,
        /// التعديل سيكون فقط على الحقول التي يتم إدخالها
        /// ولن تؤثر على الأجور السابقة للعامل أي انه
        /// اي زيادة في الأجر لن تنعكس على عدد ساعات الدوام السابقة
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> Edit(InputEmployeeDto dto)
        {
            var models = await _unitOfWork.repository<Employees>().Get(m => m.Id == dto.Id);
            if (models.Any())
            {
                var model = models.First();
                model.AdditionalWorkingHourWage = dto.AdditionalWorkingHourWage;
                model.NormHWageM = dto.NormHWageM;
                model.NormHWageG = dto.NormHWageG;
                //model.Count = dto.Count;
                model.Date = dto.Date;
                model.workshopName = dto.workshopName;
               
                model.Notes = dto.Notes;
                _unitOfWork.repository<Employees>().Update(model);
                await _unitOfWork.Complete();
                return true;

            }
            else
                return false;

        }

        /// <summary>
        /// delete Employee, 
        /// حذف عامل لن يؤثر على الأموال المدفوعة له أي لن يؤثر على الصندوق أبدا
        /// اذا اعاد هذا التابع null فإنه يجب عليك حذف تفقد العامل
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool>? Delete(int id)
        {
            var models = await _unitOfWork.repository<Employees>().Get(m => m.Id == id);
            if (models.Any())
            {
                var model = models.First();
                if (model.DailyChekEmployees.Any())
                    return false;

                _unitOfWork.repository<Employees>().Delete(model);
                await _unitOfWork.Complete();
                return true;

            }
            else
                return false;
        }

        /// <summary>
        /// Get Employees by name ,
        /// اذا اردت جميع العمال ضع الاسم فارغ
        /// </summary>
        /// <returns></returns>
        public async Task<List<EmployeeDto>> GetEmployees(string workshopName)
        {
            List<Employees> model;
            if (string.IsNullOrEmpty(workshopName))
                model = (await _unitOfWork.repository<Employees>().GetAllAsync()).ToList();
            else
                model = (await _unitOfWork.repository<Employees>().Get(m => m.workshopName.ToLower().Contains(workshopName.ToLower()))).ToList();
            return _mapper.Map<List<Employees>, List<EmployeeDto>>(model.OrderByDescending(m => m.Date).ToList());

            //}
            //else { return new List<CategoryDto>(); }
        }

        /// <summary>
        /// Get all Employees  ,
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<EmployeeDto>> GetEmployees_ForDesktop()
        {
            List<Employees> model;
            
            model = (await _unitOfWork.repository<Employees>().GetAllAsync()).ToList();
            
            return _mapper.Map<List<Employees>, List<EmployeeDto>>(model.OrderByDescending(m => m.Date).ToList());

            //}
            //else { return new List<CategoryDto>(); }
        }
        public async Task<EmployeeDto> GetEmployee_ByEmployeeId(int EmployeeId)
        {
            var models = (await _unitOfWork.repository<Employees>().Get(m => m.Id==EmployeeId)).ToList();
            if(models.Any())
            {
                return _mapper.Map<EmployeeDto>(models.FirstOrDefault());
            }
            return null;

        }

        /// <summary>
        /// Get Employee by id
        /// </summary>
        /// <returns></returns>
        public async Task<List<DailyChekEmployeesDto>> GetDailyCheckEmployeeByEmployeeId(int EmployeeId)
        {
            //if ((languageCode == (int)LanguageHelper.ARABIC) || (language == (int)LanguageHelper.ENGLISH))
            //    {
            var models = await _unitOfWork.repository<Employees>().Get(m => m.Id == EmployeeId);
            if (models.Any())
            {
                var model = models.FirstOrDefault();
                var dailyCheck = model.DailyChekEmployees.OrderByDescending(m => m.Date).ToList();
                var res = _mapper.Map<List<DailyChekEmployees>, List<DailyChekEmployeesDto>>(dailyCheck);
                foreach (var single in res)
                {
                    single.workshopName = model.workshopName;
                }

                return res;
            }
            else
                return new List<DailyChekEmployeesDto>();


            //}
            //else { return new List<CategoryDto>(); }
        }


        #region Financial

        /// <summary>
        /// إضافة تفقد يومي 
        /// في حال اعاد هذا التابع قيمة صفر فإنه يعني أن هذا الموظف غير موجود
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<int> AddNewDailyCheck(InputDailyChekEmployeesDto dto)
        {
            var model = _mapper.Map<InputDailyChekEmployeesDto, DailyChekEmployees>(dto);
            var emps = await _unitOfWork.repository<Employees>().Get(m => m.Id == dto.EmployeeId);
            if (emps.Any())
            {
                var emp = emps.FirstOrDefault();
                model.AdditionalWorkingHourWage = emp.AdditionalWorkingHourWage;
                model.NormHWageM = emp.NormHWageM;
                model.NormHWageG = emp.NormHWageG;                
                model.TotalWage = (model.MenCount * model.NormHWageM * model.NormJobHCount) +
                                  (model.GirlsCount * model.NormHWageG * model.NormJobHCount)+
                                  ((model.MenCount + model.GirlsCount) * model.AdditionalWorkingHourWage * model.AddJobHCount);

                model.ResultWage = model.TotalWage + model.Reward - model.Discount;

                model.PaidWage = dto.PaidWage;
               

                emp.Payments += model.PaidWage;
                emp.TotalDiscount += model.Discount;
                emp.TotalRewards += model.Reward;
                emp.TotalWage += model.TotalWage;
                emp.TotalWageAfterDiscount += model.ResultWage;
                emp.Remainder = emp.TotalWageAfterDiscount - emp.Payments;
                if(emp.DailyChekEmployees==null)
                    emp.DailyChekEmployees=new List<DailyChekEmployees>();
                emp.DailyChekEmployees.Add(model);



                _unitOfWork.repository<Employees>().Update(emp);
                await _unitOfWork.Complete();

                if (model.PaidWage != 0)
                {
                    if (!await UpdateTotalFundsAsync(model.PaidWage))
                    {
                        return 0;
                    }
                }

                return model.Id;

            }
            else
                return 0;
        }

        private async Task<bool> UpdateTotalFundsAsync(decimal TotalOut)
        {
            // Find the existing entity by id
            var existingTotalFunds = (await _unitOfWork.repository<Resource>().GetAllAsync()).FirstOrDefault();
            if (existingTotalFunds == null)
            {
                return false; // Entity not found
            }

            // Update the properties
            existingTotalFunds.TotalOut += TotalOut;
            existingTotalFunds.Profits = existingTotalFunds.TotalIn - existingTotalFunds.TotalOut;
            existingTotalFunds.CurrentFund -= TotalOut;

            _unitOfWork.repository<Resource>().UpdateAsync(existingTotalFunds);
            // Save changes to the database
            await _unitOfWork.Complete();

            return true; // Update successful
        }


        /// <summary>
        /// حذف تفقد يومي 
        /// </summary>
        /// <param name="DailyCheckId">معرف التفقد اليومي</param>
        /// <returns></returns>
        public async Task<bool> DeleteDailyCheck(int DailyCheckId)
        {
            bool res_ = false;
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var dailyChecks = await _unitOfWork.repository<DailyChekEmployees>().Get(m => m.Id == DailyCheckId);
                    if (dailyChecks.Any())
                    {
                        var dailyCheck = dailyChecks.FirstOrDefault();
                        var emp = dailyCheck.Employee;

                        emp.Payments -= dailyCheck.PaidWage;
                        emp.TotalDiscount -= dailyCheck.Discount;
                        emp.TotalRewards -= dailyCheck.Reward;
                        emp.TotalWage -= dailyCheck.TotalWage;
                        emp.TotalWageAfterDiscount -= dailyCheck.ResultWage;
                        emp.Remainder = emp.TotalWageAfterDiscount - emp.Payments;


                        _unitOfWork.repository<Employees>().Update(emp);
                        await _unitOfWork.Complete();

                        _unitOfWork.repository<DailyChekEmployees>().Delete(dailyCheck);
                        await _unitOfWork.Complete();
                        res_ = true;
                        if (dailyCheck.PaidWage != 0)
                        {
                            res_= await UpdateTotalFundsAsync(-(dailyCheck.PaidWage)); }
                        //

                          }
                    transactionScope.Complete(); // Commit transaction

                    return res_;


                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
       


        #endregion

    }
}
