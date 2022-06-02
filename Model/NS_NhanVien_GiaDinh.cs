﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [Table("NS_NhanVien_GiaDinh")]
    public partial class NS_NhanVien_GiaDinh
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid ID { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ID_NhanVien { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string HoTen { get; set; } = string.Empty;

        [Column(TypeName = "int")]
        public int? NgaySinh { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string NoiO { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string QuanHe { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string DiaChi { get; set; } = string.Empty;

        [Column(TypeName = "int")]
        public int TrangThai { get; set; }

        public virtual NS_NhanVien NS_NhanVien { get; set; }
    }
}
