using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sabio.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Sabio.Web.Exceptions;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Data.SqlClient;
using Sabio.Web.Models.Requests;
using Sabio.Web.Core.Services;
using System.Data;
using Sabio.Web.Models.Requests.Users;
using Sabio.Web.Domain;
using Sabio.Web.Domain.Accounts;
using Sabio.Data;
using Sabio.Web.Domain.User;

namespace Sabio.Web.Services
{
    public class UserService : BaseService
        , IUserService
        , ISecurityService
        , IIdentityProvider<string>
        , IAdminUserService
        , ISAUserService
    {
        public bool AddToRole(string userId, UserRoles role)
        {
            ApplicationUserManager userManager = GetUserManager();



            IdentityResult result = userManager.AddToRole(userId, role.ToString("G"));

            if (!result.Succeeded)
            {
                throw new IdentityResultException(result);
            }
            else
            {
                return true;
            }

        }


        public bool IsInRole(UserRoles role)
        {
            return IsInRole(GetCurrentUserId(), role);
        }



        public IList<string> GetRoles()
        {
            string currentUserId = GetCurrentUserId();
            IList<string> roles = GetRoles(currentUserId);
            return roles;
        }

        private ApplicationUserManager GetUserManager()
        {
            return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public bool IsInRole(string userId, UserRoles role)
        {
            ApplicationUserManager userManager = GetUserManager();



            return userManager.IsInRole(userId, role.ToString("G"));
        }

        public IList<string> GetRoles(string userId)
        {
            ApplicationUserManager userManager = GetUserManager();

            return userManager.GetRoles(userId);
        }

        public IdentityUser CreateUser(string email, string userName, string password)
        {
            ApplicationUserManager userManager = GetUserManager();

            ApplicationUser newUser = new ApplicationUser { UserName = userName, Email = email, LockoutEnabled = false };
            IdentityResult result = null;
            try
            {
                result = userManager.Create(newUser, password);

            }
            catch
            {
                new IdentityResultException(result);
            }

            if (result.Succeeded)
            {
                return newUser;
            }
            else
            {
                throw new IdentityResultException(result);
            }
        }

        public bool Signin(string emailaddress, string password, bool remember)
        {
            bool result = false;

            ApplicationUserManager userManager = GetUserManager();
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            ApplicationUser user = userManager.Find(emailaddress, password);
            if (user != null && user.EmailConfirmed)
            {
                ClaimsIdentity signin = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = remember }, signin);
                result = true;

            }
            return result;
        }

        public bool IsUser(string emailaddress)
        {
            bool result = false;

            ApplicationUserManager userManager = GetUserManager();
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            ApplicationUser user = userManager.FindByEmail(emailaddress);


            if (user != null)
            {

                result = true;

            }

            return result;
        }

        public ApplicationUser GetUser(string emailaddress)
        {


            ApplicationUserManager userManager = GetUserManager();
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            ApplicationUser user = userManager.FindByEmail(emailaddress);

            return user;
        }

        public ApplicationUser GetUserById(string userId)
        {

            ApplicationUserManager userManager = GetUserManager();
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            ApplicationUser user = userManager.FindById(userId);

            return user;
        }

        public ApplicationUser GetUserBy(string userName)
        {

            ApplicationUserManager userManager = GetUserManager();
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            ApplicationUser user = userManager.FindByName(userName);

            return user;
        }

        public bool ChangePassWord(string userId, string newPassword)
        {
            bool result = false;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newPassword))
            {
                throw new Exception("You must provide a userId and a password");
            }

            ApplicationUser user = GetUserById(userId);

            if (user != null)
            {

                ApplicationUserManager userManager = GetUserManager();

                userManager.RemovePassword(userId);
                IdentityResult res = userManager.AddPassword(userId, newPassword);

                result = res.Succeeded;

            }

            return result;
        }

        public bool Logout()
        {
            bool result = false;

            IdentityUser user = GetCurrentUser();

            if (user != null)
            {
                IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                result = true;
            }

            return result;
        }

        public IdentityUser GetCurrentUser()
        {
            if (!IsLoggedIn())
                return null;
            ApplicationUserManager userManager = GetUserManager();

            IdentityUser currentUserId = userManager.FindById(GetCurrentUserId());
            return currentUserId;
        }

        public string GetCurrentUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        public bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(GetCurrentUserId());

        }

        public int FinishRegistration(RegisterRequest model, IdentityUser user, int? accountId)
        {
            int id = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.AppUsers_InsertV2"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {


                   paramCollection.AddWithValue("@UserId", user.Id);
                   paramCollection.AddWithValue("@FirstName", model.FirstName);
                   paramCollection.AddWithValue("@LastName", model.LastName);
                   paramCollection.AddWithValue("@PhoneType", model.PhoneType);
                   paramCollection.AddWithValue("@Number", model.Phone);
                   paramCollection.AddWithValue("@Extension", model.Extension);
                   paramCollection.AddWithValue("@accountId", accountId);

               }, returnParameters: delegate (SqlParameterCollection param)
               {
                   //int.TryParse(param["@Id"].Value.ToString(), out id);
               }
               );


            return id;
        }

        public IdentityUser RegisterUser(RegisterRequest model, int? accountId)
        {

            IdentityUser response = null;


            string email = model.Email;
            string username = model.UserName;
            string password = model.Password;

            IdentityUser user = CreateUser(email, username, password);

            FinishRegistration(model, user, accountId);

            //Signin(username, password, true);

            return response;
        }


        public int GetAccountId(string userId)
        {
            int id = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.AppUsers_SelectAccountId"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {


                   paramCollection.AddWithValue("@UserID", userId);

                   SqlParameter p = new SqlParameter("@AccountId", System.Data.SqlDbType.Int);
                   p.Direction = System.Data.ParameterDirection.Output;

                   paramCollection.Add(p);

               }, returnParameters: delegate (SqlParameterCollection param)
               {
                   int.TryParse(param["@AccountId"].Value.ToString(), out id);
               }
               );


            return id;
        }

        public int GetCurrentAccountId()
        {
            return GetAccountId(GetCurrentUserId());
        }


        PagedList<User> IAdminUserService.Get(int pageIndex, int pageSize, int accountId, UserRoles role, string searchTerm)
        {
            List<User> users = null;
            int userTotal = 0;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AppUsers_Admin_SelectAllPaged"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@PageSize", pageSize);
                    paramCollection.AddWithValue("@PageNum", pageIndex);
                    paramCollection.AddWithValue("@AccountId", accountId);
                    paramCollection.AddWithValue("@rolename", role.ToString("G"));
                    paramCollection.AddWithValue("@searchTerm", searchTerm);
                }
                , map: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;

                    userTotal = reader.GetSafeInt32(startingIndex);

                    User u = MapUser(reader);

                    if (users == null)
                    {
                        users = new List<User>();
                    }

                    users.Add(u);
                }
            );

            if (users == null)
            {
                return null;
            }

            PagedList<User> pagedUsers = new PagedList<User>(users, pageIndex, pageSize, userTotal);

            return pagedUsers;
        }

        PagedList<User> ISAUserService.Get(int pageIndex, int pageSize, UserRoles? role, string searchTerm)
        {
            List<User> users = null;
            int userTotal = 0;
            string roleName = role.HasValue ? role.Value.ToString("G") : null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AppUsers_SA_SelectAllPaged"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@PageSize", pageSize);
                    paramCollection.AddWithValue("@PageNum", pageIndex);
                    paramCollection.AddWithValue("@rolename", roleName);
                    paramCollection.AddWithValue("@searchTerm", searchTerm);
                }
                , map: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;

                    userTotal = reader.GetSafeInt32(startingIndex);

                    User u = MapUser(reader);

                    if (users == null)
                    {
                        users = new List<User>();
                    }

                    users.Add(u);
                }
            );

            if (users == null)
            {
                return null;
            }

            PagedList<User> pagedUsers = new PagedList<User>(users, pageIndex, pageSize, userTotal);

            return pagedUsers;
        }

        private void UpdateBasic(UserUpdateRequest model, string procName)
        {
            DataProvider.ExecuteNonQuery(GetConnection, procName
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@UserId", model.UserId);
                    paramCollection.AddWithValue("@FirstName", model.FirstName);
                    paramCollection.AddWithValue("@LastName", model.LastName);
                    paramCollection.AddWithValue("@AccountId", model.AccountId);
                    paramCollection.AddWithValue("@RoleName", model.RoleName);
                });
        }

        public Profile Get(string userId)
        {
            Profile item = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AppUsers_SelectByUserId"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@UserId", userId);
                   //model binding
               }, map: delegate (IDataReader reader, short set)
               {
                   item = new Profile();
                   int startingIndex = 0; //startingOrdinal

                   item.Id = reader.GetSafeInt32(startingIndex++);
                   item.FirstName = reader.GetSafeString(startingIndex++);
                   item.LastName = reader.GetSafeString(startingIndex++);
                   item.LastInteracted = reader.GetSafeDateTimeNullable(startingIndex++);
                   item.Email = reader.GetSafeString(startingIndex++);
                   item.UserName = reader.GetSafeString(startingIndex++);
                   item.UserId = reader.GetSafeString(startingIndex++);
                   item.Phone = reader.GetSafeString(startingIndex++);
                   item.Extension = reader.GetSafeString(startingIndex++);
                   item.PhoneId = reader.GetSafeInt32(startingIndex++);
               }
               );
            return item;
        }

        public void Change(ProfileUpdateRequest model, string userId)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.AppUsers_UpdateV2"
           , inputParamMapper: delegate (SqlParameterCollection paramCollection)
           {
               paramCollection.AddWithValue("@UserId", userId);
               paramCollection.AddWithValue("@FirstName", model.FirstName);
               paramCollection.AddWithValue("@LastName", model.LastName);
               paramCollection.AddWithValue("@UserName", model.UserName);
               paramCollection.AddWithValue("@PhoneId", model.PhoneId);
               paramCollection.AddWithValue("@Number", model.Phone);
               paramCollection.AddWithValue("@Extension", model.Extension);

           });
        }

        #region - Private Mappers -
        private User MapUser(IDataReader reader)
        {
            User u = new User();
            int startingIndex = 1;

            u.LastName = reader.GetSafeString(startingIndex++);
            u.FirstName = reader.GetSafeString(startingIndex++);
            u.UserId = reader.GetSafeString(startingIndex++);
            u.Email = reader.GetSafeString(startingIndex++);
            u.Account = MapAccountBase(reader);
            u.Role = MapUserRole(reader);
            return u;
        }

        private AccountBase MapAccountBase(IDataReader reader, int startingIndex = 5)
        {
            int id = reader.GetSafeInt32(startingIndex++);

            if (id == 0)
            {
                return null;
            }

            AccountBase a = new AccountBase();
            a.Id = id;
            a.Name = reader.GetSafeString(startingIndex++);

            return a;
        }

        private UserRole MapUserRole(IDataReader reader, int startingIndex = 7)
        {
            string id = reader.GetSafeString(startingIndex++);

            if (id == null)
            {
                return null;
            }

            UserRole r = new UserRole();
            r.Id = id;
            r.Name = reader.GetSafeString(startingIndex++);

            return r;
        }


        #endregion

        void IAdminUserService.UpdateBasic(UserUpdateRequest model)
        {
            UpdateBasic(model, "dbo.AppUsers_SA_UpdateBasic");
        }
        void ISAUserService.UpdateBasic(UserUpdateRequest model)
        {
            UpdateBasic(model, "dbo.AppUsers_SA_UpdateBasic");
        }

    }
}