/*
Newgen Runtime Shared Data @ Widget API
---------------------------------------
© 2014 NS, Neeraj.
---------------------------------------
*/

newgen = new (function () {
    this.ServerLocation = function () { return 'http://##SL/'; }
    this.LicenseStatus = function () { return '##LS'; }
    this.Version = function () { return '##V'; }
    this.CloseHub = function () { window.location = '$Newgen::CloseHub'; return true; }

    this.SendMessage = function (key, msg) { $Newgen.getResponse('SendMessage', key + ';' + msg, null); }
    this.ASD = function (wn, data, callback) { $Newgen.getResponse('ASD', wn + ';' + data, callback); }
    this.GSD = function (wn, callback) { $Newgen.getResponse('GSD', wn, callback); }
    this.CSD = function (wn, callback) { $Newgen.getResponse('CSD', wn, callback); }

    this.getResponse = function (rell, data, callback, type) {
        type = type || 'text';
        var result = null;
        var xmlHttp = new XMLHttpRequest();
        xmlHttp.open('GET', $Newgen.ServerLocation() + rell + ':' + data, true);
        if (type == 'xml') { xmlHttp.setRequestHeader('Content-type', 'text/xml; charset=utf-8'); }
        if (type == 'text') { xmlHttp.setRequestHeader('Content-type', 'text/plain; charset=utf-8'); }
        xmlHttp.onreadystatechange = function () {
            if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
            { result = xmlHttp.responseText; if (callback != null) callback(result); }
        }
        xmlHttp.send();
    }

    return 'Newgen';
});
window.$Newgen = newgen;