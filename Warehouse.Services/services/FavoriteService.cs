using YaznGhanem.Domain;
using YaznGhanem.Services.Iservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using YaznGhanem.Common;
using YaznGhanem.Domain.Entities;
using YaznGhanem.Services.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace YaznGhanem.Services.services
{
    public class FavoriteService : IFavoriteService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public FavoriteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

     
        public async Task<int> AddToFavorite(FavoriteDto dto)
        {
            var model = _mapper.Map<Favorite>(dto);

             _unitOfWork.repository<Favorite>().Add(model);
            await _unitOfWork.Complete();

            return model.Id;
        }

        public async Task<bool> RemoveFromFavorite(int favoriteId, string UserId)
        {
            var models = await _unitOfWork.repository<Favorite>().Get(m => m.Id == favoriteId && m.UserId == UserId);
            if(!models.IsNullOrEmpty() && models.Any())
            {
                var model = models.FirstOrDefault();
                if(model != null) 
                {
                    _unitOfWork.repository<Favorite>().Delete(model);
                    await _unitOfWork.Complete();

                    return true;
                }
            }

            return false;
        }

        public async Task<List<FavoriteDto>> GetFavorites(string UserGuid)
        {
            var models = (await _unitOfWork.repository<Favorite>().Get(m => m.UserId == UserGuid)).ToList();
            var res = _mapper.Map<List<FavoriteDto>>(models);

            return res;
        }
    }
}
