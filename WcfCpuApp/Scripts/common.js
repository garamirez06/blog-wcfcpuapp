function copiarAlPortapapeles(id_elemento) {
    var aux = document.createElement("input");
    aux.setAttribute("value", document.getElementById(id_elemento).innerText);
    document.body.appendChild(aux);
    aux.select();
    document.execCommand("copy");
    document.body.removeChild(aux);
}

function exportToExcel(table,name) {
    var uri = 'data:application/vnd.ms-excel;base64,'
        , template = '<html xmlns: <head><!--[if gte mso 9]><xml><x: <x: <x: <x: {worksheet}</x: <x: <x:DisplayGridlines/></x:WorksheetOptions></x: ExcelWorksheet ></x: ExcelWorksheets ></x: ExcelWorkbook ></xml >< ![endif]-- > <meta http-equiv="content-type" content="text/plain; charset=UTF-8" /></head > <body><table>{table}</table></body></html > '
        , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
        , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    return function (table, name) {
        if (!table.nodeType) table = document.getElementById(table)
        var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML }
        window.location.href = uri + base64(format(template, ctx))
    }
}

function GetCountries() {


    $.ajax({
        url: "Api/cpuInfo/getData",
        timeout: 50000,
        type: "GET",
        dataType: 'json',
        contentType: "application/json;charset=utf-8",
        success: function (result) {
            console.log("volvi bebecito " + result);
            //fillCountryOptions(JSON.parse(result));
            /* console.log(JSON.stringify(obj));
 
             if (result.Result == 'OK')
                 console.log(JSON.stringify(obj));
 
             if (result.Result == 'ERROR') {
                 console.log(JSON.stringify(obj));
             }*/
        },
        error: function (jqXHR, textStatus) {
            console.log(textStatus);
        }
    });
}