﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace LABMANAGE.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Lab_ManagementEntities : DbContext
    {
        public Lab_ManagementEntities()
            : base("name=Lab_ManagementEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Attendance> Attendance { get; set; }
        public virtual DbSet<Duty> Duty { get; set; }
        public virtual DbSet<Equipment> Equipment { get; set; }
        public virtual DbSet<Leave> Leave { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<Notice> Notice { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<RoleMenu> RoleMenu { get; set; }
        public virtual DbSet<Room> Room { get; set; }
        public virtual DbSet<Summary> Summary { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Week> Week { get; set; }
    }
}