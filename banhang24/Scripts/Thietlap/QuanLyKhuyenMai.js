var ViewModel = function () {
    var self = this;
    var _maseach_KM = null;
    var _numberRowns = 10;
    var _numberPage = 1;
    var _numberPage_LS = 1;
    var _numberRowns_LS = 10;
    var AllPage = 1;
    var AllPage_LS = 1;
    var _maKM = null;
    var _tenKM = null;
    var _ghichuKM = null;
    var _trangthaiKM = true;
    var _hinhthucKM = 11;
    var _nameChiNhanhKM = null;
    var _loaiKM = 1;
    var dateTime = new Date();
    var _id_NhanVienLS = $('.idnhanvien').text();
    var _id_DonVi = $('#hd_IDdDonVi').val();
    var _IDDoiTuong = $('.idnguoidung').text();
    self.HinhThucKM = ko.observable();
    self.DonViChosed = ko.observableArray();
    self.RowsStart = ko.observable('0');
    self.RowsEnd = ko.observable('10');
    self.RowsStart_LS = ko.observable('0');
    self.RowsEnd_LS = ko.observable('10');
    self.RowsKhuyenMai = ko.observable();
    self.PagesKhuyenMai = ko.observableArray();
    self.BH_LichSuKhuyenMai = ko.observableArray();
    self.Rows_LichSuKhuyenMai = ko.observable();
    self.Pages_LichSuKhuyenMai = ko.observableArray();
    self.CurrentPage = ko.observable(1);
    self.currentPage_LS = ko.observable(1);
    self.TenChiNhanh = ko.observable();
    self.searchDonVi = ko.observableArray()
    self.BH_KhuyenMai_ChiTiet = ko.observableArray();
    self.MangChiNhanh = ko.observableArray();
    self.MangChiNhanh_Use = ko.observableArray();
    self.HangHoas = ko.observableArray();
    self.DonVis = ko.observableArray();
    var BH_DonViUri = '/api/DanhMuc/DM_DonViAPI/';
    var NhanVienUri = '/api/DanhMuc/NS_NhanVienAPI/';
    var NguoiDungUri = '/api/DanhMuc/HT_NguoiDungAPI/';
    var BH_HoaDonUri = '/api/DanhMuc/BH_HoaDonAPI/';
    var BH_XuatHuyUri = '/api/DanhMuc/BH_XuatHuyAPI/';
    var BH_KhuyenMaiUri = '/api/DanhMuc/BH_KhuyenMaiAPI/';
    var ReportUri = '/api/DanhMuc/ReportAPI/';

    self.filteredDM_KhuyenMai = ko.observableArray();
    self.rownStart = ko.observable('1');
    self.rownEnd = ko.observable('15');
    self.pageSizes = [10, 20, 30, 40, 50];
    self.pageSize = ko.observable(self.pageSizes[0]);
    self.KhuyenMai_CapNhat = ko.observable();
    self.KhuyenMai_SaoChep = ko.observable();
    self.KhuyenMai_ThemMoi = ko.observable();
    self.KhuyenMai_XemDs = ko.observable();
    self.KhuyenMai_Xoa = ko.observable();
    self.Filter_Expired = ko.observable(2);
    self.Filter_TypePromotion = ko.observable(0);
    self.StatusActive = ko.observable(1);

    function getQuyen_NguoiDung() {
        ajaxHelper(ReportUri + "getQuyen_NguoiDung?ID_NguoiDung=" + _IDDoiTuong + "&ID_DonVi=" + _id_DonVi + "&MaQuyen=" + "KhuyenMai_CapNhat", "GET").done(function (data) {
            self.KhuyenMai_CapNhat(data);
        })
        ajaxHelper(ReportUri + "getQuyen_NguoiDung?ID_NguoiDung=" + _IDDoiTuong + "&ID_DonVi=" + _id_DonVi + "&MaQuyen=" + "KhuyenMai_SaoChep", "GET").done(function (data) {
            self.KhuyenMai_SaoChep(data);
        })
        ajaxHelper(ReportUri + "getQuyen_NguoiDung?ID_NguoiDung=" + _IDDoiTuong + "&ID_DonVi=" + _id_DonVi + "&MaQuyen=" + "KhuyenMai_ThemMoi", "GET").done(function (data) {
            self.KhuyenMai_ThemMoi(data);
        })
        ajaxHelper(ReportUri + "getQuyen_NguoiDung?ID_NguoiDung=" + _IDDoiTuong + "&ID_DonVi=" + _id_DonVi + "&MaQuyen=" + "KhuyenMai_XemDS", "GET").done(function (data) {
            self.KhuyenMai_XemDs(data);
        })
        ajaxHelper(ReportUri + "getQuyen_NguoiDung?ID_NguoiDung=" + _IDDoiTuong + "&ID_DonVi=" + _id_DonVi + "&MaQuyen=" + "KhuyenMai_Xoa", "GET").done(function (data) {
            self.KhuyenMai_Xoa(data);
        })
    }

    self.NoteMaKM = function () {
        _maseach_KM = $('#EnterMaKM').val();
    }

    self.checkKichHoat = function () {
        _trangthaiKM = true;
    }
    self.checkChuaApDung = function () {
        _trangthaiKM = false;
    }
    $('.choseSuKienKM input').on('click', function () {
        if ($(this).val() === '1') {
            _trangthaiKM = true;
        }
        else {
            _trangthaiKM = false;
        }
    })
    $('#EnterMaKM').keypress(function (e) {
        if (e.keyCode == 13) {
            _numberPage = 1;
            GetListPromotion();
        }
    });

    self.Click_IconSearch = function () {
        _numberPage = 1;
        GetListPromotion();
    }
    self.SelectNameKM = function () {
        _tenKM = $('#txtNameKM').val();
    }
    self.NoteKM = function () {
        _ghichuKM = $('#txtNoteKM').val();
    }

    self.Note_MaKhuyenMai = function () {
        _maKM = $('#txt_NoteMaKM').val();
    }

    self.EventPageSize = function () {

        _numberRowns = $('#txtRownSelect').val();
        _numberPage = 1;
        GetListPromotion();
    }


    //load đơn vị
    function getDonVi() {
        ajaxHelper(BH_DonViUri + "GetDonVi_byUserSearch?ID_NguoiDung=" + _IDDoiTuong + "&TenDonVi=" + _nameChiNhanhKM, "GET").done(function (data) {
            self.DonVis(data);
            self.searchDonVi(data);
            if (self.DonVis().length < 2)
                $('.showChiNhanh').hide();
            else
                $('.showChiNhanh').show();
            for (var i = 0; i < self.DonVis().length; i++) {
                if (i == 0) {
                    _nameChiNhanhKM = self.DonVis()[i].ID;
                }
                else {
                    _nameChiNhanhKM = self.DonVis()[i].ID + "," + _nameChiNhanhKM;
                }
            }
            for (var i = 0; i < self.DonVis().length; i++) {
                if (self.DonVis()[i].ID == _id_DonVi) {
                    self.TenChiNhanh(self.DonVis()[i].TenDonVi);
                    self.SelectedDonVi_Use(self.DonVis()[i]);
                    break;
                }
            }
            GetListPromotion();
        });
    }
    self.IDSelectedDV = ko.observableArray();
    $(document).on('click', '.per_ac1 li', function () {
        var ch = $(this).index();
        $(this).remove();
        var li = document.getElementById("selec-person");
        var list = li.getElementsByTagName("li");
        for (var i = 0; i < list.length; i++) {
            $("#selec-person ul li").eq(ch).find(".fa-check").css("display", "none");
        }
        var nameDV = _id_DonVi.split('-');
        _id_DonVi = null;
        for (var i = 0; i < nameDV.length; i++) {
            if (nameDV[i].trim() != $(this).text().trim()) {
                if (_id_DonVi == null) {
                    _id_DonVi = nameDV[i];
                }
                else {
                    _id_DonVi = nameDV[i] + "-" + _id_DonVi;
                }
            }
        }

    })

    self.CloseDonVi_Use = function (item) {
        _id_DonVi = null;
        var TenChiNhanh;
        self.MangChiNhanh_Use.remove(item);
        for (var i = 0; i < self.MangChiNhanh_Use().length; i++) {
            if (_id_DonVi == null) {
                _id_DonVi = self.MangChiNhanh_Use()[i].ID;
                TenChiNhanh = self.MangChiNhanh_Use()[i].TenDonVi;
            }
            else {
                _id_DonVi = self.MangChiNhanh_Use()[i].ID + "," + _id_DonVi;
                TenChiNhanh = TenChiNhanh + ", " + self.MangChiNhanh_Use()[i].TenDonVi;
            }
        }
        if (self.MangChiNhanh_Use().length === 0) {
            $("#NoteNameDonVi_Use").attr("placeholder", "Chọn chi nhánh...");
            TenChiNhanh = 'Tất cả chi nhánh.'
            for (var i = 0; i < self.searchDonVi().length; i++) {
                if (_id_DonVi == null)
                    _id_DonVi = self.searchDonVi()[i].ID;
                else
                    _id_DonVi = self.searchDonVi()[i].ID + "," + _id_DonVi;
            }
        }
        self.TenChiNhanh(TenChiNhanh);
        $('#selec-all-DonVi_Use li').each(function () {
            if ($(this).attr('id_use') === item.ID) {
                $(this).find('i').remove();
            }
        });
        _numberPage = 1;
        GetListPromotion();
    }

    self.SelectedDonVi_Use = function (item) {
        _id_DonVi = null;
        var TenChiNhanh;
        var arrIDDonVi = [];
        for (var i = 0; i < self.MangChiNhanh_Use().length; i++) {
            if ($.inArray(self.MangChiNhanh_Use()[i], arrIDDonVi) === -1) {
                arrIDDonVi.push(self.MangChiNhanh_Use()[i].ID);
            }
        }
        if ($.inArray(item.ID, arrIDDonVi) === -1) {
            self.MangChiNhanh_Use.push(item);
            $('#NoteNameDonVi_Use').removeAttr('placeholder');
            for (var i = 0; i < self.MangChiNhanh_Use().length; i++) {
                if (_id_DonVi == null) {
                    _id_DonVi = self.MangChiNhanh_Use()[i].ID;
                    TenChiNhanh = self.MangChiNhanh_Use()[i].TenDonVi;
                }
                else {
                    _id_DonVi = self.MangChiNhanh_Use()[i].ID + "," + _id_DonVi;
                    TenChiNhanh = TenChiNhanh + ", " + self.MangChiNhanh_Use()[i].TenDonVi;
                }
            }
            self.TenChiNhanh(TenChiNhanh);
            _numberPage = 1;
        }
        //thêm dấu check vào đối tượng được chọn
        $('#selec-all-DonVi_Use li').each(function () {
            if ($(this).attr('id_use') === item.ID) {
                $(this).find('i').remove();
                $(this).append('<i class="fa fa-check check-after-li"></i>')
            }
        });

    }
    //lọc đơn vị
    self.NoteNameDonVi_Use = function () {
        var arrDonVi = [];
        var itemSearch = locdau($('#NoteNameDonVi_Use').val().toLowerCase());
        for (var i = 0; i < self.searchDonVi().length; i++) {
            var locdauInput = locdau(self.searchDonVi()[i].TenDonVi).toLowerCase();
            var R = locdauInput.split(itemSearch);
            if (R.length > 1) {
                arrDonVi.push(self.searchDonVi()[i]);
            }
        }
        self.DonVis(arrDonVi);
        if ($('#NoteNameDonVi_Use').val() == "") {
            self.DonVis(self.searchDonVi());
        }
    }
    $('#NoteNameDonVi_Use').keypress(function (e) {
        if (e.keyCode == 13 && self.DonVis().length > 0) {
            self.SelectedDonVi_Use(self.DonVis()[0]);
        }
    });

    self.Filter_TypePromotion.subscribe(function () {
        GetListPromotion();
    });

    self.Filter_Expired.subscribe(function () {
        GetListPromotion();
    });

    self.StatusActive.subscribe(function () {
        GetListPromotion();
    });

    function GetListPromotion() {
        var txt = $('#EnterMaKM').val();
        var arrIDDonVi = [];
        for (var i = 0; i < self.MangChiNhanh_Use().length; i++) {
            if ($.inArray(self.MangChiNhanh_Use()[i], arrIDDonVi) === -1) {
                arrIDDonVi.push(self.MangChiNhanh_Use()[i].ID);
            }
        }
        if (arrIDDonVi.length === 0) {
            arrIDDonVi = [_id_DonVi];
        }
        var obj = {
            IDChiNhanhs: arrIDDonVi,
            TextSearch: txt,
            StatusActive: self.StatusActive(),
            TypePromotion: self.Filter_TypePromotion(),
            Expired: self.Filter_Expired(),
            CurrentPage: self.CurrentPage() - 1,
            PageSize: self.pageSize(),
        };
        ajaxHelper(BH_KhuyenMaiUri + 'GetListPromotion', 'POST', obj).done(function (x) {
            if (x.res === true) {

                if (x.data.length > 0) {
                    AllPage = x.ToTalPage;
                    self.filteredDM_KhuyenMai(x.data);
                    self.PagesKhuyenMai([]);
                    for (let i = 0; i < x.TotalPage; i++) {
                        self.PagesKhuyenMai.push({ SoTrang: i + 1 });
                    }
                    self.RowsStart((_numberPage - 1) * _numberRowns + 1);
                    self.RowsEnd((_numberPage - 1) * _numberRowns + self.filteredDM_KhuyenMai().length);
                }
                else {
                    self.filteredDM_KhuyenMai([]);
                    self.RowsStart('0');
                    self.RowsEnd('0');
                }
                self.RowsKhuyenMai(x.TotalRow);
                self.ReserPage();
            }
            else {
                ShowMessage_Danger(x.mes);
            }
        });
    }

    var _id_LSKhuyenMai = null;
    self.getList_LichSuKhuyenMai = function () {
        ajaxHelper(BH_KhuyenMaiUri + "getList_LichSuKhuyenMai?ID_KhuyenMai=" + _id_LSKhuyenMai + "&numberPage=" + _numberPage_LS + "&PageSize=" + _numberRowns_LS, "GET").done(function (data) {
            self.BH_LichSuKhuyenMai(data.LstData);
            if (self.BH_LichSuKhuyenMai().length > 0) {
                self.RowsStart_LS((_numberPage_LS - 1) * _numberRowns_LS + 1);
                self.RowsEnd_LS((_numberPage_LS - 1) * _numberRowns_LS + self.BH_LichSuKhuyenMai().length)
            }
            else {
                self.RowsStart_LS('0');
                self.RowsEnd_LS('0');
            }
            self.Rows_LichSuKhuyenMai(data.Rowcount);
            self.Pages_LichSuKhuyenMai(data.LstPageNumber);
            AllPage_LS = self.Pages_LichSuKhuyenMai().length;
            self.ReserPage_LS();

        });
    }
    function hidewait(o) {
        $('.' + o).append('<div id="wait"><img src="/Content/images/wait.gif" width="64" height="64" /><div class="happy-wait">' +
            ' </div>' +
            '</div>')
    }
    self.gotoNextPage = function (item) {
        try {
            var kt = item.SoTrang;
            _numberPage = item.SoTrang;
            self.CurrentPage(item.SoTrang);
            GetListPromotion();
        }
        catch (e) {
        }

    }
    // Phân trang lich su khuyen mai
    self.gotoNextPage_LS = function (item) {
        try {
            var kt = item.SoTrang;
            _numberPage_LS = item.SoTrang;
            self.getList_LichSuKhuyenMai();
        }
        catch (e) {
        }

    }
    self.GetClass_LS = function (page) {
        return (page.SoTrang === self.currentPage_LS()) ? "click" : "";
    };
    self.selecPage_LS = function () {
        if (AllPage_LS > 4) {
            for (var i = 3; i < AllPage_LS; i++) {
                self.Pages_LichSuKhuyenMai.pop(i + 1);
            }
            self.Pages_LichSuKhuyenMai.push({ SoTrang: '4' });
            self.Pages_LichSuKhuyenMai.push({ SoTrang: '5' });
        }
        else {
            for (var i = 0; i < 6; i++) {
                self.Pages_LichSuKhuyenMai.pop(i);
            }
            for (var j = 0; j < AllPage_LS; j++) {
                self.Pages_LichSuKhuyenMai.push({ SoTrang: j + 1 });
            }
        }
        $('#StartPage_LS' + _id_LSKhuyenMai).hide();
        $('#BackPage_LS' + _id_LSKhuyenMai).hide();
        $('#NextPage_LS' + _id_LSKhuyenMai).show();
        $('#EndPage_LS' + _id_LSKhuyenMai).show();
    }
    self.ReserPage_LS = function (item) {
        self.selecPage_LS();
        if (_numberPage_LS > 1 && AllPage_LS > 5/* && nextPage < AllPage - 1*/) {
            if (_numberPage_LS > 3 && _numberPage_LS < AllPage_LS - 1) {
                for (var i = 0; i < 5; i++) {
                    self.Pages_LichSuKhuyenMai.replace(self.Pages_LichSuKhuyenMai()[i], { SoTrang: parseInt(_numberPage_LS) + i - 2 });
                }
            }
            else if (parseInt(_numberPage_LS) === parseInt(AllPage_LS) - 1) {
                for (var i = 0; i < 5; i++) {
                    self.Pages_LichSuKhuyenMai.replace(self.Pages_LichSuKhuyenMai()[i], { SoTrang: parseInt(_numberPage_LS) + i - 3 });
                }
            }
            else if (_numberPage_LS == AllPage_LS) {
                for (var i = 0; i < 5; i++) {
                    self.Pages_LichSuKhuyenMai.replace(self.Pages_LichSuKhuyenMai()[i], { SoTrang: parseInt(_numberPage_LS) + i - 4 });
                }
            }
        }
        self.currentPage_LS(parseInt(_numberPage_LS));
        if (_numberPage_LS > 1) {
            $('#StartPage_LS' + _id_LSKhuyenMai).show();
            $('#BackPage_LS' + _id_LSKhuyenMai).show();
        }
        else {
            $('#StartPage_LS' + _id_LSKhuyenMai).hide();
            $('#BackPage_LS' + _id_LSKhuyenMai).hide();
        }
        if (_numberPage_LS == AllPage_LS) {
            $('#NextPage_LS' + _id_LSKhuyenMai).hide();
            $('#EndPage_LS' + _id_LSKhuyenMai).hide();
        }
        else {
            $('#NextPage_LS' + _id_LSKhuyenMai).show();
            $('#EndPage_LS' + _id_LSKhuyenMai).show();
        }
    }
    self.NextPage_LS = function (item) {
        if (_numberPage_LS < AllPage_LS) {
            _numberPage_LS = _numberPage_LS + 1;
            self.ReserPage_LS();
            self.getList_LichSuKhuyenMai();
        }
    };
    self.BackPage_LS = function (item) {
        if (_numberPage_LS > 1) {
            _numberPage_LS = _numberPage_LS - 1;
            self.ReserPage_LS();
            self.getList_LichSuKhuyenMai();
        }
    };
    self.EndPage_LS = function (item) {
        _numberPage_LS = AllPage_LS;
        self.ReserPage_LS();
        self.getList_LichSuKhuyenMai();
    };
    self.StartPage_LS = function (item) {
        _numberPage_LS = 1;
        self.getList_LichSuKhuyenMai();
        self.ReserPage_LS();
    };
    // Phân trang
    self.GetClass = function (page) {
        return (page.SoTrang === self.CurrentPage()) ? "click" : "";
    };
    self.selecPage = function () {
        AllPage = self.PagesKhuyenMai().length;
        if (AllPage > 4) {
            for (var i = 3; i < AllPage; i++) {
                self.PagesKhuyenMai.pop(i + 1);
            }
            self.PagesKhuyenMai.push({ SoTrang: '4' });
            self.PagesKhuyenMai.push({ SoTrang: '5' });
        }
        else {
            for (var i = 0; i < 6; i++) {
                self.PagesKhuyenMai.pop(i);
            }
            for (var j = 0; j < AllPage; j++) {
                self.PagesKhuyenMai.push({ SoTrang: j + 1 });
            }
        }
        $('#StartPage').hide();
        $('#BackPage').hide();
        $('#NextPage').show();
        $('#EndPage').show();
    }
    self.ReserPage = function (item) {
        self.selecPage();
        if (_numberPage > 1 && AllPage > 5/* && nextPage < AllPage - 1*/) {
            if (_numberPage > 3 && _numberPage < AllPage - 1) {
                for (var i = 0; i < 5; i++) {
                    self.PagesKhuyenMai.replace(self.PagesKhuyenMai()[i], { SoTrang: parseInt(_numberPage) + i - 2 });
                }
            }
            else if (parseInt(_numberPage) === parseInt(AllPage) - 1) {
                for (var i = 0; i < 5; i++) {
                    self.PagesKhuyenMai.replace(self.PagesKhuyenMai()[i], { SoTrang: parseInt(_numberPage) + i - 3 });
                }
            }
            else if (_numberPage == AllPage) {
                for (var i = 0; i < 5; i++) {
                    self.PagesKhuyenMai.replace(self.PagesKhuyenMai()[i], { SoTrang: parseInt(_numberPage) + i - 4 });
                }
            }
        }
        self.CurrentPage(parseInt(_numberPage));
        if (_numberPage > 1) {
            $('#StartPage').show();
            $('#BackPage').show();
        }
        else {
            $('#StartPage').hide();
            $('#BackPage').hide();
        }
        if (_numberPage == AllPage) {
            $('#NextPage').hide();
            $('#EndPage').hide();
        }
        else {
            $('#NextPage').show();
            $('#EndPage').show();
        }
    }
    self.NextPage = function (item) {
        if (_numberPage < AllPage) {
            _numberPage = _numberPage + 1;
            self.ReserPage();
            GetListPromotion();
        }
    };
    self.BackPage = function (item) {
        if (_numberPage > 1) {
            _numberPage = _numberPage - 1;
            self.ReserPage();
            GetListPromotion();
        }
    };
    self.EndPage = function (item) {
        _numberPage = AllPage;
        self.ReserPage();
        GetListPromotion();
    };
    self.StartPage = function (item) {
        _numberPage = 1;
        GetListPromotion();
        self.ReserPage();
    };

    self.Quyen_NguoiDung = ko.observableArray();

    function CheckQuyenExist(maquyen) {
        var role = $.grep(self.Quyen_NguoiDung(), function (x) {
            return x.MaQuyen === maquyen;
        });
        return role.length > 0;
    }

    function GetQuyenNguoiDung() {
        ajaxHelper('/api/DanhMuc/HT_NguoiDungAPI/' + "GetListQuyen_OfNguoiDung", 'GET').done(function (data) {
            self.Quyen_NguoiDung(data);
        });
    };

    $('#datetimepicker_mask1').change(function (e) {
        var thisDate = $(this).val().trim();
        var a = thisDate.split(" ");
        _timeEnd = a[0].split("/").reverse().join("-") + " " + a[1];
    });
    $('#datetimepicker_mask').change(function (e) {
        var thisDate = $(this).val().trim();
        var a = thisDate.split(" ");
        _timeStart = a[0].split("/").reverse().join("-") + " " + a[1];
    });


    self.loadDieuKienKM = function (item, e) {
        _id_LSKhuyenMai = item.ID;
        _hinhthucKM = item.KieuHinhThuc;
        self.HinhThucKM(item.KieuHinhThuc);
        ajaxHelper(BH_KhuyenMaiUri + "getChiTiet_KhuyenMai?ID_KhuyenMai=" + item.ID, "GET").done(function (data) {
            self.BH_KhuyenMai_ChiTiet(data);
            SetHeightShowDetail($(e.currentTarget));
        });
    }
    //Sao chép
    self.CopyKhuyenMai = function (item) {
        vmThemMoiKhuyenMai.showModalUpdate(item.ID, 1);// 1. saochep, 2.update
    }
    self.UpdateKhuyenMai = function (item) {
        vmThemMoiKhuyenMai.showModalUpdate(item.ID);
    }

    // cập nhật
    self.CN_maKM = ko.observable();
    self.CN_TenKM = ko.observable();
    self.CN_TrangThaiKM = ko.observable('1');
    self.CN_Ghichu = ko.observable();
    self.CN_CheckApDungSN = ko.observable();
    self.KieuApDungSN = ko.observable('Ngày sinh nhật')
    self.CN_checkChiNhanh = ko.observable('1');
    self.CN_checkNguoiBan = ko.observable('1');
    self.CN_checkNhomKhachHang = ko.observable('1');

    var _dieukienSave = 1;

    self.showModalAddKhuyenMai = function () {
        vmThemMoiKhuyenMai.showModal();
    }
    var _ID_KhuyenMai_Delete;
    var _Ma_KhuyenMai_Delete;
    var itemDelete;
    self.maKhuyenMaiDelete = ko.observable();
    self.modalDelete = function (item) {
        itemDelete = item;
        $('#modalpopup_deleteHD').modal('show');
        _ID_KhuyenMai_Delete = item.ID;
        _Ma_KhuyenMai_Delete = item.MaKhuyenMai;
        self.maKhuyenMaiDelete(_Ma_KhuyenMai_Delete);
    };
    self.xoaKhuyenMai = function () {
        ajaxHelper(BH_KhuyenMaiUri + "deleteKhuyenMai?ID_KhuyenMai=" + _ID_KhuyenMai_Delete + "&ID_DonVi=" + _id_DonVi + "&ID_NhanVien=" + _id_NhanVienLS, "GET").done(function (data) {
            var str = data;
            if (str.length > 0) {
                if (str == 'HD')
                    bottomrightnotify('<i class="fa fa-check" aria-hidden="true"></i>' + "Chương trình khuyến mại: " + _Ma_KhuyenMai_Delete + " đã được áp dụng", "danger");
                else
                    bottomrightnotify('<i class="fa fa-check" aria-hidden="true"></i>' + "Xóa bỏ mã chương trình khuyến mại: " + _Ma_KhuyenMai_Delete + " không thành công", "danger");
            }
            else {
                bottomrightnotify('<i class="fa fa-check" aria-hidden="true"></i>' + "Xóa bỏ mã chương trình khuyến mại: " + _Ma_KhuyenMai_Delete + " thành công", "success");
                //self.filteredDM_KhuyenMai.remove(itemDelete);
                GetListPromotion();
            }
        });
        $('#modalpopup_deleteHD').modal('hide');
    }

    //===============================
    // Load lai form lưu cache bộ lọc 
    // trên grid 
    //===============================
    function LoadHtmlGrid() {
        if (window.localStorage) {
            var current = localStorage.getItem('QLkhuyenmai');
            if (!current) {
                current = [{
                    NameClass: ".m4",
                    NameId: "e4"
                },
                {
                    NameClass: ".m5",
                    NameId: "e5"
                }];
                for (var i = 0; i < current.length; i++) {
                    $(current[i].NameClass).addClass("operation");
                    document.getElementById(current[i].NameId).checked = false;

                }
                localStorage.setItem('QLkhuyenmai', JSON.stringify(current));
            } else {
                current = JSON.parse(current);
                for (var i = 0; i < current.length; i++) {
                    $(current[i].NameClass).addClass("operation");
                    document.getElementById(current[i].NameId).checked = false;

                }
            }
        }
    }
    //===============================
    // Add Các tham số cần lưu lại để 
    // cache khi load lại form
    //===============================
    function addClass(name, id, value) {

        var current = localStorage.getItem('QLkhuyenmai');
        if (!current) {
            current = [{
                NameClass: ".m4",
                NameId: "e4"
            },
            {
                NameClass: ".m5",
                NameId: "e5"
            }];
        } else {
            current = JSON.parse(current);
        }
        if (current.length > 0) {
            for (var i = 0; i < current.length; i++) {
                if (current[i].NameId === id.toString()) {
                    current.splice(i, 1);
                    break;
                }
                if (i == current.length - 1) {
                    current.push({
                        NameClass: name,
                        NameId: id,
                        Value: value
                    });
                    break;
                }
            }
        }
        else {
            current.push({
                NameClass: name,
                NameId: id,
                Value: value
            });
        }
        localStorage.setItem('QLkhuyenmai', JSON.stringify(current));
    }
    $("#e0").click(function () {
        $(".m0").toggle();
        addClass(".m0", "e0", $(this).val());
    });
    $("#e1").click(function () {
        $(".m1").toggle();
        addClass(".m1", "e1", $(this).val());
    });
    $("#e2").click(function () {
        $(".m2").toggle();
        addClass(".m2", "e2", $(this).val());
    });
    $("#e3").click(function () {
        $(".m3").toggle();
        addClass(".m3", "e3", $(this).val());
    });
    $("#e4").click(function () {
        $(".m4").toggle();
        addClass(".m4", "e4", $(this).val());
    });
    $("#e5").click(function () {
        $(".m5").toggle();
        addClass(".m5", "e5", $(this).val());
    });
    $("#e6").click(function () {
        $(".m6").toggle();
        addClass(".m6", "e6", $(this).val());
    });
    $("#e7").click(function () {
        $(".m7").toggle();
        addClass(".m7", "e7", $(this).val());
    });

    function Page_Load() {
        getQuyen_NguoiDung();
        getDonVi();

    }
    Page_Load();
}
var vmKhuyenMai = new ViewModel();
ko.applyBindings(vmKhuyenMai);


$('#vmThemMoiKhuyenMai').on('hidden.bs.modal', function () {
    if (vmThemMoiKhuyenMai.saveOK) {
        GetListPromotion();
    }
})


function getNumber(e, obj) {
    var elementAfer = $(obj).next();
    if (elementAfer.css('display') == 'none') {
        return keypressNumber(e);
    }
    else {
        if (elementAfer.hasClass('active-re')) {// %
            var keyCode = window.event.keyCode || e.which;
            if (keyCode < 48 || keyCode > 57) {
                // cho phep nhap dau .
                if (keyCode === 8 || keyCode === 127 || keyCode === 46) {
                    return;
                }
                return false;
            }
        }
        else {
            // chi cho phep nhap so
            return keypressNumber(e);
        }
    }
}
