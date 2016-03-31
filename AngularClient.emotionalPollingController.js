
(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('clientEmotionalPollingController', ClientEmotionalPollingTabController);

    ClientEmotionalPollingTabController.$inject = ['$scope', '$baseController', '$emotionalPollingService', '$filter'];

    function ClientEmotionalPollingTabController(
        $scope
      , $baseController
      , $emotionalPollingService
      , $filter) {

        var vm = this;

        $baseController.merge(vm, $baseController);

        vm.$filter = $filter;
        vm.$scope = $scope;
        vm.$emotionalPollingService = $emotionalPollingService;
        vm.numberOfDays = 7;

        vm.heading = "Emotional Polling";
        vm.options = {
            scaleBeginAtZero: true,
            //datasetFill: false
        };

        vm.labels = [];
        vm.data = [];
        vm.recentAverage = [];
        vm.isPollData = false;

        vm.items = null;
        vm.series = ['Emotion', 'Total Average'];

        vm.onClick = _onClick;
        vm.getDateFormat = _getDateFormat;
        vm.onTabChange = _onTabChange;

        //vm.onSubmit = _onSubmit;
        vm.onAverageSuccess = _onAverageSuccess;
        vm.receiveItems = _receiveItems;
        vm.onGetError = _onGetError;
        vm.onAvgError = _onAvgError;

        vm.notify = vm.$emotionalPollingService.getNotifier($scope);

        render();

        function render() {
            vm.onTabChange(6)
            var eventRaised = { title: "Emotional Polling" };
            vm.$systemEventService.broadcast('titleChanged', eventRaised);

            vm.$emotionalPollingService.get(vm.numberOfDays, vm.receiveItems, vm.onGetError);
        }

        function _onTabChange(tab) {

            vm.$systemEventService.broadcast('tabChanged', { tabToSelect: tab });
        }

        function _onClick(points, evt) {
            console.log(points, evt);
        };

        function _getDateFormat(date) {
            return $filter('date')(new Date(date), 'EEE, d');
        }

        function _receiveItems(data) {
            vm.notify(function () {
                if (data && data.items) {
                    var emotionArray = new Array();

                    vm.items = data.items;
                    for (var i = 0; i < data.items.length; i++) {  //console.log(vm.items[i]);

                        emotionArray.push(vm.items[i].value)     //Pushing emotion Value. ex: [5,5,5,5,5]
                        var clientId = vm.items[i].userId

                        var dateFormat = _getDateFormat(vm.items[i].dateAdded);
                        vm.labels.push(dateFormat);
                    };
                    vm.data.push(emotionArray);
                    vm.$emotionalPollingService.getAverage(vm.numberOfDays, vm.onAverageSuccess, vm.onAvgError)
                    vm.isPollData = true;
                }
                else {
                    vm.isPollData = false;
                }
            });
        }

        function _onAverageSuccess(data) {
            vm.notify(function () {
                //vm.$alertService.success("Yeah!");
                vm.emotionValue = data.item.value

                for (var i = 1; i <= vm.data[0].length; i++) {
                    vm.recentAverage.push(data.item.value);
                };
                vm.data.push(vm.recentAverage);
            })
        }

        function _onGetError(errorThrown) {
            console.log(errorThrown);
            vm.$alertService.error("There was an error retrieving clients data or no information available.");
        }

        function _onAvgError(errorThrown) {
            vm.$alertService.error("There was an error retrieving the Average");
        }

    }

})();
