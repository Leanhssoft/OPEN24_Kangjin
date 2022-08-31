﻿namespace Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUpdateSP_20211129 : DbMigration
    {
        public override void Up()
        {
			Sql(@"ALTER FUNCTION [dbo].[GetGiaVonOfDichVu]
(
	@ID_DonVi uniqueidentifier,
	@ID_DichVu uniqueidentifier	
)
RETURNS float
AS
BEGIN
	
	DECLARE @SumGiaVon float = 0

	declare @Level1_IDDichVu uniqueidentifier,@Level1_IDLoHang uniqueidentifier, @LaHangHoa bit, @MaHangHoa nvarchar(max), @SoLuong float
	declare _cur1 cursor for
    select 
		dl.ID_DonViQuiDoi,
		dl.ID_LoHang,
		hh.LaHangHoa,
		qd.MaHangHoa,
		dl.SoLuong
	from DinhLuongDichVu dl
	join DonViQuiDoi qd on dl.ID_DonViQuiDoi = qd.ID
	join DM_HangHoa hh on qd.ID_HangHoa = hh.ID
	where dl.ID_DichVu= @ID_DichVu
	and dl.ID_DonViQuiDoi !=@ID_DichVu -- tránh trường hợp thành phần dịch vụ là chính nó
	
	open _cur1
	fetch next from _cur1 into @Level1_IDDichVu,@Level1_IDLoHang, @LaHangHoa, @MaHangHoa, @SoLuong
	while @@FETCH_STATUS =0
	begin
		if @LaHangHoa='0'
			begin		
				set @SumGiaVon += @SoLuong *  (select dbo.GetGiaVonOfDichVu(@ID_DonVi, @Level1_IDDichVu))				
			end
		else
			begin				
				set @sumGiaVon += @SoLuong 
					*  isnull((select top 1 GiaVon from DM_GiaVon 
						where ID_DonVi= @ID_DonVi and ID_DonViQuiDoi = @Level1_IDDichVu 
						and (ID_LoHang= @Level1_IDLoHang or (ID_LoHang is null and @Level1_IDLoHang is null))),0)	--- if tpdinhluong la lohang
			end	
		fetch next from _cur1 into @Level1_IDDichVu,@Level1_IDLoHang, @LaHangHoa,@MaHangHoa, @SoLuong
	end
	close _cur1
	deallocate _cur1
	RETURN @SumGiaVon

END");

			Sql(@"ALTER PROCEDURE [dbo].[BaoCaoBanHang_LoiNhuan]
    @SearchString [nvarchar](max),
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [nvarchar](max),
    @LoaiHangHoa [nvarchar](max),
    @TheoDoi [nvarchar](max),
    @TrangThai [nvarchar](max),
    @ID_NhomHang uniqueidentifier,
	@LoaiChungTu [nvarchar](max),
    @ID_NguoiDung [uniqueidentifier]
AS
BEGIN
set nocount on;

    DECLARE @XemGiaVon as nvarchar
	Set @XemGiaVon = (Select 
		Case when nd.LaAdmin = '1' then '1' else
		Case when nd.XemGiaVon is null then '0' else nd.XemGiaVon end end as XemGiaVon
		From
		HT_NguoiDung nd	
		where nd.ID = @ID_NguoiDung);

	DECLARE @tblSearchString TABLE (Name [nvarchar](max));
	DECLARE @count int;
	INSERT INTO @tblSearchString(Name) select  Name from [dbo].[splitstringByChar](@SearchString, ' ') where Name!='';
	Select @count =  (Select count(*) from @tblSearchString);

	DECLARE @tblChiNhanh TABLE(ID UNIQUEIDENTIFIER)
	INSERT INTO @tblChiNhanh
	select Name from splitstring(@ID_ChiNhanh);

	declare @tblCTHD table (
		NgayLapHoaDon datetime,
		MaHoaDon nvarchar(max),
		LoaiHoaDon int,
		ID_DonVi uniqueidentifier,
		ID_PhieuTiepNhan uniqueidentifier,
		ID_DoiTuong uniqueidentifier,
		ID_NhanVien uniqueidentifier,
		TongTienHang float,
		TongGiamGia	float,
		KhuyeMai_GiamGia float,
		ChoThanhToan bit,
		ID uniqueidentifier,
		ID_HoaDon uniqueidentifier,
		ID_DonViQuiDoi uniqueidentifier,
		ID_LoHang uniqueidentifier,
		ID_ChiTietGoiDV	uniqueidentifier,
		ID_ChiTietDinhLuong uniqueidentifier,
		ID_ParentCombo uniqueidentifier,
		SoLuong float,
		DonGia float,
		GiaVonfloat float,
		TienChietKhau float,
		TienChiPhi float,
		ThanhTien float,
		ThanhToan float,
		GhiChu nvarchar(max),
		ChatLieu nvarchar(max),
		LoaiThoiGianBH int,
		ThoiGianBaoHanh float,
		TenHangHoaThayThe nvarchar(max),
		TienThue float,	
		GiamGiaHD float,
		GiaVon float,
		TienVon float
		)
	insert into @tblCTHD
	exec BCBanHang_GetCTHD @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu

	declare @tblChiPhi table (ID_ParentCombo uniqueidentifier,ID_DonViQuiDoi uniqueidentifier, ChiPhi float, 
		ID_NhanVien uniqueidentifier,ID_DoiTuong uniqueidentifier)
	insert into @tblChiPhi
	exec BCBanHang_GetChiPhi @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu

		SELECT 
    	a.TenHangHoa,
		a.TenHangHoaFull,
		a.MaHangHoa,
		a.TenDonViTinh,
		a.ThuocTinh_GiaTri,
		a.TenLoHang,
		a.SoLuongBan,
		a.ThanhTien,
		a.SoLuongTra,
		a.GiaTriTra,
		a.GiamGiaHD,
		a.DoanhThuThuan,
		iif(@XemGiaVon='1', a.TienVon,0) as TienVon,
		a.TienThue,
		a.ChiPhi,
		iif(@XemGiaVon='1',a.LaiLo,	0) as LaiLo	,
		iif(@XemGiaVon='1',ROUND(IIF(a.DoanhThuThuan = 0 OR a.DoanhThuThuan is null, 0,		
			a.LaiLo/ abs(a.DoanhThuThuan) * 100), 1),0) AS TySuat
    	FROM
    	(

		select 			
			hh.TenHangHoa,
			qdChuan.MaHangHoa,
			qdChuan.TenDonViTinh,
			qdChuan.ThuocTinhGiaTri as ThuocTinh_GiaTri,
			concat(hh.TenHangHoa, qdChuan.ThuocTinhGiaTri) as TenHangHoaFull,
			iif(hh.LoaiHangHoa is null, iif(hh.LaHangHoa = '1', 1, 2), hh.LoaiHangHoa) as LoaiHangHoa,
			lo.MaLoHang as TenLoHang,
			tblQD.SoLuongMua as SoLuongBan,
			tblQD.GiaTriMua as ThanhTien,
			tblQD.SoLuongTra,
			tblQD.GiaTriTra,
			tblQD.GiamGiaHangMua - tblQD.GiamGiaHangTra as GiamGiaHD,
			tblQD.GiaTriMua - tblQD.GiaTriTra - (tblQD.GiamGiaHangMua - tblQD.GiamGiaHangTra) as DoanhThuThuan,
			tblQD.GiaVonHangMua - tblQD.GiaVonHangTra as  TienVon,
			tblQD.TongThueHangMua - tblQD.TongThueHangTra as  TienThue,
			tblQD.GiaTriMua - tblQD.GiaTriTra - (tblQD.GiamGiaHangMua - tblQD.GiamGiaHangTra) - (tblQD.GiaVonHangMua - tblQD.GiaVonHangTra) - tblQD.ChiPhi  as  LaiLo,
			tblQD.GiamGiaHangMua,
			tblQD.GiamGiaHangTra,
			tblQD.TongThueHangMua,
			tblQD.TongThueHangTra,
			tblQD.ChiPhi
		from
		(
			select 
			qd.ID_HangHoa,
			tblMuaTra.ID_LoHang,
			sum(tblMuaTra.SoLuongMua  * isnull(qd.TyLeChuyenDoi,1)) as SoLuongMua,
			sum(tblMuaTra.GiaTriMua) as GiaTriMua,
			sum(tblMuaTra.TongThueHangMua) as TongThueHangMua,
			sum(tblMuaTra.GiamGiaHangMua) as GiamGiaHangMua,
			sum(tblMuaTra.GiaVonHangMua) as GiaVonHangMua,
			sum(tblMuaTra.SoLuongTra  * isnull(qd.TyLeChuyenDoi,1)) as SoLuongTra,
			sum(tblMuaTra.GiaTriTra) as GiaTriTra,
			sum(tblMuaTra.TongThueHangTra) as TongThueHangTra,
			sum(tblMuaTra.GiamGiaHangTra) as GiamGiaHangTra,
			sum(tblMuaTra.GiaVonHangTra) as GiaVonHangTra,
			sum(ISNULL(tblMuaTra.ChiPhi,0)) as ChiPhi			
		from
			(			
			select 
				ct.ID_DonViQuiDoi, ct.ID_LoHang,
				sum(SoLuong) as SoLuongMua,
				sum(ThanhTien) as GiaTriMua,
				sum(ct.TienThue) as TongThueHangMua,
				sum(ct.GiamGiaHD) as GiamGiaHangMua,
				sum(ct.TienVon) as GiaVonHangMua,				
				0 as SoLuongTra,
				0 as GiaTriTra,
				0 as TongThueHangTra,
				0 as GiamGiaHangTra,
				0 as GiaVonHangTra,
				max(ChiPhi) as ChiPhi
			from @tblCTHD ct		
			left join 
				(select ID_DonViQuiDoi, sum(ChiPhi) as ChiPhi from @tblChiPhi  group by ID_DonViQuiDoi
				 ) cp on ct.ID_DonViQuiDoi = cp.ID_DonViQuiDoi
			where (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
			and (ct.ID_ParentCombo = ct.ID or ct.ID_ParentCombo is null)	
			group by ct.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang

				---- giatritra + giavon hangtra

			union all
			select 
				ct.ID_DonViQuiDoi, ct.ID_LoHang,
				0 as SoLuongMua,
				0 as GiaTriMua,
				0 as TongThueHangMua,
				0 as GiamGiaHangMua,
				0 as GiaVonHangMua,
				sum(SoLuong) as SoLuongTra,
				sum(ThanhTien) as GiaTriTra,
				sum(ct.TienThue * ct.SoLuong) as TienThueHangTra,
				sum(iif(hd.TongTienHang=0,0, ct.ThanhTien  * hd.TongGiamGia /hd.TongTienHang)) as GiamGiaHangTra,
				sum(ct.SoLuong * ct.GiaVon) as GiaVonHangTra,
				0 as ChiPhi
			from BH_HoaDon hd
			join BH_HoaDon_ChiTiet ct on hd.id= ct.ID_HoaDon 
			where hd.ChoThanhToan= 0
			and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
			and (ct.ID_ParentCombo = ct.ID or ct.ID_ParentCombo is null)
			and hd.NgayLapHoaDon >= @timeStart and hd.NgayLapHoaDon < @timeEnd
			and exists (select ID_DonVi from @tblChiNhanh dv where hd.ID_DonVi= dv.ID)
			and hd.LoaiHoaDon =6
			and (ct.ChatLieu is null or ct.ChatLieu !='4') ---- khong lay ct sudung dichvu
			group by hd.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang
		) tblMuaTra 	
		join DonViQuiDoi qd on tblMuaTra.ID_DonViQuiDoi= qd.ID		
		group by qd.ID_HangHoa, tblMuaTra.ID_LoHang
	)tblQD
	join DM_HangHoa hh on tblQD.ID_HangHoa = hh.ID
	join DonViQuiDoi qdChuan on hh.ID= qdChuan.ID_HangHoa and qdChuan.LaDonViChuan=1
	left join DM_LoHang lo on hh.ID= lo.ID_HangHoa and tblQD.ID_LoHang = lo.ID
	left join DM_NhomHangHoa nhh on hh.ID_NhomHang= nhh.ID
		where 
		exists (SELECT ID FROM dbo.GetListNhomHangHoa(@ID_NhomHang) allnhh where nhh.ID= allnhh.ID)		
    	and hh.TheoDoi like @TheoDoi
		and qdChuan.Xoa like @TrangThai				
		AND ((select count(Name) from @tblSearchString b where 
				hh.TenHangHoa like '%'+b.Name+'%' 
    			OR hh.TenHangHoa_KhongDau like '%'+b.Name+'%' 
    			or hh.TenHangHoa_KyTuDau like '%'+b.Name+'%' 
    				or qdChuan.MaHangHoa like '%'+b.Name+'%'
    				or qdChuan.TenDonViTinh like '%' +b.Name +'%' 
					or lo.MaLoHang like '%' +b.Name +'%'
    			or qdChuan.ThuocTinhGiaTri like '%'+b.Name+'%')=@count or @count=0)
		) a
		where a.LoaiHangHoa in (select name from dbo.splitstring(@LoaiHangHoa))
		order by a.MaHangHoa  	
  
END");

			Sql(@"ALTER PROCEDURE [dbo].[BaoCaoBanHang_NhomHang]
    @SearchString [nvarchar](max),
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [nvarchar](max),
   @LoaiHangHoa [nvarchar](max),
    @TheoDoi [nvarchar](max),
    @TrangThai [nvarchar](max),
    @ID_NhomHang uniqueidentifier,
	@LoaiChungTu [nvarchar](max),
    @ID_NguoiDung [uniqueidentifier]
AS
BEGIN
    DECLARE @XemGiaVon as nvarchar
    	Set @XemGiaVon = (Select 
    		Case when nd.LaAdmin = '1' then '1' else
    		Case when nd.XemGiaVon is null then '0' else nd.XemGiaVon end end as XemGiaVon
    		From
    		HT_NguoiDung nd	
    		where nd.ID = @ID_NguoiDung)			

		DECLARE @tblSearchString TABLE (Name [nvarchar](max));
		DECLARE @count int;
		INSERT INTO @tblSearchString(Name) select  Name from [dbo].[splitstringByChar](@SearchString, ' ') where Name!='';
		Select @count =  (Select count(*) from @tblSearchString);

		declare @tblCTHD table (
		NgayLapHoaDon datetime,
		MaHoaDon nvarchar(max),
		LoaiHoaDon int,
		ID_DonVi uniqueidentifier,
		ID_PhieuTiepNhan uniqueidentifier,
		ID_DoiTuong uniqueidentifier,
		ID_NhanVien uniqueidentifier,
		TongTienHang float,
		TongGiamGia	float,
		KhuyeMai_GiamGia float,
		ChoThanhToan bit,
		ID uniqueidentifier,
		ID_HoaDon uniqueidentifier,
		ID_DonViQuiDoi uniqueidentifier,
		ID_LoHang uniqueidentifier,
		ID_ChiTietGoiDV	uniqueidentifier,
		ID_ChiTietDinhLuong uniqueidentifier,
		ID_ParentCombo uniqueidentifier,
		SoLuong float,
		DonGia float,
		GiaVonfloat float,
		TienChietKhau float,
		TienChiPhi float,
		ThanhTien float,
		ThanhToan float,
		GhiChu nvarchar(max),
		ChatLieu nvarchar(max),
		LoaiThoiGianBH int,
		ThoiGianBaoHanh float,
		TenHangHoaThayThe nvarchar(max),
		TienThue float,	
		GiamGiaHD float,
		GiaVon float,
		TienVon float
		)

	insert into @tblCTHD
	exec BCBanHang_GetCTHD @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu

	declare @tblChiPhi table (ID_ParentCombo uniqueidentifier,ID_DonViQuiDoi uniqueidentifier, ChiPhi float, 
		ID_NhanVien uniqueidentifier,ID_DoiTuong uniqueidentifier)
	insert into @tblChiPhi
	exec BCBanHang_GetChiPhi @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu
	

		select 
			dtNhom.ID_NhomHang, dtNhom.TenNhomHangHoa,
			sum(SoLuong) as SoLuong,
			sum(ThanhTien) as ThanhTien,
			sum(GiamGiaHD) as GiamGiaHD,
			sum(TienThue) as TienThue,
			sum(DoanhThu) as DoanhThu,
			iif(@XemGiaVon='1',sum(TienVon),0) as TienVon,
			iif(@XemGiaVon='1',sum(LaiLo)- sum(ChiPhi),0) as LaiLo,
			sum(ChiPhi) as ChiPhi
		from
		(
		select *
		from
		(
			select 
				hh.ID_NhomHang,		
				iif(hh.LoaiHangHoa is null, iif(hh.LaHangHoa = '1', 1, 2), hh.LoaiHangHoa) as LoaiHangHoa,
				ISNULL(nhh.TenNhomHangHoa,  N'Nhóm hàng hóa mặc định') as TenNhomHangHoa,		
				cast(c.SoLuong as float) as SoLuong,		
				cast(c.ThanhTien as float) as ThanhTien,
				cast(c.GiamGiaHD as float) as GiamGiaHD,
				cast(c.TienThue as float) as TienThue,			
				iif(@XemGiaVon='1',cast(c.TienVon as float),0) as TienVon,
				cast(c.ThanhTien - c.GiamGiaHD as float) as DoanhThu,
				iif(@XemGiaVon='1',cast(c.ThanhTien - c.GiamGiaHD - c.TienVon as float),0) as LaiLo		,
				isnull(cp.ChiPhi,0) as ChiPhi
			from 
			(
			select 			
				sum(b.SoLuong * isnull(qd.TyLeChuyenDoi,1)) as SoLuong,
				sum(b.ThanhTien) as ThanhTien,
				sum(b.TienVon) as TienVon,
				qd.ID_HangHoa,
				b.ID_LoHang,			
				sum(b.GiamGiaHD) as GiamGiaHD,
				sum(b.TienThue) as TienThue
				
			from (
			select 					
				a.ID_LoHang, a.ID_DonViQuiDoi,									
				sum(isnull(a.TienThue,0)) as TienThue,
				sum(isnull(a.GiamGiaHD,0)) as GiamGiaHD,
				sum(SoLuong) as SoLuong,
				sum(ThanhTien) as ThanhTien,
				sum(TienVon) as TienVon			
			from
			(			
				select 
					tblN.ID_DonViQuiDoi, tblN.ID_LoHang,
					sum(tblN.TienThue) as TienThue,
					sum(tblN.GiamGiaHD) as GiamGiaHD,
					sum(tblN.SoLuong) as SoLuong,
					sum(tblN.ThanhTien) as ThanhTien,
					sum(tblN.TienVon) as TienVon
				from
				(
					select 
							ct.ID,ct.ID_DonViQuiDoi, ct.ID_LoHang,
							ct.TienThue,
    						CT.GiamGiaHD,
							ct.SoLuong,
							ct.ThanhTien, 	
							CT.TienVon
					from @tblCTHD ct					
					where (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)						
					and (ct.ID_ParentCombo is null or ct.ID_ParentCombo= ct.ID)						
					) tblN group by tblN.ID_LoHang, tblN.ID_DonViQuiDoi	
				) a group by a.ID_LoHang, a.ID_DonViQuiDoi
			)b
			join DonViQuiDoi qd on b.ID_DonViQuiDoi= qd.ID
			group by qd.ID_HangHoa, b.ID_LoHang
				) c
			join DM_HangHoa hh on c.ID_HangHoa = hh.ID
			join DonViQuiDoi qd on hh.ID = qd.ID_HangHoa and qd.LaDonViChuan=1
			left join DM_LoHang lo on c.ID_LoHang = lo.ID
			left join DM_NhomHangHoa nhh on hh.ID_NhomHang= nhh.ID
			left join (
				select ID_DonViQuiDoi, sum(ChiPhi) as ChiPhi from @tblChiPhi group by ID_DonViQuiDoi
				) cp on qd.ID = cp.ID_DonViQuiDoi

			where 
			exists (SELECT ID FROM dbo.GetListNhomHangHoa(@ID_NhomHang) allnhh where nhh.ID= allnhh.ID)	
    		and hh.TheoDoi like @TheoDoi
			and qd.Xoa like @TrangThai		
			AND
			((select count(Name) from @tblSearchString b where 
    				hh.TenHangHoa_KhongDau like '%'+b.Name+'%' 
    				or hh.TenHangHoa_KyTuDau like '%'+b.Name+'%' 
    					or hh.TenHangHoa like '%'+b.Name+'%'
    					or lo.MaLoHang like '%' +b.Name +'%' 
    				or qd.MaHangHoa like '%'+b.Name+'%'
    					or nhh.TenNhomHangHoa like '%'+b.Name+'%'
    					or nhh.TenNhomHangHoa_KhongDau like '%'+b.Name+'%'
    					or nhh.TenNhomHangHoa_KyTuDau like '%'+b.Name+'%'
    					or qd.TenDonViTinh like '%'+b.Name+'%'				
    					or qd.ThuocTinhGiaTri like '%'+b.Name+'%')=@count or @count=0)
		) a where a.LoaiHangHoa in (select name from dbo.splitstring(@LoaiHangHoa))	
	)dtNhom group by dtNhom.ID_NhomHang, dtNhom.TenNhomHangHoa
  
END");

			Sql(@"ALTER PROCEDURE [dbo].[BaoCaoBanHang_TheoNhanVien]
    @TenNhanVien [nvarchar](max),
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [nvarchar](max),
    @LoaiHangHoa [nvarchar](max),
    @TheoDoi [nvarchar](max),
    @TrangThai [nvarchar](max),
    @ID_NhomHang uniqueidentifier,
	@LoaiChungTu [nvarchar](max),
    @ID_NguoiDung [uniqueidentifier],
	@IDPhongBan nvarchar(max)
AS
BEGIN
    DECLARE @XemGiaVon as nvarchar
    	Set @XemGiaVon = (Select 
    	Case when nd.LaAdmin = '1' then '1' else
    	Case when nd.XemGiaVon is null then '0' else nd.XemGiaVon end end as XemGiaVon
    	From HT_NguoiDung nd	
    	where nd.ID = @ID_NguoiDung)

		DECLARE @tblChiNhanh TABLE (ID_DonVi uniqueidentifier)
		insert into @tblChiNhanh
		select * from splitstring(@ID_ChiNhanh)

		DECLARE @tblNhomHang TABLE (ID uniqueidentifier)
		insert into @tblNhomHang
		SELECT ID FROM dbo.GetListNhomHangHoa(@ID_NhomHang)

		DECLARE @tblLoaiChungTu TABLE(ID INT)
		INSERT INTO @tblLoaiChungTu
		select Name from splitstring(@LoaiChungTu);


		DECLARE @tblDepartment TABLE (ID_PhongBan uniqueidentifier)
		if @IDPhongBan =''
			insert into @tblDepartment
			select distinct ID_PhongBan from NS_QuaTrinhCongTac pb
		else
			insert into @tblDepartment
			select * from splitstring(@IDPhongBan)
	
	DECLARE @tblSearchString TABLE (Name [nvarchar](max));
	DECLARE @count int;
	INSERT INTO @tblSearchString(Name) select  Name from [dbo].[splitstringByChar](@TenNhanVien, ' ') where Name!='';
	Select @count =  (Select count(*) from @tblSearchString);

	declare @tblCTHD table (
		NgayLapHoaDon datetime,
		MaHoaDon nvarchar(max),
		LoaiHoaDon int,
		ID_DonVi uniqueidentifier,
		ID_PhieuTiepNhan uniqueidentifier,
		ID_DoiTuong uniqueidentifier,
		ID_NhanVien uniqueidentifier,
		TongTienHang float,
		TongGiamGia	float,
		KhuyeMai_GiamGia float,
		ChoThanhToan bit,
		ID uniqueidentifier,
		ID_HoaDon uniqueidentifier,
		ID_DonViQuiDoi uniqueidentifier,
		ID_LoHang uniqueidentifier,
		ID_ChiTietGoiDV	uniqueidentifier,
		ID_ChiTietDinhLuong uniqueidentifier,
		ID_ParentCombo uniqueidentifier,
		SoLuong float,
		DonGia float,
		GiaVonfloat float,
		TienChietKhau float,
		TienChiPhi float,
		ThanhTien float,
		ThanhToan float,
		GhiChu nvarchar(max),
		ChatLieu nvarchar(max),
		LoaiThoiGianBH int,
		ThoiGianBaoHanh float,
		TenHangHoaThayThe nvarchar(max),
		TienThue float,	
		GiamGiaHD float,
		GiaVon float,
		TienVon float
		)
	insert into @tblCTHD
	exec BCBanHang_GetCTHD @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu

	declare @tblChiPhi table (ID_ParentCombo uniqueidentifier,ID_DonViQuiDoi uniqueidentifier, ChiPhi float, 
		ID_NhanVien uniqueidentifier,ID_DoiTuong uniqueidentifier)
	insert into @tblChiPhi
	exec BCBanHang_GetChiPhi @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu

    SELECT 
    	a.ID_NhanVien,
		a.MaNhanVien,
    	a.TenNhanVien, 
		a.SoLuongMua as SoLuongBan,
		cast(a.SoLuongTra as float) as SoLuongTra,
		a.GiaTriMua as ThanhTien,
		cast(a.GiaTriTra as float) as GiaTriTra,
		a.GiamGiaHangMua - a.GiamGiaHangTra as GiamGiaHD,
		a.TongThueHangMua - a.TongThueHangTra as TienThue,
		isnull(cpOut.ChiPhi,0) as ChiPhi,
		IIF(@XemGiaVon = '1',a.GiaVonHangMua - a.GiaVonHangTra,0) as TienVon,
		a.GiaTriMua - a.GiaTriTra  - (a.GiamGiaHangMua - a.GiamGiaHangTra) as DoanhThu,
		IIF(@XemGiaVon = '1',a.GiaTriMua - a.GiaTriTra  - (a.GiamGiaHangMua - a.GiamGiaHangTra) - (a.GiaVonHangMua - a.GiaVonHangTra)-isnull(cpOut.ChiPhi,0),0) as LaiLo		
    	FROM
    	(

		select 
		tblMuaTra.ID_NhanVien, 
		nv.TenNhanVien, nv.MaNhanVien,
		sum(SoLuongMua  * isnull(qd.TyLeChuyenDoi,1)) as SoLuongMua,
		sum(GiaTriMua) as GiaTriMua,
		sum(TongThueHangMua) as TongThueHangMua,
		sum(GiamGiaHangMua) as GiamGiaHangMua,
		sum(GiaVonHangMua) as GiaVonHangMua,
		sum(SoLuongTra  * isnull(qd.TyLeChuyenDoi,1)) as SoLuongTra,
		sum(GiaTriTra) as GiaTriTra,
		sum(TongThueHangTra) as TongThueHangTra,
		sum(GiamGiaHangTra) as GiamGiaHangTra,
		sum(GiaVonHangTra) as GiaVonHangTra
	from
		(		
		---- doanhthu + giavon hd le
		
		select 
			ct.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang,			
			sum(SoLuong) as SoLuongMua,
			sum(ThanhTien) as GiaTriMua,
			sum(ct.TienThue) as TongThueHangMua,
			sum(ct.GiamGiaHD) as GiamGiaHangMua,
			sum(ct.TienVon)	as GiaVonHangMua,			
			0 as SoLuongTra,
			0 as GiaTriTra,
			0 as TongThueHangTra,
			0 as GiamGiaHangTra,
			0 as GiaVonHangTra
		from @tblCTHD ct			
		where (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
		and (ct.ID_ParentCombo = ct.ID or ct.ID_ParentCombo is null)		
		group by ct.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang

			---- giatritra + giavon hangtra

		union all
		select 
			hd.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang,
			0 as SoLuongMua,
			0 as GiaTriMua,
			0 as TongThueHangMua,
			0 as GiamGiaHangMua,
			0 as GiaVonHangMua,
			sum(SoLuong) as SoLuongTra,
			sum(ThanhTien) as GiaTriTra,
			sum(ct.TienThue * ct.SoLuong) as TienThueHangTra,
			sum(iif(hd.TongTienHang=0,0, ct.ThanhTien  * hd.TongGiamGia /hd.TongTienHang)) as GiamGiaHangTra,
			sum(ct.SoLuong * ct.GiaVon) as GiaVonHangTra
		from BH_HoaDon hd		
		join BH_HoaDon_ChiTiet ct on hd.id= ct.ID_HoaDon
		where hd.ChoThanhToan= 0
		and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
		and (ct.ID_ParentCombo = ct.ID or ct.ID_ParentCombo is null)
		and hd.NgayLapHoaDon >= @timeStart and hd.NgayLapHoaDon < @timeEnd
		and exists (select ID_DonVi from @tblChiNhanh dv where hd.ID_DonVi= dv.ID_DonVi)
		and exists (select Name from dbo.splitstring(@LoaiChungTu) ctu where hd.LoaiHoaDon= ctu.Name)
		and hd.LoaiHoaDon =6
		and (ct.ChatLieu is null or ct.ChatLieu !='4') ---- khong lay ct sudung dichvu
		group by hd.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang
	) tblMuaTra 
	join NS_NhanVien nv on tblMuaTra.ID_NhanVien= nv.ID
	join DonViQuiDoi qd on tblMuaTra.ID_DonViQuiDoi= qd.ID
	join DM_HangHoa hh on qd.ID_HangHoa = hh.ID
	left join DM_NhomHangHoa nhh on hh.ID_NhomHang= nhh.ID
		where 
		exists (SELECT ID FROM dbo.GetListNhomHangHoa(@ID_NhomHang) allnhh where nhh.ID= allnhh.ID)
		and iif(hh.LoaiHangHoa is null, iif(hh.LaHangHoa = '1', 1, 2), hh.LoaiHangHoa) in (select name from dbo.splitstring(@LoaiHangHoa))		
    	and hh.TheoDoi like @TheoDoi
		and qd.Xoa like @TrangThai		
		and exists (select pb.ID_PhongBan
				from @tblDepartment pb 
				join NS_QuaTrinhCongTac ct on pb.ID_PhongBan = ct.ID_PhongBan or  ct.ID_PhongBan is null
				where ct.ID_NhanVien = nv.ID )
		AND
		((select count(Name) from @tblSearchString b where 
			nv.TenNhanVien like '%'+b.Name+'%' 
    			or nv.TenNhanVienKhongDau like '%'+b.Name+'%' 
    				or nv.TenNhanVienChuCaiDau like '%'+b.Name+'%'
    				or nv.MaNhanVien like '%' +b.Name +'%' 
					or nv.DienThoaiDiDong like '%' +b.Name +'%'
					or nv.DienThoaiNhaRieng like '%' +b.Name +'%'
    		)=@count or @count=0)
		group by tblMuaTra.ID_NhanVien, nv.TenNhanVien, nv.MaNhanVien
    	) a
		left join (
			select cp.ID_NhanVien, sum(cp.ChiPhi) as Chiphi
			from @tblChiPhi cp
			group by cp.ID_NhanVien
		) cpOut on a.ID_NhanVien= cpOut.ID_NhanVien
END");

			Sql(@"ALTER PROCEDURE [dbo].[BaoCaoBanHangChiTiet_TheoKhachHang]
    @ID_KhachHang [uniqueidentifier],
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [nvarchar](max),
    @LoaiHangHoa [nvarchar](max),
    @TheoDoi [nvarchar](max),
    @TrangThai [nvarchar](max),
    @ID_NhomHang uniqueidentifier,
	@LoaiChungTu [nvarchar](max),
    @ID_NguoiDung [uniqueidentifier]
AS
BEGIN
set nocount on;
    DECLARE @XemGiaVon as nvarchar
    Set @XemGiaVon = (Select 
    	Case when nd.LaAdmin = '1' then '1' else
    	Case when nd.XemGiaVon is null then '0' else nd.XemGiaVon end end as XemGiaVon
    	From
    	HT_NguoiDung nd	
    	where nd.ID = @ID_NguoiDung);

	declare @tblChiNhanh table(ID_DonVi uniqueidentifier)
	insert into @tblChiNhanh 
	select * from dbo.splitstring(@ID_ChiNhanh)

	declare @tblCTHD table (
		NgayLapHoaDon datetime,
		MaHoaDon nvarchar(max),
		LoaiHoaDon int,
		ID_DonVi uniqueidentifier,
		ID_PhieuTiepNhan uniqueidentifier,
		ID_DoiTuong uniqueidentifier,
		ID_NhanVien uniqueidentifier,
		TongTienHang float,
		TongGiamGia	float,
		KhuyeMai_GiamGia float,
		ChoThanhToan bit,
		ID uniqueidentifier,
		ID_HoaDon uniqueidentifier,
		ID_DonViQuiDoi uniqueidentifier,
		ID_LoHang uniqueidentifier,
		ID_ChiTietGoiDV	uniqueidentifier,
		ID_ChiTietDinhLuong uniqueidentifier,
		ID_ParentCombo uniqueidentifier,
		SoLuong float,
		DonGia float,
		GiaVonfloat float,
		TienChietKhau float,
		TienChiPhi float,
		ThanhTien float,
		ThanhToan float,
		GhiChu nvarchar(max),
		ChatLieu nvarchar(max),
		LoaiThoiGianBH int,
		ThoiGianBaoHanh float,
		TenHangHoaThayThe nvarchar(max),
		TienThue float,	
		GiamGiaHD float,
		GiaVon float,
		TienVon float
		)
	insert into @tblCTHD
	exec BCBanHang_GetCTHD @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu

	declare @tblChiPhi table (ID_ParentCombo uniqueidentifier,ID_DonViQuiDoi uniqueidentifier, ChiPhi float, 
		ID_NhanVien uniqueidentifier,ID_DoiTuong uniqueidentifier)
	insert into @tblChiPhi
	exec BCBanHang_GetChiPhi @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu

		select *
		from
		(
		select 					
			hh.TenHangHoa,
			qdChuan.MaHangHoa,
			qdChuan.TenDonViTinh,
			qdChuan.ThuocTinhGiaTri as ThuocTinh_GiaTri,
			concat(hh.TenHangHoa, qdChuan.ThuocTinhGiaTri) as TenHangHoaFull,
			iif(hh.LoaiHangHoa is null, iif(hh.LaHangHoa = '1', 1, 2), hh.LoaiHangHoa) as LoaiHangHoa,
			lo.MaLoHang as TenLoHang,
			tblQD.ChiPhi,
			tblQD.SoLuongMua,
			tblQD.GiaTriMua,
			tblQD.SoLuongTra,
			tblQD.GiaTriTra,
			tblQD.SoLuongMua - tblQD.SoLuongTra as SoLuong,
			tblQD.GiaTriMua - tblQD.GiaTriTra as ThanhTien,
			tblQD.GiamGiaHDMua - tblQD.GiamGiaHDTra as GiamGiaHD,
			tblQD.GiaTriMua - tblQD.GiaTriTra - (tblQD.GiamGiaHDMua - tblQD.GiamGiaHDTra) as DoanhThuThuan,		
			iif(@XemGiaVon = '1',tblQD.GVMua - tblQD.GVTra,1) as  TienVon,
			tblQD.ThueHDMua - tblQD.ThueHDTra as  TongTienThue,
			iif(@XemGiaVon = '1',tblQD.GiaTriMua - tblQD.GiaTriTra - (tblQD.GiamGiaHDMua - tblQD.GiamGiaHDTra) 
			- (tblQD.GVMua - tblQD.GVTra) -tblQD.ChiPhi,1) as  LaiLo			
		from
		(
	SELECT 	qd.ID_HangHoa, tbl.ID_LoHang,			
		sum(SoLuongMua  * isnull(qd.TyLeChuyenDoi,1)) as SoLuongMua,
		sum(SoLuongTra  * isnull(qd.TyLeChuyenDoi,1)) as SoLuongTra,
		sum(tbl.GiaTriMua - ISNULL(tbl.GiamGiaHangMua,0)) as GiaTriMua,
		sum(isnull(tbl.GiaTriTra,0) - isnull(GiamGiaHangTra,0)) as GiaTriTra ,
		sum(tbl.TongThueHangMua) as ThueHDMua,
		sum(isnull(tbl.TongThueHangTra,0)) as ThueHDTra ,
		sum(tbl.GiamGiaHangMua) as GiamGiaHDMua,
		sum(isnull(tbl.GiamGiaHangTra,0)) as GiamGiaHDTra,	
		sum(tbl.GiaVonHangMua) as GVMua,
		sum(isnull(tbl.GiaVonHangTra,0)) as GVTra,
		sum(isnull(cp.ChiPhi,0)) as ChiPhi	
FROM
    (
		select 
			ct.ID_DoiTuong,ct.ID_DonViQuiDoi, ct.ID_LoHang,
			sum(SoLuong) as SoLuongMua,
			sum(ThanhTien) as GiaTriMua,
			sum(ct.TienThue) as TongThueHangMua,
			sum(ct.GiamGiaHD) as GiamGiaHangMua,	
			sum(ct.TienVon) as GiaVonHangMua,		
			0 as SoLuongTra,
			0 as GiaTriTra,
			0 as TongThueHangTra,
			0 as GiamGiaHangTra,
			0 as GiaVonHangTra
		from @tblCTHD ct		
		where ct.ID_DoiTuong= @ID_KhachHang
		and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
		and (ct.ID_ParentCombo = ct.ID or ct.ID_ParentCombo is null)		
		group by ct.ID_DoiTuong,ct.ID_DonViQuiDoi, ct.ID_LoHang

			---- giatritra + giavon hangtra

		union all
		select 
			hd.ID_DoiTuong,ct.ID_DonViQuiDoi, ct.ID_LoHang,
			0 as SoLuongMua,
			0 as GiaTriMua,
			0 as TongThueHangMua,
			0 as GiamGiaHangMua,
			0 as GiaVonHangMua,
			sum(SoLuong) as SoLuongTra,
			sum(ThanhTien) as GiaTriTra,
			sum(ct.TienThue * ct.SoLuong) as TienThueHangTra,
			sum(iif(hd.TongTienHang=0,0, ct.ThanhTien  * hd.TongGiamGia /hd.TongTienHang)) as GiamGiaHangTra,
			sum(ct.SoLuong * ct.GiaVon) as GiaVonHangTra
		from BH_HoaDon hd
		join BH_HoaDon_ChiTiet ct on hd.id= ct.ID_HoaDon 
		where hd.ChoThanhToan= 0
		and hd.ID_DoiTuong= @ID_KhachHang
		and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
		and (ct.ID_ParentCombo = ct.ID or ct.ID_ParentCombo is null)
		and hd.NgayLapHoaDon >= @timeStart and hd.NgayLapHoaDon < @timeEnd
		and exists (select ID_DonVi from @tblChiNhanh dv where hd.ID_DonVi= dv.ID_DonVi)
		and hd.LoaiHoaDon =6
		and (ct.ChatLieu is null or ct.ChatLieu !='4') ---- khong lay ct sudung dichvu
		group by hd.ID_DoiTuong,ct.ID_DonViQuiDoi, ct.ID_LoHang
		)tbl	
		join DonViQuiDoi qd on tbl.ID_DonViQuiDoi= qd.ID
		left join 
			(select ID_DonViQuiDoi, sum(ChiPhi) as ChiPhi from @tblChiPhi where ID_DoiTuong= @ID_KhachHang group by ID_DonViQuiDoi
			 ) cp on qd.ID = cp.ID_DonViQuiDoi
		group by qd.ID_HangHoa, tbl.ID_LoHang
	) tblQD
	join DM_HangHoa hh on tblQD.ID_HangHoa = hh.ID
	join DonViQuiDoi qdChuan on hh.ID= qdChuan.ID_HangHoa and qdChuan.LaDonViChuan=1
	left join DM_LoHang lo on hh.ID= lo.ID_HangHoa and tblQD.ID_LoHang = lo.ID
	left join DM_NhomHangHoa nhh on hh.ID_NhomHang= nhh.ID
		where 
		exists (SELECT ID FROM dbo.GetListNhomHangHoa(@ID_NhomHang) allnhh where nhh.ID= allnhh.ID)		
    	and hh.TheoDoi like @TheoDoi
		and qdChuan.Xoa like @TrangThai				
	) a where a.LoaiHangHoa in (select name from dbo.splitstring(@LoaiHangHoa))	
END");

			Sql(@"ALTER PROCEDURE [dbo].[BaoCaoBanHangChiTiet_TheoNhanVien]
    @ID_NhanVien UNIQUEIDENTIFIER,
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [nvarchar](max),
    @LoaiHangHoa [nvarchar](max),
    @TheoDoi [nvarchar](max),
    @TrangThai [nvarchar](max),
    @ID_NhomHang uniqueidentifier,
	@LoaiChungTu [nvarchar](max),
    @ID_NguoiDung [uniqueidentifier]
AS
BEGIN
set nocount on;

		declare @tblChiNhanh table(ID_DonVi uniqueidentifier)
		insert into @tblChiNhanh
		select Name from dbo.splitstring(@ID_ChiNhanh)

		declare @tblNhomHang table(ID_NhomHang uniqueidentifier)
		insert into @tblNhomHang
		SELECT ID FROM dbo.GetListNhomHangHoa(@ID_NhomHang)


    DECLARE @XemGiaVon as nvarchar
    	Set @XemGiaVon = (Select 
    	Case when nd.LaAdmin = '1' then '1' else
    	Case when nd.XemGiaVon is null then '0' else nd.XemGiaVon end end as XemGiaVon
    	From HT_NguoiDung nd	
    	where nd.ID = @ID_NguoiDung)

	declare @tblCTHD table (
		NgayLapHoaDon datetime,
		MaHoaDon nvarchar(max),
		LoaiHoaDon int,
		ID_DonVi uniqueidentifier,
		ID_PhieuTiepNhan uniqueidentifier,
		ID_DoiTuong uniqueidentifier,
		ID_NhanVien uniqueidentifier,
		TongTienHang float,
		TongGiamGia	float,
		KhuyeMai_GiamGia float,
		ChoThanhToan bit,
		ID uniqueidentifier,
		ID_HoaDon uniqueidentifier,
		ID_DonViQuiDoi uniqueidentifier,
		ID_LoHang uniqueidentifier,
		ID_ChiTietGoiDV	uniqueidentifier,
		ID_ChiTietDinhLuong uniqueidentifier,
		ID_ParentCombo uniqueidentifier,
		SoLuong float,
		DonGia float,
		GiaVonfloat float,
		TienChietKhau float,
		TienChiPhi float,
		ThanhTien float,
		ThanhToan float,
		GhiChu nvarchar(max),
		ChatLieu nvarchar(max),
		LoaiThoiGianBH int,
		ThoiGianBaoHanh float,
		TenHangHoaThayThe nvarchar(max),
		TienThue float,	
		GiamGiaHD float,
		GiaVon float,
		TienVon float
		)
	insert into @tblCTHD
	exec BCBanHang_GetCTHD @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu

	declare @tblChiPhi table (ID_ParentCombo uniqueidentifier,ID_DonViQuiDoi uniqueidentifier, ChiPhi float, 
		ID_NhanVien uniqueidentifier,ID_DoiTuong uniqueidentifier)
	insert into @tblChiPhi
	exec BCBanHang_GetChiPhi @ID_ChiNhanh, @timeStart, @timeEnd, @LoaiChungTu
	
	select *
	from
	(
    SELECT  
			hh.TenHangHoa,
			qdChuan.MaHangHoa,
			qdChuan.TenDonViTinh,
			qdChuan.ThuocTinhGiaTri as ThuocTinh_GiaTri,
			concat(hh.TenHangHoa, qdChuan.ThuocTinhGiaTri) as TenHangHoaFull,
			iif(hh.LoaiHangHoa is null, iif(hh.LaHangHoa = '1', 1, 2), hh.LoaiHangHoa) as LoaiHangHoa,
			lo.MaLoHang as TenLoHang,
			tblQD.SoLuongMua as SoLuongBan,
			tblQD.GiaTriMua as ThanhTien,
			tblQD.SoLuongTra,
			tblQD.GiaTriTra,	
			isnull(cp.ChiPhi,0) as ChiPhi,
			tblQD.GiamGiaHDMua - tblQD.GiamGiaHDTra as GiamGiaHD,		
			iif(@XemGiaVon = '1',tblQD.GVMua - tblQD.GVTra,1) as  TienVon,
			tblQD.ThueHDMua - tblQD.ThueHDTra as  TongTienThue,
			iif(@XemGiaVon = '1',tblQD.GiaTriMua - tblQD.GiaTriTra - (tblQD.GiamGiaHDMua - tblQD.GiamGiaHDTra) - (tblQD.GVMua - tblQD.GVTra) -isnull(cp.ChiPhi,0),1) as  LaiLo
    	FROM
    	(
		SELECT 	qd.ID_HangHoa, tbl.ID_LoHang,			
			sum(SoLuongMua  * isnull(qd.TyLeChuyenDoi,1)) as SoLuongMua,
			sum(SoLuongTra  * isnull(qd.TyLeChuyenDoi,1)) as SoLuongTra,
			sum(tbl.GiaTriMua) as GiaTriMua,
			sum(isnull(tbl.GiaTriTra,0)) as GiaTriTra ,
			sum(tbl.TongThueHangMua) as ThueHDMua,
			sum(isnull(tbl.TongThueHangTra,0)) as ThueHDTra ,
			sum(tbl.GiamGiaHangMua) as GiamGiaHDMua,
			sum(isnull(tbl.GiamGiaHangTra,0)) as GiamGiaHDTra,	
			sum(tbl.GiaVonHangMua) as GVMua,
			sum(isnull(tbl.GiaVonHangTra,0)) as GVTra	
		FROM
		(		
				select 
					ct.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang,
					sum(SoLuong) as SoLuongMua,
					sum(ct.ThanhTien) as GiaTriMua,
					sum(ct.TienThue) as TongThueHangMua,
					sum(ct.GiamGiaHD) as GiamGiaHangMua,
					sum(ct.TienVon) as GiaVonHangMua,					
					0 as SoLuongTra,
					0 as GiaTriTra,
					0 as TongThueHangTra,
					0 as GiamGiaHangTra,
					0 as GiaVonHangTra
				from @tblCTHD ct				
				where (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
				and (ct.ID_ParentCombo = ct.ID or ct.ID_ParentCombo is null)
				and ct.ID_NhanVien= @ID_NhanVien
				group by ct.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang

					---- giatritra + giavon hangtra

				union all
				select 
					hd.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang,
					0 as SoLuongMua,
					0 as GiaTriMua,
					0 as TongThueHangMua,
					0 as GiamGiaHangMua,
					0 as GiaVonHangMua,
					sum(SoLuong) as SoLuongTra,
					sum(ThanhTien) as GiaTriTra,
					sum(ct.TienThue * ct.SoLuong) as TienThueHangTra,
					sum(iif(hd.TongTienHang=0,0, ct.ThanhTien  * hd.TongGiamGia /hd.TongTienHang)) as GiamGiaHangTra,
					sum(ct.SoLuong * ct.GiaVon) as GiaVonHangTra
				from BH_HoaDon hd
				join BH_HoaDon_ChiTiet ct on hd.id= ct.ID_HoaDon 
				where hd.ChoThanhToan= 0
				and hd.ID_NhanVien= @ID_NhanVien
				and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
				and (ct.ID_ParentCombo = ct.ID or ct.ID_ParentCombo is null)
				and hd.NgayLapHoaDon >= @timeStart and hd.NgayLapHoaDon < @timeEnd
				and exists (select ID_DonVi from @tblChiNhanh dv where hd.ID_DonVi= dv.ID_DonVi)
				and hd.LoaiHoaDon =6
				and (ct.ChatLieu is null or ct.ChatLieu !='4') ---- khong lay ct sudung dichvu
				group by hd.ID_NhanVien,ct.ID_DonViQuiDoi, ct.ID_LoHang
				)tbl	
				join DonViQuiDoi qd on tbl.ID_DonViQuiDoi= qd.ID
				group by qd.ID_HangHoa, tbl.ID_LoHang
   		
    	) tblQD
		join DM_HangHoa hh on tblQD.ID_HangHoa = hh.ID
	join DonViQuiDoi qdChuan on hh.ID= qdChuan.ID_HangHoa and qdChuan.LaDonViChuan=1
	left join DM_LoHang lo on hh.ID= lo.ID_HangHoa and tblQD.ID_LoHang = lo.ID
	left join DM_NhomHangHoa nhh on hh.ID_NhomHang= nhh.ID
	left join (
		select ID_DonViQuiDoi, sum(ChiPhi) as ChiPhi from @tblChiPhi group by ID_DonViQuiDoi
		) cp on qdChuan.ID= cp.ID_DonViQuiDoi
	where 
	exists (SELECT ID FROM dbo.GetListNhomHangHoa(@ID_NhomHang) allnhh where nhh.ID= allnhh.ID)
    and hh.TheoDoi like @TheoDoi
	and qdChuan.Xoa like @TrangThai	
	) a where a.LoaiHangHoa in (select name from dbo.splitstring(@LoaiHangHoa))	
	order by a.SoLuongBan desc
END");

			Sql(@"ALTER PROCEDURE [dbo].[BaoCaoDichVu_NhapXuatTon]
    @Text_Search [nvarchar](max),
    @MaHH [nvarchar](max),
    @MaKH [nvarchar](max),
    @MaKH_TV [nvarchar](max),
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [nvarchar](max),
    @LaHangHoa [nvarchar](max),
    @TheoDoi [nvarchar](max),
    @TrangThai [nvarchar](max),
	@ThoiHan [nvarchar](max),
    @ID_NhomHang [nvarchar](max),
    @ID_NhomHang_SP [nvarchar](max)
AS
BEGIN
	set nocount on;
	DECLARE @tblSearchString TABLE (Name [nvarchar](max));
    DECLARE @count int;
    INSERT INTO @tblSearchString(Name) select  Name from [dbo].[splitstringByChar](@Text_Search, ' ') where Name!='';
    Select @count =  (Select count(*) from @tblSearchString);

	declare @tblChiNhanh table( ID_DonVi uniqueidentifier)
	insert into @tblChiNhanh
	select name from dbo.splitstring(@ID_ChiNhanh)

	declare @dtNow datetime = getdate()

	declare @tblCTMua_DauKy table(
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
		GiamGiaHD float)
	insert into @tblCTMua_DauKy
	exec BaoCaoGoiDV_GetCTMua @ID_ChiNhanh,'2016-01-01',@timeStart

	declare @tblCTMua_GiuaKy table(
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
		GiamGiaHD float)
	insert into @tblCTMua_GiuaKy
	exec BaoCaoGoiDV_GetCTMua @ID_ChiNhanh,@timeStart,@timeEnd

    SELECT
    	a.MaHangHoa as MaHangHoa,
    	MAX(a.TenHangHoaFull) as TenHangHoaFull,
    	MAX(a.TenHangHoa) as TenHangHoa,
    	MAX(a.ThuocTinh_GiaTri) as ThuocTinh_GiaTri,
    	MAX(a.TenDonViTinh) as TenDonViTinh,
    	a.TenLoHang as TenLoHang,
    	CAST(ROUND((SUM(a.SoLuongBanDK - a.SoLuongSuDungDK - a.SoLuongTraDK)), 2) as float) as SoLuongConLaiDK,
    	CAST(ROUND((SUM(a.GiaTriBanDK - a.GiaTriSuDungDK - a.GiaTriTraDK)), 0) as float) as GiaTriConLaiDK,
    	CAST(ROUND((SUM(a.SoLuongBanGK)), 2) as float) as SoLuongBanGK, 
    	CAST(ROUND((SUM(a.GiaTriBanGK)), 0) as float) as GiaTriBanGK, 
		CAST(ROUND((SUM(a.SoLuongTraGK)), 2) as float) as SoLuongTraGK, 
    	CAST(ROUND((SUM(a.GiaTriTraGK)), 0) as float) as GiaTriTraGK, 
    	CAST(ROUND((SUM(a.SoLuongSuDungGK)), 2) as float) as SoLuongSuDungGK, 
    	CAST(ROUND((SUM(a.GiaTriSuDungGK)), 0) as float) as GiaTriSuDungGK,
    	CAST(ROUND((SUM(a.SoLuongBanDK - a.SoLuongSuDungDK - a.SoLuongTraDK + a.SoLuongBanGK - a.SoLuongSuDungGK - a.SoLuongTraGK)), 2) as float) as SoLuongConLaiCK,
    	CAST(ROUND((SUM(a.GiaTriBanDK - a.GiaTriSuDungDK - a.GiaTriTraDK + a.GiaTriBanGK - a.GiaTriSuDungGK - a.GiaTriTraGK)), 0) as float) as GiaTriConLaiCK
    	FROM
    	(
		Select
    	dvqd.MaHangHoa,
    	concat(hh.TenHangHoa , dvqd.ThuocTinhGiaTri) as TenHangHoaFull,
    	hh.TenHangHoa,
    	dvqd.TenDonViTinh as TenDonViTinh,
    	dvqd.ThuocTinhGiaTri as ThuocTinh_GiaTri,
    	Case when lh.MaLoHang is null then '' else lh.MaLoHang end as TenLoHang,
    	Max(dt.MaDoiTuong) as MaKhachHang,
    	Max(dt.TenDoiTuong) as TenKhachHang,
    	Max(dt.DienThoai) as DienThoai,
    	Sum(td.SoLuongBanDK) as SoLuongBanDK,
		Sum(td.GiaTriBanDK) as GiaTriBanDK,
		Sum(td.SoLuongTraDK) as SoLuongTraDK,
		Sum(td.GiaTriTraDK) as GiaTriTraDK,
    	Sum(td.SoLuongSuDungDK) as SoLuongSuDungDK,
		sum(td.GiaTriSuDungDK) as GiaTriSuDungDK,
    	
		Sum(td.SoLuongBanGK) as SoLuongBanGK,
		Sum(td.GiaTriBanGK) as GiaTriBanGK,
		Sum(td.SoLuongTraGK) as SoLuongTraGK,
		Sum(td.GiaTriTraGK) as GiaTriTraGK,
    	Sum(td.SoLuongSuDungGK) as SoLuongSuDungGK,
		sum(td.GiaTriSuDungGK) as GiaTriSuDungGK,
	
    	Case When Max(hh.ID_NhomHang) is null then '00000000-0000-0000-0000-000000000000' else Max(hh.ID_NhomHang) end as ID_NhomHang,
		Case when GETDATE() <= Max(td.HanSuDungGoiDV) then 1 else 0 end as ThoiHan
    	FROM
    	(
    	 ---- Đầu kỳ
			Select 
			ctm.MaHoaDon,
			ctm.HanSuDungGoiDV,
			ctm.ID_DoiTuong,
			ctm.ID_DonVi,
			ctm.ID_DonViQuiDoi,
			ctm.ID_LoHang,
			ctm.SoLuong as SoLuongBanDK,
			ctm.ThanhTien as GiaTriBanDK,
			isnull(tbl.SoLuongTra,0) as SoLuongTraDK,
			isnull(tbl.GiaTriTra,0) as GiaTriTraDK,
			isnull(tbl.SoLuongSuDung,0) as SoLuongSuDungDK,
			isnull(tbl.GiaTriSuDung,0) as GiaTriSuDungDK,			
			0 as SoLuongBanGK,
			0 as GiaTriBanGK,
			0 as SoLuongTraGK,
			0 as GiaTriTraGK,
			0 as SoLuongSuDungGK,
			0 as GiaTriSuDungGK
			FROM @tblCTMua_DauKy ctm
			left join (
						select 
							tblSD.ID_ChiTietGoiDV,
							sum(tblSD.SoLuongTra) as SoLuongTra,
							sum(tblSD.GiaTriTra) as GiaTriTra,
							sum(tblSD.SoLuongSuDung) as SoLuongSuDung,
							sum(tblSD.GiaTriSuDung) as GiaTriSuDung
						from 
						(
							---- hdsudung
							Select 								
								ct.ID_ChiTietGoiDV,														
								0 as SoLuongTra,
								0 as GiaTriTra,
								ct.SoLuong as SoLuongSuDung,
								ct.SoLuong * ct.DonGia as GiaTriSuDung
							FROM BH_HoaDon hd
							join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
							join @tblCTMua_DauKy ctm on ctm.ID= ct.ID_ChiTietGoiDV
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
								0 as GiaVon
							FROM BH_HoaDon hd
							join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
							join @tblCTMua_DauKy ctm on ctm.ID= ct.ID_ChiTietGoiDV
							where hd.ChoThanhToan= 0
							and hd.LoaiHoaDon = 6
							and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)							
							)tblSD group by tblSD.ID_ChiTietGoiDV
			)tbl
			on ctm.ID= tbl.ID_ChiTietGoiDV

			union all
			---- giua ky
			Select 
			ctm.MaHoaDon,
			ctm.HanSuDungGoiDV,
			ctm.ID_DoiTuong,
			ctm.ID_DonVi,
			ctm.ID_DonViQuiDoi,
			ctm.ID_LoHang,								
			0 as SoLuongBanDK,
			0 as GiaTriBanDK,
			0 as SoLuongTraDK,
			0 as GiaTriTraDK,
			0 as SoLuongSuDungDK,
			0 as GiaTriSuDungDK,
			ctm.SoLuong as SoLuongBanDK,
			ctm.ThanhTien as GiaTriBanDK,	
			isnull(tbl.SoLuongTra,0) as SoLuongTraGK,
			isnull(tbl.GiaTriTra,0) as GiaTriTraGK,
			isnull(tbl.SoLuongSuDung,0) as SoLuongSuDungGK,
			isnull(tbl.GiaTriSuDung,0) as GiaTriSuDungGK
			FROM @tblCTMua_GiuaKy ctm
			left join (
						select 
							tblSD.ID_ChiTietGoiDV,
							sum(tblSD.SoLuongTra) as SoLuongTra,
							sum(tblSD.GiaTriTra) as GiaTriTra,
							sum(tblSD.SoLuongSuDung) as SoLuongSuDung,
							sum(tblSD.GiaTriSuDung) as GiaTriSuDung
						from 
						(
							---- hdsudung
							Select 								
								ct.ID_ChiTietGoiDV,														
								0 as SoLuongTra,
								0 as GiaTriTra,
								ct.SoLuong as SoLuongSuDung,
								ct.SoLuong * ct.DonGia as GiaTriSuDung
							FROM BH_HoaDon hd
							join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
							join @tblCTMua_GiuaKy ctm on ctm.ID= ct.ID_ChiTietGoiDV
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
								0 as GiaVon
							FROM BH_HoaDon hd
							join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
							join @tblCTMua_GiuaKy ctm on ctm.ID= ct.ID_ChiTietGoiDV
							where hd.ChoThanhToan= 0
							and hd.LoaiHoaDon = 6
							and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)							
							)tblSD group by tblSD.ID_ChiTietGoiDV
			)tbl
			on ctm.ID= tbl.ID_ChiTietGoiDV					
    	)as td
    	inner join DonViQuiDoi dvqd on td.ID_DonViQuiDoi = dvqd.ID
    		inner join DM_HangHoa hh on dvqd.ID_HangHoa = hh.ID
    		left join DM_DoiTuong dt on td.ID_DoiTuong = dt.ID
    		left join DM_LoHang lh on td.ID_LoHang = lh.ID
    		where td.ID_DonVi in (select * from splitstring(@ID_ChiNhanh))
    		and hh.LaHangHoa like @LaHangHoa
    		and hh.TheoDoi like @TheoDoi
    		and (dvqd.MaHangHoa like @Text_Search or dvqd.MaHangHoa like @MaHH or hh.TenHangHoa_KhongDau like @MaHH or hh.TenHangHoa_KyTuDau like @MaHH)
			and (dt.TenDoiTuong_KhongDau like @MaKH or dt.TenDoiTuong_ChuCaiDau like @MaKH or dt.DienThoai like @MaKH or dt.MaDoiTuong like @MaKH or dt.MaDoiTuong like @MaKH_TV)
			and dvqd.Xoa like @TrangThai
			Group by td.MaHoaDon, dvqd.MaHangHoa, hh.TenHangHoa, dvqd.TenDonViTinh, dvqd.ThuocTinhGiaTri, lh.MaLoHang, dvqd.Xoa
    		) as a
    		where (a.ID_NhomHang like @ID_NhomHang or a.ID_NhomHang in (select * from splitstring(@ID_NhomHang_SP)))
			and a.ThoiHan like @ThoiHan
    		Group by a.MaHangHoa, TenLoHang
    		ORDER BY SUM(a.GiaTriBanDK - a.GiaTriSuDungDK + a.GiaTriBanGK - a.GiaTriSuDungGK) DESC
END");

			Sql(@"ALTER PROCEDURE [dbo].[BaoCaoDichVu_SoDuTongHop]
    @Text_Search [nvarchar](max),
    @MaHH [nvarchar](max),
    @MaKH [nvarchar](max),
    @MaKH_TV [nvarchar](max),
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [nvarchar](max),
    @LaHangHoa [nvarchar](max),
    @TheoDoi [nvarchar](max),
    @TrangThai [nvarchar](max),
	@ThoiHan [nvarchar](max),
    @ID_NhomHang [nvarchar](max),
    @ID_NhomHang_SP [nvarchar](max),
	@ID_NguoiDung [uniqueidentifier]
AS
BEGIN
	declare @tblChiNhanh table( ID_DonVi uniqueidentifier)
	insert into @tblChiNhanh
	select name from dbo.splitstring(@ID_ChiNhanh)

	DECLARE @tblSearchString TABLE (Name [nvarchar](max));
    DECLARE @count int;
    INSERT INTO @tblSearchString(Name) select  Name from [dbo].[splitstringByChar](@Text_Search, ' ') where Name!='';

    Select @count =  (Select count(*) from @tblSearchString);
	DECLARE @XemGiaVon as nvarchar
    Set @XemGiaVon = (Select 
    Case when nd.LaAdmin = '1' then '1' else
    Case when nd.XemGiaVon is null then '0' else nd.XemGiaVon end end as XemGiaVon
    From  HT_NguoiDung nd	   
    where nd.ID = @ID_NguoiDung)

	declare @dtNow datetime = getdate()

	---- get list GDV mua ---
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
		GiamGiaHD float)
	insert into @tblCTMua
	exec BaoCaoGoiDV_GetCTMua @ID_ChiNhanh,@timeStart,@timeEnd
		
		Select 
			a.MaHoaDon,
			a.NgayLapHoaDon,
			a.NgayApDungGoiDV,
			a.HanSuDungGoiDV,
			a.MaKhachHang,
			a.TenKhachHang,
			a.DienThoai,
			a.GioiTinh,
			a.NhomKhachHang,
			a.TenNguonKhach,
			a.NguoiGioiThieu,
			sum(a.SoLuong) as SoLuong,
			sum(a.ThanhTien) as ThanhTien,
			sum(a.SoLuongTra) as SoLuongTra,
			sum(a.GiaTriTra) as GiaTriTra,
			sum(a.SoLuongSuDung) as SoLuongSuDung,
			iif(@XemGiaVon='0',cast( 0 as float),round( sum(a.GiaVon),2)) as GiaVon,
			round(sum(a.SoLuong) -  sum(a.SoLuongTra) - sum(a.SoLuongSuDung),2) as SoLuongConLai,
			CAST(ROUND(Case when DATEADD(day,-1,GETDATE()) <= MAX(a.HanSuDungGoiDV)
				then DATEDIFF(day,DATEADD(day,-1,GETDATE()),MAX(a.HanSuDungGoiDV)) else 0 end, 0) as float) as SoNgayConHan, 
			CAST(ROUND(Case when DATEADD(day,-1,GETDATE()) > MAX(a.HanSuDungGoiDV) 
			then DATEDIFF(day,DATEADD(day,-1,GETDATE()) ,MAX(a.HanSuDungGoiDV)) * (-1) else 0 end, 0) as float) as SoNgayHetHan			
		From
		(
				---- get by idnhom, thoihan --> check where
				select *
				from
				(
			
					select 
						ctm.ID_HoaDon,
						ctm.MaHoaDon,
						ctm.NgayLapHoaDon,
						ctm.NgayApDungGoiDV,
						ctm.HanSuDungGoiDV,
						dt.MaDoiTuong as MaKhachHang,
						dt.TenDoiTuong as TenKhachHang,
						dt.DienThoai,
						Case when dt.GioiTinhNam = 1 then N'Nam' else N'Nữ' end as GioiTinh,
						gt.TenDoiTuong as NguoiGioiThieu,
						nk.TenNguonKhach,
						isnull(dt.TenNhomDoiTuongs, N'Nhóm mặc định') as NhomKhachHang ,
						iif( hh.ID_NhomHang is null, '00000000-0000-0000-0000-000000000000',hh.ID_NhomHang) as ID_NhomHang,
						iif(@dtNow <=ctm.HanSuDungGoiDV,1,0) as ThoiHan,						
						ctm.SoLuong,
						ctm.ThanhTien,
						isnull(tbl.SoLuongTra,0) as SoLuongTra,
						isnull(tbl.GiaTriTra,0) as GiaTriTra,
						isnull(tbl.SoLuongSuDung,0) as SoLuongSuDung,
						isnull(tbl.GiaVon,0) as GiaVon						
					from @tblCTMua ctm
					inner join DonViQuiDoi dvqd on ctm.ID_DonViQuiDoi = dvqd.ID
					inner join DM_HangHoa hh on dvqd.ID_HangHoa = hh.ID
					left join DM_DoiTuong dt on ctm.ID_DoiTuong = dt.ID
					left join DM_DoiTuong gt on dt.ID_NguoiGioiThieu = gt.ID
					left join DM_NguonKhachHang nk on dt.ID_NguonKhach = nk.ID
					left join (
						select 
							tblSD.ID_ChiTietGoiDV,
							sum(tblSD.SoLuongTra) as SoLuongTra,
							sum(tblSD.GiaTriTra) as GiaTriTra,
							sum(tblSD.SoLuongSuDung) as SoLuongSuDung,
							sum(tblSD.GiaVon) as GiaVon
						from 
						(
							---- hdsudung
							Select 								
								ct.ID_ChiTietGoiDV,														
								0 as SoLuongTra,
								0 as GiaTriTra,
								ct.SoLuong as SoLuongSuDung,
								ct.SoLuong * ct.GiaVon as GiaVon
							FROM BH_HoaDon hd
							join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
							join @tblCTMua ctm on ct.ID_ChiTietGoiDV= ctm.ID
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
								0 as GiaVon
							FROM BH_HoaDon hd
							join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
							join @tblCTMua ctm on ct.ID_ChiTietGoiDV= ctm.ID
							where hd.ChoThanhToan= 0
							and hd.LoaiHoaDon = 6
							and (ct.ID_ChiTietDinhLuong = ct.ID or ct.ID_ChiTietDinhLuong is null)
							)tblSD group by tblSD.ID_ChiTietGoiDV

					) tbl on ctm.ID= tbl.ID_ChiTietGoiDV
				where hh.LaHangHoa like @LaHangHoa
    			and hh.TheoDoi like @TheoDoi
    			and dvqd.Xoa like @TrangThai
				AND ((select count(Name) from @tblSearchString b where 
					ctm.MaHoaDon like '%'+b.Name+'%'
    				or hh.TenHangHoa like '%'+b.Name+'%'
    				or dvqd.MaHangHoa like '%'+b.Name+'%'
    				or hh.TenHangHoa_KhongDau like '%'+b.Name+'%'
    				or hh.TenHangHoa_KyTuDau like '%'+b.Name+'%'
					or dt.DienThoai like '%'+b.Name+'%'
    				or dt.MaDoiTuong like '%'+b.Name+'%'
    				or dt.TenDoituong like '%'+b.Name+'%'
					or dt.TenDoiTuong_KhongDau like '%'+b.Name+'%'
					or dt.TenDoiTuong_ChuCaiDau like '%'+b.Name+'%'
					)=@count or @count=0)
			) b where b.ThoiHan like @ThoiHan
				and (b.ID_NhomHang like @ID_NhomHang or b.ID_NhomHang in (select * from splitstring(@ID_NhomHang_SP)))
			) a
    	Group by a.MaHoaDon,
			a.NgayLapHoaDon,
			a.NgayApDungGoiDV,
			a.HanSuDungGoiDV,
			a.MaKhachHang,
			a.TenKhachHang,
			a.DienThoai,
			a.GioiTinh,
			a.NhomKhachHang,
			a.TenNguonKhach,
			a.NguoiGioiThieu
    	order by a.NgayLapHoaDon desc
END");

			Sql(@"ALTER PROCEDURE [dbo].[DanhMucKhachHang_CongNo]
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [uniqueidentifier],
    @MaKH [nvarchar](max),
    @LoaiKH [int],
    @ID_NhomKhachHang [nvarchar](max),
    @timeStartKH [datetime],
    @timeEndKH [datetime]
AS
BEGIN
	set nocount on

	declare @tblIDNhoms table (ID varchar(36))
	if @ID_NhomKhachHang ='%%'
		begin
			-- check QuanLyKHTheochiNhanh
			declare @QLTheoCN bit = (select QuanLyKhachHangTheoDonVi from HT_CauHinhPhanMem where ID_DonVi like @ID_ChiNhanh)
			insert into @tblIDNhoms(ID) values ('00000000-0000-0000-0000-000000000000')

			if @QLTheoCN = 1
				begin									
					insert into @tblIDNhoms(ID)
					select *  from (
						-- get Nhom not not exist in NhomDoiTuong_DonVi
						select convert(varchar(36),ID) as ID_NhomDoiTuong from DM_NhomDoiTuong nhom  
						where not exists (select ID_NhomDoiTuong from NhomDoiTuong_DonVi where ID_NhomDoiTuong= nhom.ID) 
						and LoaiDoiTuong = @LoaiKH --and (TrangThai = 0)
						union all
						-- get Nhom at this ChiNhanh
						select convert(varchar(36),ID_NhomDoiTuong)  from NhomDoiTuong_DonVi where ID_DonVi like @ID_ChiNhanh) tbl
				end
			else
				begin				
				-- insert all
				insert into @tblIDNhoms(ID)
				select convert(varchar(36),ID) as ID_NhomDoiTuong from DM_NhomDoiTuong nhom  
				where LoaiDoiTuong = @LoaiKH --and (TrangThai is null or TrangThai = 1)
				end		
		end
	else
		begin
			set @ID_NhomKhachHang = REPLACE(@ID_NhomKhachHang,'%','')
			insert into @tblIDNhoms(ID) values (@ID_NhomKhachHang)
		end

    SELECT  distinct *
    		FROM
    		(
    		  SELECT 
    		  dt.ID as ID,
    		  dt.MaDoiTuong, 
			  case when dt.IDNhomDoiTuongs='' then '00000000-0000-0000-0000-000000000000' else  ISNULL(dt.IDNhomDoiTuongs,'00000000-0000-0000-0000-000000000000') end as ID_NhomDoiTuong,
    	      dt.TenDoiTuong,
    		  dt.TenDoiTuong_KhongDau,
    		  dt.TenDoiTuong_ChuCaiDau,
			  dt.ID_TrangThai,
    		  dt.GioiTinhNam,
    		  dt.NgaySinh_NgayTLap,
			  dt.NgayGiaoDichGanNhat,
    		  dt.DienThoai,
    		  dt.Email,
    		  dt.DiaChi,
    		  dt.MaSoThue,
    		  ISNULL(dt.GhiChu,'') as GhiChu,
    		  dt.NgayTao,
    		  dt.DinhDang_NgaySinh,
    		  ISNULL(dt.NguoiTao,'') as NguoiTao,
    		  dt.ID_NguonKhach,
    		  dt.ID_NhanVienPhuTrach,
    		  dt.ID_NguoiGioiThieu,
    		  dt.LaCaNhan,
    		  ISNULL(dt.TongTichDiem,0) as TongTichDiem,
			  case when right(rtrim(dt.TenNhomDoiTuongs),1) =',' then LEFT(Rtrim(dt.TenNhomDoiTuongs), len(dt.TenNhomDoiTuongs)-1) else ISNULL(dt.TenNhomDoiTuongs,N'Nhóm mặc định') end as TenNhomDT,-- remove last coma
    		  dt.ID_TinhThanh,
    		  dt.ID_QuanHuyen,
			  ISNULL(dt.TrangThai_TheGiaTri,1) as TrangThai_TheGiaTri,
			  ISNULL(trangthai.TenTrangThai,'') as TrangThaiKhachHang,
			  ISNULL(qh.TenQuanHuyen,'') as PhuongXa,
			  ISNULL(tt.TenTinhThanh,'') as KhuVuc,
			  ISNULL(dv.TenDonVi,'') as ConTy,
			  ISNULL(dv.SoDienThoai,'') as DienThoaiChiNhanh,
			  ISNULL(dv.DiaChi,'') as DiaChiChiNhanh,
			  ISNULL(nk.TenNguonKhach,'') as TenNguonKhach,
			  ISNULL(dt2.TenDoiTuong,'') as NguoiGioiThieu,
    	      CAST(ROUND(ISNULL(a.NoHienTai,0), 0) as float) as NoHienTai,
    		  CAST(ROUND(ISNULL(a.TongBan,0), 0) as float) as TongBan,
    		  CAST(ROUND(ISNULL(a.TongBanTruTraHang,0), 0) as float) as TongBanTruTraHang,
    		  CAST(ROUND(ISNULL(a.TongMua,0), 0) as float) as TongMua,
    		  CAST(ROUND(ISNULL(a.SoLanMuaHang,0), 0) as float) as SoLanMuaHang,
			  CAST(0 as float) as TongNapThe , 
			  CAST(0 as float) as SuDungThe , 
			  CAST(0 as float) as HoanTraTheGiaTri , 
			  CAST(0 as float) as SoDuTheGiaTri , 
			   a.PhiDichVu,
			  concat(dt.MaDoiTuong,' ',lower(dt.MaDoiTuong) ,' ', dt.TenDoiTuong,' ', dt.DienThoai,' ', dt.TenDoiTuong_KhongDau)  as Name_Phone
    		  FROM DM_DoiTuong dt    
				left join DM_DoiTuong_Nhom dtn on dt.ID= dtn.ID_DoiTuong
    			LEFT join DM_TinhThanh tt on dt.ID_TinhThanh = tt.ID
    			LEFT join DM_QuanHuyen qh on dt.ID_QuanHuyen = qh.ID
				LEFT join DM_NguonKhachHang nk on dt.ID_NguonKhach = nk.ID
				LEFT join DM_DoiTuong dt2 on dt.ID_NguoiGioiThieu = dt2.ID --and dt2.LoaiDoiTuong= 1 and dt2.TheoDoi='0'
    			LEFT join DM_DonVi dv on dt.ID_DonVi = dv.ID
				LEFT join DM_DoiTuong_TrangThai trangthai on dt.ID_TrangThai = trangthai.ID
    			LEFT Join
    			(
    			  SELECT tblThuChi.ID_DoiTuong,
					SUM(ISNULL(tblThuChi.DoanhThu,0)) + SUM(ISNULL(tblThuChi.TienChi,0)) 
					- sum(isnull(tblThuChi.PhiDichVu,0)) 
					- SUM(ISNULL(tblThuChi.TienThu,0)) - SUM(ISNULL(tblThuChi.GiaTriTra,0)) AS NoHienTai,
    				SUM(ISNULL(tblThuChi.DoanhThu, 0)) as TongBan,
    				SUM(ISNULL(tblThuChi.DoanhThu,0)) -  SUM(ISNULL(tblThuChi.GiaTriTra,0)) AS TongBanTruTraHang,
    				SUM(ISNULL(tblThuChi.GiaTriTra, 0)) - SUM(ISNULL(tblThuChi.DoanhThu,0))  as TongMua,
    				SUM(ISNULL(tblThuChi.SoLanMuaHang, 0)) as SoLanMuaHang,
					sum(isnull(tblThuChi.PhiDichVu,0)) as PhiDichVu
    				FROM
    				(
						select 
							cp.ID_NhaCungCap as ID_DoiTuong,
							0 as GiaTriTra,
    						0 as DoanhThu,
							0 AS TienThu,
    						0 AS TienChi, 
    						0 AS SoLanMuaHang,				
							sum(cp.ThanhTien) as PhiDichVu
						from BH_HoaDon_ChiPhi cp
						join BH_HoaDon hd on cp.ID_HoaDon = hd.ID
						where hd.ChoThanhToan = 0
						and hd.ID_DonVi= @ID_ChiNhanh
						group by cp.ID_NhaCungCap

						union all

							-- doanh thu
    						SELECT 
    							bhd.ID_DoiTuong,
    							0 AS GiaTriTra,
    							ISNULL(bhd.PhaiThanhToan,0) AS DoanhThu,
    							0 AS TienThu,
    							0 AS TienChi,
    							0 AS SoLanMuaHang,
								0 as PhiDichVu
    						FROM BH_HoaDon bhd
    						WHERE bhd.LoaiHoaDon in (1,7,19,22, 25) AND bhd.ChoThanhToan = 0 
							AND bhd.NgayLapHoaDon >= @timeStart AND bhd.NgayLapHoaDon < @timeEnd
    						AND bhd.ID_DonVi = @ID_ChiNhanh

							
    						union all
							-- gia tri trả từ bán hàng
    						SELECT bhd.ID_DoiTuong,
    							ISNULL(bhd.PhaiThanhToan,0) AS GiaTriTra,
    							0 AS DoanhThu,
    							0 AS TienThu,
    							0 AS TienChi, 
    							0 AS SoLanMuaHang,
								0 as PhiDichVu
    						FROM BH_HoaDon bhd   						
    						WHERE (bhd.LoaiHoaDon = '6' or bhd.LoaiHoaDon = 4) AND bhd.ChoThanhToan = 0  
							AND bhd.NgayLapHoaDon >= @timeStart AND bhd.NgayLapHoaDon < @timeEnd  						
    						AND bhd.ID_DonVi = @ID_ChiNhanh

							union all

							-- tienthu
							SELECT 
    							qhdct.ID_DoiTuong,						
    							0 AS GiaTriTra,
    							0 AS DoanhThu,
    							ISNULL(qhdct.TienThu,0) AS TienThu,
    							0 AS TienChi,
								0 AS SoLanMuaHang,
								0 as PhiDichVu
    						FROM Quy_HoaDon qhd
    						JOIN Quy_HoaDon_ChiTiet qhdct ON qhd.ID = qhdct.ID_HoaDon
    						WHERE qhd.LoaiHoaDon = '11' AND  (qhd.TrangThai != '0' OR qhd.TrangThai is null)
    						AND qhd.ID_DonVi = @ID_ChiNhanh
							AND qhd.NgayLapHoaDon >= @timeStart AND qhd.NgayLapHoaDon < @timeEnd  
							
							union all

							-- tienchi
    						SELECT 
    							qhdct.ID_DoiTuong,						
    							0 AS GiaTriTra,
    							0 AS DoanhThu,
    							0 AS TienThu,
    							ISNULL(qhdct.TienThu,0) AS TienChi,
								0 AS SoLanMuaHang,
								0 as PhiDichVu
    						FROM Quy_HoaDon qhd
    						JOIN Quy_HoaDon_ChiTiet qhdct ON qhd.ID = qhdct.ID_HoaDon
    						WHERE qhd.LoaiHoaDon = '12' AND (qhd.TrangThai != '0' OR qhd.TrangThai is null)
							AND qhd.NgayLapHoaDon >= @timeStart AND qhd.NgayLapHoaDon < @timeEnd  
    						AND qhd.ID_DonVi = @ID_ChiNhanh

							Union All
							-- solan mua hang
    						Select 
    							hd.ID_DoiTuong,
    							0 AS GiaTriTra,
    							0 AS DoanhThu,
    							0 AS TienThu,
								0 as TienChi,
    							COUNT(*) AS SoLanMuaHang,
								0 as PhiDichVu
    						From BH_HoaDon hd 
    						where (hd.LoaiHoaDon = 1 or hd.LoaiHoaDon = 19)
    						and hd.ChoThanhToan = 0
    						AND hd.NgayLapHoaDon >= @timeStart AND hd.NgayLapHoaDon < @timeEnd 
							GROUP BY hd.ID_DoiTuong  						    					
							
    					)AS tblThuChi
    						GROUP BY tblThuChi.ID_DoiTuong
    				) a on dt.ID = a.ID_DoiTuong  					
						WHERE (dt.MaDoiTuong LIKE @MaKH 
							OR dt.TenDoiTuong_ChuCaiDau LIKE @MaKH  
							OR dt.TenDoiTuong_KhongDau LIKE @MaKH 
							OR dt.TenDoiTuong LIKE @MaKH 
    						OR dt.DienThoai LIKE @MaKH)
    					and dt.loaidoituong = @loaiKH
    					and dt.NgayTao >= @timeStartKH and dt.NgayTao < @timeEndKH
    					AND dt.TheoDoi =0
						and exists (select ID from @tblIDNhoms nhom where dtn.ID_NhomDoiTuong = nhom.ID OR dtn.ID_DoiTuong is null)	
						and dt.ID not like '%00000000-0000-0000-0000-0000%'
    				)b					
					--INNER JOIN @tblIDNhoms tblsearch ON CHARINDEX(CONCAT(', ', tblsearch.ID, ', '), CONCAT(', ', b.ID_NhomDoiTuong, ', '))>0
				order by b.ngaytao desc 
END");

			Sql(@"ALTER PROCEDURE [dbo].[GetAllDinhLuongDichVu]
	@ID_DonVi nvarchar(max),
    @ID_DichVu nvarchar(max)
AS
BEGIN
	set nocount on;

	select a.*,
		a.SoLuong as SoLuongMacDinh,
		a.SoLuong as SoLuongDinhLuong_BanDau,
		a.SoLuong * a.GiaVon as ThanhTien,
		iif(a.LaHangHoa= '0',0, isnull(tk.TonKho,0)) as TonKho	
	from
	(
   select 
		dl.ID,
		dl.ID_DichVu,
		dl.ID_DonViQuiDoi,
		dl.ID_LoHang,
		dl.GhiChu,
		dl.SoLuong,
		dl.STT,
		iif(dl.DonGia is null, qd.GiaBan, dl.DonGia) as DonGia,
		qd.GiaBan,
		hh.LaHangHoa,
		hh.TenHangHoa,
		hh.TenHangHoa as TenHangHoaThayThe,
		isnull(hh.DonViTinhQuyCach,'') as DonViTinhQuyCach,
		iif(hh.QuyCach is null or hh.QuyCach < 1, 1, hh.QuyCach) as QuyCach,
		qd.MaHangHoa,
		qd.TenDonViTinh,
		iif(qd.TyLeChuyenDoi is null or qd.TyLeChuyenDoi = 0, 1,qd.TyLeChuyenDoi) as TyLeChuyenDoi,
		iif(hh.LaHangHoa ='0', dbo.GetGiaVonOfDichVu(@ID_DonVi, dl.ID_DonViQuiDoi),isnull(gv.GiaVon,0)) as GiaVon,
		iif(hh.QuyCach is null or hh.QuyCach =0, 1, hh.QuyCach) * dl.SoLuong as SoLuongQuyCach,
		iif(hh.LoaiHangHoa is null, iif(hh.LaHangHoa='1',1,2), hh.LoaiHangHoa) as LoaiHangHoa,
		lo.MaLoHang
	from DinhLuongDichVu dl
	join DonViQuiDoi qd on dl.ID_DonViQuiDoi = qd.ID
	join DM_HangHoa hh on qd.ID_HangHoa = hh.ID
	left join DM_LoHang lo on dl.ID_LoHang = lo.ID 
	left join DM_GiaVon gv on dl.ID_DonViQuiDoi = gv.ID_DonViQuiDoi 
		and gv.ID_DonVi = @ID_DonVi	and (dl.ID_LoHang = gv.ID_LoHang or dl.ID_LoHang is null)
	where dl.ID_DichVu like @ID_DichVu
	and qd.Xoa='0'
	) a
	left join DM_HangHoa_TonKho tk on a.ID_DonViQuiDoi = tk.ID_DonViQuyDoi 
	and  tk.ID_DonVi = @ID_DonVi and (tk.ID_LoHang= a.ID_LoHang or a.ID_LoHang is null)
END");

			Sql(@"ALTER PROCEDURE [dbo].[GetHoaDonDatHang_afterXuLy]
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [uniqueidentifier],
    @txtSearch [nvarchar](max),
    @CurrentPage [int],
    @PageSize [int]
AS
BEGIN
    set nocount on;
	DECLARE @tblSearchString TABLE (Name [nvarchar](max));
    DECLARE @count int;
    INSERT INTO @tblSearchString(Name) select  Name from [dbo].[splitstringByChar](@txtSearch, ' ') where Name!='';
    Select @count =  (Select count(*) from @tblSearchString);
    
    SELECT 
    	c.ID,
    	c.MaHoaDon,
    	c.LoaiHoaDon,
    	c.NgayLapHoaDon,
    	c.TenDoiTuong,
    	c.DienThoai,
    	c.ID_NhanVien,
    	c.ID_DoiTuong,
    	c.ID_BangGia,
    	c.ID_DonVi,
    	c.YeuCau,
    		'' as MaHoaDonGoc,
    		ISNULL(c.MaDoiTuong,'') as MaDoiTuong,
    	ISNULL(c.NguoiTaoHD,'') as NguoiTaoHD,
    	c.TenNhanVien,
    	c.DienGiai,
    	c.TongTienHang, 
    		c.TongGiamGia, 
    		c.PhaiThanhToan, 
    		c.KhachDaTra,
    		c.TongChietKhau,
    		c.TongTienThue, 
			c.TongChiPhi, 
    	c.TrangThai,
    	c.HoaDon_HangHoa, -- string contail all MaHangHoa,TenHangHoa of HoaDon
		c.MaPhieuTiepNhan, c.BienSo, c.ID_PhieuTiepNhan, c.ID_Xe, c.ID_BaoHiem, 
		c.PTThueHoaDon,
		c.TenBaoHiem,
		c.ChoThanhToan,
		c.TongThanhToan
    	FROM
    	(
    		select 
    		a.ID as ID,
    		bhhd.MaHoaDon,
    		hdXMLOut.HoaDon_HangHoa,
    		bhhd.LoaiHoaDon,
    		bhhd.ID_NhanVien,
    		ISNULL(bhhd.ID_DoiTuong,'00000000-0000-0000-0000-000000000000') as ID_DoiTuong,
    		bhhd.ID_BangGia,
    		bhhd.NgayLapHoaDon,
    		bhhd.YeuCau,
    		bhhd.ID_DonVi,
    			dt.MaDoiTuong,
    			ISNULL(dt.TenDoiTuong, N'Khách lẻ') as TenDoiTuong,
    		ISNULL(dt.TenDoiTuong_KhongDau, N'khach le') as TenDoiTuong_KhongDau,
    			ISNULL(dt.TenDoiTuong_ChuCaiDau, N'kl') as TenDoiTuong_ChuCaiDau,
    			ISNULL(dt.DienThoai, N'') as DienThoai,
    			ISNULL(nv.TenNhanVien, N'') as TenNhanVien,
    		bhhd.DienGiai,
    		bhhd.NguoiTao as NguoiTaoHD,
			isnull(bhhd.TongThanhToan, bhhd.PhaiThanhToan) as TongThanhToan,
    		CAST(ROUND(bhhd.TongTienHang, 0) as float) as TongTienHang,
    		CAST(ROUND(bhhd.TongGiamGia, 0) as float) as TongGiamGia,
    		CAST(ROUND(bhhd.PhaiThanhToan, 0) as float) as PhaiThanhToan,
    			CAST(ROUND(bhhd.TongTienThue, 0) as float) as TongTienThue,
				isnull(bhhd.TongChiPhi,0) as TongChiPhi,
				isnull(bhhd.PTThueHoaDon,0) as PTThueHoaDon,
    			a.KhachDaTra,
    			bhhd.TongChietKhau,
				bhhd.ID_BaoHiem,
    			bhhd.ChoThanhToan,
				bhhd.ID_PhieuTiepNhan,
				bhhd.ID_Xe,
				bh.TenDoiTuong as TenBaoHiem,	
				isnull(tn.MaPhieuTiepNhan,'') as MaPhieuTiepNhan,
				isnull(xe.BienSo,'') as BienSo,
    		Case When bhhd.YeuCau = '1' then N'Phiếu tạm' when bhhd.YeuCau = '3' then N'Hoàn thành' when bhhd.YeuCau = '2' then N'Đang giao hàng' else N'Đã hủy' end as TrangThai
    		FROM
    		(
    			select 
    			b.ID,
    			SUM(ISNULL(b.KhachDaTra, 0)) as KhachDaTra
    			from
    			(
    					-- get infor PhieuThu from HDDatHang (HuyPhieuThu (qhd.TrangThai ='0')
    				Select 
    					bhhd.ID,						
    					Case when qhd.TrangThai='0' then 0 else case when qhd.LoaiHoaDon = 11 then ISNULL(hdct.Tienthu, 0) else -ISNULL(hdct.Tienthu, 0) end end as KhachDaTra,
    						0 as SoLuongBan,
    						0 as SoLuongTra				
    					from BH_HoaDon bhhd
    				left join Quy_HoaDon_ChiTiet hdct on bhhd.ID = hdct.ID_HoaDonLienQuan
    				left join Quy_HoaDon qhd on hdct.ID_HoaDon = qhd.ID 				
    				where bhhd.LoaiHoaDon = '3' and bhhd.ChoThanhToan is not null
    					and bhhd.NgayLapHoadon >= @timeStart and bhhd.NgayLapHoaDon < @timeEnd and bhhd.ID_DonVi = @ID_ChiNhanh  
    
    				union all
    					-- get infor PhieuThu/Chi from HDXuLy
    				Select
    					hdt.ID,						
    						Case when bhhd.ChoThanhToan is null or qhd.TrangThai='0' then (Case when qhd.LoaiHoaDon = 11 or qhd.TrangThai='0' then 0 else -ISNULL(hdct.Tienthu, 0) end)
    						else (Case when qhd.LoaiHoaDon = 11 then ISNULL(hdct.Tienthu, 0) else -ISNULL(hdct.Tienthu, 0) end) end as KhachDaTra,
    						0 as SoLuongBan,
    						0 as SoLuongTra
    				from BH_HoaDon bhhd
    				inner join BH_HoaDon hdt on (bhhd.ID_HoaDon = hdt.ID and hdt.ChoThanhToan = '0')
    				left join Quy_HoaDon_ChiTiet hdct on bhhd.ID = hdct.ID_HoaDonLienQuan
    				left join Quy_HoaDon qhd on (hdct.ID_HoaDon = qhd.ID)
    				where hdt.LoaiHoaDon = '3' 
    					and bhhd.ChoThanhToan='0'
    					and bhhd.NgayLapHoadon >= @timeStart and bhhd.NgayLapHoaDon < @timeEnd and bhhd.ID_DonVi = @ID_ChiNhanh					
    			) b
    			group by b.ID 
    		) as a
    		inner join BH_HoaDon bhhd on a.ID = bhhd.ID
    		left join DM_DoiTuong dt on bhhd.ID_DoiTuong = dt.ID
    		left join NS_NhanVien nv on bhhd.ID_NhanVien = nv.ID 
			left join Gara_PhieuTiepNhan tn on bhhd.ID_PhieuTiepNhan= tn.ID
			left join Gara_DanhMucXe xe on tn.ID_Xe = xe.ID
			left join DM_DoiTuong bh on bhhd.ID_BaoHiem = bh.ID
    		left join 
    			(Select distinct hdXML.ID, 
    					(
    					select qd.MaHangHoa +', '  AS [text()], hh.TenHangHoa +', '  AS [text()]
    					from BH_HoaDon_ChiTiet ct
    					join DonViQuiDoi qd on ct.ID_DonViQuiDoi= qd.ID
    					join DM_HangHoa hh on  hh.ID= qd.ID_HangHoa
    					where ct.ID_HoaDon = hdXML.ID
    					For XML PATH ('')
    				) HoaDon_HangHoa
    			from BH_HoaDon hdXML) hdXMLOut on a.ID= hdXMLOut.ID
    		) as c
    		where c.NgayLapHoadon >= @timeStart and c.NgayLapHoaDon < @timeEnd
    		and c.LoaiHoaDon = 3 and c.YeuCau in (1,2) and c.ChoThanhToan = 0
			and
				((select count(Name) from @tblSearchString b where     
				c.MaDoiTuong like '%'+b.Name+'%'
				or c.TenDoiTuong like '%'+b.Name+'%'
				or c.TenDoiTuong_KhongDau like '%'+b.Name+'%'
				or c.DienThoai like '%'+b.Name+'%'		
				or c.BienSo like '%'+b.Name+'%'
				or c.MaPhieuTiepNhan like '%'+b.Name+'%'						
				or c.MaHoaDon like '%'+b.Name+'%'	
				)=@count or @count=0)	
    	ORDER BY c.NgayLapHoaDon DESC
END");

			Sql(@"ALTER PROCEDURE [dbo].[getlist_HoaDonDatHang]	
    @timeStart [datetime],
    @timeEnd [datetime],
    @ID_ChiNhanh [nvarchar](max),
    @maHD [nvarchar](max),
	@ID_NhanVienLogin uniqueidentifier,
	@NguoiTao nvarchar(max),
	@TrangThai nvarchar(max),
	@ColumnSort varchar(max),
	@SortBy varchar(max),
	@CurrentPage int,
	@PageSize int,
	@LaHoaDonSuaChua nvarchar(10)
AS
BEGIN
	set nocount on;

	declare @tblNhanVien table (ID uniqueidentifier)
	insert into @tblNhanVien
	select * from dbo.GetIDNhanVien_inPhongBan(@ID_NhanVienLogin, @ID_ChiNhanh,'DatHang_XemDS_PhongBan','DatHang_XemDS_HeThong');

	declare @tblChiNhanh table (ID varchar(40))
	insert into @tblChiNhanh
	select Name from dbo.splitstring(@ID_ChiNhanh)

	DECLARE @tblSearch TABLE (Name [nvarchar](max));
    DECLARE @count int;
    INSERT INTO @tblSearch(Name) select  Name from [dbo].[splitstringByChar](@maHD, ' ') where Name!='';
    Select @count =  (Select count(*) from @tblSearch);
	

with data_cte
as
(
    SELECT 
    	c.ID,
    	c.MaHoaDon,
    	c.LoaiHoaDon,
    	c.NgayLapHoaDon,
    	c.TenDoiTuong,
    	c.Email,
    	c.DienThoai,
    	c.ID_NhanVien,
    	c.ID_DoiTuong,
    	c.ID_BangGia,
		c.ID_BaoHiem,
    	c.ID_DonVi,
		c.ID_PhieuTiepNhan,
		c.TongThanhToan,
    	c.YeuCau,
		'' as MaHoaDonGoc,
		c.MaSoThue,
		c.TaiKhoanNganHang,
		ISNULL(c.MaDoiTuong,'') as MaDoiTuong,
    	ISNULL(c.NguoiTaoHD,'') as NguoiTaoHD,
    	c.DiaChiKhachHang,
		c.NgaySinh_NgayTLap,
    	c.KhuVuc,
    	c.PhuongXa,
    	c.TenDonVi,
		c.DiaChiChiNhanh,
    	c.DienThoaiChiNhanh,
    	c.TenNhanVien,
    	c.DienGiai,
    	c.TenBangGia,
    	c.TongTienHang, c.TongGiamGia, c.PhaiThanhToan, c.ConNo,
		c.TienMat,
		c.TienATM,
		c.ChuyenKhoan,
		c.KhachDaTra,c.TongChietKhau,c.TongTienThue, c.ThuTuThe, c.TongChiPhi,
    	c.TrangThai,
		c.TrangThaiHD,
		c.Gara_TrangThaiBG,
    	c.TheoDoi,
    	c.TenPhongBan,
		c.ChoThanhToan,
		c.ChiPhi_GhiChu,
    	'' as HoaDon_HangHoa, -- string contail all MaHangHoa,TenHangHoa of HoaDon
		 c.MaPhieuTiepNhan,
		c.BienSo,
		c.MaBaoHiem, c.TenBaoHiem, c.BH_SDT,
		c.LienHeBaoHiem, c.SoDienThoaiLienHeBaoHiem,
		c.PhaiThanhToanBaoHiem, c.PTThueBaoHiem,
		TongTienBHDuyet, c.PTThueHoaDon, c.TongTienThueBaoHiem, SoVuBaoHiem,KhauTruTheoVu, 
		PTGiamTruBoiThuong, GiamTruBoiThuong, BHThanhToanTruocThue

    	FROM
    	(
    		select 
    		a.ID as ID,
    		bhhd.MaHoaDon,
    		bhhd.LoaiHoaDon,
    		bhhd.ID_NhanVien,
    		bhhd.ID_DoiTuong,
    		bhhd.ID_BangGia,
    		bhhd.NgayLapHoaDon,
    		bhhd.YeuCau,
    		bhhd.ID_DonVi,
			bhhd.ID_BaoHiem,
			
    		CASE 
    			WHEN dt.TheoDoi IS NULL THEN 
    				CASE WHEN dt.ID IS NULL THEN '0' ELSE '1' END
    			ELSE dt.TheoDoi
    		END AS TheoDoi,
			dt.MaDoiTuong,
			dt.MaSoThue,
			dt.TaiKhoanNganHang,
			ISNULL(dt.TenDoiTuong, N'Khách lẻ') as TenDoiTuong,
    		ISNULL(dt.TenDoiTuong_KhongDau, N'khach le') as TenDoiTuong_KhongDau,
			ISNULL(dt.TenDoiTuong_ChuCaiDau, N'kl') as TenDoiTuong_ChuCaiDau,
			dt.NgaySinh_NgayTLap,
			ISNULL(dt.Email, N'') as Email,
			ISNULL(dt.DienThoai, N'') as DienThoai,
			ISNULL(dt.DiaChi, N'') as DiaChiKhachHang,
			ISNULL(tt.TenTinhThanh, N'') as KhuVuc,
			ISNULL(qh.TenQuanHuyen, N'') as PhuongXa,
			ISNULL(dv.TenDonVi, N'') as TenDonVi,
			ISNULL(dv.DiaChi, N'') as DiaChiChiNhanh,
			ISNULL(dv.SoDienThoai, N'') as DienThoaiChiNhanh,
			ISNULL(nv.TenNhanVien, N'') as TenNhanVien,
			ISNULL(nv.TenNhanVienKhongDau, N'') as TenNhanVienKhongDau,
    		ISNULL(dt.TongTichDiem,0) AS DiemSauGD,
    		ISNULL(gb.TenGiaBan,N'Bảng giá chung') AS TenBangGia,
    		bhhd.DienGiai,
    		bhhd.NguoiTao as NguoiTaoHD,
    		ISNULL(vt.TenViTri,'') as TenPhongBan,
			ceiling(isnull(bhhd.TongTienHang,0)) as TongTienHang,
    		ceiling(isnull(bhhd.TongGiamGia,0)) as TongGiamGia,    		
			ceiling(isnull(bhhd.PhaiThanhToan,0)) as PhaiThanhToan,
			CAST(ROUND(bhhd.TongTienThue, 0) as float) as TongTienThue,
			isnull(bhhd.TongChiPhi,	0) as TongChiPhi,
			bhhd.ChiPhi_GhiChu,
    		a.KhachDaTra,
			a.ThuTuThe,
			a.TienMat,
			a.TienATM,
			a.ChuyenKhoan,
    		bhhd.TongChietKhau,
			bhhd.ID_PhieuTiepNhan,					
			bhhd.TongThanhToan,
			bhhd.TongThanhToan - a.KhachDaTra as ConNo,
			bhhd.ChoThanhToan,

			isnull(bhhd.PTThueHoaDon,0) as PTThueHoaDon,			
			isnull(bhhd.PTThueBaoHiem,0) as PTThueBaoHiem,
			isnull(bhhd.TongTienThueBaoHiem,0) as TongTienThueBaoHiem,
			isnull(bhhd.SoVuBaoHiem,0) as SoVuBaoHiem,
			isnull(bhhd.KhauTruTheoVu,0) as KhauTruTheoVu,
			isnull(bhhd.TongTienBHDuyet,0) as TongTienBHDuyet,
			isnull(bhhd.PTGiamTruBoiThuong,0) as PTGiamTruBoiThuong,
			isnull(bhhd.GiamTruBoiThuong,0) as GiamTruBoiThuong,
			isnull(bhhd.BHThanhToanTruocThue,0) as BHThanhToanTruocThue,
			isnull(bhhd.PhaiThanhToanBaoHiem,0) as PhaiThanhToanBaoHiem,

			isnull(bh.TenDoiTuong,'') as TenBaoHiem,
			isnull(bh.MaDoiTuong,'') as MaBaoHiem,
			isnull(bh.DienThoai,'') as BH_SDT,
			iif(bhhd.ID_BaoHiem is null,'',tn.NguoiLienHeBH) as LienHeBaoHiem,
			iif(bhhd.ID_BaoHiem is null,'',tn.SoDienThoaiLienHeBH) as SoDienThoaiLienHeBaoHiem,
			isnull(tn.MaPhieuTiepNhan,'') as MaPhieuTiepNhan,
			isnull(xe.BienSo,'') as BienSo,
			iif(bhhd.ID_PhieuTiepNhan is null, '0','1') as LaHoaDonSuaChua,
			case bhhd.ChoThanhToan
				when 1 then N'Phiếu tạm' 
				when 0 then 
					case bhhd.YeuCau
						when '2' then  N'Đang giao hàng' 
						when '3' then  N'Hoàn thành' 
						else N'Đã lưu' end
				else  N'Đã hủy'
				end as TrangThai,
		 
			case bhhd.ChoThanhToan
				when 1 then N'Chờ duyệt'
				when 0 then 
					case bhhd.YeuCau
						when '2' then  N'Đang xử lý' 
						when '3' then  N'Hoàn thành' 
						else N'Đã duyệt' end
				else  N'Đã hủy'
			end as Gara_TrangThaiBG    	,
		
			case bhhd.ChoThanhToan
				when 1 then '1'
				when 0 then 
					case bhhd.YeuCau
						when 2 then  '2'
						when 3 then '3'
						else '0' end
				else '4' end as TrangThaiHD -- used to where
    		FROM
    		(
    			select 
    			b.ID,
				SUM(ISNULL(b.ThuTuThe, 0)) as ThuTuThe,
				SUM(ISNULL(b.TienMat, 0)) as TienMat,
				SUM(ISNULL(b.TienATM, 0)) as TienATM,
    			SUM(ISNULL(b.TienCK, 0)) as ChuyenKhoan,
    			SUM(ISNULL(b.KhachDaTra, 0)) as KhachDaTra

    			from
    			(
					-- get infor PhieuThu from HDDatHang (HuyPhieuThu (qhd.TrangThai ='0')
    				Select 
    					bhhd.ID,
						Case when qhd.TrangThai = 0 then 0 else Case when qhd.LoaiHoaDon = 11 then ISNULL(hdct.ThuTuThe, 0) else -ISNULL(hdct.ThuTuThe, 0) end end as ThuTuThe,
						case when qhd.TrangThai = 0 then 0 else case when qhd.LoaiHoaDon = 11 then ISNULL(hdct.TienMat, 0) else -ISNULL(hdct.TienMat, 0) end end as TienMat,
						case when qhd.TrangThai = 0 then 0 else case when qhd.LoaiHoaDon = 11 then case when TaiKhoanPOS = 1 then ISNULL(hdct.TienGui, 0) else 0 end else -ISNULL(hdct.TienGui, 0) end end as TienATM,							
						case when qhd.TrangThai = 0 then 0 else case when qhd.LoaiHoaDon = 11 then case when TaiKhoanPOS = 0 then ISNULL(hdct.TienGui, 0) else 0 end else -ISNULL(hdct.TienGui, 0) end end as TienCK,
    					Case when bhhd.ChoThanhToan is null OR qhd.TrangThai='0' then 0 else case when qhd.LoaiHoaDon = 11 then ISNULL(hdct.Tienthu, 0) else -ISNULL(hdct.Tienthu, 0) end end as KhachDaTra					
   				from BH_HoaDon bhhd
    				left join Quy_HoaDon_ChiTiet hdct on bhhd.ID = hdct.ID_HoaDonLienQuan
    				left join Quy_HoaDon qhd on hdct.ID_HoaDon = qhd.ID 	
					left join DM_TaiKhoanNganHang tk on tk.ID= hdct.ID_TaiKhoanNganHang					
    				where bhhd.LoaiHoaDon = '3'
					and bhhd.NgayLapHoadon >= @timeStart and bhhd.NgayLapHoaDon < @timeEnd 
					and exists (select ID from @tblChiNhanh cn where bhhd.ID_DonVi = cn.ID)
    
    				union all
					-- get infor PhieuThu/Chi from HDXuLy
    				Select
    					hdt.ID,
						Case when bhhd.ChoThanhToan is null or qhd.TrangThai='0' then 0 else Case when qhd.LoaiHoaDon = 11 then ISNULL(hdct.ThuTuThe, 0) else -ISNULL(hdct.ThuTuThe, 0) end end as ThuTuThe,		
						Case when bhhd.ChoThanhToan is null or qhd.TrangThai='0' then 0 else Case when qhd.LoaiHoaDon= 11 then ISNULL(hdct.TienMat, 0) else -ISNULL(hdct.TienMat, 0) end end as TienMat,			
						case when bhhd.ChoThanhToan is null or qhd.TrangThai='0' then 0 else case when qhd.LoaiHoaDon = 11 then case when TaiKhoanPOS = 1 then ISNULL(hdct.TienGui, 0) else 0 end else -ISNULL(hdct.TienGui, 0) end end as TienATM,
						case when bhhd.ChoThanhToan is null or qhd.TrangThai='0' then 0 else case when qhd.LoaiHoaDon = 11 then case when TaiKhoanPOS = 0 then ISNULL(hdct.TienGui, 0) else 0 end else -ISNULL(hdct.TienGui, 0) end end as TienCK,
  						Case when bhhd.ChoThanhToan is null or qhd.TrangThai='0' then (Case when qhd.LoaiHoaDon = 11 or qhd.TrangThai='0' then 0 else -ISNULL(hdct.Tienthu, 0) end)
    						else (Case when qhd.LoaiHoaDon = 11 then ISNULL(hdct.Tienthu, 0) else -ISNULL(hdct.Tienthu, 0) end) end as KhachDaTra
    				from BH_HoaDon bhhd
    				inner join BH_HoaDon hdt on (bhhd.ID_HoaDon = hdt.ID and hdt.ChoThanhToan = '0')
    				left join Quy_HoaDon_ChiTiet hdct on bhhd.ID = hdct.ID_HoaDonLienQuan
    				left join Quy_HoaDon qhd on (hdct.ID_HoaDon = qhd.ID)
					left join DM_TaiKhoanNganHang tk on tk.ID= hdct.ID_TaiKhoanNganHang		
    				where hdt.LoaiHoaDon = '3' 
					and bhhd.NgayLapHoadon >= @timeStart and bhhd.NgayLapHoaDon < @timeEnd 
					and exists (select ID from @tblChiNhanh cn where bhhd.ID_DonVi = cn.ID)
    			) b
    			group by b.ID 
    		) as a
    		inner join BH_HoaDon bhhd on a.ID = bhhd.ID
    		left join DM_DoiTuong dt on bhhd.ID_DoiTuong = dt.ID
			left join DM_DoiTuong bh on bhhd.ID_BaoHiem = bh.ID
    		left join DM_DonVi dv on bhhd.ID_DonVi = dv.ID
    		left join NS_NhanVien nv on bhhd.ID_NhanVien = nv.ID 
    		left join DM_TinhThanh tt on dt.ID_TinhThanh = tt.ID
    		left join DM_QuanHuyen qh on dt.ID_QuanHuyen = qh.ID
    		left join DM_GiaBan gb on bhhd.ID_BangGia = gb.ID
    		left join DM_ViTri vt on bhhd.ID_ViTri = vt.ID    
			left join Gara_PhieuTiepNhan tn on bhhd.ID_PhieuTiepNhan = tn.ID
			left join Gara_DanhMucXe xe on tn.ID_Xe= xe.ID	
			where bhhd.LoaiHoaDon = 3 
			and
			bhhd.NgayLapHoadon >= @timeStart and bhhd.NgayLapHoaDon < @timeEnd 
    		) as c
			where exists( select * from @tblNhanVien nv where nv.ID= c.ID_NhanVien or c.NguoiTaoHD= @NguoiTao)
			and exists( select Name from dbo.splitstring(@TrangThai) tt where  c.TrangThaiHD  = tt.Name)
			and exists (select Name from dbo.splitstring(@LaHoaDonSuaChua) tt where c.LaHoaDonSuaChua = tt.Name)
			and
				((select count(Name) from @tblSearch b where     			
				c.MaHoaDon like '%'+b.Name+'%'
				or c.NguoiTaoHD like '%'+b.Name+'%'
				or c.TenNhanVien like '%'+b.Name+'%'
				or c.TenNhanVienKhongDau like '%'+b.Name+'%'
				or c.DienGiai like '%'+b.Name+'%'
				or c.MaDoiTuong like '%'+b.Name+'%'		
				or c.TenDoiTuong like '%'+b.Name+'%'
				or c.TenDoiTuong_KhongDau like '%'+b.Name+'%'
				or c.DienThoai like '%'+b.Name+'%'	
				or c.MaPhieuTiepNhan like '%'+b.Name+'%'
				or c.BienSo like '%'+b.Name+'%'	
				)=@count or @count=0)	
		
    ),
		count_cte
		as (
			select count(ID) as TotalRow,
				CEILING(COUNT(ID) / CAST(@PageSize as float ))  as TotalPage,
				sum(TongTienHang) as SumTongTienHang,
				sum(TongGiamGia) as SumTongGiamGia,
					sum(TongChiPhi) as SumTongChiPhi,
				sum(KhachDaTra) as SumKhachDaTra,	
				sum(PhaiThanhToan) as SumPhaiThanhToan,			
				sum(TongThanhToan) as SumTongThanhToan,				
				sum(ThuTuThe) as SumThuTuThe,				
				sum(TienMat) as SumTienMat,
				sum(TienATM) as SumPOS,
				sum(ChuyenKhoan) as SumChuyenKhoan,				
				sum(TongTienThue) as SumTongTienThue,
				sum(ConNo) as SumConNo
			from data_cte
		)
		select dt.*, cte.*		
		from data_cte dt
		cross join count_cte cte	
		order by 
			case when @SortBy <> 'ASC' then 0
			when @ColumnSort='NgayLapHoaDon' then NgayLapHoaDon end ASC,
			case when @SortBy <> 'DESC' then 0
			when @ColumnSort='NgayLapHoaDon' then NgayLapHoaDon end DESC,
			case when @SortBy <>'ASC' then ''
			when @ColumnSort='MaHoaDon' then MaHoaDon end ASC,
			case when @SortBy <>'DESC' then ''
			when @ColumnSort='MaHoaDon' then MaHoaDon end DESC,
			case when @SortBy <>'ASC' then ''
			when @ColumnSort='MaKhachHang' then dt.MaDoiTuong end ASC,
			case when @SortBy <>'DESC' then ''
			when @ColumnSort='MaKhachHang' then dt.MaDoiTuong end DESC,
			case when @SortBy <>'DESC' then ''
			when @ColumnSort='MaPhieuTiepNhan' then dt.MaPhieuTiepNhan end DESC,
			case when @SortBy <>'ASC' then ''
			when @ColumnSort='MaPhieuTiepNhan' then dt.MaPhieuTiepNhan end ASC,
			case when @SortBy <> 'ASC' then 0
			when @ColumnSort='TongTienHang' then TongTienHang end ASC,
			case when @SortBy <> 'DESC' then 0
			when @ColumnSort='TongTienHang' then TongTienHang end DESC,
			case when @SortBy <>'ASC' then 0
			when @ColumnSort='GiamGia' then TongGiamGia end ASC,
			case when @SortBy <>'DESC' then 0
			when @ColumnSort='GiamGia' then TongGiamGia end DESC,
			case when @SortBy <>'ASC' then 0
			when @ColumnSort='KhachCanTra' then PhaiThanhToan end ASC,
			case when @SortBy <>'DESC' then 0
			when @ColumnSort='KhachCanTra' then PhaiThanhToan end DESC,
			case when @SortBy <>'ASC' then 0
			when @ColumnSort='KhachDaTra' then KhachDaTra end ASC,
			case when @SortBy <>'DESC' then 0
			when @ColumnSort='KhachDaTra' then KhachDaTra end DESC	
				
		OFFSET (@CurrentPage* @PageSize) ROWS
		FETCH NEXT @PageSize ROWS ONLY
END");

			Sql(@"ALTER PROCEDURE [dbo].[GetListHHSearch]
    @ID_ChiNhanh [nvarchar](max),
    @Search [nvarchar](max),
    @SearchCoDau [nvarchar](max),
	@ConTonKho int
AS
BEGIN
    	DECLARE @tablename TABLE(
		Name [nvarchar](max))
    	DECLARE @tablenameChar TABLE(
		Name [nvarchar](max))
    	DECLARE @count int
    	DECLARE @countChar int

    	INSERT INTO @tablename(Name) select  Name from [dbo].[splitstring](@Search+',') where Name!='';   
    	Select @count =  (Select count(*) from @tablename);

		select 
			ID_DonViQuiDoi,
			ID,
			MaHangHoa,
			TenHangHoa,
			ThuocTinh_GiaTri,
			TenDonViTinh,
			QuanLyTheoLoHang,
			TyLeChuyenDoi,
			LaHangHoa,
			GiaBan,
			GiaNhap,
			b.TonKho,
			SrcImage,
			ID_LoHang,
			MaLoHang,
			NgaySanXuat,
			NgayHetHan,
			QuyCach,
			iif(b.LaHangHoa ='1', b.GiaVon, dbo.GetGiaVonOfDichVu(@ID_ChiNhanh,b.ID_DonViQuiDoi)) as GiaVon,
			iif(b.LoaiHangHoa is null, iif(b.LaHangHoa='1',1,2),b.LoaiHangHoa) as LoaiHangHoa
		from
		(
		select tbl.*,			
			Case When gv.ID is null then 0 else CAST(ROUND(( gv.GiaVon), 0) as float) end as GiaVon,
			CAST(ROUND(ISNULL(hhtonkho.TonKho, 0), 3) as float) as TonKho,
			ISNULL(an.URLAnh,'/Content/images/iconbepp18.9/gg-37.png') as SrcImage
		from
		(
		select top 500
			dvqd1.ID as ID_DonViQuiDoi,
    		dhh1.ID,
			dvqd1.ID_HangHoa,
			dvqd1.MaHangHoa,
    		dhh1.TenHangHoa,
			dvqd1.ThuocTinhGiaTri as ThuocTinh_GiaTri,
    		dvqd1.TenDonViTinh,
			dhh1.QuanLyTheoLoHang,
			dvqd1.TyLeChuyenDoi,
			dhh1.LaHangHoa,
			dhh1.LoaiHangHoa,					
			dvqd1.GiaBan,
			dvqd1.GiaNhap,			
			lh1.MaLoHang,
    		lh1.NgaySanXuat,
			lh1.NgayHetHan,
			Case when lh1.ID is null then null else lh1.ID end as ID_LoHang,
			iif(dhh1.QuyCach is null or dhh1.QuyCach=0, 1, dhh1.QuyCach) as QuyCach
		from DonViQuiDoi dvqd1
		join DM_HangHoa dhh1 on dvqd1.ID_HangHoa = dhh1.ID
		left join DM_LoHang lh1 on dvqd1.ID_HangHoa = lh1.ID_HangHoa and (lh1.TrangThai = 1 or lh1.TrangThai is null)
		where dvqd1.Xoa = 0 
		and 
		((select count(*) from @tablename b where 
    	MaHangHoa like '%'+b.Name+'%' 
    	or dhh1.TenHangHoa_KhongDau like '%'+b.Name+'%' 
    	or dhh1.TenHangHoa_KyTuDau like '%'+b.Name+'%'
		or dhh1.TenHangHoa like '%'+b.Name+'%' 
		or lh1.MaLoHang like '%' + b.Name + '%'
    		)=@count or @count=0)
    )tbl
	LEFT join DM_HangHoa_Anh an on (tbl.ID_HangHoa = an.ID_HangHoa and (an.sothutu = 1 or an.ID is null))
	left join DM_GiaVon gv on tbl.ID_DonViQuiDoi = gv.ID_DonViQuiDoi 
		and (tbl.ID_LoHang = gv.ID_LoHang or tbl.ID_LoHang is null) and gv.ID_DonVi = @ID_ChiNhanh
	left join DM_HangHoa_TonKho hhtonkho on tbl.ID_DonViQuiDoi = hhtonkho.ID_DonViQuyDoi 
	and (hhtonkho.ID_LoHang = tbl.ID_LoHang or tbl.ID_LoHang is null) 
		and hhtonkho.ID_DonVi = @ID_ChiNhanh
	) b
	 where b.LaHangHoa = 0 or b.TonKho > iif(@ConTonKho=1, 0, -99999)
	order by NgayHetHan	
END");

			Sql(@"ALTER PROCEDURE [dbo].[getListXuatKho_Import]
    @MaHangHoa [nvarchar](max),
    @MaLohang [nvarchar](max),
    @SoLuong [float],
    @ID_ChiNhanh [uniqueidentifier]
AS
BEGIN
    SELECT 
    		dvqd3.ID as ID_DonViQuiDoi, 
    		Case When a.ID_LoHang is null then NEWID() else a.ID_LoHang end as ID_LoHang,
    		dvqd3.MaHangHoa,
    		a.TenHangHoa,
    		dvqd3.ThuocTinhGiaTri as ThuocTinh_GiaTri,
    		dvqd3.TenDonViTinh, 
    		a.QuanLyTheoLoHang,
			gv.ID,
    		Case when gv.ID is null then 0 else CAST(ROUND((gv.GiaVon), 0) as float) end  as GiaVon, 
    		CAST(ROUND((dvqd3.GiaBan), 0) as float) as GiaBan,  
    		CAST(ROUND((@SoLuong), 3) as float) as SoLuong,
    		CAST(ROUND((@SoLuong), 3) as float) as SoLuongXuatHuy,
    		Case when gv.ID is null then 0 else CAST(ROUND((@SoLuong * gv.GiaVon), 0) as float) end  as GiaTriHuy,  
    		CAST(ROUND(a.TonCuoiKy, 3) as float) as TonKho,
    		a.MaLoHang as TenLoHang,
    		a.NgaySanXuat,
    		a.NgayHetHan
    	FROM 
    		(
    		SELECT 
    		dvqd.ID as ID_DonViQuiDoi,
    		dhh.TenHangHoa As TenHangHoa,
    		dvqd.TenDonViTinh AS TenDonViTinh,
    		dhh.QuanLyTheoLoHang,
    		lh.ID As ID_LoHang,
    		Case when @MaLohang != '' then (Case when lh.MaLohang is null or dhh.QuanLyTheoLoHang = '0' then '' else lh.MaLoHang end) else '' end As MaLoHang,
    		Case when @MaLohang != '' then (lh.NgaySanXuat) else null end As NgaySanXuat,
    		Case when @MaLohang != '' then (lh.NgayHetHan) else null end As NgayHetHan,
    		lh.TrangThai,
    		ISNULL(HangHoa.TonCuoiKy,0) AS TonCuoiKy
    		FROM 
    		DonViQuiDoi dvqd 
    		left join
    		(
				Select ID_DonViQuyDoi as ID_DonViQuiDoi,
				Case when ID_LoHang is null then '10000000-0000-0000-0000-000000000001' else ID_LoHang end as ID_LoHang,
				TonKho as TonCuoiKy
				FROM DM_HangHoa_TonKho tk
				where tk.ID_DonVi = @ID_ChiNhanh
    		) AS HangHoa
    		on dvqd.ID = HangHoa.ID_DonViQuiDoi
    		INNER JOIN DM_HangHoa dhh ON dhh.ID = dvqd.ID_HangHoa
    		LEFT JOIN DM_NhomHangHoa dnhh ON dnhh.ID = dhh.ID_NhomHang
    		LEFT JOIN DM_LoHang lh on HangHoa.ID_LoHang = lh.ID
    		Where dvqd.Xoa = '0' and dhh.TheoDoi = 1
    		and lh.TrangThai = 1 or lh.TrangThai is null
		) a
    	LEFT Join DonViQuiDoi dvqd3 on a.ID_DonViQuiDoi = dvqd3.ID
    	LEFT Join DM_GiaVon gv on dvqd3.ID = gv.ID_DonViQuiDoi and (gv.ID_LoHang = a.ID_LoHang or a.ID_LoHang is null) and gv.ID_DonVi = @ID_ChiNhanh
    	Where dvqd3.MaHangHoa = @MaHangHoa
    	and a.MaLoHang = @MaLoHang
    order by a.NgayHetHan
END");

			Sql(@"ALTER PROCEDURE [dbo].[GetMaPhieuThuChiMax_byTemp]
    @LoaiHoaDon [int],
    @ID_DonVi [uniqueidentifier],
    @NgayLapHoaDon [datetime]
AS
BEGIN
    SET NOCOUNT ON;
    	DECLARE @Return float = 1
    	declare @lenMaMax int = 0
    	DECLARE @isDefault bit = (select SuDungMaChungTu from HT_CauHinhPhanMem where ID_DonVi= @ID_DonVi)-- co/khong thiet lap su dung Ma MacDinh
    	DECLARE @isSetup int = (select top 1 ID_LoaiChungTu from HT_MaChungTu where ID_LoaiChungTu = @LoaiHoaDon)
    
    	if @isDefault='1' and @isSetup is not null
    		begin
    			DECLARE @machinhanh varchar(15) = (select MaDonVi from DM_DonVi where ID= @ID_DonVi)
    			DECLARE @lenMaCN int = Len(@machinhanh)
    			DECLARE @isUseMaChiNhanh varchar(15) = (select SuDungMaDonVi from HT_MaChungTu where ID_LoaiChungTu=@LoaiHoaDon) -- co/khong su dung MaChiNhanh
    			DECLARE @kituphancach1 varchar(1) = (select KiTuNganCach1 from HT_MaChungTu where ID_LoaiChungTu=@LoaiHoaDon)
    			DECLARE @kituphancach2 varchar(1) = (select KiTuNganCach2 from HT_MaChungTu where ID_LoaiChungTu=@LoaiHoaDon)
    			DECLARE @kituphancach3 varchar(1) = (select KiTuNganCach3 from HT_MaChungTu where ID_LoaiChungTu=@LoaiHoaDon)
    			DECLARE @dinhdangngay varchar(8) = (select NgayThangNam from HT_MaChungTu where ID_LoaiChungTu=@LoaiHoaDon)
    			DECLARE @dodaiSTT INT = (select CAST(DoDaiSTT AS INT) from HT_MaChungTu where ID_LoaiChungTu=@LoaiHoaDon)
    			DECLARE @kihieuchungtu varchar(10) = (select MaLoaiChungTu from HT_MaChungTu where ID_LoaiChungTu=@LoaiHoaDon)
    			DECLARE @lenMaKiHieu int = Len(@kihieuchungtu);
    			DECLARE @namthangngay varchar(10) = convert(varchar(10), @NgayLapHoaDon, 112)
    			DECLARE @year varchar(4) = Left(@namthangngay,4)
    			DECLARE @date varchar(4) = right(@namthangngay,2)
    			DECLARE @month varchar(4) = substring(@namthangngay,5,2)
    			DECLARE @datecompare varchar(10)='';
    			
    			if	@isUseMaChiNhanh='0'
    				begin 
    					set @machinhanh=''
    					set @lenMaCN=0
    				end
    
    			if @dinhdangngay='ddMMyyyy'
    				set @datecompare = CONCAT(@date,@month,@year)
    			else	
    				if @dinhdangngay='ddMMyy'
    					set @datecompare = CONCAT(@date,@month,right(@year,2))
    				else 
    					if @dinhdangngay='MMyyyy'
    						set @datecompare = CONCAT(@month,@year)
    					else	
    						if @dinhdangngay='MMyy'
    							set @datecompare = CONCAT(@month,right(@year,2))
    						else
    							if @dinhdangngay='yyyyMMdd'
    								set @datecompare = CONCAT(@year,@month,@date)
    							else 
    								if @dinhdangngay='yyMMdd'
    									set @datecompare = CONCAT(right(@year,2),@month,@date)
    								else	
    									if @dinhdangngay='yyyyMM'
    										set @datecompare = CONCAT(@year,@month)
    									else	
    										if @dinhdangngay='yyMM'
    											set @datecompare = CONCAT(right(@year,2),@month)
    										else 
    											if @dinhdangngay='yyyy'
    												set @datecompare = @year
    
    			DECLARE @sMaFull varchar(50) = concat(@machinhanh,@kituphancach1,@kihieuchungtu,@kituphancach2, @datecompare, @kituphancach3)	    			
 
				-- neu thietlapchungtu (khongco chinhanh, ngaythang, kytu ngancach)
				if @sMaFull='SQPT'
					select @Return = MAX(CAST(dbo.udf_GetNumeric(RIGHT(MaHoaDon,LEN(MaHoaDon)- len(@sMaFull)))AS float))
    				from Quy_HoaDon where SUBSTRING(MaHoaDon, 1, len(@sMaFull)) = @sMaFull and CHARINDEX('_', MaHoaDon)=0
				else
					begin
						 -- lay STTmax
					set @Return = (select max(maxSTT)
								from
								(
								select MaHoaDon,
									CAST(dbo.udf_GetNumeric(RIGHT(MaHoaDon, LEN(MaHoaDon) -LEN (@sMaFull))) AS float) as maxSTT -- lay chuoi so ben phai
								from Quy_HoaDon 
								where MaHoaDon like @sMaFull +'%' 
								) a 
							)	
					
					end
					
    			-- lay chuoi 000
    			declare @stt int =0;
    			declare @strstt varchar (10) ='0'
    			while @stt < @dodaiSTT- 1
    				begin
    					set @strstt= CONCAT('0',@strstt)
    					SET @stt = @stt +1;
    				end 
    			declare @lenSst int = len(@strstt)
    			if	@Return is null 
    					select CONCAT(@sMaFull,left(@strstt,@lenSst-1),1) as MaxCode-- left(@strstt,@lenSst-1): bỏ bớt 1 số 0			
    			else 
    				begin
    					set @Return = @Return + 1
    					set @lenMaMax =  len(@Return)
    					select 
    						case when @lenMaMax = 1 then CONCAT(@sMaFull,left(@strstt,@lenSst-1),@Return)
    							when @lenMaMax = 2 then case when @lenSst - 2 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-2), @Return) else CONCAT(@sMaFull, @Return) end
    							when @lenMaMax = 3 then case when @lenSst - 3 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-3), @Return) else CONCAT(@sMaFull, @Return) end
    							when @lenMaMax = 4 then case when @lenSst - 4 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-4), @Return) else CONCAT(@sMaFull, @Return) end
    							when @lenMaMax = 5 then case when @lenSst - 5 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-5), @Return) else CONCAT(@sMaFull, @Return) end
    							when @lenMaMax = 6 then case when @lenSst - 6 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-6), @Return) else CONCAT(@sMaFull, @Return) end
    							when @lenMaMax = 7 then case when @lenSst - 7 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-7), @Return) else CONCAT(@sMaFull, @Return) end
    							when @lenMaMax = 8 then case when @lenSst - 8 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-8), @Return) else CONCAT(@sMaFull, @Return) end
    							when @lenMaMax = 9 then case when @lenSst - 9 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-9), @Return) else CONCAT(@sMaFull, @Return) end
    							when @lenMaMax = 10 then case when @lenSst - 10 > -1 then CONCAT(@sMaFull, left(@strstt,@lenSst-10), @Return) else CONCAT(@sMaFull, @Return) end
    						else '' end as MaxCode
    				end 
    		end
    	else
    		begin
    			declare @machungtu varchar(10) = (select top 1 MaLoaiChungTu from DM_LoaiChungTu where ID= @LoaiHoaDon)
    			declare @lenMaChungTu int= LEN(@machungtu)
    
    			select @Return = MAX(CAST(dbo.udf_GetNumeric(RIGHT(MaHoaDon,LEN(MaHoaDon)- @lenMaChungTu))AS float))
    			from Quy_HoaDon where SUBSTRING(MaHoaDon, 1, len(@machungtu)) = @machungtu and CHARINDEX('_', MaHoaDon)=0
    	
    			-- do dai STT (toida = 10)
    			if	@Return is null 
    					select
    						case when @lenMaChungTu = 2 then CONCAT(@machungtu, '00000000',1)
    							when @lenMaChungTu = 3 then CONCAT(@machungtu, '0000000',1)
    							when @lenMaChungTu = 4 then CONCAT(@machungtu, '000000',1)
    							when @lenMaChungTu = 5 then CONCAT(@machungtu, '00000',1)
    						else CONCAT(@machungtu,'000000',1)
    						end as MaxCode
    			else 
    				begin
    					set @Return = @Return + 1
    					set @lenMaMax = len(@Return)
    					select 
    						case when @lenMaMax = 1 then CONCAT(@machungtu,'000000000',@Return)
    							when @lenMaMax = 2 then CONCAT(@machungtu,'00000000',@Return)
    							when @lenMaMax = 3 then CONCAT(@machungtu,'0000000',@Return)
    							when @lenMaMax = 4 then CONCAT(@machungtu,'000000',@Return)
    							when @lenMaMax = 5 then CONCAT(@machungtu,'00000',@Return)
    							when @lenMaMax = 6 then CONCAT(@machungtu,'0000',@Return)
    							when @lenMaMax = 7 then CONCAT(@machungtu,'000',@Return)
    							when @lenMaMax = 8 then CONCAT(@machungtu,'00',@Return)
    							when @lenMaMax = 9 then CONCAT(@machungtu,'0',@Return)								
    						else CONCAT(@machungtu,CAST(@Return  as decimal(22,0))) end as MaxCode
    				end 
    		end
END");

			Sql(@"ALTER PROCEDURE [dbo].[GetNhatKyBaoDuong_byCar]
    @ID_Xe [uniqueidentifier]
AS
BEGIN
    SET NOCOUNT ON;
    
    		 select hd.MaHoaDon, 				
    				 hd.NgayLapHoaDon,
    				 qd.MaHangHoa,
    				 qd.TenDonViTinh,
    				hh.TenHangHoa,
    				dt.MaDoiTuong,
    				dt.TenDoiTuong,
    				ct.SoLuong,
    				ct.GhiChu,
    				lich.LanBaoDuong,
    				lich.SoKmBaoDuong,
    				lich.TrangThai,
    				case lich.TrangThai
    					when 0 then N'Đã hủy'
    					when 1 then N'Chưa xử lý'
    					when 2 then N'Đã xử lý'
    					when 3 then N'Đã nhắc'
    					when 4 then N'Đã hủy'
    					when 5 then N'Quá hạn'
    				end as sTrangThai,
    				nv.NVThucHiens
    		 from BH_HoaDon_ChiTiet ct	
    		  join BH_HoaDon hd on ct.ID_HoaDon= hd.ID
    		  join Gara_PhieuTiepNhan tn on hd.ID_PhieuTiepNhan= tn.ID
    		  left join DM_DoiTuong dt on hd.ID_DoiTuong= dt.ID
    		  join DonViQuiDoi qd on ct.ID_DonViQuiDoi = qd.ID 
    		  join DM_HangHoa hh on qd.ID_HangHoa= hh.ID
    		  join Gara_LichBaoDuong lich on ct.ID_LichBaoDuong= lich.ID
    		  left join 
    		  (
    			select distinct thout.ID_ChiTietHoaDon,
    			 (
    			  select distinct nv.TenNhanVien + ', ' as [text()]
    			  from BH_NhanVienThucHien th
    			  join NS_NhanVien nv on th.ID_NhanVien= nv.ID
    			  where th.ID_ChiTietHoaDon = thout.ID_ChiTietHoaDon
    			  for xml path('')
    			  ) NVThucHiens
    			  from BH_NhanVienThucHien thout
    		  ) nv on ct.ID = nv.ID_ChiTietHoaDon
    		  where (ct.ID_ChiTietDinhLuong is null or ct.ID_ChiTietDinhLuong = ct.ID)
    		  and hd.LoaiHoaDon= 25
    		  and hd.ChoThanhToan= 0
    		  and tn.ID_Xe= @ID_Xe
    		  and ct.ID_LichBaoDuong is not null
    		  order by hd.NgayLapHoaDon	desc, qd.MaHangHoa
END");

			Sql(@"ALTER PROCEDURE [dbo].[Insert_ThongBaoHetTonKho]
  @ID_ChiNhanh uniqueidentifier,
   @LoaiHoaDon int,
  @tblHoaDonChiTiet ChiTietHoaDonEdit readonly
AS
BEGIN
    SET NOCOUNT ON;
	

			select tk.ID_DonVi,
			qd.ID_HangHoa,
			tk.ID_DonViQuyDoi  as ID_DonViQuiDoi, tk.ID_LoHang, 
			tk.TonKho, 
			hh.TonToiDa / iif(qd.TyLeChuyenDoi= 0 or qd.TyLeChuyenDoi is null,1, qd.TyLeChuyenDoi) as TonToiDa,
			hh.TonToiThieu / iif(qd.TyLeChuyenDoi= 0 or qd.TyLeChuyenDoi is null,1, qd.TyLeChuyenDoi) as TonToiThieu,
			MaHangHoa,  lo.MaLoHang
		into #tblTKho
		from DM_HangHoa_TonKho tk
		join DonViQuiDoi qd on tk.ID_DonViQuyDoi = qd.ID
		join DM_HangHoa hh on qd.ID_HangHoa = hh.ID
		left join DM_LoHang lo on lo.ID_HangHoa= hh.ID and ((lo.ID = tk.ID_LoHang) or (lo.ID is null and tk.ID_LoHang is null))
		where tk.ID_DonVi = @ID_ChiNhanh
		and hh.LaHangHoa = 1
		and exists( select ID_DonViQuyDoi from @tblHoaDonChiTiet qd2 where qd2.ID_DonViQuiDoi= qd.ID)
		and (exists( select ID_LoHang from @tblHoaDonChiTiet lo2 where lo2.ID_LoHang= lo.ID) Or hh.QuanLyTheoLoHang= 0)  

		if  @LoaiHoaDon in (1, 7,8,10)
		begin
		insert into HT_ThongBao
    	select newid(), @ID_ChiNhanh,0, 
    	CONCAT(N'<p onclick=""loaddadoc(''key',''')""> Hàng hóa <a onclick=""loadthongbao(''1'', ''', MaHangHoa,''', ''key'')"">',   
		'<span class=""blue"">', MaHangHoa, ' </span>', N'</a> đã hết số lượng tồn kho. Vui lòng nhập thêm để tiếp tục kinh doanh </p>'),
    		GETDATE(),''
		from #tblTKho    
		where TonKho <= 0

		insert into HT_ThongBao
		select newid(), @ID_ChiNhanh,0, 
    		CONCAT(N'<p onclick=""loaddadoc(''key', ''')""> Hàng hóa <a onclick=""loadthongbao(''1'', ''', MaHangHoa, ''', ''key'')"">',
		'<span class=""blue"">', MaHangHoa, ' </span>', N'</a> sắp hết hàng trong kho. Vui lòng nhập thêm để tiếp tục kinh doanh </p>'),
    		GETDATE(),''
		from #tblTKho  
		where TonKho < TonToiThieu and TonKho > 0
		end


		if  @LoaiHoaDon = 4
		begin
			insert into HT_ThongBao

				select newid(), @ID_ChiNhanh,0, 
    			CONCAT(N'<p onclick=""loaddadoc(''key', ''')""> Hàng hóa <a onclick=""loadthongbao(''1'', ''', MaHangHoa, ''', ''key'')"">',
			'<span class=""blue"">', MaHangHoa, ' </span>', N'</a> đã vượt quá số lượng tồn kho quy định </p>'),
    			GETDATE(),''
			from #tblTKho    
			where TonKho > TonToiDa and TonToiDa > 0
		end

END");

			Sql(@"ALTER PROCEDURE [dbo].[JqAuto_PhieuTiepNhan]
    @IDChiNhanhs [nvarchar](max),
    @TextSearch [nvarchar](max),
    @CustomerID [nvarchar](max)
AS
BEGIN
    SET NOCOUNT ON;
    	select top 10 tn.ID, tn.MaPhieuTiepNhan, xe.BienSo, tn.ID_KhachHang,		
		tn.ID_BaoHiem, 
		tn.ID_Xe,
		bh.TenDoiTuong as TenBaoHiem,
		tn.SoDienThoaiLienHeBH, tn.NguoiLienHeBH
    	from Gara_PhieuTiepNhan tn
    	join Gara_DanhMucXe xe on tn.ID_Xe= xe.ID	
		left join DM_DoiTuong bh on tn.ID_BaoHiem= bh.ID
		left join DM_DoiTuong kh on tn.ID_KhachHang= kh.ID
    	where tn.ID_KhachHang like @CustomerID	
    	and tn.TrangThai !=0
    	and tn.NgayXuatXuong is null
    	and exists (select Name from dbo.splitstring(@IDChiNhanhs) dv where dv.Name= tn.ID_DonVi )
    	and (tn.MaPhieuTiepNhan like @TextSearch 
    		or xe.BienSo like @TextSearch 		
    		)
    order by tn.NgayVaoXuong desc
END");

			Sql(@"ALTER PROCEDURE [dbo].[SP_GetHDDebit_ofKhachHang]
    @ID_DoiTuong [nvarchar](max),
    @ID_DonVi [nvarchar](max),
	@LoaiDoiTuong int
AS
BEGIN
	if @ID_DonVi='00000000-0000-0000-0000-000000000000'
		begin
			set @ID_DonVi = (select CAST(ID as varchar(40)) + ',' as  [text()] from DM_DonVi  where TrangThai is null or TrangThai='1' for xml path(''))	
			set @ID_DonVi= left(@ID_DonVi, LEN(@ID_DonVi) -1) -- remove last comma ,
		end
		-- get hoadon all chinhanh

		select *
		from
		(
		select *
		from
		(
			select 
			a.ID, a.MaHoaDon, a.NgayLapHoaDon,	a.LoaiHoaDon,
			a.TongThanhToan,
			a.TongTienThue,
			TinhChietKhauTheo,
			iif(@LoaiDoiTuong=3, a.PhaiThanhToanBaoHiem,a.PhaiThanhToan) - ISNULL(b.TongThanhToan,0) as PhaiThanhToan
			from
			(
		select hd.ID, hd.MaHoaDon, hd.NgayLapHoaDon, hd.LoaiHoaDon,
				hd.TongTienThue,
				hd.TongThanhToan,
    			ISNULL(hd.PhaiThanhToan,0) - ISNULL(hdt.PhaiThanhToan,0) as PhaiThanhToan,
				ISNULL(hd.PhaiThanhToanBaoHiem,0) - ISNULL(hdt.PhaiThanhToanBaoHiem,0) as PhaiThanhToanBaoHiem,
    			ISNULL(TinhChietKhauTheo,1) as TinhChietKhauTheo
    		from BH_HoaDon hd
    		left join BH_HoaDon hdt on hd.ID_HoaDon= hdt.ID and hdt.LoaiHoaDon= 6
    		left join 
    				(select ID_HoaDon, min(TinhChietKhauTheo) as TinhChietKhauTheo
    				from BH_NhanVienThucHien nvth
    				where nvth.ID_HoaDon is not null
    				group by ID_HoaDon) tblNV on hd.ID = tblNV.ID_HoaDon
    		where 
			exists (select Name from dbo.splitstring(@ID_DonVi) where Name= hd.ID_DonVi)
			and iif(@LoaiDoiTuong=3, hd.ID_BaoHiem, hd.ID_DoiTuong) like @ID_DoiTuong		
    		and hd.LoaiHoaDon in (1,19,4,22, 25)
    		and hd.ChoThanhToan='0' 
			) a
			left join
			(
			-- get hoadon trahang of khachhang
			select
				hd.ID_HoaDon, hd.TongThanhToan
			from BH_HoaDon hd
			where hd.ID_DoiTuong like @ID_DoiTuong
			and hd.ID_HoaDon is not null
			and hd.LoaiHoaDon= 6		
			) b on a.ID= b.ID_HoaDon
		) tbl
		--order by a.NgayLapHoaDon desc

		union all

		select 
			cp.ID_HoaDon, hd.MaHoaDon, hd.NgayLapHoaDon,hd.LoaiHoaDon,
			sum(cp.ThanhTien) as TongThanhToan,
			0 as TongTienThue,
			0 as TinhChietKhauTheo,
			sum(cp.ThanhTien) as PhaiThanhToan
		from BH_HoaDon_ChiPhi cp
		join BH_HoaDon hd on cp.ID_HoaDon = hd.ID
		where hd.ChoThanhToan= 0
		and cp.ID_NhaCungCap= @ID_DoiTuong
		group by cp.ID_HoaDon, hd.MaHoaDon, hd.NgayLapHoaDon,	hd.LoaiHoaDon
		)tblView order by NgayLapHoaDon desc
END");

			Sql(@"ALTER PROCEDURE [dbo].[SP_GetQuyHoaDon_ofDoiTuong]
    @ID_DoiTuong [nvarchar](max),
    @ID_DonVi [nvarchar](max)
AS
BEGIN
SET NOCOUNT ON;
	if @ID_DonVi='00000000-0000-0000-0000-000000000000'
		begin
			set @ID_DonVi = (select CAST(ID as varchar(40)) + ',' as  [text()] from DM_DonVi  where TrangThai is null or TrangThai='1' for xml path(''))	
			set @ID_DonVi= left(@ID_DonVi, LEN(@ID_DonVi) -1) -- remove last comma ,
		end

	declare @LoaiDoiTuong int = (select LoaiDoiTuong from DM_DoiTuong where ID= @ID_DoiTuong)

		select tbl.ID_HoaDonLienQuan, 
			sum(tbl.TongTienThu + tbl.ThuDatHang) as TongTienThu
		from
		(
		
		select ID_HoaDonLienQuan,
			sum(TongTienThu) as TongTienThu,
			0 as ThuDatHang

		from
		(		-- thutien hoadon
				select ct.ID_HoaDonLienQuan,
    				case when hd.LoaiHoaDon = 6 or hd.LoaiHoaDon = 4 then sum(ISNULL(ct.TienThu,0)) else 
    				case when qhd.LoaiHoaDon = 11 or @LoaiDoiTuong = 2 then sum(ct.TienThu) else sum(ISNULL(-ct.TienThu,0)) end end TongTienThu    			
    			from Quy_HoaDon_ChiTiet ct
    			join Quy_HoaDon qhd on ct.ID_HoaDon = qhd.ID
    			left join BH_HoaDon hd on ct.ID_HoaDonLienQuan = hd.ID
    			where ct.ID_DoiTuong like @ID_DoiTuong 
				--and exists (select Name from dbo.splitstring(@ID_DonVi) where Name= qhd.ID_DonVi)
    			and (TrangThai is  null or TrangThai = '1' ) 
				and hd.LoaiHoaDon !=3
    			group by ct.ID_HoaDonLienQuan, hd.LoaiHoaDon,TrangThai, qhd.LoaiHoaDon
		) quy group by ID_HoaDonLienQuan
		
		union all

			select thuDH.ID_HoaDonMua, 
				0 as TongTienThu,
				thuDH.ThuDatHang
			from
			(
			-- neu hd xuly tu dathang --> lay phieuthu dathang
			select tblDH.ID_HoaDonMua,
					sum(tblDH.TienThu) as ThuDatHang,		
					ROW_NUMBER() OVER(PARTITION BY tblDH.ID ORDER BY tblDH.NgayLapHoaDon ASC) AS isFirst	--- group by hdDat, sort by ngaylap hdxuly
				from
				(			
						select hd.ID as ID_HoaDonMua, hd.NgayLapHoaDon,		
							hdd.ID,
							iif(qhd.LoaiHoaDon = 11, qct.TienThu, -qct.TienThu) as TienThu			
						from Quy_HoaDon_ChiTiet qct
						join Quy_HoaDon qhd on qct.ID_HoaDon = qhd.ID					
						join BH_HoaDon hdd on hdd.ID= qct.ID_HoaDonLienQuan
						join BH_HoaDon hd on hd.ID_HoaDon= hdd.ID
						where hdd.LoaiHoaDon = 3 	
						and hd.ChoThanhToan = 0 and hdd.ChoThanhToan='0' 
						and (qhd.TrangThai= 1 Or qhd.TrangThai is null)
						and hd.ID_DoiTuong like @ID_DoiTuong 
						) tblDH group by tblDH.ID_HoaDonMua, tblDH.ID,tblDH.NgayLapHoaDon
				) thuDH where thuDH.isFirst= 1
		)tbl group by tbl.ID_HoaDonLienQuan  
 
END");

			Sql(@"ALTER PROCEDURE [dbo].[update_DanhMucHangHoa]
AS
BEGIN
SET NOCOUNT ON;
    update DM_HangHoa set TenKhac = null
	update DM_HangHoa set ID_NhomHang = '00000000-0000-0000-0000-000000000000' where ID_NhomHang is null and LaHangHoa = 1
	update DM_HangHoa set ID_NhomHang = '00000000-0000-0000-0000-000000000001' where ID_NhomHang is null and LaHangHoa = 0
	exec insert_TonKhoKhoiTaoByInsert;
	If EXISTS(select * from BH_HoaDon_ChiTiet ct join BH_HoaDon bh on bh.ID = ct.ID_HoaDon where bh.SoLanIn = -9 and bh.ChoThanhToan = '0')
	BEGIN
		UPDATE hdkkupdate
    	SET hdkkupdate.TongTienHang = dshoadonkkupdate.SoLuongGiam, hdkkupdate.TongGiamGia = dshoadonkkupdate.SoLuongLech, hdkkupdate.TongChiPhi = dshoadonkkupdate.SoLuongTang
    	FROM BH_HoaDon AS hdkkupdate
    	INNER JOIN
    	(SELECT ct.ID_HoaDon, SUM(CASE WHEN ct.SoLuong > 0 THEN ct.SoLuong ELSE 0 END) AS SoLuongTang,
    	SUM(CASE WHEN ct.SoLuong < 0 THEN ct.SoLuong ELSE 0 END) AS SoLuongGiam, SUM(SoLuong) AS SoLuongLech FROM BH_HoaDon_ChiTiet ct
    	join BH_HoaDon hd on ct.ID_HoaDon = hd.ID and hd.SoLanIn = -9 and hd.ChoThanhToan = '0' GROUP BY ct.ID_HoaDon) AS dshoadonkkupdate
    	ON hdkkupdate.ID = dshoadonkkupdate.ID_HoaDon;
	END
	else
	BEGIN 
		Delete from BH_HoaDon where SoLanIn = -9 and ChoThanhToan = '0';
	END
	update BH_HoaDon set SoLanIn = NULL where SoLanIn = -9 and ChoThanhToan = '0'
	Delete from BH_HoaDon_ChiTiet where ID_HoaDon in (Select ID from BH_HoaDon where SoLanIn = -9 and ChoThanhToan = '1')
	Delete from BH_HoaDon where SoLanIn = -9 and ChoThanhToan = '1'
END");

			Sql(@"ALTER PROCEDURE [dbo].[UpdateChiTietKiemKe_WhenEditCTHD]
    @IDHoaDonInput [uniqueidentifier],
    @IDChiNhanhInput [uniqueidentifier],
    @NgayLapHDMin [datetime]
AS
BEGIN
    SET NOCOUNT ON;
    	declare @tblCTHD ChiTietHoaDonEdit
    		INSERT INTO @tblCTHD
    		SELECT 
    			qd.ID_HangHoa, ct.ID_LoHang, ct.ID_DonViQuiDoi, qd.TyLeChuyenDoi
    		FROM BH_HoaDon_ChiTiet ct
    		INNER JOIN BH_HoaDon hd ON hd.ID = ct.ID_HoaDon			
    		INNER JOIN DonViQuiDoi qd ON qd.ID = ct.ID_DonViQuiDoi			
    		INNER JOIN DM_HangHoa hh on hh.ID = qd.ID_HangHoa    		
    		WHERE hd.ID = @IDHoaDonInput AND hh.LaHangHoa = 1
    		GROUP BY qd.ID_HangHoa,ct.ID_DonViQuiDoi,qd.TyLeChuyenDoi, ct.ID_LoHang, hd.ID_DonVi, hd.ID_CheckIn, hd.YeuCau, hd.NgaySua, hd.NgayLapHoaDon;	
    
    	-- get cthd KiemKe (LoaiHoaDon = 9) can update
    DECLARE @ChiTietHoaDonUpdate TABLE (IDHoaDon UNIQUEIDENTIFIER,ID_ChiTietHoaDon UNIQUEIDENTIFIER, NgayLapHoaDon DATETIME, SoThuThu INT, SoLuong FLOAT, 
    TyLeChuyenDoi FLOAT, TonKho FLOAT, IDDonViQuiDoi UNIQUEIDENTIFIER, ID_LoHang UNIQUEIDENTIFIER);
    INSERT INTO @ChiTietHoaDonUpdate
    select hd.ID AS ID_HoaDon, cthd.ID AS ID_ChiTietHoaDon, 
    		hd.NgayLapHoaDon, cthd.SoThuTu, cthd.SoLuong, qd.TyLeChuyenDoi,
    	[dbo].[FUNC_TonLuyKeTruocThoiGian](@IDChiNhanhInput, hh.ID, cthd.ID_LoHang, hd.NgayLapHoaDon) AS TonKho, qd.ID, cthd.ID_LoHang
    	FROM BH_HoaDon hd
    INNER JOIN BH_HoaDon_ChiTiet cthd ON hd.ID = cthd.ID_HoaDon    	
    INNER JOIN DonViQuiDoi qd ON cthd.ID_DonViQuiDoi = qd.ID    	
    INNER JOIN DM_HangHoa hh on hh.ID = qd.ID_HangHoa    
    INNER JOIN @tblCTHD cthdthemmoiupdate ON cthdthemmoiupdate.ID_HangHoa = hh.ID    	
    WHERE hd.ChoThanhToan = 0 AND hd.LoaiHoaDon = 9 
    		and hd.ID_DonVi = @IDChiNhanhInput and hd.NgayLapHoaDon >= @NgayLapHDMin
    		AND (cthd.ID_LoHang = cthdthemmoiupdate.ID_LoHang OR cthdthemmoiupdate.ID_LoHang IS NULL) 
    
    	-- UPDATE KIEM KE
    	-- update TonDauKy(TienChietKhau), SoLuongLech(SoLuong), GiaTriLech(ThanhToan) in ctkiemke
    	update ctkiemke
    	SET ctkiemke.TienChietKhau = TonDauKy, 
    		ctkiemke.SoLuong = ctkiemke.ThanhTien - TonDauKy,
    		ctkiemke.ThanhToan = ctkiemke.GiaVon * (ctkiemke.ThanhTien - TonDauKy) --- gtrilech = soluonglech * giavon
    	FROM BH_HoaDon_ChiTiet ctkiemke
    	join 
    		(select ID_ChiTietHoaDon, TonKho/TyLeChuyenDoi as TonDauKy
    		from @ChiTietHoaDonUpdate) ctupdate
    		on ctkiemke.ID = ctupdate.ID_ChiTietHoaDon
    
    	-- update SoLuongLech tang/giam(TongChiPhi/TongTienHang), SoLuongLech(TongGiamGia), GiaTriLech tang/giam (PhaiThanhToan/TongChietKhau) - todo
    UPDATE hdkkupdate
    SET hdkkupdate.TongTienHang = dshoadonkkupdate.SoLuongGiam, 
    		hdkkupdate.TongGiamGia = dshoadonkkupdate.SoLuongLech,
    		hdkkupdate.TongChiPhi = dshoadonkkupdate.SoLuongTang
    FROM BH_HoaDon AS hdkkupdate
    INNER JOIN
    	(SELECT ct.ID_HoaDon, 
    			SUM(CASE WHEN ct.SoLuong > 0 THEN ct.SoLuong ELSE 0 END) AS SoLuongTang,
    		SUM(CASE WHEN ct.SoLuong < 0 THEN ct.SoLuong ELSE 0 END) AS SoLuongGiam, 
    			SUM(SoLuong) AS SoLuongLech
    		FROM BH_HoaDon_ChiTiet ct
    	INNER JOIN (SELECT IDHoaDon, IDDonViQuiDoi, ID_LoHang FROM @ChiTietHoaDonUpdate) AS KKHoaDon
    	ON ct.ID_HoaDon = KKHoaDon.IDHoaDon AND ct.ID_DonViQuiDoi = KKHoaDon.IDDonViQuiDoi AND (ct.ID_LoHang = KKHoaDon.ID_LoHang OR KKHoaDon.ID_LoHang IS NULL) 
    		GROUP BY ct.ID_HoaDon) AS dshoadonkkupdate
    ON hdkkupdate.ID = dshoadonkkupdate.ID_HoaDon;
END");

            Sql(@"ALTER PROCEDURE [dbo].[ValueCard_ServiceUsed]
    @ID_ChiNhanhs [nvarchar](max),
    @TextSearch [nvarchar](max),
    @DateFrom [nvarchar](14),
    @DateTo [nvarchar](14),
    @Status [nvarchar](14),
    @CurrentPage [int],
    @PageSize [int]
AS
BEGIN
    SET NOCOUNT ON;   
	declare @tblChiNhanh table (ID_Donvi uniqueidentifier)
	insert into @tblChiNhanh
	select name from dbo.splitstring(@ID_ChiNhanhs)

    		DECLARE @tblSearchString TABLE (Name [nvarchar](max));
    		INSERT INTO @tblSearchString(Name) select  Name from [dbo].[splitstringByChar](@TextSearch, ' ') where Name!='';
    		DECLARE @count int =  (Select count(*) from @tblSearchString);
    	
    		select hd.ID as ID_HoaDon,tblq.ID_HoaDon as ID_PhieuThuChi, hd.MaHoaDon,tblq.NgayLapHoaDon,ISNULL(dt.MaDoiTuong,'') as MaDoiTuong, ISNULL(dt.TenDoiTuong, N'Khách lẻ') as TenDoiTuong, 
    	qd.MaHangHoa,hh.TenHangHoa,ct.SoLuong, ct.DonGia, ct.TienChietKhau, ct.ThanhTien,  ISNULL(tblq.PhatSinhGiam,0) as PhatSinhGiam, ISNULL(tblq.PhatSinhTang,0) as PhatSinhTang, tblq.MaHoaDon as MaPhieuThu,		
    		case hd.LoaiHoaDon
    			when 1 then N'Bán hàng'
    			when 3 then N'Đặt hàng'
    			when 6 then N'Trả hàng'
    			when 19 then N'Gói dịch vụ'
    			when 25 then N'Sửa chữa'
    		else '' end as SLoaiHoaDon
    	from BH_HoaDon hd
    	join BH_HoaDon_ChiTiet ct on hd.id= ct.id_hoadon
    	left join DM_DoiTuong dt on hd.ID_DoiTuong = dt.ID
    	join DonViQuiDoi qd on ct.id_donviquidoi= qd.id
    	join DM_HangHoa hh on qd.ID_HangHoa = hh.ID
    	join (select qct.ID_HoaDonLienQuan, MaHoaDon, NgayLapHoaDon, qct.ID_HoaDon,
    				case when qhd.LoaiHoaDon = 11 then SUM(ISNULL(qct.ThuTuThe ,0)) end as PhatSinhGiam,
    				case when qhd.LoaiHoaDon = 12 then SUM(ISNULL(qct.ThuTuThe ,0)) end as PhatSinhTang
    		from Quy_HoaDon_Chitiet qct 
    		join Quy_HoaDon qhd on qct.ID_HoaDon = qhd.ID
    		where qhd.TrangThai ='1' 
    			and qct.HinhThucThanhToan=4
    		and FORMAT(qhd.NgayLapHoaDon,'yyyy-MM-dd') >=@DateFrom
    		and FORMAT(qhd.NgayLapHoaDon,'yyyy-MM-dd') <= @DateTo
    		group by qct.ID_HoaDonLienQuan, qct.ID_HoaDon, qhd.MaHoaDon, qhd.NgayLapHoaDon, qhd.LoaiHoaDon) tblq on hd.ID= tblq.ID_HoaDonLienQuan
    	where hd.LoaiHoaDon in ( 1,3,6,19,25) 
    	and hd.ChoThanhToan ='0'
		and exists (select cn.ID_DonVi from @tblChiNhanh cn where hd.ID_DonVi= cn.ID_Donvi)
    	and (ct.ID_ChiTietDinhLuong is null or ct.ID_ChiTietDinhLuong = ct.ID)	
    		order by hd.NgayLapHoaDon desc
END");

            CreateStoredProcedure(name: "[dbo].[BaoCaoGoiDV_GetCTMua]", parametersAction: p => new
            {
                IDChiNhanhs = p.String(),
                DateFrom = p.DateTime(),
                DateTo = p.DateTime()
            }, body: @"SET NOCOUNT ON;

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
		Case when hd.TongTienHang = 0 
		then 0 else ct.ThanhTien * ((hd.TongGiamGia + hd.KhuyeMai_GiamGia) / iif(hd.TongTienHang=0,1, hd.TongTienHang)) end as GiamGiaHD
	from BH_HoaDon hd
	join BH_HoaDon_ChiTiet ct on hd.ID = ct.ID_HoaDon
	where hd.LoaiHoaDon = 19
	and hd.ChoThanhToan=0
	and exists (select cn.ID_DonVi from @tblChiNhanh cn where cn.ID_DonVi= hd.ID_DonVi)
	and hd.NgayLapHoaDon between @DateFrom and @DateTo
	and (ct.ID_ChiTietDinhLuong is null or ct.ID_ChiTietDinhLuong= ct.ID)
	and (ct.ID_ParentCombo is null or ct.ID_ParentCombo!= ct.ID)");

            CreateStoredProcedure(name: "[dbo].[BCBanHang_GetChiPhi]", parametersAction: p => new
            {
                IDChiNhanhs = p.String(),
                DateFrom = p.DateTime(),
                DateTo = p.DateTime(),
                LoaiChungTus = p.String()
            }, body: @"SET NOCOUNT ON;

	DECLARE @tblChiNhanh TABLE(ID UNIQUEIDENTIFIER)
    INSERT INTO @tblChiNhanh
    select Name from splitstring(@IDChiNhanhs);
    
    DECLARE @tblLoaiHoaDon TABLE(LoaiHoaDon int)
    INSERT INTO @tblLoaiHoaDon
    select Name from splitstring(@LoaiChungTus);

	select 		
		tbl.ID_ParentCombo,		
		ct.ID_DonViQuiDoi,
		tbl.ChiPhi,
		tbl.ID_NhanVien, 
		tbl.ID_DoiTuong		
	from
	(
		select 
			cpCT.ID_ParentCombo,
			cpCT.ID_NhanVien, 
			cpCT.ID_DoiTuong,				
			sum(cpCT.ThanhTien) as ChiPhi
		from
		(
			select 
				iif(ct.ID_ParentCombo is null, ct.ID, ct.ID_ParentCombo) as ID_ParentCombo, 						
				cp.ThanhTien,
				cp.ID_NhanVien, cp.ID_DoiTuong
			from
			(
				select cp.*,
					hd.MaHoaDon, hd.NgayLapHoaDon,
					hd.ID_NhanVien, hd.ID_DoiTuong					
				from BH_HoaDon_ChiPhi cp
				join BH_HoaDon hd on cp.ID_HoaDon = hd.ID
				where hd.ChoThanhToan= 0
				and hd.NgayLapHoaDon between @DateFrom and @DateTo
				and exists (select ID from @tblChiNhanh cn where cn.ID = hd.ID_DonVi)
				and exists (select LoaiHoaDon from @tblLoaiHoaDon loai where loai.LoaiHoaDon = hd.LoaiHoaDon)
		   ) cp
		   join BH_HoaDon_ChiTiet ct on cp.ID_HoaDon_ChiTiet = ct.ID
		) cpCT group by cpCT.ID_ParentCombo,cpCT.ID_NhanVien, cpCT.ID_DoiTuong			
	)tbl
	join BH_HoaDon_ChiTiet ct on tbl.ID_ParentCombo = ct.ID");

            CreateStoredProcedure(name: "[dbo].[ChangePTN_updateCus]", parametersAction: p => new
            {
                ID_PhieuTiepNhan = p.Guid(),
                ID_KhachHangOld = p.Guid(),
                ID_BaoHiemOld = p.Guid(),
                Types = p.String(20)
            }, body: @"SET NOCOUNT ON;

	declare @tblType table(Loai int)
	insert into @tblType select name from dbo.splitstring(@Types)

	---- get PTN new
	declare @PTNNew_IDCusNew uniqueidentifier, @PTNNew_BaoHiem uniqueidentifier
	select @PTNNew_IDCusNew = ID_KhachHang, @PTNNew_BaoHiem = ID_BaoHiem from Gara_PhieuTiepNhan where ID= @ID_PhieuTiepNhan

	---- get list hoadon of PTN
	select ID, ID_DoiTuong, ID_BaoHiem
	into #tblHoaDon
	from BH_HoaDon
	where ID_PhieuTiepNhan = @ID_PhieuTiepNhan
	and ChoThanhToan =0
	and LoaiHoaDon in (3,25)

	---- update cus
	if (select count(*) from @tblType where Loai= '1') > 0
	begin
		update hd set ID_DoiTuong= @PTNNew_IDCusNew
		from BH_HoaDon hd
		join #tblHoaDon hdCheck on hd.ID= hdCheck.ID
		where hdCheck.ID_DoiTuong = @ID_KhachHangOld

		if (select count(*) from @tblType where Loai= '3') > 0
		update qct set ID_DoiTuong= @PTNNew_IDCusNew
		from Quy_HoaDon_ChiTiet qct
		join #tblHoaDon hdCheck on qct.ID_HoaDonLienQuan= hdCheck.ID
		where hdCheck.ID_DoiTuong = @ID_KhachHangOld
	end
  
	---- update baohiem
	if (select count(*) from @tblType where Loai= '2') > 0
	begin
		update hd set ID_BaoHiem= @PTNNew_BaoHiem
		from BH_HoaDon hd
		join #tblHoaDon hdCheck on hd.ID= hdCheck.ID
		where hdCheck.ID_BaoHiem = @ID_BaoHiemOld

		if (select count(*) from @tblType where Loai= '3') > 0
			update qct set ID_DoiTuong= @PTNNew_BaoHiem
			from Quy_HoaDon_ChiTiet qct
			join #tblHoaDon hdCheck on qct.ID_HoaDonLienQuan= hdCheck.ID
			where hdCheck.ID_BaoHiem = @ID_BaoHiemOld
	end");

            CreateStoredProcedure(name: "[dbo].[CTHD_GetChiPhiDichVu]", parametersAction: p => new
            {
                IDHoaDons = p.String(),
                IDVendors = p.String()
            }, body: @"SET NOCOUNT ON;
	declare @sql nvarchar(max) ='', @where nvarchar(max), @tblDefined nvarchar(max) ='',
	@paramDefined nvarchar(max) ='@IDHoaDons_In nvarchar(max), @IDVendors_In nvarchar(max)'

	set @where=' where 1 = 1 and (cthd.ID_ParentCombo is null or cthd.ID_ParentCombo != cthd.ID)
	   and (cthd.ID_ChiTietDinhLuong is null or cthd.ID_ChiTietDinhLuong = cthd.ID)	'

	if isnull(@IDHoaDons,'')!=''
		begin
			set @tblDefined = concat(@tblDefined, ' declare @tblHoaDon table (ID uniqueidentifier)
			insert into @tblHoaDon select name from dbo.splitstring(@IDHoaDons_In)')
			set @where = concat(@where,' and exists (select hd2.ID from @tblHoaDon hd2 where hd.ID = hd2.ID)') 
		end
	if isnull(@IDVendors,'')!=''
		begin
			set @tblDefined = concat(@tblDefined, ' declare @tblVendor table (ID uniqueidentifier)
			insert into @tblVendor select name from dbo.splitstring(@IDVendors_In)')
			set @where =concat(@where, ' and exists (select ncc.ID from @tblVendor ncc where cp.ID_NhaCungCap = ncc.ID)' )
		end

	set @sql= CONCAT(N'
		select 
			iif(cp.ID is null, ''00000000-0000-0000-0000-000000000000'',cp.ID) as ID,	
			qd.MaHangHoa,
			qd.TenDonViTinh,
			cthd.ID_DonViQuiDoi,	
			cthd.DonGia as GiaBan,
			cp.ID_NhaCungCap,
			iif(cp.ID_HoaDon_ChiTiet is null, cthd.ID,cp.ID_HoaDon_ChiTiet) as ID_HoaDon_ChiTiet,
			iif(cp.ID_HoaDon is null, cthd.ID_HoaDon,cp.ID_HoaDon) as ID_HoaDon,
			dt.DienThoai,
			dt.MaDoiTuong as MaNhaCungCap,
			dt.TenDoiTuong as TenNhaCungCap,
			iif(cp.SoLuong is null, cthd.SoLuong,cp.SoLuong) as SoLuong,
			iif(cp.DonGia is null, 0,cp.DonGia) as DonGia,			
			iif(cp.ThanhTien is null, 0,cp.ThanhTien) as ThanhTien,
			xe.BienSo,
			hd.ChiPhi as TongChiPhi,
			hd.NgayLapHoaDon,
			hd.MaHoaDon,
			cthd.Soluong as SoLuongHoaDon, --- soluong max
			iif(cthd.TenHangHoaThayThe is null or cthd.TenHangHoaThayThe ='''', hh.TenHangHoa, cthd.TenHangHoaThayThe) as TenHangHoaThayThe,
			iif(hh.LoaiHangHoa is null, iif(hh.LaHangHoa =''1'',1,2), hh.LoaiHangHoa) as LoaiHangHoa
		from BH_HoaDon_ChiTiet cthd
		join BH_HoaDon hd on cthd.ID_HoaDon= hd.ID
		left join BH_HoaDon_ChiPhi cp on cthd.ID= cp.ID_HoaDon_ChiTiet
		left join DonViQuiDoi qd on cthd.ID_DonViQuiDoi= qd.ID
	   left join DM_HangHoa hh on qd.ID_HangHoa= hh.ID
	   left join DM_DoiTuong dt on cp.ID_NhaCungCap= dt.ID
	   left join Gara_DanhMucXe xe on hd.ID_Xe= xe.ID
	   ', @where,
	 ' order by qd.MaHangHoa ')

	 set @sql = concat(@tblDefined, @sql)

	 exec sp_executesql @sql, @paramDefined,
		@IDHoaDons_In = @IDHoaDons,
		@IDVendors_In = @IDVendors");

			CreateStoredProcedure(name: "[dbo].[GetChiPhiDichVu_byVendor]", parametersAction: p => new
			{
				IDChiNhanhs = p.String(),
				ID_NhaCungCap = p.String(),
				DateFrom = p.DateTime(),
				DateTo = p.DateTime(),
				CurrentPage = p.Int(),
				PageSize = p.Int()
			}, body: @"SET NOCOUNT ON;
	declare @paramDefined nvarchar(max) ='@IDChiNhanhs_In nvarchar(max), 
											@IDNhaCungCap_In nvarchar(max),
											@DateFrom_In datetime,
											@DateTo_In datetime,
											@CurrentPage_In int,
											@PageSize_In int'
	declare @tblDefined nvarchar(max) = N' declare @tblChiNhanh table (ID uniqueidentifier) '

	declare @sqlTable varchar(max)= '', @sql nvarchar(max)= '', @sqlSoQuy nvarchar(max)= '', 
	@where nvarchar(max) ='', @whereNCC nvarchar(max)

	set @where= ' where 1 = 1 and hd.ChoThanhToan = 0 '
	set @whereNCC= ' where 1 = 1 '

	if isnull(@CurrentPage,'') ='' set @CurrentPage = 0
	if isnull(@PageSize,'') ='' set @PageSize = 10000

	if isnull(@IDChiNhanhs,'')!=''
		begin
			set @sqlTable = ' insert into @tblChiNhanh select name from dbo.splitstring(@IDChiNhanhs_In)'
			set @where =concat(@where,' and exists (select cn.ID from @tblChiNhanh cn where hd.ID_DonVi = cn.ID)' )
		end

	if isnull(@ID_NhaCungCap,'')!=''
		begin			
			set @whereNCC = concat(@whereNCC,' and cp.ID_NhaCungCap = @IDNhaCungCap_In' )
			set @where =concat(@where,' and cp.ID_NhaCungCap = @IDNhaCungCap_In' )
		end

    if isnull(@DateFrom,'')!=''
		begin			
			set @where =concat(@where,' and hd.NgayLapHoaDon >= @DateFrom_In' )
		end

	if isnull(@DateTo,'')!=''
		begin			
			set @where =concat(@where,' and hd.NgayLapHoaDon < @DateTo_In' )
		end


		set @sql = concat(N'
		 ---- get hoadon co phidichvu
		select distinct cp.ID_HoaDon, cp.ID_NhaCungCap into #tblChiPhi
		from BH_HoaDon_ChiPhi cp ', @whereNCC , ' ;',

		N' 
		with data_cte
				as(
					select 
						hd.ID, hd.MaHoaDon, hd.NgayLapHoaDon, 
						125 as LoaiHoaDon,
						cp.ThanhTien as TongChiphi,
						isnull(thuchi.TienThu,0) as DaThanhToan,
						cp.ThanhTien - isnull(thuchi.TienThu,0) as ConNo,
						thuchi.ID_PhieuChi,
						thuchi.MaPhieuChi,
						case hd.LoaiHoaDon 
							when 1 then N''Bán lẻ''
							when 4 then N''Nhập hàng''
							when 6 then N''Trả hàng''
							when 7 then N''Trả hàng nhập''
							when 19 then N''Gói dịch vụ''
							when 25 then N''Sửa chữa''
						end as strLoaiHoaDon
					from
					(
						SELECT cp.ID_HoaDon, cp.ID_NhaCungCap,				
							sum(cp.ThanhTien) as ThanhTien						
						from BH_HoaDon_ChiPhi cp
						join BH_HoaDon hd on cp.ID_HoaDon = hd.ID
						join BH_HoaDon_ChiTiet ct on cp.ID_HoaDon_ChiTiet = ct.ID ',
						@where,
						' group by cp.ID_HoaDon, cp.ID_NhaCungCap
					)cp
					join BH_HoaDon hd on cp.ID_HoaDon = hd.ID
					left join
					(
						select	
							cp.ID_HoaDon,  ct.ID_DoiTuong, 								
							max(qhd.MaHoaDon) as MaPhieuChi,
							max(qhd.ID) as ID_PhieuChi,
							sum(TienThu) as TienThu
						from Quy_HoaDon_ChiTiet ct
						join Quy_HoaDon qhd on ct.ID_HoaDon= qhd.ID
						join #tblChiPhi cp on ct.ID_HoaDonLienQuan = cp.ID_HoaDon and cp.ID_NhaCungCap = ct.ID_DoiTuong
						where qhd.TrangThai = 1
						group by cp.ID_HoaDon,  ct.ID_DoiTuong
					) thuchi on cp.ID_HoaDon= thuchi.ID_HoaDon
				),
			count_cte
			as (
			select count(ID) as TotalRow,
				--CEILING(COUNT(ID) / CAST(@PageSize_In as float ))  as TotalPage,
				sum(TongChiphi) as SumTongTienHang,
				sum(DaThanhToan) as SumDaThanhToan,
				sum(ConNo) as SumConNo
			from data_cte dt
			)
			select dt.*, cte.*	
			from data_cte dt
			cross join count_cte cte
			order by dt.NgayLapHoaDon desc
			OFFSET (@CurrentPage_In* @PageSize_In) ROWS
		  FETCH NEXT @PageSize_In ROWS ONLY
			 ')
	set @sql= CONCAT(@tblDefined, @sqlTable,@sql)

	print @sql

	exec sp_executesql @sql,@paramDefined,
		@IDChiNhanhs_In = @IDChiNhanhs,
		@IDNhaCungCap_In = @ID_NhaCungCap,
    	@DateFrom_In = @DateFrom,
    	@DateTo_In = @DateTo,
    	@CurrentPage_In = @CurrentPage,
    	@PageSize_In = @PageSize");

			CreateStoredProcedure(name: "[dbo].[PTN_CheckChangeCus]", parametersAction: p => new
			{
				ID_PhieuTiepNhan = p.Guid(),
				ID_KhachHangNew = p.Guid(),
				ID_BaoHiemNew = p.String(40)
			}, body: @"SET NOCOUNT ON;

	if isnull(@ID_BaoHiemNew,'')=''
		set @ID_BaoHiemNew ='00000000-0000-0000-0000-000000000000'

	declare @tblReturn table(Loai int)

	---- get PTN old
	declare @PTNOld_IDCus uniqueidentifier, @PTNOld_BaoHiem uniqueidentifier
	select @PTNOld_IDCus = ID_KhachHang, @PTNOld_BaoHiem = ID_BaoHiem from Gara_PhieuTiepNhan where ID= @ID_PhieuTiepNhan

	---- get list hoadon of PTN
	select ID, ID_DoiTuong, ID_BaoHiem
	into #tblHoaDon
	from BH_HoaDon
	where ID_PhieuTiepNhan = @ID_PhieuTiepNhan
	and ChoThanhToan =0
	and LoaiHoaDon in (3,25)


	if @ID_KhachHangNew != @PTNOld_IDCus
	begin
		declare @count1 int;
		select @count1 =count(*)
		from #tblHoaDon
		where ID_DoiTuong != @ID_KhachHangNew

		if @count1 > 0
			insert into @tblReturn values (1)
	end

	if isnull(@PTNOld_BaoHiem,'00000000-0000-0000-0000-000000000000')='00000000-0000-0000-0000-000000000000'
		set @PTNOld_BaoHiem ='00000000-0000-0000-0000-000000000000'

  
	if @ID_BaoHiemNew != @PTNOld_BaoHiem
	begin
		declare @count2 int;
		select @count2 =count(*)
		from #tblHoaDon
		where isnull(ID_BaoHiem,'00000000-0000-0000-0000-000000000000') != @ID_BaoHiemNew
		
		if @count2 > 0
			insert into @tblReturn values (2)
	end

	---- check exist soquy
	declare @countSQ int
	select @countSQ = count(qhd.ID)
	from #tblHoaDon hd
	join Quy_HoaDon_ChiTiet qct on hd.ID = qct.ID_HoaDonLienQuan
	join Quy_HoaDon qhd on qct.ID_HoaDon= qhd.ID
	where qhd.TrangThai is null or qhd.TrangThai= 1

	if @countSQ > 0
	 insert into @tblReturn values (3)

	 select * from @tblReturn");

			CreateStoredProcedure(name: "[dbo].[TheGiaTri_GetLichSuNapTien]", parametersAction: p => new
			{
				IDChiNhanhs = p.String(),
				ID_Cutomer = p.String(),
				TextSearch = p.String(),
				DateFrom = p.String(),
				DateTo = p.String(),
				CurrentPage = p.Int(),
				PageSize = p.Int()
			}, body: @"SET NOCOUNT ON;
	
	declare @paramIn nvarchar(max)=' declare @isNull_txtSearch int = 1 '

	declare @tblDefined nvarchar(max)='', @sql1 nvarchar(max) ='',  @sql2 nvarchar(max) ='',
	@whereIn nvarchar(max)='', @whereOut nvarchar(max)='',
	
	@paramDefined nvarchar(max)= N'
			@IDChiNhanhs_In [nvarchar](max) ,
			@ID_Cutomer_In [nvarchar](max),
			@TextSearch_In [nvarchar](max),
			@DateFrom_In [datetime],
			@DateTo_In [datetime],		
			@CurrentPage_In [int],
			@PageSize_In [int]
			 '
		set @whereIn = ' where 1 = 1 and hd.LoaiHoaDon in (22,23)'
		set @whereOut = ' where 1 = 1'

		if isnull(@CurrentPage,'') ='' set @CurrentPage = 0
		if isnull(@PageSize,'') ='' set @PageSize = 20

		if isnull(@IDChiNhanhs,'')!=''
			begin
				set @tblDefined = concat(@tblDefined, N' declare @tblChiNhanh table (ID uniqueidentifier)
					insert into @tblChiNhanh select name from dbo.splitstring(@IDChiNhanhs_In) ')
				set @whereIn= CONCAT(@whereIn, ' and exists (select ID from @tblChiNhanh cn where hd.ID_DonVi = cn.ID)')
			end

		if isnull(@ID_Cutomer,'')!=''
		begin
			set @whereIn= CONCAT(@whereIn, ' and hd.ID_DoiTuong = @ID_Cutomer_In')
		end

	   if isnull(@DateFrom,'')!=''
		begin
			set @whereIn= CONCAT(@whereIn, ' and hd.NgayLapHoaDon >= @DateFrom_In')
		end

		if isnull(@DateTo,'')!=''
		begin
			set @whereIn= CONCAT(@whereIn, ' and hd.NgayLapHoaDon < @DateTo_In')
		end

		if isnull(@TextSearch,'')!='' and isnull(@TextSearch,'')!='%%'
			begin			
				set @paramIn = CONCAT(@paramIn, ' set @isNull_txtSearch = 0')
				set @TextSearch = CONCAT(N'%', @TextSearch, '%')
				set @whereOut= CONCAT(@whereOut, ' 
					and (MaHoaDon like @TextSearch_In
						OR MaDoiTuong like @TextSearch_In
						OR TenDoiTuong like @TextSearch_In
						OR TenDoiTuong_KhongDau like @TextSearch_In
						OR MaHoaDon like @TextSearch_In
						OR DienGiai like @TextSearch_In
						OR DienGiaiUnSign like @TextSearch_In)'
					)					
			end

			set @sql1 = concat(N'

			select hd.ID,
				hd.ID_DoiTuong,			
                hd.MaHoaDon,
                hd.LoaiHoaDon,
                hd.NgayLapHoaDon,
                hd.TongChiPhi,
                hd.TongChietKhau,
                hd.TongTienHang,
                hd.TongTienThue,
                hd.TongGiamGia,
                hd.PhaiThanhToan,                                     
                hd.DienGiai
				into #htThe
			from BH_HoaDon hd ', @whereIn)
			

		
			set @sql2 = concat(N'
		with data_cte
    	as(
			select tbl.*, 
				dt.MaDoiTuong as MaKhachHang,
				dt.TenDoiTuong as TenKhachHang
				from
				(
				select hd.ID,
					hd.ID_DoiTuong,
					hd.LoaiHoaDon,
					hd.MaHoaDon,
					hd.NgayLapHoaDon,
					hd.TongChiPhi as MucNap,
					hd.TongChietKhau as KhuyenMaiVND,
					hd.TongTienHang as TongTienNap,
					hd.TongTienHang + hd.TongChietKhau as SoDuSauNap,
					hd.TongGiamGia as ChietKhauVND,
					hd.PhaiThanhToan,
					hd.DienGiai,
					iif(@isNull_txtSearch =0, dbo.FUNC_ConvertStringToUnsign(hd.DienGiai), hd.DienGiai ) as DienGiaiUnSign,
					isnull(thu.KhachDaTra,0) as KhachDaTra
				from #htThe hd
				left join
				(
					select 
						qct.ID_HoaDonLienQuan,
						sum(qct.TienThu) as KhachDaTra
					from Quy_HoaDon_ChiTiet qct
					join #htThe hd on qct.ID_HoaDonLienQuan = hd.ID
					join Quy_HoaDon qhd on qct.ID_HoaDon= qhd.ID
					where qhd.TrangThai= 1 or qhd.TrangThai= 1
					group by qct.ID_HoaDonLienQuan
				) thu
				on hd.ID = thu.ID_HoaDonLienQuan
			) tbl 
			join DM_DoiTuong dt on tbl.ID_DoiTuong = dt.ID
			', @whereOut ,
			'),
			count_cte
				as (
				select count(dt.ID) as TotalRow,
    				CEILING(count(dt.ID) / cast(@PageSize_In as float)) as TotalPage,
    				sum(TongTienNap) as SumTongTienNap,
    				sum(SoDuSauNap) as SumSoDuSauNap,
    				sum(PhaiThanhToan) as SumPhaiThanhToan,
    				sum(KhachDaTra) as SumKhachDaTra
				from data_cte dt
    		)
    		select*
			from data_cte dt
			cross join count_cte
			order by dt.NgayLapHoaDon desc
			OFFSET(@CurrentPage_In * @PageSize_In) ROWS
			FETCH NEXT @PageSize_In ROWS ONLY
		')

		set @sql2= CONCAT(@paramIn, '; ', @tblDefined, @sql1,'; ', @sql2)		

		print @sql2

		exec sp_executesql @sql2, 
    		@paramDefined,
    		@IDChiNhanhs_In= @IDChiNhanhs,
			@ID_Cutomer_In = @ID_Cutomer,
			@TextSearch_In = @TextSearch,
			@DateFrom_In = @DateFrom,
			@DateTo_In = @DateTo,			
			@CurrentPage_In = @CurrentPage,
			@PageSize_In = @PageSize");
        }
        
        public override void Down()
        {
            DropStoredProcedure("[dbo].[BaoCaoGoiDV_GetCTMua]");
            DropStoredProcedure("[dbo].[BCBanHang_GetChiPhi]");
            DropStoredProcedure("[dbo].[ChangePTN_updateCus]");
            DropStoredProcedure("[dbo].[CTHD_GetChiPhiDichVu]");
            DropStoredProcedure("[dbo].[GetChiPhiDichVu_byVendor]");
			DropStoredProcedure("[dbo].[PTN_CheckChangeCus]");
			DropStoredProcedure("[dbo].[TheGiaTri_GetLichSuNapTien]");
        }
    }
}
