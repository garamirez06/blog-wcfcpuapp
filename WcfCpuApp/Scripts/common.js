function exportTableToExcel(tableID, filename = '') {
    var table = document.getElementById(tableID);
    var cloneTable = table.cloneNode(true);
    jQuery(cloneTable).find('.remove-this').remove();

    var downloadLink;
    var dataType = 'application/vnd.ms-excel;charset=UTF-8';
    var tableSelect = cloneTable;

    var tableHTML = tableSelect.outerHTML.replace(/ /g, '%20');
    tableHTML = tableHTML.replace("País", "Pais");
    tableHTML = tableHTML.replace("Versión", "Version");
    tableHTML = tableHTML.replace("Número", "Numero");
    tableHTML = tableHTML.replace("Á", "A");
    tableHTML = tableHTML.replace("á", "a");
    tableHTML = tableHTML.replace("É", "E");
    tableHTML = tableHTML.replace("é", "e");
    tableHTML = tableHTML.replace("Í", "I");
    tableHTML = tableHTML.replace("í", "i");
    tableHTML = tableHTML.replace("Ó", "O");
    tableHTML = tableHTML.replace("ó", "o");
    tableHTML = tableHTML.replace("Ú", "U");
    tableHTML = tableHTML.replace("ú", "u");


    // Specify file name
    filename = filename ? filename + '.xls' : 'excel_data.xls';

    // Create download link element
    downloadLink = document.createElement("a");

    document.body.appendChild(downloadLink);

    if (navigator.msSaveOrOpenBlob) {
        var blob = new Blob(['\ufeff', tableHTML], {
            type: dataType
        });
        navigator.msSaveOrOpenBlob(blob, filename);
    } else {
        // Create a link to the file
        downloadLink.href = 'data:' + dataType + ', ' + tableHTML;

        // Setting the file name
        downloadLink.download = filename;

        //triggering the function
        downloadLink.click();
    }

}




var selectedCountry = "Todos";
var selectedServer = "Todos";
var selectedService = "Todos";

function searchObject(textSearch, tableID, columnID, exact,columnAnt) {
    // Declare variables
    var input, filter, table, tr, td, i, txtValue;
    input = textSearch;
    filter = input.toUpperCase();
    table = document.getElementById(tableID);
    tr = table.getElementsByTagName("tr");

    var colSer = 2;
    var colPC = 1;
    var colPais = 0;

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 0; i < tr.length; i++) {
        //Recorro fila por fila.. obtengo la columna 1
        td = tr[i].getElementsByTagName("td")[columnID];
        if (td) {
            txtValue = td.textContent || td.innerText;
            if (exact) {
                if (txtValue.toUpperCase().indexOf(filter) > -1) {
                    tr[i].style.display = "";
                    //Aplica solo a servicios
                    if (colSer != columnID && selectedService !== "Todos" && colSer!==columnAnt && tableID == 'tbServices') {
                        searchObject(selectedService, tableID, 2, 1,columnID);
                    }
                    //Busco Servidor
                    if (colPC != columnID && selectedServer !== "Todos" && colPC !== columnAnt) {
                        searchObject(selectedServer, tableID, 1, 1, columnID);
                    }
                    //Busco Pais
                    if (colPais != columnID && selectedCountry !== "Todos" && colPC !== columnAnt) {
                        searchObject(selectedCountry, tableID, 0, 1, columnID);
                    }
                }
                else {
                    if (textSearch == "Todos") {
                        tr[i].style.display = "";
                    }
                    else {
                        tr[i].style.display = "none";
                    }
                }
            }
            else {
                if (txtValue.toUpperCase().includes(filter.toUpperCase())) {
                    tr[i].style.display = "";
                }
                else {
                    if (textSearch == "Todos") {
                        tr[i].style.display = "";
                    }
                    else {
                        tr[i].style.display = "none";
                    }
                }
            }

        }

    }
}


function searchStatus(tableID, columnID) {
    // Declare variables
    var input, filter, table, tr, td, i, txtValue;
    var textSearch = "";
    input = textSearch;
    filter = input.toUpperCase();
    table = document.getElementById(tableID);
    tr = table.getElementsByTagName("tr");
    // Loop through all table rows, and hide those who don't match the search query
    for (i = 1; i <= tr.length-1; i++) {
        //Recorro fila por fila.. obtengo la columna 1
        var td2 = tr[i].getElementsByTagName("td")[columnID];
        td = tr[i].getElementsByClassName("ui-state-default")[columnID];
        if (td) {
            txtValue = td.textContent || td.innerText;
            if (txtValue.toUpperCase().indexOf("ACTIVO") > -1 || txtValue.toUpperCase().indexOf("RUNNING") > -1 || txtValue.toUpperCase().indexOf("STARTED") > -1) {
                tr[i].cells[columnID].style.backgroundColor = "green";
                tr[i].cells[columnID].style.color = "white";
                tr[i].cells[columnID].style.fontWeight = "bold";

            } else {
                tr[i].cells[columnID].style.backgroundColor = "red";
                tr[i].cells[columnID].style.color = "white";
                tr[i].cells[columnID].style.fontWeight = "bold";
            }
        }
    }
}

function sendStartService(machineName, serviceName) {
    hub.server.sendMessageStart(machineName, serviceName);
}
