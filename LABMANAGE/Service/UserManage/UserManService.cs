﻿using LABMANAGE.Data;
using LABMANAGE.Repository;
using LABMANAGE.Service.Register.Dto;
using LABMANAGE.Service.UserManage.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LABMANAGE.Service.UserManage
{
    public class UserManService : IUserManService
    {
        public IQQInvRepository<User> userManage;
        public UserManService(IQQInvRepository<User> _userManage)
        {
            userManage = _userManage;
        }
        public List<UserManDto> getUserManage(string userName, int pageSize, int curPage, string userRole, bool selectIsTea, int roomID, out long recordCount)
        {
            var query = userManage.Query().Where(m => m.U_Role == 3);
            if (userRole == "R001")
            {
                if (selectIsTea) query = userManage.Query().Where(m => m.U_Role == 2);
                else query = userManage.Query().Where(m => m.U_Role == 3 || m.U_Role == 2);
            }
            if (!String.IsNullOrEmpty(userName))
            {
                query = query.Where(m => m.Name == userName || m.Real_Name == userName || m.Phone == userName || m.Email == userName);
            }
            if (roomID != 0)
            {
                query = query.Where(m => m.Room_ID == roomID);
            }
            recordCount = query.Count();
            query = query.OrderBy(m => m.U_Role).ThenByDescending(m => m.Register_Time).Skip((curPage - 1) * pageSize).Take(pageSize);
            List<UserManDto> adminList = query.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<User, UserManDto>(c));
            return adminList;
        }
        public bool UserCheck(int userId)
        {
            try
            {
                var query = userManage.Query().Where(m => m.ID == userId);
                User userList = query.FirstOrDefault();
                userList.IsExamine = true;
                userManage.Update(userList);
                return true;
            }
            catch { return false; }
        }
        public bool UserDel(int userId)
        {
            try
            {
                var query = userManage.Get(userId);
                userManage.Delete(query);
                return true;
            }
            catch { return false; }
        }
        public bool UpdateRole(RegisterDto UserInfo)
        {
            try
            {
                var query = userManage.Query().Where(m => m.ID == UserInfo.ID);
                User userList = query.FirstOrDefault();
                userList.Name = UserInfo.Name;
                userList.Real_Name = UserInfo.Real_Name;
                userList.Phone = UserInfo.Phone;
                userList.Email = UserInfo.Email;
                userList.Room_ID = UserInfo.Room_ID;
                userManage.Update(userList);
                return true;
            }
            catch { return false; }
        }
    }
}