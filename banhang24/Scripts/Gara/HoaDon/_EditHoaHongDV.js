﻿var vmHoaHongDV = new Vue({
    el: '#vmEditHoaHongDV',
    components: {
        'nvien-hoadon-search': cmpSearchNVDisscount,
    },
    data: {
        saveOK: false,
        isNew: true,
        isDoiTra: false,
        isCombo: false,
        LoaiChietKhauHD_NV: '2',
        TinhHoaHongTruocCK: false,
        role: {
            XemChietKhau: true,
            ThayDoiChietKhau: true,
        },
        IsShareDiscount_DichVu: '2',// 1.share, 2.no share
        IsChosingNV_ThucHien: 1,
        LaPhanTram: true,
        inforHoaDon: {
            ID_DonVi: $('#txtDonVi').val(),// get from gara/banle
            LoaiHoaDon: 1,
            MaHoaDon: '',
        },

        DichVu_isDoing: {},
        ItemChosing: {}, // item in GridNV_TVTH
        GridNV_TVTH: [],
        listData: {
            NhanViens: [],
            AllChietKhau_NhanVien: [],
        },
    },
    created: function () {
        var self = this;
        self.PageLoad();
        console.log('hhDV')
    },
    methods: {
        PageLoad: function () {
            var self = this;
            if (commonStatisJs.CheckNull(self.inforHoaDon.ID_DonVi)) {
                self.inforHoaDon.ID_DonVi = VHeader.IdDonVi;
                self.role.XemChietKhau = VHeader.Quyen.indexOf('BanHang_HoaDongDichVu_XemChietKhau') > -1;
                self.role.ThayDoiChietKhau = VHeader.Quyen.indexOf('HoaHong_ThayDoi') > -1;
            }
            ajaxHelper('/api/DanhMuc/NS_NhanVienAPI/' + 'GetChietKhauHangHoaNVien_byChiNhanh?idChiNhanh=' + self.inforHoaDon.ID_DonVi, 'GET').done(function (x) {
                if (x.res === true) {
                    self.listData.AllChietKhau_NhanVien = x.data;
                }
            });
        },

        showModal: function (item, isDoiTra = false, isCombo = false) {
            var self = this;
            // use at view gara
            self.saveOK = false;
            self.isNew = true;
            self.isDoiTra = isDoiTra;
            self.isCombo = isCombo;
            self.IsShareDiscount_DichVu = '2';
            self.DichVu_isDoing = item;
            if (commonStatisJs.CheckNull(item.HoaHongTruocChietKhau)) {
                item.HoaHongTruocChietKhau = 0;
            }
            self.TinhHoaHongTruocCK = item.HoaHongTruocChietKhau === 1;
            self.DichVu_isDoing.GiaTriTinhCK = self.GetGiaTriTinhCK();
            if (self.inforHoaDon.LoaiHoaDon === 19) {
                self.IsChosingNV_ThucHien = 4;
            }
            else {
                self.IsChosingNV_ThucHien = 1;
            }
            self.CheckUncheck_rdoShareDiscountDV();
            $('#vmEditHoaHongDV .nav-tabs li:eq(0)').addClass('active');
            $('#vmEditHoaHongDV .nav-tabs li:gt(0)').removeClass('active');
            $('#vmEditHoaHongDV').modal('show');
            $(function () {
                $('#vmEditHoaHongDV .textSearchNV').val('');
            })
        },
        showModalUpdate: function (item, isTPComBo = false) {// used to update ck nvien after save DB (dshoadon, banlamviec)
            var self = this;
            self.saveOK = false;
            self.isNew = false;
            self.isCombo = isTPComBo;// is TPCombo
            self.IsShareDiscount_DichVu = '2';
            self.DichVu_isDoing = item;

            if (self.GridNV_TVTH.length > 0) {
                item.HoaHongTruocChietKhau = self.GridNV_TVTH[0].TinhHoaHongTruocCK;
            }
            else {
                if (commonStatisJs.CheckNull(item.HoaHongTruocChietKhau)) {
                    item.HoaHongTruocChietKhau = 0;
                }
            }
            self.TinhHoaHongTruocCK = item.HoaHongTruocChietKhau === 1;
            if (self.inforHoaDon.LoaiHoaDon === 19) {
                self.IsChosingNV_ThucHien = 4;
            }
            else {
                self.IsChosingNV_ThucHien = 1;
            }
            self.CheckUncheck_rdoShareDiscountDV();
           
            if (isTPComBo) {
                item.TongPhiDichVu = item.PhiDichVu * item.SoLuong;
                if (item.LaPTPhiDichVu) {
                    item.TongPhiDichVu = RoundDecimal(item.DonGia * item.SoLuong * item.PhiDichVu / 100, 1);
                }
            }
            self.DichVu_isDoing.GiaTriTinhCK = self.GetGiaTriTinhCK();
            $('#vmEditHoaHongDV').modal('show');
        },
        Change_TacVu: function (x) {
            var self = this;
            if (self.inforHoaDon.LoaiHoaDon === 19) {
                self.IsChosingNV_ThucHien = 4;
            }
            else {
                self.IsChosingNV_ThucHien = x;
            }
            self.CheckUncheck_rdoShareDiscountDV();
        },
        Change_TinhHoaHongTruocCK: function (x) {
            let self = this;

            let gtriTinhCK = self.GetGiaTriTinhCK();
            self.DichVu_isDoing.GiaTriTinhCK = gtriTinhCK;

            for (let i = 0; i < self.GridNV_TVTH.length; i++) {
                let itemFor = self.GridNV_TVTH[i];
                let ptCK = itemFor.PT_ChietKhau;
                let tienCK = itemFor.TienChietKhau;
                if (ptCK > 0) {
                    tienCK = Math.round(gtriTinhCK * ptCK / 100 * itemFor.HeSo);
                }
                else {
                    tienCK = itemFor.ChietKhauMacDinh * self.DichVu_isDoing.SoLuong * itemFor.HeSo;
                }
                self.GridNV_TVTH[i].TienChietKhau = tienCK;
                self.GridNV_TVTH[i].TinhHoaHongTruocCK = self.TinhHoaHongTruocCK ? 1 : 0;
            }
        },
        CheckUncheck_rdoShareDiscountDV: function () {
            var self = this;
            var arrHeSo = $.grep(self.GridNV_TVTH, function (x) {
                return x.HeSo !== 1 && x.TacVu === self.IsChosingNV_ThucHien;
            });
            if (arrHeSo.length > 0) self.IsShareDiscount_DichVu = '1';
            else self.IsShareDiscount_DichVu = '2';
        },
        AddNhanVien_TVTH: function (item) {
            var self = this;
            var idNhanVien = item.ID;
            // check IDNhanVien exist in grid with same TacVu
            var itemEx = $.grep(self.GridNV_TVTH, function (x) {
                return x.ID_NhanVien === idNhanVien && x.TacVu === self.IsChosingNV_ThucHien;
            });
            if (itemEx.length > 0) {
                ShowMessage_Danger('Nhân viên ' + itemEx[0].TenNhanVien + ' đã được chọn');
                return;
            }
            // get infor ChietKhau by NhanVien and chi nhanh, and ID_QuiDoi
            var itemCK = $.grep(self.listData.AllChietKhau_NhanVien, function (x) {
                return x.ID_NhanVien === idNhanVien && x.ID_DonVi === self.inforHoaDon.ID_DonVi
                    && x.ID_DonViQuiDoi === self.DichVu_isDoing.ID_DonViQuiDoi;
            });
            if (itemCK.length > 0) {
                let newObject = self.newNhanVien_ChietKhau(itemCK[0], true);
                self.GridNV_TVTH.push(newObject);
            }
            else {
                let newObject = self.newNhanVien_ChietKhau(item, false);
                self.GridNV_TVTH.push(newObject);
            }
            self.HoaHongDV_UpdateHeSo_AndBind();// update again HeSo if current IsShareDiscount_DichVu
        },
        GetGiaTriTinhCK: function () {
            let self = this;
            let gtriTinhCK = 0;
            if (self.TinhHoaHongTruocCK) {
                gtriTinhCK = formatNumberToFloat(self.DichVu_isDoing.DonGia) * formatNumberToFloat(self.DichVu_isDoing.SoLuong);
            }
            else {
                gtriTinhCK = formatNumberToFloat(self.DichVu_isDoing.ThanhTien);
            }
            gtriTinhCK = gtriTinhCK - self.DichVu_isDoing.TongPhiDichVu;
            gtriTinhCK = gtriTinhCK < 0 ? 0 : gtriTinhCK;
            return gtriTinhCK;
        },

        newNhanVien_ChietKhau: function (item, exitChietKhau) {
            var self = this;
            var idChiTietHD = self.isNew ? '00000000-0000-0000-0000-000000000000' : self.DichVu_isDoing.ID;
            var tacVu = self.IsChosingNV_ThucHien; // 1.ThucHien, 2.TuVan, 3.yeucau,  4.BanGoi
            var isThucHien = (tacVu === 1);
            if (exitChietKhau) {
                let isPTram = false;
                let valChietKhau = 0; // get from DB
                let tienCK_NV = 0; // used to assign in Grid
                let ptramCK = 0;
                let gtriTinhCK = self.DichVu_isDoing.GiaTriTinhCK;
                switch (tacVu) {
                    case 1:
                        isPTram = item.LaPhanTram_ThucHien;
                        valChietKhau = item.ChietKhau_ThucHien;
                        break;
                    case 2:
                        isPTram = item.LaPhanTram_TuVan;
                        valChietKhau = item.ChietKhau_TuVan;
                        break;
                    case 3:
                        isPTram = item.LaPhanTram_YeuCau;
                        valChietKhau = item.ChietKhau_YeuCau;
                        break;
                    case 4:
                        isPTram = item.LaPhanTram_BanGoi;
                        valChietKhau = item.ChietKhau_BanGoi;
                        break;
                }
                if (isPTram) {
                    tienCK_NV = Math.round((valChietKhau / 100) * gtriTinhCK - self.DichVu_isDoing.TongPhiDichVu);
                    ptramCK = valChietKhau;
                }
                else {
                    ptramCK = 0;
                    tienCK_NV = valChietKhau * self.DichVu_isDoing.SoLuong; // nhan tienck theo SoLuong
                }
                tienCK_NV = tienCK_NV < 0 ? 0 : tienCK_NV;
                return {
                    ID_ChiTietHoaDon: idChiTietHD,
                    ID_NhanVien: item.ID_NhanVien,
                    MaNhanVien: item.MaNhanVien,
                    TenNhanVien: item.TenNhanVien,
                    ThucHien_TuVan: isThucHien,// save in DB: ThucHien: true, TuVan: false
                    // check in gridview
                    TacVu: tacVu,
                    // save in DB
                    TienChietKhau: tienCK_NV,
                    PT_ChietKhau: ptramCK,
                    TheoYeuCau: (tacVu === 3),
                    ChietKhauMacDinh: valChietKhau,
                    HeSo: 1,
                    TinhChietKhauTheo: tacVu === 4 ? 4 : null, // if BanGoi: TinhChietKhauTheo= 4,
                    TinhHoaHongTruocCK: self.TinhHoaHongTruocCK ? 1 : 0,
                }
            }
            else {
                return {
                    ID_ChiTietHoaDon: idChiTietHD,
                    ID_NhanVien: item.ID,
                    MaNhanVien: item.MaNhanVien,
                    TenNhanVien: item.TenNhanVien,
                    ThucHien_TuVan: isThucHien,
                    // check in gridview
                    TacVu: tacVu,
                    TienChietKhau: 0,
                    PT_ChietKhau: 0,
                    TheoYeuCau: (tacVu === 3),
                    ChietKhauMacDinh: 0,
                    HeSo: 1,
                    TinhChietKhauTheo: tacVu === 4 ? 4 : null,
                    TinhHoaHongTruocCK: self.TinhHoaHongTruocCK ? 1 : 0,
                }
            }
        },

        HoaHongDV_UpdateHeSo_AndBind: function () {
            let self = this;
            let heso = 1;
            // count NV thuchien/tuvan/bangoi
            let tacVu = self.IsChosingNV_ThucHien;
            let arrTacVu = [tacVu];
            if (tacVu === 1) {
                arrTacVu = [1, 3];
            }
            let arrTH = $.grep(self.GridNV_TVTH, function (x) {
                return $.inArray(x.TacVu, arrTacVu) > -1;
            });
            if (self.IsShareDiscount_DichVu === '1') {
                heso = RoundDecimal(1 / arrTH.length, 2);
            }

            let gtriTinhCK = self.DichVu_isDoing.GiaTriTinhCK;
            for (let i = 0; i < self.GridNV_TVTH.length; i++) {
                let itemFor = self.GridNV_TVTH[i];
                let ptCK = itemFor.PT_ChietKhau;
                let tienCK = itemFor.TienChietKhau;
                if ($.inArray(itemFor.TacVu, arrTacVu) > -1) { // nếu đang active tab (tư vấn/thực hiện): chia đều theo hệ số
                    if (ptCK > 0) {
                        tienCK = Math.round(gtriTinhCK * ptCK / 100 * heso);
                    }
                    else {
                        if (heso !== 1) {
                            tienCK = itemFor.ChietKhauMacDinh * self.DichVu_isDoing.SoLuong * heso;
                        }
                        else {
                            tienCK = tienCK / self.GridNV_TVTH[i].HeSo;
                        }
                    }
                    self.GridNV_TVTH[i].HeSo = heso;
                    self.GridNV_TVTH[i].TienChietKhau = tienCK;
                }
            }
        },

        ChangeCKDichVu_TheoYeuCau: function (item, index) {
            var self = this;
            var $this = event.currentTarget;
            var isCheck = $($this).is(':checked');
            var tacVu = isCheck === true ? 3 : 1;
            var itemCK = $.grep(self.listData.AllChietKhau_NhanVien, function (x) {
                return x.ID_NhanVien === item.ID_NhanVien && x.ID_DonVi === self.inforHoaDon.ID_DonVi
                    && x.ID_DonViQuiDoi === self.DichVu_isDoing.ID_DonViQuiDoi;
            });
            var soluong = self.DichVu_isDoing.SoLuong;
            var isPTram = false;
            var valChietKhau = 0; // get from DB
            var tienCK_NV = 0; // used to assign in Grid
            var ptramCK = 0;
            var ckMacDinh = 0;
            if (itemCK.length > 0) {
                var theoCKTH = itemCK[0].TheoChietKhau_ThucHien;
                switch (tacVu) {
                    case 1:
                        isPTram = itemCK[0].LaPhanTram_ThucHien;
                        valChietKhau = itemCK[0].ChietKhau_ThucHien;
                        break;
                    case 2:
                        isPTram = itemCK[0].LaPhanTram_TuVan;
                        valChietKhau = itemCK[0].ChietKhau_TuVan;
                        break;
                    case 3:
                        isPTram = itemCK[0].LaPhanTram_YeuCau;
                        valChietKhau = itemCK[0].ChietKhau_YeuCau;
                        break;
                    case 4:
                        isPTram = itemCK[0].LaPhanTram_BanGoi;
                        valChietKhau = itemCK[0].ChietKhau_BanGoi;
                        break;
                }
                let gtriTinhCK = self.DichVu_isDoing.GiaTriTinhCK;
                if (isCheck) {
                    var isPTramTH = itemCK[0].LaPhanTram_ThucHien;
                    var gtriCKTH = itemCK[0].ChietKhau_ThucHien;
                    if (theoCKTH === 0) {
                        if (isPTram) {
                            tienCK_NV = Math.round((valChietKhau / 100) * gtriTinhCK) * item.HeSo;
                            ckMacDinh = ptramCK = valChietKhau;
                        }
                        else {
                            ptramCK = 0;
                            ckMacDinh = valChietKhau;
                            tienCK_NV = valChietKhau * soluong * item.HeSo;
                        }
                    }
                    else {
                        if (isPTramTH) {
                            if (isPTram) {
                                ckMacDinh = ptramCK = valChietKhau + gtriCKTH;
                                tienCK_NV = Math.round((ptramCK / 100) * gtriTinhCK) * item.HeSo;
                            }
                            else {
                                ptramCK = 0;
                                tienCK_NV = Math.round((gtriCKTH / 100) * gtriTinhCK) + valChietKhau * soluong;// quy ve VND neu khac loai
                                ckMacDinh = tienCK_NV;
                                tienCK_NV = tienCK_NV * item.HeSo;
                            }
                        }
                        else {
                            // thuchien = vnd
                            if (isPTram) {
                                ptramCK = 0;
                                tienCK_NV = Math.round((valChietKhau / 100) * gtriTinhCK) + gtriCKTH * soluong;// quy ve VND neu khac loai
                                ckMacDinh = tienCK_NV; //??
                                tienCK_NV = tienCK_NV * item.HeSo;
                            }
                            else {
                                ptramCK = 0;
                                ckMacDinh = gtriCKTH + valChietKhau;
                                tienCK_NV = (gtriCKTH + valChietKhau) * soluong * item.HeSo;
                            }
                        }
                    }
                }
                else {
                    if (isPTram) {
                        tienCK_NV = Math.round((valChietKhau / 100) * gtriTinhCK) * item.HeSo;
                        ckMacDinh = ptramCK = valChietKhau;
                    }
                    else {
                        ptramCK = 0;
                        ckMacDinh = valChietKhau;
                        tienCK_NV = valChietKhau * soluong * item.HeSo;
                    }
                }
            }
            for (let i = 0; i < self.GridNV_TVTH.length; i++) {
                if (i === index) {
                    self.GridNV_TVTH[i].TacVu = tacVu;
                    self.GridNV_TVTH[i].TheoYeuCau = isCheck;
                    self.GridNV_TVTH[i].PT_ChietKhau = ptramCK;
                    self.GridNV_TVTH[i].TienChietKhau = tienCK_NV;
                    self.GridNV_TVTH[i].ChietKhauMacDinh = ckMacDinh;
                    break;
                }
            }
        },

        ShowDiv_ChietKhauNV: function (item, index) {
            var self = this;
            var thisObj = $(event.currentTarget);
            if (self.role.ThayDoiChietKhau === false) {
                ShowMessage_Danger('Không có quyền thay đổi chiết khấu nhân viên');
                return false;
            }

            var pos = thisObj.closest('td').position();
            $('.jsDiscount').show();
            $('.jsDiscount').css({
                left: (pos.left - 80) + "px",
                top: (pos.top + 31 + 37) + "px"
            });

            item.Index = index;
            self.ItemChosing = item;
            var gtriCK = 0;
            if (item.PT_ChietKhau > 0 || item.TienChietKhau === 0) {
                gtriCK = item.PT_ChietKhau;
                self.LaPhanTram = true;
            }
            else {
                self.LaPhanTram = false;
                gtriCK = formatNumber(item.TienChietKhau);
            }
            $(function () {
                let inputNext = $('.jsDiscount').children('div').eq(0).find('input');
                $(inputNext).val(gtriCK);
                $(inputNext).focus().select();
            });
        },

        HoaHongDV_EditHeSo: function (item, index) {
            var self = this;
            if (self.role.ThayDoiChietKhau === false) {
                ShowMessage_Danger('Không có quyền thay đổi chiết khấu nhân viên');
                return false;
            }
            var thisObj = $(event.currentTarget);
            var heso = formatNumberToFloat(thisObj.val());

            var gtritinhCK = self.DichVu_isDoing.GiaTriTinhCK;
            var tienCK = 0;
            var ptCK = item.PT_ChietKhau;

            if (ptCK > 0) {
                tienCK = Math.round(ptCK * gtritinhCK / 100) * heso;
            }
            else {
                tienCK = item.ChietKhauMacDinh * heso * self.DichVu_isDoing.SoLuong;
            }

            for (let i = 0; i < self.GridNV_TVTH.length; i++) {
                if (i === index) {
                    self.GridNV_TVTH[i].TienChietKhau = tienCK;
                    break;
                }
            }

            if (event.keyCode === 13 || event.which) {
                $(thisObj).closest('table').find('tr td').eq(3).select().focus();
            }
        },

        Edit_ChietKhauNV: function (item) {
            var self = this;
            var thisObj = $(event.currentTarget);
            var thisVal = $(thisObj).val();
            formatNumberObj(thisObj);
            var item = self.ItemChosing;

            var gtritinhCK = self.DichVu_isDoing.GiaTriTinhCK;
            var ckMacDinh = 0;
            var tienCK = 0;
            var ptramCK = 0;
            if (thisVal === '') {
                thisVal = 0;
            }
            if (self.LaPhanTram) {
                if (thisVal > 100) {
                    thisVal = 100;
                    thisObj.val(100);
                }
                tienCK = Math.round((thisVal / 100) * gtritinhCK) * item.HeSo;
                ptramCK = thisVal;
            }
            else {
                ckMacDinh = formatNumberToFloat(thisVal) / self.DichVu_isDoing.SoLuong;
                tienCK = formatNumberToInt(thisVal) * item.HeSo;
            }

            // update infor %CK, tienCK into grid
            for (let i = 0; i < self.GridNV_TVTH.length; i++) {
                if (i === item.Index) {
                    self.GridNV_TVTH[i].TienChietKhau = tienCK;
                    self.GridNV_TVTH[i].PT_ChietKhau = ptramCK;
                    self.GridNV_TVTH[i].ChietKhauMacDinh = ckMacDinh;
                    break;
                }
            }
        },

        RemoveNVien_THTV: function (index) {
            var self = this;
            let idNhanVien = null;
            for (let i = 0; i < self.GridNV_TVTH.length; i++) {
                if (i === index) {
                    idNhanVien = self.GridNV_TVTH[i].ID_NhanVien;
                    self.GridNV_TVTH.splice(i, 1);
                    break;
                }
            }
            self.HoaHongDV_UpdateHeSo_AndBind();

            // update status for nvien 
            for (let i = 0; i < self.listData.NhanViens.length; i++) {
                let itFor = self.listData.NhanViens[i];
                if (itFor.ID === idNhanVien) {
                    self.listData.NhanViens[i].Status = 0;
                }
            }
            self.listData.NhanViens = $.extend([], true, self.listData.NhanViens);
        },

        clickVND_NoVND_ChietKhau: function (item) {
            var self = this;
            var item = self.ItemChosing;

            var gtriCKNew = 0;
            var isPtramNew = true;
            var ckMacDinh = 0;
            var gtritinhCK = self.DichVu_isDoing.GiaTriTinhCK;

            // if before isPTram = true --> change isPtram = false
            if (self.LaPhanTram) {
                gtriCKNew = item.TienChietKhau / item.HeSo;
                isPtramNew = false;
                ckMacDinh = Math.round(gtriCKNew / self.DichVu_isDoing.SoLuong);
            }
            else {
                isPtramNew = true;
                gtriCKNew = RoundDecimal(item.TienChietKhau / gtritinhCK * 100);
            }
            self.LaPhanTram = isPtramNew;

            // update infor %CK into grid (not update TienCK)
            for (let i = 0; i < self.GridNV_TVTH.length; i++) {
                if (i === item.Index) {
                    if (isPtramNew) {
                        self.GridNV_TVTH[i].PT_ChietKhau = gtriCKNew;
                        self.GridNV_TVTH[i].ChietKhauMacDinh = gtriCKNew;
                    }
                    else {
                        self.GridNV_TVTH[i].PT_ChietKhau = 0;
                        self.GridNV_TVTH[i].ChietKhauMacDinh = ckMacDinh;
                    }
                    break;
                }
            }
        },

        AgreeNhanVien_TVTH: function () {
            var self = this;
            self.saveOK = true;
            for (let i = 0; i < self.GridNV_TVTH.length; i++) {
                let itemFor = self.GridNV_TVTH[i];
                let valCK = formatNumber(itemFor.TienChietKhau);
                if (itemFor.PT_ChietKhau > 0) {
                    valCK = itemFor.PT_ChietKhau;
                }
                self.GridNV_TVTH[i].ChietKhauMacDinh = formatNumberToFloat(valCK);
                if (itemFor.PT_ChietKhau === 0) {
                    // ckMacdinh * heso = ck --> ckMacDinh = ck/heso
                    self.GridNV_TVTH[i].ChietKhauMacDinh = formatNumberToFloat(valCK) / formatNumberToFloat(itemFor.HeSo) / self.DichVu_isDoing.SoLuong;
                }
            }
            $('#vmEditHoaHongDV').modal('hide');
        },

        SaveCKNVien_toDB: function () {
            var self = this;

            var lstNV = self.GridNV_TVTH;
            var myData = {
                NhanViens: lstNV,
                IDChiTiets: [self.DichVu_isDoing.ID],
            }

            ajaxHelper('/api/DanhMuc/BH_HoaDonAPI/UpdateCKNhanVien_DichVu', 'POST', myData).done(function (x) {
                if (x.res) {
                    self.CTHD_ChangeCKNV = [];
                    ShowMessage_Success('Cập nhật chiết khấu nhân viên thành công');
                    var diary = {
                        ID_DonVi: VHeader.IdDonVi,
                        ID_NhanVien: VHeader.IdNhanVien,
                        LoaiNhatKy: 2,
                        ChucNang: 'Sửa đổi hoa hồng nhân viên theo dịch vụ ',
                        NoiDung: 'Sửa đổi hoa hồng nhân viên theo dịch vụ cho hóa đơn <b>'.concat(self.inforHoaDon.MaHoaDon, ' </b>, Người sửa:', VHeader.UserLogin),
                        NoiDungChiTiet: 'Sửa đổi hoa hồng nhân viên theo dịch vụ cho hóa đơn '.concat(self.inforHoaDon.MaHoaDon,
                            ' <br> - Dịch vụ bị thay đổi hoa hồng gồm: ', self.DichVu_isDoing.TenHangHoa),
                    }
                    Insert_NhatKyThaoTac_1Param(diary);
                }
                else {
                    ShowMessage_Danger(x.mes);
                }
                localStorage.removeItem('cacheNVTH');
                $('#vmEditHoaHongDV').modal('hide');
            });
        },
    },
})
