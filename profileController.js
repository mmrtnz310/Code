(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('clientProfileTabController', ClientProfileTabController);

    ClientProfileTabController.$inject = ['$scope', '$baseController'];

    function ClientProfileTabController(
        $scope
      , $baseController) {

        var vm = this;
        $baseController.merge(vm, $baseController);
        vm.$scope = $scope;
        //vm.notify = vm.$contactsService.getNotifier($scope);
        vm.fireEvent = _fireEvent;

        render();

        function render() {
            vm.fireEvent(0,'tabChanged')
        }
        function _fireEvent(idx, eventName) {

            vm.$systemEventService.broadcast(eventName, { tabToSelect: idx });
        }

    }
})();

(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('userProfileController', UserProfileController);

    UserProfileController.$inject = ['$scope', '$baseController', '$userProfileService', '$uibModal'];

    function UserProfileController(
      $scope
      , $baseController
      , $userProfileService
      , $uibModal) {

        var vm = this;

        $baseController.merge(vm, $baseController);

        vm.profile = null;

        vm.$scope = $scope;
        vm.$uibModal = $uibModal;
        vm.$userProfileService = $userProfileService;
        vm.notify = vm.$userProfileService.getNotifier($scope);
        vm.fireEvent = _fireEvent;

        vm.heading = "Profile Information";

        vm.onEditClick = _onEditClick;

        vm.onProfileInfoSuccess = _onProfileInfoSuccess;
        vm.onError = _onError;

        render();

        function render() {
            vm.fireEvent(0, 'tabChanged')
            vm.$userProfileService.get(vm.onProfileInfoSuccess, vm.onError);
        };

        function _fireEvent(idx, eventName) {
            vm.$systemEventService.broadcast(eventName, { tabToSelect: idx });
        }

        function _onProfileInfoSuccess(data) {
            vm.notify(function () {
                vm.profile = data.item;
                //console.log(vm.profile);
            });
        };

        function _onEditClick(profile) {
            vm.profile = profile
            vm.animationsEnabled = true;

            var modalInstance = $uibModal.open({
                animation: vm.animationsEnabled,
                templateUrl: '/Scripts/sabio/core/profile/views/UpdateProfileForm.html',
                controller: 'profileModalController',
                controllerAs: 'proModalCtrl',
                resolve: {
                    profile: function () {
                        return vm.profile;
                    }
                }
            });

            modalInstance.result.then(function (selectedItem) {
                vm.selected = selectedItem;
            }, function () {
                //$log.info('Modal dismissed at: ' + new Date());
            });
        };

        function _onSuccess(data, status) {
            console.log(data)
            console.log(status)
        };

        function _onError(errorThrown) {
            console.log(errorThrown)
            vm.$alertService.error("There was an error retrieving your information.");
        }
    }
})();





(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('profileModalController', ProfileModalController);

    ProfileModalController.$inject = ['$scope', '$baseController', '$userProfileService', '$uibModalInstance', 'profile'];

    function ProfileModalController(
      $scope
      , $baseController
      , $userProfileService
      , $uibModalInstance
      , profile) {

        var mc = this;
        mc.showProfileErrors = false;

        mc.profile = profile;
        $baseController.merge(mc, $baseController);
        mc.$scope = $scope;
        mc.$uibModalInstance = $uibModalInstance;
        mc.$userProfileService = $userProfileService;
        mc.notify = mc.$userProfileService.getNotifier($scope);

        mc.onCancel = _onCancel;

        mc.heading = "Update Profile info";
        mc.onUpdateSubmit = _onUpdateSubmit;
        mc.onUpdateSuccess = _onUpdateSuccess;
        mc.onUpdateError = _onUpdateError;

        function _onUpdateSubmit(profile) {
            mc.showProfileErrors = true;
            //console.log("mc " + mc.profile)

            if (mc.profileForm.$valid) {
                mc.$userProfileService.update(profile, mc.onUpdateSuccess, mc.onUpdateError)
            };
        };

        function _onUpdateSuccess(data) {
            console.log(data)
            mc.$alertService.success("Your information was successfully updated!");
            $uibModalInstance.close();
        };

        function _onUpdateError(errorThrown) {
            console.log(errorThrown)
            mc.notify(function () {
                mc.$alertService.error("The username is already taken. Please choose a different username.");
            })
        };

        function _onCancel() {
            mc.profileForm.$setUntouched();
            $uibModalInstance.dismiss('cancel');
        };
    }
})();