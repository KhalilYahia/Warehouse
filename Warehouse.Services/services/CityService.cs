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

namespace YaznGhanem.Services.services
{
    public class CityService : ICityService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CityService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region InputCityDto
        public async Task<int> Add(LanguageHelper Language, InputCityDto dto)
        {
            var model = _mapper.Map<InputCityDto, City>(dto);
            model.CityDescription = new List<CityDescription>();
            CityDescription ACdes = new CityDescription();
            ACdes.CityId = model.Id;
            ACdes.LanguageId = (int)LanguageHelper.ARABIC;
            ACdes.CityName = dto.ArabicCityName;
            model.CityDescription.Add(ACdes);

            CityDescription ECdes = new CityDescription();
            ECdes.LanguageId = (int)LanguageHelper.ENGLISH;
            ECdes.CityName = dto.EnglishCityName;
            model.CityDescription.Add(ECdes);

            _unitOfWork.repository<City>().Add(model);
            await _unitOfWork.Complete();

            return model.Id;
        }

        public async Task<bool> Edit(LanguageHelper Language, InputCityDto dto)
        {
            City c =await _unitOfWork.repository<City>().GetByIdAsync(dto.Id);
            if (c.CityDescription == null)
                c.CityDescription = new List<CityDescription>();
            c.Sort = dto.Sort;
            foreach (var Cdes in c.CityDescription)
            {
                if (Cdes.LanguageId == (int)LanguageHelper.ARABIC)
                {
                    Cdes.CityName = dto.ArabicCityName;
                }
                else if (Cdes.LanguageId == (int)LanguageHelper.ENGLISH)
                {
                    Cdes.CityName = dto.EnglishCityName;
                }
            }

            _unitOfWork.repository<City>().Update(c);
            await _unitOfWork.Complete();

            return true;
        }

        public async Task<bool?> Delete(int Id)
        {
            var c1 = await _unitOfWork.repository<City>().Get(m => m.Id == Id);
            if (!c1.Any())
            {
                return null;
            }
            else
            {
                //if (c1.Single().Towns.Any())
                //    return false;

                _unitOfWork.repository<City>().Delete(c1.Single());
                await _unitOfWork.Complete();
                return true;
            }
        }

        #endregion

        #region CityDto
        public async Task<List<CityDto>> GetAllCities(LanguageHelper language)
        {
            var model = ( await _unitOfWork.repository<City>().GetAllAsync()).OrderBy(m => m.Sort).ToList();

            var modelDto = _mapper.Map<List<City>, List<CityDto>>(model);
            int index_m = 0;
            foreach (var city in model)
            {
                //modelDto[index_m].TownsDto = new List<TownDto>();
                foreach (var CDes in city.CityDescription)
                {
                    if (CDes.LanguageId == (int)language)
                    {
                        modelDto[index_m].CityName = CDes.CityName;
                    }

                }
                //int index_m2 = 0;
                //foreach (var Towns in city.Towns)
                //{
                //    TownDto towndto = new TownDto();
                //    towndto.CityId = Towns.CityId;
                //    towndto.Id = Towns.Id;
                //    foreach (var TownDes in Towns.TownDescriptions)
                //    {
                //        if (TownDes.LanguageId == (int)language)
                //        {

                //            //towndto.Gps_Latitude = Towns.Gps_Latitude;
                //            //towndto.Gps_Longitude = Towns.Gps_Longitude;
                //            towndto.TownName = TownDes.TownName;
                //            modelDto[index_m].TownsDto.Add(towndto);///////////////

                //            // modelDto[index_m].TownsDto[index_m2].TownName = TownDes.TownName;
                //        }


                //    }
                   
                //}
                index_m++;
            }
            return modelDto;
        }

        public async Task<CityDto> GetCityById(LanguageHelper language, int id)
        {
            var model1 = await _unitOfWork.repository<City>().Get(m => m.Id == id);

            if (model1.Any())
            {
                var model = model1.FirstOrDefault();
                var modelDto = _mapper.Map<City, CityDto>(model);
                if (modelDto != null)
                {
                    foreach (var CDes in model.CityDescription)
                    {
                        if (CDes.LanguageId == (int)language)
                        {
                            modelDto.CityName = CDes.CityName;
                        }

                    }
                    //modelDto.TownsDto = new List<TownDto>();

                    //foreach (var Towns in model.Towns)
                    //{
                    //    TownDto towndto = new TownDto();
                    //    towndto.CityId = Towns.CityId;
                    //    towndto.Id = Towns.Id;
                    //    foreach (var TownDes in Towns.TownDescriptions)
                    //    {
                    //        if (TownDes.LanguageId == (int)language)
                    //        {
                    //            towndto.TownName = TownDes.TownName;
                    //            modelDto.TownsDto.Add(towndto);

                    //            // modelDto[index_m].TownsDto[index_m2].TownName = TownDes.TownName;
                    //        }

                    //    }

                    //}
                }

                return modelDto;
            }
            else
                return null;

        }
        #endregion


        #region Validator

        public async Task< bool> IsNameUnique(string name, int? id)
        {
            if (name == null)
                return true;
            else
            {
                List<CityDescription> model;

                if (id.HasValue)
                    model = (await _unitOfWork.repository<CityDescription>().Get(m => m.CityId != id)).ToList();
                else
                    model = (await _unitOfWork.repository<CityDescription>().GetAllAsync()).ToList();
                return !model.Where(s => s.CityName.ToLower() == name.ToLower()).Any();

            }
        }

        public async Task<bool> IsExistId(int id)
        {
            return (await _unitOfWork.repository<City>().Get(m => m.Id == id)).Any();
        }

        #endregion
    }
}
