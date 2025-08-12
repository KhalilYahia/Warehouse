
using AutoMapper;
using firstProject.Domain.Entities;
using firstProject.Iservices;
using FirstProject.Domain;
using Services.DTO;
using Services.Iservices;
using System.Data;



namespace Services.services
{
    public class BlogsService:IBlogsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BlogsService(IUnitOfWork _unitOfWork,IMapper _mapper) 
        {
            this._unitOfWork = _unitOfWork;
            this._mapper = _mapper;
        }

        public async Task<int> AddNewBlog(AddBlogDto dto)
        {
            var blog_ = _mapper.Map<Blog>(dto);
            //var Blog_ = new Blog();
            //Blog_.Url = dto.Url;

            _unitOfWork.repository<Blog>().Add(blog_);
            var res= await _unitOfWork.Complete();
            
            return blog_.BlogId;
           

        }

        public List<GetBlogDto> GetAllBlogs()
        {
            throw new NotImplementedException();
        }
    }
}
