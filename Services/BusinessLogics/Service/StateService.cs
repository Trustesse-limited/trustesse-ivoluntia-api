using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class StateService: IStateService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public StateService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<GlobalRequestReponse<List<GetStateResponse>>> GetStates(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<List<GetStateResponse>>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
           var states = await _uow.stateRepo.GetListByExpressionAsync(s => s.CountryId == id);
            var mapState = _mapper.Map<List<GetStateResponse>>(states);
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, mapState, true);
        }
        public async Task<GlobalRequestReponse<GetStateResponse>> GetState(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<GetStateResponse>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var states = await _uow.stateRepo.GetByIdAsync(id);
            var stateMap = _mapper.Map<GetStateResponse>(states);   
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, stateMap, true);
        }
        public async Task<GlobalRequestReponse<string>> CreateState(CreateStateModel createStateModel)
        {
            var state = _mapper.Map<State>(createStateModel);
            await _uow.stateRepo.AddAsync(state);
            var response = await _uow.CompleteAsync(); 
            if(response > 0)
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "created", true);
            return ResponseHelper.BuildResponse<string>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
        }
        public async Task<GlobalRequestReponse<string>> DeleteState(string id)
        {
            if(id == null)
                return ResponseHelper.BuildResponse<string>("id cannot be null", StatusCodes.Status400BadRequest, null, true);
            var state = await _uow.stateRepo.GetByIdAsync(id);
            if(state == null)
                return ResponseHelper.BuildResponse<string>("state not found", StatusCodes.Status400BadRequest, null, true);
            await _uow.stateRepo.DeleteAsync(state);
            await _uow.CompleteAsync();
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "state deleted", true);
        }
    }
}
