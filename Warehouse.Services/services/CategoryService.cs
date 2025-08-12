using Warehouse.Domain;
using Warehouse.Services.Iservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Warehouse.Common;
using Warehouse.Domain.Entities;
using Warehouse.Services.DTO;
using Microsoft.AspNetCore.Hosting;
using System.Linq.Expressions;

namespace Warehouse.Services.services
{
    public class CategoryService : ICategoryService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _host;
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IHostingEnvironment host)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
        }

        public int Add(InputCategoryDto dto)
        {
            Category entity = _mapper.Map<InputCategoryDto, Category>(dto);
            this._unitOfWork.repository<Category>().Add(entity);
            this._unitOfWork.Complete();
            return entity.Id;
        }

        public bool Edit(InputCategoryDto dto)
        {
            Category entity = _mapper.Map<InputCategoryDto, Category>(dto);
            try
            {
                this._unitOfWork.repository<Category>().Update(entity);
                this._unitOfWork.Complete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool?> Delete(int id)
        {
            if (id <= 7)
                return new bool?(false);
            List<Category> by = (await this._unitOfWork.repository<Category>().Get((Expression<Func<Category, bool>>)(m => m.Id == id))).ToList();
            if (!by.Any<Category>())
                return new bool?(false);

            if (by.FirstOrDefault().RepositoryMaterials.Any())
                return new bool?(false);
            this._unitOfWork.repository<Category>().Delete(by.FirstOrDefault<Category>());
            this._unitOfWork.Complete();
            return new bool?(true);
        }


        public async Task<List<CategoryDto>> GetCategories()
        {
            var categories = await this._unitOfWork.repository<Category>().GetAllAsync();
            return _mapper.Map<List<Category>, List<CategoryDto>>(categories.ToList());
        }



    }
}
