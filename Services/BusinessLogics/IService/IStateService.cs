using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IStateService
    {
        Task<GlobalRequestReponse<List<GetStateResponse>>> GetStates(string id);
        Task<GlobalRequestReponse<string>> CreateState(CreateStateModel createStateModel);
        Task<GlobalRequestReponse<GetStateResponse>> GetState(string stateId);
        Task<GlobalRequestReponse<string>> DeleteState(string id);
    }
}
