using Microsoft.AspNet.Identity.EntityFramework;
using Sabio.Web.Core;
using Sabio.Web.Domain;
using Sabio.Web.Models;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sabio.Web.Controllers.Api
{

    [RoutePrefix("api/admin")]
    public class UserProfileApiController : BaseApiController
    {
        IUserService _userService = null;
        ISiteConfig _siteConfig = null;

        public UserProfileApiController(IUserService userService, IClientStateService clientStateService
            , ISiteConfig siteConfig) : base(clientStateService)
        {
            _userService = userService;
            _siteConfig = siteConfig;
        }

        [Route("profile"), HttpGet]
        public HttpResponseMessage Get()
        {
            string user = _userService.GetCurrentUserId();

            ItemResponse<Profile> response = new ItemResponse<Profile>();

            response.Item = _userService.Get(user);

            return Request.CreateResponse(response);
        }

        [Route("client/profile"), HttpGet]
        public HttpResponseMessage GetClient()
        {
            string user = ClientStateService.Read(ClientStateKey.CurrrentClientId, Request);

            ItemResponse<Profile> response = new ItemResponse<Profile>();

            response.Item = _userService.Get(user);

            return Request.CreateResponse(response);
        }

        [Route("profile"), HttpPut]
        public HttpResponseMessage Update(ProfileUpdateRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            BaseResponse baseResponse = null;
            HttpStatusCode httpStatCode = HttpStatusCode.OK;
            string currentUser = _userService.GetCurrentUserId();
            ApplicationUser existingUser = _userService.GetUserBy(model.UserName);

            if (existingUser == null)
            {
                _userService.Change(model, currentUser);
                baseResponse = new SuccessResponse();
                httpStatCode = HttpStatusCode.OK;
            }
            else if (existingUser.Id == currentUser)
            {
                _userService.Change(model, currentUser);
                baseResponse = new SuccessResponse();
                httpStatCode = HttpStatusCode.OK;
            }
            else
            {
                baseResponse = new ErrorResponse("Username already exist in the DataBase!");
                httpStatCode = HttpStatusCode.Conflict;
            }
            return Request.CreateResponse(httpStatCode, baseResponse);
        }

        [Route("client/profile"), HttpPut]
        public HttpResponseMessage UpdateClient(ProfileUpdateRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            BaseResponse baseResponse = null;
            HttpStatusCode httpStatCode = HttpStatusCode.OK;
            ApplicationUser existingUser = _userService.GetUserBy(model.UserName);
            string currentClient = ClientStateService.Read(ClientStateKey.CurrrentClientId, Request);

            if (existingUser == null)
            {
                _userService.Change(model, currentClient);
                baseResponse = new SuccessResponse();
                httpStatCode = HttpStatusCode.OK;
            }
            else if (existingUser.Id == currentClient)
            {
                _userService.Change(model, currentClient);
                baseResponse = new SuccessResponse();
                httpStatCode = HttpStatusCode.OK;
            }
            else
            {
                baseResponse = new ErrorResponse("Username already exist in the DataBase!");
                httpStatCode = HttpStatusCode.Conflict;
            }
            return Request.CreateResponse(httpStatCode, baseResponse);
        }
    }
}




