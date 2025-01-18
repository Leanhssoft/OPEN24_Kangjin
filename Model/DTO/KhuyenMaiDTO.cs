using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class DMKhuyenMaiDTO
    {
        public Guid ID { get; set; }

        public string MaKhuyenMai { get; set; } = string.Empty;

        public string TenKhuyenMai { get; set; } = string.Empty;

        public string GhiChu { get; set; } = string.Empty;

        public bool TrangThai { get; set; }// 1. Kichhoat, 2.Chua apdung

        public int HinhThuc { get; set; } // hình thức khuyến mại (21.HH - giam HH, 22.HH - Tang Hang, 23. HH - TangDiem, 24. HH - GiaBan theo SL Mua)

        public int LoaiKhuyenMai { get; set; } // Khuyến mại theo

        public DateTime ThoiGianBatDau { get; set; }

        public DateTime ThoiGianKetThuc { get; set; }

        public string NgayApDung { get; set; } = string.Empty;

        public string ThangApDung { get; set; } = string.Empty;

        public string ThuApDung { get; set; } = string.Empty;

        public string GioApDung { get; set; } = string.Empty;

        public int ApDungNgaySinhNhat { get; set; } // 0 - ko set, 1 - ngày, 2 - tuần, 3 - tháng

        public bool TatCaDonVi { get; set; }

        public bool TatCaDoiTuong { get; set; }

        public bool TatCaNhanVien { get; set; }

        public string NguoiTao { get; set; } = string.Empty;
    }
}
