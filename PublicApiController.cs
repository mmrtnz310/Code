using Microsoft.AspNet.Identity.EntityFramework;
using MGM.Web.Core;
using MGM.Web.Domain;
using MGM.Web.Exceptions;
using MGM.Web.Models;
using MGM.Web.Models.Requests;
using MGM.Web.Models.Requests.Public;
using MGM.Web.Models.Requests.Tests;
using MGM.Web.Models.Responses;
using MGM.Web.Services;
using MGM.Web.Services.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MGM.Web.Controllers.Api
{
    [RoutePrefix("api/public")]
    [AllowAnonymous]
    public class PublicApiController : BaseApiController
    {
        IPublicService _publicService = null;
        IUserService _userService = null;
        IMessageService _messageService = null;
        IClientStateService _clientStateService = null;
        ISiteConfig _siteConfig = null;


        public PublicApiController(IPublicService publicService
            , IUserService userService, IMessageService messageService, IClientStateService clientStateService, ISiteConfig siteConfig) : base(clientStateService)
        {
            _publicService = publicService;
            _userService = userService;
            _clientStateService = clientStateService;
            _messageService = messageService;
            _siteConfig = siteConfig;
        }



        [Route("Login"), HttpPost]
        public HttpResponseMessage Login(LoginRequest model)
        {
            // if the Model does not pass validation, there will be an Error response returned with errors
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            BaseResponse baseResponse = null;
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;



            if (_publicService.Login(model))
            {
                string url = null;

                ApplicationUser user = _userService.GetUserBy(model.Username);

                if (_userService.IsInRole(user.Id, UserRoles.SuperAdmin))
                {
                    url = _siteConfig.HomeUrlSuperAdmin;
                }
                else if (_userService.IsInRole(user.Id, UserRoles.Admin))
                {
                    url = _siteConfig.HomeUrlAdmin;
                }

                else if (_userService.IsInRole(user.Id, UserRoles.CaseWorker))
                {
                    url = _siteConfig.HomeUrlCaseWorker;
                }

                else if (_userService.IsInRole(user.Id, UserRoles.DataCollector))
                {
                    url = _siteConfig.HomeUrlDataCollector;
                }

                else if (_userService.IsInRole(user.Id, UserRoles.Client))
                {
                    url = _siteConfig.HomeUrlClient;
                }

                ItemResponse<string> response = new ItemResponse<string>();

                response.Item = url;

                HttpResponseMessage message = Request.CreateResponse(httpStatusCode, response);

                _clientStateService.Delete(message.Headers, ClientStateKey.CurrrentClientId);

                return message;
            }
            else
            {
                baseResponse = new ErrorResponse("Login failed. Incorrect Username or Password.");
                httpStatusCode = HttpStatusCode.Unauthorized;
            }

            return Request.CreateResponse(httpStatusCode, baseResponse);
        }

        //--------------------------------------


        [Route("ResetPasswordRequest"), HttpPost]
        public HttpResponseMessage Forgot(ForgotRequest model)
        {
            // if the Model does not pass validation, there will be an Error response returned with errors
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            BaseResponse baseResponse = null;
            HttpStatusCode httpStatCode = HttpStatusCode.OK;

            ApplicationUser user = _userService.GetUser(model.Email);


            if (user != null)
            {
                baseResponse = new SuccessResponse();
                httpStatCode = HttpStatusCode.OK;
                int accountId = _userService.GetAccountId(user.Id);
                Guid userToken = UserTokenService.Add(model, user.Id, accountId);

                _messageService.SendResetPasswordEmail(user.UserName, user.Email, userToken);
            }
            else
            {
                baseResponse = new ErrorResponse("Request failed. Incorrect Email.");
                httpStatCode = HttpStatusCode.Unauthorized;
            }

            return Request.CreateResponse(httpStatCode, baseResponse);
        }


        [Route("Register/{token:guid}"), HttpPost]
        public HttpResponseMessage AddUser(RegisterRequest model, Guid token)
        {
            BaseResponse baseResponse = null;
            HttpStatusCode httpStatusCode = new HttpStatusCode();

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            try
            {
                UserToken userTokenTableInfo = UserTokenService.Get(token); //check if tken is valid.

                string sUserRole = userTokenTableInfo.Role;     //parse string to enum of "role"
                UserRoles userRole;
                Enum.TryParse(sUserRole, out userRole);

                _userService.RegisterUser(model, userTokenTableInfo.AccountId);               // register user
                ApplicationUser user = _userService.GetUser(model.Email);       //get registered user infomartion.
                _userService.AddToRole(user.Id, userRole);          //assign user to the role he/she is invited.

                httpStatusCode = HttpStatusCode.OK;
                baseResponse = new SuccessResponse();
                ConfirmRequest confirmRequestModel = new ConfirmRequest();
                confirmRequestModel.Name = model.FirstName;
                confirmRequestModel.Email = model.Email;

                UserTokenService.Delete(token);
                _messageService.SendRegisterConfirmEmail(confirmRequestModel, user.Id); //send confirmation email, taking 2 args including email and name.

            }
            catch (IdentityResultException ire)
            {

                IEnumerable<string> errors = ire.Result.Errors;
                httpStatusCode = HttpStatusCode.BadRequest;
                baseResponse = new ErrorResponse(errors);


            }

            catch (Exception e)
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                baseResponse = new ErrorResponse(e.Message);

            }

            return Request.CreateResponse(httpStatusCode, baseResponse);

        }

        //-------------------------------------------------

        [Route("ContactUs"), HttpPost]
        public HttpResponseMessage SendMessage(ContactUsRequest model)
        {

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            SuccessResponse response = new SuccessResponse();

            _messageService.ContactUsRequest(model);

            return Request.CreateResponse(response);

        }

        [Route("confirm/{uid:guid}"), HttpPut]
        public HttpResponseMessage ConfirmEmail(Guid uid)
        {
            SuccessResponse response = new SuccessResponse();

            _publicService.ConfirmEmail(uid);

            return Request.CreateResponse(response);
        }

        [Route("ChangePassword/{token:guid}"), HttpPost]
        public HttpResponseMessage Password(ChangePasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            HttpStatusCode httpStatCode = HttpStatusCode.OK;
            UserToken userToken = UserTokenService.Get(model.Token);

            if (userToken == null)
            {
                //httpStatCode = HttpStatusCode.Unauthorized;
                httpStatCode = HttpStatusCode.NotFound;
                return Request.CreateErrorResponse(httpStatCode, "Request failed. Invalid token");
            }

            bool IsChanged = _userService.ChangePassWord(userToken.UserId, model.ConfirmPassword);

            if (IsChanged)
            {
                UserTokenService.Delete(userToken.TokenId);

                SuccessResponse response = new SuccessResponse();
                return Request.CreateResponse(response);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "There was an error changing the password. Or email was not confirmed");
            }
        }


        [Route("ChangePassword/{uid:guid}"), HttpGet]
        public HttpResponseMessage Get(Guid uid)
        {
            ItemResponse<UserToken> response = new ItemResponse<UserToken>();

            response.Item = UserTokenService.Get(uid);
            return Request.CreateResponse(response);
        }


        [Route("LogOut"), HttpPost]
        public HttpResponseMessage LogOut()
        {
            
            SuccessResponse response = new SuccessResponse();

            HttpResponseMessage message = Request.CreateResponse(HttpStatusCode.OK, response);

            _clientStateService.Delete(message.Headers, ClientStateKey.CurrrentClientId);

            _userService.Logout();
            
            return message;
        }

    }
}
