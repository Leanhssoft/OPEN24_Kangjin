﻿function resizeIframe(ifrm) {
    var height = $('#Idframe').context.body.scrollHeight;
    ifrm.style.height = height + 'px';
}
(function (d, s, id) {
    var url = window.location.search.replace('?', '').split('&').filter(o => o.indexOf('utm_user') !== 1);
    var search = '?sub=';
    var dataid = $('#' + id).data('id').split('_');
    if (dataid.length > 1) {
        search += dataid[0] + "&id_optin=" + dataid[1] + '&utm_user=';
    }
    else {
        search += dataid[0] + '&utm_user=';
    }
    //var filepath = "https://" + dataid[0]+".open24.vn";
    //var filepath = "https://0973474985.open24.test";
    var filepath = "https://localhost:44382";
    if (url.length > 0 && url[0]!=='') {
        search += url[0].split('=')[1].split('_')[0];
       
    }
    filepath += "/mark/GetLinkFormCustomer" + search;
    var ifrm = document.createElement("iframe");
    ifrm.setAttribute("src", filepath);
    //ifrm.setAttribute("onload", "resizeIframe(this)");
    ifrm.style.width = "100%";
    ifrm.style.height = "860px";
    ifrm.style.border = "  none";
    ifrm.id = "Idframe";
    $('#' + id).append(ifrm);
}(document, 'script', 'open24-root'))
