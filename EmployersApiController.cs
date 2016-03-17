using Sabio.Web.Domain.Employers;
using Sabio.Web.Models;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sabio.Web.Controllers.Api.Employers
{
    [RoutePrefix("api/Employers")]
    public class EmployersApiController : ApiController
    {
        IUserService _userService = null;
        IEmployerService _employerService = null;

        public EmployersApiController(IUserService userService, IEmployerService employerService)
        {
            _employerService = employerService;
            _userService = userService;
        }

        [Route, HttpPost]
        public HttpResponseMessage AddPerson(EmployerAddRequest model)
        {
            // if the Model does not pass validation, there will be an Error response returned with errors
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<int> response = new ItemResponse<int>();
            response.Item = _employerService.InsertEmployer(model);

            return Request.CreateResponse(response);
        }

        [Route("{id:int}"), HttpPut]
        public HttpResponseMessage Update(EmployerUpdateRequest model, int Id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            SuccessResponse response = new SuccessResponse();
            _employerService.Update(model);
            return Request.CreateResponse(response);
        }

        [Route, HttpGet]
        public HttpResponseMessage GetEmployers()
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemsResponse<Employer> response = new ItemsResponse<Employer>();
            response.Items = _employerService.GetEmployersList();

            return Request.CreateResponse(response);
        }

        [Route("{id:int}"), HttpGet]
        public HttpResponseMessage GetEmployer(int Id)
        {
            ItemResponse<Employer> response = new ItemResponse<Employer>();
            response.Item = _employerService.Get(Id);

            return Request.CreateResponse(response);
        }

        [Route("{id:int}"), HttpDelete]
        public HttpResponseMessage Delete(int Id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            SuccessResponse response = new SuccessResponse();
            EmployerDelete model = new EmployerDelete();
            model.Id = Id;
            
            _employerService.Delete(model);

            return Request.CreateResponse(response);
        }
    }
}