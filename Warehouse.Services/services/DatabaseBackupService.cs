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
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Warehouse.Domain.Entities;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;
using Warehouse.Model;

namespace Warehouse.Services.services
{
    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DatabaseBackupService(IUnitOfWork unitOfWork, IMapper mapper,ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }


        public async Task<string> ExportDatabaseToJsonAsync()
        {
            return await _unitOfWork.repository<Cars>().ExportDatabaseToJsonAsync();
        }

       

    }
}
