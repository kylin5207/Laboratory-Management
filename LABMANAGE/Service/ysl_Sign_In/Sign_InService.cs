﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop;
using LABMANAGE.Data;
using LABMANAGE.Repository;
using System.Text;
using System.Reflection;
using System.IO;

namespace LABMANAGE.Service.ysl_Sign_In.Dto
{
    public class Sign_InService :ISign_InService
    {

        public IQQInvRepository<Attendance> tableAttendance;
        public Sign_InService(IQQInvRepository<Attendance> _tableAttendance)
        {
            tableAttendance = _tableAttendance;
        }

        

        #region 获取签到表格
        public List<Sign_dateModel> Get_data(int UID, int type)
        {

            var list = tableAttendance.Query().Where(c => c.User_ID == UID && c.Type == type); //?
            List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
            AttendanceDto tt = qqList.FirstOrDefault();
            Attendance ss = AutoMapperHelp.ConvertModel<Attendance, AttendanceDto>(tt);
            int count = qqList.Count();

            List<Sign_dateModel> Sign_dataList = new List<Sign_dateModel>();

            for (int i = 0; i < count; i++ )
            {
                string face = "（ ^ _ ^ ）";
                Sign_dateModel Sign_data = new Sign_dateModel();
                if (qqList[i].State == 1)//签到状态 正常
                {
                    Sign_data.color = "green";
                }
                else if (qqList[i].State == 2)//签到状态迟到
                {
                    Sign_data.color = "bleak";
                    face = "(っ°Д°;)っ";
                }
                else if (qqList[i].State == 3)//签到状态请假
                {
                    Sign_data.color = "red";
                    face = "（ > _ < ）";
                }

                Sign_data.id = qqList[i].Shift;//签到次序

                Sign_data.start = qqList[i].Time.ToString("yyyy-MM-dd HH:mm:ss");//签到时间
                Sign_data.end = qqList[i].Time.ToString("yyyy-MM-dd");

                if (qqList[i].Shift==1)
                {
                    Sign_data.title = face + "早";
                    //Sign_data.start += " 08:30:00";
                    //Sign_data.end += " 11:30:00";
                }
                else if (qqList[i].Shift == 2)
                {
                    Sign_data.title = face + "中";//（ > _ < ）
                    //Sign_data.start += " 14:30:00";
                    //Sign_data.end += " 17:30:00";
                }
                else if (qqList[i].Shift == 3)
                {
                    //∑(っ °Д °;)っ
                    Sign_data.title = face + "晚";
                    //Sign_data.start += " 19:00:00";
                    //Sign_data.end += " 20:30:00";
                }

                
                Sign_dataList.Add(Sign_data);
            }
            
            return Sign_dataList;
        }
        #endregion

        #region List转换成Json
        /// <summary>
        /// List转换成Json
        /// </summary>
        public string Obj2Json<Sign_dateModel>(List<Sign_dateModel> data)
        {
            try
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(data.GetType());
                using (MemoryStream ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, data);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region 判断当前是否签到

        public bool Is_Sign_In(int UID)
        {
            string time = DateTime.Now.TimeOfDay.ToString();
            string day = DateTime.Now.Date.ToString();
            int Shift = time_Shift(time);
            if(Shift>10)
            {
                Shift -= 10;
            }
            else if(Shift==4)
            {
                return true;
            }
            DateTime daydt = Convert.ToDateTime(day);

            string dateday = daydt.ToString("yyyy-MM-dd");
            DateTime flagday = Convert.ToDateTime(dateday);
            DateTime flagday_end = flagday.AddDays(1);
            var list = tableAttendance.Query().Where(c => (c.Time >= flagday) && (c.Time <= flagday_end) && (c.User_ID == UID) && (c.Shift == Shift));
            List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
            int count = qqList.Count();
            if(count>0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 签到部分
        public bool userSign(int UID, string ip)
        {
            //string Uip = (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null) ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString() : HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();//修改IP信息
            string UIP = "";
            if(UIP!=ip)
            {
                return false;
            }
            DateTime time = DateTime.Now;
            Attendance att = new Attendance();
            att.Time = time;
            att.User_ID = UID;
            att.Type = 1;
            att.Shift = time_Shift(time.ToString());
            if(att.Shift<10 && att.Shift!=4)
            {
                att.State = 1;
            }
            else if(att.Shift>10)
            {
                att.State = 2;
                att.Shift = att.Shift - 10;
            }
            string dateday = time.ToString("yyyy-MM-dd");
            DateTime flagday = Convert.ToDateTime(dateday);
            DateTime flagday_end = flagday.AddDays(1);
            var list = tableAttendance.Query().Where(c => (c.Time >= flagday) && (c.Time <= flagday_end) && (c.Type == att.Type) && (c.User_ID == att.User_ID) && (c.Shift == att.Shift));
            List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
            int count = qqList.Count();
            if (count <= 0)
            {
                tableAttendance.Add(att);
            }
            return true;
        }
        #endregion

        #region 添加数据
        public void Ecxel_add(string time, int uid)
        {
            Attendance att = new Attendance();
            att.Shift = time_Shift(time);
            att.User_ID = uid;
            att.Type = 2;
            if (att.Shift < 10 && att.Shift != 4)
            {
                att.State = 1;
            }
            else if (att.Shift > 10)
            {
                att.State = 2;
                att.Shift = att.Shift - 10;
            }
            att.Time = Convert.ToDateTime(time);

            var list = tableAttendance.Query().Where(c => (c.Time >= att.Time) && (c.Type == att.Type) && (c.User_ID == uid));
            List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
            AttendanceDto tt = qqList.FirstOrDefault();
            Attendance ss = AutoMapperHelp.ConvertModel<Attendance, AttendanceDto>(tt);
            int count = qqList.Count();
            if (count <= 0)
            {
                tableAttendance.Add(att);
            }
        }

        #endregion

        #region 签到时间段
        public int time_Shift(string time)//1 8:30~11：30 （11迟到）  2.14:30~17：30 （12迟到） 3.19~20：30 （13迟到）  4不在签到时间内
        {
            string _Day1 = "08:30";
            string Day1 = "11:30";
            string _Day2 = "14:30";
            string Day2 = "17:30";
            string _Day3 = "19:00";
            string Day3 = "20:30";
            TimeSpan _Day1_time = DateTime.Parse(_Day1).TimeOfDay;
            TimeSpan Day1_time = DateTime.Parse(Day1).TimeOfDay;
            TimeSpan _Day2_time = DateTime.Parse(_Day2).TimeOfDay;
            TimeSpan Day2_time = DateTime.Parse(Day2).TimeOfDay;
            TimeSpan _Day3_time = DateTime.Parse(_Day3).TimeOfDay;
            TimeSpan Day3_time = DateTime.Parse(Day3).TimeOfDay;




            DateTime t1 = Convert.ToDateTime(time);

            TimeSpan dspNow = t1.TimeOfDay;
            if (dspNow < _Day1_time)
            {
                return 1;
            }
            else if (dspNow > _Day1_time && dspNow < Day1_time)//迟到
            {
                return 11;
            }
            else if (dspNow > Day1_time && dspNow < _Day2_time)
            {
                return 2;
            }
            else if (dspNow > _Day2_time && dspNow < Day2_time)//迟到
            {
                return 12;
            }
            else if (dspNow > Day2_time && dspNow < _Day3_time)
            {
                return 3;
            }
            else if (dspNow > _Day3_time && dspNow < Day3_time)//迟到
            {
                return 13;
            }
            else
            {
                return 4;
            }
        }
        #endregion

        #region 请假数据

        public void LeaveInsert(DateTime star,DateTime end, int uid)
        {
            Attendance att = new Attendance();
            
            att.User_ID = uid;
            att.Type = 1;

            for (; star.CompareTo(end) < 0; star = star.AddDays(1))
            {
                if (star.AddDays(1).CompareTo(end) >= 0)
                {
                    break;
                }
                string dateday = star.ToString("yyyy-MM-dd");
                DateTime flagday = Convert.ToDateTime(dateday);
                DateTime flagday_end = flagday.AddDays(1);
                string time1 = dateday + " 08:00:00";
                att.Time = Convert.ToDateTime(time1);
                att.Shift = 1;
                att.State = 3;
                if (star.CompareTo(att.Time) <= 0)
                {
                    var list = tableAttendance.Query().Where(c => (c.Time >= flagday) && (c.Time <= flagday_end) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                    List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                    int count = qqList.Count();
                    if (count <= 0)
                    {
                        tableAttendance.Add(att);
                    }
                }

                string time2 = dateday + " 14:00:00";
                att.Time = Convert.ToDateTime(time2);
                att.Shift = 2;
                att.State = 3;
                if (star.CompareTo(att.Time) <= 0)
                {
                    var list = tableAttendance.Query().Where(c => (c.Time >= flagday) && (c.Time <= flagday_end) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                    List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                    int count = qqList.Count();
                    if (count <= 0)
                    {
                        tableAttendance.Add(att);
                    }
                }

                string time3 = dateday + " 19:00:00";
                att.Time = Convert.ToDateTime(time3);
                att.Shift = 3;
                att.State = 3;
                if (star.CompareTo(att.Time) <= 0)
                {
                    var list = tableAttendance.Query().Where(c => (c.Time >= flagday) && (c.Time <= flagday_end) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                    List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                    int count = qqList.Count();
                    if (count <= 0)
                    {
                        tableAttendance.Add(att);
                    }
                }
            }


            string dateday1 = star.ToString("yyyy-MM-dd");
            DateTime flagday1 = Convert.ToDateTime(dateday1);
            DateTime flagday_end1 = flagday1.AddDays(1);
            DateTime time11 = Convert.ToDateTime(dateday1 + " 11:30:00");
            if (time11.CompareTo(end) <= 0 && star.CompareTo(time11)<=0)
            {
                string time1 = dateday1 + " 08:00:00";
                att.Time = Convert.ToDateTime(time1);
                att.Shift = 1;
                att.State = 3;
                var list = tableAttendance.Query().Where(c => (c.Time >= flagday1) && (c.Time <= flagday_end1) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                int count = qqList.Count();
                if (count <= 0)
                {
                    tableAttendance.Add(att);
                }
            }
            DateTime time21 = Convert.ToDateTime(dateday1 + " 17:30:00");
            if (time21.CompareTo(end) <= 0 && star.CompareTo(time21) <= 0)
            {
                string time1 = dateday1 + " 14:00:00";
                att.Time = Convert.ToDateTime(time1);
                att.Shift = 2;
                att.State = 3;
                var list = tableAttendance.Query().Where(c => (c.Time >= flagday1) && (c.Time <= flagday_end1) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                int count = qqList.Count();
                if (count <= 0)
                {
                    tableAttendance.Add(att);
                }
            }
            DateTime time31 = Convert.ToDateTime(dateday1 + " 20:30:00");
            if (time31.CompareTo(end) <= 0 && star.CompareTo(time31) <= 0)
            {
                string time1 = dateday1 + " 19:00:00";
                att.Time = Convert.ToDateTime(time1);
                att.Shift = 3;
                att.State = 3;
                var list = tableAttendance.Query().Where(c => (c.Time >= flagday1) && (c.Time <= flagday_end1) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                int count = qqList.Count();
                if (count <= 0)
                {
                    tableAttendance.Add(att);
                }
            }
            if(int.Parse(dateday1.Split('-')[2])<int.Parse(end.ToString("yyyy-MM-dd").Split('-')[2]))
            {
                string dateday2 = end.ToString("yyyy-MM-dd");
                DateTime flagday2 = Convert.ToDateTime(dateday2);
                DateTime flagday_end2 = flagday2.AddDays(1);
                DateTime time12 = Convert.ToDateTime(dateday2 + " 11:30:00");
                if (time12.CompareTo(end) <= 0 && star.CompareTo(time12) <= 0)
                {
                    string time1 = dateday2 + " 08:00:00";
                    att.Time = Convert.ToDateTime(time1);
                    att.Shift = 1;
                    att.State = 3;
                    var list = tableAttendance.Query().Where(c => (c.Time >= flagday2) && (c.Time <= flagday_end2) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                    List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                    int count = qqList.Count();
                    if (count <= 0)
                    {
                        tableAttendance.Add(att);
                    }
                }
                DateTime time22 = Convert.ToDateTime(dateday2 + " 17:30:00");
                if (time22.CompareTo(end) <= 0 && star.CompareTo(time22) <= 0)
                {
                    string time1 = dateday2 + " 14:00:00";
                    att.Time = Convert.ToDateTime(time1);
                    att.Shift = 2;
                    att.State = 3;
                    var list = tableAttendance.Query().Where(c => (c.Time >= flagday2) && (c.Time <= flagday_end2) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                    List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                    int count = qqList.Count();
                    if (count <= 0)
                    {
                        tableAttendance.Add(att);
                    }
                }
                DateTime time32 = Convert.ToDateTime(dateday2 + " 20:30:00");
                if (time32.CompareTo(end) <= 0 && star.CompareTo(time32) <= 0)
                {
                    string time1 = dateday2 + " 19:00:00";
                    att.Time = Convert.ToDateTime(time1);
                    att.Shift = 3;
                    att.State = 3;
                    var list = tableAttendance.Query().Where(c => (c.Time >= flagday2) && (c.Time <= flagday_end2) && (c.Type == att.Type) && (c.User_ID == uid) && (c.Shift == att.Shift));
                    List<AttendanceDto> qqList = list.ToList().ConvertAll(c => AutoMapperHelp.ConvertToDto<Attendance, AttendanceDto>(c));
                    int count = qqList.Count();
                    if (count <= 0)
                    {
                        tableAttendance.Add(att);
                    }
                }
            }
            
        }

        #endregion
    }
}