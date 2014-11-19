(function (angular) {
    
    var theModule = angular.module("messageAuditModule", ["ngSanitize"]);
    
    theModule.controller("homeViewController", ['$scope', '$http', '$sanitize', function ($scope, $http, $sanitize) {
            
            $scope.correlationId = '';
            
            $scope.findMessages = function () {
                
                var param = encodeURIComponent($scope.correlationId);
                $http.get('http://localhost:1337/messages/getByCorrelationId/' + param)
                .success(function (data) {
                        $scope.messages = _(data).map(function(message) {
                            return {
                                downloadTime: message.DownloadTime,
                                copyTime: message.AuditCopyTime,
                                label: message.Label
                            };
                        });
                        
                    /*
                    var min = _($scope.messages).min(function(message) {
                        return message.downloadTime;
                    }).copyTime;
                    
                    var max = _($scope.messages).max(function (message) {
                            return message.downloadTime;
                        }).copyTime;

                    $scope.TotalTimeSpend = (max - min);
                    */
                });

            };

        
        }]);


})(window.angular)