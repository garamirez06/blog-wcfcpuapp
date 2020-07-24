$(function () {

    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Whether we're connected or not
        self.connected = ko.observable(false);

        // Collection of machines that are connected
        self.machines = ko.observableArray();
        self.services = ko.observableArray();
    };

    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#computerInfo")[0]);
    ko.applyBindings(vm, $("#servicesInfo")[0]);

    // Get a reference to our hub
    var hub = $.connection.cpuInfo;

    // Add a handler to receive updates from the server
    hub.client.cpuInfoMessage = function (machineName, cpu, memUsage, memTotal, services, ips, disk, sysos, procesador, filesVersion) {
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
            filesVersion: filesVersion
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
            var machineName = this.machineName;
            var serviceName = this.serviceName;
            var serviceDisplayName = this.serviceDisplayName;
            var serviceType = this.serviceType;
            var status = this.status;
            var startType = this.startType;
            var Path = this.Path;
            var serviceVersion = this.serviceVersion;
            var filesVersion = this.filesVersion;

            var service = {
                machineName: this.machineName,
                serviceName: this.serviceName,
                serviceDisplayName: this.serviceDisplayName,
                serviceType: this.serviceType,
                status: this.status,
                startType: this.startType,
                path: this.Path,
                serviceVersion: this.serviceVersion,
                filesVersion: this.filesVersion
            };

            var serviceModel = ko.mapping.fromJS(service);
            // Check if we already have it:
            var flag;
            var match = vm.services().forEach(function (item) {
                if (item.machineName() == machineName && item.serviceName() == serviceName) {
                    flag = true;
                }
            });

            match = flag;
            if (!match)
                vm.services.push(serviceModel);
            else {
                var index = vm.services.indexOf(match);
                vm.services.replace(vm.services()[index], serviceModel);
            }
        });
    };

    // Start the connection
    $.connection.hub.start().done(function () {
        vm.connected(true);
    });
});