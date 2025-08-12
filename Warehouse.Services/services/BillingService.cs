using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Warehouse.Data;
using Warehouse.Domain;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;

namespace Warehouse.Services.services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BillingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }



        public async Task<BillingDto> GetBilling(Search_BillingDto dto)
        {
            var result = new BillingDto();
            result.StartDate = dto.StartDate;
            result.EndDate = dto.EndDate;
            result.Refrigerator = dto.Refrigerator;
            result.CoolingRooms = dto.CoolingRooms;
            result.ExternalEnvoices = dto.ExternalEnvoices;
            result.Daily = dto.Daily;
            result.Tabali = dto.Tabali;
            result.Karasta = dto.Karasta;
            result.Fuel = dto.Fuel;
            result.Cars = dto.Cars;
            result.Plastic = dto.Plastic;
            result.Employees = dto.Employees;
            result.DealerId = dto.DealerId;
            decimal past_ = 0;

            result.DetailsBillingDto = new List<DetailsBillingDto>();
            if (dto.CoolingRooms)
            {
                var models = (await _unitOfWork.repository<CoolingRooms>().Get(m => (m.ClientId == dto.DealerId) &&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();
                decimal sum = 0;
                foreach(var model in models)
                {
                    result.DetailsBillingDto.Add(new DetailsBillingDto
                    {
                        Date = model.Date,
                        Count = model.TotalBoxes,
                        WeightAfterDiscount_2Percent=model.Weight,
                        Statement = model.MaterialName,
                        UnitPrice = model.CostOfUnit,
                        TotalPrice = model.CostOfAll
                    });
                    sum += model.CostOfAll;
                }
                result.Total = sum;
                result.DelearName = models.Any()?models.FirstOrDefault().ClientName : dto.DealerName;

            }
            else if(dto.Refrigerator)
            {
                var models = (await _unitOfWork.repository<Refrigerator>().Get(m => (m.BuyerId == dto.DealerId) &&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();
                decimal sum = 0;
                result.RefrigeratorDtos = new List<RefrigeratorDto>();
                if (models.Count() == 1)
                {
                    var model = models.FirstOrDefault();
                    result.RefrigeratorDtos.Add(new RefrigeratorDto
                    {
                        BuyerName = model.BuyerName,
                        Date = model.Date,
                        TotalBoxes = model.TotalBoxes,
                        TotalEmptyBoxesWeight = model.TotalEmptyBoxesWeight,
                        TotalBalanceCardWeight = model.TotalBalanceCardWeight,
                        TotalWeightAfterDiscount_2Percent = model.TotalWeightAfterDiscount_2Percent,
                        TotalSalesPriceOfAll = model.TotalSalesPriceOfAll,
                        RefrigeratorDetailsDtos = _mapper.Map<List<RefrigeratorDetailsDto>>(model.RefrigeratorDetails.ToList())

                    }) ;
                  
                    result.DelearName = models.Any() ? models.FirstOrDefault().BuyerName : dto.DealerName;
                }
                else
                {
                    foreach (var model in models)
                    {
                        result.RefrigeratorDtos.Add(new RefrigeratorDto
                        {
                            BuyerName = model.BuyerName,
                            Date = model.Date,
                            TotalBoxes = model.TotalBoxes,
                            TotalEmptyBoxesWeight = model.TotalEmptyBoxesWeight,
                            TotalBalanceCardWeight = model.TotalBalanceCardWeight,
                            TotalWeightAfterDiscount_2Percent = model.TotalWeightAfterDiscount_2Percent,
                            TotalSalesPriceOfAll = model.TotalSalesPriceOfAll                           

                        });
                        sum += model.TotalSalesPriceOfAll;
                    }
                    result.Total = sum;
                    result.DelearName = models.Any() ? models.FirstOrDefault().BuyerName : dto.DealerName;
                }

            }
            else if (dto.ExternalEnvoices)
            {
                var models = (await _unitOfWork.repository<ExternalEnvoices>().Get(m => (m.BuyerId == dto.DealerId) &&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();
                decimal sum = 0;
                foreach (var model in models)
                {
                    result.DetailsBillingDto.Add(new DetailsBillingDto
                    {
                        Date = model.Date,
                        Count = model.TotalBoxes,
                        WeightAfterDiscount_2Percent = model.Weight,
                        Statement = model.MaterialName,
                        UnitPrice = model.SalesPriceOfUnit,
                        TotalPrice = model.SalesPriceOfAll
                    });
                    sum += model.SalesPriceOfAll;
                }
                result.Total = sum;
                result.DelearName = models.Any() ? models.FirstOrDefault().BuyerName : dto.DealerName;
                                    

            }
            //else if (dto.OtherSales)
            //{

            //    var models = (await _unitOfWork.repository<OtherSales>().Get(m => m.BuyerId == dto.DealerId &&
            //                    dto.StartDate.HasValue ? m.Date > dto.StartDate : true &&
            //                    dto.EndDate.HasValue ? m.Date < dto.EndDate : true)).ToList();
            //    decimal sum = 0;
            //    foreach (var model in models)
            //    {
            //        result.DetailsBillingDto.Add(new DetailsBillingDto
            //        {
            //            Date = model.Date,
            //            Count = model.,
            //            Statement = model.MaterialName,
            //            UnitPrice = model.SalesPriceOfUnit,
            //            TotalPrice = model.SalesPriceOfAll
            //        });
            //        sum += model.SalesPriceOfAll;
            //    }
            //    result.Total = sum;
            //    result.DelearName = models.Any() ? models.FirstOrDefault().BuyerName
            //                        : (await _unitOfWork.repository<Buyers>().Get(m => m.Id == dto.DealerId)).FirstOrDefault().Name;

            //}

            #region مصاريف 

            
            else if (dto.Daily)
            {
                past_ = 0;
                List<Daily> models;
                if (dto.DealerId == 0 && dto.SupplierId == 0)
                    return result;
                else if(dto.DealerId == 0 && dto.SupplierId != 0)
                {
                    models = (await _unitOfWork.repository<Daily>().Get(m => (m.SupplierOfFarmsId == dto.SupplierId) &&
                               (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                               (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true)
                               )).OrderBy(m => m.Date).ToList();

                    var models_Past = (await _unitOfWork.repository<Daily>().Get(m => (m.SupplierOfFarmsId == dto.SupplierId) &&
                               (dto.StartDate.HasValue ? !(m.Date > dto.StartDate) : true)
                               )).ToList();

                    decimal sum_Past = 0;
                    sum_Past = models_Past.Sum(m => m.CuttingCostOfAll);
                    past_ = sum_Past;

                }
                else
                {
                    models = (await _unitOfWork.repository<Daily>().Get(m => (m.FarmerId == dto.DealerId) &&
                               (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                               (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true) &&
                               (dto.SupplierId != 0 ? (m.SupplierOfFarmsId == dto.SupplierId) : true)
                               )).OrderBy(m=>m.Date).ToList();


                    if (dto.StartDate.HasValue && dto.SupplierId == 0)
                    {
                        var models_Past = (await _unitOfWork.repository<Daily>().Get(m => (m.FarmerId == dto.DealerId) &&
                               (dto.StartDate.HasValue ? !(m.Date > dto.StartDate) : true) 
                               )).ToList();

                        decimal sum_Past = 0;
                        sum_Past = models_Past.Sum(m => m.BuyPriceOfAll);
                        past_ = sum_Past;
                    }
                }

                

                decimal sum = 0;
                foreach (var model in models)
                {
                    result.DetailsBillingDto.Add(new DetailsBillingDto
                    {
                        Date = model.Date,
                        Count = model.TotalBoxes,
                        BalanceCardWeight = model.BalanceCardWeight,
                        WeightAfterDiscount_2Percent = model.WeightAfterDiscount_2Percent,
                        Statement = model.MaterialName,
                        UnitPrice = model.BuyPriceOfUnit,
                        TotalPrice = model.BuyPriceOfAll,
                        DelearName = model.FarmerName,
                        CuttingCostOfUnit = model.CuttingCostOfUnit,
                        CuttingCostOfAll =model.CuttingCostOfAll
                    });
                    sum += model.BuyPriceOfAll;
                }
                result.Total = sum;
                result.TotalBoxes = result.DetailsBillingDto.Sum(m => m.Count);
                result.TotalBalanceCardWeight = result.DetailsBillingDto.Sum(m => m.BalanceCardWeight);
                result.WeightAfterDiscount_2Percent = result.DetailsBillingDto.Sum(m => m.WeightAfterDiscount_2Percent);
                result.TotalCuttingCostOfAll = result.DetailsBillingDto.Sum(m => m.CuttingCostOfAll);

                if (dto.DealerId == 0 && dto.SupplierId != 0)
                {
                    var FinancialEntitlrment_ = (await _unitOfWork.repository<Daily>().Get(m => (m.SupplierOfFarmsId == dto.SupplierId))).Count() != 0 ?
                                           (await _unitOfWork.repository<Daily>().Get(m => (m.SupplierOfFarmsId == dto.SupplierId))).FirstOrDefault().SupplierOfFarmsFinancialEntitlement : null;

                    result.PaymentsBillingDtos = new List<PaymentsBillingDto>();
                    result.TotalPayments = 0;
                    if (FinancialEntitlrment_ != null)
                    {
                        var SomeFinancialEntitlrment_ = FinancialEntitlrment_.Paymenties.Where(m =>
                             (dto.StartDate.HasValue ? (m.PaymentDate >= dto.StartDate) : true) &&
                                    (dto.EndDate.HasValue ? (m.PaymentDate < dto.EndDate) : true)).OrderBy(m => m.PaymentDate).ToList();
                        // Past payments
                        if (dto.StartDate.HasValue)
                        {
                            var SomeFinancialEntitlrment_Past = FinancialEntitlrment_.Paymenties.Where(m =>
                           !(m.PaymentDate >= dto.StartDate)).ToList();
                            decimal sum_payments_Past = 0;
                            sum_payments_Past = SomeFinancialEntitlrment_Past.Sum(m => m.AmountPayment);
                            result.Remainder_Past = past_ - sum_payments_Past;
                        }

                        decimal sum_payments = 0;
                        foreach (var SingleFinancialEntitlrment in SomeFinancialEntitlrment_)
                        {
                            result.PaymentsBillingDtos.Add(new PaymentsBillingDto
                            {
                                Date = SingleFinancialEntitlrment.PaymentDate,
                                Payment = SingleFinancialEntitlrment.AmountPayment
                            });
                            sum_payments += SingleFinancialEntitlrment.AmountPayment;
                        }
                        result.TotalPayments = sum_payments;
                    }
                    // كامل المبلغ المتبقي
                    result.Remainder = result.TotalCuttingCostOfAll - result.TotalPayments;
                }
                else
                {
                    var FinancialEntitlrment_ = (await _unitOfWork.repository<Daily>().Get(m => (m.FarmerId == dto.DealerId))).Count() != 0 ?
                                            (await _unitOfWork.repository<Daily>().Get(m => (m.FarmerId == dto.DealerId))).FirstOrDefault().FinancialEntitlement : null;

                    result.PaymentsBillingDtos = new List<PaymentsBillingDto>();
                    result.TotalPayments = 0;
                    if (FinancialEntitlrment_ != null)
                    {
                        var SomeFinancialEntitlrment_ = FinancialEntitlrment_.Paymenties.Where(m =>
                             (dto.StartDate.HasValue ? (m.PaymentDate >= dto.StartDate) : true) &&
                                    (dto.EndDate.HasValue ? (m.PaymentDate < dto.EndDate) : true)).OrderBy(m => m.PaymentDate).ToList();
                        // Past payments
                        if (dto.StartDate.HasValue)
                        {
                            var SomeFinancialEntitlrment_Past = FinancialEntitlrment_.Paymenties.Where(m =>
                           !(m.PaymentDate >= dto.StartDate)).ToList();
                            decimal sum_payments_Past = 0;
                            sum_payments_Past = SomeFinancialEntitlrment_Past.Sum(m => m.AmountPayment);
                            result.Remainder_Past = past_ - sum_payments_Past;
                        }

                        decimal sum_payments = 0;
                        foreach (var SingleFinancialEntitlrment in SomeFinancialEntitlrment_)
                        {
                            result.PaymentsBillingDtos.Add(new PaymentsBillingDto
                            {
                                Date = SingleFinancialEntitlrment.PaymentDate,
                                Payment = SingleFinancialEntitlrment.AmountPayment
                            });
                            sum_payments += SingleFinancialEntitlrment.AmountPayment;
                        }
                        result.TotalPayments = sum_payments;
                    }
                    // كامل المبلغ المتبقي
                    result.Remainder = result.Total - result.TotalPayments;
                }

                result.DelearName = models.Any() ? models.FirstOrDefault().FarmerName : dto.DealerName;
                result.SupplierName = models.Any() ? models.FirstOrDefault().SupplierOfFarms.Name : dto.SupplierName;
                result.SupplierId = dto.SupplierId;

            }

            else if (dto.Tabali)
            {
                past_ = 0;
                var models = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId) && (m.RepositoryMaterial.CategoryId==4)&&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();
               
                if (dto.StartDate.HasValue)
                {
                    var models_Past = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId) && (m.RepositoryMaterial.CategoryId == 4) &&
                           (dto.StartDate.HasValue ? !(m.Date > dto.StartDate) : true)
                           )).ToList();

                    decimal sum_Past = 0;
                    sum_Past = models_Past.Sum(m => m.BuyPriceOfAll);
                    past_ = sum_Past;
                }

                decimal sum = 0;
                foreach (var model in models)
                {
                    result.DetailsBillingDto.Add(new DetailsBillingDto
                    {
                        Date = model.Date,
                        Count = model.Amount,
                        Statement = model.Name,
                        UnitPrice = model.BuyPriceOfUnit,
                        TotalPrice = model.BuyPriceOfAll
                    });
                    sum += model.BuyPriceOfAll;
                }
                result.Total = sum;

                var FinancialEntitlrment_ = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId))).Count() != 0 ?
                                            (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId))).FirstOrDefault().FinancialEntitlement : null;

                result.PaymentsBillingDtos = new List<PaymentsBillingDto>();
                result.TotalPayments = 0;
                if (FinancialEntitlrment_ != null)
                {
                    var SomeFinancialEntitlrment_ = FinancialEntitlrment_.Paymenties.Where(m =>
                         (dto.StartDate.HasValue ? (m.PaymentDate >= dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.PaymentDate < dto.EndDate) : true)).OrderBy(m => m.PaymentDate).ToList();

                    // Past payments
                    if (dto.StartDate.HasValue)
                    {
                        var SomeFinancialEntitlrment_Past = FinancialEntitlrment_.Paymenties.Where(m =>
                       !(m.PaymentDate >= dto.StartDate)).ToList();
                        decimal sum_payments_Past = 0;
                        sum_payments_Past = SomeFinancialEntitlrment_Past.Sum(m => m.AmountPayment);
                        result.Remainder_Past = past_ - sum_payments_Past;
                    }

                    decimal sum_payments = 0;
                    foreach (var SingleFinancialEntitlrment in SomeFinancialEntitlrment_)
                    {
                        result.PaymentsBillingDtos.Add(new PaymentsBillingDto
                        {
                            Date = SingleFinancialEntitlrment.PaymentDate,
                            Payment = SingleFinancialEntitlrment.AmountPayment
                        });
                        sum_payments += SingleFinancialEntitlrment.AmountPayment;
                    }
                    result.TotalPayments = sum_payments;
                }
                // كامل المبلغ المتبقي
                result.Remainder = result.Total - result.TotalPayments;


                result.DelearName = models.Any() ? models.FirstOrDefault().SupplierName : dto.DealerName;

            }
            else if (dto.Plastic)
            {
                past_ = 0;
                var models = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId) && (m.RepositoryMaterial.CategoryId == 10) &&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();
               
                if (dto.StartDate.HasValue)
                {
                    var models_Past = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId) && (m.RepositoryMaterial.CategoryId == 10) &&
                           (dto.StartDate.HasValue ? !(m.Date > dto.StartDate) : true)
                           )).ToList();

                    decimal sum_Past = 0;
                    sum_Past = models_Past.Sum(m => m.BuyPriceOfAll);
                    past_ = sum_Past;
                }

                decimal sum = 0;
                foreach (var model in models)
                {
                    result.DetailsBillingDto.Add(new DetailsBillingDto
                    {
                        Date = model.Date,
                        Count = model.Amount,
                        Statement = model.Name,
                        UnitPrice = model.BuyPriceOfUnit,
                        TotalPrice = model.BuyPriceOfAll
                    });
                    sum += model.BuyPriceOfAll;
                }
                result.Total = sum;

                var FinancialEntitlrment_ = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId))).Count() != 0 ?
                                            (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId))).FirstOrDefault().FinancialEntitlement : null;

                result.PaymentsBillingDtos = new List<PaymentsBillingDto>();
                result.TotalPayments = 0;
                if (FinancialEntitlrment_ != null)
                {
                    var SomeFinancialEntitlrment_ = FinancialEntitlrment_.Paymenties.Where(m =>
                         (dto.StartDate.HasValue ? (m.PaymentDate >= dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.PaymentDate < dto.EndDate) : true)).OrderBy(m => m.PaymentDate).ToList();

                    // Past payments
                    if (dto.StartDate.HasValue)
                    {
                        var SomeFinancialEntitlrment_Past = FinancialEntitlrment_.Paymenties.Where(m =>
                       !(m.PaymentDate >= dto.StartDate)).ToList();
                        decimal sum_payments_Past = 0;
                        sum_payments_Past = SomeFinancialEntitlrment_Past.Sum(m => m.AmountPayment);
                        result.Remainder_Past = past_ - sum_payments_Past;
                    }

                    decimal sum_payments = 0;
                    foreach (var SingleFinancialEntitlrment in SomeFinancialEntitlrment_)
                    {
                        result.PaymentsBillingDtos.Add(new PaymentsBillingDto
                        {
                            Date = SingleFinancialEntitlrment.PaymentDate,
                            Payment = SingleFinancialEntitlrment.AmountPayment
                        });
                        sum_payments += SingleFinancialEntitlrment.AmountPayment;
                    }
                    result.TotalPayments = sum_payments;
                }
                // كامل المبلغ المتبقي
                result.Remainder = result.Total - result.TotalPayments;


                result.DelearName = models.Any() ? models.FirstOrDefault().SupplierName : dto.DealerName;

            }
            else if (dto.Karasta)
            {
                past_ = 0;
                var models = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId) && (m.RepositoryMaterial.CategoryId == 3) &&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();
                
                if (dto.StartDate.HasValue)
                {
                    var models_Past = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId) && (m.RepositoryMaterial.CategoryId == 3) &&
                           (dto.StartDate.HasValue ? !(m.Date > dto.StartDate) : true)
                           )).ToList();

                    decimal sum_Past = 0;
                    sum_Past = models_Past.Sum(m => m.BuyPriceOfAll);
                    past_ = sum_Past;
                }

                decimal sum = 0;
                foreach (var model in models)
                {
                    result.DetailsBillingDto.Add(new DetailsBillingDto
                    {
                        Date = model.Date,
                        Count = model.Amount,
                        Statement = model.Name,
                        UnitPrice = model.BuyPriceOfUnit,
                        TotalPrice = model.BuyPriceOfAll
                    });
                    sum += model.BuyPriceOfAll;
                }
                result.Total = sum;

                var FinancialEntitlrment_ = (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId))).Count() != 0 ?
                                            (await _unitOfWork.repository<Repository_InOut>().Get(m => (m.SupplierId == dto.DealerId))).FirstOrDefault().FinancialEntitlement : null;

                result.PaymentsBillingDtos = new List<PaymentsBillingDto>();
                result.TotalPayments = 0;
                if (FinancialEntitlrment_ != null)
                {
                    var SomeFinancialEntitlrment_ = FinancialEntitlrment_.Paymenties.Where(m =>
                         (dto.StartDate.HasValue ? (m.PaymentDate >= dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.PaymentDate < dto.EndDate) : true)).OrderBy(m => m.PaymentDate).ToList();

                    // Past payments
                    if (dto.StartDate.HasValue)
                    {
                        var SomeFinancialEntitlrment_Past = FinancialEntitlrment_.Paymenties.Where(m =>
                       !(m.PaymentDate >= dto.StartDate)).ToList();
                        decimal sum_payments_Past = 0;
                        sum_payments_Past = SomeFinancialEntitlrment_Past.Sum(m => m.AmountPayment);
                        result.Remainder_Past = past_ - sum_payments_Past;
                    }

                    decimal sum_payments = 0;
                    foreach (var SingleFinancialEntitlrment in SomeFinancialEntitlrment_)
                    {
                        result.PaymentsBillingDtos.Add(new PaymentsBillingDto
                        {
                            Date = SingleFinancialEntitlrment.PaymentDate,
                            Payment = SingleFinancialEntitlrment.AmountPayment
                        });
                        sum_payments += SingleFinancialEntitlrment.AmountPayment;
                    }
                    result.TotalPayments = sum_payments;
                }
                // كامل المبلغ المتبقي
                result.Remainder = result.Total - result.TotalPayments;


                result.DelearName = models.Any() ? models.FirstOrDefault().SupplierName : dto.DealerName;

            }
            else if (dto.Cars)
            {
                var models = (await _unitOfWork.repository<Cars>().Get(m => (m.DriverId == dto.DealerId) &&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();
                decimal sum = 0;
                foreach (var model in models)
                {
                    result.DetailsBillingDto.Add(new DetailsBillingDto
                    {
                        Date = model.Date,
                        Count = model.LoadsPerDay,
                        UnitPrice = model.PriceOfOne,
                        TotalPrice = model.TotalPrice
                    });
                    sum += model.TotalPrice;
                }
                result.Total = sum;

                var FinancialEntitlrment_ = (await _unitOfWork.repository<Cars>().Get(m => (m.DriverId == dto.DealerId))).Count() != 0 ?
                                            (await _unitOfWork.repository<Cars>().Get(m => (m.DriverId == dto.DealerId))).FirstOrDefault().FinancialEntitlement : null;

                result.PaymentsBillingDtos = new List<PaymentsBillingDto>();
                result.TotalPayments = 0;
                if (FinancialEntitlrment_ != null)
                {
                    var SomeFinancialEntitlrment_ = FinancialEntitlrment_.Paymenties.Where(m =>
                         (dto.StartDate.HasValue ? (m.PaymentDate > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.PaymentDate < dto.EndDate) : true)).OrderBy(m => m.PaymentDate).ToList();

                    decimal sum_payments = 0;
                    foreach (var SingleFinancialEntitlrment in SomeFinancialEntitlrment_)
                    {
                        result.PaymentsBillingDtos.Add(new PaymentsBillingDto
                        {
                            Date = SingleFinancialEntitlrment.PaymentDate,
                            Payment = SingleFinancialEntitlrment.AmountPayment
                        });
                        sum_payments += SingleFinancialEntitlrment.AmountPayment;
                    }
                    result.TotalPayments = sum_payments;
                }
                // كامل المبلغ المتبقي
                result.Remainder = result.Total - result.TotalPayments;


                result.DelearName = models.Any() ? models.FirstOrDefault().DriverName : dto.DealerName;

            }
            else if (dto.Fuel)
            {
                var models = (await _unitOfWork.repository<Fuel>().Get(m => (m.SourceId == dto.DealerId) &&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();
                decimal sum = 0;
                foreach (var model in models)
                {
                    result.DetailsBillingDto.Add(new DetailsBillingDto
                    {
                        Date = model.Date,
                        Count = model.Amount,
                        Statement = model.Type,
                        UnitPrice = model.PriceOfOne,
                        TotalPrice = model.TotalPrice
                    });
                    sum += model.TotalPrice;
                }
                result.Total = sum;

                var FinancialEntitlrment_ = (await _unitOfWork.repository<Fuel>().Get(m => (m.SourceId == dto.DealerId))).Count() != 0 ?
                                            (await _unitOfWork.repository<Fuel>().Get(m => (m.SourceId == dto.DealerId))).FirstOrDefault().FinancialEntitlement : null;

                result.PaymentsBillingDtos = new List<PaymentsBillingDto>();
                result.TotalPayments = 0;
                if (FinancialEntitlrment_ != null)
                {
                    var SomeFinancialEntitlrment_ = FinancialEntitlrment_.Paymenties.Where(m =>
                         (dto.StartDate.HasValue ? (m.PaymentDate > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.PaymentDate < dto.EndDate) : true)).OrderBy(m => m.PaymentDate).ToList();

                    decimal sum_payments = 0;
                    foreach (var SingleFinancialEntitlrment in SomeFinancialEntitlrment_)
                    {
                        result.PaymentsBillingDtos.Add(new PaymentsBillingDto
                        {
                            Date = SingleFinancialEntitlrment.PaymentDate,
                            Payment = SingleFinancialEntitlrment.AmountPayment
                        });
                        sum_payments += SingleFinancialEntitlrment.AmountPayment;
                    }
                    result.TotalPayments = sum_payments;
                }
                // كامل المبلغ المتبقي
                result.Remainder = result.Total - result.TotalPayments;


                result.DelearName = models.Any() ? models.FirstOrDefault().SourceName : dto.DealerName;

            }
            else if (dto.Employees)
            {
                var models = (await _unitOfWork.repository<DailyChekEmployees>().Get(m => (m.EmployeeId == dto.DealerId) &&
                                (dto.StartDate.HasValue ? (m.Date > dto.StartDate) : true) &&
                                (dto.EndDate.HasValue ? (m.Date < dto.EndDate) : true))).OrderBy(m => m.Date).ToList();

                result.DailyChekEmployeesDtos= _mapper.Map<List<DailyChekEmployees>, List<DailyChekEmployeesDto>>(models);
                result.Total = result.DailyChekEmployeesDtos.Sum(m => m.ResultWage);

                decimal sum_payments = 0;
                result.PaymentsBillingDtos = new List<PaymentsBillingDto>();
                foreach (var SingleCheck in result.DailyChekEmployeesDtos)
                {
                    if (SingleCheck.PaidWage != 0)
                    {
                        result.PaymentsBillingDtos.Add(new PaymentsBillingDto
                        {
                            Date = SingleCheck.Date,
                            Payment = SingleCheck.PaidWage
                        });
                    }
                    sum_payments += SingleCheck.PaidWage;
                }
                result.TotalPayments = sum_payments;
               
              
                // كامل المبلغ المتبقي
                result.Remainder = result.Total - result.TotalPayments;


                result.DelearName = models.Any() ? models.FirstOrDefault().Employee.workshopName : dto.DealerName;

            }

            #endregion




            return result;
           
        }




       
    }
}
