// Get a reference to our hub
var hub = $.connection.cpuInfo;
var mono = "mono";

$(function () {
    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Whether we're connected or not
        self.connected = ko.observable(false);

        // Collection of machines that are connected
        self.machines = ko.observableArray();
        self.services = ko.observableArray();
        self.sites = ko.observableArray();
        self.sitesNode = ko.observableArray();
    };

    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#computerInfo")[0]);
    ko.applyBindings(vm, $("#servicesInfo")[0]);
    ko.applyBindings(vm, $("#sitesIIS")[0]);
    ko.applyBindings(vm, $("#sitesNode")[0]);

    /*
    // Get a reference to our hub
    var hub = $.connection.cpuInfo;
    */

    // Add a handler to receive updates from the server
    hub.client.cpuInfoMessage = function (machineName, cpu, memUsage, memTotal, services, ips, disk, sysos, procesador, filesVersion, pais, iisSites, processNode, descriptionServer) {
        var country = pais;
        disk = disk.replace("\r\n", "<br>", "g");
        var machine = {
            machineName: machineName,
            cpu: cpu.toFixed(0),
            memUsage: (memUsage / 1024).toFixed(2),
            memTotal: (memTotal / 1024).toFixed(2),
            memPercent: ((memUsage / memTotal) * 100).toFixed(1) + "%",
            services: services,
            ipAddress: ips,
            disk: disk,
            sysos: sysos,
            processador: procesador,
            filesVersion: filesVersion,
            pais: pais,
            iisSites: iisSites,
            processNode: processNode,
            status: "ACTIVO",
            descriptionServer: descriptionServer
        };


        var machineModel = ko.mapping.fromJS(machine);
        // Check if we already have it:
        var match = ko.utils.arrayFirst(vm.machines(), function (item) {
            return item.machineName() == machineName;
        });

        if (!match)
            vm.machines.push(machineModel);
        else {
            var index = vm.machines.indexOf(match);
            vm.machines.replace(vm.machines()[index], machineModel);
        }

        //Procesamos servicios
        var json = JSON.parse(services);

        jQuery.each(json, function () {
            var pais = this.pais;
            var machineName = this.machineName;
            var serviceName = this.serviceName;
            var serviceDisplayName = this.serviceDisplayName;
            var serviceType = this.serviceType;
            var status = this.status;
            var startType = this.startType;
            var Path = this.Path;
            var serviceVersion = this.serviceVersion;
            var filesVersion = this.filesVersion;
            //var connectionID = this.connectionID;
            var service = {
                pais: this.pais,
                machineName: this.machineName,
                serviceName: this.serviceName,
                serviceDisplayName: this.serviceDisplayName,
                serviceType: this.serviceType,
                status: this.status,
                startType: this.startType,
                path: this.Path,
                serviceVersion: this.serviceVersion,
                filesVersion: this.filesVersion
                //connectionID: this.connectionID
            };

            var serviceModel = ko.mapping.fromJS(service);

            // Check if we already have it:
            var flag;
            /*
             * //Filtro para combos
            if (mono !== pais) {
                console.log("NO hay resultados");
                for (var i = 0; i < vm.services().length; i++) {
                    vm.services.remove(vm.services()[i]);
                }

            }
            */
            const datos = vm.services();
            //Filtro para ver si existe el servicio
            const searchService = datos.find(serv => serv.machineName() === machineName && serv.serviceName() === serviceName && serv.status() === status);
            //Filtro por Estado
            const searchStatus = datos.find(serv => serv.machineName() === machineName && serv.serviceName() === serviceName && serv.status() !== status);
            //Filtro por Version
            const searchVersion = datos.find(serv => serv.machineName() === machineName && serv.serviceName() === serviceName && serv.serviceVersion() !== serviceVersion);

            if (typeof searchStatus !== 'undefined') {
                var index = vm.services.indexOf(searchStatus);
                vm.services.replace(vm.services()[index], serviceModel);
            }
            else {
                if (typeof searchService === 'undefined')
                    vm.services.push(serviceModel);
            }

            if (typeof searchVersion !== 'undefined') {
                var index2 = vm.services.indexOf(searchVersion);
                vm.services.replace(vm.services()[index2], serviceModel);
            }



        });

        //Procesamos los IIS Sites
        var jsonSites = JSON.parse(iisSites);
        jQuery.each(jsonSites, function () {
            var pais = this.pais;
            var machineName = this.machineName;
            var siteID = this.siteID;
            var siteName = this.siteName;
            var siteBinding = this.siteBinding;
            var siteState = this.siteState;
            var sitePath = this.sitePath;

            var site = {
                pais: this.pais,
                machineName: this.machineName,
                siteID: this.siteID,
                siteName: this.siteName,
                siteBinding: this.siteBinding,
                siteState: this.siteState,
                sitePath: this.sitePath
            };

            var siteModel = ko.mapping.fromJS(site);

            // Check if we already have it:
            var flag;

            const datos = vm.sites();
            //Filtro para ver si existe el Sitio
            const searchSite = datos.find(webSite => webSite.machineName() === machineName && webSite.siteName() === siteName && webSite.siteState() === siteState);
            //Filtro por Estado
            const searchStatus = datos.find(webSite => webSite.machineName() === machineName && webSite.siteName() === siteName && webSite.siteState() !== siteState);
            //Filtro por Enlace
            const searchBinding = datos.find(webSite => webSite.machineName() === machineName && webSite.siteName() === siteName && webSite.siteBinding() !== siteBinding);

            if (typeof searchStatus !== 'undefined') {
                var index = vm.sites.indexOf(searchStatus);
                vm.sites.replace(vm.sites()[index], siteModel);
            }
            else {
                if (typeof searchBinding !== 'undefined') {
                    var index2 = vm.sites.indexOf(searchBinding);
                    vm.sites.replace(vm.sites()[index2], siteModel);
                }
                if (typeof searchSite === 'undefined')
                    vm.sites.push(siteModel);
            }

        });

        //Procesamos los Node Sites
        var jsonSitesNode = JSON.parse(processNode);
        jQuery.each(jsonSitesNode, function () {
            var pais = this.pais;
            var machineName = this.machineName;
            var processID = this.processID;
            var siteName = this.siteName;
            var processName = this.processName;
            var instanceName = this.instanceName;
            var commandLine = this.commandLine;

            var siteNode = {
                pais: this.pais,
                machineName: this.machineName,
                processID: this.processID,
                processName: this.processName,
                instanceName: this.instanceName,
                commandLine: this.commandLine
            };

            var siteNodeModel = ko.mapping.fromJS(siteNode);

            // Check if we already have it:
            var flag;

            const datos = vm.sitesNode();
            //Filtro para ver si existe el Sitio
            const searchSite = datos.find(webSiteNode => webSiteNode.machineName() === machineName && webSiteNode.processName() === processName && webSiteNode.processID() === processID);
            //Filtro por Linea de Comando
            const searchLine = datos.find(webSiteNode => webSiteNode.machineName() === machineName && webSiteNode.processName() === processName && webSiteNode.processID() === processID && webSiteNode.commandLine() !== commandLine);

            if (typeof searchLine !== 'undefined') {
                var index = vm.sitesNode.indexOf(searchLine);
                vm.sitesNode.replace(vm.sitesNode()[index], siteNodeModel);
            }
            else {

                if (typeof searchSite === 'undefined')
                    vm.sitesNode.push(siteNodeModel);
            }

        });
        //Ejecutamos el Status
        searchStatus('tbMachines', 9);
        searchStatus('tbServices', 4);
        searchStatus('tbSitesIIS', 5);
    };

    //Add a handler to receive disconnect
    hub.client.onUserDisconnected = function (connectionID, machineName) {
        console.log(machineName + " se ha desconectado");
        notificar(machineName);
        var today = new Date();

        var input, filter, table, tr, tdName, tdStatus, i, txtValue;
        input = machineName;
        filter = input.toUpperCase();
        table = document.getElementById('tbMachines');
        tr = table.getElementsByTagName("tr");
        // Loop through all table rows, and hide those who don't match the search query
        for (i = 1; i <= tr.length - 1; i++) {
            //Recorro fila por fila.. obtengo la columna 1
            tdName = tr[i].getElementsByTagName("td")[1];
            if (tdName) {
                txtValue = tdName.textContent || tdName.innerText;
                if (txtValue.toUpperCase().indexOf(filter) > -1) {
                    tdStatus = tr[i].getElementsByTagName("td")[9]
                    txtValue = tdStatus.textContent || tdStatus.innerText;
                    if (txtValue.toUpperCase().indexOf("ACTIVO") > -1) {
                        tr[i].style.backgroundColor = "red";
                        tr[i].style.color = "white";
                        tdStatus.innerHTML = "DESCONECTADO - " + today.toLocaleString("es-AR");
                    }
                } else {
                    tr[i].style.display = "none";
                }
            }
        }

        //Ejecutamos el Status
        searchStatus('tbMachines', 9);
        searchStatus('tbServices', 4);
        searchStatus('tbSitesIIS', 5);

    };

    // Start the connection
    $.connection.hub.start().catch(err => console.error(err.toString())).then(function () {
        //Send the connectionId to controller
        var connectionID = $.connection.hub.id;
        console.log("Connection ID: " + connectionID);
        vm.connected(true);
        hub.server.connect();
    });

    
});

function notificar(serverName) {
    var dts = Math.floor(Date.now());
    if (Notification.permission === "granted") {
        var wNotify = new Notification("Alerta - Falcon", {
            body: "Problemas con el servidor " + serverName,
            icon: "/Images/falconIco4.png",
            badge: "/Images/falconIco4.png",
            requireInteraction: true,
            timestamp: dts
        });
        wNotify.onclick = (e) => {
            window.open("https://google.com", "_blank");
            wNotify.close();
        };

        setTimeout(function () { wNotify.close() }, 10000)
    }
}