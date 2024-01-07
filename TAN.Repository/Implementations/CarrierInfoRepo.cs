using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class CarrierInfoRepo : Repository<CarrierInfomation>, ICarrierInformation
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public CarrierInfoRepo(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }

        public async Task<string> AddCarrier(CarrierInfomationViewModel carrier)
        {
            var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var newCarrier = new CarrierInfomation()
                {
                    CarrierName = carrier.CarrierName,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsActive = true,
                };

                context.CarrierInfomation.Add(newCarrier);
                await context.SaveChangesAsync();

                return "success";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Add New Carrier  :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public IEnumerable<CarrierInfomationViewModel> GetAllCarriers(int skip, int take, string? searchValue, string sortOrder, out int carrierCount)
        {
            try
            {
                var lowercaseSearchValue = searchValue?.ToLower();
                var carriers = (from c in context.CarrierInfomation
                                where c.IsActive
                                orderby c.Id
                                select new CarrierInfomationViewModel
                                {
                                    CarrierId = c.Id,
                                    CarrierName = c.CarrierName,
                                    CreatedDate = c.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                                }).AsEnumerable();

                if (!string.IsNullOrEmpty(lowercaseSearchValue))
                {
                    carriers = ApplySearch(carriers, lowercaseSearchValue);
                }

                carrierCount = carriers.Count();

                carriers = carriers.Skip(skip).Take(take);

                //sort data
                if (!string.IsNullOrEmpty(sortOrder))
                {
                    switch (sortOrder)
                    {
                        case "carriername_asc":
                            carriers = carriers.OrderBy(x => x.CarrierName);
                            break;
                        case "carriername_desc":
                            carriers = carriers.OrderByDescending(x => x.CarrierName);
                            break;
                        case "createddate_asc":
                            carriers = carriers.OrderBy(x => x.CreatedDate);
                            break;
                        case "createddate_desc":
                            carriers = carriers.OrderByDescending(x => x.CreatedDate);
                            break;
                    }
                }
                return carriers;
            }
            catch (Exception ex)
            {
                carrierCount = 0;
                Log.Error(ex, "An Error Occured in Data Service Controller / Viewing All Carriers :{ErrorMsg}", ex.Message);
                return Enumerable.Empty<CarrierInfomationViewModel>();
            }
        }

        private IEnumerable<CarrierInfomationViewModel> ApplySearch(IEnumerable<CarrierInfomationViewModel> carriers, string lowercaseSearchValue)
        {
            carriers = carriers.Where(c =>
                                         c.CarrierName.ToLower().Contains(lowercaseSearchValue) ||
                                         c.CreatedDate.ToString().Contains(lowercaseSearchValue));
            return carriers;
        }

        public async Task<bool> GetCarrierName(string carrierName, int carrierId)
        {
            try
            {
                var result = await context.CarrierInfomation.Where(c => c.CarrierName == carrierName).FirstOrDefaultAsync();
                if (result != null)
                {
                    if (result.Id == carrierId)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Getting Carrier Name :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<CarrierInfomationViewModel> GetCarrierDetail(int carrierId)
        {
            try
            {
                var carrierDetails = await context.CarrierInfomation.Where(c => c.Id == carrierId).FirstOrDefaultAsync();
                CarrierInfomationViewModel carrierViewModel = new CarrierInfomationViewModel()
                {
                    CarrierId = carrierDetails.Id,
                    CarrierName = carrierDetails.CarrierName
                };

                return carrierViewModel;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Getting Carrier Details :{ErrorMsg}", ex.Message);
                return new CarrierInfomationViewModel();
            }
        }

        public async Task<string> UpdateCarrier(CarrierInfomationViewModel carrier)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingCarrier = await context.CarrierInfomation.Where(c => c.Id == carrier.CarrierId).FirstOrDefaultAsync();

                if (existingCarrier != null)
                {
                    existingCarrier.CarrierName = carrier.CarrierName;
                    existingCarrier.UpdatedDate = DateTime.UtcNow;
                    existingCarrier.UpdatedBy = userId;
                    existingCarrier.IsActive = true;
                    context.CarrierInfomation.Update(existingCarrier);
                    await context.SaveChangesAsync();

                    return "success";
                }
                else
                {
                    return "failed";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Updating Existing Carrier :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<string> DeleteCarrierDetail(int carrierId)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var carrierDetails = context.CarrierInfomation.Where(c => c.Id == carrierId).FirstOrDefault();
                if (carrierDetails != null)
                {
                    carrierDetails.UpdatedBy = userId;
                    carrierDetails.UpdatedDate = DateTime.UtcNow;
                    carrierDetails.IsActive = false;
                    context.CarrierInfomation.Update(carrierDetails);
                    await context.SaveChangesAsync();
                    return "success";
                }
                else
                {
                    return "failed";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Delete Existing Carrier :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }
    }
}
