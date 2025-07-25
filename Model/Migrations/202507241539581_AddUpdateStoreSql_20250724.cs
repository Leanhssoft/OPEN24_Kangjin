namespace Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUpdateStoreSql_20250724 : DbMigration
    {
        public override void Up()
        {
            Sql(@"DROP PROCEDURE IF EXISTS [dbo].[BaoCaoGoiDV_GetCTMua_v2]");
            Sql(@"CREATE PROCEDURE [dbo].[BaoCaoGoiDV_GetCTMua_v2]
                @IDChiNhanhs [nvarchar](max),
                @DateFrom [datetime],
                @DateTo [datetime]
            AS
            BEGIN
                SET NOCOUNT ON;
    
    	            declare @tblChiNhanh table( ID_DonVi uniqueidentifier)
    	            insert into @tblChiNhanh
    	            select name from dbo.splitstring(@IDChiNhanhs)
    
    	            ---- get gdvmua
    	            select 
    		            hd.MaHoaDon,
    		            hd.NgayLapHoaDon,
    		            hd.NgayApDungGoiDV,
    		            hd.HanSuDungGoiDV,
    		            hd.ID_DonVi,
    		            hd.ID_DoiTuong,
    		            ct.ID,
    		            ct.ID_HoaDon,
    		            ct.ID_DonViQuiDoi,
    		            ct.ID_LoHang,
    		            ct.SoLuong,
    		            ct.DonGia,
    		            ct.TienChietKhau,
    		            ct.ThanhTien,
			            IIF(ct.SoLuong = 0, 0, ct.ThanhTien / ct.SoLuong) as DonGiaSauCK, 
    		            Case when hd.TongTienHang = 0 
    		            then 0 else ct.ThanhTien * ((hd.TongGiamGia + hd.KhuyeMai_GiamGia) / iif(hd.TongTienHang=0,1, hd.TongTienHang)) end as GiamGiaHD
    	            from BH_HoaDon hd
    	            join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
    	            where hd.LoaiHoaDon = 19
    	            and hd.ChoThanhToan=0
    	            and exists (select cn.ID_DonVi from @tblChiNhanh cn where cn.ID_DonVi= hd.ID_DonVi)
    	            and hd.NgayLapHoaDon between @DateFrom and @DateTo
    	            and (ct.ID_ChiTietDinhLuong is null or ct.ID_ChiTietDinhLuong= ct.ID)
    	            and (ct.ID_ParentCombo is null or ct.ID_ParentCombo!= ct.ID)
            END");


            Sql(@"ALTER PROCEDURE [dbo].[BaoCaoDichVu_NhatKySuDungTongHop]
				@Text_Search [nvarchar](max),    
				@timeStart [datetime],
				@timeEnd [datetime],
				@ID_ChiNhanh [nvarchar](max),
				@LaHangHoa [nvarchar](max),
				@TheoDoi [nvarchar](max),
				@TrangThai [nvarchar](max),
				@ThoiHan [nvarchar](max),
				@ID_NhomHang UNIQUEIDENTIFIER
			AS
			BEGIN
				SET NOCOUNT ON;
				DECLARE @tblSearchString TABLE (Name [nvarchar](max));
				DECLARE @count int;
				INSERT INTO @tblSearchString(Name) select  Name from [dbo].[splitstringByChar](@Text_Search, ' ') where Name!='';
				Select @count =  (Select count(*) from @tblSearchString);

				declare @dtNow datetime = getdate()

				declare @tblCTMua table(
					MaHoaDon nvarchar(max),
					NgayLapHoaDon datetime,
					NgayApDungGoiDV datetime,
					HanSuDungGoiDV datetime,
					ID_DonVi uniqueidentifier,
					ID_DoiTuong uniqueidentifier,
					ID uniqueidentifier,
					ID_HoaDon uniqueidentifier,
					ID_DonViQuiDoi uniqueidentifier,
					ID_LoHang uniqueidentifier,
					SoLuong float,
					DonGia float,
					TienChietKhau float,
					ThanhTien float,
					DonGiaSauCK float,
					GiamGiaHD float)
				insert into @tblCTMua
				exec BaoCaoGoiDV_GetCTMua_v2 @ID_ChiNhanh,@timeStart,@timeEnd

						select 
							b.MaHangHoa, 
							b.TenHangHoa, 
							b.MaLoHang as TenLoHang,
							b.ThuocTinhGiaTri as ThuocTinh_GiaTri,
							CONCAT(b.TenHangHoa, b.ThuocTinhGiaTri) as TenHangHoaFull,
							b.TenDonViTinh,
							b.TenNhomHang,				
							b.MaDoiTuong as MaKhachHang,
							b.TenDoiTuong as TenKhachHang,
							b.DienThoai, 
							b.GioiTinh, 
							b.TenNguonKhach, 
							b.NhomKhachHang,
							b.NguoiGioiThieu,
							sum(SoLuong) as SoLuongMua,
							sum(SoLuongTra) as SoLuongTra,
							sum(SoLuongSuDung) as SoLuongSuDung,
							sum(GiaTriSD) as GiaTriSD,
							round(sum(SoLuong) - sum(SoLuongTra) -  sum(SoLuongSuDung),2) as SoLuongConLai,
							sum(GiaTriConLai) as GiaTriConLai
							from
							(
			
								select 
								ctm.id,
									ctm.ID_HoaDon,
									ctm.MaHoaDon,
									ctm.NgayLapHoaDon,
									ctm.NgayApDungGoiDV,
									ctm.HanSuDungGoiDV,
									dt.MaDoiTuong,
									dt.TenDoiTuong,
									dt.DienThoai,
									Case when dt.GioiTinhNam = 1 then N'Nam' else N'Nữ' end as GioiTinh,
									gt.TenDoiTuong as NguoiGioiThieu,
									nk.TenNguonKhach,
									isnull(dt.TenNhomDoiTuongs, N'Nhóm mặc định') as NhomKhachHang ,
									iif( hh.ID_NhomHang is null, '00000000-0000-0000-0000-000000000000',hh.ID_NhomHang) as ID_NhomHang,
									iif(@dtNow <=ctm.HanSuDungGoiDV,1,0) as ThoiHan,						
									ctm.SoLuong,
									ctm.DonGia,
									ctm.TienChietKhau,
									ctm.ThanhTien,
									ctm.DonGiaSauCK,
									isnull(tbl.SoLuongTra,0) as SoLuongTra,
									isnull(tbl.GiaTriTra,0) as GiaTriTra,
									isnull(tbl.SoLuongSuDung,0) as SoLuongSuDung,
									CASE WHEN ctm.ThanhTien = 0 THEN 0 
									ELSE isnull(tbl.GiaTriSD,0) 
									END as GiaTriSD,
									ctm.SoLuong- isnull(tbl.SoLuongTra,0) - isnull(tbl.SoLuongSuDung,0)  as SoLuongConLai,
									CASE WHEN ctm.ThanhTien = 0 THEN 0 
										 ELSE (ctm.ThanhTien - isnull(tbl.GiaTriTra,0) - isnull(tbl.GiaTriSD,0)) 
									END as GiaTriConLai,
									qd.MaHangHoa,
									qd.TenDonViTinh,
									hh.TenHangHoa,
									qd.ThuocTinhGiaTri,
									lo.MaLoHang,
									nhom.TenNhomHangHoa as TenNhomHang
								from @tblCTMua ctm
								inner join DonViQuiDoi qd on ctm.ID_DonViQuiDoi = qd.ID
								inner join DM_HangHoa hh on qd.ID_HangHoa = hh.ID
								left join DM_LoHang lo on ctm.ID_LoHang= lo.ID
								left join DM_NhomHangHoa nhom on hh.ID_NhomHang= nhom.ID
								left join DM_DoiTuong dt on ctm.ID_DoiTuong = dt.ID
								left join DM_DoiTuong gt on dt.ID_NguoiGioiThieu = gt.ID
								left join DM_NguonKhachHang nk on dt.ID_NguonKhach = nk.ID		
					
								left join (
									select 
										tblSD.ID_ChiTietGoiDV,
										sum(tblSD.SoLuongTra) as SoLuongTra,
										sum(tblSD.GiaTriTra) as GiaTriTra,
										sum(tblSD.SoLuongSuDung) as SoLuongSuDung,
										sum(tblSD.GiaTriSD) as GiaTriSD,
										sum(tblSD.GiaVon) as GiaVon
									from 
									(
										---- hdsudung
										Select 								
											ct.ID_ChiTietGoiDV,														
											0 as SoLuongTra,
											0 as GiaTriTra,
											ct.SoLuong as SoLuongSuDung,
											ct.SoLuong * ctm.DonGiaSauCK as GiaTriSD,
											ct.SoLuong * ct.GiaVon as GiaVon
										FROM BH_HoaDon hd
										join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
										join @tblCTMua ctm on ct.ID_ChiTietGoiDV = ctm.ID
										where hd.ChoThanhToan= 0
										and hd.LoaiHoaDon in (1,25)
										and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
							

										union all
										--- hdtra
										Select 							
											ct.ID_ChiTietGoiDV,															
											ct.SoLuong as SoLuongTra,
											ct.ThanhTien as GiaTriTra,
											0 as SoLuongSuDung,
											0 as GiaTriSD,
											0 as GiaVon
										FROM BH_HoaDon hd
										join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
										join @tblCTMua ctm on ct.ID_ChiTietGoiDV = ctm.ID
										where hd.ChoThanhToan= 0
										and hd.LoaiHoaDon = 6
										and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)							
										)tblSD group by tblSD.ID_ChiTietGoiDV

								) tbl on ctm.ID= tbl.ID_ChiTietGoiDV
							where 
							--hh.LaHangHoa like @LaHangHoa
				--			and
							hh.TheoDoi like @TheoDoi
    						and qd.Xoa like @TrangThai
							and (
								@ID_NhomHang is null 
								or exists (select ID from dbo.GetListNhomHangHoa(@ID_NhomHang) nhomS where nhom.ID= nhomS.ID)
							)
							AND ((select count(Name) from @tblSearchString b where 
								ctm.MaHoaDon like '%'+b.Name+'%'
    							or hh.TenHangHoa like '%'+b.Name+'%'
    							or qd.MaHangHoa like '%'+b.Name+'%'
    							or hh.TenHangHoa_KhongDau like '%'+b.Name+'%'
    							or hh.TenHangHoa_KyTuDau like '%'+b.Name+'%'
								or dt.DienThoai like '%'+b.Name+'%'
    							or dt.MaDoiTuong like '%'+b.Name+'%'
    							or dt.TenDoituong like '%'+b.Name+'%'
								or dt.TenDoiTuong_KhongDau like '%'+b.Name+'%'
								or dt.TenDoiTuong_ChuCaiDau like '%'+b.Name+'%'
								)=@count or @count=0)
						) b where b.ThoiHan like @ThoiHan
							group by b.MaHangHoa, b.DonGiaSauCK,                -- thêm vào group by
						   --b.ID_HoaDon, b.id,
							b.TenHangHoa, b.ThuocTinhGiaTri,b.TenDonViTinh, b.MaLoHang, b.TenNhomHang,				
							b.MaDoiTuong, b.TenDoiTuong, b.DienThoai, b.GioiTinh, b.TenNguonKhach, b.NhomKhachHang, b.NguoiGioiThieu
			END");
        }
        
        public override void Down()
        {
            Sql(@"DROP PROCEDURE IF EXISTS [dbo].[GetMaDoiTuongMax_byTemp]");
        }
    }
}
