// Get a reference to our hub
var hub = $.connection.cpuInfo;

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
    };

    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#computerInfo")[0]);
    ko.applyBindings(vm, $("#servicesInfo")[0]);
    ko.applyBindings(vm, $("#sitesIIS")[0]);

    /*
    // Get a reference to our hub
    var hub = $.connection.cpuInfo;
    */

    // Add a handler to receive updates from the server
    hub.client.cpuInfoMessage = function (machineName, cpu, memUsage, memTotal, services, ips, disk, sysos, procesador, filesVersion, pais, iisSites) {
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
            iisSites: iisSites
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

//function serviceStart() {
//    var txt = $(this).text();
//    console.log(txt);
//}
